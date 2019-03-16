using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ServiceModel;
using System.Runtime.Serialization;
using static ConcentrationLibrary.Concentration;
using System.Windows.Markup;
using System.Threading;
using System.Windows.Threading;

namespace ConcentrationLibrary
{
    [ServiceContract]
    public interface IConcentration {
        [OperationContract] Player GetCurrentPlayer();
        [OperationContract] void PointScored();

        List<Player> Players { [OperationContract]get; [OperationContract]set; }
        int CurrentPlayer { [OperationContract]get; [OperationContract]set; }
        int CardsFlipped { [OperationContract]get; [OperationContract]set; }
        string FirstBtnXaml { [OperationContract]get; [OperationContract]set; }
        string SecondBtnXaml { [OperationContract]get; [OperationContract]set; }
        string GameGridXaml { [OperationContract]get; [OperationContract]set; }
        Card FirstCard { [OperationContract]get; [OperationContract]set; }
        Card SecondCard { [OperationContract]get; [OperationContract]set; }
        Deck GameDeck { [OperationContract]get; [OperationContract]set; }
        Difficulty GameDifficulty { [OperationContract]get; [OperationContract]set; }

        int NumPlayers { [OperationContract]get; [OperationContract]set; }
        [OperationContract] int AddPlayer();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Concentration : IConcentration
    {
        public enum Difficulty { Easy = 25, Normal = 15, Hard = 5 };
        public Difficulty GameDifficulty { get; set; }
        public List<Player> Players { get; set; }
        public string FirstBtnXaml { get; set; }
        public string SecondBtnXaml { get; set; }
        public string GameGridXaml { get; set; }
        public int CardsFlipped { get; set; }
        public int NumPlayers { get; set; }
        public Card FirstCard { get; set; }
        public Card SecondCard { get; set; }
        public Deck GameDeck { get; set; }

        private Grid gameGrid;

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

        public Concentration() {
            Players = new List<Player>();
            _CurrentPlayer = 1;
            GameDifficulty = Difficulty.Hard;

            gameGrid = new Grid() { IsEnabled = false };

            for (int j = 0; j < 4; j++)
                gameGrid.RowDefinitions.Add(new RowDefinition());

            for (int j = 0; j < 13; j++)
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());

            if (GameDeck == null)
                GameDeck = new Deck();
            else
                GameDeck.Repopulate();

            for (int i = 0; i < 13; ++i)
                for (int j = 0; j < 4; ++j)
                {
                    // Draw a new card from the deck
                    Card c = GameDeck.Draw();

                    // The shown side (back of card)
                    Button back = new Button();
                    back.SetValue(Grid.RowProperty, j);
                    back.SetValue(Grid.ColumnProperty, i);
                    back.Tag = c;

                    // The Hidden side (Front of card)
                    Image img = new Image();
                    img.SetValue(Grid.RowProperty, j);
                    img.SetValue(Grid.ColumnProperty, i);
                    img.Margin = new Thickness(3);
                    img.Tag = c;

                    // Add the front of the card,
                    // then the back of the card
                    gameGrid.Children.Add(img);
                    gameGrid.Children.Add(back);
                }
            GameGridXaml = XamlWriter.Save(gameGrid);
        }

        public void PointScored() {
            foreach (Player p in Players)
                if (p.PlayerID == _CurrentPlayer)
                    p.Points++;
        }

        public Player GetCurrentPlayer() => Players.Find(p => p.PlayerID == _CurrentPlayer);
        public int AddPlayer(){
            Players.Add(new Player(++NumPlayers));
            return NumPlayers; 
        }
    }
}
