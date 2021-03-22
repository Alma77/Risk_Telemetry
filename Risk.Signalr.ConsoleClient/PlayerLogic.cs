using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Risk.Signalr.ConsoleClient
{
    public class PlayerLogic : IPlayerLogic
    {
        public PlayerLogic(string playerName, bool shouldSleep)
        {
            MyPlayerName = playerName;
            this.shouldSleep = shouldSleep;
            sleepFrequency = rng.Next(5, 20);
        }

        public string MyPlayerName { get; set; }
        int turnNum = 0;
        Random rng = new Random();
        int sleepFrequency = 0;
        private readonly bool shouldSleep;

        public Location WhereDoYouWantToDeploy(IEnumerable<BoardTerritory> board)
        {
            randomSleep();

            var myTerritory = board.FirstOrDefault(t => t.OwnerName == MyPlayerName) ?? board.Skip(board.Count() / 2).First(t => t.OwnerName == null);
            return myTerritory.Location;
        }

        private void randomSleep()
        {
            if (!shouldSleep)
                return;

            if (turnNum++ % sleepFrequency == 0)
            {
                int secondsToSleep = rng.Next(0, 3);
                Console.WriteLine($"Sleeping for {secondsToSleep}");
                Thread.Sleep(TimeSpan.FromSeconds(secondsToSleep));
            }
        }

        public (Location from, Location to) WhereDoYouWantToAttack(IEnumerable<BoardTerritory> board)
        {
            randomSleep();

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
