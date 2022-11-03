using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryMatchingGame
{
    public class MemoryTile : GameObject
    {
        #region Ctor

        public MemoryTile(double scale)
        {
            Tag = ElementType.MEMORYTILE;

            Height = Constants.CARD_SIZE * scale;
            Width = Constants.CARD_SIZE * scale;

            Style = App.Current.Resources["CardStyle"] as Style;
        }

        #endregion

        #region Properties

        public int Id { get; set; }

        #endregion
    }

    public enum PowerUpType
    {
        TimeLapse,
        ScoreMultiplier,
    }
}
