// ============================================================
//  Helpers/ConsoleUI.cs
//
//  Centralises all console I/O: coloured headers, prompts,
//  password masking, dividers, pause, etc.
//  Every other file calls these — nothing calls Console directly.
// ============================================================

namespace SMS.Helpers
{
    public static class ConsoleUI
    {
        // ── Banner / header ─────────────────────────────────

        public static void Banner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════════════╗
  ║        STUDENT  MANAGEMENT  SYSTEM                   ║
  ║        C# Console  |  SQLite Local Database          ║
  ╠══════════════════════════════════════════════════════╣
  ║   1.  Admin Login          2.  Admin Signup          ║
  ║   3.  Student Login        4.  Student Signup        ║
  ║   0.  Exit                                           ║
  ╚══════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        public static void Header(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔══════════════════════════════════════════════╗");
            Console.WriteLine($" ║  {title.PadRight(44)}");                     ║
            Console.WriteLine("  ╚══════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        // ── Feedback messages ───────────────────────────────

        public static void Success(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓  {msg}");
            Console.ResetColor();
        }

        public static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗  {msg}");
            Console.ResetColor();
        }

        public static void Info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ►  {msg}");
            Console.ResetColor();
        }

        public static void Divider() =>
            Console.WriteLine("  ────────────────────────────────────────────────");

        // ── Input helpers ───────────────────────────────────

        public static string ReadInput(string prompt)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {prompt}: ");
            Console.ResetColor();
            return Console.ReadLine()?.Trim() ?? "";
        }

        /// <summary>Masks password with asterisks while typing.</summary>
        public static string ReadPassword(string prompt = "Password")
        {
            Console.Write($"  {prompt}: ");
            string pass = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass[..^1];
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return pass;
        }

        /// <summary>Keeps re-asking until an integer in [min,max] is entered.</summary>
        public static int ReadInt(string prompt, int min = 0, int max = int.MaxValue)
        {
            while (true)
            {
                string raw = ReadInput(prompt);
                if (int.TryParse(raw, out int val) && val >= min && val <= max)
                    return val;
                Error($"Please enter a whole number between {min} and {max}.");
            }
        }

        /// <summary>Keeps re-asking until a non-negative double is entered.</summary>
        public static double ReadDouble(string prompt)
        {
            while (true)
            {
                string raw = ReadInput(prompt);
                if (double.TryParse(raw, out double val) && val >= 0)
                    return val;
                Error("Please enter a valid positive number.");
            }
        }

        /// <summary>Waits for any key before continuing.</summary>
        public static void Pause()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n  Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        /// <summary>Prints a colour-coded attendance status.</summary>
        public static void PrintStatus(string status)
        {
            Console.ForegroundColor = status switch
            {
                "Present" => ConsoleColor.Green,
                "Leave"   => ConsoleColor.Yellow,
                _         => ConsoleColor.Red
            };
            Console.Write(status);
            Console.ResetColor();
        }

        /// <summary>Prints a colour-coded fee status.</summary>
        public static void PrintFeeStatus(string status)
        {
            Console.ForegroundColor = status switch
            {
                "Paid"    => ConsoleColor.Green,
                "Partial" => ConsoleColor.Yellow,
                _         => ConsoleColor.Red
            };
            Console.Write(status);
            Console.ResetColor();
        }
    }
}
