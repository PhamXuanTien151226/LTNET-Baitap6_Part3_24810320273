using MySql.Data.MySqlClient;

namespace Lab6Part3App.DAL
{
    public static class DatabaseHelper
    {
        public static readonly string LibConn = "Server=localhost;Port=3306;Database=LibraryDB;Uid=root;Pwd=;Charset=utf8mb4;";
        public static readonly string ClinicConn = "Server=localhost;Port=3306;Database=ClinicDB;Uid=root;Pwd=;Charset=utf8mb4;";
        public static readonly string BilliardsConn = "Server=localhost;Port=3306;Database=BilliardsDB;Uid=root;Pwd=;Charset=utf8mb4;";

        public static MySqlConnection GetLibConnection() => new MySqlConnection(LibConn);
        public static MySqlConnection GetClinicConnection() => new MySqlConnection(ClinicConn);
        public static MySqlConnection GetBilliardsConnection() => new MySqlConnection(BilliardsConn);
    }
}