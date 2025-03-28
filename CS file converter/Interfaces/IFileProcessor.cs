using CSFileConverter.Models;

namespace CSFileConverter.Interfaces
{
    public interface IFileProcessor
    {
        /// <summary>
        /// Determines if this processor can handle the given file type
        /// </summary>
        bool CanProcess(string filePath);

        /// <summary>
        /// Processes the content of a file according to the specified options
        /// </summary>
        string ProcessContent(string content, FileProcessorOptions options);

        /// <summary>
        /// Gets the file extension this processor handles
        /// </summary>
        string FileExtension { get; }
    }
}
