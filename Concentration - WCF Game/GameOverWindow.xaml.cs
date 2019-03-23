using ConcentrationLibrary;
using System;
using System.Windows;

namespace ConcentrationClient
{
    public partial class GameOverWindow : Window
    {
        public GameOverWindow(Player winner, int playerID, TimeSpan gameTime)
        {
            InitializeComponent();

            // Winner
            if(winner.PlayerID == playerID)
            {
                tbWinnerStats.Text = string.Format("Player {0} won with {1}/26 points!", winner.PlayerID, winner.Points);
                tbGameTime.Text = string.Format("Elapsed game time: {0:mm\\:ss\\:ff}", gameTime);
            }
            else // Loser
            {
                tbWinner.Visibility = Visibility.Hidden;
                tbLoser.Visibility = Visibility.Visible;
                imgWinner.Visibility = Visibility.Hidden;
                imgLoser.Visibility = Visibility.Visible;
                tbWinnerStats.Text = string.Format("Player {0} won with {1}/26 points!", winner.PlayerID, winner.Points);
                tbGameTime.Text = string.Format("Elapsed game time: {0:mm\\:ss\\:ff}", gameTime);
            }

        }

        // Close the game
        private void Window_Closed(object sender, EventArgs e) => Environment.Exit(0);
    }
}
