using CSFileConverter.Interfaces;

namespace CSFileConverter.OutputFormatters
{
    /// <summary>
    /// Places all files in a single directory with project name prefixes
    /// </summary>
    public class SuperFlatOutputFormatter : IOutputFormatter
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

            return Path.Combine(destDir, $"{projectName}.{Path.ChangeExtension(fileName, ".txt")}");
        }
    }
}
