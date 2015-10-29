using System;
using System.Drawing;
using System.Text;

namespace SmoothPursuit.Detectors.Tracks
{
    internal class OffsetXY : Track
    {
        #region Declarations

        protected class Processor
        {
            public double DecreaseSTD { get; private set; }
            public double IncreaseSTD { get; private set; }

            public Processor(Points.Offset[] aBuffer)
            {
                Point[] decreaseOffsets = new Point[aBuffer.Length];
                Point[] increaseOffsets = new Point[aBuffer.Length];

                int i = 0;
                foreach (Points.Offset offset in aBuffer)
                {
                    decreaseOffsets[i] = offset.OffsetDecrease;
                    increaseOffsets[i] = offset.OffsetIncrease;
                    i++;
                }

                DecreaseSTD = ComputeSTD(decreaseOffsets);
                IncreaseSTD = ComputeSTD(increaseOffsets);
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

        #endregion

        #region Constants

        private const double DISTANCE_STD_THRESHOLD = 15.0;     // pixels, default=25
        private const int CONFLICTING_PURSUING_TOLERANCE = 7;   // number of samples

        #endregion

        #region Internal members

        private static State sLastState = State.Unknown;
        private static int sConflictringDetectionCount = 0;

        private int iDataCount = 0;
        private Processor iProcessor;

        #endregion

        #region Public methods

        public OffsetXY(Points.Offset[] aBuffer)
            : base(aBuffer[0], aBuffer[aBuffer.Length - 1])
        {
            Points.Offset first = aBuffer[0];
            Points.Offset last = aBuffer[aBuffer.Length - 1];

            if (first != null && last != null)
            {
                iDataCount = aBuffer.Length;
                iProcessor = new Processor(aBuffer);
                sLastState = ComputeState();
            }
        }

        public override string ToString()
        {
            return iProcessor == null ? "INVALID TRACK" :
                new StringBuilder(base.ToString()).
                AppendFormat("\tC={0}", iDataCount).
                AppendFormat("\tCDC={0}", sConflictringDetectionCount).
                AppendFormat("\tIDs={0:N2}", iProcessor.IncreaseSTD).
                AppendFormat("\tDDs={0:N2}", iProcessor.DecreaseSTD).
                ToString();
        }

        #endregion

        #region Internal methods

        private State ComputeState()
        {
            bool isDecreasing = iProcessor.DecreaseSTD < DISTANCE_STD_THRESHOLD;
            bool isIncreasing = iProcessor.IncreaseSTD < DISTANCE_STD_THRESHOLD;

            int conflictringDetectionCount = sConflictringDetectionCount;
            sConflictringDetectionCount = 0;

            if (isIncreasing && isDecreasing)
            {
                State = sLastState;
                State newState = iProcessor.IncreaseSTD < iProcessor.DecreaseSTD ? State.Increase : State.Decrease;

                bool isJumpedToAnotherCue = State != State.Unknown && State != newState;
                if (isJumpedToAnotherCue)
                {
                    if (conflictringDetectionCount > CONFLICTING_PURSUING_TOLERANCE)
                    {
                        conflictringDetectionCount = 0;
                        State = newState;
                    }
                    sConflictringDetectionCount = conflictringDetectionCount + 1;
                }
            }
            else if (isIncreasing)
            {
                State = State.Increase;
            }
            else if (isDecreasing)
            {
                State = State.Decrease;
            }

            return State;
        }

        #endregion
    }
}
