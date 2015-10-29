using System;
using System.Collections.Generic;
using System.Drawing;

namespace SmoothPursuit.Detectors
{
    public class Offset<Track, MS, P> : IPursueDetector 
        where Track : Tracks.Offset<MS, P>
        where MS : Tracks.MovementStats
        where P : Tracks.Processor<MS>
    {
        public Offset(ICue aCueIncrease, ICue aCueDecrease)
            : base(aCueIncrease, aCueDecrease)
        {
            Console.WriteLine(this);
        }

        #region Internal methods

        protected override Points.Gaze CreateGazePoint(int aTimestamp, Point aPoint)
        {
            Point cueIncrease = iCueIncrease.Location;
            Point cueDecrease = iCueDecrease.Location;
            return new Points.Offset(aTimestamp, aPoint,
                new Point(aPoint.X - cueIncrease.X, aPoint.Y - cueIncrease.Y),
                new Point(aPoint.X - cueDecrease.X, aPoint.Y - cueDecrease.Y));
        }

        protected override Tracks.Track CreateTrack(Points.Gaze aNewGazePoint)
        {
            List<Points.Offset> offsetPoints = new List<Points.Offset>();
            foreach (Points.Gaze point in iDataBuffer.ToArray())
            {
                offsetPoints.Add((Points.Offset)point);
            }

            Track track = Activator.CreateInstance<Track>();
            track.init(offsetPoints.ToArray());
            return track;
        }

        #endregion
    }

    public class OffsetA : Offset<Tracks.OffsetXY, Tracks.MovementStatsXY, Tracks.Processor<Tracks.MovementStatsXY>>
    {
        public OffsetA(ICue aCueIncrease, ICue aCueDecrease)
            : base(aCueIncrease, aCueDecrease) { }
        
        public override string ToString()
        {
            return "OffsetA";
        }
    }

    public class OffsetB : Offset<Tracks.Offset2, Tracks.MovementStats2, Tracks.Processor2>
    {
        public OffsetB(ICue aCueIncrease, ICue aCueDecrease)
            : base(aCueIncrease, aCueDecrease) { }

        public override string ToString()
        {
            return "OffsetB";
        }
    }
}
