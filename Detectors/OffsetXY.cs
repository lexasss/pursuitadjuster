using System.Collections.Generic;
using System.Drawing;

namespace SmoothPursuit.Detectors
{
    public class OffsetXY : IPursueDetector
    {
        public OffsetXY(ICue aCueIncrease, ICue aCueDecrease)
            : base(aCueIncrease, aCueDecrease) { }

        #region Internal methods

        protected override Points.Data CreateDataPoint(int aTimestamp, Point aPoint)
        {
            Point cueIncrease = iCueIncrease.Location;
            Point cueDecrease = iCueDecrease.Location;
            return new Points.Offset(aTimestamp, aPoint,
                new Point(aPoint.X - cueIncrease.X, aPoint.Y - cueIncrease.Y),
                new Point(aPoint.X - cueDecrease.X, aPoint.Y - cueDecrease.Y));
        }

        protected override Tracks.Track CreateTrack(Points.Data aFirstDataPoint, Points.Data aLastDataPoint)
        {
            List<Points.Offset> gazePoints = new List<Points.Offset>();
            foreach (Points.Data point in iDataBuffer.ToArray())
            {
                gazePoints.Add((Points.Offset)point);
            }

            return new Tracks.OffsetXY(gazePoints.ToArray());
        }

        #endregion
    }
}
