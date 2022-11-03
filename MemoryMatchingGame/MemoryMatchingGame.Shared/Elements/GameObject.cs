using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Foundation;

namespace MemoryMatchingGame
{
    public class GameObject : Border
    {
        #region Fields

        private readonly Image _content = new() { Stretch = Stretch.Uniform, Visibility = Microsoft.UI.Xaml.Visibility.Collapsed };

        #endregion

        #region Properties

        public bool HasFaded => Opacity <= 0;

        public bool HasAppeared => Opacity >= 1;

        #endregion

        #region Ctor

        public GameObject()
        {
            Child = _content;
            CanDrag = false;
        }

        #endregion        

        #region Methods

        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double GetTop()
        {
            return Canvas.GetTop(this);
        }

        public double GetLeft()
        {
            return Canvas.GetLeft(this);
        }

        public void SetTop(double top)
        {
            Canvas.SetTop(this, top);
        }

        public void SetLeft(double left)
        {
            Canvas.SetLeft(this, left);
        }

        public void SetZ(int z)
        {
            Canvas.SetZIndex(this, z);
        }

        public void SetPosition(double left, double top)
        {
            Canvas.SetTop(this, top);
            Canvas.SetLeft(this, left);
        }

        public void SetContent(Uri uri)
        {
            _content.Source = new BitmapImage(uri);
            _content.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }

        public void HideContent()
        {
            _content.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        public void Fade()
        {
            Opacity -= 0.1;
        }

        public void Appear()
        {
            Opacity += 0.1;
        }

        #endregion
    }

    public enum ElementType
    {
        NONE,
        MEMORYTILE,
        POWERUP,
    }
}
