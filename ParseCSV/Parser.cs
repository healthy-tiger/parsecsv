using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HealthyTiger.CSVParser
{
    public class Parser
    {
        private static IEnumerable<string> LinesFromString(string src)
        {
            foreach (string line in src.Split('\n'))
            {
                yield return line.TrimEnd('\r');
            }
        }

        private static List<List<string>> Parse_(IEnumerable<string> srclines, char sep)
        {
            List<string> items = new List<string>();
            List<List<string>> lines = new List<List<string>>();
            bool inQuote = false;
            char[] sepchars = new char[] { sep };
            string sepstr = new string(sepchars);

            foreach (string s in srclines)
            {
                ParseLine(s, sepchars, sepstr, ref inQuote, items, lines);
            }
            return lines;
        }

        public static List<List<string>> Parse(Stream stream, char sep)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                return Parse_(LinesFromStream(stream), sep);
            }
        }

        public static List<List<string>> Parse(string src, char sep)
        {
            return Parse_(LinesFromString(src), sep);
        }

        private static IEnumerable<string> LinesFromStream(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = sr.ReadLine();
                }
            }
        }

        private static async IAsyncEnumerable<string> LinesFromStreamAsync(Stream stream, [EnumeratorCancellation] CancellationToken token = default)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                while (!token.IsCancellationRequested)
                {
                    string line = await sr.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }
                    yield return line;
                }
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
            }
        }

        private static void ParseLine(string s, char[] sepchars, string sepstr, ref bool inQuote, List<string> items, List<List<string>> lines)
        {
            string[] cols = s.Split(sepchars, StringSplitOptions.None);
            bool isLeftMostColumn = true;
            foreach (string col in cols)
            {
                if (inQuote)
                {
                    var tmp = items[items.Count - 1];
                    if (!isLeftMostColumn)
                    {
                        tmp += sepstr;
                    }
                    var t = col.TrimEnd();
                    if (t.EndsWith("\""))
                    {
                        if (items.Count == 0)
                        {
                            throw new FormatException("unmatched double quotes");
                        }
                        tmp += t.Substring(0, t.Length - 1);
                        inQuote = false;
                    }
                    else
                    {
                        tmp += col;
                    }
                    items[items.Count - 1] = tmp;
                }
                else
                {
                    var t = col.TrimStart();
                    if (t.StartsWith("\""))
                    {
                        var trim = t.TrimEnd();
                        if (trim.EndsWith("\""))
                        {
                            items.Add(trim.Substring(1, trim.Length - 2));
                        }
                        else
                        {
                            items.Add(t.Substring(1));
                            inQuote = true;
                        }
                    }
                    else
                    {
                        items.Add(col.Trim());
                    }
                }
                isLeftMostColumn = false;
            }
            if (inQuote)
            {
                items[items.Count - 1] += "\n";
            }
            else
            {
                lines.Add(items);
                items = new List<string>();
            }
        }

        private static async Task<List<List<string>>> ParseAsync_(IAsyncEnumerable<string> srclines, char sep, CancellationToken token = default)
        {
            List<string> items = new List<string>();
            List<List<string>> lines = new List<List<string>>();
            bool inQuote = false;
            char[] sepchars = new char[] { sep };
            string sepstr = new string(sepchars);

            await foreach (string s in srclines)
            {
                ParseLine(s, sepchars, sepstr, ref inQuote, items, lines);
            }
            return lines;
        }

        public static async Task<List<List<string>>> ParseAsync(Stream stream, char sep, CancellationToken token = default)
        {
            return await ParseAsync_(LinesFromStreamAsync(stream), sep, token);
        }
    }
}
