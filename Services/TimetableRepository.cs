// ============================================================
//  Services/TimetableRepository.cs
// ============================================================

using Microsoft.Data.Sqlite;
using SMS.Data;
using SMS.Models;

namespace SMS.Services
{
    public class TimetableRepository
    {
        public void Add(Timetable t)
        {
            using var conn = DatabaseHelper.GetConnection();
            DatabaseHelper.ExecuteNonQuery(conn, @"
                INSERT INTO Timetable (CourseId,Day,StartTime,EndTime,Room)
                VALUES ($c,$d,$s,$e,$r);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$c", t.CourseId);
                    cmd.Parameters.AddWithValue("$d", t.Day);
                    cmd.Parameters.AddWithValue("$s", t.StartTime);
                    cmd.Parameters.AddWithValue("$e", t.EndTime);
                    cmd.Parameters.AddWithValue("$r", t.Room);
                });
        }

        public void Delete(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            DatabaseHelper.ExecuteNonQuery(conn,
                "DELETE FROM Timetable WHERE Id=$id;",
                cmd => cmd.Parameters.AddWithValue("$id", id));
        }

        public List<Timetable> GetAll()
        {
            var list = new List<Timetable>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT t.*, c.Title AS CTitle, c.Code AS CCode
                FROM Timetable t
                INNER JOIN Courses c ON c.Id = t.CourseId
                ORDER BY
                    CASE t.Day
                        WHEN 'Monday'    THEN 1
                        WHEN 'Tuesday'   THEN 2
                        WHEN 'Wednesday' THEN 3
                        WHEN 'Thursday'  THEN 4
                        WHEN 'Friday'    THEN 5
                        ELSE 6
                    END, t.StartTime;";
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public List<Timetable> GetForStudent(int studentId)
        {
            var list = new List<Timetable>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT t.*, c.Title AS CTitle, c.Code AS CCode
                FROM Timetable t
                INNER JOIN Courses    c ON c.Id = t.CourseId
                INNER JOIN Enrollments e ON e.CourseId = t.CourseId
                WHERE e.StudentId = $s
                ORDER BY
                    CASE t.Day
                        WHEN 'Monday'    THEN 1
                        WHEN 'Tuesday'   THEN 2
                        WHEN 'Wednesday' THEN 3
                        WHEN 'Thursday'  THEN 4
                        WHEN 'Friday'    THEN 5
                        ELSE 6
                    END, t.StartTime;";
            cmd.Parameters.AddWithValue("$s", studentId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        private static Timetable Map(SqliteDataReader r) => new()
        {
            Id          = r.GetInt32(r.GetOrdinal("Id")),
            CourseId    = r.GetInt32(r.GetOrdinal("CourseId")),
            Day         = r.GetString(r.GetOrdinal("Day")),
            StartTime   = r.GetString(r.GetOrdinal("StartTime")),
            EndTime     = r.GetString(r.GetOrdinal("EndTime")),
            Room        = r.GetString(r.GetOrdinal("Room")),
            CourseTitle = r.GetString(r.GetOrdinal("CTitle")),
            CourseCode  = r.GetString(r.GetOrdinal("CCode")),
        };
    }
}
