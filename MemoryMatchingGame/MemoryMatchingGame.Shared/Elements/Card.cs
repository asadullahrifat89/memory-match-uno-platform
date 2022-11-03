using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryMatchingGame
{
    public class Card : GameObject
    {
        #region Ctor

        public Card(double scale)
        {
            Tag = ElementType.CARD;

            Height = Constants.CARD_SIZE * scale;
            Width = Constants.CARD_SIZE * scale;
        }

        #endregion
    }
}
