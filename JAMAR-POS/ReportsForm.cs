
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JAMAR_POS
{
    public class ReportsForm : Form
    {
        private readonly DateTimePicker from = new DateTimePicker
        {
            Width = 145
        };

        private readonly DateTimePicker to = new DateTimePicker
        {
            Width = 145
        };

        private readonly DataGridView grid = UI.Grid();

        public ReportsForm()
        {
            Text = "JAMAR-POS | Sales Reports";
            Size = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = DesktopTheme.Light;
            Font = DesktopTheme.Font();

            from.Value = DateTime.Today.AddDays(-30);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = DesktopTheme.Navy
            };

            var heading = new Label
            {
                Text = "Sales & Inventory Reports",
                Font = DesktopTheme.Font(20, true),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(24, 20)
            };

            header.Controls.Add(heading);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 105,
                Padding = new Padding(15, 12, 10, 8),
                WrapContents = true,
                AutoScroll = true,
                BackColor = Color.White
            };

            toolbar.Controls.Add(DesktopTheme.Label("From", 10, true));
            toolbar.Controls.Add(from);
            toolbar.Controls.Add(DesktopTheme.Label("To", 10, true));
            toolbar.Controls.Add(to);

            toolbar.Controls.Add(DesktopTheme.Button(
                "Daily Sales",
                (a, b) => Run("dbo.usp_Report_DailySales")));

            toolbar.Controls.Add(DesktopTheme.Button(
                "Top Products",
                (a, b) => Run("dbo.usp_Report_TopProducts")));

            toolbar.Controls.Add(DesktopTheme.Button(
                "By Category",
                (a, b) => Run("dbo.usp_Report_SalesByCategory")));

            toolbar.Controls.Add(DesktopTheme.Button(
                "Low Stock",
                (a, b) => ShowLowStock()));

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18)
            };

            DesktopTheme.StyleGrid(grid);
            body.Controls.Add(grid);

            Controls.Add(body);
            Controls.Add(toolbar);
            Controls.Add(header);

            Load += (a, b) => Run("dbo.usp_Report_DailySales");
        }

        private void Run(string procedure)
        {
            try
            {
                if (from.Value.Date > to.Value.Date)
                    throw new Exception(
                        "From date must not be after To date.");

                grid.DataSource = Db.Proc(
                    procedure,
                    Db.P("@From", from.Value.Date),
                    Db.P("@To", to.Value.Date));
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }

        private void ShowLowStock()
        {
            try
            {
                grid.DataSource = Db.Proc(
                    "dbo.usp_Report_LowStock");
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }
    }
}
