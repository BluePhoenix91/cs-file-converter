using CSFileConverter.Interfaces;
using CSFileConverter.Processors;

namespace CSFileConverter.Services
{
    /// <summary>
    /// Factory for creating file processors based on file type
    /// </summary>
    public class FileProcessorFactory
    {
        private readonly List<IFileProcessor> _processors = new();

        public FileProcessorFactory()
        {
            // Register available processors here
            _processors.Add(new CSharpFileProcessor());
            // Future: Add Python, JavaScript, etc. processors
        }

        /// <summary>
        /// Gets a processor that can handle the given file
        /// </summary>
        public IFileProcessor GetProcessor(string filePath)
        {
            var processor = _processors.FirstOrDefault(p => p.CanProcess(filePath));

            if (processor == null)
            {
                throw new NotSupportedException($"No processor available for file: {filePath}");
            }

            return processor;
        }

        /// <summary>
        /// Returns all registered file processors
        /// </summary>
        public IEnumerable<IFileProcessor> GetAllProcessors()
        {
            return _processors;
        }
    }
}
