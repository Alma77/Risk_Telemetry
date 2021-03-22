# Risk server for the Spring 2021 coding challenge

Play a (simplified) game of Risk with automated bots as players.

### How it works

- Communication between the Client(s) and Server is over a SignalR web socket. 
- Each bot will send a "Signup" message with your player name as an argument.
- The server will respond with a "JoinConfirmation" message sending your assigned name as an argument.  For example, if Johnny already signs up, but another client tries to sign up with the name Johnny, the server will respond with "Johnny1" as the assigned name, so there will be two players: Johnny and Johnny1.
- The game starts when someone sends a "StartGame" message with the correct secret code as a parameter.
- Placement Phase
  - The server will send a "YourTurnToDeploy" message with the current state of the board (a list of BoardTerritory objects).
  - You will respond with a "DeployRequest" message sending a Location as a parameter.
  - If the requested location is invalid, another request will be sent to that same player, but you'll get a strike.  
  - If the requested location is valid (e.g. it is unoccupied or that player occupies the territory), the number of available armies yet to place will be decremented and the next player will be asked to place an army.
  - When all players have placed all armies the game moves to the attacking phase.
- Attacking Phase
  - The server will send a "YourTurnToAttack" message sending the current state of the board (a list of BoardTerritory objects).
  - The player will send a "AttackRequest" message sending the from and to locations for their attack.
  - Optionally, the player could send a "AttackComplete" message yielding their turn to the next player.
  - The server will validate that the current player owns `fromLocation`, and the current player does *not* own `toLocation`
  - If `toLocation` is unoccupied, the attacker 'wins' and takes possession of the territory.  One army is left in `fromLocation` and all other armies are moved into `toLocation`
  - The server will roll min(armies, 3) attacking dice and min(armies, 2) defending dice.  The attacking and defending dice will be ordered greatest to lest, then each successive pair will be evaluated.  Ties go to the defender.
      For example, if the attacker rolls a 1, 3, 6 and the defender rolls a 3, 5 then the highest dice is 6 attacker, 5 defender - attacker wins (defender loses an army).  The next highest is 3 attacker, 3 defender - defender wins (attacker loses an army).
  - If the last defending army dies, the attacker takes posession of the territory.  One army is left in `fromLocation` and all other armies are moved into `toLocation`
  - If the attacker still has 2 or more armies, the server will send a "YourTurnToAttack" message sending the current state of the board and the attack sequence begins again.
- Game over
  - Once every army has completed every possible attack the game is over.
  - Final score is calculated as armyCount + territoryCount * 2
    

### Object Types
##### Board Territory
- Location : Location
- OwnerName : string
- Armies : int

##### Location
- Row : int
- Column : int
