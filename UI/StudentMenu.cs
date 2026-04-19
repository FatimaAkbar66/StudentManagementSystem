// ============================================================
//  UI/StudentMenu.cs
//
//  All student-facing screens.
//  No SQL here — uses repositories for data access.
// ============================================================

using SMS.Helpers;
using SMS.Models;
using SMS.Services;

namespace SMS.UI
{
    public class StudentMenu
    {
        private readonly StudentRepository    _students;
        private readonly CourseRepository     _courses;
        private readonly GradeRepository      _grades;
        private readonly AttendanceRepository _attendance;
        private readonly FeeRepository        _fees;
        private readonly TimetableRepository  _timetable;

        public StudentMenu(
            StudentRepository    students,
            CourseRepository     courses,
            GradeRepository      grades,
            AttendanceRepository attendance,
            FeeRepository        fees,
            TimetableRepository  timetable)
        {
            _students   = students;
            _courses    = courses;
            _grades     = grades;
            _attendance = attendance;
            _fees       = fees;
            _timetable  = timetable;
        }

        // ══════════════════════════════════════════════════
        //  AUTH
        // ══════════════════════════════════════════════════

        public Student? Login()
        {
            ConsoleUI.Header("Student Login");
            string email = ConsoleUI.ReadInput("Email");
            string pass  = ConsoleUI.ReadPassword();
            var    s     = _students.GetByEmail(email);

            if (s != null && s.Password == pass)
            { ConsoleUI.Success($"Welcome back, {s.Name}!"); return s; }

            ConsoleUI.Error("Invalid email or password.");
            return null;
        }

        public void Signup()
        {
            ConsoleUI.Header("Student Signup");
            string email = ConsoleUI.ReadInput("Email");
            if (_students.GetByEmail(email) != null)
            { ConsoleUI.Error("Email already registered. Please login."); return; }

            string pass = ConsoleUI.ReadPassword("Password");
            string conf = ConsoleUI.ReadPassword("Confirm Password");
            if (pass != conf) { ConsoleUI.Error("Passwords do not match."); return; }

            var s = new Student
            {
                Name       = ConsoleUI.ReadInput("Full Name"),
                Email      = email,
                Password   = pass,
                Department = ConsoleUI.ReadInput("Department (CS / EE / ME / BBA)"),
                Semester   = ConsoleUI.ReadInt("Semester", 1, 8),
                Phone      = ConsoleUI.ReadInput("Phone Number"),
            };

            if (_students.Add(s))
            {
                // Lookup newly created record to get auto-generated ID
                var created = _students.GetByEmail(email);
                ConsoleUI.Success($"Registration successful! Your Student ID is: {created?.Id}");
                ConsoleUI.Info("An initial fee record will be created by your department admin.");
            }
            else
            {
                ConsoleUI.Error("Registration failed. Please try again.");
            }
        }

        // ══════════════════════════════════════════════════
        //  MAIN MENU LOOP
        // ══════════════════════════════════════════════════

        public void Run(Student student)
        {
            // Reload student each loop so edits appear immediately
            while (true)
            {
                var s = _students.GetById(student.Id) ?? student;
                Console.Clear();
                ConsoleUI.Header($"Student Portal  ►  {s.Name}  [ID: {s.Id}]");
                Console.WriteLine($@"
  ─── My Profile ───────────────────────────────────
   1.  View Profile         2.  Update Profile
   3.  Change Password

  ─── Academics ────────────────────────────────────
   4.  My Enrolled Courses
   5.  My Grades & GPA
   6.  My Attendance Summary

  ─── Finance & Schedule ───────────────────────────
   7.  My Fee Status
   8.  My Timetable

   0.  Logout
  ──────────────────────────────────────────────────");

                int ch = ConsoleUI.ReadInt("  Choice", 0, 8);
                Console.Clear();

                switch (ch)
                {
                    case 1: ViewProfile(s);        break;
                    case 2: UpdateProfile(s);      break;
                    case 3: ChangePassword(s);     break;
                    case 4: ViewMyCourses(s);      break;
                    case 5: ViewMyGrades(s);       break;
                    case 6: ViewMyAttendance(s);   break;
                    case 7: ViewMyFees(s);         break;
                    case 8: ViewMyTimetable(s);    break;
                    case 0: ConsoleUI.Info("Logged out."); return;
                }
                ConsoleUI.Pause();
            }
        }

        // ══════════════════════════════════════════════════
        //  PROFILE
        // ══════════════════════════════════════════════════

        private void ViewProfile(Student s)
        {
            ConsoleUI.Header("My Profile");
            Console.WriteLine($"\n  {"Student ID",-20}: {s.Id}");
            Console.WriteLine($"  {"Full Name",-20}: {s.Name}");
            Console.WriteLine($"  {"Email",-20}: {s.Email}");
            Console.WriteLine($"  {"Department",-20}: {s.Department}");
            Console.WriteLine($"  {"Semester",-20}: {s.Semester}");
            Console.WriteLine($"  {"Phone",-20}: {s.Phone}");
            Console.WriteLine($"  {"Enrolled On",-20}: {s.EnrolledOn:dd-MMM-yyyy}");
        }

        private void UpdateProfile(Student s)
        {
            ConsoleUI.Header("Update Profile");
            ConsoleUI.Info("Press Enter to keep the current value.");

            string name  = ConsoleUI.ReadInput($"Full Name [{s.Name}]");
            string dept  = ConsoleUI.ReadInput($"Department [{s.Department}]");
            string phone = ConsoleUI.ReadInput($"Phone [{s.Phone}]");
            string semS  = ConsoleUI.ReadInput($"Semester [{s.Semester}]");

            if (!string.IsNullOrEmpty(name))  s.Name       = name;
            if (!string.IsNullOrEmpty(dept))  s.Department = dept;
            if (!string.IsNullOrEmpty(phone)) s.Phone      = phone;
            if (int.TryParse(semS, out int sem) && sem is >= 1 and <= 8) s.Semester = sem;

            _students.Update(s);
            ConsoleUI.Success("Profile updated successfully.");
        }

        private void ChangePassword(Student s)
        {
            ConsoleUI.Header("Change Password");
            string current = ConsoleUI.ReadPassword("Current Password");
            if (current != s.Password)
            { ConsoleUI.Error("Incorrect current password."); return; }

            string newPass = ConsoleUI.ReadPassword("New Password");
            string confirm = ConsoleUI.ReadPassword("Confirm New Password");
            if (newPass != confirm)
            { ConsoleUI.Error("Passwords do not match."); return; }

            s.Password = newPass;
            _students.Update(s);
            ConsoleUI.Success("Password changed successfully.");
        }

        // ══════════════════════════════════════════════════
        //  COURSES
        // ══════════════════════════════════════════════════

        private void ViewMyCourses(Student s)
        {
            ConsoleUI.Header("My Enrolled Courses");
            var courses = _courses.GetCoursesForStudent(s.Id);
            if (!courses.Any())
            { ConsoleUI.Info("You are not enrolled in any courses. Contact your admin."); return; }

            Console.WriteLine($"\n  {"Code",-10} {"Title",-32} {"Instructor",-20} {"CR"}");
            ConsoleUI.Divider();
            courses.ForEach(c =>
                Console.WriteLine($"  {c.Code,-10} {c.Title,-32} {c.Instructor,-20} {c.CreditHours}"));
            Console.WriteLine($"\n  Total: {courses.Count} course(s)");
        }

        // ══════════════════════════════════════════════════
        //  GRADES
        // ══════════════════════════════════════════════════

        private void ViewMyGrades(Student s)
        {
            ConsoleUI.Header("My Grades & GPA");
            var grades = _grades.GetForStudent(s.Id);
            if (!grades.Any())
            { ConsoleUI.Info("No grades available yet."); return; }

            Console.WriteLine($"\n  {"Course",-32} {"Marks",8} {"Grade",7} {"Status",7}");
            ConsoleUI.Divider();
            foreach (var g in grades)
            {
                string status = g.LetterGrade == "F" ? "FAIL" : "PASS";
                Console.ForegroundColor = status == "PASS" ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"  {g.CourseName,-32} {g.Marks,8:F1} {g.LetterGrade,7} {status,7}");
                Console.ResetColor();
            }
            double gpa = _grades.GetGPA(s.Id);
            ConsoleUI.Divider();
            Console.ForegroundColor = gpa >= 3.0 ? ConsoleColor.Green
                : gpa >= 2.0 ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.WriteLine($"  {"CGPA (4.0 Scale):",-32} {gpa,8:F2}");
            Console.ResetColor();
        }

        // ══════════════════════════════════════════════════
        //  ATTENDANCE
        // ══════════════════════════════════════════════════

        private void ViewMyAttendance(Student s)
        {
            ConsoleUI.Header("My Attendance Summary");
            var courses = _courses.GetCoursesForStudent(s.Id);
            if (!courses.Any())
            { ConsoleUI.Info("No enrolled courses."); return; }

            Console.WriteLine($"\n  {"Course",-32} {"Attended",-10} {"Percent",-10} Status");
            ConsoleUI.Divider();

            foreach (var c in courses)
            {
                var (total, present) = _attendance.GetSummary(s.Id, c.Id);
                double pct = total > 0 ? (double)present / total * 100 : 0;
                Console.ForegroundColor = pct >= 75 ? ConsoleColor.Green
                    : pct >= 50 ? ConsoleColor.Yellow : ConsoleColor.Red;
                string warn = pct < 75 ? "⚠ Shortage" : "✓ OK";
                Console.WriteLine($"  {c.Title,-32} {present}/{total,-6}   {pct,-10:F1}% {warn}");
                Console.ResetColor();
            }

            // Detailed log
            ConsoleUI.Divider();
            ConsoleUI.Info("Detailed log:");
            var records = _attendance.GetForStudent(s.Id);
            Console.WriteLine($"\n  {"Course",-28} {"Date",-14} Status");
            ConsoleUI.Divider();
            foreach (var r in records)
            {
                Console.Write($"  {r.CourseName,-28} {r.Date:dd-MMM-yyyy}    ");
                ConsoleUI.PrintStatus(r.Status.ToString());
                Console.WriteLine();
            }
        }

        // ══════════════════════════════════════════════════
        //  FEES
        // ══════════════════════════════════════════════════

        private void ViewMyFees(Student s)
        {
            ConsoleUI.Header("My Fee Status");
            var fees = _fees.GetForStudent(s.Id);
            if (!fees.Any())
            { ConsoleUI.Info("No fee records found. Contact the admin."); return; }

            foreach (var f in fees)
            {
                ConsoleUI.Divider();
                Console.WriteLine($"  Semester    : {f.Semester}");
                Console.WriteLine($"  Total Fee   : Rs. {f.TotalAmount:N0}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  Paid        : Rs. {f.PaidAmount:N0}");
                Console.ForegroundColor = f.Balance > 0 ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine($"  Balance     : Rs. {f.Balance:N0}");
                Console.ResetColor();
                Console.Write($"  Status      : ");
                ConsoleUI.PrintFeeStatus(f.Status.ToString());
                Console.WriteLine();
                Console.WriteLine($"  Due Date    : {f.DueDate:dd-MMM-yyyy}");
            }
            ConsoleUI.Divider();
        }

        // ══════════════════════════════════════════════════
        //  TIMETABLE
        // ══════════════════════════════════════════════════

        private void ViewMyTimetable(Student s)
        {
            ConsoleUI.Header("My Timetable");
            var slots = _timetable.GetForStudent(s.Id);
            if (!slots.Any())
            { ConsoleUI.Info("No timetable found for your enrolled courses."); return; }

            Console.WriteLine($"\n  {"Day",-12} {"Course",-28} {"Code",-9} {"Start",-8} {"End",-8} {"Room"}");
            ConsoleUI.Divider();
            foreach (var t in slots)
                Console.WriteLine($"  {t.Day,-12} {t.CourseTitle,-28} {t.CourseCode,-9} {t.StartTime,-8} {t.EndTime,-8} {t.Room}");
        }
    }
}
