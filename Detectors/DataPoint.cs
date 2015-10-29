using System.Text;

namespace SmoothPursuit.Detectors
{
    public class DataPoint
    {
        public int Timestamp { get; private set; }

        public DataPoint(int aTimestamp)
        {
            Timestamp = aTimestamp;
        }

        public override string ToString()
        {
            return new StringBuilder().
                AppendFormat("{0}", Timestamp).
                ToString();
        }
    }
}
