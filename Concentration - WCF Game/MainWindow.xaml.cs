﻿using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using System.Diagnostics;

namespace ConcentrationClient
{
    public partial class MainWindow : Window
    {
        
        BackgroundWorker worker;
        DispatcherTimer timer;
        Stopwatch stopwatch;
        
        Grid gameGrid;
        IConcentration game;
        ObservableCollection<Player> players;
        ChannelFactory<IConcentration> channel;

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

        public static readonly DependencyProperty PlayerIDProperty = DependencyProperty.Register("PlayerID", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        public int PlayerID {
            get { return (int)GetValue(PlayerIDProperty); }
            set { SetValue(PlayerIDProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            channel = new ChannelFactory<IConcentration>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:5000/ConcentrationLibrary/Concentration"));
            game = channel.CreateChannel();

            // Assign the player number
            PlayerID = game.AddPlayer();

            if(PlayerID >= 7) {
                MessageBox.Show("Players 6/6. Game is full, please try again later.", "Game is Full", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            gameGrid = XamlReader.Parse(game.GameGridXaml) as Grid;
            
            // Add the listeners to the buttons and images
            foreach (UIElement b in gameGrid.Children)
                if(b.GetType() == typeof(Button))
                    (b as Button).Click += FlipCard;
                else if (b.GetType() == typeof(Image))
                    (b as Image).Source = new BitmapImage(new Uri("Images/" + ((b as Image).Tag as Card).GetRankString() + ((b as Image).Tag as Card).Suit.ToString() + ".png", UriKind.Relative));

            mainGrid.Children.Add(gameGrid);

            // Add a background worker for the progress bar
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_RememberCardsTimer;
            worker.ProgressChanged += Worker_UpdateProgressBar;
            worker.RunWorkerCompleted += Worker_Complete;

            // Add a timer to track the game time
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            stopwatch = new Stopwatch();

            players = new ObservableCollection<Player>();
            lbPlayers.ItemsSource = players;
            UpdatePlayers();

            DataContext = this;
        }

        ///////////////////////
        // GUI Events/Helpers
        //////////////////////
        private void StartGame(object sender, RoutedEventArgs e) {
            (sender as Button).IsEnabled = false;
            gameGrid.IsEnabled = true;
            btnPause.IsEnabled = true;

            timer.Start();
            stopwatch.Start();

            // Update the players
            UpdatePlayers();
            lbPlayers.SelectedIndex = CurrentPlayer - 1;

            // Reset the progress bar
            pbText.Text = "";
            pbRememberCardsTimer.Value = 0;
        }

        private void PauseGame(object sender, RoutedEventArgs e) {
            (sender as Button).IsEnabled = false;
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
                }
            }
        }

        private void UpdatePlayers(){
            players.Clear();
            foreach (Player p in game.Players)
                players.Add(p);
        }

        private Button GetButtonFromXaml(string xaml) => XamlReader.Parse(xaml) as Button;

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

            // Change player turn
            game.CurrentPlayer++;
            CurrentPlayer = game.CurrentPlayer;
            lbPlayers.SelectedIndex = CurrentPlayer - 1;
        }

        private void Timer_Tick(object sender, EventArgs e) => CurrentTime = stopwatch.Elapsed.ToString("mm\\:ss\\:ff");
    }
}
