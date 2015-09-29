using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SmoothVolume.Rotation
{
    internal class Cue : ICue
    {
        #region Consts

        private const double RADIUS = 0.35;         // fraction of the min(width, height)
        private const double INITIAL_ANGLE = 90;    // degrees

        #endregion

        #region Internal members

        private readonly Point iCenter;
        private readonly double iRadius;
        private readonly int iBitmapWidth;
        private readonly int iBitmapHeight;
        // Speed is in degrees/step

        private Angle iAngle;                       // degrees

        #endregion

        #region Properties

        public double Radius { get { return iRadius; } }

        #endregion

        #region Public methods

        public Cue(Bitmap aBitmap, double aSpeed, Size aKnobSize)
            : base(aBitmap, aSpeed)
        {
            iCenter = new Point(aKnobSize.Width / 2, aKnobSize.Height / 2);
            iRadius = Math.Min(aKnobSize.Width, aKnobSize.Height) * RADIUS;
            iBitmapWidth = aBitmap.Width;
            iBitmapHeight = aBitmap.Height;
        }

        #endregion

        #region Internal methods

        private void SetAngle(double aAngle)
        {
            iAngle = new Angle(aAngle, true);

            double dx = iRadius * Math.Cos(iAngle.Radians);
            double dy = iRadius * Math.Sin(iAngle.Radians);

            Location = new Point(
                (int)(iCenter.X - iBitmapWidth / 2 + dx), 
                (int)(iCenter.Y - iBitmapHeight / 2 + dy));
        }

        #endregion

        protected override void SetInitialLocation()
        {
            SetAngle(INITIAL_ANGLE + 2 * iSpeed);
        }

        protected override void UpdateLocation()
        {
            double speed = iSpeed;
            if (iStepCounter < ACCELERATION_STEPS)
            {
                Angle angle = new Angle(90 * (Math.Abs((double)iStepCounter) / ACCELERATION_STEPS), true);
                speed = iSpeed * Math.Sin(angle.Radians);
            }

            SetAngle(iAngle.Degrees + speed);
        }
    }
}
