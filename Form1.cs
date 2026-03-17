using System.Data;

namespace AssesmentIndofoodNet
{
    public partial class Form1 : Form
    {
        private Label lblTotalMesin, lblPerluMaint, lblRataRh, lblBadgeNotif;
        private Button btnStart, btnStop, btnProsesMaint, btnTambah, btnEdit, btnHapus;
        private DataGridView dgvMesin;
        private ListBox lstNotifikasi;

        private System.Windows.Forms.Timer timerSimulasi;
        private bool isSimulasiBerjalan;
        private MesinRepository _repository;
        private MaintenanceService _service;

        public Form1()
        {
            _repository = new MesinRepository();
            _service = new MaintenanceService();

            SetupUI();
            LoadDataMesin();
            SetupTimer();
            UpdateStatistik();
            UpdateBadgeNotif();
        }

        private void SetupUI()
        {
            Text = "Sistem Monitoring Maintenance Mesin - Indofood";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

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

            dgvMesin = new DataGridView() { Location = new Point(20, 110), Size = new Size(930, 250), AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            lblBadgeNotif = new Label() { Text = "0", Location = new Point(195, 378), AutoSize = true, BackColor = Color.Red, ForeColor = Color.White, Font = new Font("Arial", 9, FontStyle.Bold), Padding = new Padding(3), Visible = false };
            lstNotifikasi = new ListBox() { Location = new Point(20, 410), Size = new Size(930, 150), Font = new Font("Arial", 10) };
            Label lblNotif = new Label() { Text = "NOTIFIKASI TERBARU:", Location = new Point(20, 380), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            Controls.AddRange([lblTotalMesin, lblPerluMaint, lblRataRh, btnStart, btnStop, btnProsesMaint, btnTambah, btnEdit, btnHapus, dgvMesin, lblNotif, lblBadgeNotif, lstNotifikasi]);
        }

        private void LoadDataMesin()
        {
            try
            {
                dgvMesin.DataSource = _repository.GetAllMesin();
                dgvMesin.Columns["id"].Visible = false;
                UpdateWarnaGrid();
                UpdateStatistik();
            }
            catch (Exception ex) { MessageBox.Show("Gagal memuat data: " + ex.Message); }
        }

        private void UpdateBadgeNotif()
        {
            int count = _repository.GetUnreadNotificationCount();
            lblBadgeNotif.Text = count.ToString();
            lblBadgeNotif.Visible = count > 0;
        }

        private void BtnProsesMaint_Click(object sender, EventArgs e)
        {
            if (dgvMesin.SelectedRows.Count == 0) return;
            var row = dgvMesin.SelectedRows[0];
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
            string jenisMaint = _service.DapatkanJenisMaintenance(status);

            using (FormTindakanMaintenance form = new FormTindakanMaintenance(kode, nama, op, rh, jenisMaint))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadDataMesin();
                    UpdateBadgeNotif();
                    lstNotifikasi.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - [Info] {nama} maintenance selesai.");
                }
            }
        }

        private void BtnTambah_Click(object sender, EventArgs e)
        {
            using (FormInputMesin form = new FormInputMesin())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _repository.InsertMesin(form.KodeMesin, form.NamaMesin, form.Flavor, form.OperatorMesin, form.RunningHour);
                    LoadDataMesin();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvMesin.SelectedRows.Count == 0) return;
            var row = dgvMesin.SelectedRows[0];
            string kode = row.Cells["Kode"].Value.ToString();

            using (FormInputMesin form = new FormInputMesin(kode, row.Cells["Nama"].Value.ToString(), row.Cells["Flavor"].Value.ToString(), row.Cells["Operator"].Value.ToString(), Convert.ToInt32(row.Cells["RH (jam)"].Value)))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _repository.UpdateMesin(kode, form.NamaMesin, form.Flavor, form.OperatorMesin, form.RunningHour);
                    LoadDataMesin();
                }
            }
        }

        private void BtnHapus_Click(object sender, EventArgs e)
        {
            if (dgvMesin.SelectedRows.Count == 0) return;
            string kode = dgvMesin.SelectedRows[0].Cells["Kode"].Value.ToString();
            if (MessageBox.Show("Hapus data?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _repository.DeleteMesin(kode);
                LoadDataMesin();
                UpdateBadgeNotif();
            }
        }

        private void SetupTimer()
        {
            timerSimulasi = new System.Windows.Forms.Timer { Interval = 1000 };
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
            if (!isSimulasiBerjalan) return;

            DataTable dt = (DataTable)dgvMesin.DataSource;
            if (dt == null) return;
            bool isThereNewNotif = false;

            foreach (DataRow row in dt.Rows)
            {
                int currentRh = Convert.ToInt32(row["RH (jam)"]) + 1;
                row["RH (jam)"] = currentRh;
                string kode = row["Kode"].ToString();
                string nama = row["Nama"].ToString();

                _repository.ExecuteCekJadwalStoredProcedure(kode, currentRh);

                string statusBaru = _service.TentukanStatus(currentRh);
                if (statusBaru != null)
                {
                    row["Status"] = statusBaru;
                    lstNotifikasi.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - {_service.GetPesanNotifikasi(nama, statusBaru, currentRh)}");
                    isThereNewNotif = true;
                }
            }

            dgvMesin.Refresh();
            UpdateWarnaGrid();
            UpdateStatistik();
            if (isThereNewNotif) UpdateBadgeNotif();
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
            int total = dgvMesin.Rows.Count;
            int perlu = 0; int totalRh = 0;
            foreach (DataGridViewRow row in dgvMesin.Rows)
            {
                if (row.Cells["Status"].Value.ToString() != "Normal") perlu++;
                totalRh += Convert.ToInt32(row.Cells["RH (jam)"].Value);
            }
            lblTotalMesin.Text = "Total Mesin: " + total;
            lblPerluMaint.Text = "Perlu Maint: " + perlu;
            lblRataRh.Text = "Rata2 RH: " + (total > 0 ? (totalRh / total) : 0);
        }
    }
}