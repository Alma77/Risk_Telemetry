using System;
using System.Collections;

namespace Risk.Shared
{
    public class Location
    {
        public Location() { }

        public Location(int row, int col)
        {
            Row = row;
            Column = col;
        }

        /// <summary>
        /// Starting from the bottom, row 0 is the bottom row
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Starting from the left.  Column 0 is the far left column.
        /// </summary>
        public int Column { get; set; }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                Location l => l.Row == Row && l.Column == Column,
                _ => base.Equals(obj)
            };
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator== (Location l1, Location l2)
        {
            return l1?.Equals(l2) ?? false;
        }

        public static bool operator!= (Location l1, Location l2)
        {
            return l1?.Equals(l2) is false;
        }

        public override string ToString()
        {
            return $"({Row}, {Column})";
        }
    }
}