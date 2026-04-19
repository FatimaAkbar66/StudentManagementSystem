// ============================================================
//  Models/Enrollment.cs
//  Junction table linking Students ↔ Courses
// ============================================================

namespace SMS.Models
{
    public class Enrollment
    {
        public int    Id         { get; set; }
        public int    StudentId  { get; set; }
        public int    CourseId   { get; set; }
        public DateTime EnrolledOn { get; set; } = DateTime.Now;

        // Navigation helpers
        public string StudentName { get; set; } = "";
        public string CourseTitle { get; set; } = "";
    }
}
