using CSFileConverter.Interfaces;

namespace CSFileConverter.OutputFormatters
{
    /// <summary>
    /// Preserves the original directory structure of files
    /// </summary>
    public class StructuredOutputFormatter : IOutputFormatter
    {
        public string GetDestinationPath(string sourceFile, string sourceDir, string destDir,
            IDictionary<string, string> projectMapping)
        {
            string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
            return Path.Combine(destDir, Path.ChangeExtension(relativePath, ".txt"));
        }
    }
}
