namespace CSFileConverter.Interfaces
{
    public interface IOutputFormatter
    {
        /// <summary>
        /// Determines the destination path for a processed file
        /// </summary>
        string GetDestinationPath(string sourceFile, string sourceDir, string destDir,
            IDictionary<string, string> projectMapping);
    }
}
