using System.Text;

namespace ReadSV
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string[] singleLineTests =
            {
                "1,2,3,10,20,30",
                "1,2,\"(x,y)\",3,4,\"[a,b]\"",
                "1,2,\"(x,y)\",3,4,\"[a,b]\",5,6",
                "\"(x,y)\",3,4,\"[a,b]\",5,6",
            };
            string[] multiLineTests =
            {
                "1,2,\"(x\r\n,y)\",3,4,\"[\r\na,b]\"",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,b]\r\n\",5,6",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,\r\nb]\",5,6",
                "\"(\r\nx,y)\",3,4,\"[a,b]\",5,6",
            };

            foreach (string line in singleLineTests)
            {
                Stream s = new MemoryStream(Encoding.UTF8.GetBytes(line));
                List<string>[] rows = await ReadSV(s, ',', CancellationToken.None);
                Console.WriteLine(line);
                foreach (List<string> row in rows)
                {
                    foreach (string column in row)
                    {
                        Console.WriteLine($"\t\"{column}\"");
                    }
                }
            }

            foreach (string line in multiLineTests)
            {
                Stream s = new MemoryStream(Encoding.UTF8.GetBytes(line));
                List<string>[] rows = await ReadSV(s, ',', CancellationToken.None);
                Console.WriteLine(line);
                foreach (List<string> row in rows)
                {
                    foreach (string column in row)
                    {
                        Console.WriteLine($"\t\"{column}\"");
                    }
                }
            }
        }

        static async Task<List<string>[]> ReadSV(Stream stream, char sep, CancellationToken token)
        {
            using StreamReader sr = new StreamReader(stream);
            List<string> items = [];
            List<List<string>> lines = [];
            bool inQuote = false;
            string sepstr = new string([sep]);

            string? line = null;
            while (true)
            {
                line = await sr.ReadLineAsync(token);
                if (line == null)
                {
                    break;
                }
                var cols = line.Split(sep, StringSplitOptions.None).Select(c => c.Trim());
                bool isLeftMostColumn = true;
                foreach (var col in cols)
                {
                    if (inQuote)
                    {
                        var tmp = items[items.Count - 1];
                        if (!isLeftMostColumn)
                        {
                            tmp += sepstr;
                        }
                        var t = col.TrimEnd();
                        if (t.EndsWith('\"'))
                        {
                            if (items.Count == 0)
                            {
                                throw new FormatException("unexpected double quote");
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
                        if (t.StartsWith('\"'))
                        {
                            items.Add(t.Substring(1));
                            inQuote = true;
                        }
                        else
                        {
                            items.Add(t.TrimEnd());
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
                    items = [];
                }
            }
            return lines.ToArray();
        }
    }
}
