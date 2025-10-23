namespace LexerWinFormsCS
{
    partial class Form1
    {
       
        
        private System.ComponentModel.IContainer components = null;

        /// Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        /// <param name="disposing">yönetilen kaynaklar dispose edilmeliyse true; aksi halde false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        
        /// Tasarımcı desteği için gerekli metot — 
        /// kod düzenleyici üzerinden değiştirmeyin.
    
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(914, 600);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "Lexical Analyzer (C# / WinForms)";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
