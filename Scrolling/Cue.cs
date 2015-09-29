using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume.Scrolling
{
    internal class Cue : ICue
    {
        #region Internal members

        private double iAltitude;
        private Point iInitialPosition;
        private Rectangle iPathRect;

        private readonly PointF iBitmapCenter;

        #endregion

        #region Public methods

        public Cue(Bitmap aBitmap, double aSpeed, Rectangle aPathRect)
            : base(aBitmap, aSpeed)
        {
            iBitmapCenter = new PointF(aBitmap.Width / 2, aBitmap.Height / 2);
            iPathRect = aPathRect;
            iInitialPosition = new Point(
                iSpeed > 0 ? aPathRect.Left : aPathRect.Right,
                iSpeed > 0 ? aPathRect.Top : aPathRect.Bottom);
        }

        #endregion

        #region Internal methods

        protected override void SetInitialLocation()
        {
            SetAltitude(iInitialPosition.Y);
        }

        protected override void UpdateLocation()
        {
            double speed = iSpeed;
            if (iStepCounter < ACCELERATION_STEPS)
            {
                speed = iSpeed * (Math.Abs((double)iStepCounter) / ACCELERATION_STEPS);
            }

            SlideBy(speed);
        }

        private void SetAltitude(double aY)
        {
            iAltitude = aY;
            Location = new Point(
                (int)Math.Round((double)(iInitialPosition.X - iBitmapCenter.X)),
                (int)Math.Round(iAltitude - iBitmapCenter.Y));
        }

        private void SlideBy(double aDistance)
        {
            double y = iAltitude + aDistance;

            if (y > iPathRect.Bottom)
            {
                y = iPathRect.Top;
            }
            if (y < iPathRect.Top)
            {
                y = iPathRect.Bottom;
            }

            SetAltitude(y);
        }

        #endregion
    }
}
