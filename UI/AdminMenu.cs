// ============================================================
//  UI/AdminMenu.cs
//
//  All admin screens.  Each method handles one menu action.
//  Repositories are injected via the constructor so this
//  class only does UI — no SQL lives here.
// ============================================================

using SMS.Helpers;
using SMS.Models;
using SMS.Services;

namespace SMS.UI
{
    public class AdminMenu
    {
        private readonly AdminRepository      _admins;
        private readonly StudentRepository    _students;
        private readonly CourseRepository     _courses;
        private readonly GradeRepository      _grades;
        private readonly AttendanceRepository _attendance;
        private readonly FeeRepository        _fees;
        private readonly TimetableRepository  _timetable;

        public AdminMenu(
            AdminRepository      admins,
            StudentRepository    students,
            CourseRepository     courses,
            GradeRepository      grades,
            AttendanceRepository attendance,
            FeeRepository        fees,
            TimetableRepository  timetable)
        {
            _admins     = admins;
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

        public Admin? Login()
        {
            ConsoleUI.Header("Admin Login");
            string user = ConsoleUI.ReadInput("Username");
            string pass = ConsoleUI.ReadPassword();
            var admin   = _admins.GetByCredentials(user, pass);
            if (admin != null) { ConsoleUI.Success("Login successful!"); return admin; }
            ConsoleUI.Error("Invalid username or password.");
            return null;
        }

        public void Signup()
        {
            ConsoleUI.Header("Admin Signup");
            string key = ConsoleUI.ReadPassword("Admin secret key");
            if (key != "ADMIN2024")
            { ConsoleUI.Error("Wrong secret key. Contact the system administrator."); return; }

            string user = ConsoleUI.ReadInput("New username");
            if (_admins.UsernameExists(user))
            { ConsoleUI.Error("Username already taken."); return; }

            string pass = ConsoleUI.ReadPassword("Password");
            string conf = ConsoleUI.ReadPassword("Confirm password");
            if (pass != conf) { ConsoleUI.Error("Passwords do not match."); return; }

            _admins.Add(new Admin { Username = user, Password = pass });
            ConsoleUI.Success($"Admin account '{user}' created.");
        }

        // ══════════════════════════════════════════════════
        //  MAIN MENU LOOP
        // ══════════════════════════════════════════════════

        public void Run(Admin admin)
        {
            while (true)
            {
                Console.Clear();
                ConsoleUI.Header($"Admin Panel  ►  {admin.Username}");
                Console.WriteLine(@"
  ─── Student Management ───────────────────────────
   1.  Add Student          2.  View All Students
   3.  Edit Student         4.  Delete Student
   5.  Search Student

  ─── Course Management ────────────────────────────
   6.  Add Course           7.  View All Courses
   8.  Enroll Student       9.  Delete Course
  10.  View Enrolled Students

  ─── Academics ────────────────────────────────────
  11.  Assign / Update Grade
  12.  View Student Grades
  13.  Mark Attendance
  14.  View Attendance Report

  ─── Finance & Operations ─────────────────────────
  15.  Add Fee Record       16.  Record Fee Payment
  17.  View All Fees        18.  Fee Summary Report
  19.  Manage Timetable     20.  Generate Reports

   0.  Logout
  ──────────────────────────────────────────────────");

                int ch = ConsoleUI.ReadInt("  Choice", 0, 20);
                Console.Clear();

                switch (ch)
                {
                    case  1: AddStudent();           break;
                    case  2: ViewAllStudents();      break;
                    case  3: EditStudent();          break;
                    case  4: DeleteStudent();        break;
                    case  5: SearchStudent();        break;
                    case  6: AddCourse();            break;
                    case  7: ViewAllCourses();       break;
                    case  8: EnrollStudent();        break;
                    case  9: DeleteCourse();         break;
                    case 10: ViewEnrolledStudents(); break;
                    case 11: AssignGrade();          break;
                    case 12: ViewStudentGrades();    break;
                    case 13: MarkAttendance();       break;
                    case 14: ViewAttendanceReport(); break;
                    case 15: AddFeeRecord();         break;
                    case 16: RecordPayment();        break;
                    case 17: ViewAllFees();          break;
                    case 18: FeeSummaryReport();     break;
                    case 19: ManageTimetable();      break;
                    case 20: GenerateReports();      break;
                    case  0: ConsoleUI.Info("Logged out."); return;
                }
                ConsoleUI.Pause();
            }
        }

        // ══════════════════════════════════════════════════
        //  STUDENT MANAGEMENT
        // ══════════════════════════════════════════════════

        private void AddStudent()
        {
            ConsoleUI.Header("Add New Student");
            var s = new Student
            {
                Name       = ConsoleUI.ReadInput("Full Name"),
                Email      = ConsoleUI.ReadInput("Email"),
                Password   = ConsoleUI.ReadPassword("Initial Password"),
                Department = ConsoleUI.ReadInput("Department (CS / EE / ME / BBA)"),
                Semester   = ConsoleUI.ReadInt("Semester", 1, 8),
                Phone      = ConsoleUI.ReadInput("Phone"),
            };
            if (_students.Add(s))
                ConsoleUI.Success($"Student '{s.Name}' added successfully.");
            else
                ConsoleUI.Error("Email already exists. Use a different email.");
        }

        private void ViewAllStudents()
        {
            ConsoleUI.Header("All Students");
            var list = _students.GetAll();
            if (!list.Any()) { ConsoleUI.Info("No students found."); return; }

            Console.WriteLine($"\n  {"ID",-6} {"Name",-22} {"Dept",-6} {"Sem",-4} {"Email",-28} {"Phone"}");
            ConsoleUI.Divider();
            foreach (var s in list)
                Console.WriteLine($"  {s.Id,-6} {s.Name,-22} {s.Department,-6} {s.Semester,-4} {s.Email,-28} {s.Phone}");
            Console.WriteLine($"\n  Total: {list.Count} student(s)");
        }

        private void EditStudent()
        {
            ConsoleUI.Header("Edit Student");
            ViewAllStudents();
            int id  = ConsoleUI.ReadInt("\n  Enter Student ID");
            var s   = _students.GetById(id);
            if (s == null) { ConsoleUI.Error("Student not found."); return; }

            ConsoleUI.Info("Leave blank to keep the current value.");
            string name  = ConsoleUI.ReadInput($"Name [{s.Name}]");
            string dept  = ConsoleUI.ReadInput($"Department [{s.Department}]");
            string phone = ConsoleUI.ReadInput($"Phone [{s.Phone}]");
            string semS  = ConsoleUI.ReadInput($"Semester [{s.Semester}]");

            if (!string.IsNullOrEmpty(name))  s.Name       = name;
            if (!string.IsNullOrEmpty(dept))  s.Department = dept;
            if (!string.IsNullOrEmpty(phone)) s.Phone      = phone;
            if (int.TryParse(semS, out int sem) && sem is >= 1 and <= 8) s.Semester = sem;

            _students.Update(s);
            ConsoleUI.Success("Student record updated.");
        }

        private void DeleteStudent()
        {
            ConsoleUI.Header("Delete Student");
            ViewAllStudents();
            int id  = ConsoleUI.ReadInt("\n  Enter Student ID to delete");
            var s   = _students.GetById(id);
            if (s == null) { ConsoleUI.Error("Student not found."); return; }

            ConsoleUI.Info($"You are about to delete: {s.Name}");
            string confirm = ConsoleUI.ReadInput($"Type the student's name to confirm");
            if (confirm != s.Name) { ConsoleUI.Info("Deletion cancelled."); return; }

            _students.Delete(id);
            ConsoleUI.Success($"Student '{s.Name}' and all related records deleted.");
        }

        private void SearchStudent()
        {
            ConsoleUI.Header("Search Student");
            string keyword = ConsoleUI.ReadInput("Enter name or email keyword");
            var results    = _students.Search(keyword);
            if (!results.Any()) { ConsoleUI.Error("No matching students found."); return; }
            ConsoleUI.Divider();
            results.ForEach(s => Console.WriteLine($"  {s}"));
        }

        // ══════════════════════════════════════════════════
        //  COURSE MANAGEMENT
        // ══════════════════════════════════════════════════

        private void AddCourse()
        {
            ConsoleUI.Header("Add Course");
            var c = new Course
            {
                Code        = ConsoleUI.ReadInput("Course Code (e.g. CS301)"),
                Title       = ConsoleUI.ReadInput("Course Title"),
                Instructor  = ConsoleUI.ReadInput("Instructor Name"),
                CreditHours = ConsoleUI.ReadInt("Credit Hours", 1, 6),
            };
            if (_courses.Add(c))
                ConsoleUI.Success($"Course '{c.Code} — {c.Title}' added.");
            else
                ConsoleUI.Error("Course code already exists.");
        }

        private void ViewAllCourses()
        {
            ConsoleUI.Header("All Courses");
            var list = _courses.GetAll();
            if (!list.Any()) { ConsoleUI.Info("No courses found."); return; }

            Console.WriteLine($"\n  {"ID",-5} {"Code",-9} {"Title",-30} {"Instructor",-20} {"CR",-4} {"Enrolled"}");
            ConsoleUI.Divider();
            foreach (var c in list)
            {
                int enrolled = _courses.GetEnrolledCount(c.Id);
                Console.WriteLine($"  {c.Id,-5} {c.Code,-9} {c.Title,-30} {c.Instructor,-20} {c.CreditHours,-4} {enrolled}");
            }
        }

        private void EnrollStudent()
        {
            ConsoleUI.Header("Enroll Student in Course");
            ViewAllStudents();
            int sid = ConsoleUI.ReadInt("\n  Student ID");
            var s   = _students.GetById(sid);
            if (s == null) { ConsoleUI.Error("Student not found."); return; }

            Console.Clear();
            ViewAllCourses();
            int cid = ConsoleUI.ReadInt("\n  Course ID");
            var c   = _courses.GetById(cid);
            if (c == null) { ConsoleUI.Error("Course not found."); return; }

            if (_courses.EnrollStudent(sid, cid))
                ConsoleUI.Success($"{s.Name} enrolled in '{c.Title}'.");
            else
                ConsoleUI.Error("Student is already enrolled in this course.");
        }

        private void DeleteCourse()
        {
            ConsoleUI.Header("Delete Course");
            ViewAllCourses();
            int id  = ConsoleUI.ReadInt("\n  Course ID to delete");
            var c   = _courses.GetById(id);
            if (c == null) { ConsoleUI.Error("Course not found."); return; }

            string confirm = ConsoleUI.ReadInput($"Type '{c.Code}' to confirm deletion");
            if (confirm != c.Code) { ConsoleUI.Info("Deletion cancelled."); return; }

            _courses.Delete(id);
            ConsoleUI.Success($"Course '{c.Code}' and all related records deleted.");
        }

        private void ViewEnrolledStudents()
        {
            ConsoleUI.Header("Students Enrolled in a Course");
            ViewAllCourses();
            int cid     = ConsoleUI.ReadInt("\n  Course ID");
            var course  = _courses.GetById(cid);
            if (course == null) { ConsoleUI.Error("Course not found."); return; }

            var students = _courses.GetStudentsInCourse(cid);
            Console.WriteLine($"\n  Students in: {course.Code} — {course.Title}");
            ConsoleUI.Divider();
            if (!students.Any()) { ConsoleUI.Info("No students enrolled."); return; }
            students.ForEach(s => Console.WriteLine($"  {s}"));
        }

        // ══════════════════════════════════════════════════
        //  GRADES
        // ══════════════════════════════════════════════════

        private void AssignGrade()
        {
            ConsoleUI.Header("Assign / Update Grade");
            ViewAllStudents();
            int sid = ConsoleUI.ReadInt("\n  Student ID");
            var s   = _students.GetById(sid);
            if (s == null) { ConsoleUI.Error("Student not found."); return; }

            var myCourses = _courses.GetCoursesForStudent(sid);
            if (!myCourses.Any()) { ConsoleUI.Error("Student is not enrolled in any courses."); return; }

            Console.WriteLine($"\n  Courses enrolled by {s.Name}:");
            myCourses.ForEach(c => Console.WriteLine($"  {c}"));

            int cid = ConsoleUI.ReadInt("\n  Course ID");
            var c2  = _courses.GetById(cid);
            if (c2 == null || !_courses.IsEnrolled(sid, cid))
            { ConsoleUI.Error("Invalid course or student not enrolled."); return; }

            double marks  = ConsoleUI.ReadDouble($"  Marks obtained (out of 100)");
            string letter = Grade.ToLetter((marks / 100) * 100);

            _grades.Upsert(new Grade
            {
                StudentId   = sid,
                CourseId    = cid,
                Marks       = marks,
                TotalMarks  = 100,
                LetterGrade = letter,
            });
            ConsoleUI.Success($"Grade saved →  {letter}  ({marks}/100)");
        }

        private void ViewStudentGrades()
        {
            ConsoleUI.Header("View Student Grades");
            ViewAllStudents();
            int sid = ConsoleUI.ReadInt("\n  Student ID");
            var s   = _students.GetById(sid);
            if (s == null) { ConsoleUI.Error("Student not found."); return; }

            var grades = _grades.GetForStudent(sid);
            Console.WriteLine($"\n  Grades for: {s.Name}  ({s.Department}, Sem {s.Semester})");
            ConsoleUI.Divider();
            if (!grades.Any()) { ConsoleUI.Info("No grades recorded yet."); return; }

            Console.WriteLine($"  {"Course",-30} {"Marks",8} {"Grade",7} {"Status",8}");
            ConsoleUI.Divider();
            foreach (var g in grades)
            {
                string status = g.LetterGrade == "F" ? "FAIL" : "PASS";
                Console.ForegroundColor = status == "PASS" ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"  {g.CourseName,-30} {g.Marks,8:F1} {g.LetterGrade,7} {status,8}");
                Console.ResetColor();
            }
            double gpa = _grades.GetGPA(sid);
            ConsoleUI.Divider();
            Console.WriteLine($"  {"Cumulative GPA (4.0 Scale):",-30} {gpa,8:F2}");
        }

        // ══════════════════════════════════════════════════
        //  ATTENDANCE
        // ══════════════════════════════════════════════════

        private void MarkAttendance()
        {
            ConsoleUI.Header("Mark Attendance");
            ViewAllCourses();
            int cid    = ConsoleUI.ReadInt("\n  Course ID");
            var course = _courses.GetById(cid);
            if (course == null) { ConsoleUI.Error("Course not found."); return; }

            var students = _courses.GetStudentsInCourse(cid);
            if (!students.Any()) { ConsoleUI.Info("No students enrolled."); return; }

            Console.WriteLine($"\n  Marking attendance for: {course.Code} — {course.Title}");
            Console.WriteLine($"  Date: {DateTime.Today:dd-MMM-yyyy}");
            ConsoleUI.Divider();
            Console.WriteLine("  [P] Present   [A] Absent   [L] Leave\n");

            foreach (var s in students)
            {
                Console.Write($"  {s.Name,-24} → ");
                string input  = (Console.ReadLine() ?? "A").Trim().ToUpper();
                var status    = input switch
                {
                    "P" => AttendanceStatus.Present,
                    "L" => AttendanceStatus.Leave,
                    _   => AttendanceStatus.Absent
                };
                _attendance.MarkOrUpdate(new Attendance
                { StudentId = s.Id, CourseId = cid, Date = DateTime.Today, Status = status });
            }
            ConsoleUI.Success("Attendance saved to database.");
        }

        private void ViewAttendanceReport()
        {
            ConsoleUI.Header("Attendance Report");
            ViewAllStudents();
            int sid = ConsoleUI.ReadInt("\n  Student ID");
            var s   = _students.GetById(sid);
            if (s == null) { ConsoleUI.Error("Student not found."); return; }

            var courses = _courses.GetCoursesForStudent(sid);
            Console.WriteLine($"\n  Attendance for: {s.Name}");
            ConsoleUI.Divider();

            foreach (var c in courses)
            {
                var (total, present) = _attendance.GetSummary(sid, c.Id);
                double pct = total > 0 ? (double)present / total * 100 : 0;
                Console.ForegroundColor = pct >= 75 ? ConsoleColor.Green
                    : pct >= 50 ? ConsoleColor.Yellow : ConsoleColor.Red;
                Console.WriteLine($"  {c.Title,-32} {present}/{total} classes  ({pct:F1}%)" +
                    (pct < 75 ? "  ⚠ Shortage" : "  ✓ OK"));
                Console.ResetColor();
            }

            // Detailed log
            ConsoleUI.Divider();
            var records = _attendance.GetForStudent(sid);
            Console.WriteLine($"\n  {"Course",-28} {"Date",-14} Status");
            ConsoleUI.Divider();
            foreach (var r in records)
            {
                Console.Write($"  {r.CourseName,-28} {r.Date:dd-MMM-yyyy,-14} ");
                ConsoleUI.PrintStatus(r.Status.ToString());
                Console.WriteLine();
            }
        }

        // ══════════════════════════════════════════════════
        //  FEES
        // ══════════════════════════════════════════════════

        private void AddFeeRecord()
        {
            ConsoleUI.Header("Add Fee Record");
            ViewAllStudents();
            int sid = ConsoleUI.ReadInt("\n  Student ID");
            if (_students.GetById(sid) == null)
            { ConsoleUI.Error("Student not found."); return; }

            var fee = new FeeRecord
            {
                StudentId   = sid,
                Semester    = ConsoleUI.ReadInput("Semester (e.g. Spring 2025)"),
                TotalAmount = ConsoleUI.ReadDouble("  Total Fee Amount (Rs.)"),
                PaidAmount  = 0,
                Status      = FeeStatus.Unpaid,
                DueDate     = DateTime.Now.AddMonths(1),
            };
            _fees.Add(fee);
            ConsoleUI.Success("Fee record added.");
        }

        private void RecordPayment()
        {
            ConsoleUI.Header("Record Fee Payment");
            ViewAllStudents();
            int sid     = ConsoleUI.ReadInt("\n  Student ID");
            var student = _students.GetById(sid);
            if (student == null) { ConsoleUI.Error("Student not found."); return; }

            var feeList = _fees.GetForStudent(sid);
            var unpaid  = feeList.Where(f => f.Status != FeeStatus.Paid).ToList();
            if (!unpaid.Any()) { ConsoleUI.Info("No outstanding fees."); return; }

            Console.WriteLine($"\n  Outstanding Fees for {student.Name}:");
            foreach (var f in unpaid)
                Console.WriteLine($"  [{f.Id}]  {f.Semester}  Balance: Rs. {f.Balance:N0}  ({f.Status})");

            int feeId = ConsoleUI.ReadInt("\n  Fee Record ID");
            var fee   = unpaid.FirstOrDefault(f => f.Id == feeId);
            if (fee == null) { ConsoleUI.Error("Fee record not found."); return; }

            double amount = ConsoleUI.ReadDouble($"  Amount to pay (Balance: Rs. {fee.Balance:N0})");
            if (amount > fee.Balance) { ConsoleUI.Error("Amount exceeds balance."); return; }

            double newPaid  = fee.PaidAmount + amount;
            var newStatus   = newPaid >= fee.TotalAmount ? FeeStatus.Paid : FeeStatus.Partial;
            _fees.UpdatePayment(feeId, newPaid, newStatus);
            ConsoleUI.Success($"Payment of Rs. {amount:N0} recorded. New balance: Rs. {fee.Balance - amount:N0}");
        }

        private void ViewAllFees()
        {
            ConsoleUI.Header("All Fee Records");
            var list = _fees.GetAll();
            if (!list.Any()) { ConsoleUI.Info("No fee records found."); return; }

            Console.WriteLine($"\n  {"Student",-22} {"Semester",-14} {"Total",10} {"Paid",10} {"Balance",10}  Status");
            ConsoleUI.Divider();
            foreach (var f in list)
            {
                Console.Write($"  {f.StudentName,-22} {f.Semester,-14} {f.TotalAmount,10:N0} {f.PaidAmount,10:N0} {f.Balance,10:N0}  ");
                ConsoleUI.PrintFeeStatus(f.Status.ToString());
                Console.WriteLine();
            }
        }

        private void FeeSummaryReport()
        {
            ConsoleUI.Header("Fee Summary Report");
            var (total, paid, balance) = _fees.GetSummary();
            var all     = _fees.GetAll();
            int paidC   = all.Count(f => f.Status == FeeStatus.Paid);
            int partC   = all.Count(f => f.Status == FeeStatus.Partial);
            int unpaidC = all.Count(f => f.Status == FeeStatus.Unpaid);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n  Total Billed    :  Rs. {total:N0}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Total Collected :  Rs. {paid:N0}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Total Pending   :  Rs. {balance:N0}");
            Console.ResetColor();
            ConsoleUI.Divider();
            Console.WriteLine($"  Fully Paid   : {paidC} record(s)");
            Console.WriteLine($"  Partial      : {partC} record(s)");
            Console.WriteLine($"  Unpaid       : {unpaidC} record(s)");
        }

        // ══════════════════════════════════════════════════
        //  TIMETABLE
        // ══════════════════════════════════════════════════

        private void ManageTimetable()
        {
            ConsoleUI.Header("Timetable Management");
            Console.WriteLine("  1. View All Timetable Slots");
            Console.WriteLine("  2. Add Timetable Slot");
            Console.WriteLine("  3. Delete Timetable Slot");
            Console.WriteLine("  0. Back");
            int ch = ConsoleUI.ReadInt("\n  Choice", 0, 3);

            if (ch == 1)
            {
                PrintTimetable(_timetable.GetAll());
            }
            else if (ch == 2)
            {
                ViewAllCourses();
                int cid = ConsoleUI.ReadInt("\n  Course ID");
                if (_courses.GetById(cid) == null)
                { ConsoleUI.Error("Course not found."); return; }

                _timetable.Add(new Timetable
                {
                    CourseId  = cid,
                    Day       = ConsoleUI.ReadInput("Day (Monday/Tuesday/Wednesday/Thursday/Friday)"),
                    StartTime = ConsoleUI.ReadInput("Start Time (HH:MM, e.g. 08:00)"),
                    EndTime   = ConsoleUI.ReadInput("End Time   (HH:MM, e.g. 09:30)"),
                    Room      = ConsoleUI.ReadInput("Room / Lab"),
                });
                ConsoleUI.Success("Timetable slot added.");
            }
            else if (ch == 3)
            {
                PrintTimetable(_timetable.GetAll());
                int id = ConsoleUI.ReadInt("\n  Slot ID to delete");
                _timetable.Delete(id);
                ConsoleUI.Success("Slot deleted.");
            }
        }

        private static void PrintTimetable(List<Timetable> slots)
        {
            if (!slots.Any()) { ConsoleUI.Info("No timetable slots found."); return; }
            Console.WriteLine($"\n  {"ID",-5} {"Day",-12} {"Course",-28} {"Start",-8} {"End",-8} {"Room"}");
            ConsoleUI.Divider();
            foreach (var t in slots)
                Console.WriteLine($"  {t.Id,-5} {t.Day,-12} {t.CourseTitle,-28} {t.StartTime,-8} {t.EndTime,-8} {t.Room}");
        }

        // ══════════════════════════════════════════════════
        //  REPORTS
        // ══════════════════════════════════════════════════

        private void GenerateReports()
        {
            ConsoleUI.Header("Reports");
            Console.WriteLine("  1. Student Summary by Department");
            Console.WriteLine("  2. Course Enrollment Report");
            Console.WriteLine("  3. Top Performing Students");
            Console.WriteLine("  4. Students with Attendance Shortage");
            Console.WriteLine("  0. Back");
            int ch = ConsoleUI.ReadInt("\n  Choice", 0, 4);
            Console.Clear();

            switch (ch)
            {
                case 1: StudentSummaryReport();       break;
                case 2: CourseEnrollmentReport();     break;
                case 3: TopPerformers();              break;
                case 4: AttendanceShortageReport();   break;
            }
        }

        private void StudentSummaryReport()
        {
            ConsoleUI.Header("Student Summary by Department");
            var all    = _students.GetAll();
            var byDept = all.GroupBy(s => s.Department).OrderBy(g => g.Key);
            Console.WriteLine($"\n  Total Students: {all.Count}");
            ConsoleUI.Divider();
            foreach (var d in byDept)
                Console.WriteLine($"  {d.Key,-10}  {d.Count()} student(s)");
        }

        private void CourseEnrollmentReport()
        {
            ConsoleUI.Header("Course Enrollment Report");
            var courses = _courses.GetAll();
            Console.WriteLine($"\n  {"Course",-32} {"Instructor",-20} {"Enrolled"}");
            ConsoleUI.Divider();
            foreach (var c in courses.OrderByDescending(c => _courses.GetEnrolledCount(c.Id)))
            {
                int count = _courses.GetEnrolledCount(c.Id);
                Console.WriteLine($"  {c.Title,-32} {c.Instructor,-20} {count}");
            }
        }

        private void TopPerformers()
        {
            ConsoleUI.Header("Top 5 Performers by GPA");
            var students = _students.GetAll();
            var ranked   = students
                .Select(s => new { s, gpa = _grades.GetGPA(s.Id) })
                .OrderByDescending(x => x.gpa)
                .Take(5).ToList();

            int rank = 1;
            Console.WriteLine($"\n  {"Rank",-6} {"Name",-24} {"Dept",-8} {"GPA"}");
            ConsoleUI.Divider();
            foreach (var p in ranked)
            {
                Console.ForegroundColor = rank == 1 ? ConsoleColor.Yellow : ConsoleColor.White;
                Console.WriteLine($"  #{rank,-5} {p.s.Name,-24} {p.s.Department,-8} {p.gpa:F2}");
                Console.ResetColor();
                rank++;
            }
        }

        private void AttendanceShortageReport()
        {
            ConsoleUI.Header("Students with Attendance Shortage (<75%)");
            var students = _students.GetAll();
            bool found   = false;

            Console.WriteLine($"\n  {"Student",-24} {"Course",-28} {"Attended",-10} {"Percent"}");
            ConsoleUI.Divider();

            foreach (var s in students)
            {
                var courses = _courses.GetCoursesForStudent(s.Id);
                foreach (var c in courses)
                {
                    var (total, present) = _attendance.GetSummary(s.Id, c.Id);
                    if (total == 0) continue;
                    double pct = (double)present / total * 100;
                    if (pct < 75)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  {s.Name,-24} {c.Title,-28} {present}/{total,-6}   {pct:F1}%");
                        Console.ResetColor();
                        found = true;
                    }
                }
            }
            if (!found) ConsoleUI.Success("No attendance shortage found. All students are above 75%.");
        }
    }
}
