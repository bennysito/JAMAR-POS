
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JAMAR_POS
{
    public class ProductsForm : Form
    {
        private readonly Session session;
        private readonly DataGridView grid = UI.Grid();
        private readonly TextBox search = UI.Text();

        private bool CanEdit
        {
            get
            {
                return string.Equals(
                    session.Role, "Admin",
                    StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                    session.Role, "Manager",
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        public ProductsForm(Session s)
        {
            session = s;

            Text = "JAMAR-POS | Products & Inventory";
            Size = new Size(1200, 720);
            MinimumSize = new Size(950, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = DesktopTheme.Light;
            Font = DesktopTheme.Font();

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = DesktopTheme.Navy
            };

            Label title = new Label
            {
                Text = "Products & Inventory",
                ForeColor = Color.White,
                Font = DesktopTheme.Font(20, true),
                AutoSize = true,
                Location = new Point(25, 16)
            };

            Label hint = new Label
            {
                Text = CanEdit
                    ? "Manage products, pricing and available stock"
                    : "View product information and available stock",
                ForeColor = Color.FromArgb(197, 214, 231),
                Font = DesktopTheme.Font(9),
                AutoSize = true,
                Location = new Point(27, 55)
            };

            header.Controls.Add(title);
            header.Controls.Add(hint);

            FlowLayoutPanel toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.White,
                Padding = new Padding(13, 10, 10, 8),
                AutoScroll = true,
                WrapContents = false
            };

            search.Width = 205;
            search.Height = 32;

            toolbar.Controls.Add(
                DesktopTheme.Label("Search", 10, true));

            toolbar.Controls.Add(search);

            toolbar.Controls.Add(
                DesktopTheme.Button(
                    "Find",
                    (sender, e) => LoadData()));

            if (CanEdit)
            {
                toolbar.Controls.Add(
                    DesktopTheme.Button(
                        "Add Product",
                        (sender, e) => Edit(false)));

                toolbar.Controls.Add(
                    DesktopTheme.Button(
                        "Edit Product",
                        (sender, e) => Edit(true)));

                toolbar.Controls.Add(
                    DesktopTheme.Button(
                        "Deactivate",
                        (sender, e) => DeactivateProduct()));

                toolbar.Controls.Add(
                    DesktopTheme.Button(
                        "Adjust Stock",
                        (sender, e) => Adjust()));
            }

            Panel body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = DesktopTheme.Light
            };

            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;

            DesktopTheme.StyleGrid(grid);

            body.Controls.Add(grid);

            Controls.Add(body);
            Controls.Add(toolbar);
            Controls.Add(header);

            Load += (sender, e) => LoadData();

            search.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    LoadData();
                    e.SuppressKeyPress = true;
                }
            };
        }

        private int SelectedId
        {
            get
            {
                if (grid.CurrentRow == null)
                    return 0;

                return Convert.ToInt32(
                    grid.CurrentRow.Cells["ProductId"].Value);
            }
        }

        private bool CheckPermission()
        {
            if (CanEdit)
                return true;

            MessageBox.Show(
                "Only Admin and Manager can modify inventory.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        private void LoadData()
        {
            try
            {
                grid.DataSource = Db.Query(
                    @"SELECT
                        p.ProductId,
                        p.Sku,
                        p.Barcode,
                        p.Name,
                        c.Name AS Category,
                        p.CategoryId,
                        p.Price,
                        p.Cost,
                        p.StockQty,
                        p.ReorderLevel,
                        p.IsActive
                      FROM dbo.Products p
                      INNER JOIN dbo.Categories c
                          ON c.CategoryId = p.CategoryId
                      WHERE p.Name LIKE @q
                         OR p.Sku LIKE @q
                         OR p.Barcode LIKE @q
                      ORDER BY p.Name",
                    Db.P(
                        "@q",
                        "%" + search.Text.Trim() + "%"));

                if (grid.Columns.Contains("CategoryId"))
                {
                    grid.Columns["CategoryId"].Visible = false;
                }

                if (grid.Columns.Contains("ProductId"))
                {
                    grid.Columns["ProductId"].Visible = false;
                }

                if (!CanEdit)
                {
                    if (grid.Columns.Contains("Cost"))
                        grid.Columns["Cost"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }

        private void Edit(bool existing)
        {
            if (!CheckPermission())
                return;

            if (existing && SelectedId == 0)
            {
                MessageBox.Show("Select a product first.");
                return;
            }

            using (ProductEditorForm form =
                new ProductEditorForm(
                    session,
                    existing ? SelectedId : 0))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void DeactivateProduct()
        {
            if (!CheckPermission())
                return;

            if (SelectedId == 0)
            {
                MessageBox.Show("Select a product first.");
                return;
            }

            if (MessageBox.Show(
                "Deactivate the selected product?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Db.Proc(
                    "dbo.usp_Product_Delete",
                    Db.P("@ProductId", SelectedId));

                LoadData();

                MessageBox.Show(
                    "Product deactivated successfully.",
                    "JAMAR-POS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }

        private void Adjust()
        {
            if (!CheckPermission())
                return;

            if (SelectedId == 0)
            {
                MessageBox.Show("Select a product first.");
                return;
            }

            using (StockForm form =
                new StockForm(session, SelectedId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }
    }

    public class ProductEditorForm : Form
    {
        private readonly int id;
        private readonly Session session;

        private readonly TextBox sku = UI.Text();
        private readonly TextBox barcode = UI.Text();
        private readonly TextBox name = UI.Text();
        private readonly TextBox price = UI.Text("0");
        private readonly TextBox cost = UI.Text("0");
        private readonly TextBox stock = UI.Text("0");
        private readonly TextBox reorder = UI.Text("5");

        private readonly ComboBox category = new ComboBox
        {
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        private readonly CheckBox active = new CheckBox
        {
            Text = "Product is active",
            Checked = true,
            AutoSize = true
        };

        private bool CanEdit
        {
            get
            {
                return string.Equals(
                    session.Role, "Admin",
                    StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                    session.Role, "Manager",
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        public ProductEditorForm(Session s, int productId)
        {
            session = s;
            id = productId;

            Text = id == 0
                ? "Add Product"
                : "Edit Product";

            Size = new Size(570, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = DesktopTheme.Light;
            Font = DesktopTheme.Font();

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = DesktopTheme.Navy
            };

            Label heading = new Label
            {
                Text = id == 0
                    ? "New Product"
                    : "Edit Product",
                ForeColor = Color.White,
                Font = DesktopTheme.Font(19, true),
                AutoSize = true,
                Location = new Point(24, 20)
            };

            header.Controls.Add(heading);

            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                ColumnCount = 2,
                RowCount = 10,
                AutoScroll = true
            };

            table.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 180));

            table.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            string[] labels =
            {
                "SKU",
                "Barcode",
                "Product Name",
                "Category",
                "Selling Price",
                "Cost",
                "Initial Stock",
                "Reorder Level",
                "Status"
            };

            Control[] inputs =
            {
                sku,
                barcode,
                name,
                category,
                price,
                cost,
                stock,
                reorder,
                active
            };

            for (int i = 0; i < labels.Length; i++)
            {
                table.RowStyles.Add(
                    new RowStyle(SizeType.Absolute, 50));

                Label label = DesktopTheme.Label(
                    labels[i], 10, true);

                label.Anchor = AnchorStyles.Left;

                inputs[i].Anchor =
                    AnchorStyles.Left | AnchorStyles.Right;

                inputs[i].Margin = new Padding(4, 7, 4, 7);

                table.Controls.Add(label, 0, i);
                table.Controls.Add(inputs[i], 1, i);
            }

            Button save = DesktopTheme.Button(
                "Save Product",
                (sender, e) => SaveProduct());

            save.Width = 180;

            table.Controls.Add(save, 1, 9);

            Controls.Add(table);
            Controls.Add(header);

            Load += (sender, e) =>
            {
                if (!CanEdit)
                {
                    MessageBox.Show(
                        "Access denied. Inventory editing requires " +
                        "Admin or Manager access.",
                        "Access Denied",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    Close();
                    return;
                }

                InitializeData();
            };

            AcceptButton = save;
        }

        private void InitializeData()
        {
            try
            {
                DataTable categories = Db.Query(
                    @"SELECT CategoryId, Name
                      FROM dbo.Categories
                      ORDER BY Name");

                category.DisplayMember = "Name";
                category.ValueMember = "CategoryId";
                category.DataSource = categories;

                if (id == 0)
                    return;

                DataTable data = Db.Query(
                    @"SELECT *
                      FROM dbo.Products
                      WHERE ProductId = @id",
                    Db.P("@id", id));

                if (data.Rows.Count == 0)
                {
                    throw new Exception("Product not found.");
                }

                DataRow row = data.Rows[0];

                sku.Text = row["Sku"].ToString();
                barcode.Text = row["Barcode"].ToString();
                name.Text = row["Name"].ToString();
                category.SelectedValue = row["CategoryId"];
                price.Text = row["Price"].ToString();
                cost.Text = row["Cost"].ToString();
                stock.Text = row["StockQty"].ToString();
                stock.Enabled = false;
                reorder.Text = row["ReorderLevel"].ToString();
                active.Checked =
                    Convert.ToBoolean(row["IsActive"]);
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }

        private void SaveProduct()
        {
            if (!CanEdit)
            {
                MessageBox.Show(
                    "You do not have permission to save products.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(sku.Text) ||
                    string.IsNullOrWhiteSpace(name.Text))
                {
                    throw new Exception(
                        "SKU and product name are required.");
                }

                decimal pr = decimal.Parse(price.Text);
                decimal co = decimal.Parse(cost.Text);
                int qty = int.Parse(stock.Text);
                int re = int.Parse(reorder.Text);

                if (pr < 0 || co < 0 || qty < 0 || re < 0)
                {
                    throw new Exception(
                        "Negative values are not allowed.");
                }

                object barcodeValue =
                    string.IsNullOrWhiteSpace(barcode.Text)
                        ? null
                        : (object)barcode.Text.Trim();

                if (id == 0)
                {
                    Db.Proc(
                        "dbo.usp_Product_Insert",
                        Db.P("@Sku", sku.Text.Trim()),
                        Db.P("@Barcode", barcodeValue),
                        Db.P("@Name", name.Text.Trim()),
                        Db.P("@CategoryId", category.SelectedValue),
                        Db.P("@Price", pr),
                        Db.P("@Cost", co),
                        Db.P("@StockQty", qty),
                        Db.P("@ReorderLevel", re),
                        Db.P("@UserId", session.Id));
                }
                else
                {
                    Db.Proc(
                        "dbo.usp_Product_Update",
                        Db.P("@ProductId", id),
                        Db.P("@Sku", sku.Text.Trim()),
                        Db.P("@Barcode", barcodeValue),
                        Db.P("@Name", name.Text.Trim()),
                        Db.P("@CategoryId", category.SelectedValue),
                        Db.P("@Price", pr),
                        Db.P("@Cost", co),
                        Db.P("@ReorderLevel", re),
                        Db.P("@IsActive", active.Checked));
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }
    }

    public class StockForm : Form
    {
        private readonly Session session;
        private readonly int productId;

        private readonly TextBox quantity = UI.Text("1");
        private readonly TextBox reason =
            UI.Text("Stock adjustment");

        private bool CanEdit
        {
            get
            {
                return string.Equals(
                    session.Role, "Admin",
                    StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                    session.Role, "Manager",
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        public StockForm(Session s, int id)
        {
            session = s;
            productId = id;

            Text = "Adjust Product Stock";
            Size = new Size(460, 330);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = DesktopTheme.Light;
            Font = DesktopTheme.Font();

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = DesktopTheme.Navy
            };

            Label title = new Label
            {
                Text = "Inventory Adjustment",
                ForeColor = Color.White,
                Font = DesktopTheme.Font(17, true),
                AutoSize = true,
                Location = new Point(20, 18)
            };

            header.Controls.Add(title);

            FlowLayoutPanel content = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(25),
                BackColor = DesktopTheme.Light
            };

            quantity.Width = 350;
            reason.Width = 350;

            content.Controls.Add(
                DesktopTheme.Label(
                    "Quantity change (example: 10 or -2)",
                    10,
                    true));

            content.Controls.Add(quantity);

            content.Controls.Add(
                DesktopTheme.Label("Reason", 10, true));

            content.Controls.Add(reason);

            content.Controls.Add(
                DesktopTheme.Button(
                    "Apply Adjustment",
                    (sender, e) => ApplyAdjustment()));

            Controls.Add(content);
            Controls.Add(header);

            Load += (sender, e) =>
            {
                if (!CanEdit)
                {
                    MessageBox.Show(
                        "Only Admin and Manager can adjust stock.",
                        "Access Denied",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    Close();
                }
            };
        }

        private void ApplyAdjustment()
        {
            if (!CanEdit)
            {
                MessageBox.Show(
                    "You do not have permission to adjust stock.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                int change = int.Parse(quantity.Text);

                if (string.IsNullOrWhiteSpace(reason.Text))
                {
                    throw new Exception(
                        "Enter an adjustment reason.");
                }

                Db.Proc(
                    "dbo.usp_Inventory_Adjust",
                    Db.P("@ProductId", productId),
                    Db.P("@ChangeQty", change),
                    Db.P("@Reason", reason.Text.Trim()),
                    Db.P("@UserId", session.Id));

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }
    }
}
