using System.Collections.Generic;
using LexerWinFormsCS.Models;

namespace LexerWinFormsCS.Core
{
    public class DFALexer
    {
        private readonly HashSet<string> _keywords;
        private readonly Dictionary<string, string> _defines;

        public DFALexer(HashSet<string> keywords, Dictionary<string, string> defines)
        {
            _keywords = keywords ?? new HashSet<string>();
            _defines = defines ?? new Dictionary<string, string>();
        }

        public List<Token> Tokenize(string code)
        {
            var tokens = new List<Token>();

            int i = 0, n = code.Length;
            int line = 1, col = 1;

            char Peek(int k = 0) => (i + k < n) ? code[i + k] : '\0';

            char Advance()
            {
                var ch = code[i++];
                if (ch == '\n') { line++; col = 1; }
                else col++;
                return ch;
            }

            void Emit(string type, string lex, int l, int c) =>
                tokens.Add(new Token(type, lex, l, c));

            bool IsDelim(char c) => "(){}[],;".IndexOf(c) >= 0;
            bool IsOpChar(char c) => "+-*/%!=<>&|.".IndexOf(c) >= 0;

            while (i < n)
            {
                var ch = Peek();
                int startLine = line, startCol = col;

                // whitespace
                if (char.IsWhiteSpace(ch)) { Advance(); continue; }

                // delimiter
                if (IsDelim(ch))
                {
                    Emit("DELIM", Advance().ToString(), startLine, startCol);
                    continue;
                }

                // string
                if (ch == '"')
                {
                    var lex = "";
                    lex += Advance(); // "
                    bool escaped = false;
                    while (i < n)
                    {
                        var c = Advance();
                        lex += c;
                        if (escaped) { escaped = false; continue; }
                        if (c == '\\') { escaped = true; continue; }
                        if (c == '"') break;
                    }
                    Emit("STRING", lex, startLine, startCol);
                    continue;
                }

                // char literal
                if (ch == '\'')
                {
                    var lex = "";
                    lex += Advance(); // '
                    bool escaped = false;
                    while (i < n)
                    {
                        var c = Advance();
                        lex += c;
                        if (escaped) { escaped = false; continue; }
                        if (c == '\\') { escaped = true; continue; }
                        if (c == '\'') break;
                    }
                    Emit("CHAR", lex, startLine, startCol);
                    continue;
                }

                // two-char ops
                var two = (i + 1 < n) ? $"{ch}{Peek(1)}" : "";
                if (two is "==" or "!=" or "<=" or ">=" or "&&" or "||" or "++" or "--")
                {
                    Emit("OP", two, startLine, startCol);
                    Advance(); Advance();
                    continue;
                }

                // one-char ops ('.' sayı başlangıcı olabilir)
                if (IsOpChar(ch))
                {
                    if (ch == '.' && char.IsDigit(Peek(1)))
                    {
                        // sayı koluna düş
                    }
                    else
                    {
                        Emit("OP", Advance().ToString(), startLine, startCol);
                        continue;
                    }
                }

                // number (int/float)
                if (char.IsDigit(ch) || (ch == '.' && char.IsDigit(Peek(1))))
                {
                    bool dot = (ch == '.');
                    var lex = "";
                    while (i < n)
                    {
                        var p = Peek();
                        if (char.IsDigit(p)) lex += Advance();
                        else if (p == '.' && !dot) { dot = true; lex += Advance(); }
                        else break;
                    }
                    if (lex.Length == 0 && ch == '.')
                    {
                        lex += Advance();
                        Emit("DELIM", lex, startLine, startCol);
                    }
                    else
                    {
                        Emit(dot ? "FLOAT" : "INT", lex, startLine, startCol);
                    }
                    continue;
                }

                // identifier / keyword
                if (char.IsLetter(ch) || ch == '_')
                {
                    var lex = "";
                    while (i < n)
                    {
                        var p = Peek();
                        if (char.IsLetterOrDigit(p) || p == '_') lex += Advance();
                        else break;
                    }

                    
                    var type = _keywords.Contains(lex) ? "KEYWORD" : "IDENT";
                    Emit(type, lex, startLine, startCol);
                    continue;
                }

                // preproc (önişlemden kaçan)
                if (ch == '#')
                {
                    var lex = "";
                    while (i < n && Peek() != '\n') lex += Advance();
                    Emit("PREPROC", lex, startLine, startCol);
                    continue;
                }

                // tek nokta
                if (ch == '.')
                {
                    Emit("DELIM", Advance().ToString(), startLine, startCol);
                    continue;
                }

                // bilinmeyen
                Emit("ERROR", Advance().ToString(), startLine, startCol);
            }

            return tokens;
        }
    }
}
