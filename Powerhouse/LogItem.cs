using System;
using System.Collections.Generic;
using System.Text;

namespace Powerhouse
{
    public struct LogItem
    {
        public long Ticks;
        public ulong OffenderId;
        public ulong StaffmemberId;
        public ulong TimeEnd;
        public string Reason;
        public ActionTaken Action;
        public bool Timed;
        public string Timestamp
        {
            get
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(Ticks).ToString();
            }
        }
    }
}
