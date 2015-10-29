using System.Text;

namespace SmoothPursuit.Detectors.Tracks
{
    public abstract class Track
    {
        public State State { get; protected set; }
        public int Duration { get; private set; }

        public Track(Points.Data aFirst, Points.Data aLast)
        {
            State = State.Unknown;
            if (aLast != null && aFirst != null)
            {
                Duration = aLast.Timestamp - aFirst.Timestamp;
            }
        }

        public virtual bool isFollowingIncreaseCue()
        {
            return State == State.Increase;
        }

        public virtual bool isFollowingDecreaseCue()
        {
            return State == State.Decrease;
        }

        public override string ToString()
        {
            return new StringBuilder().
                AppendFormat("\t{0,12}", Duration).
                AppendFormat("\t{0}", State).
                ToString();
        }
    }
}
