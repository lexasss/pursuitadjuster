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

        private double iLattitude;
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
            SetLattitude(iInitialPosition.X);
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

        private void SetLattitude(double aX)
        {
            iLattitude = aX;
            Location = new Point(
                (int)Math.Round((double)(iLattitude - iBitmapCenter.X)),
                (int)Math.Round(iInitialPosition.Y - iBitmapCenter.Y - 8)); // 8 - a hack to show the cue higher
        }

        private void SlideBy(double aDistance)
        {
            double x = iLattitude + aDistance;

            if (x > iPathRect.Right)
            {
                x = iPathRect.Left;
            }
            if (x < iPathRect.Left)
            {
                x = iPathRect.Right;
            }

            SetLattitude(x);
        }

        #endregion
    }
}
