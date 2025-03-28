using CSFileConverter.Models;

namespace CSFileConverter.Interfaces
{
    public interface IFileFilter
    {
        /// <summary>
        /// Determines whether a file should be included in processing
        /// </summary>
        bool ShouldInclude(string filePath);

        /// <summary>
        /// Configures the filter with processor-specific options
        /// </summary>
        void Configure(FileProcessorOptions options);
    }
}
