using Microsoft.AspNetCore.SignalR.Client;
using Risk.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Risk.Signalr.CS1400Client
{
    class Program
    {
        static HubConnection hubConnection;
        static string officialName;
        static (string[,] cellOwners, int[,] cellArmies) ToArrays(IEnumerable<BoardTerritory> cells) {
            var cellList = cells.ToList();
            int numRows = cells.Max(cell => cell.Location.Row) + 1;
            int numCols = cells.Max(cell => cell.Location.Row) + 1;
            string[,] cellOwners = new string[numRows, numCols];
            int[,] cellArmies = new int[numRows, numCols];
            foreach (var cell in cells) {
                cellOwners[cell.Location.Row, cell.Location.Column] = cell.OwnerName;
                cellArmies[cell.Location.Row, cell.Location.Column] = cell.Armies;
            }
            return (cellOwners, cellArmies);
        }

        static async Task Main(string[] args)
        {
            var playerName = PlayerLogic.WhatIsYourName();
            var serverAddress = "http://localhost:5000";
            if(args.Length == 1)
            {
                serverAddress = args[0];
            }
            Console.WriteLine($"Talking to the server at {serverAddress}.");

            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{serverAddress}/riskhub")
                .Build();
            hubConnection.On<IEnumerable<BoardTerritory>>(MessageTypes.YourTurnToDeploy, async (board) =>
            {
                int row, column;
                (var owners, var armies) = ToArrays(board);
                PlayerLogic.WhereDoYouWantToDeploy(
                    officialName, owners, armies, out row, out column);
                await DeployAsync(new Location(row, column));
            });
            hubConnection.On<IEnumerable<BoardTerritory>>(MessageTypes.YourTurnToAttack, async (board) =>
            {
                (var owners, var armies) = ToArrays(board);
                if (PlayerLogic.DoYouWantToAttack(officialName, owners, armies)) {
                    int fromRow, fromColumn;
                    int toRow, toColumn;
                    PlayerLogic.WhatAttackDoYouWantToDo(
                        officialName, owners, armies,
                        out fromRow, out fromColumn,
                        out toRow, out toColumn);
                    await AttackAsync(new Location(fromRow, fromColumn), new Location(toRow, toColumn));
                } else {
                    await AttackCompleteAsync();
                }
            });
            hubConnection.On<string>(MessageTypes.JoinConfirmation, validatedName => {
                officialName = validatedName;
                Console.Title = validatedName;
                Console.WriteLine($"Successfully joined server. Assigned Name is {validatedName}");
            });

            await hubConnection.StartAsync();
            Console.WriteLine("My connection id is " + hubConnection.ConnectionId);
            await SignupAsync(playerName);
            // just wait ....
            Console.ReadLine();
        }

        static async Task SignupAsync(string playerName)
        {
            await hubConnection.SendAsync(MessageTypes.Signup, playerName);
        }
        static async Task DeployAsync(Location desiredLocation) => await hubConnection.SendAsync(MessageTypes.DeployRequest, desiredLocation);
        static async Task AttackAsync(Location from, Location to) => await hubConnection.SendAsync(MessageTypes.AttackRequest, from, to);
        static async Task AttackCompleteAsync() => await hubConnection.SendAsync(MessageTypes.AttackComplete);
    }
}
