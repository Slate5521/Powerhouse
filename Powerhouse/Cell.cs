using System;
using System.Collections.Generic;
using System.Text;

namespace Powerhouse
{
    public struct Cell
    {
        public char Column;
        public int Row;
        public string Location
        {
            get => Column + Row.ToString();
        }

        public static Cell InvalidCell
            = new Cell
            {
                Column = '\0',
                Row = 0
            };

        internal static Cell ZeroBasedRowToCell(char column, int index)
        {
            return new Cell
            {
                Column = column,
                Row = index + 2,
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Cell cell &&
                   Column == cell.Column &&
                   Row == cell.Row;
        }

        public override int GetHashCode()
        {
            var hashCode = 656739706;
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            hashCode = hashCode * -1521134295 + Row.GetHashCode();
            return hashCode;
        }
    }
}
