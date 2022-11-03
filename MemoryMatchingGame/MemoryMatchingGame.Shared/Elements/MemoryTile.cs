using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace MemoryMatchingGame
{
    public class MemoryTile : GameObject
    {
        #region Fields

        private GameObject _hiddenObject;

        private int _revealTileCounter;
        private readonly int _revealTileCounterDefault = 100;

        private int _matchTileCounter;
        private readonly int _matchTileCounterDefault = 50;

        private bool _isRevealed;
        private bool _hasMatched;

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
        }

        #endregion

        #region Properties

        public string Id { get; set; }

        public int Number { get; set; } = 0;

        #endregion

        #region Methods

        public void SetTileContent(Uri uri)
        {
            _hiddenObject.SetContent(uri);
            Child = _hiddenObject;
        }

        public void MatchTile()
        {
            _hasMatched = true;
            _matchTileCounter = _matchTileCounterDefault;
        }

        public void RevealTile()
        {
            _isRevealed = true;
            _revealTileCounter = _revealTileCounterDefault;
        }

        public void AnimateTile()
        {
            if (_hasMatched)
            {
                _matchTileCounter--;

                // once matched fade the tile
                if (_matchTileCounter <= 0 && !HasFaded)
                    Fade();
            }
            else
            {
                // on reveal appear the hidden tile inside
                if (_isRevealed)
                {
                    if (!_hiddenObject.HasAppeared)
                        _hiddenObject.Appear();

                    _revealTileCounter--;

                    if (_revealTileCounter <= 0)
                        _isRevealed = false;
                }
                else
                {
                    if (!_hiddenObject.HasFaded)
                        _hiddenObject.Fade();
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
