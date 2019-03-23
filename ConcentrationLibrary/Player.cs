// Names:   Jordan Travaux & Abel Emun
// Date:    March 18, 2019
// Purpose: Data contract to be used by the Concentration service

using System.Runtime.Serialization;

namespace ConcentrationLibrary
{
    [DataContract]
    public class Player
    {
        [DataMember] public int Points { get; set; }
        [DataMember] public int PlayerID { get; set; }

        // Create a new player
        public Player(int playerNum) {
            PlayerID = playerNum;
            Points = 0;
        }

        public override string ToString() => "Player: " + PlayerID + ", Points: " + Points;
    }
}
