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
            private static double AngleOffset = 0.0;
            private static double LastAngle = 0.0;
            
            public int Timestamp;
            public double Length;
            public double Angle;
            
            public Ray(int aTimestamp, double aLength, double aAngle)
            {
                Timestamp = aTimestamp;
                Length = aLength;
                Angle = aAngle + AngleOffset * 2 * Math.PI;

                if ((Angle - LastAngle) > Math.PI)
                {
                    LastAngle--;
                    Angle = aAngle + AngleOffset * 2 * Math.PI;
                }
                else if ((Angle - LastAngle) < -Math.PI)
                {
                    LastAngle++;
                    Angle = aAngle + AngleOffset * 2 * Math.PI;
                }
            }

            public Ray(int aTimestamp, Point aPoint, Point aCenter)
            {
                Timestamp = aTimestamp;
                int dx = aPoint.X - aCenter.X;
                int dy = aPoint.Y - aCenter.Y;
                Length = Math.Sqrt(dx * dx + dy * dy);
                Angle = Math.Atan2(dy, dx);
            }

            public bool isInRange(double aMinLength, double aMaxLength)
            {
                return aMinLength <= Length && Length <= aMaxLength;
            }
        }

        private class GazeTrack
        {
            public double Angle { get; private set; }
            public int Duration { get; private set; }
            public double Speed { get { return Angle * 1000 / Duration; } }
            
            public GazeTrack(Ray aFirst, Ray aLast)
            {
                double angle = aLast.Angle - aFirst.Angle;
                if (angle < -Math.PI)
                    angle += Math.PI;
                else if (angle > Math.PI)
                    angle -= Math.PI;

                Angle = angle;
                Duration = aLast.Timestamp - aFirst.Timestamp;
            }

            public GazeTrack(double aAngle, int aDuration)
            {
                Angle = aAngle;
                Duration = aDuration;
            }

            public bool isSpeedInRange(double aMin, double aMax)
            {
                var speed = this.Speed;
                return aMin <= speed && speed <= aMax;
            }
        }

        private const int BUFFER_DURATION = 1000;   // ms
        private const int MIN_BUFFER_DURATION = (int)(0.7 * BUFFER_DURATION);   // ms
        private const double ERROR_THRESHOLD = 0.2; // fraction
        private const double ANGLE_CHANGE = 1;      // degrees

        private bool iReady = false;
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
        }

        public void reset()
        {
            iReady = false;
            iBuffer.Clear();
        }

        public void feed(int aTimestamp, Point aPoint)
        {
            LimitBuffer(aTimestamp);

            Ray newRay = new Ray(aTimestamp, aPoint, iCenter);
            if (newRay.isInRange(iRadius * (1.0 - ERROR_THRESHOLD), iRadius * (1.0 + ERROR_THRESHOLD)))
            {
                iBuffer.Enqueue(newRay);

                if (iReady)
                {
                    State state = ComputeState(newRay);

                    if (state == State.Increase)
                        OnAngleChanged(this, new AngleChangedArgs(ANGLE_CHANGE));
                    else if (state == State.Decrease)
                        OnAngleChanged(this, new AngleChangedArgs(-ANGLE_CHANGE));
                }
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

        private State ComputeState(Ray aRayLast)
        {
            State result = State.Unknown;

            Ray rayFirst = iBuffer.Peek();
            GazeTrack track = new GazeTrack(rayFirst, aRayLast);
            if (track.isSpeedInRange(iExpectedSpeed * (1 - ERROR_THRESHOLD), iExpectedSpeed * (1 + ERROR_THRESHOLD)))
                result = State.Increase;
            else if (track.isSpeedInRange(-iExpectedSpeed * (1 + ERROR_THRESHOLD), -iExpectedSpeed * (1 - ERROR_THRESHOLD)))
                result = State.Decrease;

            Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", aRayLast.Length - iRadius, aRayLast.Angle, track.Angle, track.Duration, track.Speed, result);
            return result;
        }
    }
}
