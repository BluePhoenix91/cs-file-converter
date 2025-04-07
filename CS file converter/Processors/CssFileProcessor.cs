using CSFileConverter.Interfaces;
using CSFileConverter.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSFileConverter.Processors
{
    /// <summary>
    /// Processes CSS/SCSS files commonly used in Angular projects
    /// </summary>
    public class CssFileProcessor : IFileProcessor
    {
        public string FileExtension => ".css";

        public bool CanProcess(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(FileExtension, StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".scss", StringComparison.OrdinalIgnoreCase) ||
                   filePath.EndsWith(".component.css", StringComparison.OrdinalIgnoreCase) ||
                   filePath.EndsWith(".component.scss", StringComparison.OrdinalIgnoreCase);
        }

        public string ProcessContent(string content, FileProcessorOptions options)
        {
            // Apply processing based on options
            content = RemoveCssComments(content);

            if (options.RemoveEmptyLines)
            {
                content = RemoveEmptyLines(content);
            }

            if (options.OptimizeWhitespace)
            {
                content = OptimizeWhitespace(content);
            }

            return content;
        }

        #region Content Processing Methods

        private static string RemoveEmptyLines(string content)
        {
            content = Regex.Replace(content, @"^\s*$\n\s*$\n", "", RegexOptions.Multiline);
            return content;
        }

        private static string OptimizeWhitespace(string content)
        {
            // Remove unnecessary whitespace while preserving selector structure
            content = Regex.Replace(content, @"[\t ]+", " ");
            content = Regex.Replace(content, @"[ \t]*{[ \t]*", "{");
            content = Regex.Replace(content, @"[ \t]*}[ \t]*", "}");
            content = Regex.Replace(content, @"[ \t]*:[ \t]*", ":");
            content = Regex.Replace(content, @"[ \t]*;[ \t]*", ";");
            content = Regex.Replace(content, @"[ \t]*,[ \t]*", ",");
            content = Regex.Replace(content, @"[ \t]+$", "", RegexOptions.Multiline);
            return content;
        }

        private static string RemoveCssComments(string content)
        {
            // Remove CSS comments /* ... */
            return Regex.Replace(content, @"/\*[\s\S]*?\*/", "");
        }

        #endregion
    }
}
