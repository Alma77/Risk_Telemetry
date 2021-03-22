using Microsoft.Extensions.Logging;
using Risk.Server.Hubs;
using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Server
{
    public class GameRunner
    {
        private readonly Game.Game game;
        private readonly IList<Player> removedPlayers;
        private readonly ILogger<GameRunner> logger;
        public const int MaxFailedTries = 5;

        public GameRunner(Game.Game game, ILogger<GameRunner> logger, RiskHub hub)
        {
            this.game = game;
            this.removedPlayers = new List<Player>();
            this.logger = logger;
        }

        public async Task StartGameAsync()
        {
            await deployArmiesAsync();
            await doBattle();
            await reportWinner();
        }

        private Task deployArmiesAsync()
        {
            throw new NotImplementedException();
        }

        private Task doBattle()
        {
            throw new NotImplementedException();
        }

        private Task reportWinner()
        {
            throw new NotImplementedException();
        }

        public void BootPlayerFromGame(Player apiPlayer)
        {
            throw new NotImplementedException();
        }
    }
}
