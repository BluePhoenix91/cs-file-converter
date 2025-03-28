using CSFileConverter.Filters;
using CSFileConverter.Models;

namespace CSFileConverter.Services
{
    /// <summary>
    /// Core service that handles file conversion process
    /// </summary>
    public class FileConversionService
    {
        private readonly FileProcessorFactory _processorFactory;
        private readonly OutputFormatterFactory _formatterFactory;
        private readonly ProjectInfoService _projectInfoService;

        public FileConversionService(
            FileProcessorFactory processorFactory,
            OutputFormatterFactory formatterFactory,
            ProjectInfoService projectInfoService)
        {
            _processorFactory = processorFactory;
            _formatterFactory = formatterFactory;
            _projectInfoService = projectInfoService;
        }

        /// <summary>
        /// Converts source files based on the provided options
        /// </summary>
        public ConversionResult Convert(string sourceDir, ProcessingOptions options)
        {
            var result = new ConversionResult();

            try
            {
                // Ensure output directory exists
                Directory.CreateDirectory(options.OutputDirectory);

                // Get project mapping information
                var projectMapping = _projectInfoService.GetProjectMapping(sourceDir);

                // Create file filter
                var filter = new DefaultExclusionFilter(options);

                // Get output formatter
                var formatter = _formatterFactory.GetFormatter(options.SelectedOutputStructure);

                // Find all processable files
                var filesToProcess = new List<string>();

                foreach (var processor in _processorFactory.GetAllProcessors())
                {
                    var extension = processor.FileExtension;
                    var files = Directory.GetFiles(sourceDir, $"*{extension}", SearchOption.AllDirectories)
                        .Where(filter.ShouldInclude);

                    filesToProcess.AddRange(files);
                }

                if (!filesToProcess.Any())
                {
                    return new ConversionResult
                    {
                        ErrorMessages = { "No files found to process with current settings." }
                    };
                }

                result.TotalFiles = filesToProcess.Count;

                // Process each file
                foreach (var sourceFile in filesToProcess)
                {
                    try
                    {
                        // Get appropriate processor
                        var processor = _processorFactory.GetProcessor(sourceFile);

                        // Map processor options from processing options
                        var processorOptions = new FileProcessorOptions
                        {
                            RemoveDocumentation = options.RemoveXmlDocs,
                            RemoveEmptyLines = options.RemoveEmptyLines,
                            RemoveRegions = options.RemoveRegions,
                            OptimizeWhitespace = options.RemoveWhitespace
                        };

                        // Set up output path
                        string destPath = formatter.GetDestinationPath(
                            sourceFile, sourceDir, options.OutputDirectory, projectMapping);

                        string? destDir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(destDir))
                        {
                            Directory.CreateDirectory(destDir);
                        }

                        // Process file content
                        string content = File.ReadAllText(sourceFile);
                        string processedContent = processor.ProcessContent(content, processorOptions);

                        // Write output
                        File.WriteAllText(destPath, processedContent);

                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        result.ErrorMessages.Add($"Error processing {sourceFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessages.Add($"Critical error: {ex.Message}");
            }

            return result;
        }
    }
}
