
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JAMAR_POS
{
    public class LoginForm : Form
    {
        private TextBox username;
        private TextBox password;
        private Label status;

        public LoginForm()
        {
            Text = "JAMAR-POS | Sign In";
            Size = new Size(790, 460);
            MinimumSize = new Size(790, 460);
            MaximumSize = new Size(790, 460);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = DesktopTheme.White;
            Font = DesktopTheme.Font();

            BuildInterface();
        }

        private void BuildInterface()
        {
            var left = new Panel
            {
                Dock = DockStyle.Left,
                Width = 315,
                BackColor = DesktopTheme.Navy
            };

            var brand = new Label
            {
                Text = "JAMAR-POS",
                ForeColor = Color.White,
                Font = DesktopTheme.Font(27, true),
                AutoSize = true,
                Location = new Point(30, 80)
            };

            var caption = new Label
            {
                Text = "BUSINESS MANAGEMENT SYSTEM",
                ForeColor = Color.FromArgb(188, 208, 227),
                Font = DesktopTheme.Font(9),
                AutoSize = true,
                Location = new Point(32, 135)
            };

            var description = new Label
            {
                Text = "Manage products, inventory,\nsales and business operations\nin one desktop workspace.",
                ForeColor = Color.White,
                Font = DesktopTheme.Font(12),
                AutoSize = true,
                Location = new Point(32, 220)
            };

            left.Controls.Add(brand);
            left.Controls.Add(caption);
            left.Controls.Add(description);

            var right = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var heading = DesktopTheme.Label("Welcome Back", 22, true);
            heading.Location = new Point(55, 56);

            var hint = DesktopTheme.Label(
                "Sign in to continue to JAMAR-POS.", 10);
            hint.ForeColor = DesktopTheme.Gray;
            hint.Location = new Point(58, 105);

            var userLabel = DesktopTheme.Label("Username", 10, true);
            userLabel.Location = new Point(58, 157);

            username = new TextBox
            {
                Text = "admin",
                Location = new Point(58, 185),
                Width = 355,
                Font = DesktopTheme.Font(12),
                BorderStyle = BorderStyle.FixedSingle
            };

            var passLabel = DesktopTheme.Label("Password", 10, true);
            passLabel.Location = new Point(58, 237);

            password = new TextBox
            {
                Location = new Point(58, 265),
                Width = 355,
                Font = DesktopTheme.Font(12),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            var login = DesktopTheme.Button(
                "SIGN IN", (a, b) => Login());
            login.Location = new Point(58, 321);
            login.Size = new Size(355, 43);

            status = DesktopTheme.Label("", 9);
            status.ForeColor = Color.Firebrick;
            status.Location = new Point(58, 380);
            status.MaximumSize = new Size(355, 40);

            right.Controls.Add(heading);
            right.Controls.Add(hint);
            right.Controls.Add(userLabel);
            right.Controls.Add(username);
            right.Controls.Add(passLabel);
            right.Controls.Add(password);
            right.Controls.Add(login);
            right.Controls.Add(status);

            Controls.Add(right);
            Controls.Add(left);

            AcceptButton = login;
        }

        private void Login()
        {
            try
            {
                status.Text = "";

                var t = Db.Proc(
                    "dbo.usp_Login",
                    Db.P("@Username", username.Text.Trim()));

                if (t.Rows.Count == 0 ||
                    !Convert.ToBoolean(t.Rows[0]["IsActive"]) ||
                    !Security.Verify(
                        password.Text,
                        t.Rows[0]["Salt"].ToString(),
                        t.Rows[0]["PasswordHash"].ToString()))
                {
                    status.Text = "Invalid username or password.";
                    return;
                }

                var r = t.Rows[0];

                var session = new Session
                {
                    Id = Convert.ToInt32(r["UserId"]),
                    Name = r["FullName"].ToString(),
                    Role = r["RoleName"].ToString()
                };

                Db.Proc(
                    "dbo.usp_User_TouchLogin",
                    Db.P("@UserId", session.Id));

                Hide();

                try
                {
                    using (var main = new MainForm(session))
                        main.ShowDialog();
                }
                finally
                {
                    password.Clear();
                    Show();
                    password.Focus();
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex);
            }
        }
    }
}
