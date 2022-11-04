using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace MemoryMatchingGame
{
    public sealed partial class LoginPage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private double _windowHeight, _windowWidth;
        private double _scale;

        private readonly int _gameSpeed = 8;

        private int _markNum;

        private Uri[] _memoryTiles;

        private readonly IBackendService _backendService;

        #endregion

        #region Ctor

        public LoginPage()
        {
            this.InitializeComponent();
            _backendService = (Application.Current as App).Host.Services.GetRequiredService<IBackendService>();

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            this.Loaded += LoginPage_Loaded;
            this.Unloaded += LoginPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetLocalization();

            SizeChanged += GamePage_SizeChanged;
            StartAnimation();

            // if user was already logged in or came here after sign up
            if (PlayerCredentialsHelper.GetCachedPlayerCredentials() is PlayerCredentials authCredentials
                && !authCredentials.UserName.IsNullOrBlank()
                && !authCredentials.Password.IsNullOrBlank())
            {
                UserNameBox.Text = authCredentials.UserName;
                PasswordBox.Text = authCredentials.Password;
            }
            else
            {
                UserNameBox.Text = null;
                PasswordBox.Text = null;
            }
        }

        private void LoginPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePage_SizeChanged;
            StopAnimation();
        }

        private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            SetViewSize();

#if DEBUG
            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
#endif
        }

        #endregion

        #region Buttons

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(StartPage));
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(SignUpPage));
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginButton.IsEnabled)
                await PerformLogin();
        }

        #endregion

        #region Input Fields

        private void UserNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableLoginButton();
        }

        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableLoginButton();
        }

        private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && LoginButton.IsEnabled)
                await PerformLogin();
        }

        #endregion

        #endregion

        #region Methods      

        #region Logic

        private async Task PerformLogin()
        {
            this.RunProgressBar();

            if (await Authenticate() && await GetGameProfile() && await GenerateSession())
            {
                if (PlayerScoreHelper.GameScoreSubmissionPending)
                {
                    if (await SubmitScore())
                        PlayerScoreHelper.GameScoreSubmissionPending = false;
                }

                this.StopProgressBar();

                NavigateToPage(typeof(LeaderboardPage));
            }
        }

        private async Task<bool> Authenticate()
        {
            (bool IsSuccess, string Message) = await _backendService.AuthenticateUser(
                userNameOrEmail: UserNameBox.Text.Trim(),
                password: PasswordBox.Text.Trim());

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        private async Task<bool> GetGameProfile()
        {
            (bool IsSuccess, string Message, _) = await _backendService.GetUserGameProfile();

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        private async Task<bool> GenerateSession()
        {
            (bool IsSuccess, string Message) = await _backendService.GenerateUserSession();

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        private async Task<bool> SubmitScore()
        {
            (bool IsSuccess, string Message) = await _backendService.SubmitUserGameScore(PlayerScoreHelper.PlayerScore.Score);

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        private void EnableLoginButton()
        {
            LoginButton.IsEnabled = !UserNameBox.Text.IsNullOrBlank() && !PasswordBox.Text.IsNullOrBlank();
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

            UnderView.Width = _windowWidth;
            UnderView.Height = _windowHeight;
        }

        private void NavigateToPage(Type pageType)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region Animation

        #region Game

        private void PopulateGameViews()
        {
#if DEBUG
            Console.WriteLine("INITIALIZING GAME");
#endif
            SetViewSize();
            PopulateUnderView();
        }

        private void LoadGameElements()
        {
            _memoryTiles = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.MEMORYTILE).Select(x => x.Value).ToArray();
        }

        private void PopulateUnderView()
        {
            // add some clouds underneath
            for (int i = 0; i < 10; i++)
            {
                SpawnMemoryTile();
            }
        }

        private void StartAnimation()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif      
            RecycleGameObjects();
            RunGame();
        }

        private void RecycleGameObjects()
        {
            foreach (GameObject x in UnderView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.MEMORYTILE:
                        {
                            RecyleMemoryTile(x);
                        }
                        break;
                    default:
                        break;
                }
            }
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
            UpdateGameObjects();
        }

        private void UpdateGameObjects()
        {
            foreach (GameObject x in UnderView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.MEMORYTILE:
                        {
                            UpdateMemoryTile(x);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void StopAnimation()
        {
            _gameViewTimer?.Dispose();
        }

        #endregion

        #region MemoryTile

        private void SpawnMemoryTile()
        {
            MemoryTile mmoryTile = new(_scale);
            RandomizeMemoryTilePosition(mmoryTile);
            UnderView.Children.Add(mmoryTile);
        }

        private void UpdateMemoryTile(GameObject memoryTile)
        {
            memoryTile.SetLeft(memoryTile.GetLeft() + _gameSpeed);

            if (memoryTile.GetLeft() > UnderView.Width)
                RecyleMemoryTile(memoryTile);
        }

        private void RecyleMemoryTile(GameObject memoryTile)
        {
            _markNum = _random.Next(0, _memoryTiles.Length);
            memoryTile.SetContent(_memoryTiles[_markNum]);
            RandomizeMemoryTilePosition(memoryTile);
        }

        private void RandomizeMemoryTilePosition(GameObject memoryTile)
        {
            memoryTile.SetPosition(
                left: _random.Next(0, (int)UnderView.Width) * -1,
                top: _random.Next(0, (int)UnderView.Height));
        }

        #endregion

        #region Sound

        private void StartGameSounds()
        {
            SoundHelper.RandomizeSound(SoundType.INTRO);
            SoundHelper.PlaySound(SoundType.INTRO);
        }

        #endregion        

        #endregion

        #endregion
    }
}
