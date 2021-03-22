using System.Collections.Generic;

namespace Risk.Shared
{
    public interface IPlayer
    {
        public string Name { get; }
        public string Token { get; }
        public int Strikes { get; set; }
        public List<int> TerritoryCards { get; set; }
    }
}