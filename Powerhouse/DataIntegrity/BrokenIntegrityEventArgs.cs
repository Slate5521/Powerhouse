using System;
using System.Collections.Generic;
using System.Text;

namespace Powerhouse.DataIntegrity
{
    public class BrokenIntegrityEventArgs
    {
        public long Timestamp { get; }
        public char Column { get; }
        public int Row { get; }
        public string Info { get; }

        public BrokenIntegrityEventArgs(long timestamp, char column, int row, string info)
        {
            Timestamp = timestamp;
            Column = column;
            Row = row;
            Info = info;
        } 
    }
}
