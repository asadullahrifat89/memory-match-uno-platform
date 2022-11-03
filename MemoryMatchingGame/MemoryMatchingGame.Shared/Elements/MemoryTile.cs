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

            Height = Constants.TILE_SIZE * scale;
            Width = Constants.TILE_SIZE * scale;

            Style = App.Current.Resources["CardStyle"] as Style;
        }

        #endregion

        #region Properties

        public int Id { get; set; }

        public bool IsViewing { get; set; }

        #endregion
    }

    public enum PowerUpType
    {
        TimeLapse,
        ScoreMultiplier,
    }
}
