// ============================================================
//  Models/Student.cs
//  Represents a student entity stored in the database.
// ============================================================

namespace SMS.Models
{
    public class Student
    {
        public int      Id         { get; set; }
        public string   Name       { get; set; } = "";
        public string   Email      { get; set; } = "";
        public string   Password   { get; set; } = "";   // plain-text for learning; hash in production
        public string   Department { get; set; } = "";
        public int      Semester   { get; set; }
        public string   Phone      { get; set; } = "";
        public DateTime DateOfBirth{ get; set; }
        public DateTime EnrolledOn { get; set; } = DateTime.Now;

        public override string ToString() =>
            $"[{Id}]  {Name,-22}  {Department,-6}  Sem {Semester}  {Email}";
    }
}
