using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace EsemkaLibrary
{
    public partial class NewBorrowing : Form
    {
        private DatabaseHelper _dbHelper = new DatabaseHelper();
        private int memberId;

        public NewBorrowing(int memberId)
        {
            InitializeComponent();
            txtSerach.Focus();
            this.memberId = memberId;
        }

        private void loadBook(string title)
        {
            try
            {
                string query = @"
                SELECT
                    Book.id AS Id,
                    Book.title AS Title,
                    STRING_AGG(Genre.name, ', ') AS Genre,
                    Book.author AS Author,
                    FORMAT(Book.publish_date, 'd MMMM yyyy') AS Publish_Date,
                    Book.stock AS Stock
                FROM
                    [Book]
                INNER JOIN
                    [BookGenre] ON Book.id = BookGenre.book_id
                INNER JOIN
                    [Genre] ON BookGenre.genre_id = Genre.id
                WHERE
                    Book.title LIKE '%' + @titleSearch + '%'
                    AND Book.deleted_at IS NULL
                GROUP BY
                    Book.id, Book.title, Book.author, Book.publish_date, Book.stock
                ORDER BY
                    Book.title
                ";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@titleSearch", title)
                };

                DataTable tableBooks = _dbHelper.getData(query, parameters);
                dgvBooks.DataSource = tableBooks;

                dgvBooks.Columns["Id"].Visible = false;

                if (!dgvBooks.Columns.Contains("llbBorrow"))
                {
                    DataGridViewLinkColumn llbBorrow = new DataGridViewLinkColumn
                    {
                        HeaderText = "Action",
                        Text = "Borrow",
                        Name = "llbBorrow",
                        UseColumnTextForLinkValue = false
                    };
                    dgvBooks.Columns.Add(llbBorrow);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Terjadi Kesalahan Database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi Kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string titleSerach = txtSerach.Text.Trim();

            if (string.IsNullOrEmpty(titleSerach))
            {
                MessageBox.Show("Tolong Masukan Judul!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                loadBook(titleSerach);
            }
        }

        private void dgvBooks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBooks.Columns[e.ColumnIndex].Name == "Stock")
            {
                object stockValue = dgvBooks.Rows[e.RowIndex].Cells["Stock"].Value;
                if (stockValue != null && int.TryParse(stockValue.ToString(), out int stock))
                {
                    if (stock == 0)
                    {
                        dgvBooks.Rows[e.RowIndex].Cells["llbBorrow"].Value = "";
                        dgvBooks.Rows[e.RowIndex].Cells["llbBorrow"].ReadOnly = true;
                        dgvBooks.Rows[e.RowIndex].Cells["llbBorrow"].Style.ForeColor = Color.Transparent;
                        dgvBooks.Rows[e.RowIndex].Cells["llbBorrow"].Style.SelectionForeColor = Color.Transparent;

                        dgvBooks.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                        dgvBooks.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White;
                        dgvBooks.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.Red;
                        dgvBooks.Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = Color.White;
                    }
                    else
                    {
                        dgvBooks.Rows[e.RowIndex].Cells["llbBorrow"].Value = "Borrow";
                        dgvBooks.Rows[e.RowIndex].Cells["llbBorrow"].ReadOnly = false;
                    }
                }
            }
        }

        private void dgvBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBooks.Columns[e.ColumnIndex].Name == "llbBorrow")
            {
                object BookId = dgvBooks.Rows[e.RowIndex].Cells["Id"].Value;
                string title = dgvBooks.Rows[e.RowIndex].Cells["Title"].Value.ToString();

                if (BookId != null && int.TryParse(BookId.ToString(), out int id))
                {
                    try
                    {
                        string query = "UPDATE [Book] SET [stock] -= 1 WHERE id = @bookId AND deleted_at IS NULL";
                        SqlParameter[] parameters =
                        {
                            new SqlParameter("@bookId", id)
                        };

                        int result = _dbHelper.executeNonQuery(query, parameters);

                        string insertBorrowQuery = "INSERT INTO [Borrowing] ([member_id], [book_id], [borrow_date], [created_at]) VALUES (@memberId, @bookId, GETDATE(), GETDATE())";
                        SqlParameter[] paramInserts =
                        {
                            new SqlParameter("@memberId", memberId),
                            new SqlParameter("@bookId", BookId)
                        };

                        int affectedRow = _dbHelper.executeNonQuery(insertBorrowQuery, paramInserts);

                        if (affectedRow > 0)
                        {
                            string memberNameQuery = "SELECT [name] FROM [Member] WHERE id = @memberId";

                            SqlParameter[] paramMemeberId =
                            {
                                new SqlParameter("@memberId", memberId)
                            };

                            string memberName = _dbHelper.executeScalar(memberNameQuery, paramMemeberId).ToString();

                            MessageBox.Show("Success Borrow `" + title + ".` Due date is 7 days from today.", "Notification");
                            EsemkaLibrary esemkaLibraryform = new EsemkaLibrary(memberName);
                            esemkaLibraryform.StartPosition = FormStartPosition.CenterScreen;
                            this.Hide();
                            esemkaLibraryform.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Failed Borrow `" + title + ".` ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Terjadi Kesalahan Database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Terjadi Kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}