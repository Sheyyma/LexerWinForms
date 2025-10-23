namespace LexerWinFormsCS.Models
{
    public class Token
    {
        public string Type { get; set; }
        public string Lexeme { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }

        public Token(string type, string lexeme, int line, int col)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Col = col;
        }
    }
}
