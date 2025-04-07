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
    /// Processes HTML/Angular template files
    /// </summary>
    public class HtmlFileProcessor : IFileProcessor
    {
        public string FileExtension => ".html";

        public bool CanProcess(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(FileExtension, StringComparison.OrdinalIgnoreCase) ||
                   filePath.EndsWith(".component.html", StringComparison.OrdinalIgnoreCase);
        }

        public string ProcessContent(string content, FileProcessorOptions options)
        {
            // Apply processing based on options
            content = RemoveHtmlComments(content);

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
            // Remove excess whitespace while preserving content structure
            content = Regex.Replace(content, @">[ \t]+<", "><");
            content = Regex.Replace(content, @"[ \t]+", " ");
            content = Regex.Replace(content, @"[ \t]+$", "", RegexOptions.Multiline);
            return content;
        }

        private static string RemoveHtmlComments(string content)
        {
            // Remove HTML comments <!-- ... -->
            // but be careful with Angular structural directives and bindings
            return Regex.Replace(content, @"<!--(?![(=*+])[^-]*(?:-[^-]+)*-->", "");
        }

        #endregion
    }
}
