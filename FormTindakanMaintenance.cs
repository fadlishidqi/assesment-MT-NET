using System;
using System.Drawing;
using System.Windows.Forms;
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
            this.Text = "SOP & Tindakan Maintenance";
            this.Size = new Size(400, 480);
            this.StartPosition = FormStartPosition.CenterParent; // Muncul di tengah
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Info Mesin
            Label lblInfo = new Label();
            lblInfo.Text = $"Kode Mesin\t: {kodeMesin}\nNama Mesin\t: {namaMesin}\nOperator\t: {operatorMesin}\nRunning Hour\t: {currentRh} Jam\nJenis Maint.\t: {jenisMaint}";
            lblInfo.Location = new Point(20, 20);
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("Arial", 10, FontStyle.Bold);

            // Label Perintah
            Label lblTindakan = new Label();
            lblTindakan.Text = "Daftar Tindakan (Centang jika sudah dilakukan):";
            lblTindakan.Location = new Point(20, 110);
            lblTindakan.AutoSize = true;

            // Kotak Centang (Checkbox List)
            clbTindakan = new CheckedListBox();
            clbTindakan.Location = new Point(20, 140);
            clbTindakan.Size = new Size(340, 220);
            clbTindakan.CheckOnClick = true;

            // Tombol Selesai
            btnSelesai = new Button();
            btnSelesai.Text = "✔ Selesai Maintenance";
            btnSelesai.Location = new Point(100, 380);
            btnSelesai.Size = new Size(180, 40);
            btnSelesai.BackColor = Color.LightGreen;
            btnSelesai.Click += BtnSelesai_Click;

            this.Controls.Add(lblInfo);
            this.Controls.Add(lblTindakan);
            this.Controls.Add(clbTindakan);
            this.Controls.Add(btnSelesai);
        }

        private void LoadTindakanDariDatabase()
        {
            try
            {
                using (MySqlConnection conn = dbConn.GetConnection())
                {
                    // Mengambil daftar tindakan dari database sesuai jenis maintenance-nya
                    string query = $"SELECT tindakan FROM tbl_jenis_tindakan WHERE jenis_maintenance = '{jenisMaint}'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // Memisahkan teks berdasarkan baris baru (Enter)
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
            // Validasi: Pastikan semua tindakan sudah dicentang
            if (clbTindakan.CheckedItems.Count < clbTindakan.Items.Count)
            {
                MessageBox.Show("Mohon selesaikan dan centang SEMUA tindakan sebelum mengakhiri maintenance!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = dbConn.GetConnection())
                {
                    // 1. Simpan ke Riwayat Maintenance
                    string queryRiwayat = $"INSERT INTO tbl_riwayat_maintenance (kode_mesin, jenis_maintenance, running_hour_saat_itu) VALUES ('{kodeMesin}', '{jenisMaint}', {currentRh})";
                    new MySqlCommand(queryRiwayat, conn).ExecuteNonQuery();

                    // 2. Kembalikan status mesin ke Normal
                    string queryUpdateMesin = $"UPDATE tbl_mesin SET status = 'Normal', terakhir_maintenance = NOW() WHERE kode_mesin = '{kodeMesin}'";
                    new MySqlCommand(queryUpdateMesin, conn).ExecuteNonQuery();

                    // 3. Update status notifikasi menjadi Sudah Dibaca
                    string queryUpdateNotif = $"UPDATE tbl_notifikasi SET status_baca = 'Sudah Dibaca' WHERE kode_mesin = '{kodeMesin}' AND status_baca = 'Belum Dibaca'";
                    new MySqlCommand(queryUpdateNotif, conn).ExecuteNonQuery();

                    MessageBox.Show("Maintenance berhasil diselesaikan dan dicatat ke riwayat!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Tutup form dengan status OK
                    this.DialogResult = DialogResult.OK; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat menyimpan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}