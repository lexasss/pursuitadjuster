using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SmoothVolume
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
            public int Duration { get; private set; }
            public double Speed { get { return GetLength() * 1000 / Duration; } }
            public State State { get; set; }

            public Track(DataPoint aFirst, DataPoint aLast)
            {
                Duration = aLast.Timestamp - aFirst.Timestamp;
                State = State.Unknown;
            }

            public bool isSpeedInRange(double aMin, double aMax)
            {
                var speed = this.Speed;
                return aMin <= speed && speed <= aMax;
            }

            public override string ToString()
            {
                return new StringBuilder().
                    AppendFormat("\t{0,12}", Duration).
                    AppendFormat("\t{0,8:N3}", Speed).
                    AppendFormat("\t{0}", State).
                    ToString();
            }

            protected abstract double GetLength();
        }

        #endregion

        #region Consts

        protected int BUFFER_DURATION = 1000;           // ms
        protected double SPEED_ERROR_THRESHOLD = 0.4;   // fraction
        protected double VALUE_CHANGE = 1;
        
        #endregion

        #region Internal members

        protected Queue<DataPoint> iDataBuffer = new Queue<DataPoint>();
        protected bool iReady = false;
        protected double iExpectedSpeed;      

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

        public IPursueDetector(double aExpectedSpeed)
        {
            iExpectedSpeed = aExpectedSpeed;
            //Console.WriteLine("Expected speed: {0:N3} [{1:N3} - {2:N3}]", iExpectedSpeed, iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD));
        }

        public virtual void start()
        {
            iReady = false;
            iDataBuffer.Clear();
        }

        public virtual void saccade()
        {
            iReady = false;
            iDataBuffer.Clear();
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
                    Track track = ComputeTrack(newDataPoint);

                    if (track.State == State.Increase)
                        OnValueChangeRequest(this, new ValueChangeRequestArgs(VALUE_CHANGE));
                    else if (track.State == State.Decrease)
                        OnValueChangeRequest(this, new ValueChangeRequestArgs(-VALUE_CHANGE));
                    //Console.WriteLine("{0}\t\t|\t\t{1}", newDataPoint, track);
                }
                else
                {
                    //Console.WriteLine("{0}", iDataBuffer.Count);
                }
            }
            else
            {
                //Console.WriteLine("{0}\t{1}", newDataPoint.Length, ToDegrees(newDataPoint.Angle));
            }
        }

        #endregion

        #region Internal methods

        protected abstract DataPoint CreateDataPoint(int aTimestamp, Point aPoint);
        protected abstract Track CreateTrack(DataPoint aFirstDataPoint, DataPoint aLastDataPoint);

        protected void LimitBuffer(int aTimestamp)
        {
            while (iDataBuffer.Count > 0 && aTimestamp - iDataBuffer.Peek().Timestamp > BUFFER_DURATION)
            {
                iDataBuffer.Dequeue();
                iReady = true;
            }

            iReady = iReady && iDataBuffer.Count > 1;
        }

        private Track ComputeTrack(DataPoint aLastDataPoint)
        {
            DataPoint firstDataPoint = iDataBuffer.Peek();
            Track track = CreateTrack(firstDataPoint, aLastDataPoint);
            if (track.isSpeedInRange(iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD), iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD)))
                track.State = State.Increase;
            else if (track.isSpeedInRange(-iExpectedSpeed * (1 + SPEED_ERROR_THRESHOLD), -iExpectedSpeed * (1 - SPEED_ERROR_THRESHOLD)))
                track.State = State.Decrease;

            return track;
        }

        #endregion
    }
}
