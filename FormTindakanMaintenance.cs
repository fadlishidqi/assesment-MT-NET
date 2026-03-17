using MySql.Data.MySqlClient;

namespace AssesmentIndofoodNet
{
    public class FormTindakanMaintenance : Form
    {
        private string kodeMesin, namaMesin, operatorMesin, jenisMaint;
        private int currentRh;
        private DbConnection dbConn;

        private CheckedListBox clbTindakan;
        private Button btnSelesai;

        public FormTindakanMaintenance(string kode, string nama, string op, int rh, string jenis)
        {
            kodeMesin = kode; 
            namaMesin = nama; 
            operatorMesin = op; 
            currentRh = rh; 
            jenisMaint = jenis;
            
            dbConn = new DbConnection();
            SetupUI();
            LoadTindakanDariDatabase();
        }

        private void SetupUI()
        {
            Text = "SOP & Tindakan Maintenance";
            Size = new Size(400, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label lblInfo = new Label
            {
                Text = $"Kode Mesin\t: {kodeMesin}\nNama Mesin\t: {namaMesin}\nOperator\t: {operatorMesin}\nRunning Hour\t: {currentRh} Jam\nJenis Maint.\t: {jenisMaint}",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            Label lblTindakan = new Label
            {
                Text = "Daftar Tindakan (Centang jika sudah dilakukan):",
                Location = new Point(20, 110),
                AutoSize = true
            };

            clbTindakan = new CheckedListBox
            {
                Location = new Point(20, 140),
                Size = new Size(340, 220),
                CheckOnClick = true
            };

            btnSelesai = new Button
            {
                Text = "✔ Selesai Maintenance",
                Location = new Point(100, 380),
                Size = new Size(180, 40),
                BackColor = Color.LightGreen
            };
            btnSelesai.Click += BtnSelesai_Click;

            Controls.Add(lblInfo);
            Controls.Add(lblTindakan);
            Controls.Add(clbTindakan);
            Controls.Add(btnSelesai);
        }

        private void LoadTindakanDariDatabase()
        {
            try
            {
                using (MySqlConnection conn = dbConn.GetConnection())
                {
                    string query = $"SELECT tindakan FROM tbl_jenis_tindakan WHERE jenis_maintenance = '{jenisMaint}'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string[] listTindakan = result.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string t in listTindakan)
                        {
                            clbTindakan.Items.Add(t.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat daftar tindakan: " + ex.Message);
            }
        }

        private void BtnSelesai_Click(object sender, EventArgs e)
        {
            if (clbTindakan.CheckedItems.Count < clbTindakan.Items.Count)
            {
                MessageBox.Show("Mohon selesaikan dan centang SEMUA tindakan sebelum mengakhiri maintenance!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = dbConn.GetConnection())
                {
                    string queryRiwayat = $"INSERT INTO tbl_riwayat_maintenance (kode_mesin, jenis_maintenance, running_hour_saat_itu) VALUES ('{kodeMesin}', '{jenisMaint}', {currentRh})";
                    new MySqlCommand(queryRiwayat, conn).ExecuteNonQuery();

                    string queryUpdateMesin = $"UPDATE tbl_mesin SET status = 'Normal', terakhir_maintenance = NOW() WHERE kode_mesin = '{kodeMesin}'";
                    new MySqlCommand(queryUpdateMesin, conn).ExecuteNonQuery();

                    string queryUpdateNotif = $"UPDATE tbl_notifikasi SET status_baca = 'Sudah Dibaca' WHERE kode_mesin = '{kodeMesin}' AND status_baca = 'Belum Dibaca'";
                    new MySqlCommand(queryUpdateNotif, conn).ExecuteNonQuery();

                    MessageBox.Show("Maintenance berhasil diselesaikan dan dicatat ke riwayat!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    DialogResult = DialogResult.OK; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat menyimpan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}