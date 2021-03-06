﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SmoothPursuit.Detectors.Tracks
{
    public enum CueType
    {
        Increase,
        Decrease
    }

    public abstract class Data<T>
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

    public class Distances : Data<double>
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

    public class Angles : Data<Angle>
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

    public class Movement
    {
        public double Distance { get; private set; }
        public Point Direction { get; private set; }

        public Movement(Points.Offset aFirst, Points.Offset aLast, bool aIsIncrease)
        {
            Point cueFirst = GetCuePoint(aFirst.Location, aIsIncrease ? aFirst.OffsetIncrease : aFirst.OffsetDecrease);
            Point cueLast = GetCuePoint(aLast.Location, aIsIncrease ? aLast.OffsetIncrease : aLast.OffsetDecrease);
            Distance = GetDistance(cueFirst, cueLast);
            Direction = new Point(cueLast.X - cueFirst.X, cueLast.Y - cueFirst.Y);
        }

        public Movement(Points.Offset aFirst, Points.Offset aLast)
        {
            Distance = GetDistance(aFirst.Location, aLast.Location);
            Direction = new Point(aLast.Location.X - aFirst.Location.X, aLast.Location.Y - aFirst.Location.Y);
        }

        private Point GetCuePoint(Point aGazePoint, Point aOffset)
        {
            return new Point(aGazePoint.X - aOffset.X, aGazePoint.Y - aOffset.Y);
        }

        private double GetDistance(Point aFirst, Point aLast)
        {
            double dx = aLast.X - aFirst.X;
            double dy = aLast.Y - aFirst.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public class MovementStatsDist : MovementStats
    {
        public Movement Cue { get; private set; }

        public double DistancesMean { get; private set; }
        public double DistancesSTD { get; private set; }
        public double AngleMean { get; private set; }
        public double AngleSTD { get; private set; }

        public override void compute(Point[] aOffsets)
        {
            throw new NotImplementedException();
        }

        public void compute(Distances aDistances, Angles aAngles)
        {
            DistancesMean = aDistances.Mean;
            DistancesSTD = aDistances.STD;
            AngleMean = aAngles.Mean;
            AngleSTD = aAngles.STD;
        }

        public void createCueMovementStats(Points.Offset aFirst, Points.Offset aLast, CueType aType)
        {
            Cue = new Movement(aFirst, aLast, aType == CueType.Increase);
        }
    }

    public class ProcessorDist : Processor<MovementStatsDist>
    {
        public override void compute(Points.Offset[] aBuffer)
        {
            Points.Offset first = aBuffer[0];
            Points.Offset last = aBuffer[aBuffer.Length - 1];

            Distances increaseDistances = new Distances();
            Distances decreaseDistances = new Distances();
            Angles increaseAngles = new Angles();
            Angles decreaseAngles = new Angles();

            foreach (Points.Offset point in aBuffer)
            {
                increaseDistances.feed(point.OffsetIncrease);
                decreaseDistances.feed(point.OffsetDecrease);
                //increaseAngles.feed(point.OffsetIncrease);
                //decreaseAngles.feed(point.OffsetDecrease);
            }

            Increase = new MovementStatsDist();
            Increase.createCueMovementStats(first, last, CueType.Increase);
            Increase.compute(increaseDistances, increaseAngles);
            
            Decrease = new MovementStatsDist();
            Decrease.createCueMovementStats(first, last, CueType.Decrease);
            Decrease.compute(decreaseDistances, decreaseAngles);
        }
    }

    public class OffsetDist : Offset<MovementStatsDist, ProcessorDist>
    {
        #region Constants

        private const double DISTANCE_STD_THRESHOLD = 15.0;     // pixels, d=15, increase (20) if too bad tracking
        private const double ANGLE_STD_THRESHOLD = 6.0;         // degrees
        private const double MAX_VAR_FROM_CUE_DISTANCE = 0.5;   // fraction, d=0.5, decrease (0.3) if too bad tracking

        #endregion

        #region Internal members

        private Movement iTrack;

        #endregion

        #region Public methods

        public override void init(Points.Offset[] aBuffer)
        {
            Points.Offset first = aBuffer[0];
            Points.Offset last = aBuffer[aBuffer.Length - 1];

            base.init(first, last);

            if (first != null && last != null)
            {
                iTrack = new Movement(first, last);

                iDataCount = aBuffer.Length;
                iProcessor = new ProcessorDist();
                iProcessor.compute(aBuffer);

                sLastState = ComputeState();
            }
        }

        public override string ToString()
        {
            return iProcessor == null ? "INVALID TRACK" :
                new StringBuilder(base.ToString()).
                AppendFormat("\tIDs={0:N2}", iProcessor.Increase.DistancesSTD).
                //AppendFormat("\tISa={0:N2}", iProcessor.Increase.AngleSTD).
                AppendFormat("\tDDs={0:N2}", iProcessor.Decrease.DistancesSTD).
                //AppendFormat("\tDSa={0:N2}", iProcessor.Decrease.AngleSTD).
                AppendFormat("\tTD={0:N0}", iTrack.Distance).
                AppendFormat("\tTV={0}", iTrack.Direction).
                AppendFormat("\tID={0:N0}", iProcessor.Increase.Cue.Distance).
                AppendFormat("\tIV={0}", iProcessor.Increase.Cue.Direction).
                AppendFormat("\tDD={0:N0}", iProcessor.Decrease.Cue.Distance).
                AppendFormat("\tDV={0}", iProcessor.Decrease.Cue.Direction).
                ToString();
        }

        #endregion

        #region Internal methods

        protected override bool IsFollowingMovement(MovementStatsDist aMovementStats)
        {
            // check distance STD against threshold, and also angle STD if the distance is long enough
            /*
            return aMovementStats.DistancesSTD < DISTANCE_STD_THRESHOLD && (
                aMovementStats.DistancesMean > 5 * DISTANCE_STD_THRESHOLD ?
                aMovementStats.AngleSTD * aMovementStats.DistancesMean < ANGLE_STD_THRESHOLD : true);
            */
            return aMovementStats.DistancesSTD < DISTANCE_STD_THRESHOLD &&
                iTrack.Distance < aMovementStats.Cue.Distance * (1 + MAX_VAR_FROM_CUE_DISTANCE) &&
                iTrack.Distance > aMovementStats.Cue.Distance * (1 - MAX_VAR_FROM_CUE_DISTANCE);
        }

        protected override bool IsIncreaseCloserThanDecrease()
        {
            return GetDistance(iProcessor.Increase.Cue.Direction, iTrack.Direction) <
                GetDistance(iProcessor.Decrease.Cue.Direction, iTrack.Direction);
        }

        private double GetDistance(Point aPoint1, Point aPoint2)
        {
            double dx = aPoint2.X - aPoint1.X;
            double dy = aPoint2.Y - aPoint1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        #endregion
    }
}
