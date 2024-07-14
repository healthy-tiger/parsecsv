using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HealthyTiger.CSV
{
    public class Parser
    {
        private static IEnumerable<string> LinesFromString(string src)
        {
            if (src.Length == 0)
            {
                yield break;
            }
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
            if (inQuote)
            {
                throw new FormatException("There is no closing quotation mark.");
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
                    var tmp = items[^1];
                    if (!isLeftMostColumn)
                    {
                        tmp += sepstr;
                    }
                    if (col.EndsWith("\""))
                    {
                        if (col.Count(c => c == '\"') % 2 == 1)
                        {
                            tmp += col.TrimEnd('\"');
                            inQuote = false;
                        }
                        else
                        {
                            tmp += col;
                        }
                    }
                    else
                    {
                        tmp += col;
                    }

                    if (items.Count > 0)
                    {
                        items[^1] = tmp;
                    }
                    else
                    {
                        items.Add(tmp);
                    }
                }
                else
                {
                    if (col.StartsWith("\""))
                    {
                        if (col.EndsWith("\""))
                        {
                            items.Add(col.Trim('\"'));
                        }
                        else
                        {
                            items.Add(col[1..]);
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
                items[^1] += "\n";
            }
            else
            {
                lines.Add(new List<string>(items));
                items.Clear();
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
            if (inQuote)
            {
                throw new FormatException("There is no closing quotation mark.");
            }
            return lines;
        }

        public static async Task<List<List<string>>> ParseAsync(Stream stream, char sep, CancellationToken token = default)
        {
            return await ParseAsync_(LinesFromStreamAsync(stream, token), sep, token);
        }


    }
}
