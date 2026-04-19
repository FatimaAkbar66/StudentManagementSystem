// ============================================================
//  Services/FeeRepository.cs
// ============================================================

using Microsoft.Data.Sqlite;
using SMS.Data;
using SMS.Models;

namespace SMS.Services
{
    public class FeeRepository
    {
        public void Add(FeeRecord f)
        {
            using var conn = DatabaseHelper.GetConnection();
            DatabaseHelper.ExecuteNonQuery(conn, @"
                INSERT INTO Fees (StudentId,Semester,TotalAmount,PaidAmount,Status,DueDate)
                VALUES ($s,$sem,$t,$p,$st,$d);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$s",   f.StudentId);
                    cmd.Parameters.AddWithValue("$sem", f.Semester);
                    cmd.Parameters.AddWithValue("$t",   f.TotalAmount);
                    cmd.Parameters.AddWithValue("$p",   f.PaidAmount);
                    cmd.Parameters.AddWithValue("$st",  f.Status.ToString());
                    cmd.Parameters.AddWithValue("$d",   f.DueDate.ToString("yyyy-MM-dd"));
                });
        }

        public void UpdatePayment(int feeId, double newPaid, FeeStatus newStatus)
        {
            using var conn = DatabaseHelper.GetConnection();
            DatabaseHelper.ExecuteNonQuery(conn, @"
                UPDATE Fees SET PaidAmount=$p, Status=$st WHERE Id=$id;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("$p",  newPaid);
                    cmd.Parameters.AddWithValue("$st", newStatus.ToString());
                    cmd.Parameters.AddWithValue("$id", feeId);
                });
        }

        public List<FeeRecord> GetForStudent(int studentId)
        {
            var list = new List<FeeRecord>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Fees WHERE StudentId=$s ORDER BY DueDate;";
            cmd.Parameters.AddWithValue("$s", studentId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public List<FeeRecord> GetAll()
        {
            var list = new List<FeeRecord>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT f.*, s.Name AS SName FROM Fees f
                INNER JOIN Students s ON s.Id = f.StudentId
                ORDER BY f.DueDate;";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var fee = Map(r);
                fee.StudentName = r.GetString(r.GetOrdinal("SName"));
                list.Add(fee);
            }
            return list;
        }

        // ── Summary for report ──────────────────────────────

        public (double total, double paid, double balance) GetSummary()
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    SUM(TotalAmount) AS T,
                    SUM(PaidAmount)  AS P
                FROM Fees;";
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                double t = r.IsDBNull(0) ? 0 : r.GetDouble(0);
                double p = r.IsDBNull(1) ? 0 : r.GetDouble(1);
                return (t, p, t - p);
            }
            return (0, 0, 0);
        }

        // ── Mapper ──────────────────────────────────────────

        private static FeeRecord Map(SqliteDataReader r) => new()
        {
            Id          = r.GetInt32(r.GetOrdinal("Id")),
            StudentId   = r.GetInt32(r.GetOrdinal("StudentId")),
            Semester    = r.GetString(r.GetOrdinal("Semester")),
            TotalAmount = r.GetDouble(r.GetOrdinal("TotalAmount")),
            PaidAmount  = r.GetDouble(r.GetOrdinal("PaidAmount")),
            Status      = Enum.Parse<FeeStatus>(r.GetString(r.GetOrdinal("Status"))),
            DueDate     = DateTime.Parse(r.GetString(r.GetOrdinal("DueDate"))),
        };
    }
}
