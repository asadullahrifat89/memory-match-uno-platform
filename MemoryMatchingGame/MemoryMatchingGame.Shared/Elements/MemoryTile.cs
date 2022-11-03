using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace MemoryMatchingGame
{
    public class MemoryTile : GameObject
    {
        #region Fields

        private GameObject _hiddenObject;
        private GameObject _overlayObject;

        private Grid _content;

        private int _revealTileCounter;
        private readonly int _revealTileCounterDefault = 70;

        private int _matchTileCounter;
        private readonly int _matchTileCounterDefault = 50;

        #endregion

        #region Ctor

        public MemoryTile(double scale)
        {
            Tag = ElementType.MEMORYTILE;

            Height = Constants.TILE_SIZE * scale;
            Width = Constants.TILE_SIZE * scale;

            Style = App.Current.Resources["CardStyle"] as Style;

            _hiddenObject = new GameObject() { Height = Height, Width = Width };
            _hiddenObject.Opacity = 0;

            _overlayObject = new GameObject() { Height = Height, Width = Width };
            _overlayObject.SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key == ElementType.MEMORYTILE_OVERLAY).Value);
            _overlayObject.Opacity = 0;

            _content = new Grid();

            _content.Children.Add(_hiddenObject);
            _content.Children.Add(_overlayObject);
        }

        #endregion

        #region Properties

        public string Id { get; set; }

        public int Number { get; set; } = 0;

        public bool IsRevealed { get; set; }

        public bool HasMatched { get; set; }

        #endregion

        #region Methods

        public void SetMemoryTileContent(Uri uri)
        {
            _hiddenObject.SetContent(uri);
            _overlayObject.Opacity = 1;

            Child = _content;
        }

        public void MatchMemoryTile()
        {
            HasMatched = true;
            IsRevealed = false;
            _matchTileCounter = _matchTileCounterDefault;
            _hiddenObject.Opacity = 1;
            _overlayObject.Opacity = 0;
        }

        public void RevealMemoryTile()
        {
            IsRevealed = true;
            _revealTileCounter = _revealTileCounterDefault;
            _overlayObject.Opacity = 0;
        }

        public void AnimateMemoryTile()
        {
            if (HasMatched)
            {
                _matchTileCounter--;

                // once matched fade the parent tile that hosts the overlay and hidden tile
                if (_matchTileCounter <= 0 && !HasFaded)
                    Fade();
            }
            else
            {
                // if revealed make the hidden tile appear
                if (IsRevealed)
                {
                    if (!_hiddenObject.HasAppeared)
                        _hiddenObject.Appear();

                    _revealTileCounter--;

                    if (_revealTileCounter <= 0)
                        IsRevealed = false;
                }
                else
                {
                    if (!_hiddenObject.HasFaded)
                        _hiddenObject.Fade();
                    else
                        _overlayObject.Opacity = 1;
                }
            }
        }

        #endregion
    }

    public enum PowerUpType
    {
        TimeLapse,
        ScoreMultiplier,
    }
}
