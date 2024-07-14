using System.IO.Pipes;
using System.Text;
using HealthyTiger.CSV;

namespace ParseCSVTest
{
    [TestClass]
    public class ParseTest
    {
        public static bool CsvEquals(List<List<string>> a, List<List<string>> b)
        {
            if (a.Count != b.Count) return false;

            for (int i = 0; i < a.Count; i++)
            {
                var ai = a[i];
                var bi = b[i];

                if (ai.Count != bi.Count)
                {
                    return false;
                }

                for (int j = 0; j < ai.Count; j++)
                {
                    var astr = ai[j];
                    var bstr = bi[j];

                    if (!astr.Equals(bstr))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [TestMethod]
        public async Task SingleLineAsync()
        {
            string[] src = new string[] {
                "1,2,3,10,20,30",
                "1,2,\"(x,y)\",3,4,\"[a,b]\"",
                "1,2,\"(x,y)\",3,4,\"[a,b]\",5,6",
                "\"(x,y)\",3,4,\"[a,b]\",5,6",
                "\"1\",2,3,4",
                "1,\"2\",3,4",
                "1,2,3,\"4\"",
                "1,2,\"\",4",
            };

            List<List<string>>[] expected = [
                [["1","2","3","10","20","30"]],
                [["1","2","(x,y)","3","4","[a,b]"]],
                [["1","2","(x,y)","3","4","[a,b]","5","6"]],
                [["(x,y)","3","4","[a,b]","5","6"]],
                [["1","2","3","4"]],
                [["1","2","3","4"]],
                [["1","2","3","4"]],
                [["1","2","","4"]],
            ];

            for (int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                List<List<string>> e = expected[i];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = await Parser.ParseAsync(ms, ',', CancellationToken.None);
                Assert.AreEqual(CsvEquals(e, ans), true);
            }

        }

        [TestMethod]
        public async Task MultiLineAsync()
        {
            string src = string.Join("\r\n", [
                "1,2,\"(x\r\n,y)\",3,4,\"[\r\na,b]\"",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,b]\r\n\",5,6",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,\r\nb]\",5,6",
                "\"(\r\nx,y)\",3,4,\"[a,b]\",5,6",
                "\"(\r\nx,y)\",\"3\",4,\"[a,b]\",5,6",
            ]);
            List<List<string>> expected =
            [
                ["1","2","(x\n,y)","3","4","[\na,b]"],
                ["1","2","(x\n,y)","3","4","[a,b]\n","5","6"],
                ["1","2","(x\n,y)","3","4","[a,\nb]","5","6"],
                ["(\nx,y)","3","4","[a,b]","5","6"],
                ["(\nx,y)","3","4","[a,b]","5","6"],
            ];
            MemoryStream ms = new(Encoding.UTF8.GetBytes(src));
            List<List<string>> ans = await Parser.ParseAsync(ms, ',', CancellationToken.None);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void SingleLine()
        {
            string[] src = new string[] {
                "1,2,3,10,20,30",
                "1,2,\"(x,y)\",3,4,\"[a,b]\"",
                "1,2,\"(x,y)\",3,4,\"[a,b]\",5,6",
                "\"(x,y)\",3,4,\"[a,b]\",5,6",
                "\"1\",2,3,4",
                "1,\"2\",3,4",
                "1,2,3,\"4\"",
                "1,2,\"\",4",
            };

            List<List<string>>[] expected = [
                [["1","2","3","10","20","30"]],
                [["1","2","(x,y)","3","4","[a,b]"]],
                [["1","2","(x,y)","3","4","[a,b]","5","6"]],
                [["(x,y)","3","4","[a,b]","5","6"]],
                [["1","2","3","4"]],
                [["1","2","3","4"]],
                [["1","2","3","4"]],
                [["1","2","","4"]],
            ];

            for (int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                List<List<string>> e = expected[i];
                List<List<string>> ans = RFC4180.ParseString(s);
                Assert.AreEqual(CsvEquals(e, ans), true);
            }
        }

        [TestMethod]
        public void MultiLine()
        {
            string src = string.Join("\r\n", [
                "1,2,\"(x\r\n,y)\",3,4,\"[\r\na,b]\"",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,b]\r\n\",5,6",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,\r\nb]\",5,6",
                "\"(\r\nx,y)\",3,4,\"[a,b]\",5,6",
                "\"(\r\nx,y)\",\"3\",4,\"[a,b]\",5,6",
            ]);
            // TODO ññîˆÇ…â¸çsÇ™Ç†ÇÈèÍçáÇ∆Ç»Ç¢èÍçá
            List<List<string>> expected =
            [
                ["1","2","(x\n,y)","3","4","[\na,b]"],
                ["1","2","(x\n,y)","3","4","[a,b]\n","5","6"],
                ["1","2","(x\n,y)","3","4","[a,\nb]","5","6"],
                ["(\nx,y)","3","4","[a,b]","5","6"],
                ["(\nx,y)","3","4","[a,b]","5","6"],
            ];
            MemoryStream ms = new(Encoding.UTF8.GetBytes(src));
            List<List<string>> ans = Parser.Parse(ms, ',');
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void SingleLineString()
        {
            string[] src = new string[] {
                "1,2,3,10,20,30",
                "1,2,\"(x,y)\",3,4,\"[a,b]\"",
                "1,2,\"(x,y)\",3,4,\"[a,b]\",5,6",
                "\"(x,y)\",3,4,\"[a,b]\",5,6",
                "\"1\",2,3,4",
                "1,\"2\",3,4",
                "1,2,3,\"4\"",
                "1,2,\"\",4",
            };

            List<List<string>>[] expected = [
                [["1","2","3","10","20","30"]],
                [["1","2","(x,y)","3","4","[a,b]"]],
                [["1","2","(x,y)","3","4","[a,b]","5","6"]],
                [["(x,y)","3","4","[a,b]","5","6"]],
                [["1","2","3","4"]],
                [["1","2","3","4"]],
                [["1","2","3","4"]],
                [["1","2","","4"]],
            ];

            for (int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                List<List<string>> e = expected[i];
                List<List<string>> ans = Parser.Parse(s, ',');
                Assert.AreEqual(CsvEquals(e, ans), true);
            }
        }

        [TestMethod]
        public void MultiLineString()
        {
            string src = string.Join("\r\n", [
                "1,2,\"(x\r\n,y)\",3,4,\"[\r\na,b]\"",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,b]\r\n\",5,6",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,\r\nb]\",5,6",
                "\"(\r\nx,y)\",3,4,\"[a,b]\",5,6",
                "\"(\r\nx,y)\",\"3\",4,\"[a,b]\",5,6",
            ]);
            List<List<string>> expected =
            [
                ["1","2","(x\n,y)","3","4","[\na,b]"],
                ["1","2","(x\n,y)","3","4","[a,b]\n","5","6"],
                ["1","2","(x\n,y)","3","4","[a,\nb]","5","6"],
                ["(\nx,y)","3","4","[a,b]","5","6"],
                ["(\nx,y)","3","4","[a,b]","5","6"],
            ];
            List<List<string>> ans = Parser.Parse(src, ',');
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public async Task CancelStreamAsync()
        {
            using AnonymousPipeServerStream server = new(direction: PipeDirection.Out);
            using AnonymousPipeClientStream client = new(PipeDirection.In, server.GetClientHandleAsString());
            CancellationTokenSource cts = new();

            Task writer = Task.Run(() =>
            {
                Thread.Sleep(100);
                using StreamWriter sw = new(server);
                for (int i = 0; i < 10; i++)
                {
                    sw.WriteLine($"{i},1,2,3");
                }
                cts.Cancel();
            });

            bool isCanceled = false;
            List<List<string>>? lines = null;
            try
            {
                lines = await Parser.ParseAsync(client, ',', cts.Token);
            }
            catch (OperationCanceledException)
            {
                isCanceled = true;
            }

            await writer;

            Assert.AreEqual(true, isCanceled);
        }
    }
}