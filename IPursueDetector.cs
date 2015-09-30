using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SmoothPursuit
{
    public abstract class IPursueDetector
    {
        #region Declarations

        protected enum State
        {
            Unknown,
            Decrease,
            Increase
        };

        protected class DataPoint
        {
            public int Timestamp { get; private set; }

            public DataPoint(int aTimestamp)
            {
                Timestamp = aTimestamp;
            }
        }

        protected abstract class Track
        {
            public State State { get; protected set; }
            public int Duration { get; private set; }

            public Track(DataPoint aFirst, DataPoint aLast)
            {
                Duration = aLast.Timestamp - aFirst.Timestamp;
                State = State.Unknown;
            }

            public virtual bool isFollowingIncreaseCue()
            {
                return State == State.Increase;
            }

            public virtual bool isFollowingDecreaseCue()
            {
                return State == State.Decrease;
            }

            public override string ToString()
            {
                return new StringBuilder().
                    AppendFormat("\t{0,12}", Duration).
                    AppendFormat("\t{0}", State).
                    ToString();
            }
        }

        protected abstract class SpeedTrack : Track
        {
            public const double SPEED_ERROR_THRESHOLD = 0.4;   // fraction

            protected double iExpectedSpeed;
            
            public double Speed { get { return GetLength() * 1000 / Duration; } }   // per second

            public SpeedTrack(DataPoint aFirst, DataPoint aLast, double aExpectedSpeed)
                : base(aFirst, aLast)
            {
                iExpectedSpeed = aExpectedSpeed;
            }

            public override bool isFollowingIncreaseCue()
            {
                if (IsMovingWithSpeed(iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD)))
                {
                    State = State.Increase;
                }

                return State == State.Increase;
            }

            public override bool isFollowingDecreaseCue()
            {
                if (IsMovingWithSpeed(-iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD), -iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD)))
                {
                    State = State.Decrease;
                }

                return State == State.Decrease;
            }

            public override string ToString()
            {
                return new StringBuilder(base.ToString()).
                    AppendFormat("\t{0,8:N3}", Speed).
                    ToString();
            }

            protected abstract double GetLength();

            protected bool IsMovingWithSpeed(double aMinSpeed, double aMaxSpeed)
            {
                var speed = this.Speed;
                return aMinSpeed <= speed && speed <= aMaxSpeed;
            }
        }

        #endregion

        #region Consts

        protected const int BUFFER_DURATION = 1000;           // ms
        
        #endregion

        #region Internal members

        protected Queue<DataPoint> iDataBuffer = new Queue<DataPoint>();
        protected bool iReady = false;
        protected double iValueStep = 1;

        #endregion

        #region Events

        public class ValueChangeRequestArgs : EventArgs
        {
            public double ValueChange { get; private set; }
            public ValueChangeRequestArgs(double aValueChange)
            {
                ValueChange = aValueChange;
            }
        }
        public delegate void ValueChangeRequestHandler(object aSender, ValueChangeRequestArgs aArgs);
        public event ValueChangeRequestHandler OnValueChangeRequest = delegate { };

        #endregion

        #region Public methods

        public virtual void start()
        {
            iReady = false;
            iDataBuffer.Clear();
        }

        public virtual void saccade()
        {
            iReady = false;
            iDataBuffer.Clear();
            Console.WriteLine("=== SACCADE === ");
        }

        public virtual void addGazePoint(int aTimestamp, Point aPoint)
        {
            LimitBuffer(aTimestamp);

            DataPoint newDataPoint = CreateDataPoint(aTimestamp, aPoint);
            if (newDataPoint != null)
            {
                iDataBuffer.Enqueue(newDataPoint);

                if (iReady)
                {
                    DataPoint firstDataPoint = iDataBuffer.Peek();
                    Track track = CreateTrack(firstDataPoint, newDataPoint);

                    if (track.isFollowingIncreaseCue())
                        OnValueChangeRequest(this, new ValueChangeRequestArgs(iValueStep));
                    else if (track.isFollowingDecreaseCue())
                        OnValueChangeRequest(this, new ValueChangeRequestArgs(-iValueStep));
                    //Console.WriteLine("{0}\t\t|\t\t{1}", newDataPoint, track);
                }
                else
                {
                    //Console.WriteLine("{0}", iDataBuffer.Count);
                }
            }
            else
            {
                //Console.WriteLine("{0}", aPoint);
            }
        }

        #endregion

        #region Internal methods

        protected abstract DataPoint CreateDataPoint(int aTimestamp, Point aPoint);
        protected abstract Track CreateTrack(DataPoint aFirstDataPoint, DataPoint aLastDataPoint);

        private void LimitBuffer(int aTimestamp)
        {
            while (iDataBuffer.Count > 0 && aTimestamp - iDataBuffer.Peek().Timestamp > BUFFER_DURATION)
            {
                iDataBuffer.Dequeue();
                iReady = true;
            }

            iReady = iReady && iDataBuffer.Count > 1;
        }

        #endregion
    }
}
