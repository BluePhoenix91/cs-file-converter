using CSFileConverter.Models;
using CSFileConverter.Services;

namespace CSFileConverter.UI
{
    /// <summary>
    /// Console user interface for the file converter
    /// </summary>
    public class ConsoleUI(ProjectInfoService projectInfoService)
    {

        /// <summary>
        /// Displays the application header
        /// </summary>
        public void ShowHeader()
        {
            Console.WriteLine("=== Code File Converter ===");
            Console.WriteLine("Supports .NET solutions and Angular projects\n");
        }

        /// <summary>
        /// Gets processing options from user input
        /// </summary>
        public ProcessingOptions GetProcessingOptions()
        {
            var options = new ProcessingOptions();

            Console.WriteLine("\n=== File Processing Options ===");

            Console.Write("Include migration files? (Y/N): ");
            options.IncludeMigrations = GetYesNoResponse();

            if (!options.IncludeMigrations)
            {
                Console.Write("Enter the migrations folder name (default is 'Migrations'): ");
                string folder = Console.ReadLine()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(folder))
                {
                    options.MigrationsFolder = folder;
                }
            }

            Console.Write("Include interface files (I*.cs, I*.ts)? (Y/N): ");
            options.IncludeInterfaces = GetYesNoResponse();

            Console.Write("Include test files (*.test.*, *.spec.*)? (Y/N): ");
            options.IncludeTestFiles = GetYesNoResponse();

            Console.Write("Include generated files? (Y/N): ");
            options.IncludeGeneratedFiles = GetYesNoResponse();

            Console.Write("Remove documentation comments? (Y/N): ");
            options.RemoveXmlDocs = GetYesNoResponse();

            Console.Write("Remove empty lines? (Y/N): ");
            options.RemoveEmptyLines = GetYesNoResponse();

            Console.Write("Optimize whitespace? (Y/N): ");
            options.RemoveWhitespace = GetYesNoResponse();

            return options;
        }

        /// <summary>
        /// Gets the source directory path from user input
        /// </summary>
        public string GetSourceDirectory()
        {
            Console.Write("Enter the project directory path (.NET solution or Angular project): ");
            string sourceDir = Console.ReadLine()?.Trim() ?? "";

            while (!Directory.Exists(sourceDir) || !projectInfoService.IsSolutionDirectory(sourceDir))
            {
                Console.WriteLine("\nError: The specified directory does not exist or is not a valid project directory.");
                Console.WriteLine("Please ensure the directory contains either:");
                Console.WriteLine("  - A .sln file (for .NET solutions)");
                Console.WriteLine("  - An angular.json or package.json file (for Angular projects)");
                Console.Write("\nPlease enter a valid project directory path: ");
                sourceDir = Console.ReadLine()?.Trim() ?? "";
            }

            return sourceDir;
        }

        /// <summary>
        /// Gets the destination directory path from user input
        /// </summary>
        public string GetDestinationDirectory()
        {
            Console.Write("\nEnter the destination directory path: ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        /// <summary>
        /// Gets the output structure choice from user input
        /// </summary>
        public OutputStructure GetOutputStructureChoice()
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

        /// <summary>
        /// Displays the conversion results
        /// </summary>
        public void ShowResults(ConversionResult result)
        {
            Console.WriteLine("\nConversion process completed!");
            Console.WriteLine($"Total files processed: {result.TotalFiles}");
            Console.WriteLine($"Successfully converted: {result.SuccessCount}");
            Console.WriteLine($"Errors encountered: {result.ErrorCount}");

            if (result.ErrorCount > 0)
            {
                Console.WriteLine("\nError details:");
                foreach (var error in result.ErrorMessages)
                {
                    Console.WriteLine($"- {error}");
                }
            }
        }

        /// <summary>
        /// Displays progress of the conversion process
        /// </summary>
        public void ShowProgress(string sourceFile, string destPath, int current, int total)
        {
            Console.WriteLine($"Successfully converted: {sourceFile}");
            Console.WriteLine($"New file created: {destPath}");
            Console.WriteLine($"Progress: {current}/{total} files processed");
            Console.WriteLine(new string('-', 50));
        }

        /// <summary>
        /// Waits for user to press a key
        /// </summary>
        public void WaitForKeyPress()
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Helper method to get a yes/no response
        /// </summary>
        private static bool GetYesNoResponse()
        {
            return Console.ReadLine()?.Trim().ToUpper() == "Y";
        }
    }
}