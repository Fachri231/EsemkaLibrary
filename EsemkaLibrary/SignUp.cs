using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace EsemkaLibrary
{
    public partial class SignUp : Form
    {
        private DatabaseHelper _dbHelper = new DatabaseHelper();

        public SignUp()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login loginform = new Login();
            loginform.StartPosition = FormStartPosition.CenterScreen;
            this.Hide();
            loginform.ShowDialog();
        }

        private void cbShow_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = cbShow.Checked ? '\0' : '*';
            txtKonfPass.PasswordChar = cbShow.Checked ? '\0' : '*';
        }

        private void btnSignUp_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confPassword = txtKonfPass.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confPassword))
            {
                MessageBox.Show("Semua Field Harus Di Isi", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confPassword)
            {
                MessageBox.Show("Password Tidak Sesuai!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string queryCek = "SELECT COUNT(*) FROM [users] WHERE username = @username";

                SqlParameter[] paramCek =
                {
                    new SqlParameter("@username", username)
                };

                int count = (int)_dbHelper.executeScalar(queryCek, paramCek);

                if (count > 0)
                {
                    MessageBox.Show("Username Yang Anda Masukan Sudah Di Pakai, Harap Masukan Username Lain!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                string hashedPassword = HashPassword(password);

                string query = "INSERT INTO [users] (username, password) VALUES (@username, @password)";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@username", username),
                    new SqlParameter("@password", hashedPassword)
                };

                int affectedRow = _dbHelper.executeNonQuery(query, parameters);

                if (affectedRow > 0)
                {
                    MessageBox.Show("Login Berhasil!, Silakan Login Dengan Akun Anda", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Login loginform = new Login();
                    loginform.StartPosition = FormStartPosition.CenterScreen;
                    this.Hide();
                    loginform.ShowDialog();
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