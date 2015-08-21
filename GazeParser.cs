using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume
{
    public class GazeParser
    {
        private enum State
        {
            Unknown,
            Increase,
            Decrease
        };

        private class Ray
        {
            private static int AngleOffset = 0;
            private static double LastAngle = 0.0;
            
            public int Timestamp;
            public double Length;
            public double Angle;
            
            public Ray(int aTimestamp, double aLength, double aAngle)
            {
                Timestamp = aTimestamp;
                Length = aLength;
                Angle = EnsureAngleNotCirculed(aAngle);
            }

            public Ray(int aTimestamp, Point aPoint, Point aCenter)
            {
                Timestamp = aTimestamp;
                int dx = aPoint.X - aCenter.X;
                int dy = aPoint.Y - aCenter.Y;
                Length = Math.Sqrt(dx * dx + dy * dy);
                Angle = EnsureAngleNotCirculed(Math.Atan2(dy, dx));
            }

            public bool isInRange(double aMinLength, double aMaxLength)
            {
                return aMinLength <= Length && Length <= aMaxLength;
            }

            private double EnsureAngleNotCirculed(double aAngle)
            {
                double angle = aAngle + AngleOffset * 2 * Math.PI;
                if ((angle - LastAngle) > Math.PI)
                {
                    AngleOffset--;
                    Console.WriteLine("-- Was {0} now {1}", angle, aAngle + AngleOffset * 2 * Math.PI);
                    angle = aAngle + AngleOffset * 2 * Math.PI;
                }
                else if ((angle - LastAngle) < -Math.PI)
                {
                    AngleOffset++;
                    Console.WriteLine("++ Was {0} now {1}", angle, aAngle + AngleOffset * 2 * Math.PI);
                    angle = aAngle + AngleOffset * 2 * Math.PI;
                }

                LastAngle = angle;
                return angle;
            }
        }

        private class GazeTrack
        {
            public double Angle { get; private set; }
            public int Duration { get; private set; }
            public double Speed { get { return Angle * 1000 / Duration; } }
            public State State { get; set; }
            
            public GazeTrack(Ray aFirst, Ray aLast)
            {
                double angle = aLast.Angle - aFirst.Angle;
                if (angle < -Math.PI)
                    angle += Math.PI;
                else if (angle > Math.PI)
                    angle -= Math.PI;

                Angle = angle;
                Duration = aLast.Timestamp - aFirst.Timestamp;
                State = GazeParser.State.Unknown;
            }

            public GazeTrack(double aAngle, int aDuration)
            {
                Angle = aAngle;
                Duration = aDuration;
                State = GazeParser.State.Unknown;
            }

            public bool isSpeedInRange(double aMin, double aMax)
            {
                var speed = this.Speed;
                return aMin <= speed && speed <= aMax;
            }
        }

        private const int BUFFER_DURATION = 1000;   // ms
        private const int MIN_BUFFER_DURATION = (int)(0.7 * BUFFER_DURATION);   // ms
        private const double RADIUS_ERROR_THRESHOLD = 0.2; // fraction
        private const double SPEED_ERROR_THRESHOLD = 0.4; // fraction
        private const double ANGLE_CHANGE = 1;      // degrees
        private const double MIN_FIX_DIST = 50;      // pixels

        private bool iReady = false;
        private Point iLastPoint = Point.Empty;
        private Queue<Ray> iBuffer = new Queue<Ray>();

        private Point iCenter;
        private double iRadius;
        private double iExpectedSpeed;  // radians per second

        public class AngleChangedArgs : EventArgs
        {
            public double AngleChange { get; private set; }
            public AngleChangedArgs(double aAngleChange)
            {
                AngleChange = aAngleChange;
            }
        }
        public delegate void AngleChangedHandler(object aSender, AngleChangedArgs aArgs);
        public event AngleChangedHandler OnAngleChanged = delegate { };

        public GazeParser(int aCenterX, int aCenterY, double aRadius, double aExpectedSpeed)
        {
            iCenter = new Point(aCenterX, aCenterY);
            iRadius = aRadius;
            iExpectedSpeed = aExpectedSpeed * Math.PI / 180;
            Console.WriteLine("Radius limits: {0} - {1}", iRadius * (1.0 - RADIUS_ERROR_THRESHOLD), iRadius * (1.0 + RADIUS_ERROR_THRESHOLD));
            Console.WriteLine("Speed limits: {0} - {1}", iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD));
        }

        public void reset()
        {
            iReady = false;
            iBuffer.Clear();
        }

        public void feed(int aTimestamp, Point aPoint)
        {
            LimitBuffer(aTimestamp);
            EnsureSmoothPursuit(aPoint);

            Ray newRay = new Ray(aTimestamp, aPoint, iCenter);
            if (newRay.isInRange(iRadius * (1.0 - RADIUS_ERROR_THRESHOLD), iRadius * (1.0 + RADIUS_ERROR_THRESHOLD)))
            {
                iBuffer.Enqueue(newRay);

                if (iReady)
                {
                    GazeTrack track = ComputeTrack(newRay);

                    if (track.State == State.Increase)
                        OnAngleChanged(this, new AngleChangedArgs(ANGLE_CHANGE));
                    else if (track.State == State.Decrease)
                        OnAngleChanged(this, new AngleChangedArgs(-ANGLE_CHANGE));
                    //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", newRay.Length - iRadius, ToDegrees(newRay.Angle), ToDegrees(track.Angle), track.Duration, track.Speed, track.State);
                }
                else
                {
                    //Console.WriteLine("{0}", iBuffer.Count);
                }
            }
            else
            {
                //Console.WriteLine("{0}\t{1}", newRay.Length, ToDegrees(newRay.Angle));
            }
        }

        private void LimitBuffer(int aTimestamp)
        {
            while (iBuffer.Count > 0 && aTimestamp - iBuffer.Peek().Timestamp > BUFFER_DURATION)
            {
                iBuffer.Dequeue();
                iReady = true;
            }

            iReady = iReady && iBuffer.Count > 1;
        }

        private void EnsureSmoothPursuit(Point aPoint)
        {
            if (!iLastPoint.IsEmpty)
            {
                int dx = aPoint.X - iLastPoint.X;
                int dy = aPoint.Y - iLastPoint.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist > MIN_FIX_DIST)
                {
                    iBuffer.Clear();
                }
            }

            iLastPoint = aPoint;
            iReady = iReady && iBuffer.Count > 1;
        }

        private GazeTrack ComputeTrack(Ray aRayLast)
        {
            Ray rayFirst = iBuffer.Peek();
            GazeTrack track = new GazeTrack(rayFirst, aRayLast);
            if (track.isSpeedInRange(iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD)))
                track.State = State.Increase;
            else if (track.isSpeedInRange(-iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD), -iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD)))
                track.State = State.Decrease;

            return track;
        }

        private double ToDegrees(double aRadians)
        {
            return aRadians * 180 / Math.PI;
        }
    }
}
