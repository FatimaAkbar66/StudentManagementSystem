// ============================================================
//  Services/AdminRepository.cs
// ============================================================

using Microsoft.Data.Sqlite;
using SMS.Data;
using SMS.Models;

namespace SMS.Services
{
    public class AdminRepository
    {
        public Admin? GetByCredentials(string username, string password)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Admins WHERE Username=$u AND Password=$p;";
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$p", password);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new Admin
            {
                Id       = r.GetInt32(r.GetOrdinal("Id")),
                Username = r.GetString(r.GetOrdinal("Username")),
                Password = r.GetString(r.GetOrdinal("Password")),
            };
        }

        public bool UsernameExists(string username)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Admins WHERE Username=$u;";
            cmd.Parameters.AddWithValue("$u", username);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void Add(Admin a)
        {
            using var conn = DatabaseHelper.GetConnection();
            DatabaseHelper.ExecuteNonQuery(conn,
                "INSERT INTO Admins (Username,Password) VALUES ($u,$p);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$u", a.Username);
                    cmd.Parameters.AddWithValue("$p", a.Password);
                });
        }
    }
}
