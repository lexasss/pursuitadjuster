using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume
{
    public class RotationDetector : IGazeControllable
    {
        #region Declarations

        private enum State
        {
            Unknown,
            Increase,
            Decrease
        };

        private class Ray
        {
            private static int AngleCycle = 0;
            private static Angle LastAngle = new Angle();

            public int Timestamp;
            public double Length;
            public Angle Angle;

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
                State = State.Unknown;
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

        #endregion

        #region Consts

        private const int BUFFER_DURATION = 1000;           // ms
        private const int MIN_BUFFER_DURATION = (int)(0.7 * BUFFER_DURATION);   // ms
        private const double RADIUS_ERROR_THRESHOLD = 0.2;  // fraction
        private const double SPEED_ERROR_THRESHOLD = 0.4;   // fraction
        private const double ANGLE_CHANGE = 1;              // degrees

        #endregion

        #region Internal members

        private bool iReady = false;
        private Queue<Ray> iRayBuffer = new Queue<Ray>();

        private Point iCenter;
        private double iRadius;             // pixels
        private double iExpectedSpeed;      // rad / sec

        #endregion 

        #region Events

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

        #endregion

        #region Public methods

        public RotationDetector(int aCenterX, int aCenterY, double aRadius, double aExpectedSpeed)
        {
            iCenter = new Point(aCenterX, aCenterY);
            iRadius = aRadius;
            iExpectedSpeed = aExpectedSpeed * Math.PI / 180;
            
            //Console.WriteLine("Radius: {0} [{1} - {2}]", iRadius, iRadius * (1.0 - RADIUS_ERROR_THRESHOLD), iRadius * (1.0 + RADIUS_ERROR_THRESHOLD));
            //Console.WriteLine("Expected speed: {0:N3} [{1:N3} - {2:N3}]", iExpectedSpeed, iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD));
        }

        public void invalidate()
        {
            iReady = false;
            iRayBuffer.Clear();
        }

        public void addGazePoint(int aTimestamp, Point aPoint)
        {
            LimitBuffer(aTimestamp);

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

        #endregion

        #region Internal methods

        private void LimitBuffer(int aTimestamp)
        {
            while (iRayBuffer.Count > 0 && aTimestamp - iRayBuffer.Peek().Timestamp > BUFFER_DURATION)
            {
                iRayBuffer.Dequeue();
                iReady = true;
            }

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

        #endregion
    }
}
