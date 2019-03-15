using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ConcentrationLibrary;
using System.ServiceModel;
using System;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConcentrationClient
{
    public partial class MainWindow : Window
    {
        BackgroundWorker worker;
        IConcentration game;
        Grid gameGrid;
        ObservableCollection<Player> players;
        Deck deck;

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(int), typeof(MainWindow), new PropertyMetadata(1));
        public int CurrentPlayer {
            get => (int)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public MainWindow()
        {
            
            InitializeComponent();

            ChannelFactory<IConcentration> channel = new ChannelFactory<IConcentration>(
                new NetTcpBinding(), 
                new EndpointAddress("net.tcp://localhost:5000/ConcentrationLibrary/Concentration"));

            game = channel.CreateChannel();

            gameGrid = new Grid() { IsEnabled = false };

            for (int j = 0; j < 4; j++)
                gameGrid.RowDefinitions.Add(new RowDefinition());

            for (int j = 0; j < 13; j++)
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());

            SetupGame();

            mainGrid.Children.Add(gameGrid);

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_RememberCardsTimer;
            worker.ProgressChanged += Worker_UpdateProgressBar;
            worker.RunWorkerCompleted += Worker_Complete;

            players = new ObservableCollection<Player>();
            lbPlayers.ItemsSource = players;
            UpdatePlayers();

            DataContext = this;
        }

        public void SetupGame() {
            if (deck == null)
                deck = new Deck();
            else
                deck.Repopulate();

            gameGrid.Children.Clear();

            for (int i = 0; i < 13; ++i)
                for (int j = 0; j < 4; ++j)
                {
                    // Draw a new card from the deck
                    Card c = deck.Draw();

                    // The shown side (back of card)
                    Button back = new Button();
                    back.SetValue(Grid.RowProperty, j);
                    back.SetValue(Grid.ColumnProperty, i);
                    back.Click += FlipCard;
                    back.Tag = c;

                    // The Hidden side (Front of card)
                    Image img = new Image();
                    img.SetValue(Grid.RowProperty, j);
                    img.SetValue(Grid.ColumnProperty, i);
                    img.Source = new BitmapImage(new Uri("Images/" + c.GetCharRank() + c.Suit.ToString() + ".png", UriKind.Relative));
                    img.Margin = new Thickness(3);

                    // Add the front of the card,
                    // then the back of the card
                    gameGrid.Children.Add(img);
                    gameGrid.Children.Add(back);
                }
        }

        ///////////////////////
        // GUI Events/Helpers
        //////////////////////
        private void StartGame(object sender, RoutedEventArgs e) {
            (sender as Button).IsEnabled = false;
            gameGrid.IsEnabled = true;
            btnEnd.IsEnabled = true;

            lbPlayers.SelectedIndex = CurrentPlayer - 1;
        }

        private void EndGame(object sender, RoutedEventArgs e) {
            (sender as Button).IsEnabled = false;
            gameGrid.IsEnabled = false;
            btnStart.IsEnabled = true;

            // Reset the game and update the players
            game.ResetGame();
            UpdatePlayers();
            CurrentPlayer = game.CurrentPlayer;

            // Reset the progress bar
            pbText.Text = "";
            pbRememberCardsTimer.Value = 0;

            // Setup next game
            SetupGame();
        }
        
        private void FlipCard(object sender, RoutedEventArgs e) {
            game.CardsFlipped++;

            if (game.CardsFlipped == 1)
            {
                game.FirstBtnXaml = XamlWriter.Save(sender as Button);
                game.FirstCard = GetButtonFromXaml(game.FirstBtnXaml).Tag as Card;
                FindButtonChangeVisibility(GetButtonFromXaml(game.FirstBtnXaml), true);
            }
            else if (game.CardsFlipped == 2)
            {
                game.SecondBtnXaml = XamlWriter.Save(sender as Button);
                game.SecondCard = GetButtonFromXaml(game.SecondBtnXaml).Tag as Card;
                FindButtonChangeVisibility(GetButtonFromXaml(game.SecondBtnXaml), true);

                // See if a match...
                if ((game.FirstCard.Color == game.SecondCard.Color) && (game.FirstCard.Rank == game.SecondCard.Rank)) {
                    game.PointScored();
                    UpdatePlayers();
                    game.CardsFlipped = 0;
                    pbText.Text = "Point Awarded!";
                    pbRememberCardsTimer.Foreground = Brushes.LimeGreen;
                    pbRememberCardsTimer.Value = 100;
                    lbPlayers.SelectedIndex = CurrentPlayer - 1;
                }
                else
                {
                    pbText.Text = "Remember the cards...";
                    pbRememberCardsTimer.Foreground = Brushes.IndianRed;
                    worker.RunWorkerAsync();

                    // Change player turn on no point
                    game.CurrentPlayer++;
                    CurrentPlayer = game.CurrentPlayer;
                    lbPlayers.SelectedIndex = CurrentPlayer - 1;
                }
            }
        }

        private void UpdatePlayers(){
            players.Clear();
            foreach (Player p in game.Players)
                players.Add(p);
        }

        private Button GetButtonFromXaml(string xaml) => XamlReader.Parse(xaml) as Button;

        private void FindButtonChangeVisibility(Button btn, bool hidden = false)
        {
            if (hidden) {
                foreach (UIElement b in gameGrid.Children)
                    if(b.GetType() == typeof(Button))
                        if (((Button)b).Tag != null)
                            if (((Button)b).Tag.ToString() == btn.Tag.ToString())
                                b.Visibility = Visibility.Hidden;
            } else {
                foreach (UIElement b in gameGrid.Children)
                    if (b.GetType() == typeof(Button))
                        if (((Button)b).Tag != null)
                            if (((Button)b).Tag.ToString() == btn.Tag.ToString())
                                b.Visibility = Visibility.Visible;
            }
        }

        /*---------------
         Worker methods
        --------------- */
        void Worker_UpdateProgressBar(object sender, ProgressChangedEventArgs e) => pbRememberCardsTimer.Value = e.ProgressPercentage;

        void Worker_RememberCardsTimer(object sender, DoWorkEventArgs e) {
            for (int i = 0; i < 100; i++) {
                (sender as BackgroundWorker).ReportProgress(i);
                Thread.Sleep((int)game.GameDifficulty);
            }
        }

        void Worker_Complete(object sender, RunWorkerCompletedEventArgs e) {
            pbRememberCardsTimer.Value = 0;
            pbText.Text = "";
            FindButtonChangeVisibility(GetButtonFromXaml(game.FirstBtnXaml));
            FindButtonChangeVisibility(GetButtonFromXaml(game.SecondBtnXaml));
            game.CardsFlipped = 0;
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show(this.Width.ToString() + "," + this.Height.ToString());
        }
    }
}
