using System.Text;
using HealthyTiger.CSVParser;

namespace ParseCSVTest
{
    [TestClass]
    public class ParseTest
    {
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

            string[] expected = [
                "1,2,3,10,20,30",
                "1,2,(x,y),3,4,[a,b]",
                "1,2,(x,y),3,4,[a,b],5,6",
                "(x,y),3,4,[a,b],5,6",
                "1,2,3,4",
                "1,2,3,4",
                "1,2,3,4",
                "1,2,,4",
            ];

            for(int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                string e = expected[i];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = await Parser.ParseAsync(ms, ',', CancellationToken.None);
                string a = string.Join("\n", ans.Select(x => string.Join(",", x)));
                Assert.AreEqual(e, a, false);
            }

        }

        [TestMethod]
        public async Task MultiLineAsync()
        {
            string[] src =
            {
                "1,2,\"(x\r\n,y)\",3,4,\"[\r\na,b]\"",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,b]\r\n\",5,6",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,\r\nb]\",5,6",
                "\"(\r\nx,y)\",3,4,\"[a,b]\",5,6",
                "\"(\r\nx,y)\",\"3\",4,\"[a,b]\",5,6",
            };
            string[] expected =
            {
                "1,2,(x\n,y),3,4,[\na,b]",
                "1,2,(x\n,y),3,4,[a,b]\n,5,6",
                "1,2,(x\n,y),3,4,[a,\nb],5,6",
                "(\nx,y),3,4,[a,b],5,6",
                "(\nx,y),3,4,[a,b],5,6",
            };
            for (int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                string e = expected[i];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = await Parser.ParseAsync(ms, ',', CancellationToken.None);
                string a = string.Join("\n", ans.Select(x => string.Join(",", x)));
                Assert.AreEqual(e, a, false);
            }
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

            string[] expected = [
                "1,2,3,10,20,30",
                "1,2,(x,y),3,4,[a,b]",
                "1,2,(x,y),3,4,[a,b],5,6",
                "(x,y),3,4,[a,b],5,6",
                "1,2,3,4",
                "1,2,3,4",
                "1,2,3,4",
                "1,2,,4",
            ];

            for (int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                string e = expected[i];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = Parser.Parse(ms, ',');
                string a = string.Join("\n", ans.Select(x => string.Join(",", x)));
                Assert.AreEqual(e, a, false);
            }

        }

        [TestMethod]
        public void MultiLine()
        {
            string[] src =
            {
                "1,2,\"(x\r\n,y)\",3,4,\"[\r\na,b]\"",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,b]\r\n\",5,6",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,\r\nb]\",5,6",
                "\"(\r\nx,y)\",3,4,\"[a,b]\",5,6",
                "\"(\r\nx,y)\",\"3\",4,\"[a,b]\",5,6",
            };
            string[] expected =
            {
                "1,2,(x\n,y),3,4,[\na,b]",
                "1,2,(x\n,y),3,4,[a,b]\n,5,6",
                "1,2,(x\n,y),3,4,[a,\nb],5,6",
                "(\nx,y),3,4,[a,b],5,6",
                "(\nx,y),3,4,[a,b],5,6",
            };
            for (int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                string e = expected[i];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = Parser.Parse(ms, ',');
                string a = string.Join("\n", ans.Select(x => string.Join(",", x)));
                Assert.AreEqual(e, a, false);
            }
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

            string[] expected = [
                "1,2,3,10,20,30",
                "1,2,(x,y),3,4,[a,b]",
                "1,2,(x,y),3,4,[a,b],5,6",
                "(x,y),3,4,[a,b],5,6",
                "1,2,3,4",
                "1,2,3,4",
                "1,2,3,4",
                "1,2,,4",
            ];

            for (int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                string e = expected[i];
                List<List<string>> ans = Parser.Parse(s, ',');
                string a = string.Join("\n", ans.Select(x => string.Join(",", x)));
                Assert.AreEqual(e, a, false);
            }

        }

        [TestMethod]
        public void MultiLineString()
        {
            string[] src =
            {
                "1,2,\"(x\r\n,y)\",3,4,\"[\r\na,b]\"",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,b]\r\n\",5,6",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,\r\nb]\",5,6",
                "\"(\r\nx,y)\",3,4,\"[a,b]\",5,6",
                "\"(\r\nx,y)\",\"3\",4,\"[a,b]\",5,6",
            };
            string[] expected =
            {
                "1,2,(x\n,y),3,4,[\na,b]",
                "1,2,(x\n,y),3,4,[a,b]\n,5,6",
                "1,2,(x\n,y),3,4,[a,\nb],5,6",
                "(\nx,y),3,4,[a,b],5,6",
                "(\nx,y),3,4,[a,b],5,6",
            };
            for (int i = 0; i < src.Length; i++)
            {
                string s = src[i];
                string e = expected[i];
                List<List<string>> ans = Parser.Parse(s, ',');
                string a = string.Join("\n", ans.Select(x => string.Join(",", x)));
                Assert.AreEqual(e, a, false);
            }
        }
    }
}