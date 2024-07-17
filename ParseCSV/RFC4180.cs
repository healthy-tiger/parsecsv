using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace HealthyTiger.Data.CSV
{
    public unsafe class RFC4180
    {
        private enum ParseState : byte
        {
            Init,
            Escaped,
            NonEscaped,
            EndOfEscaped,
            EndOfRecord,
            EndOfFile,
        }

        private ArrayBufferWriter<char> FieldBuf = new ArrayBufferWriter<char>();

        private ParseState CurrentState;

        private List<string> Fields = new List<string>();

        readonly private List<List<string>> Records_ = new List<List<string>>();

        public List<List<string>> Records { get => Records_; }

        private enum CharType : byte
        {
            EOF = 0x00,
            DQUOTE = 0x22,
            COMMA = 0x2C,
            CR = 0x0D,
            LF = 0x0A,
            TEXTDATA = 6,
            UNKNOWN = byte.MaxValue,
        }

        private readonly CharType[] CharTypeTable = new CharType[char.MaxValue + 1];

        public RFC4180()
        {
            CurrentState = ParseState.Init;
            Records_ = new List<List<string>>();
            CharTypeTable = RFC4180DefaultCharTypeTable;
        }

        private const byte MaxChar = 0x7F;

        private static CharType[] CreateDefaultCharTable()
        {
            CharType[] tbl = new CharType[MaxChar + 1];
            fixed (CharType* pct = &tbl[0])
            {
                *(pct + 0x00) = CharType.EOF;
                *(pct + 0x01) = CharType.UNKNOWN;
                *(pct + 0x02) = CharType.UNKNOWN;
                *(pct + 0x03) = CharType.UNKNOWN;
                *(pct + 0x04) = CharType.UNKNOWN;
                *(pct + 0x05) = CharType.UNKNOWN;
                *(pct + 0x06) = CharType.UNKNOWN;
                *(pct + 0x07) = CharType.UNKNOWN;
                *(pct + 0x08) = CharType.UNKNOWN;
                *(pct + 0x09) = CharType.UNKNOWN;
                *(pct + 0x0A) = CharType.LF;
                *(pct + 0x0B) = CharType.UNKNOWN;
                *(pct + 0x0C) = CharType.UNKNOWN;
                *(pct + 0x0D) = CharType.CR;
                *(pct + 0x0E) = CharType.UNKNOWN;
                *(pct + 0x0F) = CharType.UNKNOWN;
                *(pct + 0x10) = CharType.UNKNOWN;
                *(pct + 0x11) = CharType.UNKNOWN;
                *(pct + 0x12) = CharType.UNKNOWN;
                *(pct + 0x13) = CharType.UNKNOWN;
                *(pct + 0x14) = CharType.UNKNOWN;
                *(pct + 0x15) = CharType.UNKNOWN;
                *(pct + 0x16) = CharType.UNKNOWN;
                *(pct + 0x17) = CharType.UNKNOWN;
                *(pct + 0x18) = CharType.UNKNOWN;
                *(pct + 0x19) = CharType.UNKNOWN;
                *(pct + 0x1A) = CharType.UNKNOWN;
                *(pct + 0x1B) = CharType.UNKNOWN;
                *(pct + 0x1C) = CharType.UNKNOWN;
                *(pct + 0x1D) = CharType.UNKNOWN;
                *(pct + 0x1E) = CharType.UNKNOWN;
                *(pct + 0x1F) = CharType.UNKNOWN;
                *(pct + 0x20) = CharType.TEXTDATA;
                *(pct + 0x21) = CharType.TEXTDATA;
                *(pct + 0x22) = CharType.DQUOTE;
                *(pct + 0x23) = CharType.TEXTDATA;
                *(pct + 0x24) = CharType.TEXTDATA;
                *(pct + 0x25) = CharType.TEXTDATA;
                *(pct + 0x26) = CharType.TEXTDATA;
                *(pct + 0x27) = CharType.TEXTDATA;
                *(pct + 0x28) = CharType.TEXTDATA;
                *(pct + 0x29) = CharType.TEXTDATA;
                *(pct + 0x2A) = CharType.TEXTDATA;
                *(pct + 0x2B) = CharType.TEXTDATA;
                *(pct + 0x2C) = CharType.COMMA;
                CharType* e = pct + MaxChar;
                for (CharType* s = pct + 0x2D; s < e; s++) { *s = CharType.TEXTDATA; }
                *(pct + MaxChar) = CharType.TEXTDATA;
            }
            return tbl;
        }

        private static readonly CharType[] RFC4180DefaultCharTypeTable;

        static RFC4180()
        {
            RFC4180DefaultCharTypeTable = CreateDefaultCharTable();
        }

        public bool IsEndOfFile
        {
            get => CurrentState == ParseState.EndOfFile;
        }

        private static readonly char[] Quote = { '\"' };

        public void Process(char[] src)
        {
            fixed (char* psrc = &src[0])
            {
                fixed (CharType* ctbl = &CharTypeTable[0])
                {
                    char* s = psrc;
                    char* e = psrc + src.Length;
                    while (s < e)
                    {
                        char c = *s;
                        int i = c >= MaxChar ? MaxChar : c % MaxChar;
                        CharType ct = *(ctbl + i);
                        switch (CurrentState)
                        {
                            case ParseState.Init:
                                {
                                    switch (ct)
                                    {
                                        case CharType.EOF:
                                            if (Fields.Count > 0)
                                            {
                                                OnEndOfRecord();
                                            }
                                            CurrentState = ParseState.EndOfFile;
                                            break;

                                        case CharType.DQUOTE:
                                            CurrentState = ParseState.Escaped;
                                            break;

                                        case CharType.COMMA:
                                            OnEndOfField();
                                            CurrentState = ParseState.Init;
                                            break;

                                        case CharType.CR:
                                            OnEndOfField();
                                            CurrentState = ParseState.EndOfRecord;
                                            break;

                                        case CharType.TEXTDATA:
                                            CurrentState = ParseState.NonEscaped;
                                            FieldBuf.Write(new ReadOnlySpan<char>(s, 1));
                                            break;

                                        default:
                                            throw new FormatException("An unexpected character was encountered.");
                                    }
                                }
                                break;

                            case ParseState.Escaped:
                                {
                                    switch (ct)
                                    {
                                        case CharType.EOF:
                                            throw new FormatException("An unexpected EOF was encountered.");

                                        case CharType.DQUOTE:
                                            CurrentState = ParseState.EndOfEscaped;
                                            break;

                                        case CharType.COMMA:
                                        case CharType.CR:
                                        case CharType.LF:
                                        case CharType.TEXTDATA:
                                            FieldBuf.Write(new ReadOnlySpan<char>(s, 1));
                                            break;

                                        default:
                                            throw new FormatException("An unexpected character was encountered.");
                                    }
                                }
                                break;

                            case ParseState.NonEscaped:
                                {
                                    switch (ct)
                                    {
                                        case CharType.EOF:
                                            OnEndOfField();
                                            OnEndOfRecord();
                                            CurrentState = ParseState.EndOfFile;
                                            break;

                                        case CharType.DQUOTE:
                                            throw new FormatException("An unexpected double quote was encountered.");

                                        case CharType.COMMA:
                                            OnEndOfField();
                                            CurrentState = ParseState.Init;
                                            break;

                                        case CharType.CR:
                                            OnEndOfField();
                                            CurrentState = ParseState.EndOfRecord;
                                            break;

                                        case CharType.TEXTDATA:
                                            FieldBuf.Write(new ReadOnlySpan<char>(s, 1));
                                            break;

                                        default:
                                            throw new FormatException("An unexpected character was encountered.");
                                    }
                                }
                                break;

                            case ParseState.EndOfEscaped:
                                {
                                    switch (ct)
                                    {
                                        case CharType.EOF:
                                            OnEndOfField();
                                            OnEndOfRecord();
                                            CurrentState = ParseState.EndOfFile;
                                            break;

                                        case CharType.DQUOTE:
                                            FieldBuf.Write(new ReadOnlySpan<char>(Quote));
                                            CurrentState = ParseState.Escaped;
                                            break;

                                        case CharType.COMMA:
                                            OnEndOfField();
                                            CurrentState = ParseState.Init;
                                            break;

                                        case CharType.CR:
                                            OnEndOfField();
                                            CurrentState = ParseState.EndOfRecord;
                                            break;

                                        default:
                                            throw new FormatException("An unexpected character was encountered.");
                                    }
                                }
                                break;

                            case ParseState.EndOfRecord:
                                {
                                    switch (ct)
                                    {
                                        case CharType.LF:
                                            OnEndOfRecord();
                                            CurrentState = ParseState.Init;
                                            break;

                                        case CharType.EOF:
                                            throw new FormatException("An unexpected EOF was encountered.");

                                        default:
                                            throw new FormatException("An unexpected character was encountered.");
                                    }
                                }
                                break;

                            case ParseState.EndOfFile:
                                break;
                        }
                        s++;
                    }
                }
            }
        }

        public void EndOfFile()
        {
            char[] eof = { '\0' };
            Process(eof);
            if (!IsEndOfFile)
            {
                throw new FormatException("Incomplete input sequence");
            }
        }

        public static List<List<string>> ParseString(string s)
        {
            char[] chars = s.ToCharArray();
            RFC4180 p = new RFC4180();
            p.Process(chars);
            p.EndOfFile();
            return p.Records;
        }

        private void OnEndOfField()
        {
            Fields.Add(FieldBuf.WrittenMemory.ToString());
            FieldBuf.Clear();
        }

        private void OnEndOfRecord()
        {
            Records_.Add(Fields);
            Fields = new List<string>();
        }
    }
}
