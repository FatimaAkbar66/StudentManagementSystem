// ============================================================
//  Models/Attendance.cs
// ============================================================

namespace SMS.Models
{
    public enum AttendanceStatus { Present, Absent, Leave }

    public class Attendance
    {
        public int              Id         { get; set; }
        public int              StudentId  { get; set; }
        public int              CourseId   { get; set; }
        public DateTime         Date       { get; set; }
        public AttendanceStatus Status     { get; set; }

        // Navigation helpers
        public string CourseName  { get; set; } = "";
        public string StudentName { get; set; } = "";
    }
}
