﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HealthyTiger.CSVParser
{
    public class Parser
    {
        private static async IAsyncEnumerable<string> LinesFromStreamAsync(Stream stream, [EnumeratorCancellation] CancellationToken token = default)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                while(!token.IsCancellationRequested)
                {
                    string line = await sr.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }
                    yield return line;
                }
                if(token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
            }
        }

        public static async Task<List<List<string>>> ParseStreamAsync(Stream stream, char sep, CancellationToken token = default)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                List<string> items = new List<string>();
                List<List<string>> lines = new List<List<string>>();
                bool inQuote = false;
                char[] sepchars = new char[] { sep };
                string sepstr = new string(sepchars);

                await foreach(string line in LinesFromStreamAsync(stream, token))
                {
                    string[] cols = line.Split(sepchars, StringSplitOptions.None);
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
                return lines;
            }
        }
    }
}
