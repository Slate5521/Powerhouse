using System;
using System.Collections.Generic;
using System.Text;

namespace Powerhouse
{
    internal struct LogItem
    {
        public long Ticks;
        public ulong OffenderId;
        public ulong StaffmemberId;
        public long TimeEnd;
        public string Reason;
        public ActionTaken Action;
        public string Guid;

        public bool Timed
        {
            get => TimeEnd > 0;
        }
        internal string GetTimestamp() => DateTimeOffset.FromUnixTimeMilliseconds(Ticks).ToString();
        internal List<object> ToValueRange(LogItem logItem)
        {
            return new List<object>
                {
                    Guid,
                    logItem.GetTimestamp(),
                    '"' + logItem.OffenderId.ToString() + '"',
                    logItem.Action.ToString(),
                    '"' + logItem.StaffmemberId.ToString() + '"',
                    logItem.Timed ? ('"' + logItem.TimeEnd.ToString() + '"') : "\"n/a\"",
                    logItem.Reason,
                };

        }
    }
}
