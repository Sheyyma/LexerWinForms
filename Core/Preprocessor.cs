using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LexerWinFormsCS.Core
{
   
    
    /// 1) Yorumları temizler (// ... ve /* ... */)
    /// 2) #define satırlarını toplayıp Defines sözlüğüne atar
    /// 3) (ÖNEMLİ) ApplyDefinesToText ile metnin içinde NAME → VALUE ikamesini sözcük sınırıyla yapar
   
    public class Preprocessor
    {
        public Dictionary<string, string> Defines { get; } = new();

        public string StripComments(string code)
        {
            // /* ... */ çok satırlı
            code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
            // // ... tek satır
            code = Regex.Replace(code, @"//.*?$", "", RegexOptions.Multiline);
            return code;
        }

        public string CollectDefines(string code)
        {
            var lines = code.Replace("\r\n", "\n").Split('\n');
            var kept = new List<string>();
            var re = new Regex(@"^\s*#\s*define\s+([A-Za-z_]\w*)\s+(.*)$");

            foreach (var ln in lines)
            {
                var m = re.Match(ln);
                if (m.Success)
                {
                    var name = m.Groups[1].Value;
                    var val = m.Groups[2].Value.Trim();
                    Defines[name] = val;
                }
                else kept.Add(ln);
            }
            return string.Join("\n", kept);
        }

        
        /// İkameleri doğrudan METİN ÜZERİNDE yapar (ödevin "değerler program içine yazılsın" şartı).
        /// Uzun isimleri önce değiştirir (YANLIŞ eşleşmeleri azaltmak için).
        /// Word-boundary (\b) ile sözcük sınırına dikkat eder.
    
        public string ApplyDefinesToText(string code)
        {
            if (Defines.Count == 0) return code;

            foreach (var kv in Defines.OrderByDescending(kv => kv.Key.Length))
            {
                var name = Regex.Escape(kv.Key);
                var val = kv.Value;
                code = Regex.Replace(code, $@"\b{name}\b", val);
            }
            return code;
        }

      
        /// Toplam pipeline: yorumları sil, #define'ları topla, (ikameyi şimdilik yapma).
        /// İkameyi çağıran tarafta ApplyDefinesToText ile yapacağız ki metin editörde birebir görünsün.
        
        public string Preprocess(string code)
        {
            code = StripComments(code);
            code = CollectDefines(code);
            return code;
        }
    }
}
