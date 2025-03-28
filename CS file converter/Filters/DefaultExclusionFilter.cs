using CSFileConverter.Interfaces;
using CSFileConverter.Models;

using System.Text.RegularExpressions;

namespace CSFileConverter.Filters
{
    /// <summary>
    /// Default implementation for excluding files based on patterns and folders
    /// </summary>
    public class DefaultExclusionFilter : IFileFilter
    {
        private readonly List<string> _excludedFolders =
        [
            "obj", "bin", ".vs", "packages"
        ];

        private readonly List<string> _excludedPatterns = [];
        private ProcessingOptions _options;

        public DefaultExclusionFilter(ProcessingOptions options)
        {
            _options = options;
            ConfigureExclusions();
        }

        public bool ShouldInclude(string filePath)
        {
            // Check folder exclusions
            if (_excludedFolders.Any(folder =>
                filePath.Replace(Path.DirectorySeparatorChar, '/').Contains($"/{folder}/")))
            {
                return false;
            }

            // Check pattern exclusions
            string fileName = Path.GetFileName(filePath);
            foreach (var pattern in _excludedPatterns)
            {
                string regexPattern = "^" + Regex.Escape(pattern)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".")
                    .Replace("\\[", "[")
                    .Replace("\\]", "]") + "$";

                if (Regex.IsMatch(fileName, regexPattern, RegexOptions.None))
                {
                    return false;
                }
            }

            return true;
        }

        public void Configure(FileProcessorOptions options)
        {
            // Add any processor-specific configuration here
        }

        private void ConfigureExclusions()
        {
            if (!_options.IncludeMigrations)
            {
                _excludedFolders.Add(_options.MigrationsFolder);
            }

            if (!_options.IncludeInterfaces)
            {
                _excludedPatterns.Add("I*.cs");
            }

            if (!_options.IncludeTestFiles)
            {
                _excludedPatterns.Add("*Test*.cs");
                _excludedPatterns.Add("*Tests*.cs");
                _excludedFolders.Add("Tests");
                _excludedFolders.Add("Test");
            }

            if (!_options.IncludeGeneratedFiles)
            {
                _excludedPatterns.Add("*.g.cs");
                _excludedPatterns.Add("*.generated.cs");
            }
        }
    }
}
