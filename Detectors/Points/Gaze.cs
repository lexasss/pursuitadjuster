using System.Drawing;
using System.Text;

namespace SmoothPursuit.Detectors.Points
{
    internal class Gaze : Data
    {
        public Point Location { get; private set; }

        public Gaze(int aTimestamp, Point aLocation)
            : base(aTimestamp)
        {
            Location = aLocation;
        }

        public override string ToString()
        {
            return new StringBuilder(base.ToString()).
                AppendFormat("\t{0},{1}", Location.X, Location.Y).
                ToString();
        }
    }
}
