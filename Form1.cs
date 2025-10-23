using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using LexerWinFormsCS.Core;
using LexerWinFormsCS.Models;

namespace LexerWinFormsCS
{
    public partial class Form1 : Form
    {
        private TextBox txtCode;
        private DataGridView grid;
        private Button btnOpen, btnTokenize;
        private OpenFileDialog ofd;
        private StatusStrip status;
        private ToolStripStatusLabel statusLabel;

        private string CsvPath => Path.Combine(AppContext.BaseDirectory, "keywords.csv");

        public Form1()
        {
            Text = "Lexical Analyzer (C# / WinForms)";
            Width = 1100;
            Height = 700;
            BackColor = Color.WhiteSmoke;

            //  GRID (sağ taraf) 
            grid = new DataGridView
            {
                Dock = DockStyle.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                GridColor = Color.LightGray,
                BackgroundColor = Color.White,
                Width = 550
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "LEXEME",
                DataPropertyName = "Lexeme",
                Width = 260
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TYPE", DataPropertyName = "Type", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "LINE", DataPropertyName = "Line", Width = 60 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "COL", DataPropertyName = "Col", Width = 60 });
            grid.SelectionChanged += Grid_SelectionChanged;

            Controls.Add(grid);

            //SOL METİN 
            txtCode = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 10),
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            Controls.Add(txtCode);

            // ÜST PANEL 
            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.Gainsboro
            };
            Controls.Add(top);

            btnOpen = new Button { Text = "Aç", Left = 8, Top = 8, Width = 80 };
            btnOpen.Click += BtnOpen_Click;
            top.Controls.Add(btnOpen);

            btnTokenize = new Button { Text = "Tokenize", Left = 96, Top = 8, Width = 100 };
            btnTokenize.Click += BtnTokenize_Click;
            top.Controls.Add(btnTokenize);

            // STATUS BAR
            status = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Hazır");
            status.Items.Add(statusLabel);
            Controls.Add(status);

            ofd = new OpenFileDialog { Filter = "Source files|*.c;*.cpp;*.h;*.hpp;*.txt;*.*" };
        }

        private void BtnOpen_Click(object? sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtCode.Text = File.ReadAllText(ofd.FileName);
                statusLabel.Text = $"Açıldı: {Path.GetFileName(ofd.FileName)}";
            }
        }

        private void BtnTokenize_Click(object? sender, EventArgs e)
        {
            try
            {
                EnsureKeywordsCsv();

                var original = txtCode.Text ?? string.Empty;
                var pp = new Preprocessor();
                var stripped = pp.Preprocess(original);
                var replaced = pp.ApplyDefinesToText(stripped);
                txtCode.Text = replaced;

                var kws = KeywordStore.LoadFromCsv(CsvPath);
                var lexer = new DFALexer(kws, pp.Defines);
                var tokens = lexer.Tokenize(replaced);

                MessageBox.Show($"Token sayısı: {tokens.Count}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                grid.DataSource = null;
                grid.Rows.Clear();
                grid.Refresh();
                grid.DataSource = tokens;

                var groups = tokens.GroupBy(t => t.Type).Select(g => $"{g.Key}:{g.Count()}");
                statusLabel.Text = $"TOKENS: {tokens.Count}  |  " + string.Join("  |  ", groups);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureKeywordsCsv()
        {
            if (File.Exists(CsvPath)) return;

            var devPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\keywords.csv"));
            if (File.Exists(devPath))
            {
                File.Copy(devPath, CsvPath, overwrite: true);
                return;
            }

            using (var dlg = new OpenFileDialog { Title = "keywords.csv seç", Filter = "CSV files|*.csv|All files|*.*" })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(dlg.FileName, CsvPath, overwrite: true);
                    return;
                }
            }

            var sample = string.Join(Environment.NewLine, new[]
            {
                "if","else","while","for","do","switch","case","default","break","continue","return",
                "int","float","double","char","void","long","short","signed","unsigned","const","static",
                "struct","union","enum","typedef","sizeof","extern","register","volatile","auto","goto",
                "include","define","elif","endif","ifdef","ifndef"
            });
            File.WriteAllText(CsvPath, sample);
            MessageBox.Show($"keywords.csv oluşturuldu:\n{CsvPath}", "Bilgi");
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (grid.CurrentRow?.DataBoundItem is not Token t) return;

            var lines = txtCode.Text.Replace("\r\n", "\n").Split('\n');
            if (t.Line <= 0 || t.Line > lines.Length) return;

            int startIdx = 0;
            for (int i = 0; i < t.Line - 1; i++)
                startIdx += lines[i].Length + 1;

            startIdx += (t.Col - 1 >= 0 ? t.Col - 1 : 0);
            int length = t.Lexeme?.Length ?? 0;

            txtCode.Focus();
            txtCode.SelectionStart = Math.Max(0, Math.Min(startIdx, txtCode.TextLength));
            txtCode.SelectionLength = Math.Max(0, Math.Min(length, txtCode.TextLength - txtCode.SelectionStart));
        }

        private void Form1_Load(object? sender, EventArgs e) { }
    }
}
