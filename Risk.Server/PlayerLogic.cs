using Microsoft.Extensions.Configuration;
using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Server
{
    public class PlayerLogic
    {

        public PlayerLogic(string playerName)
        {
            MyPlayerName = playerName;
        }

        public string MyPlayerName { get; set; }

        public Location WhereDoYouWantToDeploy(IEnumerable<BoardTerritory> board)
        {
            var myTerritory = board.FirstOrDefault(t => t.OwnerName == MyPlayerName) ?? board.Skip(board.Count() / 2).First(t => t.OwnerName == null);
            return myTerritory.Location;
        }

        public (Location to, Location from) WhereDoYouWantToAttack(IEnumerable<BoardTerritory> board)
        {
            foreach (var myTerritory in board.Where(t => t.OwnerName == MyPlayerName).OrderByDescending(t => t.Armies))
            {
                var myNeighbors = GetNeighbors(myTerritory, board);
                var destination = myNeighbors.Where(t => t.OwnerName != MyPlayerName).OrderBy(t => t.Armies).FirstOrDefault();
                if (destination != null)
                {
                    return (myTerritory.Location, destination.Location);
                }
            }
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
