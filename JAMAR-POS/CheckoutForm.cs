
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Forms;

namespace JAMAR_POS
{
    public class CheckoutForm : Form
    {
        private readonly Session session;
        private readonly DataGridView products = UI.Grid();
        private readonly DataGridView cart = UI.Grid();
        private readonly DataTable cartTable = new DataTable();

        private readonly TextBox search = UI.Text();
        private readonly TextBox discount = UI.Text("0");
        private readonly TextBox tax = UI.Text("0");
        private readonly TextBox paid = UI.Text("0");

        private readonly NumericUpDown quantity = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 10000,
            Value = 1,
            Width = 75
        };

        private readonly ComboBox method = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 100
        };

        private readonly Label total = DesktopTheme.Label("Total: 0.00", 17, true);

        public CheckoutForm(Session s)
        {
            session = s;
            Text = "JAMAR-POS | Point of Sale";
            Size = new Size(1250, 780);
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = DesktopTheme.Light;
            Font = DesktopTheme.Font();

            cartTable.Columns.Add("ProductId", typeof(int));
            cartTable.Columns.Add("Product", typeof(string));
            cartTable.Columns.Add("Price", typeof(decimal));
            cartTable.Columns.Add("Quantity", typeof(int));
            cartTable.Columns.Add("LineTotal", typeof(decimal));

            cart.DataSource = cartTable;
            cart.ReadOnly = true;
            cart.CellEndEdit += (a, b) => Recalculate();

            BuildInterface();

            Load += (a, b) => LoadProducts();

            search.KeyDown += (a, b) =>
            {
                if (b.KeyCode == Keys.Enter)
                {
                    LoadProducts();
                    b.SuppressKeyPress = true;
                }
            };
        }

        private void BuildInterface()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = DesktopTheme.Navy
            };

            var title = new Label
            {
                Text = "Point of Sale",
                Font = DesktopTheme.Font(20, true),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(24, 18)
            };

            header.Controls.Add(title);

            var searchBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 65,
                Padding = new Padding(15, 9, 10, 5),
                BackColor = Color.White,
                WrapContents = false,
                AutoScroll = true
            };

            search.Width = 245;

            searchBar.Controls.Add(DesktopTheme.Label("Find Product", 10, true));
            searchBar.Controls.Add(search);
            searchBar.Controls.Add(DesktopTheme.Button(
                "Search", (a, b) => LoadProducts()));
            searchBar.Controls.Add(DesktopTheme.Label("Quantity", 10, true));
            searchBar.Controls.Add(quantity);
            searchBar.Controls.Add(DesktopTheme.Button(
                "Add to Cart", (a, b) => AddToCart()));

            var payment = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                Padding = new Padding(12, 10, 10, 10),
                BackColor = Color.White,
                AutoScroll = true,
                WrapContents = true
            };

            method.Items.AddRange(new object[]
            {
                "Cash", "Card", "Mobile"
            });
            method.SelectedIndex = 0;

            discount.Width = 65;
            tax.Width = 55;
            paid.Width = 95;

            payment.Controls.Add(DesktopTheme.Label("Discount", 9, true));
            payment.Controls.Add(discount);
            payment.Controls.Add(DesktopTheme.Label("Tax %", 9, true));
            payment.Controls.Add(tax);
            payment.Controls.Add(DesktopTheme.Label("Payment", 9, true));
            payment.Controls.Add(method);
            payment.Controls.Add(DesktopTheme.Label("Amount Paid", 9, true));
            payment.Controls.Add(paid);

            payment.Controls.Add(DesktopTheme.Button(
                "Remove Item", (a, b) =>
                {
                    if (cart.CurrentRow == null) return;
                    cartTable.Rows.RemoveAt(cart.CurrentRow.Index);
                    Recalculate();
                }));

            payment.Controls.Add(DesktopTheme.Button(
                "COMPLETE SALE", (a, b) => Checkout()));

            total.ForeColor = DesktopTheme.Navy;
            total.Margin = new Padding(15, 9, 5, 5);
            payment.Controls.Add(total);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 255,
                BackColor = DesktopTheme.Light
            };

            DesktopTheme.StyleGrid(products);
            DesktopTheme.StyleGrid(cart);

            var productPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            var cartPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            productPanel.Controls.Add(products);
            cartPanel.Controls.Add(cart);
            split.Panel1.Controls.Add(productPanel);
            split.Panel2.Controls.Add(cartPanel);

            Controls.Add(split);
            Controls.Add(payment);
            Controls.Add(searchBar);
            Controls.Add(header);
        }

        private void LoadProducts()
        {
            try
            {
                products.DataSource = Db.Query(
                    @"SELECT ProductId, Sku, Barcode, Name,
                             Price, StockQty
                      FROM dbo.Products
                      WHERE IsActive = 1
                        AND (Sku LIKE @q
                             OR Barcode LIKE @q
                             OR Name LIKE @q)
                      ORDER BY Name",
                    Db.P("@q", "%" + search.Text.Trim() + "%"));
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }

        private void AddToCart()
        {
            if (products.CurrentRow == null) return;

            try
            {
                var r = products.CurrentRow;

                int id = Convert.ToInt32(
                    r.Cells["ProductId"].Value);
                int qty = (int)quantity.Value;
                int stock = Convert.ToInt32(
                    r.Cells["StockQty"].Value);

                var existing = cartTable.AsEnumerable()
                    .FirstOrDefault(x =>
                        x.Field<int>("ProductId") == id);

                int old = existing == null
                    ? 0 : existing.Field<int>("Quantity");

                if (old + qty > stock)
                    throw new Exception("Not enough stock available.");

                decimal price = Convert.ToDecimal(
                    r.Cells["Price"].Value);

                if (existing != null)
                {
                    existing["Quantity"] = old + qty;
                    existing["LineTotal"] = (old + qty) * price;
                }
                else
                {
                    cartTable.Rows.Add(
                        id, r.Cells["Name"].Value,
                        price, qty, price * qty);
                }

                Recalculate();
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }

        private decimal Subtotal()
        {
            return cartTable.AsEnumerable().Sum(x =>
                x.Field<decimal>("Price") *
                x.Field<int>("Quantity"));
        }

        private void Recalculate()
        {
            try
            {
                foreach (DataRow r in cartTable.Rows)
                {
                    r["LineTotal"] =
                        Convert.ToDecimal(r["Price"]) *
                        Convert.ToInt32(r["Quantity"]);
                }

                decimal sub = Subtotal();
                decimal d = decimal.Parse(discount.Text);
                decimal rate = decimal.Parse(tax.Text);

                decimal finalTotal =
                    sub - d + Math.Round((sub - d) * rate / 100, 2);

                total.Text = "Total: " + finalTotal.ToString("N2");
            }
            catch
            {
                total.Text = "Check discount / tax";
            }
        }

        private void Checkout()
        {
            try
            {
                cart.EndEdit();
                Recalculate();

                if (cartTable.Rows.Count == 0)
                    throw new Exception("Cart is empty.");

                decimal d = decimal.Parse(discount.Text);
                decimal rate = decimal.Parse(tax.Text);
                decimal amount = decimal.Parse(paid.Text);

                if (d < 0 || rate < 0 || rate > 100 ||
                    d > Subtotal())
                    throw new Exception("Invalid discount or tax rate.");

                if (cartTable.AsEnumerable().Any(
                    r => Convert.ToInt32(r["Quantity"]) <= 0))
                    throw new Exception("Quantity must be positive.");

                var xml = new XElement(
                    "Items",
                    cartTable.AsEnumerable().Select(r =>
                        new XElement(
                            "Item",
                            new XAttribute("ProductId", r["ProductId"]),
                            new XAttribute("Quantity", r["Quantity"]))));

                var result = Db.Proc(
                    "dbo.usp_Order_Create",
                    Db.P("@UserId", session.Id),
                    Db.P("@PaymentMethod", method.Text),
                    Db.P("@DiscountAmount", d),
                    Db.P("@TaxRate", rate),
                    Db.P("@AmountPaid", amount),
                    new System.Data.SqlClient.SqlParameter(
                        "@ItemsXml", SqlDbType.Xml)
                    {
                        Value = xml.ToString()
                    });

                int id = Convert.ToInt32(result.Rows[0]["Id"]);

                var order = Db.Query(
                    @"SELECT OrderNumber, Total, ChangeDue
                      FROM dbo.Orders
                      WHERE OrderId=@id",
                    Db.P("@id", id)).Rows[0];

                MessageBox.Show(
                    "Sale completed!\n\nOrder: " +
                    order["OrderNumber"] +
                    "\nTotal: " + order["Total"] +
                    "\nChange: " + order["ChangeDue"],
                    "Transaction Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                cartTable.Clear();
                LoadProducts();
                Recalculate();
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }
    }
}
