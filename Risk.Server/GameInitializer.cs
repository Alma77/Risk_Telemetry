using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Server
{
    public static class GameInitializer
    {
        public static Game.Game InitializeGame(int height, int width, int numOfArmies)
        {
            GameStartOptions startOptions = new GameStartOptions
            {
                Height = height,
                Width = width,
                StartingArmiesPerPlayer = numOfArmies
            };
            Game.Game newGame = new Game.Game(startOptions);

            newGame.StartJoining();
            return newGame;
        }
    }
}
