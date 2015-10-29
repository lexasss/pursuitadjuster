using System;
using System.Collections.Generic;
using System.Drawing;

namespace SmoothPursuit.Detectors
{
    public abstract class IPursueDetector
    {
        #region Declarations

        public enum Direction
        {
            Increase,
            Decrease
        }

        #endregion

        #region Consts

        protected const int BUFFER_DURATION = 600;           // ms
        
        #endregion

        #region Internal members

        protected Queue<Points.Gaze> iDataBuffer = new Queue<Points.Gaze>();
        protected bool iReady = false;

        protected ICue iCueIncrease;
        protected ICue iCueDecrease;
        
        #endregion

        #region Events

        public class ValueChangeRequestArgs : EventArgs
        {
            public Direction Direction { get; private set; }
            public ValueChangeRequestArgs(Direction aDirection)
            {
                Direction = aDirection;
            }
        }
        public delegate void ValueChangeRequestHandler(object aSender, ValueChangeRequestArgs aArgs);
        public event ValueChangeRequestHandler OnValueChangeRequest = delegate { };

        #endregion

        #region Public methods

        public IPursueDetector(ICue aCueIncrease, ICue aCueDecrease)
        {
            iCueIncrease = aCueIncrease;
            iCueDecrease = aCueDecrease;
        }

        public virtual void start()
        {
            iReady = false;
            iDataBuffer.Clear();
        }

        public virtual void reset()
        {
            iReady = false;
            iDataBuffer.Clear();
            //Console.WriteLine("=== SACCADE === ");
        }

        public virtual void addGazePoint(int aTimestamp, Point aPoint)
        {
            LimitBuffer(aTimestamp);

            Points.Gaze newGazePoint = CreateGazePoint(aTimestamp, aPoint);
            if (newGazePoint != null)
            {
                iDataBuffer.Enqueue(newGazePoint);

                if (iReady)
                {
                    Tracks.Track track = CreateTrack(newGazePoint);

                    if (track.isFollowingIncreaseCue())
                        OnValueChangeRequest(this, new ValueChangeRequestArgs(Direction.Increase));
                    else if (track.isFollowingDecreaseCue())
                        OnValueChangeRequest(this, new ValueChangeRequestArgs(Direction.Decrease));
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

        protected abstract Tracks.Track CreateTrack(Points.Gaze aNewDataPoint);
        
        protected virtual Points.Gaze CreateGazePoint(int aTimestamp, Point aPoint)
        {
            return new Points.Gaze(aTimestamp, aPoint);
        }

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
