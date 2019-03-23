// Names:   Jordan Travaux & Abel Emun
// Date:    March 18, 2019
// Purpose: Data contract to be used by the Concentration service

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ServiceModel;
using System.Windows.Markup;
using System.Linq;

namespace ConcentrationLibrary
{
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IConcentration {
        [OperationContract(IsOneWay = true)] void PointScored();
        [OperationContract(IsOneWay = true)] void NotifyCardFlip();
        [OperationContract] bool StartGame();
        [OperationContract] bool PauseGame();
        [OperationContract] int AddPlayer();
        [OperationContract] bool ToggleCallbacks();

        List<Player> Players { [OperationContract]get; [OperationContract]set; }
        int CurrentPlayer { [OperationContract]get; [OperationContract]set; }
        int CardsFlipped { [OperationContract]get; [OperationContract]set; }
        int NumPlayers { [OperationContract]get; [OperationContract]set; }
        string FirstBtnXaml { [OperationContract]get; [OperationContract]set; }
        string SecondBtnXaml { [OperationContract]get; [OperationContract]set; }
        string GameGridXaml { [OperationContract]get; [OperationContract]set; }
        Card FirstCard { [OperationContract]get; [OperationContract]set; }
        Card SecondCard { [OperationContract]get; [OperationContract]set; }
        Deck GameDeck { [OperationContract]get; [OperationContract]set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Concentration : IConcentration
    {
        private int currentPlayer;
        private HashSet<ICallback> callbacks;

        public List<Player> Players { get; set; }
        public string FirstBtnXaml { get; set; }
        public string SecondBtnXaml { get; set; }
        public string GameGridXaml { get; set; }
        public int NumPlayers { get; set; }
        public int CardsFlipped { get; set; }
        public Card FirstCard { get; set; }
        public Card SecondCard { get; set; }
        public Deck GameDeck { get; set; }
        public int CurrentPlayer {
            get => currentPlayer;
            set {
                currentPlayer = currentPlayer >= Players.Count ? 1 : value;
                foreach (ICallback callback in callbacks)
                    callback.NextPlayer();
            }
        }
      
        public Concentration() {
            Players = new List<Player>();
            callbacks = new HashSet<ICallback>();
            Grid gameGrid = new Grid() { IsEnabled = false };
            GameDeck = new Deck();
            currentPlayer = 1;

            // Add 4 rows and 13 columns
            for (int i = 0; i < 13; i++){
                if (i < 4)
                    gameGrid.RowDefinitions.Add(new RowDefinition());
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Create the game board
            for (int i = 0; i < 13; ++i)
                for (int j = 0; j < 4; ++j) {
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

        // Assigns a point to the current player
        public void PointScored() {
            foreach (Player p in Players)
                if (p.PlayerID == currentPlayer)
                    p.Points++;

            foreach (ICallback callback in callbacks)
                callback.RescanPlayers();

            // Check for a winner
            int totalPoints = 0;
            foreach (Player p in Players)
                totalPoints += p.Points;

            // Game is over, max points have been scored
            if (totalPoints == 26)
            {
                // Detirmine who won
                int mostPoints = Players.Max(p => p.Points);
                Player winner = Players.First(p => p.Points == mostPoints);

                foreach (ICallback callback in callbacks)
                    callback.GameFinished(winner);
            }
        }

        // Add a player to the game
        public int AddPlayer() {
            if(NumPlayers + 1 >= 7)
                return 0;
            else
                Players.Add(new Player(++NumPlayers));

            foreach (ICallback callback in callbacks)
                callback.RescanPlayers();

            return NumPlayers;
        }

        // Send a notification to all clients that the game has started
        public bool StartGame() {
            foreach (ICallback callback in callbacks)
                callback.GameStarted();
            return true;
        }

        // Send a notification to all clients that the game has been paused
        // because in this multiplayer game, you can pause!
        public bool PauseGame() {
            foreach (ICallback callback in callbacks)
                callback.GamePaused();
            return true;
        }

        // Send a notification to all clients that a card has been flipped
        public void NotifyCardFlip() {
            foreach (ICallback callback in callbacks)
                if (CardsFlipped == 1)
                    callback.CardFlipped(FirstBtnXaml);
                else
                    callback.CardFlipped(SecondBtnXaml);
        }

        // Toggle callbacks for the clients
        public bool ToggleCallbacks() {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
            if (callbacks.Contains(cb)) {
                callbacks.Remove(cb);
                return false;
            } else {
                callbacks.Add(cb);
                return true;
            }
        }

    } // end class
} // end namespace