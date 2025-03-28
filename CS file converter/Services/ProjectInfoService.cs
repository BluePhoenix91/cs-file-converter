using System.Xml.Linq;

namespace CSFileConverter.Services
{
    // <summary>
    /// Service for retrieving information about projects in a solution
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
        /// Determines if a directory contains a Visual Studio solution
        /// </summary>
        public bool IsSolutionDirectory(string path)
        {
            return Directory.GetFiles(path, "*.sln").Length != 0;
        }

        /// <summary>
        /// Gets all project files in a solution directory
        /// </summary>
        private static IEnumerable<string> GetProjectFiles(string solutionDir)
        {
            var excludedFolders = new List<string> { "obj", "bin", ".vs", "packages" };

            return Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories)
                .Where(file => !excludedFolders.Any(folder =>
                    file.Replace(Path.DirectorySeparatorChar, '/').Contains($"/{folder}/")));
        }

        /// <summary>
        /// Gets the name of a project from its .csproj file
        /// </summary>
        private static string GetProjectName(string projectPath)
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
    }
}
