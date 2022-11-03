using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Foundation;
using Windows.System;
using static System.Formats.Asn1.AsnWriter;

namespace MemoryMatchingGame
{
    public sealed partial class GamePage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();
        private int _markNum;

        private double _gameSpeed;
        private readonly double _gameSpeedDefault = 5;

        private bool _isGameOver;
        private bool _isPowerMode;

        private double _windowHeight, _windowWidth;
        private double _scale;

        private Uri[] _cards;
        private Uri[] _powerUps;

        private PowerUpType _powerUpType;

        private int _cardPeekCounter;
        private readonly int _cardPeekCounterDefault = 50;

        private int _powerModeDurationCounter;
        private readonly int _powerModeDuration = 1000;

        private double _score;
        private double _scoreCap;
        private double _difficultyMultiplier;

        private int _collectibleCollected;

        #endregion

        #region Ctor

        public GamePage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Events

        #region Button

        private void QuitGameButton_Checked(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void QuitGameButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ResumeGame();
        }

        private void ConfirmQuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(StartPage));
        }

        #endregion

        #endregion

        #region Methods

        #region Animation

        #region Game

        private void LoadGameElements()
        {
            _cards = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.CARD).Select(x => x.Value).ToArray();           
            _powerUps = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.POWERUP).Select(x => x.Value).ToArray();
        }

        private void PopulateGameViews()
        {
#if DEBUG
            Console.WriteLine("INITIALIZING GAME");
#endif
            SetViewSize();
            PopulateGameView();
        }

        private void PopulateGameView()
        {
           //TODO: add cards to container
        }

        private void StartGame()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif
            HideInGameTextMessage();
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            _gameSpeed = _gameSpeedDefault * _scale;

         

            _isGameOver = false;
            _isPowerMode = false;

            _powerModeDurationCounter = _powerModeDuration;            

            _score = 0;
            _scoreCap = 50;
            _difficultyMultiplier = 1;

            _collectibleCollected = 0;
            ScoreText.Text = "0";

            StartGameSounds();

            RunGame();
#if DEBUG
            Console.WriteLine($"GAME SPEED: {_gameSpeed}");
#endif
        }

        private async void RunGame()
        {
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
            {
                GameViewLoop();
            }
        }

        private void GameViewLoop()
        {
            ScoreText.Text = _score.ToString("#");
            
            //UpdateGameObjects();
            //RemoveGameObjects();

            //if (_isPowerMode)
            //{
            //    PowerUpCoolDown();
            //    if (_powerModeDurationCounter <= 0)
            //        PowerDown();
            //}

        }

        private void PauseGame()
        {   
            ShowInGameTextMessage("GAME_PAUSED");

            _gameViewTimer?.Dispose();

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            PauseGameSounds();
        }

        private void ResumeGame()
        {            
            HideInGameTextMessage();

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            SoundHelper.ResumeSound(SoundType.BACKGROUND);

            RunGame();
        }

        private void StopGame()
        {
            _gameViewTimer?.Dispose();
            StopGameSounds();
        }

        private void GameOver()
        {
            _isGameOver = true;

            PlayerScoreHelper.PlayerScore = new MemoryMatchingGameScore()
            {
                Score = Math.Ceiling(_score),
                CollectiblesCollected = _collectibleCollected
            };

            SoundHelper.PlaySound(SoundType.GAME_OVER);

            //TODO: to game over page
            //NavigateToPage(typeof(GameOverPage));
        }

        #endregion 

        #endregion

        #region Sound

        private void StartGameSounds()
        {
            SoundHelper.RandomizeSound(SoundType.BACKGROUND);
            SoundHelper.PlaySound(SoundType.BACKGROUND);
        }

        private void StopGameSounds()
        {
            SoundHelper.StopSound(SoundType.BACKGROUND);
        }

        private void PauseGameSounds()
        {
            SoundHelper.PauseSound(SoundType.BACKGROUND);
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);
#if DEBUG
            Console.WriteLine($"SCALE: {_scale}");
#endif
        }

        private void NavigateToPage(Type pageType)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region InGameMessage

        private void ShowInGameTextMessage(string resourceKey)
        {
            InGameMessageText.Text = LocalizationHelper.GetLocalizedResource(resourceKey);
            InGameMessagePanel.Visibility = Visibility.Visible;
        }

        private void HideInGameTextMessage()
        {
            InGameMessageText.Text = "";
            InGameMessagePanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion
    }
}
