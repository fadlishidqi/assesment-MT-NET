using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AssesmentIndofoodNet
{
    public partial class Form1 : Form
    {
        // Deklarasi Komponen UI
        private Label lblTotalMesin, lblPerluMaint, lblRataRh, lblBadgeNotif;
        private Button btnStart, btnStop;
        private Button btnProsesMaint, btnTambah, btnEdit, btnHapus; 
        private DataGridView dgvMesin; 
        private ListBox lstNotifikasi; 

        // Deklarasi Variabel Sistem
        private System.Windows.Forms.Timer timerSimulasi;
        private bool isSimulasiBerjalan = false;
        private DbConnection dbConn;

        public Form1()
        {
            dbConn = new DbConnection();
            dbConn.AutoMigrate(); 

            SetupUI();
            LoadDataMesin();
            SetupTimer(); 
            UpdateStatistik(); 
            UpdateBadgeNotif(); // Hitung badge notifikasi saat aplikasi pertama dibuka
        }

        // ==========================================
        // 1. SETUP TAMPILAN (UI)
        // ==========================================
        private void SetupUI()
        {
            this.Text = "Sistem Monitoring Maintenance Mesin - Indofood";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen; 
            this.BackColor = Color.WhiteSmoke;

            lblTotalMesin = new Label() { Text = "Total Mesin: 0", Location = new Point(20, 20), AutoSize = true, Font = new Font("Arial", 11, FontStyle.Bold) };
            lblPerluMaint = new Label() { Text = "Perlu Maint: 0", Location = new Point(200, 20), AutoSize = true, Font = new Font("Arial", 11, FontStyle.Bold) };
            lblRataRh = new Label() { Text = "Rata2 RH: 0", Location = new Point(380, 20), AutoSize = true, Font = new Font("Arial", 11, FontStyle.Bold) };

            btnStart = new Button() { Text = "Start Simulasi", Location = new Point(20, 60), Size = new Size(120, 35), BackColor = Color.LightGreen };
            btnStop = new Button() { Text = "Stop / Reset", Location = new Point(150, 60), Size = new Size(120, 35), BackColor = Color.LightCoral };
            
            btnProsesMaint = new Button() { Text = "🔧 Proses Maint.", Location = new Point(480, 60), Size = new Size(130, 35), BackColor = Color.Plum, Font = new Font("Arial", 9, FontStyle.Bold) };
            btnTambah = new Button() { Text = "+ Tambah", Location = new Point(650, 60), Size = new Size(90, 35), BackColor = Color.LightBlue };
            btnEdit = new Button() { Text = "Edit", Location = new Point(750, 60), Size = new Size(90, 35), BackColor = Color.LightYellow };
            btnHapus = new Button() { Text = "Hapus", Location = new Point(850, 60), Size = new Size(90, 35), BackColor = Color.LightPink };

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnProsesMaint.Click += BtnProsesMaint_Click;
            btnTambah.Click += BtnTambah_Click;
            btnEdit.Click += BtnEdit_Click;
            btnHapus.Click += BtnHapus_Click;

            dgvMesin = new DataGridView();
            dgvMesin.Location = new Point(20, 110);
            dgvMesin.Size = new Size(930, 250);
            dgvMesin.AllowUserToAddRows = false;
            dgvMesin.ReadOnly = true; 
            dgvMesin.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMesin.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; 

            Label lblNotif = new Label() { Text = "NOTIFIKASI TERBARU:", Location = new Point(20, 380), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            
            // UI BARU: Badge Notifikasi (Indikator Merah)
            lblBadgeNotif = new Label() { 
                Text = "0", 
                Location = new Point(195, 378), 
                AutoSize = true, 
                BackColor = Color.Red, 
                ForeColor = Color.White, 
                Font = new Font("Arial", 9, FontStyle.Bold),
                Padding = new Padding(3)
            };
            lblBadgeNotif.Visible = false; // Sembunyikan jika angka 0

            lstNotifikasi = new ListBox();
            lstNotifikasi.Location = new Point(20, 410);
            lstNotifikasi.Size = new Size(930, 150);
            lstNotifikasi.Font = new Font("Arial", 10);

            this.Controls.Add(lblTotalMesin); this.Controls.Add(lblPerluMaint); this.Controls.Add(lblRataRh);
            this.Controls.Add(btnStart); this.Controls.Add(btnStop); this.Controls.Add(btnProsesMaint);
            this.Controls.Add(btnTambah); this.Controls.Add(btnEdit); this.Controls.Add(btnHapus);
            this.Controls.Add(dgvMesin);
            this.Controls.Add(lblNotif); this.Controls.Add(lblBadgeNotif); this.Controls.Add(lstNotifikasi);
        }

        // ==========================================
        // 2. FUNGSI LOAD DATA & BADGE
        // ==========================================
        private void LoadDataMesin()
        {
            try
            {
                using (MySqlConnection conn = dbConn.GetConnection())
                {
                    string query = "SELECT id, kode_mesin AS 'Kode', nama_mesin AS 'Nama', flavor AS 'Flavor', operator AS 'Operator', running_hour AS 'RH (jam)', status AS 'Status' FROM tbl_mesin";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt); 

                    dgvMesin.DataSource = dt;
                    dgvMesin.Columns["id"].Visible = false; 
                    
                    UpdateWarnaGrid();
                    UpdateStatistik();
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal memuat data: " + ex.Message); }
        }

        // FUNGSI BARU: Mengambil jumlah notifikasi belum dibaca dari database
        private void UpdateBadgeNotif()
        {
            try
            {
                using (MySqlConnection conn = dbConn.GetConnection())
                {
                    string query = "SELECT COUNT(*) FROM tbl_notifikasi WHERE status_baca = 'Belum Dibaca'";
                    int count = Convert.ToInt32(new MySqlCommand(query, conn).ExecuteScalar());
                    
                    lblBadgeNotif.Text = count.ToString();
                    lblBadgeNotif.Visible = count > 0; // Jika ada notif, tampilkan badgenya
                }
            }
            catch { /* Silent Error */ }
        }

        // ==========================================
        // 3. FITUR PROSES MAINTENANCE
        // ==========================================
        private void BtnProsesMaint_Click(object sender, EventArgs e)
        {
            if (dgvMesin.SelectedRows.Count == 0) return;
            DataGridViewRow row = dgvMesin.SelectedRows[0];
            string status = row.Cells["Status"].Value.ToString();

            if (status == "Normal")
            {
                MessageBox.Show("Mesin berstatus Normal.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string kode = row.Cells["Kode"].Value.ToString();
            string nama = row.Cells["Nama"].Value.ToString();
            string op = row.Cells["Operator"].Value.ToString();
            int rh = Convert.ToInt32(row.Cells["RH (jam)"].Value);

            string jenisMaint = status == "[!!] Critical" ? "Maintenance Berat" : (status == "[!] Warning" ? "Maintenance Medium" : "Maintenance Ringan");

            using (FormTindakanMaintenance formMaint = new FormTindakanMaintenance(kode, nama, op, rh, jenisMaint))
            {
                if (formMaint.ShowDialog() == DialogResult.OK)
                {
                    LoadDataMesin(); 
                    UpdateBadgeNotif(); // Kurangi/update badge setelah maintenance diselesaikan
                    lstNotifikasi.Items.Insert(0, $"{DateTime.Now.ToString("HH:mm:ss")} - [Info] {nama} maintenance selesai dilakukan.");
                }
            }
        }

        // ==========================================
        // 4. FUNGSI CRUD
        // ==========================================
        private void BtnTambah_Click(object sender, EventArgs e)
        {
            using (FormInputMesin formInput = new FormInputMesin())
            {
                if (formInput.ShowDialog() == DialogResult.OK)
                {
                    using (MySqlConnection conn = dbConn.GetConnection())
                    {
                        new MySqlCommand($"INSERT INTO tbl_mesin (kode_mesin, nama_mesin, flavor, operator, running_hour) VALUES ('{formInput.KodeMesin}', '{formInput.NamaMesin}', '{formInput.Flavor}', '{formInput.OperatorMesin}', {formInput.RunningHour})", conn).ExecuteNonQuery();
                        LoadDataMesin();
                    }
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvMesin.SelectedRows.Count == 0) return;
            DataGridViewRow row = dgvMesin.SelectedRows[0];
            string kode = row.Cells["Kode"].Value.ToString();

            using (FormInputMesin formInput = new FormInputMesin(kode, row.Cells["Nama"].Value.ToString(), row.Cells["Flavor"].Value.ToString(), row.Cells["Operator"].Value.ToString(), Convert.ToInt32(row.Cells["RH (jam)"].Value)))
            {
                if (formInput.ShowDialog() == DialogResult.OK)
                {
                    using (MySqlConnection conn = dbConn.GetConnection())
                    {
                        new MySqlCommand($"UPDATE tbl_mesin SET nama_mesin='{formInput.NamaMesin}', flavor='{formInput.Flavor}', operator='{formInput.OperatorMesin}', running_hour={formInput.RunningHour} WHERE kode_mesin='{kode}'", conn).ExecuteNonQuery();
                        LoadDataMesin();
                    }
                }
            }
        }

        private void BtnHapus_Click(object sender, EventArgs e)
        {
            if (dgvMesin.SelectedRows.Count == 0) return;
            string kode = dgvMesin.SelectedRows[0].Cells["Kode"].Value.ToString();
            if (MessageBox.Show("Hapus data?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (MySqlConnection conn = dbConn.GetConnection())
                {
                    new MySqlCommand($"DELETE FROM tbl_mesin WHERE kode_mesin='{kode}'", conn).ExecuteNonQuery();
                    LoadDataMesin();
                    UpdateBadgeNotif(); // Update badge jika ada notif terkait mesin ini ikut terhapus
                }
            }
        }

        // ==========================================
        // 5. FITUR TIMER & SIMULASI
        // ==========================================
        private void SetupTimer()
        {
            timerSimulasi = new System.Windows.Forms.Timer();
            timerSimulasi.Interval = 1000; 
            timerSimulasi.Tick += Timer_Tick; 
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            isSimulasiBerjalan = true;
            timerSimulasi.Start();
            btnStart.Enabled = false; 
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            isSimulasiBerjalan = false;
            timerSimulasi.Stop();
            btnStart.Enabled = true; 
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvMesin.DataSource;
            if (dt == null) return;

            bool isThereNewNotif = false; // Deteksi jika ada notif baru masuk

            using (MySqlConnection conn = dbConn.GetConnection())
            {
                foreach (DataRow row in dt.Rows)
                {
                    int currentRh = Convert.ToInt32(row["RH (jam)"]);
                    currentRh += 1; 
                    row["RH (jam)"] = currentRh; 

                    string kode = row["Kode"].ToString();
                    string nama = row["Nama"].ToString();

                    try
                    {
                        MySqlCommand cmd = new MySqlCommand("sp_cek_jadwal_maintenance", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_kode_mesin", kode);
                        cmd.Parameters.AddWithValue("@p_running_hour", currentRh);
                        cmd.ExecuteNonQuery();
                    }
                    catch { }

                    if (currentRh > 0)
                    {
                        if (currentRh % 2000 == 0) {
                            UpdateStatusLayar(row, nama, "[!!] Critical", $"[Critical] {nama} perlu Maintenance Berat ({currentRh} jam)");
                            isThereNewNotif = true;
                        } else if (currentRh % 1000 == 0) {
                            UpdateStatusLayar(row, nama, "[!] Warning", $"[Warning] {nama} perlu Maintenance Medium ({currentRh} jam)");
                            isThereNewNotif = true;
                        } else if (currentRh % 100 == 0) {
                            UpdateStatusLayar(row, nama, "[!] Maintenance", $"[Warning] {nama} perlu Maintenance Ringan ({currentRh} jam)");
                            isThereNewNotif = true;
                        }
                    }
                }
            }

            dgvMesin.Refresh(); 
            UpdateWarnaGrid();  
            UpdateStatistik();  
            
            // Jika ada notifikasi masuk di detik ini, hitung ulang badgenya
            if (isThereNewNotif) UpdateBadgeNotif(); 
        }

        // ==========================================
        // 6. FUNGSI HELPER UI
        // ==========================================
        private void UpdateStatusLayar(DataRow row, string nama, string statusBaru, string pesanNotif)
        {
            row["Status"] = statusBaru;
            lstNotifikasi.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") + " - " + pesanNotif);
        }

        private void UpdateWarnaGrid()
        {
            foreach (DataGridViewRow row in dgvMesin.Rows)
            {
                string status = row.Cells["Status"].Value.ToString();
                if (status == "[!!] Critical") row.DefaultCellStyle.BackColor = Color.LightCoral; 
                else if (status == "[!] Warning") row.DefaultCellStyle.BackColor = Color.Orange; 
                else if (status == "[!] Maintenance") row.DefaultCellStyle.BackColor = Color.LightYellow; 
                else row.DefaultCellStyle.BackColor = Color.White; 
            }
        }

        private void UpdateStatistik()
        {
            int totalMesin = dgvMesin.Rows.Count;
            int perluMaint = 0;
            int totalRh = 0;

            foreach (DataGridViewRow row in dgvMesin.Rows)
            {
                string status = row.Cells["Status"].Value.ToString();
                if (status != "Normal") perluMaint++;
                totalRh += Convert.ToInt32(row.Cells["RH (jam)"].Value);
            }

            lblTotalMesin.Text = "Total Mesin: " + totalMesin;
            lblPerluMaint.Text = "Perlu Maint: " + perluMaint;
            lblRataRh.Text = "Rata2 RH: " + (totalMesin > 0 ? (totalRh / totalMesin) : 0);
        }
    }
}