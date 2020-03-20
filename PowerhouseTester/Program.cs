using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Powerhouse;

namespace PowerhouseTester
{
    class Program
    {
        static Regex DateRegex
            = new Regex(@"(\d+)\s?(months?|days?|weeks?|wks?|hours?|hrs?|minutes?|mins?)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        static void Main()
        {
            BotSpreadsheetWrapper wrapper = 
                new BotSpreadsheetWrapper("Powerhouse-50015918ccdf.json", "14ZJpxhMWP9kmMBmaz-NNdHyZmAtG49qKvrlJMcnfkV0", "A2:G");

            //bool successfulAdd = wrapper.AddWarn(DateTimeOffset.UtcNow, 146669176738676736, 131626628211146752, "idiot").Result;
            bool successfulUpdate = wrapper.UpdateEntry("RW4af7c8031c7e490eaf97e44a5540a9a9", 1337, "dogshit").Result;
            //Cell foundVal = wrapper.QueryGuid("value3im2lok").Result;

            //LogItem[] whatever = wrapper.QueryPendingActions().Result;
            //bool whatever = wrapper.DeleteEntry("RW2dbd9c676497418f870390329d05e211").Result;
            Console.Read();

            string input = String.Empty; do
            {
                string cmd;

                if (input.Length > "warn".Length && input.Substring(0, 4).Equals("warn"))
                    cmd = "warn";
                else if (input.Length > "mute".Length && input.Substring(0, 4).Equals("mute"))
                    cmd = "mute";
                else if (input.Length > "kick".Length && input.Substring(0, 4).Equals("kick"))
                    cmd = "kick";
                else if (input.Length > "ban".Length && input.Substring(0, 3).Equals("ban"))
                    cmd = "ban";
                else cmd = String.Empty;

                ActionTaken action;

                switch(cmd)
                {
                    case "warn":
                        action = ActionTaken.Warn;

                        goto case "finalLabel";
                    case "mute":
                        action = ActionTaken.Mute;

                        goto case "finalLabel";
                    case "kick":
                        action = ActionTaken.Kick;

                        goto case "finalLabel";
                    case "ban":
                        action = ActionTaken.Ban;

                        goto case "finalLabel";
                    case "finalLabel":

                        // Process the arguments

                        break;
                    default: // Regular text
                        break;
                }

            } while (input.Length > 0);
        }
    }
}