using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace EsemkaLibrary
{
    public partial class EsemkaLibrary : Form
    {
        private DatabaseHelper _dbHelper = new DatabaseHelper();
        private string _memberName;
        public EsemkaLibrary(string memberName = null)
        {
            InitializeComponent();
            loadDateTimeNow();
            loadTimer();

            if (memberName != null)
            {
                this._memberName = memberName;
                loadBorrowingData(_memberName);
                txtSerach.Text = _memberName;
            }

        }

        private void loadTimer()
        {
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (sender, e) => loadDateTimeNow();
            timer.Start();
        }

        private void loadDateTimeNow()
        {
            DateTime dateTimeNow = DateAndTime.Now;
            lbDateTime.Text = dateTimeNow.ToString("dddd, MMMM yyyy HH:mm:ss");
        }

        private void loadBorrowingData(string memberName)
        {
            try
            {
                string queryMemberId = "SELECT [id] FROM [Member] WHERE [name] = @name AND deleted_at IS NULL";
                SqlParameter[] paramName =
                {
                    new SqlParameter("@name", memberName)
                };

                object result = _dbHelper.executeScalar(queryMemberId, paramName);
                int memberId = result != null ? Convert.ToInt32(result) : 0;

                if (memberId == 0)
                {
                    MessageBox.Show("Member Dengan Nama: " + memberName + " Tidak Tersedia", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string queryBookBorrowing = @"
                    SELECT
                        Borrowing.id AS BorrowingId,
                        Book.id AS BookId,
                        Book.title,
                        FORMAT(Borrowing.borrow_date, 'd MMMM yyyy') AS Borrow_Date,
                        FORMAT(DATEADD(DAY, 7, Borrowing.borrow_date), 'd MMMM yyyy') AS Due_Date,
                    CASE
                        WHEN DATEDIFF(DAY, DATEADD(DAY, 7, Borrowing.borrow_date), GETDATE()) > 0
                        THEN DATEDIFF(DAY, DATEADD(DAY, 7, Borrowing.borrow_date), GETDATE())
                        ELSE 0
                    END AS Overdue_Days
                    FROM
                        [Borrowing]
                    INNER JOIN
                        [Book]
                    ON
                        Borrowing.book_id = Book.id
                    WHERE
                        Borrowing.member_id = @memberId
                        AND Borrowing.return_date IS NULL
                        AND Borrowing.deleted_at IS NULL
                        AND Book.deleted_at IS NULL
                    ";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@memberId", memberId)
                };

                DataTable BorrowingTable = _dbHelper.getData(queryBookBorrowing, parameters);
                dgvBorrowingData.DataSource = BorrowingTable;

                dgvBorrowingData.Columns["BorrowingId"].Visible = false;
                dgvBorrowingData.Columns["BookId"].Visible = false;
                dgvBorrowingData.ClearSelection();

                if (!dgvBorrowingData.Columns.Contains("llbReturn"))
                {
                    DataGridViewLinkColumn llbReturn = new DataGridViewLinkColumn
                    {
                        HeaderText = "Action",
                        Text = "Return",
                        Name = "llbReturn",
                        UseColumnTextForLinkValue = true
                    };
                    dgvBorrowingData.Columns.Add(llbReturn);
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

        private void loadCheckButton(string memberName)
        {
            try
            {
                string queryName = "SELECT [id] FROM [Member] WHERE [name] = @memberName AND deleted_at IS NULL";

                SqlParameter[] paramMember =
                {
                    new SqlParameter("@membername", memberName)
                };

                object result = _dbHelper.executeScalar(queryName, paramMember);

                int memberId = result != null ? Convert.ToInt32(result) : 0;

                if (memberId > 0)
                {
                    string query = "SELECT COUNT(*) FROM [Borrowing] WHERE [member_id] = @memberId AND [return_date] IS NULL AND deleted_at IS NULL";

                    SqlParameter[] parameters =
                    {
                    new SqlParameter("@memberId", memberId)
                };

                    int count = (int)_dbHelper.executeScalar(query, parameters);

                    if (count == 3)
                    {
                        btnNewBorrowing.Enabled = false;
                    }
                    else
                    {
                        btnNewBorrowing.Enabled = true;
                    }
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
            string name = txtSerach.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Harap Masukan Nama Member!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                btnSearch.Tag = true;
                loadBorrowingData(name);
                loadCheckButton(name);
            }
        }

        private void dgvBorrowingData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBorrowingData.Columns[e.ColumnIndex].Name == "Overdue_Days")
            {
                object overdueValue = dgvBorrowingData.Rows[e.RowIndex].Cells["Overdue_Days"].Value;

                if (overdueValue != null && int.TryParse(overdueValue.ToString(), out int overdueDays))
                {
                    if (overdueDays > 0)
                    {
                        dgvBorrowingData.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                        dgvBorrowingData.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White;
                        dgvBorrowingData.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.Red;
                        dgvBorrowingData.Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = Color.White;
                    }
                    else if (overdueDays == 0)
                    {
                        dgvBorrowingData.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                        dgvBorrowingData.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                        dgvBorrowingData.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.Yellow;
                        dgvBorrowingData.Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = Color.Black;
                    }
                }
            }
        }

        private void dgvBorrowingData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBorrowingData.Columns[e.ColumnIndex].Name == "llbReturn")
            {
                string memberName = txtSerach.Text.Trim();
                int BorrowId = Convert.ToInt32(dgvBorrowingData.Rows[e.RowIndex].Cells["BorrowingId"].Value);
                int BookId = Convert.ToInt32(dgvBorrowingData.Rows[e.RowIndex].Cells["BookId"].Value);
                string titleBook = dgvBorrowingData.Rows[e.RowIndex].Cells["title"].Value.ToString();

                try
                {
                    string query = @"
                        UPDATE
                            [Borrowing]
                        SET
                            [return_date] = GETDATE(),
                            [fine] =
                        CASE
                            WHEN DATEDIFF(DAY, DATEADD(DAY, 7, borrow_date), GETDATE()) > 0
                            THEN DATEDIFF(DAY, DATEADD(DAY, 7, borrow_date), GETDATE()) * 2000
                            ELSE 0
                        END
                        WHERE
                            [id] = @id
                            AND deleted_at IS NULL
                        ";

                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@id", BorrowId)
                    };

                    int resultUpdate = _dbHelper.executeNonQuery(query, parameters);

                    string insertBookReturnQuery = "UPDATE [Book] SET [stock] = [stock] + 1 WHERE [id] = @id";

                    SqlParameter[] paramBookId =
                    {
                        new SqlParameter("@id", BookId)
                    };

                    int affectedRow = _dbHelper.executeNonQuery(insertBookReturnQuery, paramBookId);

                    if (affectedRow > 0)
                    {
                        string queryFine = "SELECT [fine] FROM [Borrowing] WHERE [id] = @id AND deleted_at IS NULL";

                        SqlParameter[] paramId =
                        {
                            new SqlParameter("@id", BorrowId)
                        };

                        object result = _dbHelper.executeScalar(queryFine, paramId);

                        int fineResult = result != null ? Convert.ToInt32(result) : 0;

                        if (fineResult >= 0)
                        {
                            MessageBox.Show("Success return `" + titleBook + ",` Member needs to pay fine: " + fineResult + "IDR.", "Notification", MessageBoxButtons.OK);
                            loadBorrowingData(memberName);
                            loadCheckButton(memberName);
                        }
                        else
                        {
                            MessageBox.Show("Failed return `" + titleBook + ".` ", "Notification", MessageBoxButtons.OK);
                            loadBorrowingData(memberName);
                            loadCheckButton(memberName);
                        }
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

        private void btnNewBorrowing_Click(object sender, EventArgs e)
        {
            string memberName = txtSerach.Text.Trim();

            if (btnSearch.Tag == null || (bool)btnSearch.Tag == false)
            {
                MessageBox.Show("Harap Mencari Member Terlebih Dahulu Sebelum Meminjam Buku Baru", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query = "SELECT [id] FROM [Member] WHERE [name] = @name AND deleted_at IS NULL";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@name", memberName)
                };

                int id = (int)_dbHelper.executeScalar(query, parameters);

                if (id > 0)
                {
                    NewBorrowing newBorrowingform = new NewBorrowing(id);
                    newBorrowingform.StartPosition = FormStartPosition.CenterScreen;
                    this.Hide();
                    newBorrowingform.ShowDialog();
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

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}