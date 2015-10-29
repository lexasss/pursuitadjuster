using System.Collections.Generic;
using System.Drawing;

namespace SmoothPursuit.Detectors
{
    public class Offset : IPursueDetector
    {
        public Offset(ICue aCueIncrease, ICue aCueDecrease)
            : base(aCueIncrease, aCueDecrease) { }

        #region Internal methods

        protected override DataPoint CreateDataPoint(int aTimestamp, Point aPoint)
        {
            Point cueIncrease = iCueIncrease.Location;
            Point cueDecrease = iCueDecrease.Location;
            return new OffsetGazePoint(aTimestamp, aPoint,
                new Point(aPoint.X - cueIncrease.X, aPoint.Y - cueIncrease.Y),
                new Point(aPoint.X - cueDecrease.X, aPoint.Y - cueDecrease.Y));
        }

        protected override Track CreateTrack(DataPoint aFirstDataPoint, DataPoint aLastDataPoint)
        {
            List<OffsetGazePoint> gazePoints = new List<OffsetGazePoint>();
            foreach (DataPoint point in iDataBuffer.ToArray())
            {
                gazePoints.Add((OffsetGazePoint)point);
            }

            return new GazeTrack(gazePoints.ToArray());
        }

        #endregion
    }
}
