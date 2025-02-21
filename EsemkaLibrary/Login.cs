using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace EsemkaLibrary
{
    public partial class Login : Form
    {
        private DatabaseHelper _dbHelper = new DatabaseHelper();

        public Login()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void btnSignUp_Click(object sender, EventArgs e)
        {
            SignUp signUpfrom = new SignUp();
            signUpfrom.StartPosition = FormStartPosition.CenterParent;
            this.Hide();
            signUpfrom.ShowDialog();
        }

        private void cbShow_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = cbShow.Checked ? '\0' : '*';
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username Dan Password Harap Di Isi", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hashedPassword = HashPassword(password);

            try
            {
                string query = "SELECT COUNT(*) FROM [users] WHERE [username] = @username AND [password] = @password";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@username", username),
                    new SqlParameter("@password", hashedPassword)
                };

                int count = (int)_dbHelper.executeScalar(query, parameters);

                if (count > 0)
                {
                    MessageBox.Show("Login Berhasil!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    EsemkaLibrary esemkaLibraryform = new EsemkaLibrary();
                    esemkaLibraryform.StartPosition = FormStartPosition.CenterScreen;
                    this.Hide();
                    esemkaLibraryform.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Username Atau Password Salah", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Terjadi Kesalahan Database : " + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi Kesalahan : " + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string HashPassword(string password)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha512.ComputeHash(bytes);

                StringBuilder hashPassword = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashPassword.Append(b.ToString("x2"));
                }
                return hashPassword.ToString();
            }
        }
    }
}