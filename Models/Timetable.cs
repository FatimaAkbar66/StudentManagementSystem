// ============================================================
//  Models/Timetable.cs
// ============================================================

namespace SMS.Models
{
    public class Timetable
    {
        public int    Id        { get; set; }
        public int    CourseId  { get; set; }
        public string Day       { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime   { get; set; } = "";
        public string Room      { get; set; } = "";

        // Navigation helper
        public string CourseTitle { get; set; } = "";
        public string CourseCode  { get; set; } = "";
    }
}
