using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ServiceModel;
using System.Runtime.Serialization;
using static ConcentrationLibrary.Concentration;

namespace ConcentrationLibrary
{
    [ServiceContract]
    [ServiceKnownType(typeof(UIElement))]
    public interface IConcentration {
        [OperationContract] Player GetCurrentPlayer();
        [OperationContract] void ResetGame();
        [OperationContract] void PointScored();

        List<Player> Players { [OperationContract]get; [OperationContract]set; }
        int CurrentPlayer { [OperationContract]get; [OperationContract]set; }
        int CardsFlipped { [OperationContract]get; [OperationContract]set; }
        Difficulty GameDifficulty { [OperationContract]get; [OperationContract]set; }

        string FirstBtnXaml { [OperationContract]get; [OperationContract]set; }
        string SecondBtnXaml { [OperationContract]get; [OperationContract]set; }
        Card FirstCard { [OperationContract]get; [OperationContract]set; }
        Card SecondCard { [OperationContract]get; [OperationContract]set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Concentration : IConcentration
    {
        public enum Difficulty { Easy = 25, Normal = 15, Hard = 5 };
        public Difficulty GameDifficulty { get; set; }
        public List<Player> Players { get; set; }

        public string FirstBtnXaml { get; set; }
        public string SecondBtnXaml { get; set; }
        public int CardsFlipped { get; set; }
        public Card FirstCard { get; set; }
        public Card SecondCard { get; set; }

        private int _CurrentPlayer;
        public int CurrentPlayer {
            get { return _CurrentPlayer; }
            set {
                if (_CurrentPlayer >= Players.Count)
                    _CurrentPlayer = 1;
                else
                    _CurrentPlayer = value;
            }
        }

        public Player GetCurrentPlayer() {
            return Players.Find(p => p.PlayerID == _CurrentPlayer);
        }

        public Concentration() {
            Players = new List<Player>();
            _CurrentPlayer = 1;
            GameDifficulty = Difficulty.Hard;

            for (int i = 1; i <= 2; i++)
                Players.Add(new Player(i));
        }

        public void ResetGame() {
            foreach (Player p in Players)
                p.Points = 0;
            _CurrentPlayer = 1;
            CardsFlipped = 0;
        }

        public void PointScored() {
            foreach (Player p in Players)
                if (p.PlayerID == _CurrentPlayer)
                    p.Points++;
        }
    }
}
