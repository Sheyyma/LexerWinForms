using System.Collections.Generic;
using System.IO;

namespace LexerWinFormsCS.Core
{
    public static class KeywordStore
    {
        public static HashSet<string> LoadFromCsv(string path)
        {
            var set = new HashSet<string>();
            foreach (var line in File.ReadAllLines(path))
            {
                var w = line.Trim();
                if (!string.IsNullOrWhiteSpace(w))
                    set.Add(w);
            }
            return set;
        }
    }
}
