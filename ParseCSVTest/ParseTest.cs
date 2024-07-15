using System.IO.Pipes;
using System.Text;
using HealthyTiger.Data.CSV;

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
        public void TestRFC4180_1()
        {
            string src = "aaa,bbb,ccc\r\nzzz,yyy,xxx\r\n";
            List<List<string>> expected = [["aaa","bbb","ccc"],["zzz","yyy","xxx"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void TestRFC4180_2()
        {
            string src = "aaa,bbb,ccc\r\nzzz,yyy,xxx";
            List<List<string>> expected = [["aaa", "bbb", "ccc"], ["zzz", "yyy", "xxx"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void TestRFC4180_3()
        {
            string src = "aaa,bbb,ccc\r\n";
            List<List<string>> expected = [["aaa","bbb","ccc"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void TestRFC4180_4()
        {
            string src = "aaa,bbb,ccc";
            List<List<string>> expected = [["aaa","bbb","ccc"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void TestRFC4180_5()
        {
            string src = "\"aaa\",\"bbb\",\"ccc\"\r\nzzz,yyy,xxx";
            List<List<string>> expected = [["aaa","bbb","ccc"],["zzz","yyy","xxx"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void TestRFC4180_6()
        {
            string src = "\"aaa\",\"b\r\nbb\",\"ccc\"\r\nzzz,yyy,xxx";
            List<List<string>> expected = [["aaa","b\r\nbb","ccc"],["zzz","yyy","xxx"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void TestRFC4180_7()
        {
            string src = "\"aaa\",\"b\"\"bb\",\"ccc\"";
            List<List<string>> expected = [["aaa","b\"bb","ccc"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void TestRFC4180_8()
        {
            string src = "zzz,yyy,xxx\r\n\"aaa\",\"bbb\",\"ccc\"\r\n";
            List<List<string>> expected = [["zzz", "yyy", "xxx"],["aaa","bbb","ccc"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }

        [TestMethod]
        public void TestRFC4180_9()
        {
            string src = "zzz,yyy,xxx\r\n\"aaa\",\"bbb\",\"ccc\"";
            List<List<string>> expected = [["zzz", "yyy", "xxx"],["aaa","bbb","ccc"]];

            List<List<string>> ans = RFC4180.ParseString(src);
            Assert.AreEqual(CsvEquals(expected, ans), true);
        }
    }
}