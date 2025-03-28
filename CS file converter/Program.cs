using CSFileConverter.Services;
using CSFileConverter.UI;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Create services
            var projectInfoService = new ProjectInfoService();
            var processorFactory = new FileProcessorFactory();
            var formatterFactory = new OutputFormatterFactory();
            var conversionService = new FileConversionService(
                processorFactory, formatterFactory, projectInfoService);

            // Create UI
            var ui = new ConsoleUI(projectInfoService);

            // Display header
            ui.ShowHeader();

            // Get processing options
            var options = ui.GetProcessingOptions();

            // Get source directory
            string sourceDir = ui.GetSourceDirectory();

            // Get destination directory
            string destDir = ui.GetDestinationDirectory();
            options.OutputDirectory = destDir;

            // Get output structure
            options.SelectedOutputStructure = ui.GetOutputStructureChoice();

            // Perform conversion
            Console.WriteLine("\nStarting conversion process...\n");
            var result = conversionService.Convert(sourceDir, options);

            // Show results
            ui.ShowResults(result);

            // Wait for user to exit
            ui.WaitForKeyPress();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nA critical error occurred: {ex.Message}");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}