using System.Data;
using MySql.Data.MySqlClient;

namespace MaintenanceMesin
{
    public partial class Form1 : Form
    {
        private DataGridView dgvMesin;
        private DbConnection dbConn;

        public Form1()
        {
            InitializeComponent();
            dbConn = new DbConnection();
            SetupUI();
            LoadDataMesin();
        }

        // Fungsi untuk mengatur tampilan (UI)
        private void SetupUI()
        {
            Text = "Sistem Monitoring Maintenance Mesin"; // Judul Jendela
            Size = new Size(1000, 600); // Ukuran Jendela
            StartPosition = FormStartPosition.CenterScreen;

            // Membuat judul di atas tabel
            Label lblJudul = new Label();
            lblJudul.Text = "DATA MESIN";
            lblJudul.Font = new Font("Arial", 14, FontStyle.Bold);
            lblJudul.Dock = DockStyle.Top;
            lblJudul.Height = 40;
            lblJudul.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblJudul);

            dgvMesin = new DataGridView();
            dgvMesin.Dock = DockStyle.Fill; // Mengisi sisa ruang di bawah judul
            dgvMesin.AllowUserToAddRows = false; // Menghilangkan baris kosong di bawah
            dgvMesin.ReadOnly = true; // Tabel hanya bisa dibaca
            dgvMesin.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Pilih 1 baris penuh
            dgvMesin.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Kolom menyesuaikan lebar

            Controls.Add(dgvMesin);
            dgvMesin.BringToFront(); // Memastikan tabel tidak tertutup label
        }

        // Fungsi untuk mengambil data dari MySQL dan memasukkannya ke tabel
        private void LoadDataMesin()
        {
            using (MySqlConnection conn = dbConn.GetConnection())
            {
                try
                {
                    conn.Open();
                   
                    string query = @"
                        SELECT 
                            @row_number:=@row_number+1 AS 'No', 
                            kode_mesin AS 'Kode Mesin', 
                            nama_mesin AS 'Nama Mesin', 
                            flavor AS 'Flavor', 
                            operator AS 'Operator', 
                            CONCAT(FORMAT(running_hour, 0), ' jam') AS 'Running Hour', 
                            status AS 'Status',
                            '-' AS 'Terakhir Maintenance'
                        FROM tbl_mesin, (SELECT @row_number:=0) AS t";
                    
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    
                    dgvMesin.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat memuat data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}