using Risk.Game;
using Risk.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Risk.Server.Hubs
{
    public interface IRiskHub
    {
        Task SendMessage(string from, string message);
        Task Signup(string playerName);
        Task GetStatus();
        Task SendStatus(GameStatus status);
        Task StartGame(string secretCode);
        Task DeployRequest(Location location);
        Task AttackRequest(Location from, Location to);
        Task AttackComplete();
        Task YourTurnToDeploy(IEnumerable<BoardTerritory> currentBoard);
        Task YourTurnToAttack(IEnumerable<BoardTerritory> currentBoard);
        Task JoinConfirmation(string name);
        Task RestartGame(string secretCode);
    }
}