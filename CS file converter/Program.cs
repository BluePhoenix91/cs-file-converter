using System.Xml.Linq;

class Program
{
    private static readonly List<string> ExcludedFolders = new List<string> { "obj", "bin", ".vs", "packages" };
    private enum OutputStructure { SuperFlat, Flat, Structured }

    static void Main(string[] args)
    {
        Console.WriteLine("=== Solution C# to TXT File Converter ===\n");

        // Get source directory from user
        Console.Write("Enter the solution directory path: ");
        string sourceDir = Console.ReadLine().Trim();

        // Validate source directory
        while (!Directory.Exists(sourceDir) || !IsSolutionDirectory(sourceDir))
        {
            Console.WriteLine("\nError: The specified directory does not exist or is not a valid solution directory.");
            Console.WriteLine("Please ensure the directory contains a .sln file.");
            Console.Write("Please enter a valid solution directory path: ");
            sourceDir = Console.ReadLine().Trim();
        }

        // Get migration preferences
        Console.Write("\nInclude migration files? (Y/N): ");
        bool includeMigrations = Console.ReadLine().Trim().ToUpper() == "Y";

        if (!includeMigrations)
        {
            Console.Write("\nEnter the default migrations folder name (default is 'Migrations'): ");
            string migrationsFolder = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(migrationsFolder))
            {
                migrationsFolder = "Migrations";
            }
            ExcludedFolders.Add(migrationsFolder);
            Console.WriteLine($"\nFiles in '{migrationsFolder}' folders will be excluded.");
        }

        // Get destination directory from user
        Console.Write("\nEnter the destination directory path: ");
        string destDir = Console.ReadLine().Trim();

        // Get output structure preference
        OutputStructure structureChoice = GetOutputStructureChoice();

        try
        {
            // Create destination directory if it doesn't exist
            Directory.CreateDirectory(destDir);

            // Get all project files in the solution
            var projectFiles = GetProjectFiles(sourceDir);
            var projectPaths = projectFiles.ToDictionary(
                pf => pf,
                pf => GetProjectName(pf)
            );

            // Get all .cs files from source directory, excluding specified folders
            var csFiles = GetSolutionFiles(sourceDir);

            if (!csFiles.Any())
            {
                Console.WriteLine("\nNo .cs files found in the solution directory (with current exclusion settings).");
                WaitForKeyPress();
                return;
            }

            Console.WriteLine($"\nFound {csFiles.Count()} .cs files to convert.");
            Console.WriteLine("\nStarting conversion process...\n");

            int convertedCount = 0;
            int errorCount = 0;

            foreach (string csFile in csFiles)
            {
                try
                {
                    string destPath;
                    switch (structureChoice)
                    {
                        case OutputStructure.SuperFlat:
                            // Find which project this file belongs to
                            var projectFile = projectPaths
                                .Where(p => csFile.StartsWith(Path.GetDirectoryName(p.Key)))
                                .OrderByDescending(p => p.Key.Length)
                                .FirstOrDefault();

                            string projectName = projectFile.Value ?? "Unknown";
                            string fileName = Path.GetFileName(csFile);
                            destPath = Path.Combine(destDir, $"{projectName}.{Path.ChangeExtension(fileName, ".txt")}");
                            break;

                        case OutputStructure.Flat:
                            projectFile = projectPaths
                                .Where(p => csFile.StartsWith(Path.GetDirectoryName(p.Key)))
                                .OrderByDescending(p => p.Key.Length)
                                .FirstOrDefault();

                            projectName = projectFile.Value ?? "Unknown";
                            fileName = Path.GetFileName(csFile);
                            destPath = Path.Combine(destDir, projectName, Path.ChangeExtension(fileName, ".txt"));
                            break;

                        default: // Structured
                            string relativePath = Path.GetRelativePath(sourceDir, csFile);
                            destPath = Path.Combine(destDir, Path.ChangeExtension(relativePath, ".txt"));
                            break;
                    }

                    // Create subdirectories in destination if needed (not needed for super flat)
                    if (structureChoice != OutputStructure.SuperFlat)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    }

                    // Copy file with new extension
                    File.Copy(csFile, destPath, true);
                    convertedCount++;

                    Console.WriteLine($"Successfully converted: {csFile}");
                    Console.WriteLine($"New file created: {destPath}");
                    Console.WriteLine($"Progress: {convertedCount}/{csFiles.Count()} files processed");
                    Console.WriteLine(new string('-', 50));
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"\nError converting {csFile}:");
                    Console.WriteLine($"Error details: {ex.Message}");
                    Console.WriteLine(new string('-', 50));
                }
            }

            Console.WriteLine("\nConversion process completed!");
            Console.WriteLine($"Total files processed: {csFiles.Count()}");
            Console.WriteLine($"Successfully converted: {convertedCount}");
            Console.WriteLine($"Errors encountered: {errorCount}");

            WaitForKeyPress();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nA critical error occurred: {ex.Message}");
            WaitForKeyPress();
        }
    }

    static OutputStructure GetOutputStructureChoice()
    {
        while (true)
        {
            Console.WriteLine("\nChoose output structure:");
            Console.WriteLine("1. Super Flat (all files in one folder, prefixed with project name)");
            Console.WriteLine("2. Flat (organized by project name folders)");
            Console.WriteLine("3. Structured (maintains original folder structure)");
            Console.Write("Enter your choice (1, 2, or 3): ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 1) return OutputStructure.SuperFlat;
                if (choice == 2) return OutputStructure.Flat;
                if (choice == 3) return OutputStructure.Structured;
            }
            Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
        }
    }

    static bool IsSolutionDirectory(string path)
    {
        return Directory.GetFiles(path, "*.sln").Any();
    }

    static IEnumerable<string> GetProjectFiles(string solutionDir)
    {
        return Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories)
            .Where(file => !ExcludedFolders.Any(folder =>
                file.Replace(Path.DirectorySeparatorChar, '/').Contains($"/{folder}/")));
    }

    static string GetProjectName(string projectPath)
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
            // Fallback to file name without extension
            return Path.GetFileNameWithoutExtension(projectPath);
        }
        catch
        {
            return Path.GetFileNameWithoutExtension(projectPath);
        }
    }

    static IEnumerable<string> GetSolutionFiles(string solutionDir)
    {
        return Directory.GetFiles(solutionDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => !ExcludedFolders.Any(folder =>
                file.Replace(Path.DirectorySeparatorChar, '/').Contains($"/{folder}/")));
    }

    static void WaitForKeyPress()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}