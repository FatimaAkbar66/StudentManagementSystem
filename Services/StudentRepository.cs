// ============================================================
//  Services/StudentRepository.cs
//
//  All database operations that involve the Students table.
//  Uses parameterised queries to prevent SQL injection.
// ============================================================

using Microsoft.Data.Sqlite;
using SMS.Data;
using SMS.Models;

namespace SMS.Services
{
    public class StudentRepository
    {
        // ── CRUD ────────────────────────────────────────────

        public bool Add(Student s)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                DatabaseHelper.ExecuteNonQuery(conn, @"
                    INSERT INTO Students
                        (Name, Email, Password, Department, Semester, Phone, DateOfBirth, EnrolledOn)
                    VALUES ($n,$e,$p,$d,$s,$ph,$dob,$en);",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("$n",  s.Name);
                        cmd.Parameters.AddWithValue("$e",  s.Email);
                        cmd.Parameters.AddWithValue("$p",  s.Password);
                        cmd.Parameters.AddWithValue("$d",  s.Department);
                        cmd.Parameters.AddWithValue("$s",  s.Semester);
                        cmd.Parameters.AddWithValue("$ph", s.Phone);
                        cmd.Parameters.AddWithValue("$dob",s.DateOfBirth.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("$en", DateTime.Now.ToString("o"));
                    });
                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // UNIQUE constraint failed (duplicate email)
                return false;
            }
        }

        public List<Student> GetAll()
        {
            var list = new List<Student>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Students ORDER BY Name;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        public Student? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Students WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public Student? GetByEmail(string email)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Students WHERE Email = $e;";
            cmd.Parameters.AddWithValue("$e", email);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public List<Student> Search(string keyword)
        {
            var list = new List<Student>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT * FROM Students
                WHERE LOWER(Name)  LIKE $kw
                   OR LOWER(Email) LIKE $kw
                ORDER BY Name;";
            cmd.Parameters.AddWithValue("$kw", $"%{keyword.ToLower()}%");
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public bool Update(Student s)
        {
            using var conn = DatabaseHelper.GetConnection();
            DatabaseHelper.ExecuteNonQuery(conn, @"
                UPDATE Students
                SET Name=$n, Department=$d, Semester=$s, Phone=$ph, Password=$p
                WHERE Id=$id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$n",  s.Name);
                    cmd.Parameters.AddWithValue("$d",  s.Department);
                    cmd.Parameters.AddWithValue("$s",  s.Semester);
                    cmd.Parameters.AddWithValue("$ph", s.Phone);
                    cmd.Parameters.AddWithValue("$p",  s.Password);
                    cmd.Parameters.AddWithValue("$id", s.Id);
                });
            return true;
        }

        public void Delete(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            // Cascade deletes enrollments, grades, attendance, fees automatically
            DatabaseHelper.ExecuteNonQuery(conn,
                "DELETE FROM Students WHERE Id = $id;",
                cmd => cmd.Parameters.AddWithValue("$id", id));
        }

        // ── Mapper ──────────────────────────────────────────

        private static Student Map(SqliteDataReader r) => new()
        {
            Id          = r.GetInt32(r.GetOrdinal("Id")),
            Name        = r.GetString(r.GetOrdinal("Name")),
            Email       = r.GetString(r.GetOrdinal("Email")),
            Password    = r.GetString(r.GetOrdinal("Password")),
            Department  = r.GetString(r.GetOrdinal("Department")),
            Semester    = r.GetInt32(r.GetOrdinal("Semester")),
            Phone       = r.IsDBNull(r.GetOrdinal("Phone")) ? "" : r.GetString(r.GetOrdinal("Phone")),
            EnrolledOn  = DateTime.Parse(r.GetString(r.GetOrdinal("EnrolledOn"))),
        };
    }
}
