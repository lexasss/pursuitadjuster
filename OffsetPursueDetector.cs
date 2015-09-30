using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SmoothPursuit
{
    public class OffsetPursueDetector : IPursueDetector
    {
        #region Declarations

        private class GazePoint : DataPoint
        {
            public Point Location { get; private set; }
            public Point OffsetIncrease { get; private set; }
            public Point OffsetDecrease { get; private set; }

            public GazePoint(int aTimestamp, Point aLocation, Point aOffsetIncrease, Point aOffsetDecrease)
                : base(aTimestamp)
            {
                Location = aLocation;
                OffsetIncrease = aOffsetIncrease;
                OffsetDecrease = aOffsetDecrease;
            }

            public override string ToString()
            {
                return new StringBuilder().
                    AppendFormat("\t{0},{1}", Location.X, Location.Y).
                    AppendFormat("\t{0},{1}", OffsetIncrease.X, OffsetIncrease.Y).
                    AppendFormat("\t{0},{1}", OffsetDecrease.X, OffsetDecrease.Y).
                    ToString();
            }
        }

        private class GazeTrack : Track
        {
            private class Processor
            {
                private class SmoothedOffset
                {
                    private const double ALPHA = 2.0;

                    private double iPrevDistance = Double.NaN;
                    private List<double> iDistances = new List<double>();
                    private double iSum = 0;

                    public double Mean { get { return Count > 0 ? iSum / Count : 0; } }
                    public int Count { get { return iDistances.Count; } }
                    public double STD
                    {
                        get
                        {
                            if (Count < 2)
                                return 1000000;

                            double mean = Mean;
                            double squareSum = 0;
                            foreach (double dist in iDistances)
                            {
                                double deviation = dist - mean;
                                squareSum += deviation * deviation;
                            }
                            return Math.Sqrt(squareSum / Count);
                        }
                    }

                    public double feed(Point aOffset)
                    {
                        double distance = Math.Sqrt(aOffset.X * aOffset.X + aOffset.Y * aOffset.Y);
                        return feed(distance);
                    }

                    public double feed(double aDistance)
                    {
                        double smoothed = Double.IsNaN(iPrevDistance) ? aDistance : (aDistance + ALPHA * iPrevDistance) / (1.0 + ALPHA);

                        iPrevDistance = smoothed;
                        iDistances.Add(smoothed);
                        iSum += smoothed;

                        return smoothed;
                    }
                }

                private SmoothedOffset iSmoothedOffsetIncrease = new SmoothedOffset();
                private SmoothedOffset iSmoothedOffsetDecrease = new SmoothedOffset();
                
                public double STDIncrease { get { return iSmoothedOffsetIncrease.STD; } }
                public double STDDecrease { get { return iSmoothedOffsetDecrease.STD; } }

                public Processor(GazePoint[] aBuffer)
                {
                    foreach (GazePoint point in aBuffer)
                    {
                        iSmoothedOffsetIncrease.feed(point.OffsetIncrease);
                        iSmoothedOffsetDecrease.feed(point.OffsetDecrease);
                    }
                }
            }

            private const double DISTANCE_STD_THRESHOLD = 7.0;

            private Processor iProcessor;

            public GazeTrack(GazePoint[] aBuffer)
                : base(aBuffer[0], aBuffer[aBuffer.Length - 1])
            {
                iProcessor = new Processor(aBuffer);

                double stdIncrease = iProcessor.STDIncrease;
                double stdDecrease = iProcessor.STDDecrease;

                if (stdIncrease < DISTANCE_STD_THRESHOLD && stdIncrease < stdDecrease)
                {
                    State = State.Increase;
                }
                else if (stdDecrease < DISTANCE_STD_THRESHOLD && stdDecrease < stdIncrease)
                {
                    State = State.Decrease;
                }
            }

            public override string ToString()
            {
                return new StringBuilder(base.ToString()).
                    AppendFormat("\t{0}", iProcessor.STDIncrease).
                    AppendFormat("\t{0}", iProcessor.STDDecrease).
                    ToString();
            }
        }

        #endregion

        #region Internal members

        private ICue iCueIncrease;
        private ICue iCueDecrease;

        #endregion

        #region Public methods

        public OffsetPursueDetector(ICue aCueIncrease, ICue aCueDecrease)
            : base()
        {
            iCueIncrease = aCueIncrease;
            iCueDecrease = aCueDecrease;
            iValueStep = 1;
        }

        #endregion

        #region Internal methods

        protected override DataPoint CreateDataPoint(int aTimestamp, Point aPoint)
        {
            Point cueIncrease = iCueIncrease.Location;
            Point cueDecrease = iCueDecrease.Location;
            return new GazePoint(aTimestamp, aPoint,
                new Point(aPoint.X - cueIncrease.X, aPoint.Y - cueIncrease.Y),
                new Point(aPoint.X - cueDecrease.X, aPoint.Y - cueDecrease.Y));
        }

        protected override Track CreateTrack(DataPoint aFirstDataPoint, DataPoint aLastDataPoint)
        {
            List<GazePoint> gazePoints = new List<GazePoint>();
            foreach (DataPoint point in iDataBuffer.ToArray())
            {
                gazePoints.Add((GazePoint)point);
            }

            return new GazeTrack(gazePoints.ToArray());
        }

        #endregion
    }
}
