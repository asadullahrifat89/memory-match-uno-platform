﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Uno.Extensions;

namespace MemoryMatchingGame
{
    public sealed partial class StartPage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private double _windowHeight, _windowWidth;
        private double _scale;

        private readonly int _gameSpeed = 5;

        private int _markNum;

        private Uri[] _memoryTiles;

        private readonly IBackendService _backendService;

        #endregion

        #region Ctor

        public StartPage()
        {
            InitializeComponent();
            _backendService = (Application.Current as App).Host.Services.GetRequiredService<IBackendService>();

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            Loaded += GamePlayPage_Loaded;
            Unloaded += GamePlayPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private async void GamePlayPage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += GamePlayPage_SizeChanged;
            StartAnimation();

            await AppSettingsHelper.LoadAppSettings();

            LocalizationHelper.CheckLocalizationCache();
            await LocalizationHelper.LoadLocalizationKeys(() =>
            {
                this.SetLocalization();

                SoundHelper.LoadGameSounds(() =>
                {
                    StartGameSounds();
                    AssetHelper.PreloadAssets(progressBar: ProgressBar, messageBlock: ProgressBarMessageBlock, completed: () =>
                    {
                        PlayButton.IsEnabled = true;
                    });
                });
            });

            if (await GetCompanyBrand())
                await CheckUserSession();
        }

        private void GamePlayPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePlayPage_SizeChanged;
            StopAnimation();
        }

        private void GamePlayPage_SizeChanged(object sender, SizeChangedEventArgs args)
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

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string tag)
            {
                SoundHelper.PlaySound(SoundType.MENU_SELECT);

                LocalizationHelper.CurrentCulture = tag;

                if (CookieHelper.IsCookieAccepted())
                    LocalizationHelper.SaveLocalizationCache(tag);

                this.SetLocalization();
            }
        }

        private void HowToPlayButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(HowToPlayPage));
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameProfileHelper.HasUserLoggedIn() ? await GenerateSession() : true)
                NavigateToPage(typeof(GamePlayPage));
        }

        private void LeaderboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(LeaderboardPage));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(LoginPage));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLogout();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(SignUpPage));
        }

        private void CookieAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            CookieHelper.SetCookieAccepted();
            CookieToast.Visibility = Visibility.Collapsed;
            LocalizationHelper.SaveLocalizationCache(LocalizationHelper.CurrentCulture);
        }

        private void CookieDeclineButton_Click(object sender, RoutedEventArgs e)
        {
            CookieHelper.SetCookieDeclined();
            CookieToast.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

        #region Methods

        #region Logic

        private async Task<bool> GetCompanyBrand()
        {
            // if company is not already fetched, fetch it
            if (CompanyHelper.Company is null)
            {
                (bool IsSuccess, string Message, Company Company) = await _backendService.GetCompanyBrand();

                if (!IsSuccess)
                {
                    var error = Message;
                    this.ShowError(error);
                    return false;
                }

                if (Company is not null)
                    CompanyHelper.Company = Company;
            }

            if (CompanyHelper.Company is not null)
            {
                if (!CompanyHelper.Company.WebSiteUrl.IsNullOrBlank())
                    BrandButton.NavigateUri = new Uri(CompanyHelper.Company.WebSiteUrl);

                if (!CookieHelper.IsCookieAccepted() && !CompanyHelper.Company.DefaultLanguage.IsNullOrBlank())
                {
                    LocalizationHelper.CurrentCulture = CompanyHelper.Company.DefaultLanguage;
                    this.SetLocalization();
                }
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

        private async Task CheckUserSession()
        {
            AuthTokenHelper.TryLoadRefreshToken();

            if (GameProfileHelper.HasUserLoggedIn())
            {
                SetLogoutContext();
            }
            else
            {
                if (!AuthTokenHelper.RefreshToken.IsNullOrEmpty() && await GetGameProfile())
                {
                    SetLogoutContext();
                    ShowWelcomeBackToast();
                }
                else
                {
                    SetLoginContext();
                    ShowCookieToast();
                }
            }
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

        private void PerformLogout()
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            AuthTokenHelper.RemoveCachedRefreshToken();

            AuthTokenHelper.AuthToken = null;
            AuthTokenHelper.RefreshToken = null;

            GameProfileHelper.GameProfile = null;
            PlayerScoreHelper.PlayerScore = null;

            SetLoginContext();
        }

        private void ShowCookieToast()
        {
            if (!CookieHelper.IsCookieAccepted())
                CookieToast.Visibility = Visibility.Visible;
        }

        private void SetLogoutContext()
        {
            LogoutButton.Visibility = Visibility.Visible;
            LeaderboardButton.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Collapsed;
            RegisterButton.Visibility = Visibility.Collapsed;
        }

        private void SetLoginContext()
        {
            LogoutButton.Visibility = Visibility.Collapsed;
            LeaderboardButton.Visibility = Visibility.Collapsed;
            LoginButton.Visibility = Visibility.Visible;
            RegisterButton.Visibility = Visibility.Visible;
        }

        private async void ShowWelcomeBackToast()
        {
            SoundHelper.PlaySound(SoundType.POWER_UP);
            UserName.Text = GameProfileHelper.GameProfile.User.UserName;

            WelcomeBackToast.Opacity = 1;
            await Task.Delay(TimeSpan.FromSeconds(5));
            WelcomeBackToast.Opacity = 0;
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
            if (pageType == typeof(GamePlayPage))
                SoundHelper.StopSound(SoundType.INTRO);

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);

            App.EnterFullScreen(true);
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
