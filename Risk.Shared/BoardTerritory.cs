using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class BoardTerritory
    {
        public Location Location { get; set; }
        public string OwnerName { get; set; }
        public int Armies { get; set; }
        public override string ToString() => $"{Location}: {Armies:n0} of {OwnerName ?? "(Unoccupied)"}";
    }
}
