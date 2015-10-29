using System.Drawing;
using System.Text;

namespace SmoothPursuit.Detectors.Points
{
    public class Gaze
    {
        public int Timestamp { get; private set; }
        public Point Location { get; private set; }

        public Gaze(int aTimestamp, Point aLocation)
        {
            Timestamp = aTimestamp;
            Location = aLocation;
        }

        public override string ToString()
        {
            return new StringBuilder().
                AppendFormat("{0}", Timestamp).
                AppendFormat("\t{0},{1}", Location.X, Location.Y).
                ToString();
        }
    }
}
