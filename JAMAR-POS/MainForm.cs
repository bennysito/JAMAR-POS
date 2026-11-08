
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JAMAR_POS
{
    public class MainForm : Form
    {
        private readonly Session session;

        private Label sales;
        private Label orders;
        private Label lowStock;
        private Label products;
        private Label updated;

        private bool IsAdmin
        {
            get
            {
                return string.Equals(
                    session.Role, "Admin",
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool IsManager
        {
            get
            {
                return string.Equals(
                    session.Role, "Manager",
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool CanManageInventory
        {
            get { return IsAdmin || IsManager; }
        }

        private bool CanViewReports
        {
            get { return IsAdmin || IsManager; }
        }

        public MainForm(Session s)
        {
            session = s;

            Text = "JAMAR-POS | Business Management";
            Size = new Size(1250, 760);
            MinimumSize = new Size(1020, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = DesktopTheme.Light;
            Font = DesktopTheme.Font();

            BuildInterface();

            Load += (sender, e) => RefreshDashboard();
        }

        private void BuildInterface()
        {
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = DesktopTheme.Dark
            };

            Panel brand = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = DesktopTheme.Navy
            };

            Label title = new Label
            {
                Text = "JAMAR-POS",
                ForeColor = Color.White,
                Font = DesktopTheme.Font(21, true),
                AutoSize = true,
                Location = new Point(19, 20)
            };

            Label subtitle = new Label
            {
                Text = "BUSINESS MANAGEMENT",
                ForeColor = Color.FromArgb(185, 205, 225),
                Font = DesktopTheme.Font(8),
                AutoSize = true,
                Location = new Point(21, 61)
            };

            brand.Controls.Add(title);
            brand.Controls.Add(subtitle);

            FlowLayoutPanel menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 18, 5, 10)
            };

            AddNav(
                menu,
                "Dashboard",
                (sender, e) => RefreshDashboard(),
                true);

            AddNav(
                menu,
                "Products / Inventory",
                (sender, e) => OpenInventory());

            AddNav(
                menu,
                "New Sale",
                (sender, e) => OpenSale());

            if (CanViewReports)
            {
                AddNav(
                    menu,
                    "Sales Reports",
                    (sender, e) => OpenReports());
            }

            if (IsAdmin)
            {
                AddNav(
                    menu,
                    "User Management",
                    (sender, e) => OpenUsers());
            }

            AddNav(
                menu,
                "Refresh Dashboard",
                (sender, e) => RefreshDashboard());

            AddNav(
                menu,
                "Logout",
                (sender, e) => Close());

            sidebar.Controls.Add(menu);
            sidebar.Controls.Add(brand);

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = Color.White
            };

            Label heading = DesktopTheme.Label(
                "Business Overview", 20, true);

            heading.Location = new Point(30, 12);

            Label greeting = DesktopTheme.Label(
                "Welcome back, " + session.Name +
                "  |  " + session.Role, 9);

            greeting.ForeColor = DesktopTheme.Gray;
            greeting.Location = new Point(32, 52);

            header.Controls.Add(heading);
            header.Controls.Add(greeting);

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesktopTheme.Light,
                AutoScroll = true,
                Padding = new Padding(18)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 600,
                ColumnCount = 1,
                RowCount = 4
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 46));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 160));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 320));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 48));

            Label section = DesktopTheme.Label(
                "TODAY'S BUSINESS SNAPSHOT", 11, true);

            section.Dock = DockStyle.Fill;
            section.TextAlign = ContentAlignment.MiddleLeft;

            layout.Controls.Add(section, 0, 0);

            TableLayoutPanel metrics = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };

            for (int i = 0; i < 4; i++)
            {
                metrics.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 25));
            }

            sales = AddMetric(
                metrics, 0, "TODAY'S SALES", "0.00");

            orders = AddMetric(
                metrics, 1, "ORDERS TODAY", "0");

            lowStock = AddMetric(
                metrics, 2, "LOW STOCK", "0");

            products = AddMetric(
                metrics, 3, "ACTIVE PRODUCTS", "0");

            layout.Controls.Add(metrics, 0, 1);

            Panel workflow = DesktopTheme.Card();
            workflow.Dock = DockStyle.Fill;

            Label workflowTitle = DesktopTheme.Label(
                "Business Workflow", 15, true);

            workflowTitle.Location = new Point(23, 22);

            Label workflowHint = DesktopTheme.Label(
                "Manage your daily operations from one workspace.", 9);

            workflowHint.ForeColor = DesktopTheme.Gray;
            workflowHint.Location = new Point(24, 57);

            workflow.Controls.Add(workflowTitle);
            workflow.Controls.Add(workflowHint);

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 205,
                Padding = new Padding(12),
                AutoScroll = true,
                WrapContents = true
            };

            if (CanManageInventory)
            {
                AddAction(
                    actions,
                    "Manage Products",
                    OpenInventory);
            }
            else
            {
                AddAction(
                    actions,
                    "View Inventory",
                    OpenInventory);
            }

            AddAction(
                actions,
                "Process Sale",
                OpenSale);

            AddAction(
                actions,
                "Inventory Review",
                OpenInventory);

            if (CanViewReports)
            {
                AddAction(
                    actions,
                    "Sales Reports",
                    OpenReports);
            }

            if (IsAdmin)
            {
                AddAction(
                    actions,
                    "User Accounts",
                    OpenUsers);
            }

            AddAction(
                actions,
                "Refresh Data",
                RefreshDashboard);

            workflow.Controls.Add(actions);
            layout.Controls.Add(workflow, 0, 2);

            updated = DesktopTheme.Label("Ready", 9);
            updated.ForeColor = DesktopTheme.Gray;
            updated.Dock = DockStyle.Fill;
            updated.TextAlign = ContentAlignment.MiddleRight;

            layout.Controls.Add(updated, 0, 3);

            content.Controls.Add(layout);

            Controls.Add(content);
            Controls.Add(header);
            Controls.Add(sidebar);
        }

        private void AddNav(
            FlowLayoutPanel menu,
            string text,
            EventHandler click,
            bool selected = false)
        {
            Button button = new Button
            {
                Tag = "nav",
                Text = "    " + text,
                Width = 200,
                Height = 47,
                Margin = new Padding(2, 3, 2, 3),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                BackColor = selected
                    ? DesktopTheme.Blue
                    : DesktopTheme.Dark,
                ForeColor = Color.White,
                Font = DesktopTheme.Font(10),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor =
                DesktopTheme.Blue;

            button.Click += click;
            menu.Controls.Add(button);
        }

        private Label AddMetric(
            TableLayoutPanel table,
            int column,
            string caption,
            string initialValue)
        {
            Panel card = DesktopTheme.Card();
            card.Dock = DockStyle.Fill;

            Label label = DesktopTheme.Label(
                caption, 9, true);

            label.ForeColor = DesktopTheme.Gray;
            label.Location = new Point(18, 22);

            Label value = DesktopTheme.Label(
                initialValue, 25, true);

            value.ForeColor = DesktopTheme.Navy;
            value.Location = new Point(18, 57);

            Panel accent = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 4,
                BackColor = DesktopTheme.Blue
            };

            card.Controls.Add(label);
            card.Controls.Add(value);
            card.Controls.Add(accent);

            table.Controls.Add(card, column, 0);

            return value;
        }

        private void AddAction(
            FlowLayoutPanel panel,
            string caption,
            Action action)
        {
            Button button = DesktopTheme.Button(
                caption,
                (sender, e) => action(),
                false);

            button.Size = new Size(190, 68);
            button.Margin = new Padding(7);
            button.BackColor = Color.FromArgb(234, 243, 251);
            button.ForeColor = DesktopTheme.Navy;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor =
                DesktopTheme.Border;

            panel.Controls.Add(button);
        }

        private void OpenInventory()
        {
            using (ProductsForm form = new ProductsForm(session))
            {
                form.ShowDialog(this);
            }

            RefreshDashboard();
        }

        private void OpenSale()
        {
            using (CheckoutForm form = new CheckoutForm(session))
            {
                form.ShowDialog(this);
            }

            RefreshDashboard();
        }

        private void OpenReports()
        {
            if (!CanViewReports)
            {
                MessageBox.Show(
                    "Only Admin and Manager can access reports.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (ReportsForm form = new ReportsForm())
            {
                form.ShowDialog(this);
            }
        }

        private void OpenUsers()
        {
            if (!IsAdmin)
            {
                MessageBox.Show(
                    "Only the Administrator can manage users.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (UsersForm form = new UsersForm())
            {
                form.ShowDialog(this);
            }
        }

        private void RefreshDashboard()
        {
            try
            {
                DataTable table = Db.Proc(
                    "dbo.usp_Report_Dashboard");

                if (table.Rows.Count == 0)
                    return;

                DataRow row = table.Rows[0];

                sales.Text = Convert.ToDecimal(
                    row["TodaySales"]).ToString("N2");

                orders.Text =
                    row["TodayOrders"].ToString();

                lowStock.Text =
                    row["LowStockCount"].ToString();

                products.Text =
                    row["ActiveProducts"].ToString();

                updated.Text = "Last updated: " +
                    DateTime.Now.ToString(
                        "MMM dd, yyyy  hh:mm tt");
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }
    }
}
