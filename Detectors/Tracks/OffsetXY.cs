using System;
using System.Drawing;
using System.Text;

namespace SmoothPursuit.Detectors.Tracks
{
    public class MovementStatsXY : MovementStats
    {
        public double STD { get; private set; }

        public override void compute(Point[] aOffsets)
        {
            STD = ComputeSTD(aOffsets);
        }

        private double ComputeSTD(Point[] aBuffer)
        {
            int count = aBuffer.Length;
            if (count == 0)
                return 1000000.0;

            double x = 0;
            double y = 0;

            foreach (Point offset in aBuffer)
            {
                x += offset.X;
                y += offset.Y;
            }

            x /= count;
            y /= count;

            double dist = 0;
            foreach (Point offset in aBuffer)
            {
                double dx = offset.X - x;
                double dy = offset.Y - y;
                dist += dx * dx + dy * dy;
            }

            return Math.Sqrt(dist / count);
        }
    }

    public class OffsetXY : Offset<MovementStatsXY, Processor<MovementStatsXY>>
    {
        #region Constants

        private const double DISTANCE_STD_THRESHOLD = 25.0;     // pixels, default=25

        #endregion

        #region Public methods

        public override string ToString()
        {
            return iProcessor == null ? "INVALID TRACK" :
                new StringBuilder(base.ToString()).
                AppendFormat("\tIDs={0:N2}", iProcessor.Increase.STD).
                AppendFormat("\tDDs={0:N2}", iProcessor.Decrease.STD).
                ToString();
        }

        #endregion

        #region Internal methods

        protected override bool IsFollowingMovement(MovementStatsXY aMovementStats)
        {
            return aMovementStats.STD < DISTANCE_STD_THRESHOLD;
        }

        protected override bool IsIncreaseCloserThanDecrease()
        {
            return iProcessor.Increase.STD < iProcessor.Decrease.STD;
        }

        #endregion
    }
}