
using System.Runtime.Serialization;

namespace ConcentrationLibrary
{
    [DataContract]
    public class Card
    {
        // Enumerations
        public enum SuitID { C, D, H, S }; // Clubs, Diamonds, Hearts, Spades
        public enum RankID { Ace, King, Queen, Jack, Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two };
        public enum ColorID { Black, Red };

        // Public properties
        [DataMember] public SuitID Suit { get; set; }
        [DataMember] public RankID Rank { get; set; }
        [DataMember] public ColorID Color { get; set; }

        // Constructors
        public Card() { }
        public Card(SuitID s, RankID r, ColorID c) { Suit = s; Rank = r; Color = c; }

        public override string ToString() => Rank.ToString() + "\n" + Suit.ToString() + "\n" + Color.ToString();

        public string GetRankString() {
            switch (Rank) {
                case RankID.Ace:
                    return "A";
                case RankID.King:
                    return "K";
                case RankID.Queen:
                    return "Q";
                case RankID.Jack:
                    return "J";
                case RankID.Ten:
                    return "10";
                case RankID.Nine:
                    return "9";
                case RankID.Eight:
                    return "8";
                case RankID.Seven:
                    return "7";
                case RankID.Six:
                    return "6";
                case RankID.Five:
                    return "5";
                case RankID.Four:
                    return "4";
                case RankID.Three:
                    return "3";
                case RankID.Two:
                    return "2";
                default:
                    return "/";
            }
        }
    }
}
