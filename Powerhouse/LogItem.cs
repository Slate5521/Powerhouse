using System;
using System.Collections.Generic;
using System.Text;

namespace Powerhouse
{
    public class LogItem
    {
        private const string GUID_PREFIX = "RW";
        
        /// <summary>When the log item occurred.</summary>
        public readonly long Seconds;
        /// <summary>Guid of the log item.</summary>
        public readonly string Guid;
        public ulong OffenderId;
        public ulong StaffmemberId;
        /// <summary>Time the punishment ends. Defaults to -1 for no time to end.</summary>
        public long Duration;
        public string Reason;
        public ActionTaken Action;

        public bool Timed
        {
            get => Duration > 0;
        }

        public LogItem(long seconds, ulong offenderId, ulong staffmemberId, ActionTaken action, long duration = 0, string reason = "", string guid = "")
        {
            Seconds = seconds;
            OffenderId = offenderId;
            StaffmemberId = staffmemberId;
            Action = action;
            Duration = duration;
            Reason = reason;

            if (guid.Length == 0)
                Guid = GUID_PREFIX + System.Guid.NewGuid().ToString("N");
            else
                Guid = guid;
        }

        internal static LogItem GenerateLogItemFromIList(IList<object> values)
        {
            LogItem logItem;

            // Set to true if there has been an error in the parsing process.
            bool err = false;

            long seconds; // Seconds the warning was logged at
            string guid; // Guid of the warning
            string reason; // Reason of the warning

                // Try to parse the guid.
                if (values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.GuidColumn)].ToString().Length > 0)
                    guid = values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.GuidColumn)].ToString();
                else
                {
                    err = true;
                    guid = default;
                }

            // Try to parse the date.
            if (!err && DateTimeOffset.TryParse(values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.TimestampColumn)].ToString(), out DateTimeOffset dto))
                seconds = dto.ToUnixTimeSeconds();
            else
            {
                err = true;
                seconds = default;
            }

            // Try to parse the offender's ID.
            if (!err && ulong.TryParse(values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.OffenderColumn)].ToString().Substring(1), out ulong offenderId));
            else
            {
                err = true;
                offenderId = default;
            }

            // Try to parse the action taken.
            if (!err && Enum.TryParse(values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.ActionColumn)].ToString(), out ActionTaken action));
            else
            {
                err = true;
                action = default;
            }

            // Try to parse the staffmember's id.
            if (!err && ulong.TryParse(values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.StaffColumn)].ToString().Substring(1), out ulong staffmemberId));
            else
            {
                err = true;
                staffmemberId = default;
            }

            // Try to parse the duration of the warn.
            if (!err && long.TryParse(values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.TimeColumn)].ToString().Substring(1), out long duration));
            else
            {
                err = true;
                duration = default;
            }

            // Try to parse the reason.
            if (!err && values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.ReasonColumn)].ToString().Length > 0)
                reason = values[SpreadsheetInfo.CTZBI(SpreadsheetInfo.ReasonColumn)].ToString();
            else
            {
                err = true;
                reason = default;
            }

            // If this scope is entered, there were no errors.
            if (!err)
            {
                logItem = new LogItem(
                    seconds: seconds,
                    offenderId: offenderId,
                    staffmemberId: staffmemberId,
                    action: action,
                    duration: duration,
                    reason: reason,
                    guid: guid);
            }
            else logItem = default; // err was true.
            
            return logItem;
        }

        /// <summary>Gets the remaining time before the action expires.</summary>
        /// <param name="currTime">Current time in milliseconds.</param>
        /// <returns>The remaining time in milliseconds, -1 if this action is not timed.</returns>
        public long RemainingTicks(long currTime)
        {
            long returnVal;
            if (Timed)
                returnVal = Math.Max(Seconds + Duration - currTime, 0);
            else returnVal = -1;

            return returnVal;
        }

        public string GetTimestamp() => DateTimeOffset.FromUnixTimeSeconds(Seconds).ToString();
        internal List<object> ToValueRange(LogItem logItem)
        {
            return new List<object>
                {
                    Guid,
                    logItem.GetTimestamp(),
                    '`' + logItem.OffenderId.ToString(),
                    logItem.Action.ToString(),
                    '`' + logItem.StaffmemberId.ToString(),
                    logItem.Timed ? ('`' + logItem.Duration.ToString()) : "`-1",
                    logItem.Reason,
                };

        }

        public override bool Equals(object obj)
        {
            return obj is LogItem item &&
                   Seconds == item.Seconds &&
                   Guid == item.Guid &&
                   OffenderId == item.OffenderId &&
                   StaffmemberId == item.StaffmemberId &&
                   Duration == item.Duration &&
                   Reason == item.Reason &&
                   Action == item.Action;
        }

        public override int GetHashCode()
        {
            var hashCode = -908413599;
            hashCode = hashCode * -1521134295 + Seconds.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Guid);
            hashCode = hashCode * -1521134295 + OffenderId.GetHashCode();
            hashCode = hashCode * -1521134295 + StaffmemberId.GetHashCode();
            hashCode = hashCode * -1521134295 + Duration.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Reason);
            hashCode = hashCode * -1521134295 + Action.GetHashCode();
            return hashCode;
        }
    }
}
