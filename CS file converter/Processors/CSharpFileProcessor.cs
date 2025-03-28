using CSFileConverter.Interfaces;
using CSFileConverter.Models;

using System.Text.RegularExpressions;

namespace CSFileConverter.Processors
{
    /// <summary>
    /// Processes C# source files
    /// </summary>
    public class CSharpFileProcessor : IFileProcessor
    {
        public string FileExtension => ".cs";

        public bool CanProcess(string filePath)
        {
            return Path.GetExtension(filePath).Equals(FileExtension, StringComparison.OrdinalIgnoreCase);
        }

        public string ProcessContent(string content, FileProcessorOptions options)
        {
            // Apply processing based on options
            content = RemoveComments(content);

            if (options.RemoveDocumentation)
            {
                content = RemoveXmlDocumentation(content);
            }

            if (options.RemoveEmptyLines)
            {
                content = RemoveEmptyLines(content);
            }

            if (options.RemoveRegions)
            {
                content = RemoveRegions(content);
            }

            if (options.OptimizeWhitespace)
            {
                content = OptimizeWhitespace(content);
            }

            return content;
        }

        #region Content Processing Methods

        private static string RemoveXmlDocumentation(string content)
        {
            content = Regex.Replace(content, @"^\s*///.*$\n?", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"/\*\*(?!\*)[\s\S]*?\*/", "");
            return content;
        }

        private static string RemoveEmptyLines(string content)
        {
            content = Regex.Replace(content, @"^\s*$\n\s*$\n", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"{\s*\n\s*\n", "{\n");
            content = Regex.Replace(content, @"\n\s*\n\s*}", "\n}");
            return content;
        }

        private static string RemoveRegions(string content)
        {
            // Remove #region and #endregion directives
            content = Regex.Replace(content, @"^\s*#region.*$\n?", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"^\s*#endregion.*$\n?", "", RegexOptions.Multiline);
            return content;
        }

        private static string OptimizeWhitespace(string content)
        {
            // Remove leading whitespace on each line
            content = Regex.Replace(content, @"^[ \t]+", "", RegexOptions.Multiline);

            // Collapse multiple spaces into single spaces (except in string literals)
            content = Regex.Replace(content, @"([^""\r\n])([ \t]{2,})", "$1 ", RegexOptions.Multiline);

            // Remove spaces around operators to reduce token count
            content = Regex.Replace(content, @" *([+\-*/=<>!&|:;,.()\[\]{}]) *", "$1", RegexOptions.Multiline);

            // Add back spacing for readability in some cases
            content = Regex.Replace(content, @"([=<>!&|]{2})", " $1 ", RegexOptions.Multiline);
            content = Regex.Replace(content, @"([=<>!&|])([^=<>!&|])", " $1 $2", RegexOptions.Multiline);

            // Remove trailing whitespace
            content = Regex.Replace(content, @"[ \t]+$", "", RegexOptions.Multiline);

            return content;
        }

        private static string RemoveComments(string content)
        {
            var lines = content.Split('\n');
            var result = new List<string>();
            bool inMultiLineComment = false;
            string multiLineBuffer = "";

            foreach (var line in lines)
            {
                if (inMultiLineComment)
                {
                    int endIndex = line.IndexOf("*/");
                    if (endIndex != -1)
                    {
                        inMultiLineComment = false;
                        string remainingLine = line.Substring(endIndex + 2);
                        if (!string.IsNullOrWhiteSpace(remainingLine))
                        {
                            multiLineBuffer += remainingLine;
                            result.Add(multiLineBuffer.Trim());
                        }
                        multiLineBuffer = "";
                    }
                    continue;
                }

                string currentLine = line;
                int multiLineStart = currentLine.IndexOf("/*");

                if (multiLineStart != -1)
                {
                    int multiLineEnd = currentLine.IndexOf("*/", multiLineStart);
                    if (multiLineEnd != -1)
                    {
                        currentLine = currentLine.Remove(multiLineStart, multiLineEnd - multiLineStart + 2);
                    }
                    else
                    {
                        inMultiLineComment = true;
                        multiLineBuffer = currentLine.Substring(0, multiLineStart).Trim();
                        continue;
                    }
                }

                int commentIndex = currentLine.IndexOf("//");
                if (commentIndex != -1)
                {
                    currentLine = currentLine.Substring(0, commentIndex);
                }

                if (!string.IsNullOrWhiteSpace(currentLine))
                {
                    result.Add(currentLine.TrimEnd());
                }
            }

            return string.Join("\n", result);
        }

        #endregion
    }
}
