namespace CSFileConverter.Models
{
    /// <summary>
     /// Processor-specific options for file processing
     /// </summary>
    public class FileProcessorOptions
    {
        public bool RemoveDocumentation { get; set; }
        public bool RemoveEmptyLines { get; set; }
        public bool RemoveRegions { get; set; }
        public bool OptimizeWhitespace { get; set; }
    }
}
