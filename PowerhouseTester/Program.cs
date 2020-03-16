using System;
using System.Threading.Tasks;
using Powerhouse;

namespace PowerhouseTester
{
    class Program
    {
        static void Main()
        {
            SpreadsheetInfo spreadsheetInfo = new SpreadsheetInfo(@"Powerhouse-50015918ccdf.json", @"14ZJpxhMWP9kmMBmaz-NNdHyZmAtG49qKvrlJMcnfkV0", "A2:E");

            LogItem li = new LogItem()
            {
                OffenderId = 159397733151670272,
                StaffmemberId = 131626628211146752,
                Action = ActionTaken.Warn,
                Reason = "DUMB\nDUMB2",
                Timed = false,
                TimeEnd = 0,
                Ticks = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };

            bool meh = Task.Run(() => spreadsheetInfo.CreateEntryAsync(li, @"A2:G")).Result;


            /*string input = String.Empty; do
            {
                Console.WriteLine("Text String Input:");
                input = Console.ReadLine();

            } while (input.Length > 0);*/
        }
    }
}
