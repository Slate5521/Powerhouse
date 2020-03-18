using System;
using System.Collections.Generic;
using System.Text;

namespace Powerhouse
{
    public class LogItem
    {
        private const string GUID_PREFIX = "RW";
        
        /// <summary>When the log item occurred.</summary>
        public readonly long Ticks;
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

        public LogItem(long ticks, ulong offenderId, ulong staffmemberId, ActionTaken action, long duration = 0, string reason = "")
        {
            Ticks = ticks;
            OffenderId = offenderId;
            StaffmemberId = staffmemberId;
            Action = action;
            Duration = duration;
            Reason = reason;

            Guid = GUID_PREFIX + System.Guid.NewGuid().ToString("N");
        }

        internal static LogItem GenerateLogItemFromIList(IList<object> values)
        {
            LogItem logItem;

            // Set to true if there has been an error in the parsing process.
            bool err = false;

            long ticks; // Ticks (ms) the warning was logged at
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
                ticks = dto.ToUnixTimeMilliseconds();
            else
            {
                err = true;
                ticks = default;
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
                    ticks: ticks,
                    offenderId: offenderId,
                    staffmemberId: staffmemberId,
                    action: action,
                    duration: duration,
                    reason: reason);
            }
            else logItem = default; // err was true.
            
            return logItem;
        }

        public string GetTimestamp() => DateTimeOffset.FromUnixTimeMilliseconds(Ticks).ToString();
        internal List<object> ToValueRange(LogItem logItem)
        {
            return new List<object>
                {
                    Guid,
                    logItem.GetTimestamp(),
                    '`' + logItem.OffenderId.ToString(),
                    logItem.Action.ToString(),
                    '`' + logItem.StaffmemberId.ToString(),
                    logItem.Timed ? ('`' + logItem.Duration.ToString()) : "`n/a",
                    logItem.Reason,
                };

        }
    }
}
