namespace AssesmentIndofoodNet
{
    public class FormInputMesin : Form
    {
        public string KodeMesin { get; private set; }
        public string NamaMesin { get; private set; }
        public string Flavor { get; private set; }
        public string OperatorMesin { get; private set; }
        public int RunningHour { get; private set; }

        private TextBox txtKode, txtNama, txtOperator, txtRH;
        private ComboBox cbFlavor;
        private Button btnSimpan, btnBatal;

        public FormInputMesin(string kode = "", string nama = "", string flavor = "", string op = "", int rh = 0)
        {
            bool isEdit = !string.IsNullOrEmpty(kode);

            Text = isEdit ? "Edit Data Mesin" : "Tambah Mesin Baru";
            Size = new Size(350, 330);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label lblKode = new Label() { Text = "Kode Mesin:", Location = new Point(20, 20), AutoSize = true };
            txtKode = new TextBox() { Location = new Point(120, 20), Width = 180, Text = kode };
            if (isEdit) txtKode.Enabled = false;

            Label lblNama = new Label() { Text = "Nama Mesin:", Location = new Point(20, 60), AutoSize = true };
            txtNama = new TextBox() { Location = new Point(120, 60), Width = 180, Text = nama };

            Label lblFlavor = new Label() { Text = "Flavor:", Location = new Point(20, 100), AutoSize = true };
            cbFlavor = new ComboBox() { Location = new Point(120, 100), Width = 180 };
            cbFlavor.Items.AddRange(["Flavor A", "Flavor B", "Flavor C"]);
            cbFlavor.SelectedItem = string.IsNullOrEmpty(flavor) ? "Flavor A" : flavor;

            Label lblOp = new Label() { Text = "Operator:", Location = new Point(20, 140), AutoSize = true };
            txtOperator = new TextBox() { Location = new Point(120, 140), Width = 180, Text = op };

            Label lblRH = new Label() { Text = "RH Awal:", Location = new Point(20, 180), AutoSize = true };
            txtRH = new TextBox() { Location = new Point(120, 180), Width = 180, Text = rh.ToString() };

            btnSimpan = new Button() { Text = "Simpan", Location = new Point(120, 230), Width = 80, BackColor = Color.LightGreen };
            btnBatal = new Button() { Text = "Batal", Location = new Point(220, 230), Width = 80, BackColor = Color.LightCoral };

            btnSimpan.Click += BtnSimpan_Click;
            btnBatal.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblKode); Controls.Add(txtKode);
            Controls.Add(lblNama); Controls.Add(txtNama);
            Controls.Add(lblFlavor); Controls.Add(cbFlavor);
            Controls.Add(lblOp); Controls.Add(txtOperator);
            Controls.Add(lblRH); Controls.Add(txtRH);
            Controls.Add(btnSimpan); Controls.Add(btnBatal);
        }

        private void BtnSimpan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKode.Text) || string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Kode dan Nama Mesin tidak boleh kosong!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtRH.Text, out int parsedRh))
            {
                MessageBox.Show("Running Hour harus berupa angka!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (parsedRh < 0 || parsedRh > 5000)
            {
                MessageBox.Show("Running Hour harus berada dalam rentang 0 sampai 5000 jam!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            KodeMesin = txtKode.Text;
            NamaMesin = txtNama.Text;
            Flavor = cbFlavor.SelectedItem.ToString();
            OperatorMesin = txtOperator.Text;
            RunningHour = parsedRh;

            DialogResult = DialogResult.OK; 
        }
    }
}