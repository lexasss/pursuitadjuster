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
            private abstract class Data<T>
            {
                protected List<T> iValues = new List<T>();
                protected double iSum = 0;

                public double Mean { get { return Count > 0 ? iSum / Count : 0; } }
                public int Count { get { return iValues.Count; } }
                public double STD
                {
                    get
                    {
                        if (Count < 2)
                            return 1000000;

                        double mean = Mean;
                        double squareSum = 0;
                        foreach (T value in iValues)
                        {
                            double deviation = GetNumValue(value) - mean;
                            squareSum += deviation * deviation;
                        }
                        return Math.Sqrt(squareSum / Count);
                    }
                }

                public void feed(Point aOffset)
                {
                    T value = ConvertOffsetToData(aOffset);
                    iValues.Add(value);
                    iSum += GetNumValue(value);
                }

                protected abstract double GetNumValue(T aValue);
                protected abstract T ConvertOffsetToData(Point aOffset);
            }

            private class Distances : Data<double>
            {
                private const double ALPHA = 1.0;

                private double iPrevDistance = Double.NaN;

                protected override double GetNumValue(double aValue)
                {
                    return aValue;
                }

                protected override double ConvertOffsetToData(Point aOffset)
                {
                    double distance = Math.Sqrt(aOffset.X * aOffset.X + aOffset.Y * aOffset.Y);

                    //double smoothed = Double.IsNaN(iPrevDistance) ? distance : (distance + ALPHA * iPrevDistance) / (1.0 + ALPHA);
                    double smoothed = distance;

                    iPrevDistance = smoothed;
                    return smoothed;
                }
            }

            private class Angles : Data<Angle>
            {
                private static int AngleCycle = 0;
                private static Angle LastAngle = new Angle();

                protected override double GetNumValue(Angle aValue)
                {
                    return aValue.Degrees;
                }

                protected override Angle ConvertOffsetToData(Point aOffset)
                {
                    Angle angle = new Angle(Math.Atan2(aOffset.Y, aOffset.X)).rotateBy(AngleCycle).keepCloseTo(LastAngle, ref AngleCycle);
                    LastAngle = new Angle(angle.Radians, angle.Cycles);

                    return angle;
                }
            }

            private class MovementStats
            {
                public double DistancesMean { get; private set; }
                public double DistancesSTD { get; private set; }
                public double AngleMean { get; private set; }
                public double AngleSTD { get; private set; }

                public MovementStats(Distances aDistances, Angles aAngles)
                {
                    DistancesMean = aDistances.Mean;
                    DistancesSTD = aDistances.STD;
                    AngleMean = aAngles.Mean;
                    AngleSTD = aAngles.STD;
                }
            }

            private class Processor
            {
                public MovementStats Increase { get; private set; }
                public MovementStats Decrease { get; private set; }

                public Processor(GazePoint[] aBuffer)
                {
                    Distances increaseDistances = new Distances();
                    Distances decreaseDistances = new Distances();
                    Angles increaseAngles = new Angles();
                    Angles decreaseAngles = new Angles();

                    foreach (GazePoint point in aBuffer)
                    {
                        increaseDistances.feed(point.OffsetIncrease);
                        decreaseDistances.feed(point.OffsetDecrease);
                        //increaseAngles.feed(point.OffsetIncrease);
                        //decreaseAngles.feed(point.OffsetDecrease);
                    }

                    Increase = new MovementStats(increaseDistances, increaseAngles);
                    Decrease = new MovementStats(decreaseDistances, decreaseAngles);
                }
            }

            private const double DISTANCE_STD_THRESHOLD = 15.0;     // pixels
            private const double ANGLE_STD_THRESHOLD = 6.0;         // degrees
            private const double MAX_VAR_FROM_CUE_DISTANCE = 0.3;   // fraction

            private Processor iProcessor;
            private int iDataCount;
            private double iTrackDistance;
            private double iCueIncreaseDistance;
            private double iCueDecreaseDistance;

            public GazeTrack(GazePoint[] aBuffer)
                : base(aBuffer[0], aBuffer[aBuffer.Length - 1])
            {
                GazePoint first = aBuffer[0];
                GazePoint last = aBuffer[aBuffer.Length - 1];
                iTrackDistance = GetDistance(first.Location, last.Location);
                iCueIncreaseDistance = GetDistance(
                    new Point(first.Location.X - first.OffsetIncrease.X, first.Location.Y - first.OffsetIncrease.Y),
                    new Point(last.Location.X - last.OffsetIncrease.X, last.Location.Y - last.OffsetIncrease.Y));
                iCueDecreaseDistance = GetDistance(
                    new Point(first.Location.X - first.OffsetDecrease.X, first.Location.Y - first.OffsetDecrease.Y),
                    new Point(last.Location.X - last.OffsetDecrease.X, last.Location.Y - last.OffsetDecrease.Y));

                iDataCount = aBuffer.Length;
                iProcessor = new Processor(aBuffer);

                bool isIncreasing = IsFollowingMovement(iProcessor.Increase, iCueIncreaseDistance);
                bool isDecreasing = IsFollowingMovement(iProcessor.Decrease, iCueDecreaseDistance);
                
                if (isIncreasing && !isDecreasing)
                {
                    State = State.Increase;
                }
                else if (isDecreasing && !isIncreasing)
                {
                    State = State.Decrease;
                }
            }

            public override string ToString()
            {
                return new StringBuilder(base.ToString()).
                    AppendFormat("\tC={0}", iDataCount).
                    AppendFormat("\tSid={0:N2}", iProcessor.Increase.DistancesSTD).
                    AppendFormat("\tSia={0:N2}", iProcessor.Increase.AngleSTD).
                    AppendFormat("\tSdd={0:N2}", iProcessor.Decrease.DistancesSTD).
                    AppendFormat("\tSda={0:N2}", iProcessor.Decrease.AngleSTD).
                    AppendFormat("\tTD={0:N0}", iTrackDistance).
                    AppendFormat("\tCDi={0:N0}", iCueIncreaseDistance).
                    AppendFormat("\tCDd={0:N0}", iCueDecreaseDistance).
                    ToString();
            }

            private bool IsFollowingMovement(MovementStats aMovementStats, double aCueDistance)
            {
                // check distance STD against threshold, and also angle STD if the distance is long enough
                /*
                return aMovementStats.DistancesSTD < DISTANCE_STD_THRESHOLD && (
                    aMovementStats.DistancesMean > 5 * DISTANCE_STD_THRESHOLD ?
                    aMovementStats.AngleSTD * aMovementStats.DistancesMean < ANGLE_STD_THRESHOLD : true);
                */
                return aMovementStats.DistancesSTD < DISTANCE_STD_THRESHOLD &&
                    iTrackDistance < aCueDistance * (1 + MAX_VAR_FROM_CUE_DISTANCE) &&
                    iTrackDistance > aCueDistance * (1 - MAX_VAR_FROM_CUE_DISTANCE);
            }

            private double GetDistance(Point aFirst, Point aLast)
            {
                double dx = aLast.X - aFirst.X;
                double dy = aLast.Y - aFirst.Y;
                return Math.Sqrt(dx * dx + dy * dy);
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
