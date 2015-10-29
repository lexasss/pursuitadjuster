namespace SmoothPursuit.Detectors.Tracks
{
    internal class Dwell : Track
    {
        public Dwell()
            : base(null, null)
        {
        }

        public void updateState(bool aIsIncreaseActivated, bool aIsDecreaseActivated)
        {
            if (aIsIncreaseActivated)
                State = Detectors.State.Increase;
            else if (aIsDecreaseActivated)
                State = Detectors.State.Decrease;
            else
                State = Detectors.State.Unknown;
        }
    }
}
