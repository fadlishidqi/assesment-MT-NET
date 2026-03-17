using System.Data;
using MySql.Data.MySqlClient;

namespace AssesmentIndofoodNet
{
    public class MesinRepository
    {
        private readonly DbConnection _dbConn;

        public MesinRepository()
        {
            _dbConn = new DbConnection();
            _dbConn.AutoMigrate();
        }

        public DataTable GetAllMesin()
        {
            using (MySqlConnection conn = _dbConn.GetConnection())
            {
                string query = "SELECT id, kode_mesin AS 'Kode', nama_mesin AS 'Nama', flavor AS 'Flavor', operator AS 'Operator', running_hour AS 'RH (jam)', status AS 'Status' FROM tbl_mesin";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public int GetUnreadNotificationCount()
        {
            using (MySqlConnection conn = _dbConn.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM tbl_notifikasi WHERE status_baca = 'Belum Dibaca'";
                return Convert.ToInt32(new MySqlCommand(query, conn).ExecuteScalar());
            }
        }

        public void ExecuteCekJadwalStoredProcedure(string kode, int rh)
        {
            using (MySqlConnection conn = _dbConn.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("sp_cek_jadwal_maintenance", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@p_kode_mesin", kode);
                cmd.Parameters.AddWithValue("@p_running_hour", rh);
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertMesin(string kode, string nama, string flavor, string op, int rh)
        {
            using (MySqlConnection conn = _dbConn.GetConnection())
            {
                string query = $"INSERT INTO tbl_mesin (kode_mesin, nama_mesin, flavor, operator, running_hour) VALUES ('{kode}', '{nama}', '{flavor}', '{op}', {rh})";
                new MySqlCommand(query, conn).ExecuteNonQuery();
            }
        }

        public void UpdateMesin(string kode, string nama, string flavor, string op, int rh)
        {
            using (MySqlConnection conn = _dbConn.GetConnection())
            {
                string query = $"UPDATE tbl_mesin SET nama_mesin='{nama}', flavor='{flavor}', operator='{op}', running_hour={rh} WHERE kode_mesin='{kode}'";
                new MySqlCommand(query, conn).ExecuteNonQuery();
            }
        }

        public void DeleteMesin(string kode)
        {
            using (MySqlConnection conn = _dbConn.GetConnection())
            {
                string query = $"DELETE FROM tbl_mesin WHERE kode_mesin='{kode}'";
                new MySqlCommand(query, conn).ExecuteNonQuery();
            }
        }
    }
}