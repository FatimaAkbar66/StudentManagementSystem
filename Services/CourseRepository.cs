// ============================================================
//  Services/CourseRepository.cs
//  DB operations for Courses and Enrollments tables.
// ============================================================

using Microsoft.Data.Sqlite;
using SMS.Data;
using SMS.Models;

namespace SMS.Services
{
    public class CourseRepository
    {
        // ── Courses ─────────────────────────────────────────

        public bool Add(Course c)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                DatabaseHelper.ExecuteNonQuery(conn, @"
                    INSERT INTO Courses (Code,Title,Instructor,CreditHours)
                    VALUES ($co,$t,$i,$cr);",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("$co", c.Code);
                        cmd.Parameters.AddWithValue("$t",  c.Title);
                        cmd.Parameters.AddWithValue("$i",  c.Instructor);
                        cmd.Parameters.AddWithValue("$cr", c.CreditHours);
                    });
                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return false; // duplicate code
            }
        }

        public List<Course> GetAll()
        {
            var list = new List<Course>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Courses ORDER BY Code;";
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public Course? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Courses WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        public void Delete(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            DatabaseHelper.ExecuteNonQuery(conn,
                "DELETE FROM Courses WHERE Id=$id;",
                cmd => cmd.Parameters.AddWithValue("$id", id));
        }

        public int GetEnrolledCount(int courseId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Enrollments WHERE CourseId=$c;";
            cmd.Parameters.AddWithValue("$c", courseId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Enrollments ─────────────────────────────────────

        public bool EnrollStudent(int studentId, int courseId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                DatabaseHelper.ExecuteNonQuery(conn, @"
                    INSERT INTO Enrollments (StudentId,CourseId,EnrolledOn)
                    VALUES ($s,$c,$e);",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("$s", studentId);
                        cmd.Parameters.AddWithValue("$c", courseId);
                        cmd.Parameters.AddWithValue("$e", DateTime.Now.ToString("o"));
                    });
                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return false; // already enrolled
            }
        }

        public bool IsEnrolled(int studentId, int courseId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Enrollments WHERE StudentId=$s AND CourseId=$c;";
            cmd.Parameters.AddWithValue("$s", studentId);
            cmd.Parameters.AddWithValue("$c", courseId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<Course> GetCoursesForStudent(int studentId)
        {
            var list = new List<Course>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT c.* FROM Courses c
                INNER JOIN Enrollments e ON e.CourseId = c.Id
                WHERE e.StudentId = $s
                ORDER BY c.Code;";
            cmd.Parameters.AddWithValue("$s", studentId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public List<Student> GetStudentsInCourse(int courseId)
        {
            var list = new List<Student>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT s.* FROM Students s
                INNER JOIN Enrollments e ON e.StudentId = s.Id
                WHERE e.CourseId = $c
                ORDER BY s.Name;";
            cmd.Parameters.AddWithValue("$c", courseId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Student
                {
                    Id         = r.GetInt32(r.GetOrdinal("Id")),
                    Name       = r.GetString(r.GetOrdinal("Name")),
                    Email      = r.GetString(r.GetOrdinal("Email")),
                    Department = r.GetString(r.GetOrdinal("Department")),
                    Semester   = r.GetInt32(r.GetOrdinal("Semester")),
                    Phone      = r.IsDBNull(r.GetOrdinal("Phone")) ? "" : r.GetString(r.GetOrdinal("Phone")),
                });
            return list;
        }

        // ── Mapper ──────────────────────────────────────────

        private static Course Map(SqliteDataReader r) => new()
        {
            Id          = r.GetInt32(r.GetOrdinal("Id")),
            Code        = r.GetString(r.GetOrdinal("Code")),
            Title       = r.GetString(r.GetOrdinal("Title")),
            Instructor  = r.GetString(r.GetOrdinal("Instructor")),
            CreditHours = r.GetInt32(r.GetOrdinal("CreditHours")),
        };
    }
}
