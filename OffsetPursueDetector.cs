using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothPursuit
{
    public class OffsetPursueDetector : IPursueDetector
    {
        #region Declarations

        private class GazePoint : DataPoint
        {
            public Point Location { get; private set; }
            public Point Offset1 { get; private set; }
            public Point Offset2 { get; private set; }

            public GazePoint(int aTimestamp, Point aLocation, Point aOffset1, Point aOffset2)
                : base(aTimestamp)
            {
                Location = aLocation;
                Offset1 = aOffset1;
                Offset2 = aOffset2;
            }

            public override string ToString()
            {
                return new StringBuilder().
                    AppendFormat("\t{0},{1}", Location.X, Location.Y).
                    AppendFormat("\t{0},{1}", Offset1.X, Offset1.Y).
                    AppendFormat("\t{0},{1}", Offset2.X, Offset2.Y).
                    ToString();
            }
        }

        private class GazeTrack : Track
        {
            public Point Distance { get; private set; }

            public GazeTrack(GazePoint aFirst, GazePoint aLast, double aExpectedSpeed)
                : base(aFirst, aLast, aExpectedSpeed)
            {
                Distance = new Point(aLast.Location.X - aFirst.Location.X, aLast.Location.Y - aFirst.Location.Y);
            }

            public virtual bool isSpeedInRange(double aMin, double aMax)
            {
                if (aMax > 0)   // 
                {
                }
                return true;
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

        private ICue iCue1;
        private ICue iCue2;

        #endregion

        #region Public methods

        public OffsetPursueDetector(ICue aCue1, ICue aCue2)
            : base(1)
        {
            iCue1 = aCue1;
            iCue2 = aCue2;
            VALUE_CHANGE = 1;
        }

        #endregion

        #region Internal methods

        protected override DataPoint CreateDataPoint(int aTimestamp, Point aPoint)
        {
            Point cue1 = iCue1.Location;
            Point cue2 = iCue2.Location;
            return new GazePoint(aTimestamp, aPoint,
                new Point(aPoint.X - cue1.X, aPoint.Y - cue1.Y),
                new Point(aPoint.X - cue2.X, aPoint.Y - cue2.Y));
        }

        protected override Track CreateTrack(DataPoint aFirstDataPoint, DataPoint aLastDataPoint)
        {
            return new GazeTrack((GazePoint)aFirstDataPoint, (GazePoint)aLastDataPoint, iExpectedSpeed);
        }

        #endregion
    }
}
