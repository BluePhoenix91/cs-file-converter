using System.Xml.Linq;
using System.Text.RegularExpressions;

class Program
{
    private static readonly List<string> ExcludedFolders = new List<string> { "obj", "bin", ".vs", "packages" };
    private static readonly List<string> ExcludedPatterns = new List<string>();
    private enum OutputStructure { SuperFlat, Flat, Structured }

    class ProcessingOptions
    {
        public bool IncludeMigrations { get; set; } = true;
        public bool IncludeInterfaces { get; set; } = true;
        public bool IncludeTestFiles { get; set; } = true;
        public bool IncludeGeneratedFiles { get; set; } = true;
        public bool RemoveXmlDocs { get; set; } = false;
        public bool RemoveEmptyLines { get; set; } = false;
        public string MigrationsFolder { get; set; } = "Migrations";
    }

    static void Main(string[] args)
    {
        Console.WriteLine("=== Solution C# to TXT File Converter ===\n");

        var options = GetProcessingOptions();

        // Get source directory from user
        Console.Write("Enter the solution directory path: ");
        string sourceDir = Console.ReadLine().Trim();

        while (!Directory.Exists(sourceDir) || !IsSolutionDirectory(sourceDir))
        {
            Console.WriteLine("\nError: The specified directory does not exist or is not a valid solution directory.");
            Console.WriteLine("Please ensure the directory contains a .sln file.");
            Console.Write("Please enter a valid solution directory path: ");
            sourceDir = Console.ReadLine().Trim();
        }

        // Apply exclusion patterns based on options
        if (!options.IncludeMigrations)
        {
            ExcludedFolders.Add(options.MigrationsFolder);
        }
        if (!options.IncludeInterfaces)
        {
            // Use a more specific pattern for interface files
            // Matches files starting with I followed by uppercase letter
            ExcludedPatterns.Add("I[A-Z]*.cs");
        }
        if (!options.IncludeTestFiles)
        {
            ExcludedPatterns.Add("*Test*.cs");
            ExcludedPatterns.Add("*Tests*.cs");
            ExcludedFolders.Add("Tests");
            ExcludedFolders.Add("Test");
        }
        if (!options.IncludeGeneratedFiles)
        {
            ExcludedPatterns.Add("*.g.cs");
            ExcludedPatterns.Add("*.generated.cs");
        }

        // Get destination directory from user
        Console.Write("\nEnter the destination directory path: ");
        string destDir = Console.ReadLine().Trim();

        // Get output structure preference
        OutputStructure structureChoice = GetOutputStructureChoice();

        ProcessFiles(sourceDir, destDir, structureChoice, options);
    }

    static ProcessingOptions GetProcessingOptions()
    {
        var options = new ProcessingOptions();

        Console.WriteLine("\n=== File Processing Options ===");

        Console.Write("Include migration files? (Y/N): ");
        options.IncludeMigrations = Console.ReadLine().Trim().ToUpper() == "Y";

        if (!options.IncludeMigrations)
        {
            Console.Write("Enter the migrations folder name (default is 'Migrations'): ");
            string folder = Console.ReadLine().Trim();
            if (!string.IsNullOrEmpty(folder))
            {
                options.MigrationsFolder = folder;
            }
        }

        Console.Write("Include interface files (I*.cs)? (Y/N): ");
        options.IncludeInterfaces = Console.ReadLine().Trim().ToUpper() == "Y";

        Console.Write("Include test files? (Y/N): ");
        options.IncludeTestFiles = Console.ReadLine().Trim().ToUpper() == "Y";

        Console.Write("Include generated files? (Y/N): ");
        options.IncludeGeneratedFiles = Console.ReadLine().Trim().ToUpper() == "Y";

        Console.Write("Remove XML documentation? (Y/N): ");
        options.RemoveXmlDocs = Console.ReadLine().Trim().ToUpper() == "Y";

        Console.Write("Remove empty lines? (Y/N): ");
        options.RemoveEmptyLines = Console.ReadLine().Trim().ToUpper() == "Y";

        return options;
    }

    static void ProcessFiles(string sourceDir, string destDir, OutputStructure structureChoice, ProcessingOptions options)
    {
        try
        {
            Directory.CreateDirectory(destDir);

            var projectFiles = GetProjectFiles(sourceDir);
            var projectPaths = projectFiles.ToDictionary(
                pf => pf,
                pf => GetProjectName(pf)
            );

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
                    string destPath = GetDestinationPath(csFile, destDir, sourceDir, structureChoice, projectPaths);

                    if (structureChoice != OutputStructure.SuperFlat)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    }

                    // Process file content
                    string content = File.ReadAllText(csFile);

                    // Remove comments first
                    content = RemoveComments(content);

                    if (options.RemoveXmlDocs)
                    {
                        content = RemoveXmlDocumentation(content);
                    }

                    if (options.RemoveEmptyLines)
                    {
                        content = RemoveEmptyLines(content);
                    }

                    File.WriteAllText(destPath, content);
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

    static string GetDestinationPath(string sourceFile, string destDir, string sourceDir,
        OutputStructure structureChoice, Dictionary<string, string> projectPaths)
    {
        switch (structureChoice)
        {
            case OutputStructure.SuperFlat:
                var projectFile = projectPaths
                    .Where(p => sourceFile.StartsWith(Path.GetDirectoryName(p.Key)))
                    .OrderByDescending(p => p.Key.Length)
                    .FirstOrDefault();
                string projectName = projectFile.Value ?? "Unknown";
                string fileName = Path.GetFileName(sourceFile);
                return Path.Combine(destDir, $"{projectName}.{Path.ChangeExtension(fileName, ".txt")}");

            case OutputStructure.Flat:
                projectFile = projectPaths
                    .Where(p => sourceFile.StartsWith(Path.GetDirectoryName(p.Key)))
                    .OrderByDescending(p => p.Key.Length)
                    .FirstOrDefault();
                projectName = projectFile.Value ?? "Unknown";
                fileName = Path.GetFileName(sourceFile);
                return Path.Combine(destDir, projectName, Path.ChangeExtension(fileName, ".txt"));

            default: // Structured
                string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
                return Path.Combine(destDir, Path.ChangeExtension(relativePath, ".txt"));
        }
    }

    static string RemoveXmlDocumentation(string content)
    {
        // Remove XML documentation comments (///...)
        content = Regex.Replace(content, @"^\s*///.*$\n?", "", RegexOptions.Multiline);

        // Remove multi-line XML documentation
        content = Regex.Replace(content, @"/\*\*(?!\*)[\s\S]*?\*/", "");

        return content;
    }

    static string RemoveEmptyLines(string content)
    {
        // Remove multiple consecutive empty lines, preserve single empty lines
        content = Regex.Replace(content, @"^\s*$\n\s*$\n", "", RegexOptions.Multiline);

        // Remove empty lines after opening braces
        content = Regex.Replace(content, @"{\s*\n\s*\n", "{\n");

        // Remove empty lines before closing braces
        content = Regex.Replace(content, @"\n\s*\n\s*}", "\n}");

        return content;
    }

    static string RemoveComments(string content)
    {
        var lines = content.Split('\n');
        var result = new List<string>();
        bool inMultiLineComment = false;
        string multiLineBuffer = "";

        foreach (var line in lines)
        {
            if (inMultiLineComment)
            {
                int endIndex = line.IndexOf("*/");
                if (endIndex != -1)
                {
                    // End of multi-line comment found
                    inMultiLineComment = false;
                    string remainingLine = line.Substring(endIndex + 2);
                    if (!string.IsNullOrWhiteSpace(remainingLine))
                    {
                        multiLineBuffer += remainingLine;
                        result.Add(multiLineBuffer.Trim());
                    }
                    multiLineBuffer = "";
                }
                continue;
            }

            string currentLine = line;

            // Handle multi-line comment start
            int multiLineStart = currentLine.IndexOf("/*");
            if (multiLineStart != -1)
            {
                int multiLineEnd = currentLine.IndexOf("*/", multiLineStart);
                if (multiLineEnd != -1)
                {
                    // Multi-line comment starts and ends on same line
                    currentLine = currentLine.Remove(multiLineStart, multiLineEnd - multiLineStart + 2);
                }
                else
                {
                    // Multi-line comment starts but doesn't end
                    inMultiLineComment = true;
                    multiLineBuffer = currentLine.Substring(0, multiLineStart).Trim();
                    continue;
                }
            }

            // Handle single-line comments
            int commentIndex = currentLine.IndexOf("//");
            if (commentIndex != -1)
            {
                currentLine = currentLine.Substring(0, commentIndex);
            }

            // Only add non-empty lines
            if (!string.IsNullOrWhiteSpace(currentLine))
            {
                result.Add(currentLine.TrimEnd());
            }
        }

        return string.Join("\n", result);
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
        // First, get all .cs files excluding by folder
        var files = Directory.GetFiles(solutionDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => !ExcludedFolders.Any(folder =>
                file.Replace(Path.DirectorySeparatorChar, '/').Contains($"/{folder}/")));

        // Then filter by pattern exclusions
        var result = files.ToList();
        var filesToExclude = new List<string>();

        foreach (var pattern in ExcludedPatterns)
        {
            // Convert the pattern to a regex pattern
            string regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".")
                .Replace("\\[", "[")  // Allow character classes to work
                .Replace("\\]", "]") + "$";

            var matchedFiles = result
                .Where(file => Regex.IsMatch(Path.GetFileName(file), regexPattern, RegexOptions.None))
                .ToList();

            filesToExclude.AddRange(matchedFiles);

            // Log matching files for debugging
            if (matchedFiles.Any())
            {
                Console.WriteLine($"Pattern '{pattern}' excluded {matchedFiles.Count} files");
                foreach (var file in matchedFiles.Take(5))  // Show first 5 matches
                {
                    Console.WriteLine($"  - {Path.GetFileName(file)}");
                }
                if (matchedFiles.Count > 5)
                {
                    Console.WriteLine($"  - ... and {matchedFiles.Count - 5} more");
                }
            }
        }

        return result.Except(filesToExclude);
    }

    static void WaitForKeyPress()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}