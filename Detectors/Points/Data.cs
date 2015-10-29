using System.Text;

namespace SmoothPursuit.Detectors.Points
{
    public class Data
    {
        public int Timestamp { get; private set; }

        public Data(int aTimestamp)
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
