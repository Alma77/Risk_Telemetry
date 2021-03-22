using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Shared
{
    public class Player : IPlayer
    {
        public Player(string conId, string nam)
        {
            this.Name = nam;
            Token = conId;
            TerritoryCards = new List<int>();
            Strikes = 0;
        }

        public string Name { get; set; }
        public string Token { get; set; }
        public List<int> TerritoryCards { get; set; }
        public int Strikes { get; set; }
    }
}
