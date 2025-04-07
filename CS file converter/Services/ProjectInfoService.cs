using System.Xml.Linq;

namespace CSFileConverter.Services
{
    // <summary>
    /// Service for retrieving information about projects in a solution or Angular application
    /// </summary>
    public class ProjectInfoService
    {
        /// <summary>
        /// Gets a dictionary mapping project file paths to project names
        /// </summary>
        public Dictionary<string, string> GetProjectMapping(string solutionDirectory)
        {
            var projectFiles = GetProjectFiles(solutionDirectory);

            return projectFiles.ToDictionary(
                pf => pf,
                pf => GetProjectName(pf)
            );
        }

        /// <summary>
        /// Determines if a directory contains a Visual Studio solution or an Angular project
        /// </summary>
        public bool IsSolutionDirectory(string path)
        {
            // Check for .NET solution
            bool isSolution = Directory.GetFiles(path, "*.sln").Length != 0;

            // Check for Angular project
            bool isAngular = File.Exists(Path.Combine(path, "angular.json")) ||
                             File.Exists(Path.Combine(path, "package.json"));

            return isSolution || isAngular;
        }

        /// <summary>
        /// Gets all project files in a solution or Angular directory
        /// </summary>
        private static IEnumerable<string> GetProjectFiles(string solutionDir)
        {
            var excludedFolders = new List<string> { "obj", "bin", ".vs", "packages", "node_modules", "dist", ".angular" };
            var projectFiles = new List<string>();

            // Get .NET project files
            projectFiles.AddRange(Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories)
                .Where(file => !excludedFolders.Any(folder =>
                    file.Replace(Path.DirectorySeparatorChar, '/').Contains($"/{folder}/"))));

            // Check for Angular projects
            string angularJsonPath = Path.Combine(solutionDir, "angular.json");
            if (File.Exists(angularJsonPath))
            {
                projectFiles.Add(angularJsonPath);
            }
            else
            {
                // Check for package.json as fallback
                string packageJsonPath = Path.Combine(solutionDir, "package.json");
                if (File.Exists(packageJsonPath))
                {
                    projectFiles.Add(packageJsonPath);
                }
            }

            return projectFiles;
        }

        /// <summary>
        /// Gets the name of a project from its file
        /// </summary>
        private static string GetProjectName(string projectPath)
        {
            string extension = Path.GetExtension(projectPath).ToLowerInvariant();

            try
            {
                // Handle different project file types
                switch (extension)
                {
                    case ".csproj":
                        return GetCSharpProjectName(projectPath);

                    case ".json":
                        return GetAngularProjectName(projectPath);

                    default:
                        return Path.GetFileNameWithoutExtension(projectPath);
                }
            }
            catch
            {
                return Path.GetFileNameWithoutExtension(projectPath);
            }
        }

        /// <summary>
        /// Gets the name of a C# project from its .csproj file
        /// </summary>
        private static string GetCSharpProjectName(string projectPath)
        {
            try
            {
                var doc = XDocument.Load(projectPath);
                var propertyGroup = doc.Descendants("PropertyGroup").FirstOrDefault();

                if (propertyGroup != null)
                {
                    var assemblyName = propertyGroup.Element("AssemblyName")?.Value;
                    if (!string.IsNullOrEmpty(assemblyName))
                        return assemblyName;
                }

                return Path.GetFileNameWithoutExtension(projectPath);
            }
            catch
            {
                return Path.GetFileNameWithoutExtension(projectPath);
            }
        }

        /// <summary>
        /// Gets the name of an Angular project from its angular.json or package.json file
        /// </summary>
        private static string GetAngularProjectName(string projectPath)
        {
            try
            {
                string fileName = Path.GetFileName(projectPath).ToLowerInvariant();
                string jsonContent = File.ReadAllText(projectPath);

                if (fileName == "angular.json")
                {
                    // Try to extract project name from angular.json
                    // This is a simplified approach - for complete solution we'd use a JSON parser
                    string namePattern = "\"defaultProject\"\\s*:\\s*\"([^\"]+)\"";
                    var match = System.Text.RegularExpressions.Regex.Match(jsonContent, namePattern);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value;
                    }
                }

                if (fileName == "package.json")
                {
                    // Try to extract project name from package.json
                    string namePattern = "\"name\"\\s*:\\s*\"([^\"]+)\"";
                    var match = System.Text.RegularExpressions.Regex.Match(jsonContent, namePattern);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value;
                    }
                }

                // Fallback to directory name for Angular projects
                return Path.GetFileName(Path.GetDirectoryName(projectPath)) ?? "AngularApp";
            }
            catch
            {
                return "AngularApp";
            }
        }
    }
}
