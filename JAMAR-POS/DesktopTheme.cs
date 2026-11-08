
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JAMAR_POS
{
    public static class DesktopTheme
    {
        public static readonly Color Navy = Color.FromArgb(25, 47, 73);
        public static readonly Color Dark = Color.FromArgb(17, 33, 54);
        public static readonly Color Blue = Color.FromArgb(30, 105, 175);
        public static readonly Color Light = Color.FromArgb(241, 245, 250);
        public static readonly Color White = Color.White;
        public static readonly Color Ink = Color.FromArgb(39, 56, 76);
        public static readonly Color Gray = Color.FromArgb(106, 122, 140);
        public static readonly Color Border = Color.FromArgb(216, 225, 235);

        public static Font Font(float size = 10, bool bold = false)
        {
            return new Font("Segoe UI", size,
                bold ? FontStyle.Bold : FontStyle.Regular);
        }

        public static Label Label(string text, float size = 10,
            bool bold = false)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = Font(size, bold),
                ForeColor = Ink
            };
        }

        public static Button Button(string text, EventHandler click,
            bool primary = true)
        {
            var b = new Button
            {
                Text = text,
                Height = 39,
                Width = 135,
                Margin = new Padding(5),
                Font = Font(9, true),
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? Blue : White,
                ForeColor = primary ? White : Navy,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = primary ? 0 : 1;
            b.FlatAppearance.BorderColor = Border;
            b.Click += click;
            return b;
        }

        public static Panel Card()
        {
            return new Panel
            {
                BackColor = White,
                Padding = new Padding(18),
                Margin = new Padding(7)
            };
        }

        public static void Apply(Form form)
        {
            form.BackColor = Light;
            form.Font = Font();
            Style(form);
        }

        private static void Style(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                var b = c as Button;
                if (b != null && b.Tag as string != "nav")
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 0;
                    b.BackColor = Blue;
                    b.ForeColor = White;
                    b.Font = Font(9, true);
                    b.Cursor = Cursors.Hand;
                }

                var t = c as TextBox;
                if (t != null)
                {
                    t.Font = Font();
                    t.BackColor = White;
                    t.ForeColor = Ink;
                    t.BorderStyle = BorderStyle.FixedSingle;
                }

                var combo = c as ComboBox;
                if (combo != null)
                {
                    combo.Font = Font();
                    combo.BackColor = White;
                }

                var grid = c as DataGridView;
                if (grid != null) StyleGrid(grid);

                if (c.HasChildren) Style(c);
            }
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = White;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = White;
            grid.ColumnHeadersDefaultCellStyle.Font = Font(10, true);
            grid.ColumnHeadersHeight = 43;
            grid.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 35;
            grid.DefaultCellStyle.Font = Font(9);
            grid.DefaultCellStyle.ForeColor = Ink;
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(216, 233, 248);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(247, 249, 252);
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            grid.GridColor = Border;
        }
    }
}
