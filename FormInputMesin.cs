using System;
using System.Drawing;
using System.Windows.Forms;

namespace AssesmentIndofoodNet
{
    // Form kecil ini berfungsi sebagai jendela Pop-up untuk Tambah dan Edit data
    public class FormInputMesin : Form
    {
        // Variabel untuk menyimpan data yang diketik user
        public string KodeMesin { get; private set; }
        public string NamaMesin { get; private set; }
        public string Flavor { get; private set; }
        public string OperatorMesin { get; private set; }
        public int RunningHour { get; private set; }

        private TextBox txtKode, txtNama, txtOperator, txtRH;
        private ComboBox cbFlavor;
        private Button btnSimpan, btnBatal;

        // Constructor: Jika 'kode' kosong berarti mode Tambah, jika ada isinya berarti mode Edit
        public FormInputMesin(string kode = "", string nama = "", string flavor = "", string op = "", int rh = 0)
        {
            bool isEdit = !string.IsNullOrEmpty(kode);

            this.Text = isEdit ? "Edit Data Mesin" : "Tambah Mesin Baru";
            this.Size = new Size(350, 330);
            this.StartPosition = FormStartPosition.CenterParent; // Muncul di tengah form utama
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Membuat inputan form
            Label lblKode = new Label() { Text = "Kode Mesin:", Location = new Point(20, 20), AutoSize = true };
            txtKode = new TextBox() { Location = new Point(120, 20), Width = 180, Text = kode };
            if (isEdit) txtKode.Enabled = false; // Kode mesin tidak boleh diubah saat edit

            Label lblNama = new Label() { Text = "Nama Mesin:", Location = new Point(20, 60), AutoSize = true };
            txtNama = new TextBox() { Location = new Point(120, 60), Width = 180, Text = nama };

            Label lblFlavor = new Label() { Text = "Flavor:", Location = new Point(20, 100), AutoSize = true };
            cbFlavor = new ComboBox() { Location = new Point(120, 100), Width = 180 };
            cbFlavor.Items.AddRange(new string[] { "Flavor A", "Flavor B", "Flavor C" });
            cbFlavor.SelectedItem = string.IsNullOrEmpty(flavor) ? "Flavor A" : flavor;

            Label lblOp = new Label() { Text = "Operator:", Location = new Point(20, 140), AutoSize = true };
            txtOperator = new TextBox() { Location = new Point(120, 140), Width = 180, Text = op };

            Label lblRH = new Label() { Text = "RH Awal:", Location = new Point(20, 180), AutoSize = true };
            txtRH = new TextBox() { Location = new Point(120, 180), Width = 180, Text = rh.ToString() };

            btnSimpan = new Button() { Text = "Simpan", Location = new Point(120, 230), Width = 80, BackColor = Color.LightGreen };
            btnBatal = new Button() { Text = "Batal", Location = new Point(220, 230), Width = 80, BackColor = Color.LightCoral };

            // Aksi saat tombol diklik
            btnSimpan.Click += BtnSimpan_Click;
            btnBatal.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Masukkan ke layar
            this.Controls.Add(lblKode); this.Controls.Add(txtKode);
            this.Controls.Add(lblNama); this.Controls.Add(txtNama);
            this.Controls.Add(lblFlavor); this.Controls.Add(cbFlavor);
            this.Controls.Add(lblOp); this.Controls.Add(txtOperator);
            this.Controls.Add(lblRH); this.Controls.Add(txtRH);
            this.Controls.Add(btnSimpan); this.Controls.Add(btnBatal);
        }

        private void BtnSimpan_Click(object sender, EventArgs e)
        {
            // Validasi data tidak boleh kosong
            if (string.IsNullOrWhiteSpace(txtKode.Text) || string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Kode dan Nama Mesin tidak boleh kosong!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            KodeMesin = txtKode.Text;
            NamaMesin = txtNama.Text;
            Flavor = cbFlavor.SelectedItem.ToString();
            OperatorMesin = txtOperator.Text;
            
            int parsedRh = 0;
            int.TryParse(txtRH.Text, out parsedRh);
            RunningHour = parsedRh;

            this.DialogResult = DialogResult.OK; // Tandai form sukses diisi
        }
    }
}