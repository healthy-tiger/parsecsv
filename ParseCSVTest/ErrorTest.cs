using HealthyTiger.CSV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseCSVTest
{
    [TestClass]
    public class ErrorTest
    {
        [TestMethod]
        public void EmptyString()
        {
            {
                string s = "";
                List<List<string>> e = [];
                List<List<string>> ans = Parser.Parse(s, ',');
                Assert.AreEqual(ParseTest.CsvEquals(e, ans), true);
            }

            {
                string s = "\n";
                List<List<string>> e = [[""],[""]];
                List<List<string>> ans = Parser.Parse(s, ',');
                Assert.AreEqual(ParseTest.CsvEquals(e, ans), true);
            }
        }

        [TestMethod]
        public void EmptyStream()
        {
            {
                string s = "";
                List<List<string>> e = [];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = Parser.Parse(ms, ',');
                Assert.AreEqual(ParseTest.CsvEquals(e, ans), true);
            }

            {
                string s = "\n";
                List<List<string>> e = [[""]];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = Parser.Parse(ms, ',');
                Assert.AreEqual(ParseTest.CsvEquals(e, ans), true);
            }
        }

        [TestMethod]
        public async Task EmptyStreamAsync()
        {
            {
                string s = "";
                List<List<string>> e = [];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = await Parser.ParseAsync(ms, ',', CancellationToken.None);
                Assert.AreEqual(ParseTest.CsvEquals(e, ans), true);
            }

            {
                string s = "\n";
                List<List<string>> e = [[""]];
                MemoryStream ms = new(Encoding.UTF8.GetBytes(s));
                List<List<string>> ans = await Parser.ParseAsync(ms, ',', CancellationToken.None);
                Assert.AreEqual(ParseTest.CsvEquals(e, ans), true);
            }
        }

        [TestMethod]
        public void IncompleteQuotedString()
        {
            string src = string.Join("\r\n", [
                "1,2,\"(x\r\n,y)\",3,4,\"[\r\na,b]\"",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,b]\r\n\",5,6",
                "1,2,\"(x\r\n,y)\",3,4,\"[a,\r\nb]\",5,6",
                "\"(\r\nx,y)\",3,4,\"[a,b],5,6",
                "\"(\r\nx,y)\",\"3\",4,\"[a,b]\",5,6",
            ]);
            Assert.ThrowsException<FormatException>(() => Parser.Parse(src, ','));
        }
    }
}
