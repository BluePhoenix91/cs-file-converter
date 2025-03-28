using CSFileConverter.Interfaces;
using CSFileConverter.Models;

using System.Text.RegularExpressions;

namespace CSFileConverter.Processors
{
    /// <summary>
    /// Processes Python source files
    /// </summary>
    public class PythonFileProcessor : IFileProcessor
    {
        public string FileExtension => ".py";

        public bool CanProcess(string filePath)
        {
            return Path.GetExtension(filePath).Equals(FileExtension, StringComparison.OrdinalIgnoreCase);
        }

        public string ProcessContent(string content, FileProcessorOptions options)
        {
            // Apply processing based on options
            content = RemovePythonComments(content);

            if (options.RemoveDocumentation)
            {
                content = RemovePythonDocstrings(content);
            }

            if (options.RemoveEmptyLines)
            {
                content = RemoveEmptyLines(content);
            }

            if (options.RemoveRegions)
            {
                content = RemovePythonRegions(content);
            }

            if (options.OptimizeWhitespace)
            {
                content = OptimizeWhitespace(content);
            }

            return content;
        }

        #region Content Processing Methods

        private static string RemovePythonDocstrings(string content)
        {
            // Remove triple-quoted docstrings
            content = Regex.Replace(content,
                @"(?<!\S)(?:'''|""{3})[\s\S]*?(?:'''|""{3})",
                "", RegexOptions.Multiline);

            return content;
        }

        private static string RemoveEmptyLines(string content)
        {
            content = Regex.Replace(content, @"^\s*$\n\s*$\n", "", RegexOptions.Multiline);
            return content;
        }

        private string RemovePythonRegions(string content)
        {
            // Remove Python-style region markers (# region and # endregion)
            content = Regex.Replace(content, @"^\s*#\s*region.*$\n?", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"^\s*#\s*endregion.*$\n?", "", RegexOptions.Multiline);
            return content;
        }

        private static string OptimizeWhitespace(string content)
        {
            // Remove leading whitespace on each line (careful with Python indentation!)
            // This is a simplified version - for Python we would need to be more careful
            content = Regex.Replace(content, @"[ \t]+$", "", RegexOptions.Multiline);
            return content;
        }

        private static string RemovePythonComments(string content)
        {
            var lines = content.Split('\n');
            var result = new List<string>();
            bool inMultiLineString = false;
            int quoteCount = 0;
            string stringDelimiter = "";

            foreach (var line in lines)
            {
                string processedLine = line;

                // Skip processing if we're in a multi-line string
                if (inMultiLineString)
                {
                    result.Add(processedLine);

                    // Check if this line ends the multi-line string
                    if (processedLine.Contains(stringDelimiter))
                    {
                        int delimiterPos = 0;
                        while ((delimiterPos = processedLine.IndexOf(stringDelimiter, delimiterPos)) >= 0)
                        {
                            // Check if the delimiter is escaped
                            if (delimiterPos > 0 && processedLine[delimiterPos - 1] == '\\')
                            {
                                int backslashCount = 0;
                                int pos = delimiterPos - 1;
                                while (pos >= 0 && processedLine[pos] == '\\')
                                {
                                    backslashCount++;
                                    pos--;
                                }

                                // If even number of backslashes, it's an escaped quote
                                if (backslashCount % 2 == 0)
                                {
                                    quoteCount++;
                                    if (quoteCount % 2 == 0)
                                    {
                                        inMultiLineString = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                quoteCount++;
                                if (quoteCount % 2 == 0)
                                {
                                    inMultiLineString = false;
                                    break;
                                }
                            }

                            delimiterPos += stringDelimiter.Length;
                        }
                    }

                    continue;
                }

                // Check for triple quoted strings (potential docstrings)
                if (processedLine.Contains("'''") || processedLine.Contains("\"\"\""))
                {
                    bool tripleDoubleQuoteFound = processedLine.Contains("\"\"\"");
                    bool tripleSingleQuoteFound = processedLine.Contains("'''");

                    stringDelimiter = tripleDoubleQuoteFound ? "\"\"\"" : "'''";

                    // Count occurrences to see if we're entering or exiting a multi-line string
                    quoteCount = Regex.Matches(processedLine, Regex.Escape(stringDelimiter)).Count;

                    if (quoteCount % 2 != 0)
                    {
                        inMultiLineString = true;
                    }

                    result.Add(processedLine);
                    continue;
                }

                // Handle regular comments (not in strings)
                int commentPos = processedLine.IndexOf('#');
                if (commentPos >= 0)
                {
                    // Check if the # is inside a string
                    bool inString = false;
                    char currentStringChar = '\0';

                    for (int i = 0; i < commentPos; i++)
                    {
                        if ((processedLine[i] == '\'' || processedLine[i] == '\"') &&
                            (i == 0 || processedLine[i - 1] != '\\'))
                        {
                            if (!inString)
                            {
                                inString = true;
                                currentStringChar = processedLine[i];
                            }
                            else if (processedLine[i] == currentStringChar)
                            {
                                inString = false;
                            }
                        }
                    }

                    if (!inString)
                    {
                        processedLine = processedLine.Substring(0, commentPos).TrimEnd();
                    }
                }

                if (!string.IsNullOrWhiteSpace(processedLine))
                {
                    result.Add(processedLine);
                }
            }

            return string.Join("\n", result);
        }

        #endregion
    }
}
