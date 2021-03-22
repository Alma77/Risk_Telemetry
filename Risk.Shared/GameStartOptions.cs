using System.Collections;
using System.Collections.Generic;

namespace Risk.Shared
{
    public class GameStartOptions
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int StartingArmiesPerPlayer { get; set; }
        public int ArmiesDeployedPerTurn { get; set; }
    }
}