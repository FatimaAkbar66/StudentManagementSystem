// ============================================================
//  Data/DatabaseHelper.cs
//
//  Manages the SQLite connection and creates all tables
//  on first run.  The .db file lives next to the .exe so
//  Visual Studio finds it automatically — no server needed.
// ============================================================

using Microsoft.Data.Sqlite;

namespace SMS.Data
{
    public static class DatabaseHelper
    {
        // Database file path — created automatically next to the exe
        private static readonly string DbPath =
    Environment.GetEnvironmentVariable("DB_PATH")
    ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sms.db");

        // Connection string
        public static string ConnectionString =>
            $"Data Source={DbPath}";

        /// <summary>
        /// Opens and returns a ready-to-use SQLite connection.
        /// Caller is responsible for disposing it (use 'using').
        /// </summary>
        public static SqliteConnection GetConnection()
        {
            var dir = Path.GetDirectoryName(DbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Creates all tables if they don't exist yet.
        /// Called once at application startup.
        /// </summary>
        public static void InitializeDatabase()
        {
            using var conn = GetConnection();

            // Enable foreign key enforcement in SQLite
            ExecuteNonQuery(conn, "PRAGMA foreign_keys = ON;");

            // ── Admins ─────────────────────────────────────
            ExecuteNonQuery(conn, @"
                CREATE TABLE IF NOT EXISTS Admins (
                    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL
                );");

            // ── Students ───────────────────────────────────
            ExecuteNonQuery(conn, @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name        TEXT    NOT NULL,
                    Email       TEXT    NOT NULL UNIQUE,
                    Password    TEXT    NOT NULL,
                    Department  TEXT    NOT NULL,
                    Semester    INTEGER NOT NULL,
                    Phone       TEXT,
                    DateOfBirth TEXT,
                    EnrolledOn  TEXT    NOT NULL
                );");

            // ── Courses ────────────────────────────────────
            ExecuteNonQuery(conn, @"
                CREATE TABLE IF NOT EXISTS Courses (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Code        TEXT    NOT NULL UNIQUE,
                    Title       TEXT    NOT NULL,
                    Instructor  TEXT    NOT NULL,
                    CreditHours INTEGER NOT NULL
                );");

            // ── Enrollments (Student ↔ Course junction) ───
            ExecuteNonQuery(conn, @"
                CREATE TABLE IF NOT EXISTS Enrollments (
                    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId  INTEGER NOT NULL REFERENCES Students(Id) ON DELETE CASCADE,
                    CourseId   INTEGER NOT NULL REFERENCES Courses(Id)  ON DELETE CASCADE,
                    EnrolledOn TEXT    NOT NULL,
                    UNIQUE(StudentId, CourseId)
                );");

            // ── Grades ─────────────────────────────────────
            ExecuteNonQuery(conn, @"
                CREATE TABLE IF NOT EXISTS Grades (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId   INTEGER NOT NULL REFERENCES Students(Id) ON DELETE CASCADE,
                    CourseId    INTEGER NOT NULL REFERENCES Courses(Id)  ON DELETE CASCADE,
                    Marks       REAL    NOT NULL,
                    TotalMarks  REAL    NOT NULL DEFAULT 100,
                    LetterGrade TEXT    NOT NULL,
                    UNIQUE(StudentId, CourseId)
                );");

            // ── Attendance ─────────────────────────────────
            ExecuteNonQuery(conn, @"
                CREATE TABLE IF NOT EXISTS Attendance (
                    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL REFERENCES Students(Id) ON DELETE CASCADE,
                    CourseId  INTEGER NOT NULL REFERENCES Courses(Id)  ON DELETE CASCADE,
                    Date      TEXT    NOT NULL,
                    Status    TEXT    NOT NULL,
                    UNIQUE(StudentId, CourseId, Date)
                );");

            // ── Fees ───────────────────────────────────────
            ExecuteNonQuery(conn, @"
                CREATE TABLE IF NOT EXISTS Fees (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId   INTEGER NOT NULL REFERENCES Students(Id) ON DELETE CASCADE,
                    Semester    TEXT    NOT NULL,
                    TotalAmount REAL    NOT NULL,
                    PaidAmount  REAL    NOT NULL DEFAULT 0,
                    Status      TEXT    NOT NULL DEFAULT 'Unpaid',
                    DueDate     TEXT    NOT NULL
                );");

            // ── Timetable ──────────────────────────────────
            ExecuteNonQuery(conn, @"
                CREATE TABLE IF NOT EXISTS Timetable (
                    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    CourseId  INTEGER NOT NULL REFERENCES Courses(Id) ON DELETE CASCADE,
                    Day       TEXT    NOT NULL,
                    StartTime TEXT    NOT NULL,
                    EndTime   TEXT    NOT NULL,
                    Room      TEXT    NOT NULL
                );");

            // ── Seed default admin if none exists ──────────
            SeedDefaultAdmin(conn);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  [DB] Database ready: {DbPath}");
            Console.ResetColor();
        }

        private static void SeedDefaultAdmin(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Admins;";
            long count = (long)(cmd.ExecuteScalar() ?? 0L);
            if (count == 0)
            {
                ExecuteNonQuery(conn,
                    "INSERT INTO Admins (Username, Password) VALUES ('admin', 'admin123');");
            }
        }

        /// <summary>Convenience wrapper for commands with no return value.</summary>
        public static void ExecuteNonQuery(SqliteConnection conn, string sql,
            Action<SqliteCommand>? paramBuilder = null)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            paramBuilder?.Invoke(cmd);
            cmd.ExecuteNonQuery();
        }
    }
}
