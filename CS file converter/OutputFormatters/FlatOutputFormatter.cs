using CSFileConverter.Interfaces;

namespace CSFileConverter.OutputFormatters
{
    /// <summary>
    /// Organizes files by project in separate directories
    /// </summary>
    public class FlatOutputFormatter : IOutputFormatter
    {
        public string GetDestinationPath(string sourceFile, string sourceDir, string destDir,
            IDictionary<string, string> projectMapping)
        {
            var projectFile = projectMapping
                .Where(p => sourceFile.StartsWith(Path.GetDirectoryName(p.Key)))
                .OrderByDescending(p => p.Key.Length)
                .FirstOrDefault();

            string projectName = projectFile.Value ?? "Unknown";
            string fileName = Path.GetFileName(sourceFile);

            return Path.Combine(destDir, projectName, Path.ChangeExtension(fileName, ".txt"));
        }
    }
}
