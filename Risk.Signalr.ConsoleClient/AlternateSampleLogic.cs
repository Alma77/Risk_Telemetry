using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Risk.Signalr.ConsoleClient
{
    public class AlternateSampleLogic : IPlayerLogic
    {
        public AlternateSampleLogic(string playerName)
        {
            MyPlayerName = playerName;
        }

        public string MyPlayerName { get; set; }
        private Random rng = new Random();

        public Location WhereDoYouWantToDeploy(IEnumerable<BoardTerritory> board)
        {
            var openOrMine = (from cell in board
                              where cell.OwnerName == null || cell.OwnerName == MyPlayerName
                              select cell).ToArray();
            return openOrMine[rng.Next(0,openOrMine.Length)].Location;
        }

        public (Location from, Location to) WhereDoYouWantToAttack(IEnumerable<BoardTerritory> board)
        {
            var attacks = from cell in board
                          where cell.OwnerName == MyPlayerName && cell.Armies > 1
                          let neighbors = GetNeighbors(cell, board)
                          let enemies = neighbors.OrderBy(n => n.Armies).Where(n => n.OwnerName != MyPlayerName)
                          where enemies.Any()
                          let weakestEnemy = enemies.First()
                          let delta = cell.Armies - weakestEnemy.Armies
                          orderby delta descending
                          select new { Me = cell, Them = weakestEnemy };
            var attack = attacks.First();

            if(attack.Them != null)
                return (attack.Me.Location, attack.Them.Location);
            else
                throw new Exception("Unable to find place to attack");
        }

        private IEnumerable<BoardTerritory> GetNeighbors(BoardTerritory territory, IEnumerable<BoardTerritory> board)
        {
            var l = territory.Location;
            var neighborLocations = new[] {
                new Location(l.Row+1, l.Column-1),
                new Location(l.Row+1, l.Column),
                new Location(l.Row+1, l.Column+1),
                new Location(l.Row, l.Column-1),
                new Location(l.Row, l.Column+1),
                new Location(l.Row-1, l.Column-1),
                new Location(l.Row-1, l.Column),
                new Location(l.Row-1, l.Column+1),
            };
            return board.Where(t => neighborLocations.Contains(t.Location));
        }
    }
}
