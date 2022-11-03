using System;
using System.Collections.Generic;

namespace MemoryMatchingGame
{
    public static class Constants
    {
        public const string GAME_ID = "memory-match";

        #region Measurements

        public const double DEFAULT_FRAME_TIME = 18;

        public const double CARD_SIZE = 80;
        public const double POWERUP_SIZE = 80;

        #endregion

        #region Images

        public static KeyValuePair<ElementType, Uri>[] ELEMENT_TEMPLATES = new KeyValuePair<ElementType, Uri>[]
        {
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card2.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card3.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card4.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card5.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card6.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card7.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card8.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card9.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card11.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card12.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card13.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card14.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.CARD, new Uri("ms-appx:///Assets/Images/card15.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.POWERUP, new Uri("ms-appx:///Assets/Images/powerup1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.POWERUP, new Uri("ms-appx:///Assets/Images/powerup2.png")),
        };

        #endregion

        #region Sounds

        public static KeyValuePair<SoundType, string>[] SOUND_TEMPLATES = new KeyValuePair<SoundType, string>[]
        {
            new KeyValuePair<SoundType, string>(SoundType.MENU_SELECT, "Assets/Sounds/menu-select.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro2.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background3.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.GAME_OVER, "Assets/Sounds/game-over.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.POWER_UP, "Assets/Sounds/power-up.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.POWER_DOWN, "Assets/Sounds/power-down.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.CARD_FLIP, "Assets/Sounds/card_flip.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.CORRECT_MATCH, "Assets/Sounds/correct_match.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.INCORRECT_MATCH, "Assets/Sounds/incorrect_match.mp3"),
        };

        #endregion

        #region Web Api Base Urls
#if DEBUG
        public const string GAME_API_BASEURL = "https://localhost:7238";
#else
        public const string GAME_API_BASEURL = "https://selise-space-shooter-backend.seliselocal.com";
#endif
        #endregion

        #region Web Api Endpoints

        public const string Action_Ping = "/api/Query/Ping";

        public const string Action_Authenticate = "/api/Command/Authenticate";
        public const string Action_SignUp = "/api/Command/SignUp";
        public const string Action_SubmitGameScore = "/api/Command/SubmitGameScore";
        public const string Action_GenerateSession = "/api/Command/GenerateSession";
        public const string Action_ValidateSession = "/api/Command/ValidateSession";

        public const string Action_GetGameProfile = "/api/Query/GetGameProfile";
        public const string Action_GetGameProfiles = "/api/Query/GetGameProfiles";
        public const string Action_GetGameScores = "/api/Query/GetGameScores";
        public const string Action_GetUser = "/api/Query/GetUser";
        public const string Action_CheckIdentityAvailability = "/api/Query/CheckIdentityAvailability";

        #endregion

        #region Session Keys

        public const string CACHE_SESSION_KEY = "Session";
        public const string CACHE_LANGUAGE_KEY = "Language";

        #endregion

        #region Cookie Keys

        public const string COOKIE_KEY = "Cookie";
        public const string COOKIE_ACCEPTED_KEY = "Accepted";

        #endregion
    }
}
