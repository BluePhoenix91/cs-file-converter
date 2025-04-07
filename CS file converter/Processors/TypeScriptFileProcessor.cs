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
    /// Processes TypeScript/Angular source files
    /// </summary>
    public class TypeScriptFileProcessor : IFileProcessor
    {
        public string FileExtension => ".ts";

        public bool CanProcess(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(FileExtension, StringComparison.OrdinalIgnoreCase) ||
                   filePath.EndsWith(".component.ts", StringComparison.OrdinalIgnoreCase);
        }

        public string ProcessContent(string content, FileProcessorOptions options)
        {
            // Apply processing based on options
            content = RemoveTypeScriptComments(content);

            if (options.RemoveDocumentation)
            {
                content = RemoveTypeScriptDocumentation(content);
            }

            if (options.RemoveEmptyLines)
            {
                content = RemoveEmptyLines(content);
            }

            if (options.RemoveRegions)
            {
                content = RemoveTypeScriptRegions(content);
            }

            if (options.OptimizeWhitespace)
            {
                content = OptimizeWhitespace(content);
            }

            return content;
        }

        #region Content Processing Methods

        private static string RemoveTypeScriptDocumentation(string content)
        {
            // Remove JSDoc style comments (/** ... */)
            content = Regex.Replace(content,
                @"/\*\*[\s\S]*?\*/",
                "", RegexOptions.Multiline);

            return content;
        }

        private static string RemoveEmptyLines(string content)
        {
            content = Regex.Replace(content, @"^\s*$\n\s*$\n", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"{\s*\n\s*\n", "{\n");
            content = Regex.Replace(content, @"\n\s*\n\s*}", "\n}");
            return content;
        }

        private string RemoveTypeScriptRegions(string content)
        {
            // Remove TypeScript region indicators if present
            // For example: // #region NAME and // #endregion
            content = Regex.Replace(content, @"^\s*//\s*#region.*$\n?", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"^\s*//\s*#endregion.*$\n?", "", RegexOptions.Multiline);
            return content;
        }

        private static string OptimizeWhitespace(string content)
        {
            // Remove trailing whitespace on each line
            content = Regex.Replace(content, @"[ \t]+$", "", RegexOptions.Multiline);

            // Collapse multiple spaces into single spaces (except in string literals)
            content = Regex.Replace(content, @"([^""\r\n])([ \t]{2,})", "$1 ", RegexOptions.Multiline);

            return content;
        }

        private static string RemoveTypeScriptComments(string content)
        {
            var lines = content.Split('\n');
            var result = new List<string>();
            bool inMultiLineComment = false;
            bool inTemplate = false;
            string templateDelimiter = "";

            foreach (var line in lines)
            {
                // Skip processing if inside a multiline comment
                if (inMultiLineComment)
                {
                    int endIndex = line.IndexOf("*/");
                    if (endIndex != -1)
                    {
                        inMultiLineComment = false;
                        string remainingLine = line.Substring(endIndex + 2);
                        if (!string.IsNullOrWhiteSpace(remainingLine))
                        {
                            result.Add(ProcessLineForComments(remainingLine, ref inTemplate, ref templateDelimiter));
                        }
                    }
                    continue;
                }

                // Handle template literals which can span multiple lines in TypeScript
                if (inTemplate)
                {
                    int endIndex = line.IndexOf(templateDelimiter);
                    if (endIndex != -1 && !IsEscaped(line, endIndex))
                    {
                        inTemplate = false;
                        templateDelimiter = "";
                    }
                    result.Add(line); // Keep template literals intact
                    continue;
                }

                result.Add(ProcessLineForComments(line, ref inTemplate, ref templateDelimiter));
            }

            return string.Join("\n", result);
        }

        private static string ProcessLineForComments(string line, ref bool inTemplate, ref string templateDelimiter)
        {
            // Check for template literal start
            if (!inTemplate)
            {
                int backtickIndex = line.IndexOf("`");
                if (backtickIndex != -1 && !IsEscaped(line, backtickIndex))
                {
                    inTemplate = true;
                    templateDelimiter = "`";
                    return line; // Keep template literals intact
                }
            }

            // Process multi-line comment start
            int multiLineStart = line.IndexOf("/*");
            if (multiLineStart != -1 && !IsEscaped(line, multiLineStart))
            {
                int multiLineEnd = line.IndexOf("*/", multiLineStart + 2);
                if (multiLineEnd != -1)
                {
                    // Remove the comment section
                    string beforeComment = line.Substring(0, multiLineStart);
                    string afterComment = line.Substring(multiLineEnd + 2);
                    return ProcessLineForComments(beforeComment + afterComment, ref inTemplate, ref templateDelimiter);
                }
                else
                {
                    // Start of a multi-line comment
                    inTemplate = false;
                    string result = line.Substring(0, multiLineStart).TrimEnd();
                    return string.IsNullOrWhiteSpace(result) ? "" : result;
                }
            }

            // Process single-line comments
            int singleLineComment = line.IndexOf("//");
            if (singleLineComment != -1 && !IsEscaped(line, singleLineComment))
            {
                string result = line.Substring(0, singleLineComment).TrimEnd();
                return string.IsNullOrWhiteSpace(result) ? "" : result;
            }

            return line;
        }

        private static bool IsEscaped(string text, int position)
        {
            if (position <= 0)
                return false;

            int backslashCount = 0;
            int index = position - 1;
            while (index >= 0 && text[index] == '\\')
            {
                backslashCount++;
                index--;
            }

            return backslashCount % 2 != 0;
        }

        #endregion
    }
}
