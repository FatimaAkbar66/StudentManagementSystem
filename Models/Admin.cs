// ============================================================
//  Models/Admin.cs
//  Represents an admin account.
// ============================================================

namespace SMS.Models
{
    public class Admin
    {
        public int    Id       { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
