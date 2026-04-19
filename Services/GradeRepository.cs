// ============================================================
//  Services/GradeRepository.cs
// ============================================================

using Microsoft.Data.Sqlite;
using SMS.Data;
using SMS.Models;

namespace SMS.Services
{
    public class GradeRepository
    {
        public void Upsert(Grade g)
        {
            using var conn = DatabaseHelper.GetConnection();
            // INSERT OR REPLACE handles both add and update
            DatabaseHelper.ExecuteNonQuery(conn, @"
                INSERT INTO Grades (StudentId,CourseId,Marks,TotalMarks,LetterGrade)
                VALUES ($s,$c,$m,$t,$l)
                ON CONFLICT(StudentId,CourseId) DO UPDATE
                SET Marks=$m, LetterGrade=$l;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$s", g.StudentId);
                    cmd.Parameters.AddWithValue("$c", g.CourseId);
                    cmd.Parameters.AddWithValue("$m", g.Marks);
                    cmd.Parameters.AddWithValue("$t", g.TotalMarks);
                    cmd.Parameters.AddWithValue("$l", g.LetterGrade);
                });
        }

        public List<Grade> GetForStudent(int studentId)
        {
            var list = new List<Grade>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT g.*, c.Title AS CourseTitle
                FROM Grades g
                INNER JOIN Courses c ON c.Id = g.CourseId
                WHERE g.StudentId = $s;";
            cmd.Parameters.AddWithValue("$s", studentId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Grade
                {
                    Id          = r.GetInt32(r.GetOrdinal("Id")),
                    StudentId   = studentId,
                    CourseId    = r.GetInt32(r.GetOrdinal("CourseId")),
                    Marks       = r.GetDouble(r.GetOrdinal("Marks")),
                    TotalMarks  = r.GetDouble(r.GetOrdinal("TotalMarks")),
                    LetterGrade = r.GetString(r.GetOrdinal("LetterGrade")),
                    CourseName  = r.GetString(r.GetOrdinal("CourseTitle")),
                });
            return list;
        }

        /// <summary>Returns GPA on a 4.0 scale for one student.</summary>
        public double GetGPA(int studentId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT g.LetterGrade, c.CreditHours
                FROM Grades g
                INNER JOIN Courses c ON c.Id = g.CourseId
                WHERE g.StudentId = $s;";
            cmd.Parameters.AddWithValue("$s", studentId);
            using var r = cmd.ExecuteReader();

            double totalPoints = 0, totalCredits = 0;
            while (r.Read())
            {
                double pts = Grade.ToGpaPoints(r.GetString(0));
                int    cr  = r.GetInt32(1);
                totalPoints  += pts * cr;
                totalCredits += cr;
            }
            return totalCredits == 0 ? 0 : totalPoints / totalCredits;
        }

        /// <summary>Returns all grades for all students (for admin report).</summary>
        public List<Grade> GetAll()
        {
            var list = new List<Grade>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT g.*, s.Name AS Sname, c.Title AS Ctitle
                FROM Grades g
                INNER JOIN Students s ON s.Id = g.StudentId
                INNER JOIN Courses  c ON c.Id = g.CourseId;";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Grade
                {
                    Id          = r.GetInt32(r.GetOrdinal("Id")),
                    StudentId   = r.GetInt32(r.GetOrdinal("StudentId")),
                    CourseId    = r.GetInt32(r.GetOrdinal("CourseId")),
                    Marks       = r.GetDouble(r.GetOrdinal("Marks")),
                    TotalMarks  = r.GetDouble(r.GetOrdinal("TotalMarks")),
                    LetterGrade = r.GetString(r.GetOrdinal("LetterGrade")),
                    StudentName = r.GetString(r.GetOrdinal("Sname")),
                    CourseName  = r.GetString(r.GetOrdinal("Ctitle")),
                });
            return list;
        }
    }
}
