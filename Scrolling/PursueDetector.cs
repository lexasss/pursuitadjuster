using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume.Scrolling
{
    internal class PursueDetector : IPursueDetector
    {
        #region Declarations

        private class GazePoint : DataPoint
        {
            public Point Location { get; private set; }

            public GazePoint(int aTimestamp, Point aLocation)
                : base(aTimestamp)
            {
                Location = aLocation;
            }

            public bool isOnSlide(Rectangle aRect)
            {
                return aRect.Contains(Location);
            }

            public override string ToString()
            {
                return new StringBuilder().
                    AppendFormat("\t{0}", Location.X).
                    AppendFormat("\t{0}", Location.Y).
                    ToString();
            }
        }

        private class GazeTrack : Track
        {
            public Point Distance { get; private set; }

            public GazeTrack(GazePoint aFirst, GazePoint aLast)
                : base(aFirst, aLast)
            {
                Distance = new Point(aLast.Location.X - aFirst.Location.X, aLast.Location.Y - aFirst.Location.Y);
            }

            public override string ToString()
            {
                return new StringBuilder(base.ToString()).
                    AppendFormat("\t{0}", Distance).
                    ToString();
            }

            protected override double GetLength()
            {
                return Distance.X;
            }
        }

        #endregion

        #region Consts

        private const int MAPPING_PRECISION = 40;           // pixels

        #endregion

        #region Internal members

        private Rectangle iSlideRect;

        #endregion

        #region Public methods

        public PursueDetector(Rectangle aSlideRect, double aExpectedSpeed)  // aExpectedSpeed = pixels / sec
            : base(aExpectedSpeed)
        {
            iSlideRect = aSlideRect;
            iSlideRect.Inflate(MAPPING_PRECISION, MAPPING_PRECISION);

            VALUE_CHANGE = 1;
        }

        #endregion

        #region Internal methods

        protected override DataPoint CreateDataPoint(int aTimestamp, Point aPoint)
        {
            GazePoint newDataPoint = new GazePoint(aTimestamp, aPoint);
            if (newDataPoint.isOnSlide(iSlideRect))
            {
                return newDataPoint;
            }

            return null;
        }

        protected override Track CreateTrack(DataPoint aFirstDataPoint, DataPoint aLastDataPoint)
        {
            return new GazeTrack((GazePoint)aFirstDataPoint, (GazePoint)aLastDataPoint);
        }

        #endregion
    }
}
