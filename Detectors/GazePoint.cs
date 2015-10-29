using System.Drawing;
using System.Text;

namespace SmoothPursuit.Detectors
{
    internal class GazePoint : DataPoint
    {
        public Point Location { get; private set; }

        public GazePoint(int aTimestamp, Point aLocation)
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
