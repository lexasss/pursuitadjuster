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

        private struct GazePoint
        {
            public int Timestamp;
            public Point Point;
            public GazePoint(int aTimestamp, Point aPoint)
            {
                Timestamp = aTimestamp;
                Point = aPoint;
            }
        }

        private class Ray
        {
            private static int AngleCycle = 0;
            private static Angle LastAngle = new Angle();

            public int Timestamp;
            public double Length;
            public Angle Angle;
            /*
            public Ray(int aTimestamp, double aLength, double aAngle)
            {
                Timestamp = aTimestamp;
                Length = aLength;
                Angle = new Angle(aAngle).Rotate(AngleCycle).KeepCloseTo(LastAngle, ref AngleCycle);

                LastAngle = new Angle(Angle.Radians, Angle.Cycles);
            }*/

            public Ray(int aTimestamp, Point aPoint, Point aCenter)
            {
                Timestamp = aTimestamp;
                int dx = aPoint.X - aCenter.X;
                int dy = aPoint.Y - aCenter.Y;
                Length = Math.Sqrt(dx * dx + dy * dy);
                Angle = new Angle(Math.Atan2(dy, dx)).RotateBy(AngleCycle).KeepCloseTo(LastAngle, ref AngleCycle);

                LastAngle = new Angle(Angle.Radians, Angle.Cycles);
            }

            public bool isInRange(double aMinLength, double aMaxLength)
            {
                return aMinLength <= Length && Length <= aMaxLength;
            }

            public override string ToString()
            {
                return new StringBuilder().
                    AppendFormat("\t{0,8:N3}", Angle.Degrees).
                    AppendFormat("\t{0,8:N1}", Length).
                    ToString();
            }
        }

        private class GazeTrack
        {
            public Angle Angle { get; private set; }
            public int Duration { get; private set; }
            public double Speed { get { return Angle.Radians * 1000 / Duration; } }     // rad / sec
            public State State { get; set; }
            
            public GazeTrack(Ray aFirst, Ray aLast)
            {
                Angle = (aLast.Angle - aFirst.Angle).Normalize();
                Duration = aLast.Timestamp - aFirst.Timestamp;
                State = GazeParser.State.Unknown;
            }
            /*
            public GazeTrack(double aAngle, int aDuration)
            {
                Angle = aAngle;
                Duration = aDuration;
                State = GazeParser.State.Unknown;
            }*/

            public bool isSpeedInRange(double aMin, double aMax)
            {
                var speed = this.Speed;
                return aMin <= speed && speed <= aMax;
            }

            public override string ToString()
            {
                return new StringBuilder().
                    AppendFormat("\t{0,8:N3}", Angle.Degrees).
                    AppendFormat("\t{0,12}", Duration).
                    AppendFormat("\t{0,8:N3}", Speed).
                    AppendFormat("\t{0}", State).
                    ToString();
            }
        }

        private const int BUFFER_DURATION = 1000;           // ms
        private const int MIN_BUFFER_DURATION = (int)(0.7 * BUFFER_DURATION);   // ms
        private const double RADIUS_ERROR_THRESHOLD = 0.2;  // fraction
        private const double SPEED_ERROR_THRESHOLD = 0.4;   // fraction
        private const double ANGLE_CHANGE = 1;              // degrees
        private const double MIN_FIX_DIST = 50;             // pixels

        private bool iReady = false;
        private Point iLastPoint = Point.Empty;
        private Queue<Ray> iRayBuffer = new Queue<Ray>();

        private Queue<GazePoint> iPointBuffer = new Queue<GazePoint>();
        private System.Windows.Forms.Timer iPointsTimer = new System.Windows.Forms.Timer();

        private Point iCenter;
        private double iRadius;
        private double iExpectedSpeed;                      // rad / sec

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

            iPointsTimer.Interval = 30;
            iPointsTimer.Tick += PointsTimer_Tick;

            Console.WriteLine("Radius: {0} [{1} - {2}]", iRadius, iRadius * (1.0 - RADIUS_ERROR_THRESHOLD), iRadius * (1.0 + RADIUS_ERROR_THRESHOLD));
            Console.WriteLine("Expected speed: {0:N3} [{1:N3} - {2:N3}]", iExpectedSpeed, iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD));
        }

        public void start()
        {
            iReady = false;
            iRayBuffer.Clear();
            iPointBuffer.Clear();
            iPointsTimer.Start();
        }

        public void stop()
        {
            iPointsTimer.Stop();
        }

        public void feed(int aTimestamp, Point aPoint)
        {
            lock (iPointBuffer)
            {
                iPointBuffer.Enqueue(new GazePoint(aTimestamp, aPoint));
            }
        }

        private void LimitBuffer(int aTimestamp)
        {
            while (iRayBuffer.Count > 0 && aTimestamp - iRayBuffer.Peek().Timestamp > BUFFER_DURATION)
            {
                iRayBuffer.Dequeue();
                iReady = true;
            }

            iReady = iReady && iRayBuffer.Count > 1;
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
                    iRayBuffer.Clear();
                }
            }

            iLastPoint = aPoint;
            iReady = iReady && iRayBuffer.Count > 1;
        }

        private GazeTrack ComputeTrack(Ray aRayLast)
        {
            Ray rayFirst = iRayBuffer.Peek();
            GazeTrack track = new GazeTrack(rayFirst, aRayLast);
            if (track.isSpeedInRange(iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD)))
                track.State = State.Increase;
            else if (track.isSpeedInRange(-iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD), -iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD)))
                track.State = State.Decrease;

            return track;
        }

        private void ProcessNewPoint(int aTimestamp, Point aPoint)
        {
            LimitBuffer(aTimestamp);
            EnsureSmoothPursuit(aPoint);

            Ray newRay = new Ray(aTimestamp, aPoint, iCenter);
            if (newRay.isInRange(iRadius * (1.0 - RADIUS_ERROR_THRESHOLD), iRadius * (1.0 + RADIUS_ERROR_THRESHOLD)))
            {
                iRayBuffer.Enqueue(newRay);

                if (iReady)
                {
                    GazeTrack track = ComputeTrack(newRay);

                    if (track.State == State.Increase)
                        OnAngleChanged(this, new AngleChangedArgs(ANGLE_CHANGE));
                    else if (track.State == State.Decrease)
                        OnAngleChanged(this, new AngleChangedArgs(-ANGLE_CHANGE));
                    //Console.WriteLine("{0}\t\t|\t\t{1}", newRay, track);
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

        private void PointsTimer_Tick(object sender, EventArgs e)
        {
            int timestamp = 0;
            Point point = new Point(0, 0);
            int bufferSize = 0;

            lock (iPointBuffer)
            {
                while (iPointBuffer.Count > 0)
                {
                    GazePoint gp = iPointBuffer.Dequeue();
                    timestamp = gp.Timestamp;
                    point.X += gp.Point.X;
                    point.Y += gp.Point.Y;
                    bufferSize++;
                }
            }

            if (bufferSize > 0)
            {
                point.X /= bufferSize;
                point.Y /= bufferSize;

                ProcessNewPoint(timestamp, point);
            }
        }
    }
}
