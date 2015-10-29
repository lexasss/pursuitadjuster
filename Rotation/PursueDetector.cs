using System;
using System.Text;
using System.Drawing;

namespace SmoothPursuit.Rotation
{
    /*
    internal class PursueDetector : IPursueDetector
    {
        #region Declarations

        private class Ray : DataPoint
        {
            private static int AngleCycle = 0;
            private static Angle LastAngle = new Angle();

            public double Length;
            public Angle Angle;

            public Ray(int aTimestamp, Point aPoint, Point aCenter)
                : base(aTimestamp)
            {
                int dx = aPoint.X - aCenter.X;
                int dy = aPoint.Y - aCenter.Y;
                Length = Math.Sqrt(dx * dx + dy * dy);
                Angle = new Angle(Math.Atan2(dy, dx)).rotateBy(AngleCycle).keepCloseTo(LastAngle, ref AngleCycle);

                LastAngle = new Angle(Angle.Radians, Angle.Cycles);
            }

            public bool isInRange(double aMinLength, double aMaxLength)
            {
                return aMinLength <= Length && Length <= aMaxLength;
            }

            public override string ToString()
            {
                return new StringBuilder().
                    AppendFormat("\t{0,8:N3}", Angle.Degrees).
                    AppendFormat("\t{0,8:N1}", Length).
                    ToString();
            }
        }

        private class GazeTrack : SpeedTrack
        {
            public Angle Angle { get; private set; }
            // Speed is in rad / sec

            public GazeTrack(Ray aFirst, Ray aLast, double aExpectedSpeed)
                : base(aFirst, aLast, aExpectedSpeed)
            {
                Angle = (aLast.Angle - aFirst.Angle).normalize();
            }

            public override string ToString()
            {
                return new StringBuilder(base.ToString()).
                    AppendFormat("\t{0,8:N3}", Angle.Degrees).
                    ToString();
            }

            protected override double GetLength()
            {
                return Angle.Radians;
            }
        }

        #endregion

        #region Consts

        private const double RADIUS_ERROR_THRESHOLD = 0.2;  // fraction

        #endregion

        #region Internal members

        private Point iCenter;
        private double iRadius;             // pixels
        private double iExpectedSpeed;

        #endregion 

        #region Public methods

        // aExpectedSpeed = rad / sec
        public PursueDetector(int aCenterX, int aCenterY, double aRadius, double aExpectedSpeed)
            : base()
        {
            iCenter = new Point(aCenterX, aCenterY);
            iRadius = aRadius;
            iExpectedSpeed = aExpectedSpeed;

            //Console.WriteLine("Radius: {0} [{1} - {2}]", iRadius, iRadius * (1.0 - RADIUS_ERROR_THRESHOLD), iRadius * (1.0 + RADIUS_ERROR_THRESHOLD));
        }

        #endregion

        #region Internal methods

        protected override DataPoint CreateDataPoint(int aTimestamp, Point aPoint)
        {
            Ray newDataPoint = new Ray(aTimestamp, aPoint, iCenter);
            if (newDataPoint.isInRange(iRadius * (1.0 - RADIUS_ERROR_THRESHOLD), iRadius * (1.0 + RADIUS_ERROR_THRESHOLD)))
            {
                return newDataPoint;
            }

            return null;
        }

        protected override Track CreateTrack(DataPoint aFirstDataPoint, DataPoint aLastDataPoint)
        {
            return new GazeTrack((Ray)aFirstDataPoint, (Ray)aLastDataPoint, iExpectedSpeed);
        }

        #endregion
    } */
}
