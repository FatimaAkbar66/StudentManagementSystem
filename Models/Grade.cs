// ============================================================
//  Models/Grade.cs
// ============================================================

namespace SMS.Models
{
    public class Grade
    {
        public int    Id          { get; set; }
        public int    StudentId   { get; set; }
        public int    CourseId    { get; set; }
        public double Marks       { get; set; }
        public double TotalMarks  { get; set; } = 100;
        public string LetterGrade { get; set; } = "";

        // Navigation helpers (filled at query time, not stored as columns)
        public string CourseName  { get; set; } = "";
        public string StudentName { get; set; } = "";

        /// <summary>Converts percentage to a letter grade.</summary>
        public static string ToLetter(double percentage) => percentage switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B",
            >= 60 => "C",
            >= 50 => "D",
            _     => "F"
        };

        /// <summary>Converts letter grade to 4.0 GPA points.</summary>
        public static double ToGpaPoints(string letter) => letter switch
        {
            "A+" or "A" => 4.0,
            "B"         => 3.0,
            "C"         => 2.0,
            "D"         => 1.0,
            _           => 0.0
        };
    }
}
