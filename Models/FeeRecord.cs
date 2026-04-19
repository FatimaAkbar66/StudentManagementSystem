// ============================================================
//  Models/FeeRecord.cs
// ============================================================

namespace SMS.Models
{
    public enum FeeStatus { Unpaid, Partial, Paid }

    public class FeeRecord
    {
        public int       Id          { get; set; }
        public int       StudentId   { get; set; }
        public string    Semester    { get; set; } = "";
        public double    TotalAmount { get; set; }
        public double    PaidAmount  { get; set; }
        public FeeStatus Status      { get; set; }
        public DateTime  DueDate     { get; set; }

        // Computed — not stored
        public double Balance   => TotalAmount - PaidAmount;
        public string StudentName { get; set; } = "";
    }
}
