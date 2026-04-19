// ============================================================
//  Models/Course.cs
// ============================================================

namespace SMS.Models
{
    public class Course
    {
        public int    Id          { get; set; }
        public string Code        { get; set; } = "";
        public string Title       { get; set; } = "";
        public string Instructor  { get; set; } = "";
        public int    CreditHours { get; set; }

        public override string ToString() =>
            $"[{Id}]  {Code,-8}  {Title,-30}  {Instructor,-18}  {CreditHours} CR";
    }
}
