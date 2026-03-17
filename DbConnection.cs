using System;
using MySql.Data.MySqlClient;

namespace MaintenanceMesin
{
    public class DbConnection
    {
        private string connectionString = "Server=localhost;Database=assesment-indofood-net;Uid=root;Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}