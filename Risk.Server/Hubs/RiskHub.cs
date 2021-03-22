using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Game;
using Microsoft.Extensions.Configuration;
using Risk.Shared;
using System.Threading;

namespace Risk.Server.Hubs
{
    public class RiskHub : Hub<IRiskHub>
    {
        private readonly ILogger<RiskHub> logger;
        private readonly IConfiguration config;
        public const int MaxFailedTries = 5;
        public const int TimeoutInSeconds = 2;
        private Player currentPlayer => (game.CurrentPlayer as Player);
        private Risk.Game.Game game { get; set; }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public RiskHub(ILogger<RiskHub> logger, IConfiguration config, Game.Game game)
        {
            this.logger = logger;
            this.config = config;
            this.game = game;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                logger.LogInformation(Context.ConnectionId);
                await base.OnConnectedAsync();
            }
            catch(Exception ex)
            {
                logger.LogError(Context.ConnectionId + ": " + ex.Message);
            }
            
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var player = game.RemovePlayerByToken(Context.ConnectionId);
            await BroadCastMessageAsync($"Player {player.Name} disconnected.  Removed from game.");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendMessage(user, message);
        }

        public async Task Signup(string user)
        {
            var duplicatePlayer = game.Players.ToList().FirstOrDefault(player => player.Token == Context.ConnectionId);
            if(duplicatePlayer != null)
            {
                await Clients.Client(duplicatePlayer.Token).SendMessage("Server", $"There is already a player registered on your client named {duplicatePlayer.Name}");
                (duplicatePlayer as Player).Strikes++;
                logger.LogWarning($"{duplicatePlayer} currently has {(duplicatePlayer as Player).Strikes}. {MaxFailedTries - (duplicatePlayer as Player).Strikes} more strikes will result in being kicked from the game.");
            }
            else if(game.GameState == GameState.Deploying || game.GameState == GameState.Attacking)
            {
                logger.LogError($"For {user}: There's already a game in progress.");
                await Clients.Client(Context.ConnectionId).SendMessage("Server", "There's already a game in progress.  Disconnect then try again once the game has finished.");
            }
            else
            {
                int i = 1;
                var baseName = user;
                while (game.Players.Any(p => p.Name == user))
                {
                    user = string.Concat(baseName, i.ToString());
                    i++;
                }

                logger.LogInformation($"{Context.ConnectionId}: {user}");

                try
                {
                    var newPlayer = new Player(Context.ConnectionId, user);
                    game.AddPlayer(newPlayer);
                    await BroadCastMessageAsync(newPlayer.Name + " has joined the game");
                    await Clients.Client(newPlayer.Token).SendMessage("Server", "Welcome to the game " + newPlayer.Name);
                    await Clients.Client(newPlayer.Token).JoinConfirmation(newPlayer.Name);
                }
                catch(Exception ex)
                {
                    logger.LogError("Client Connection Exception for Player " + user + ": " + ex.Message);
                }
            }
        }

        private async Task BroadCastMessageAsync(string message)
        {
            await Clients.All.SendMessage("Server", message);
        }

        private GameStatus getStatus()
        {
            GameStatus status = game.GetGameStatus();
            status.CurrentPlayer = currentPlayer.Name;
            return status;
        }

        public async Task GetStatus()
        {
            await Clients.Client(Context.ConnectionId).SendMessage("Server", game.GameState.ToString());            
            await Clients.Client(Context.ConnectionId).SendStatus(getStatus());
        }

        public async Task RestartGame(string password, GameStartOptions startOptions)
        {
            if(password == config["StartGameCode"])
            {
                if(game.Players.Count() == 0)
                {
                    await BroadCastMessageAsync("No players connected.  Unable to restart.");
                    return;
                }

                await BroadCastMessageAsync("Restarting game...");
                game.RestartGame(startOptions);
                await StartDeployPhase();
                await Clients.All.SendStatus(getStatus());
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendMessage("Server", "Incorrect password.");
            }
        }

        public async Task StartGame(string Password)
        {
            if (Password == config["StartGameCode"])
            {
                await BroadCastMessageAsync("The Game has started");
                game.StartGame();
                logger.LogInformation("The game has entered the Deployment Phase");
                await StartDeployPhase();
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendMessage("Server", "Incorrect password");
            }
        }

        private async Task StartDeployPhase()
        {
            game.CurrentPlayer = game.Players.First();

            await Clients.Client(currentPlayer.Token).YourTurnToDeploy(game.Board.SerializableTerritories);
        }

        public async Task DeployRequest(Location l)
        {
            if (game.GameState == GameState.GameOver)
                return;

            logger.LogInformation("Received DeployRequest from {connectionId}", Context.ConnectionId);

            if(Context.ConnectionId == currentPlayer.Token)
            {
                logger.LogInformation($"{Context.ConnectionId} belongs to {currentPlayer}");

                if(currentPlayer.Strikes >= MaxFailedTries)
                {
                    if(game.Players.Count() == 1)
                    {
                        await sendGameOverAsync();
                        return;
                    }
                    logger.LogInformation("Deployment Phase: {0} has too many strikes.  Booting from game.", currentPlayer.Name);
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", "Too many bad requests. No risk for you");
                    game.RemovePlayerByToken(currentPlayer.Token);
                    game.RemovePlayerFromBoard(currentPlayer.Token);
                    await tellNextPlayerToDeploy();
                    return;
                }

                if(game.TryPlaceArmy(Context.ConnectionId, l))
                {
                    await Clients.All.SendStatus(getStatus());
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", $"Successfully Deployed At {l.Row}, {l.Column}");
                    logger.LogInformation("{currentPlayer} deployed at {l}", currentPlayer, l);

                    if(game.GameState == GameState.Deploying)
                    {
                        try 
                        {
                            logger.LogInformation($"{currentPlayer.Name} is now telling next player to deploy.");
                            await tellNextPlayerToDeploy();
                        }
                        catch(Exception ex)
                        {
                            logger.LogError($"An exception occurred while {currentPlayer.Name} was passing the turn to the next player: {ex.Message}");
                        }
                    }
                    else
                    {
                        logger.LogInformation("All armies that can be deployed have been deployed.  Game State changing to Attack Phase.");
                        await StartAttackPhase();
                    }
                }
                else
                {
                    currentPlayer.Strikes++;
                    logger.LogWarning($"{currentPlayer} tried to deploy at {l} but deploy failed.  Increasing strikes.  You now have {currentPlayer.Strikes} strikes and will be kicked after {MaxFailedTries - currentPlayer.Strikes} more!");
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", "Did not deploy successfully");
                    await Clients.Client(currentPlayer.Token).YourTurnToDeploy(game.Board.SerializableTerritories);
                }
            }
            else
            {
                var badPlayer = game.Players.Single(p => p.Token == Context.ConnectionId) as Player;
                badPlayer.Strikes++;
                await Clients.Client(badPlayer.Token).SendMessage("Server", "It's not your turn");
                logger.LogInformation("{badPlayer} tried to deploy when it wasn't their turn.  Increasing invalid request count.  You now have {strikes} strikes!",
                    badPlayer.Name, badPlayer.Strikes);
            }
        }

        private async Task tellNextPlayerToDeploy()
        {
            if (game.GameState == GameState.GameOver)
                return;

            var players = game.Players.ToList();
            var currentPlayerIndex = players.IndexOf(game.CurrentPlayer);
            var nextPlayerIndex = currentPlayerIndex + 1;
            if (nextPlayerIndex >= players.Count)
            {
                nextPlayerIndex = 0;
            }

            if(players.Count <= nextPlayerIndex)
            {
                logger.LogWarning("What happened to all the players?!");
                await sendGameOverAsync();
                return;
            }

            game.CurrentPlayer = players[nextPlayerIndex];
            await Clients.Client(currentPlayer.Token).YourTurnToDeploy(game.Board.SerializableTerritories);
        }

        private async Task StartAttackPhase()
        {
            game.CurrentPlayer = game.Players.First();

            await Clients.Client(currentPlayer.Token).YourTurnToAttack(game.Board.SerializableTerritories);
        }

        public async Task AttackRequest(Location from, Location to)
        {
            if (game.GameState == GameState.GameOver)
                return;

            logger.LogInformation($"Received Attack Request from {Context.ConnectionId}");

            if (Context.ConnectionId == currentPlayer.Token)
            {
                logger.LogInformation($"{Context.ConnectionId} belongs to {currentPlayer}");

                game.OutstandingAttackRequestCount--;

                if (currentPlayer.Strikes >= MaxFailedTries)
                {
                    if (game.Players.Count() == 1)
                    {
                        await sendGameOverAsync();
                        return;
                    }
                    logger.LogInformation("Attacking Phase: {0} has too many strikes.  Booting from game.", currentPlayer.Name);
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", $"Too many bad requests. No risk for you");
                    game.RemovePlayerByToken(currentPlayer.Token);
                    game.RemovePlayerFromBoard(currentPlayer.Token);
                    await tellNextPlayerToAttack();
                    return;
                }

                if (game.Players.Count() > 1 && game.GameState == GameState.Attacking && game.Players.Any(p => game.PlayerCanAttack(p)))
                {
                    if (game.PlayerCanAttack(currentPlayer))
                    {
                        TryAttackResult attackResult = new TryAttackResult { AttackInvalid = false };
                        Territory attackingTerritory = null;
                        Territory defendingTerritory = null;
                        try
                        {
                            attackingTerritory = game.Board.GetTerritory(from);
                            defendingTerritory = game.Board.GetTerritory(to);

                            logger.LogInformation($"{currentPlayer.Name} wants to attack from {attackingTerritory} to {defendingTerritory}");

                            attackResult = game.TryAttack(currentPlayer.Token, attackingTerritory, defendingTerritory);
                            await Clients.All.SendStatus(getStatus());
                        }
                        catch (Exception ex)
                        {
                            attackResult = new TryAttackResult { AttackInvalid = true, Message = ex.Message };
                        }
                        if (attackResult.AttackInvalid)
                        {
                            currentPlayer.Strikes++;                         
                            logger.LogWarning($"Invalid attack request! from {attackingTerritory} to {defendingTerritory}. {currentPlayer.Name} now has {currentPlayer.Strikes} and will be kicked after {MaxFailedTries - currentPlayer.Strikes} more strikes!");
                            await Clients.Client(currentPlayer.Token).SendMessage("Server", $"Invalid attack request: {attackResult.Message} :(  You now have {currentPlayer.Strikes} strike(s)!");
                            await Clients.Client(currentPlayer.Token).YourTurnToAttack(game.Board.SerializableTerritories);
                        }
                        else
                        {
                            logger.LogInformation($"{currentPlayer} attacked from ({from.Row}), ({from.Column}) to ({to.Row}), ({to.Column})");
                            await Clients.Client(Context.ConnectionId).SendMessage("Server", $"Successfully Attacked From ({from.Row}, {from.Column}) To ({to.Row}, {to.Column})");

                            if (game.GameState == GameState.Attacking)
                            {
                                if (game.PlayerCanAttack(currentPlayer))
                                {
                                    await Clients.Client(currentPlayer.Token).YourTurnToAttack(game.Board.SerializableTerritories);
                                }
                                else
                                    await tellNextPlayerToAttack();
                            }
                            else
                            {
                                await sendGameOverAsync();
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            await Clients.Client(currentPlayer.Token).SendMessage("Server", "You are unable to attack.  Moving to next player.");
                            logger.LogInformation("Player {currentPlayer} cannot attack. anymore passing turn to next player", currentPlayer.Name);
                            await tellNextPlayerToAttack();
                        }
                        catch(Exception ex) 
                        {
                            logger.LogError($"An exception occurred while {currentPlayer.Name} was passing their turn to the next player: {ex.Message}");
                        }
                    }
                }
                else
                {
                    await sendGameOverAsync();
                }
            }
            else
            {
                var badPlayer = game.Players.Single(p => p.Token == Context.ConnectionId) as Player;
                badPlayer.Strikes++;
                logger.LogInformation("Player {currentPlayer} tried to play when it's not their turn.  You now have {strikes} strikes!", badPlayer.Name, badPlayer.Strikes);
                await Clients.Client(badPlayer.Token).SendMessage("Server", "It's not your turn");
            }
        }

        public async Task AttackComplete()
        {
            if (game.GameState == GameState.GameOver)
                return;

            if (game.Players.Count() > 1 && game.GameState == GameState.Attacking && game.Players.Any(p => game.PlayerCanAttack(p)))
                await tellNextPlayerToAttack();
            else
                await sendGameOverAsync();
        }

        private async Task tellNextPlayerToAttack()
        {
            if (game.GameState == GameState.GameOver)
                return;

            var players = game.Players.ToList();
            if (game.OutstandingAttackRequestCount >= players.Count * Game.Game.MaxTimesAPlayerCanNotAttack)
            {
                logger.LogInformation("Too many players skipped attacking, ending game.");
                await sendGameOverAsync();
                return;
            }
            game.OutstandingAttackRequestCount++;
            var currentPlayerIndex = players.IndexOf(game.CurrentPlayer);
            var nextPlayerIndex = currentPlayerIndex + 1;
            if (nextPlayerIndex >= players.Count)
            {
                nextPlayerIndex = 0;
            }
            if (players.Count <= nextPlayerIndex)
            {
                logger.LogWarning("What happened to all the players?!");
                await sendGameOverAsync();
                return;
            }

            game.CurrentPlayer = players[nextPlayerIndex];
            await Clients.Client(currentPlayer.Token).YourTurnToAttack(game.Board.SerializableTerritories);
        }

        private async Task sendGameOverAsync()
        {
            game.SetGameOver();
            var status = getStatus();
            logger.LogInformation("Game Over.\n\n{gameStatus}", status);
            var winners = status.PlayerStats.Where(s => s.Score == status.PlayerStats.Max(s => s.Score)).Select(s => s.Name);
            await BroadCastMessageAsync($"Game Over - {string.Join(',', winners)} win{(winners.Count() > 1?"":"s")}!");
            await Clients.All.SendStatus(getStatus());
        }
    }
}
