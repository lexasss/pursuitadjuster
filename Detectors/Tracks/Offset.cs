using System;
using System.Drawing;
using System.Text;

namespace SmoothPursuit.Detectors.Tracks
{
    public abstract class MovementStats
    {
        public MovementStats() { }

        public abstract void compute(Point[] aOffsets);
    }

    public class Processor<T> where T : MovementStats
    {
        public T Decrease { get; protected set; }
        public T Increase { get; protected set; }

        public virtual void compute(Points.Offset[] aBuffer)
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

            Decrease = Activator.CreateInstance<T>();
            Decrease.compute(decreaseOffsets);
            Increase = Activator.CreateInstance<T>();
            Increase.compute(increaseOffsets);
        }
    }

    public abstract class Offset<MS, P> : Track 
        where MS : MovementStats
        where P : Processor<MS>
    {
        #region Constants

        private const int CONFLICTING_PURSUING_TOLERANCE = 7;   // number of samples

        #endregion

        #region Internal members

        protected static State sLastState = State.Unknown;
        private static int sConflictringDetectionCount = 0;

        protected int iDataCount = 0;
        protected P iProcessor;

        #endregion

        #region Public methods

        public virtual void init(Points.Offset[] aBuffer)
        {
            Points.Offset first = aBuffer[0];
            Points.Offset last = aBuffer[aBuffer.Length - 1];

            if (first != null && last != null)
            {
                base.init(first, last);

                iDataCount = aBuffer.Length;
                iProcessor = Activator.CreateInstance<P>(); 
                iProcessor.compute(aBuffer);
                sLastState = ComputeState();
            }
        }

        public override string ToString()
        {
            return iProcessor == null ? "INVALID TRACK" :
                new StringBuilder(base.ToString()).
                AppendFormat("\tC={0}", iDataCount).
                AppendFormat("\tCDC={0}", sConflictringDetectionCount).
                ToString();
        }

        #endregion

        #region Internal methods

        protected abstract bool IsFollowingMovement(MS aMovementStats);
        protected abstract bool IsIncreaseCloserThanDecrease();

        protected State ComputeState()
        {
            bool isIncreasing = IsFollowingMovement(iProcessor.Increase);
            bool isDecreasing = IsFollowingMovement(iProcessor.Decrease);

            int conflictringDetectionCount = sConflictringDetectionCount;
            sConflictringDetectionCount = 0;

            if (isIncreasing && isDecreasing)
            {
                State = sLastState;
                State newState = IsIncreaseCloserThanDecrease() ? State.Increase : State.Decrease;

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