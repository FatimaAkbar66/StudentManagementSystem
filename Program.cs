// ============================================================
//  Program.cs  —  Application Entry Point
//
//  This file:
//    1. Initialises the SQLite database (creates tables)
//    2. Seeds sample data on first run
//    3. Creates repository instances (our "services")
//    4. Injects them into the UI menus
//    5. Runs the main menu loop
//
//  Think of this as the "wiring" file — it connects every
//  other part of the project together.
// ============================================================

using SMS.Data;
using SMS.Helpers;
using SMS.Services;
using SMS.UI;

// ── 1. Database setup ────────────────────────────────────────
DatabaseHelper.InitializeDatabase();   // creates sms.db + all tables
SeedData.Run();                        // inserts sample data (once only)

// ── 2. Create repositories ───────────────────────────────────
//  Each repository handles one "table group" in the database.
var adminRepo      = new AdminRepository();
var studentRepo    = new StudentRepository();
var courseRepo     = new CourseRepository();
var gradeRepo      = new GradeRepository();
var attendanceRepo = new AttendanceRepository();
var feeRepo        = new FeeRepository();
var timetableRepo  = new TimetableRepository();

// ── 3. Create menus (inject repositories) ────────────────────
var adminMenu = new AdminMenu(
    adminRepo, studentRepo, courseRepo,
    gradeRepo, attendanceRepo, feeRepo, timetableRepo);

var studentMenu = new StudentMenu(
    studentRepo, courseRepo, gradeRepo,
    attendanceRepo, feeRepo, timetableRepo);

// ── 4. Main loop ─────────────────────────────────────────────
while (true)
{
    ConsoleUI.Banner();

    int choice = ConsoleUI.ReadInt("\n  Select option", 0, 4);
    Console.Clear();

    switch (choice)
    {
        case 1:   // Admin Login
            var admin = adminMenu.Login();
            if (admin != null) adminMenu.Run(admin);
            break;

        case 2:   // Admin Signup
            adminMenu.Signup();
            ConsoleUI.Pause();
            break;

        case 3:   // Student Login
            var student = studentMenu.Login();
            if (student != null) studentMenu.Run(student);
            break;

        case 4:   // Student Signup
            studentMenu.Signup();
            ConsoleUI.Pause();
            break;

        case 0:   // Exit
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Thank you for using the Student Management System. Goodbye!\n");
            Console.ResetColor();
            return;
    }
}
