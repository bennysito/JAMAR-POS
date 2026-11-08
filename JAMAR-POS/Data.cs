
using System.Windows.Forms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace JAMAR_POS
{
    public sealed class PosDataContext : DataContext
    {
        public PosDataContext()
            : base(ConfigurationManager.ConnectionStrings["arnoco"].ConnectionString)
        {
        }
    }

    public static class Db
    {
        public static string Conn =>
            ConfigurationManager.ConnectionStrings["arnoco"].ConnectionString;

        public static DataTable Query(string sql, params SqlParameter[] args)
        {
            using (var c = new SqlConnection(Conn))
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddRange(args);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var t = new DataTable();
                    da.Fill(t);
                    return t;
                }
            }
        }

        public static DataTable Proc(string name, params SqlParameter[] args)
        {
            using (var c = new SqlConnection(Conn))
            using (var cmd = new SqlCommand(name, c))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(args);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var t = new DataTable();
                    da.Fill(t);
                    return t;
                }
            }
        }

        public static int Execute(string sql, params SqlParameter[] args)
        {
            using (var c = new SqlConnection(Conn))
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddRange(args);
                c.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static SqlParameter P(string n, object v) =>
            new SqlParameter(n, v ?? DBNull.Value);
    }

    public class Session
    {
        public int Id;
        public string Name;
        public string Role;
    }

    public static class Security
    {
        public static bool Verify(string password, string salt, string expected)
        {
            using (var sha = SHA256.Create())
            {
                var b = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(salt + password)
                );

                return string.Equals(
                    BitConverter.ToString(b).Replace("-", ""),
                    expected,
                    StringComparison.OrdinalIgnoreCase
                );
            }
        }

        public static string Salt()
        {
            var b = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(b);

            return BitConverter.ToString(b).Replace("-", "");
        }

        public static string Hash(string pass, string salt)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(
                    sha.ComputeHash(Encoding.UTF8.GetBytes(salt + pass))
                ).Replace("-", "");
        }
    }

    public static class UI
    {
        public static Button Btn(string label, EventHandler click)
        {
            var b = new Button
            {
                Text = label,
                AutoSize = true,
                Height = 34,
                Margin = new Padding(5)
            };

            b.Click += click;
            return b;
        }

        public static TextBox Text(string value = "")
        {
            return new TextBox
            {
                Text = value,
                Width = 180,
                Margin = new Padding(5)
            };
        }

        public static Label Label(string s)
        {
            return new Label
            {
                Text = s,
                AutoSize = true,
                Margin = new Padding(6)
            };
        }

        public static DataGridView Grid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
        }

        public static void Error(Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "JAMAR-POS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        public static FlowLayoutPanel Bar()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 53,
                AutoScroll = true,
                WrapContents = false
            };
        }
    }
}
