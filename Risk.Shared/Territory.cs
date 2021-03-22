using System;
using System.Text.Json.Serialization;

namespace Risk.Shared
{
    public class Territory
    {
        public Territory()
        {

        }

        public Territory(Location location)
        {
            Location = location;
        }

        public Location Location { get; set; }

        [JsonIgnore]
        public IPlayer Owner { get; set; }

        public int Armies { get; set; }

        public override string ToString() => $"{Location}: {Armies:n0} of {Owner?.Name ?? "(Unoccupied)"}";
    }
}
