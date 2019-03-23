// Names:   Jordan Travaux & Abel Emun
// Date:    March 20, 2019
// Purpose: GUI for the concentration game, which uses the ServiceContract
//          and implements the callback interface

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
using System.Windows.Threading;
using System.Diagnostics;

namespace ConcentrationClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        BackgroundWorker worker;
        DispatcherTimer timer;
        Grid gameGrid;
        IConcentration game;
        ObservableCollection<Player> players;
        Stopwatch stopwatch;

        bool callbacksEnabled;
        bool gameStarted;
        bool gamePaused;
        int playerID;

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(int), typeof(MainWindow), new PropertyMetadata(1));
        public int CurrentPlayer {
            get => (int)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public static readonly DependencyProperty CurrentTimeProperty = DependencyProperty.Register("CurrentTime", typeof(string), typeof(MainWindow), new PropertyMetadata(null));
        public string CurrentTime {
            get => (string)GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        public MainWindow() {
            InitializeComponent();

            DuplexChannelFactory<IConcentration> channel = new DuplexChannelFactory<IConcentration>(this, "ConcentrationService");
            game = channel.CreateChannel();

            // Assign the player number
            playerID = game.AddPlayer();

            // See if the game is full (6/6)
            if (playerID == 0) {
                MessageBox.Show("Players 6/6. Game is full, please try again later.", "Game is Full", MessageBoxButton.OK, MessageBoxImage.Error);
                channel.Close();
                Close();
            }

            // Subscribe to callbacks
            callbacksEnabled = game.ToggleCallbacks();

            // Get and set the game board & set player ID
            gameGrid = XamlReader.Parse(game.GameGridXaml) as Grid;
            lblPlayerID.Content = playerID;

            // Add the listeners to the buttons, and images for the cards
            foreach (UIElement b in gameGrid.Children)
                if(b.GetType() == typeof(Button))
                    (b as Button).Click += FlipCard;
                else if (b.GetType() == typeof(Image))
                    (b as Image).Source = new BitmapImage(new Uri("Images/" + ((b as Image).Tag as Card).GetRankString() + ((b as Image).Tag as Card).Suit.ToString() + ".png", UriKind.Relative));

            // Add all the game board to the window
            mainGrid.Children.Add(gameGrid);

            // Add a background worker for the progress bar
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_RememberCardsTimer;
            worker.ProgressChanged += Worker_UpdateProgressBar;
            worker.RunWorkerCompleted += Worker_Complete;

            // Add a timer to track the game time
            stopwatch = new Stopwatch();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            
            // Store the players of the game
            players = new ObservableCollection<Player>();
            lbPlayers.ItemsSource = players;
            UpdatePlayers();

            // Set the datacontext for binding to DP's
            DataContext = this;
        }

        ///////////////////////
        // GUI Events/Helpers
        //////////////////////
        private void StartGame(object sender, RoutedEventArgs e) {

            if (!gameStarted) {
                gameStarted = game.StartGame();
                gamePaused = false;
            }

            // Detirmine who's turn it is
            NextPlayer();

            btnStart.IsEnabled = false;
            gameGrid.IsEnabled = true;
            btnPause.IsEnabled = true;

            // Update the players
            UpdatePlayers();
            lbPlayers.SelectedIndex = CurrentPlayer - 1;

            // (Re)set the progress bar
            pbText.Text = "";
            pbRememberCardsTimer.Value = 0;

            // Start the timers
            timer.Start();
            stopwatch.Start();
        }

        private void PauseGame(object sender, RoutedEventArgs e) {

            if (!gamePaused){
                gamePaused = game.PauseGame();
                gameStarted = false;
            }

            btnPause.IsEnabled = false;
            gameGrid.IsEnabled = false;
            btnStart.IsEnabled = true;

            // Reset the progress bar
            pbText.Text = "Game Paused";
            pbRememberCardsTimer.Foreground = Brushes.Yellow;
            pbRememberCardsTimer.Value = 100;

            // Stop the timers
            timer.Stop();
            stopwatch.Stop();
        }
        
        private void FlipCard(object sender, RoutedEventArgs e) {
            game.CardsFlipped++;

            if (game.CardsFlipped == 1)
            {
                game.FirstBtnXaml = XamlWriter.Save(sender as Button);
                game.FirstCard = GetButtonFromXaml(game.FirstBtnXaml).Tag as Card;
                game.NotifyCardFlip();
            }
            else if (game.CardsFlipped == 2)
            {
                game.SecondBtnXaml = XamlWriter.Save(sender as Button);
                game.SecondCard = GetButtonFromXaml(game.SecondBtnXaml).Tag as Card;
                game.NotifyCardFlip();

                // See if a match...
                if ((game.FirstCard.Color == game.SecondCard.Color) && (game.FirstCard.Rank == game.SecondCard.Rank)) {
                    game.PointScored();
                    game.CardsFlipped = 0;

                    UpdatePlayers();
                    lbPlayers.SelectedIndex = CurrentPlayer - 1;
                    pbText.Text = "Point Awarded!";
                    pbRememberCardsTimer.Foreground = Brushes.LimeGreen;
                    pbRememberCardsTimer.Value = 100;
                }
                else
                {
                    // Change player turn 
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

        private void FindButtonChangeVisibility(Button btn, bool hidden = false) {
            foreach (UIElement b in gameGrid.Children)
                if (b.GetType() == typeof(Button))
                    if (((Button)b).Tag != null)
                        if (((Button)b).Tag.ToString() == btn.Tag.ToString())
                            if(hidden)
                                b.Visibility = Visibility.Hidden;
                            else
                                b.Visibility = Visibility.Visible;
        }

        private Button GetButtonFromXaml(string xaml) => XamlReader.Parse(xaml) as Button;

        /*---------------
         Worker methods
        --------------- */
        void Timer_Tick(object sender, EventArgs e) => CurrentTime = stopwatch.Elapsed.ToString("mm\\:ss\\:ff");
        void Worker_UpdateProgressBar(object sender, ProgressChangedEventArgs e) => pbRememberCardsTimer.Value = e.ProgressPercentage;
        
        void Worker_RememberCardsTimer(object sender, DoWorkEventArgs e) {
            for (int i = 0; i < 100; i++) {
                (sender as BackgroundWorker).ReportProgress(i);
                Thread.Sleep(7);
            }
        }

        void Worker_Complete(object sender, RunWorkerCompletedEventArgs e) {
            pbRememberCardsTimer.Value = 0;
            pbText.Text = "";
            FindButtonChangeVisibility(GetButtonFromXaml(game.FirstBtnXaml));
            FindButtonChangeVisibility(GetButtonFromXaml(game.SecondBtnXaml));
            game.CardsFlipped = 0;
        }

        /////////////////////
        // Callback Methods
        /////////////////////
        public delegate void CallbackDelegate();
        public delegate void CardFlippedDelegate(string btnXaml);

        public void RescanPlayers() {
            if (Thread.CurrentThread == Dispatcher.Thread)
                UpdatePlayers();
            else
                Dispatcher.BeginInvoke(new CallbackDelegate(RescanPlayers));
        }

        public void CardFlipped(string btnXaml) {
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                FindButtonChangeVisibility(GetButtonFromXaml(btnXaml), true);
                if (game.CardsFlipped == 2)
                {
                    pbText.Text = "Remember the cards...";
                    pbRememberCardsTimer.Foreground = Brushes.IndianRed;

                    // If a point is NOT scored...
                    if (!((game.FirstCard.Color == game.SecondCard.Color) && (game.FirstCard.Rank == game.SecondCard.Rank)))
                        worker.RunWorkerAsync();
                    else {
                        pbText.Text = "Point Awarded to Player " + game.CurrentPlayer + "...";
                        pbRememberCardsTimer.Foreground = Brushes.OrangeRed;
                        pbRememberCardsTimer.Value = 100;
                    }
                }
            }
            else
                Dispatcher.BeginInvoke(new CardFlippedDelegate(CardFlipped), btnXaml);
        }

        public void GameStarted() {
            if (Thread.CurrentThread == Dispatcher.Thread)
                StartGame(null, null);
            else
                Dispatcher.BeginInvoke(new CallbackDelegate(GameStarted));
        }

        public void GamePaused() {
            if (Thread.CurrentThread == Dispatcher.Thread)
                PauseGame(null, null);
            else
                Dispatcher.BeginInvoke(new CallbackDelegate(GamePaused));
        }

        public void GameFinished() {
            throw new NotImplementedException();
        }

        public void NextPlayer() {
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                if (game.CurrentPlayer == playerID)
                    foreach (UIElement b in gameGrid.Children)
                        b.IsEnabled = true;
                else
                    foreach (UIElement b in gameGrid.Children)
                        b.IsEnabled = false;

                CurrentPlayer = game.CurrentPlayer;
                lbPlayers.SelectedIndex = CurrentPlayer - 1;
            }
            else
                Dispatcher.BeginInvoke(new CallbackDelegate(NextPlayer));
        }
    }
}
