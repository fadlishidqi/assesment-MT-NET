using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace AssesmentIndofoodNet
{
    public class DbConnection
    {
        private readonly string connectionString = "Server=localhost;Database=assesment-indofood-net;User ID=root;Password=;";

        public MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal terkoneksi ke database.\n\nPesan Error: " + ex.Message, "Error Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return conn;
        }

        // ==========================================
        // FITUR AUTO-MIGRATE (TIDAK PERLU KE PHPMYADMIN)
        // ==========================================
        public void AutoMigrate()
        {
            using (MySqlConnection conn = GetConnection())
            {
                if (conn.State != ConnectionState.Open) return;

                try
                {
                    // 1. BUAT TABEL OTOMATIS JIKA BELUM ADA
                    string queryTables = @"
                        CREATE TABLE IF NOT EXISTS tbl_mesin (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            kode_mesin VARCHAR(10) UNIQUE NOT NULL,
                            nama_mesin VARCHAR(50) NOT NULL,
                            flavor VARCHAR(20) NOT NULL,
                            operator VARCHAR(50) NOT NULL,
                            running_hour INT DEFAULT 0,
                            status VARCHAR(50) DEFAULT 'Normal',
                            terakhir_maintenance DATETIME NULL
                        );

                        CREATE TABLE IF NOT EXISTS tbl_jenis_tindakan (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            interval_jam INT NOT NULL,
                            jenis_maintenance VARCHAR(50) NOT NULL,
                            tindakan TEXT NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS tbl_notifikasi (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            kode_mesin VARCHAR(10) NOT NULL,
                            pesan VARCHAR(255) NOT NULL,
                            tingkat_urgensi VARCHAR(20) NOT NULL,
                            status_baca VARCHAR(20) DEFAULT 'Belum Dibaca',
                            waktu_notifikasi DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (kode_mesin) REFERENCES tbl_mesin(kode_mesin) ON DELETE CASCADE
                        );

                        CREATE TABLE IF NOT EXISTS tbl_riwayat_maintenance (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            kode_mesin VARCHAR(10) NOT NULL,
                            jenis_maintenance VARCHAR(50) NOT NULL,
                            running_hour_saat_itu INT NOT NULL,
                            tanggal_selesai DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (kode_mesin) REFERENCES tbl_mesin(kode_mesin) ON DELETE CASCADE
                        );
                    ";
                    new MySqlCommand(queryTables, conn).ExecuteNonQuery();

                    // 2. INSERT DATA DUMMY TINDAKAN JIKA KOSONG
                    int countTindakan = Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM tbl_jenis_tindakan", conn).ExecuteScalar());
                    if (countTindakan == 0)
                    {
                        string insertTindakan = @"
                            INSERT INTO tbl_jenis_tindakan (interval_jam, jenis_maintenance, tindakan) VALUES
                            (100, 'Maintenance Ringan', '1. Pembersihan debu\n2. Pengecekan oli\n3. Pelumasan komponen gerak\n4. Pengecekan baut dan mur'),
                            (1000, 'Maintenance Medium', '1. Semua tindakan maintenance ringan\n2. Ganti oli gearbox\n3. Kalibrasi sensor\n4. Pengecekan keausan belt\n5. Pembersihan filter udara'),
                            (2000, 'Maintenance Berat', '1. Semua tindakan maintenance medium\n2. Overhaul mesin\n3. Ganti bearing\n4. Ganti seal dan gasket\n5. Test performa menyeluruh');
                        ";
                        new MySqlCommand(insertTindakan, conn).ExecuteNonQuery();
                    }

                    // 3. INSERT 10 DATA MESIN AWAL JIKA KOSONG
                    int countMesin = Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM tbl_mesin", conn).ExecuteScalar());
                    if (countMesin == 0)
                    {
                        string insertMesin = @"
                            INSERT INTO tbl_mesin (kode_mesin, nama_mesin, flavor, operator, running_hour) VALUES
                            ('M001', 'Mesin 1', 'Flavor A', 'Agus', 0),
                            ('M002', 'Mesin 2', 'Flavor A', 'Agus', 0),
                            ('M003', 'Mesin 3', 'Flavor A', 'Agus', 0),
                            ('M004', 'Mesin 4', 'Flavor B', 'Supri', 0),
                            ('M005', 'Mesin 5', 'Flavor B', 'Supri', 0),
                            ('M006', 'Mesin 6', 'Flavor B', 'Supri', 0),
                            ('M007', 'Mesin 7', 'Flavor C', 'Yanto', 0),
                            ('M008', 'Mesin 8', 'Flavor C', 'Yanto', 0),
                            ('M009', 'Mesin 9', 'Flavor C', 'Yanto', 0),
                            ('M010', 'Mesin 10', 'Flavor C', 'Yanto', 0);
                        ";
                        new MySqlCommand(insertMesin, conn).ExecuteNonQuery();
                    }

                    // 4. BUAT STORED PROCEDURE OTOMATIS
                    new MySqlCommand("DROP PROCEDURE IF EXISTS sp_cek_jadwal_maintenance;", conn).ExecuteNonQuery();
                    string createSp = @"
                        CREATE PROCEDURE sp_cek_jadwal_maintenance(
                            IN p_kode_mesin VARCHAR(10),
                            IN p_running_hour INT
                        )
                        BEGIN
                            DECLARE v_status VARCHAR(50);
                            DECLARE v_urgensi VARCHAR(20);
                            DECLARE v_pesan VARCHAR(255);
                            DECLARE v_nama_mesin VARCHAR(50);
                            
                            SELECT nama_mesin INTO v_nama_mesin FROM tbl_mesin WHERE kode_mesin = p_kode_mesin;

                            IF p_running_hour > 0 THEN
                                IF p_running_hour % 2000 = 0 THEN
                                    SET v_status = '[!!] Critical';
                                    SET v_urgensi = 'Critical';
                                    SET v_pesan = CONCAT('[Critical] ', v_nama_mesin, ' perlu Maintenance Berat (', p_running_hour, ' jam)');
                                ELSEIF p_running_hour % 1000 = 0 THEN
                                    SET v_status = '[!] Warning';
                                    SET v_urgensi = 'Warning';
                                    SET v_pesan = CONCAT('[Warning] ', v_nama_mesin, ' perlu Maintenance Medium (', p_running_hour, ' jam)');
                                ELSEIF p_running_hour % 100 = 0 THEN
                                    SET v_status = '[!] Maintenance';
                                    SET v_urgensi = 'Warning';
                                    SET v_pesan = CONCAT('[Warning] ', v_nama_mesin, ' perlu Maintenance Ringan (', p_running_hour, ' jam)');
                                END IF;
                                
                                IF v_status IS NOT NULL THEN
                                    UPDATE tbl_mesin SET status = v_status, running_hour = p_running_hour WHERE kode_mesin = p_kode_mesin;
                                    INSERT INTO tbl_notifikasi (kode_mesin, pesan, tingkat_urgensi) VALUES (p_kode_mesin, v_pesan, v_urgensi);
                                ELSE
                                    UPDATE tbl_mesin SET running_hour = p_running_hour WHERE kode_mesin = p_kode_mesin;
                                END IF;
                            ELSE
                                UPDATE tbl_mesin SET running_hour = p_running_hour WHERE kode_mesin = p_kode_mesin;
                            END IF;
                        END;
                    ";
                    new MySqlCommand(createSp, conn).ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal melakukan Auto-Migrate: " + ex.Message, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}