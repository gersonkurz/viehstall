using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace viehstall
{
    internal class GestureHandling
    {
        public delegate void GestureEvent();

        public event GestureEvent LeftSwipe;
        public event GestureEvent RightSwipe;
        public event GestureEvent UpSwipe;
        public event GestureEvent DownSwipe;

        public GestureHandling(UIElement control)
        {
            control.ManipulationMode = ManipulationModes.All;
            control.ManipulationStarted += OnManipulationStarted;
            control.ManipulationCompleted += OnManipulationCompleted;
        }

        

        private Point InitialPoint;

        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            InitialPoint = e.Position;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Point currentPoint = e.Position;
            double deltaX = currentPoint.X - InitialPoint.X;
            double deltaY = currentPoint.Y - InitialPoint.Y;

            
            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                if (currentPoint.X > InitialPoint.X)
                {
                    if (RightSwipe != null)
                        RightSwipe();
                }
                else
                {
                    if( LeftSwipe != null )
                        LeftSwipe();
                }

            }
            else
            {
                if (currentPoint.Y > InitialPoint.Y)
                {
                    if (DownSwipe != null)
                        DownSwipe();
                }
                else
                {
                    if( UpSwipe != null )
                        UpSwipe();
                }
            }
        }

    }
}
