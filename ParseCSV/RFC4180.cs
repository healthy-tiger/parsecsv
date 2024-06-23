using System;
using System.Collections.Generic;
using System.Text;

namespace HealthyTiger.CSVParser
{
    public class RFC4180
    {
        private void Init(char c)
        {
            CharType ct = CharTypeTable[c];
            switch (ct)
            {
                case CharType.EOF:
                    if (Fields.Count > 0)
                    {
                        OnEndOfRecord();
                    }
                    CurrentState = EndOfFile;
                    break;

                case CharType.DQUOTE:
                    CurrentState = Escaped;
                    break;

                case CharType.COMMA:
                    OnEndOfField();
                    CurrentState = Init;
                    break;

                case CharType.CR:
                    OnEndOfField();
                    CurrentState = EndOfRecord;
                    break;

                case CharType.TEXTDATA:
                    CurrentState = NonEscaped;
                    FieldBuf.Add(c);
                    break;

                default:
                    throw new FormatException("An unexpected character was encountered.");
            }
        }

        private void Escaped(char c)
        {
            CharType ct = CharTypeTable[c];
            switch (ct)
            {
                case CharType.EOF:
                    throw new FormatException("An unexpected EOF was encountered.");

                case CharType.DQUOTE:
                    CurrentState = EndOfEscaped;
                    break;

                case CharType.COMMA:
                case CharType.CR:
                case CharType.LF:
                case CharType.TEXTDATA:
                    FieldBuf.Add(c);
                    break;

                default:
                    throw new FormatException("An unexpected character was encountered.");
            }
        }

        private void NonEscaped(char c)
        {
            CharType ct = CharTypeTable[c];
            switch (ct)
            {
                case CharType.EOF:
                    OnEndOfField();
                    OnEndOfRecord();
                    CurrentState = EndOfFile;
                    break;

                case CharType.DQUOTE:
                    throw new FormatException("An unexpected double quote was encountered.");

                case CharType.COMMA:
                    OnEndOfField();
                    CurrentState = Init;
                    break;

                case CharType.CR:
                    OnEndOfField();
                    CurrentState = EndOfRecord;
                    break;

                case CharType.TEXTDATA:
                    FieldBuf.Add(c);
                    break;

                default:
                    throw new FormatException("An unexpected character was encountered.");
            }
        }

        private void EndOfEscaped(char c)
        {
            CharType ct = CharTypeTable[c];
            switch (ct)
            {
                case CharType.EOF:
                    OnEndOfField();
                    OnEndOfRecord();
                    CurrentState = EndOfFile;
                    break;

                case CharType.DQUOTE:
                    FieldBuf.Add('"');
                    CurrentState = Escaped;
                    break;

                case CharType.COMMA:
                    OnEndOfField();
                    CurrentState = Init;
                    break;

                case CharType.CR:
                    OnEndOfField();
                    CurrentState = EndOfRecord;
                    break;

                default:
                    throw new FormatException("An unexpected character was encountered.");
            }
        }

        private void EndOfRecord(char c)
        {
            CharType ct = CharTypeTable[c];

            switch (ct)
            {
                case CharType.LF:
                    OnEndOfRecord();
                    CurrentState = Init;
                    break;

                case CharType.EOF:
                    throw new FormatException("An unexpected EOF was encountered.");

                default:
                    throw new FormatException("An unexpected character was encountered.");
            }
        }

        private void EndOfFile(char c) { }

        private readonly List<char> FieldBuf = new List<char>();

        private Action<char> CurrentState;

        private List<string> Fields = new List<string>();

        private List<List<string>> Records_ = new List<List<string>>();

        public List<List<string>> Records {  get => Records_; }

        private enum CharType : byte
        {
            EOF = 0,
            DQUOTE = 1,
            COMMA = 3,
            CR = 4,
            LF = 5,
            TEXTDATA = 6,
            UNKNOWN = byte.MaxValue,
        }

        private readonly CharType[] CharTypeTable = new CharType[char.MaxValue + 1];

        public RFC4180()
        {
            Reset();
            CharTypeTable = CreateDefaultCharTable();
        }

        public void Reset()
        {
            CurrentState = this.Init;
            FieldBuf.Clear();
            Fields.Clear();
            Records_ = new List<List<string>>();
        }

        private static CharType[] CreateDefaultCharTable()
        {
            CharType[] tbl = new CharType[char.MaxValue + 1];
            Array.Fill(tbl, CharType.UNKNOWN);
            tbl[0x00] = CharType.EOF;
            tbl[0x22] = CharType.DQUOTE;
            tbl[0x0A] = CharType.LF;
            tbl[0x0D] = CharType.CR;
            tbl[0x20] = CharType.TEXTDATA;
            tbl[0x21] = CharType.TEXTDATA;
            tbl[0x2C] = CharType.COMMA;
            Array.Fill(tbl, CharType.TEXTDATA, 0x23, 0x2B - 0x23 + 1);
            Array.Fill(tbl, CharType.TEXTDATA, 0x2D, 0x7E - 0x2D + 1);
            Array.Fill(tbl, CharType.TEXTDATA, byte.MaxValue + 1, char.MaxValue - byte.MaxValue);
            return tbl;
        }

        public bool IsEndOfFile
        {
            get => CurrentState == EndOfFile;
        }

        public void Process(char[] src)
        {
            if (src == null)
            {
                CurrentState('\0');
            }
            else
            {
                foreach (char c in src)
                {
                    CurrentState(c);
                }
            }
        }

        public void Process()
        {
            Process(null);
        }

        public static List<List<string>> ParseString(string s)
        {
            char[] chars = s.ToCharArray();
            RFC4180 p = new RFC4180();
            p.Process(chars);
            p.Process(null);
            if(!p.IsEndOfFile)
            {
                throw new FormatException("Incomplete input string");
            }
            return p.Records;
        }

        private void OnEndOfField()
        {
            Fields.Add(new string(FieldBuf.ToArray()));
            FieldBuf.Clear();
        }

        private void OnEndOfRecord()
        {
            Records_.Add(Fields);
            Fields = new List<string>();
        }
    }
}
