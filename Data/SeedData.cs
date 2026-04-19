// ============================================================
//  Data/SeedData.cs
//
//  Inserts sample students, courses, grades, attendance,
//  fees and timetable rows so the app is usable on first run.
//  Only runs when the Students table is empty.
// ============================================================

using Microsoft.Data.Sqlite;
using SMS.Models;

namespace SMS.Data
{
    public static class SeedData
    {
        public static void Run()
        {
            using var conn = DatabaseHelper.GetConnection();

            // Skip if data already exists
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM Students;";
            long count = (long)(check.ExecuteScalar() ?? 0L);
            if (count > 0) return;

            // ── Sample students ──────────────────────────
            InsertStudent(conn, "Ali Hassan",   "ali@uni.edu",   "pass123", "CS", 3, "0301-1111111");
            InsertStudent(conn, "Sara Khan",    "sara@uni.edu",  "pass456", "EE", 5, "0302-2222222");
            InsertStudent(conn, "Bilal Ahmed",  "bilal@uni.edu", "pass789", "CS", 1, "0333-3333333");
            InsertStudent(conn, "Ayesha Noor",  "ayesha@uni.edu","pass000", "BBA",2, "0321-4444444");

            // ── Sample courses ───────────────────────────
            InsertCourse(conn, "CS101", "Intro to Programming",  "Dr. Raza",    3);
            InsertCourse(conn, "CS201", "Data Structures",       "Dr. Fatima",  3);
            InsertCourse(conn, "EE101", "Circuit Analysis",      "Prof. Tariq", 4);
            InsertCourse(conn, "MTH101","Calculus",              "Dr. Aslam",   3);
            InsertCourse(conn, "BBA101","Principles of Mgmt.",   "Ms. Zara",    3);

            // ── Enrollments ──────────────────────────────
            //  Ali (1): CS101, CS201, MTH101
            //  Sara(2): EE101, MTH101
            //  Bilal(3): CS101, MTH101
            //  Ayesha(4): BBA101, MTH101
            int[][] enrollments =
            {
                new[]{1,1}, new[]{1,2}, new[]{1,4},
                new[]{2,3}, new[]{2,4},
                new[]{3,1}, new[]{3,4},
                new[]{4,5}, new[]{4,4},
            };
            foreach (var e in enrollments)
                InsertEnrollment(conn, e[0], e[1]);

            // ── Grades ───────────────────────────────────
            InsertGrade(conn, 1, 1, 85);
            InsertGrade(conn, 1, 2, 72);
            InsertGrade(conn, 1, 4, 91);
            InsertGrade(conn, 2, 3, 65);
            InsertGrade(conn, 2, 4, 78);
            InsertGrade(conn, 3, 1, 55);
            InsertGrade(conn, 3, 4, 88);
            InsertGrade(conn, 4, 5, 80);
            InsertGrade(conn, 4, 4, 74);

            // ── Attendance ───────────────────────────────
            var today = DateTime.Today;
            InsertAttendance(conn, 1, 1, today.AddDays(-2), AttendanceStatus.Present);
            InsertAttendance(conn, 1, 1, today.AddDays(-1), AttendanceStatus.Absent);
            InsertAttendance(conn, 1, 1, today,             AttendanceStatus.Present);
            InsertAttendance(conn, 2, 3, today.AddDays(-2), AttendanceStatus.Present);
            InsertAttendance(conn, 2, 3, today.AddDays(-1), AttendanceStatus.Leave);
            InsertAttendance(conn, 3, 1, today.AddDays(-1), AttendanceStatus.Present);
            InsertAttendance(conn, 3, 1, today,             AttendanceStatus.Present);
            InsertAttendance(conn, 4, 5, today.AddDays(-1), AttendanceStatus.Present);

            // ── Fees ─────────────────────────────────────
            InsertFee(conn, 1, "Fall 2024", 50000, 50000, FeeStatus.Paid);
            InsertFee(conn, 2, "Fall 2024", 50000, 25000, FeeStatus.Partial);
            InsertFee(conn, 3, "Fall 2024", 50000, 0,     FeeStatus.Unpaid);
            InsertFee(conn, 4, "Fall 2024", 50000, 50000, FeeStatus.Paid);

            // ── Timetable ────────────────────────────────
            InsertSlot(conn, 1, "Monday",    "08:00", "09:30", "LH-1");
            InsertSlot(conn, 1, "Wednesday", "08:00", "09:30", "LH-1");
            InsertSlot(conn, 2, "Tuesday",   "10:00", "11:30", "CS-Lab");
            InsertSlot(conn, 3, "Thursday",  "14:00", "15:30", "EE-Lab");
            InsertSlot(conn, 4, "Friday",    "09:00", "10:30", "LH-3");
            InsertSlot(conn, 5, "Monday",    "11:00", "12:30", "BBA-Hall");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [DB] Sample data seeded.");
            Console.ResetColor();
        }

        // ── Private insert helpers ───────────────────────────

        private static void InsertStudent(SqliteConnection c,
            string name, string email, string pwd, string dept, int sem, string phone)
        {
            DatabaseHelper.ExecuteNonQuery(c, @"
                INSERT INTO Students (Name,Email,Password,Department,Semester,Phone,EnrolledOn)
                VALUES ($n,$e,$p,$d,$s,$ph,$en);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$n",  name);
                    cmd.Parameters.AddWithValue("$e",  email);
                    cmd.Parameters.AddWithValue("$p",  pwd);
                    cmd.Parameters.AddWithValue("$d",  dept);
                    cmd.Parameters.AddWithValue("$s",  sem);
                    cmd.Parameters.AddWithValue("$ph", phone);
                    cmd.Parameters.AddWithValue("$en", DateTime.Now.ToString("o"));
                });
        }

        private static void InsertCourse(SqliteConnection c,
            string code, string title, string instr, int cr)
        {
            DatabaseHelper.ExecuteNonQuery(c, @"
                INSERT INTO Courses (Code,Title,Instructor,CreditHours)
                VALUES ($co,$t,$i,$cr);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$co", code);
                    cmd.Parameters.AddWithValue("$t",  title);
                    cmd.Parameters.AddWithValue("$i",  instr);
                    cmd.Parameters.AddWithValue("$cr", cr);
                });
        }

        private static void InsertEnrollment(SqliteConnection c, int sid, int cid)
        {
            DatabaseHelper.ExecuteNonQuery(c, @"
                INSERT OR IGNORE INTO Enrollments (StudentId,CourseId,EnrolledOn)
                VALUES ($s,$c,$e);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$s", sid);
                    cmd.Parameters.AddWithValue("$c", cid);
                    cmd.Parameters.AddWithValue("$e", DateTime.Now.ToString("o"));
                });
        }

        private static void InsertGrade(SqliteConnection c, int sid, int cid, double marks)
        {
            string letter = Models.Grade.ToLetter(marks);
            DatabaseHelper.ExecuteNonQuery(c, @"
                INSERT OR IGNORE INTO Grades (StudentId,CourseId,Marks,TotalMarks,LetterGrade)
                VALUES ($s,$c,$m,100,$l);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$s", sid);
                    cmd.Parameters.AddWithValue("$c", cid);
                    cmd.Parameters.AddWithValue("$m", marks);
                    cmd.Parameters.AddWithValue("$l", letter);
                });
        }

        private static void InsertAttendance(SqliteConnection c,
            int sid, int cid, DateTime date, AttendanceStatus status)
        {
            DatabaseHelper.ExecuteNonQuery(c, @"
                INSERT OR IGNORE INTO Attendance (StudentId,CourseId,Date,Status)
                VALUES ($s,$c,$d,$st);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$s",  sid);
                    cmd.Parameters.AddWithValue("$c",  cid);
                    cmd.Parameters.AddWithValue("$d",  date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("$st", status.ToString());
                });
        }

        private static void InsertFee(SqliteConnection c,
            int sid, string sem, double total, double paid, FeeStatus status)
        {
            DatabaseHelper.ExecuteNonQuery(c, @"
                INSERT INTO Fees (StudentId,Semester,TotalAmount,PaidAmount,Status,DueDate)
                VALUES ($s,$sem,$t,$p,$st,$d);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$s",   sid);
                    cmd.Parameters.AddWithValue("$sem", sem);
                    cmd.Parameters.AddWithValue("$t",   total);
                    cmd.Parameters.AddWithValue("$p",   paid);
                    cmd.Parameters.AddWithValue("$st",  status.ToString());
                    cmd.Parameters.AddWithValue("$d",   DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd"));
                });
        }

        private static void InsertSlot(SqliteConnection c,
            int cid, string day, string start, string end, string room)
        {
            DatabaseHelper.ExecuteNonQuery(c, @"
                INSERT INTO Timetable (CourseId,Day,StartTime,EndTime,Room)
                VALUES ($c,$d,$s,$e,$r);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$c", cid);
                    cmd.Parameters.AddWithValue("$d", day);
                    cmd.Parameters.AddWithValue("$s", start);
                    cmd.Parameters.AddWithValue("$e", end);
                    cmd.Parameters.AddWithValue("$r", room);
                });
        }
    }
}
