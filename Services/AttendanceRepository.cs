// ============================================================
//  Services/AttendanceRepository.cs
// ============================================================

using Microsoft.Data.Sqlite;
using SMS.Data;
using SMS.Models;

namespace SMS.Services
{
    public class AttendanceRepository
    {
        public void MarkOrUpdate(Attendance a)
        {
            using var conn = DatabaseHelper.GetConnection();
            DatabaseHelper.ExecuteNonQuery(conn, @"
                INSERT INTO Attendance (StudentId,CourseId,Date,Status)
                VALUES ($s,$c,$d,$st)
                ON CONFLICT(StudentId,CourseId,Date) DO UPDATE SET Status=$st;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$s",  a.StudentId);
                    cmd.Parameters.AddWithValue("$c",  a.CourseId);
                    cmd.Parameters.AddWithValue("$d",  a.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("$st", a.Status.ToString());
                });
        }

        public List<Attendance> GetForStudent(int studentId)
        {
            var list = new List<Attendance>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT a.*, c.Title AS CTitle
                FROM Attendance a
                INNER JOIN Courses c ON c.Id = a.CourseId
                WHERE a.StudentId = $s
                ORDER BY a.Date DESC;";
            cmd.Parameters.AddWithValue("$s", studentId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Attendance
                {
                    Id         = r.GetInt32(r.GetOrdinal("Id")),
                    StudentId  = studentId,
                    CourseId   = r.GetInt32(r.GetOrdinal("CourseId")),
                    Date       = DateTime.Parse(r.GetString(r.GetOrdinal("Date"))),
                    Status     = Enum.Parse<AttendanceStatus>(r.GetString(r.GetOrdinal("Status"))),
                    CourseName = r.GetString(r.GetOrdinal("CTitle")),
                });
            return list;
        }

        /// <summary>Returns attendance % for a student in one course.</summary>
        public (int total, int present) GetSummary(int studentId, int courseId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    COUNT(*) AS Total,
                    SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END) AS Present
                FROM Attendance
                WHERE StudentId=$s AND CourseId=$c;";
            cmd.Parameters.AddWithValue("$s", studentId);
            cmd.Parameters.AddWithValue("$c", courseId);
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return (r.GetInt32(0), r.GetInt32(1));
            return (0, 0);
        }

        public List<Attendance> GetForStudentCourse(int studentId, int courseId)
        {
            var list = new List<Attendance>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT * FROM Attendance
                WHERE StudentId=$s AND CourseId=$c
                ORDER BY Date DESC;";
            cmd.Parameters.AddWithValue("$s", studentId);
            cmd.Parameters.AddWithValue("$c", courseId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Attendance
                {
                    Id        = r.GetInt32(r.GetOrdinal("Id")),
                    StudentId = studentId,
                    CourseId  = courseId,
                    Date      = DateTime.Parse(r.GetString(r.GetOrdinal("Date"))),
                    Status    = Enum.Parse<AttendanceStatus>(r.GetString(r.GetOrdinal("Status"))),
                });
            return list;
        }
    }
}
