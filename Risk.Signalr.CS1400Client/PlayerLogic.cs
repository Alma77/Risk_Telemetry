// Sample Client for Tisk coding challenge

namespace Risk.Signalr.CS1400Client
{
    public class PlayerLogic
    {
        // returns the name you would like the server to call you
        // you will actually be assigned a name based on this, but
        // it might be slightly different which is why the other
        // methods accept your officialName as an argument
        public static string WhatIsYourName()
        {
            return "CS1400 Player";
        }

        // chooses a row and column for the deployment of your next army
        // as a function of your official name and the current state of the
        // board (owner and number of armies at each cell)
        public static void WhereDoYouWantToDeploy(string officialName, string[,] territoryOwners, int[,] territoryArmyCounts, out int chosenRow, out int chosenColumn)
        {
            chosenRow = 0;
            chosenColumn = 0;
        }

        // returns true if you want to continue attacking as a
        // function of your official name and the current state of the
        // board (owner and number of armies at each cell)
        public static bool DoYouWantToAttack(string officialName, string[,] territoryOwners, int[,] territoryArmyCounts)
        {
            return true;
        }

        // chooses a row and column to attack from and a row and column to attack
        // as a function of your official name and the current state of the
        // board (owner and number of armies at each cell)
        public static void WhatAttackDoYouWantToDo(string officialName, string[,] territoryOwners, int[,] territoryArmyCounts, out int chosenRowToAttackFrom, out int chosenColumnToAttackFrom, out int chosenRowToAttack, out int chosenColumnToAttack)
        {
            chosenRowToAttackFrom = 0;
            chosenColumnToAttackFrom = 0;
            chosenRowToAttack = 0;
            chosenColumnToAttack = 1;
        }
    }
}
