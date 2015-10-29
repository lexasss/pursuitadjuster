using System.Text;

namespace SmoothPursuit.Detectors.Tracks
{
    internal abstract class Speed : Track
    {
        public const double SPEED_ERROR_THRESHOLD = 0.4;   // fraction

        protected double iExpectedSpeed;

        public double Value { get { return GetLength() * 1000 / Duration; } }   // per second

        public Speed(Points.Data aFirst, Points.Data aLast, double aExpectedSpeed)
            : base(aFirst, aLast)
        {
            iExpectedSpeed = aExpectedSpeed;
        }

        public override bool isFollowingIncreaseCue()
        {
            if (IsMovingWithSpeed(iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD)))
            {
                State = State.Increase;
            }

            return State == State.Increase;
        }

        public override bool isFollowingDecreaseCue()
        {
            if (IsMovingWithSpeed(-iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD), -iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD)))
            {
                State = State.Decrease;
            }

            return State == State.Decrease;
        }

        public override string ToString()
        {
            return new StringBuilder(base.ToString()).
                AppendFormat("\t{0,8:N3}", Value).
                ToString();
        }

        protected abstract double GetLength();

        protected bool IsMovingWithSpeed(double aMinSpeed, double aMaxSpeed)
        {
            var speed = this.Value;
            return aMinSpeed <= speed && speed <= aMaxSpeed;
        }
    }
}
