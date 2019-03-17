using System.Runtime.Serialization;

namespace ConcentrationLibrary
{
    [DataContract]
    public class Player
    {
        [DataMember] public int Points { get; set; }
        [DataMember] public int PlayerID { get; set; }

        public Player(int playerNum) {
            PlayerID = playerNum;
            Points = 0;
        }

        public override string ToString() => "Player: " + PlayerID + ", Points: " + Points;
    }
}
