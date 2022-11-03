using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.System;

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

        private Uri[] _memoryTiles;
        private Uri[] _powerUps;

        private PowerUpType _powerUpType;

        private int _powerModeDurationCounter;
        private readonly int _powerModeDuration = 1000;

        private double _score;
        private double _difficultyMultiplier;

        private double _playerHealth;
        private readonly double _playerHealthDefault = 100;

        private int _collectibleCollected;

        private int _playerHealthDepletionCounter;
        private double _playerHealthDepletionPoint;
        private double _playerHealthRejuvenationPoint;

        private readonly double _healthDepletePointDefault = 0.5;
        private readonly double _healthGainPointDefault = 10;

        private int _rows = 2;
        private int _columns = 2;
        private int _memoryTilePairsCount;

        private ObservableCollection<MemoryTile> _createdMemoryTiles = new();

        private (string Id, int Number) _selectedTile1 = new(null, -1);
        private (string Id, int Number) _selectedTile2 = new(null, -1);

        private bool _canSelect;

        private IEnumerable<MemoryTile> _memoryTilesInGame;

        #endregion

        #region Ctor

        public GamePage()
        {
            this.InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            SetGameView();

            Loaded += GamePage_Loaded;
            Unloaded += GamePage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += GamePage_SizeChanged;
        }

        private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePage_SizeChanged;
            StopGame();
        }

        private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            SetViewSize();

            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
        }

        #endregion

        #region Input

        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isGameOver)
            {
                App.EnterFullScreen(true);

                InputView.Visibility = Visibility.Collapsed;
                StartGame();
            }
        }

        private void MemoryTile_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!_canSelect)
            {
                SoundHelper.PlaySound(SoundType.SELECTION_BARRED);
                return;
            }

            MemoryTile tile = sender as MemoryTile;

            if (_selectedTile1.Id.IsNullOrBlank())
            {
                _selectedTile1 = new(tile.Id, tile.Number);
                tile.RevealTile();
#if DEBUG
                Console.WriteLine($"TILE1 ID: {_selectedTile1} NUMBER: {tile.Number}");
#endif
            }
            else if (!_selectedTile1.Id.IsNullOrBlank())
            {
                _selectedTile2 = new(tile.Id, tile.Number);
                tile.RevealTile();
#if DEBUG
                Console.WriteLine($"TILE2 ID: {_selectedTile2} NUMBER: {tile.Number}");
#endif
            }

            if (!_selectedTile1.Id.IsNullOrBlank() && !_selectedTile2.Id.IsNullOrBlank())
            {
                var tiles = _memoryTilesInGame;

                var tile1 = tiles.FirstOrDefault(x => x.Id == _selectedTile1.Id && x.Number == _selectedTile1.Number);
                var tile2 = tiles.FirstOrDefault(x => x.Id == _selectedTile2.Id && x.Number == _selectedTile2.Number);

#if DEBUG
                Console.WriteLine($"MATCHING TILES: {tile1.Id}, {tile1.Number} & {tile2.Id}, {tile2.Number}");
#endif

                if (tile1.Id == tile2.Id && tile1.Number != tile2.Number)
                {
                    _collectibleCollected++;

                    AddScore(5);
                    AddHealth(_playerHealthRejuvenationPoint);

                    SoundHelper.PlaySound(SoundType.CORRECT_MATCH);

                    tile1.MatchTile();
                    tile2.MatchTile();

#if DEBUG
                    Console.WriteLine("TILES MATCH");
#endif
                }
                else
                {
                    SoundHelper.PlaySound(SoundType.INCORRECT_MATCH);

#if DEBUG
                    Console.WriteLine("TILES DON'T MATCH");
#endif
                }

                _selectedTile1 = new(null, -1);
                _selectedTile2 = new(null, -1);
            }
            else
            {
                SoundHelper.PlaySound(SoundType.TILE_FLIP);
            }
        }

        #endregion

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
            _memoryTiles = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.MEMORYTILE).Select(x => x.Value).ToArray();
            _powerUps = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.POWERUP).Select(x => x.Value).ToArray();
        }

        private void SetGameView()
        {
#if DEBUG
            Console.WriteLine("INITIALIZING GAME");
#endif
            SetViewSize();
            SetRowsAndColumns();
        }

        private void SetRowsAndColumns()
        {
            _rows = 2;
            _columns = 2;
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
            _difficultyMultiplier = 1;

            _collectibleCollected = 0;
            ScoreText.Text = "0";

            _playerHealth = _playerHealthDefault;
            _playerHealthDepletionPoint = _healthDepletePointDefault;
            _playerHealthRejuvenationPoint = _healthGainPointDefault;
            _playerHealthDepletionCounter = 10;

            PlayerHealthBar.Foreground = new SolidColorBrush(Colors.Green);

            StartGameSounds();
            SpawnMemoryTiles();

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
            PlayerHealthBar.Value = _playerHealth;

            _memoryTilesInGame = GameView.GetGameObjects<MemoryTile>();

            DepleteHealth();
            UpdateGameObjects();
            RemoveGameObjects();

            // once all the tiles are matched move to next level and start game
            if (!_memoryTilesInGame.Any())
            {
                LevelUp();
                SpawnMemoryTiles();

                return;
            }

            // if all tiles are hidden then reset the selected tiles
            if (_memoryTilesInGame.All(x => !x.IsRevealed))
            {
                _selectedTile1 = new(null, -1);
                _selectedTile2 = new(null, -1);

                _canSelect = true;
            }
            else
            {
                if (_memoryTilesInGame.Count(x => x.IsRevealed) == 2)
                    _canSelect = false;
            }

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
            NavigateToPage(typeof(GameOverPage));
        }

        #endregion

        #region GameObject

        private void UpdateGameObjects()
        {
            foreach (GameObject x in GameView.GetGameObjects<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.MEMORYTILE:
                        {
                            UpdateMemoryTile(x as MemoryTile);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void RemoveGameObjects()
        {
            GameView.RemoveDestroyableGameObjects();
        }

        #endregion

        #region Health

        private void AddHealth(double health)
        {
            if (_playerHealth < 100)
            {
                if (_playerHealth + health > 100)
                    health = _playerHealth + health - 100;

                _playerHealth += health;
            }
        }

        private void DepleteHealth()
        {
            _playerHealthDepletionCounter--;

            if (_playerHealthDepletionCounter <= 0)
            {
                _playerHealth -= _playerHealthDepletionPoint;
                _playerHealthDepletionCounter = 10;
            }

            if (_playerHealth < _playerHealthDefault / 3)
            {
                PlayerHealthBar.Foreground = new SolidColorBrush(Colors.Crimson);
            }
            else if (_playerHealth < _playerHealthDefault / 2)
            {
                PlayerHealthBar.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else if (_playerHealth > _playerHealthDefault / 2)
            {
                PlayerHealthBar.Foreground = new SolidColorBrush(Colors.Green);
            }

            if (_playerHealth <= 0)
                GameOver();
        }

        #endregion

        #region MemoryTile

        private void SpawnMemoryTiles()
        {
            _selectedTile1 = new(null, -1);
            _selectedTile2 = new(null, -1);

            CreateMemoryTiles();
            ShuffleMemoryTiles();
            SetMemoryTiles();
            SetViewSizeFromTiles();
        }

        private void UpdateMemoryTile(MemoryTile memoryTile)
        {
            memoryTile.AnimateTile();

            if (memoryTile.HasFaded)
            {
                memoryTile.PointerPressed -= MemoryTile_PointerPressed;
                GameView.AddDestroyableGameObject(memoryTile);
            }
        }

        private void SetMemoryTiles()
        {
            GameView.Children.Clear();

            int tileNum = 0;

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    MemoryTile memoryTile = _createdMemoryTiles[tileNum];

                    memoryTile.Number = tileNum;

                    memoryTile.SetTop((memoryTile.Height + 5) * i);
                    memoryTile.SetLeft((memoryTile.Width + 5) * j);

                    memoryTile.PointerPressed += MemoryTile_PointerPressed;

                    GameView.Children.Add(memoryTile);

                    tileNum++;
                }
            }
        }

        private void CreateMemoryTiles()
        {
            _memoryTilePairsCount = (_rows * _columns) / 2;

            _createdMemoryTiles.Clear();

            var _tileStart = _random.Next(0, _memoryTiles.Length / 2);

            for (int i = 0; i < _memoryTilePairsCount; i++)
            {
                _markNum = _tileStart + i;

                var id = Guid.NewGuid().ToString();

                var memoryTile = new MemoryTile(_scale) { Id = id };
                memoryTile.SetTileContent(_memoryTiles[_markNum]);

                var memoryTileMatch = new MemoryTile(_scale) { Id = id };
                memoryTileMatch.SetTileContent(_memoryTiles[_markNum]);

                _createdMemoryTiles.Add(memoryTile);
                _createdMemoryTiles.Add(memoryTileMatch);
            }

#if DEBUG
            Console.WriteLine(_createdMemoryTiles);
#endif
        }

        private void ShuffleMemoryTiles()
        {
            //Shuffle memory tiles
            for (int i = 0; i < 64; i++)
            {
                _createdMemoryTiles.Reverse();
                _createdMemoryTiles.Move(_random.Next(0, _createdMemoryTiles.Count), _random.Next(0, _createdMemoryTiles.Count));
            }
        }

        #endregion

        #endregion

        #region Score

        private void AddScore(double score)
        {
            _score += score;
        }

        #endregion

        #region Difficulty

        private void LevelUp()
        {
            _playerHealth = _playerHealthDefault;

            _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * _difficultyMultiplier;
            _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * _difficultyMultiplier;
            _gameSpeed = _gameSpeedDefault + 0.2 * _difficultyMultiplier;

            _difficultyMultiplier++;

            // increase columns up to 4
            if (_columns < 4)
                _columns++;

            // increase rows upto 5
            if (_columns == 4 && _rows < 5)
                _rows++;

            SoundHelper.PlaySound(SoundType.LEVEL_UP);

#if DEBUG
            Console.WriteLine($"GAME SPEED: {_gameSpeed}");
#endif

        }

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

            if (_createdMemoryTiles.Count > 0 && !_isGameOver)
            {
                SetViewSizeFromTiles();
            }
            else
            {
                GameView.Width = _windowWidth <= 400 ? 300 : _windowWidth / 3;
                GameView.Height = _windowHeight < 700 ? 600 : _windowHeight / 1.5;
            }
#if DEBUG
            Console.WriteLine($"SCALE: {_scale}");
#endif
        }

        private void SetViewSizeFromTiles()
        {
            GameView.Height = (_rows * Constants.TILE_SIZE * _scale) + ((2.5 * _scale) * _createdMemoryTiles.Count);
            GameView.Width = (_columns * Constants.TILE_SIZE * _scale) + ((2.5 * _scale) * _createdMemoryTiles.Count);
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
