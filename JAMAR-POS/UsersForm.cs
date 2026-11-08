
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JAMAR_POS
{
    public class UsersForm : Form
    {
        private readonly DataGridView grid = UI.Grid();

        private readonly TextBox username = UI.Text();
        private readonly TextBox fullName = UI.Text();
        private readonly TextBox password = UI.Text();

        private readonly ComboBox role = new ComboBox
        {
            Width = 115,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        public UsersForm()
        {
            Text = "JAMAR-POS | User Management";
            Size = new Size(1150, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = DesktopTheme.Light;
            Font = DesktopTheme.Font();

            role.Items.AddRange(new object[]
            {
                "Admin", "Manager", "Cashier"
            });

            role.SelectedIndex = 2;
            password.UseSystemPasswordChar = true;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = DesktopTheme.Navy
            };

            var heading = new Label
            {
                Text = "User Management",
                ForeColor = Color.White,
                Font = DesktopTheme.Font(20, true),
                AutoSize = true,
                Location = new Point(24, 20)
            };

            header.Controls.Add(heading);

            var creation = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 105,
                Padding = new Padding(14, 10, 10, 8),
                WrapContents = true,
                AutoScroll = true,
                BackColor = Color.White
            };

            username.Width = 125;
            fullName.Width = 150;
            password.Width = 130;

            creation.Controls.Add(
                DesktopTheme.Label("Username", 9, true));
            creation.Controls.Add(username);
            creation.Controls.Add(
                DesktopTheme.Label("Full Name", 9, true));
            creation.Controls.Add(fullName);
            creation.Controls.Add(
                DesktopTheme.Label("Password", 9, true));
            creation.Controls.Add(password);
            creation.Controls.Add(role);
            creation.Controls.Add(DesktopTheme.Button(
                "Create User", (a, b) => CreateUser()));

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 62,
                Padding = new Padding(14, 7, 10, 5),
                WrapContents = false,
                AutoScroll = true,
                BackColor = DesktopTheme.Light
            };

            actions.Controls.Add(DesktopTheme.Button(
                "Deactivate",
                (a, b) => SetActive(false)));

            actions.Controls.Add(DesktopTheme.Button(
                "Activate",
                (a, b) => SetActive(true)));

            actions.Controls.Add(DesktopTheme.Button(
                "Refresh",
                (a, b) => LoadData()));

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18)
            };

            DesktopTheme.StyleGrid(grid);
            body.Controls.Add(grid);

            Controls.Add(body);
            Controls.Add(actions);
            Controls.Add(creation);
            Controls.Add(header);

            Load += (a, b) => LoadData();
        }

        private void LoadData()
        {
            try
            {
                grid.DataSource = Db.Query(
                    @"SELECT
                        u.UserId,
                        u.Username,
                        u.FullName,
                        r.RoleName,
                        u.IsActive,
                        u.CreatedAt,
                        u.LastLoginAt
                      FROM dbo.Users u
                      INNER JOIN dbo.Roles r
                        ON r.RoleId = u.RoleId
                      ORDER BY u.UserId");

                if (grid.Columns.Contains("UserId"))
                    grid.Columns["UserId"].Visible = false;
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }

        private void CreateUser()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username.Text) ||
                    string.IsNullOrWhiteSpace(fullName.Text) ||
                    password.Text.Length < 8)
                {
                    throw new Exception(
                        "Enter username, full name and a password " +
                        "of at least 8 characters.");
                }

                string salt = Security.Salt();

                Db.Execute(
                    @"INSERT INTO dbo.Users
                        (Username, PasswordHash, Salt, FullName, RoleId)
                      SELECT @u, @h, @s, @n, RoleId
                      FROM dbo.Roles
                      WHERE RoleName=@r",
                    Db.P("@u", username.Text.Trim()),
                    Db.P("@h", Security.Hash(password.Text, salt)),
                    Db.P("@s", salt),
                    Db.P("@n", fullName.Text.Trim()),
                    Db.P("@r", role.Text));

                password.Clear();
                username.Clear();
                fullName.Clear();

                LoadData();

                MessageBox.Show(
                    "User created successfully.",
                    "JAMAR-POS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }

        private void SetActive(bool isActive)
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Select a user first.");
                return;
            }

            try
            {
                int id = Convert.ToInt32(
                    grid.CurrentRow.Cells["UserId"].Value);

                Db.Execute(
                    @"UPDATE dbo.Users
                      SET IsActive=@active
                      WHERE UserId=@id",
                    Db.P("@active", isActive),
                    Db.P("@id", id));

                LoadData();
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }
    }
}
