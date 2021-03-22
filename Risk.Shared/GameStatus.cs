using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Risk.Shared
{
    public class GameStatus
    {
        public IEnumerable<string> Players { get; set; }
        public Collection<PlayerStats> PlayerStats { get; set; }
        public int StartingArmies { get; set; }
        public GameState GameState { get; set; }
        public IEnumerable<BoardTerritory> Board { get; set; }
        public string CurrentPlayer { get; set; }

        public GameStatus()
        {
            Players = new List<string>();
            PlayerStats = new Collection<PlayerStats>();
        }

        public GameStatus(IEnumerable<string> players, GameState gameState, IEnumerable<BoardTerritory> board, IEnumerable<PlayerStats> playerStats, int startingArmies )
        {
            Players = players;
            GameState = gameState;
            Board = board;
            PlayerStats = new Collection<PlayerStats>(playerStats.ToList());
            StartingArmies = startingArmies;
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            foreach(var stat in PlayerStats)
            {
                s.AppendLine(stat.ToString());
            }
            return s.ToString();
        }
    }

    public class PlayerStats
    {
        public string Name { get; set; }
        public int Strikes { get; set; }
        public int Armies { get; set; }
        public int Territories { get; set; }
        public int Score { get; set; }

        public override string ToString()
        {
            return $"{Name,-20} Strikes: {Strikes,3} Armies: {Armies,3} Territories: {Territories,3} Score: {Score,3}";
        }
    }
}
