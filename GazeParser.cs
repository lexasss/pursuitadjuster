using System;
using System.Collections.Generic;
using System.Drawing;

namespace SmoothVolume
{
    public class GazeParser
    {
        #region Declarations

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

        #endregion

        #region Consts

        private const double MIN_FIX_DIST = 50;      // pixels

        #endregion

        #region Internal members

        private Point iLastPoint = Point.Empty;

        private Queue<GazePoint> iPointBuffer = new Queue<GazePoint>();
        private System.Windows.Forms.Timer iPointsTimer = new System.Windows.Forms.Timer();

        #endregion

        #region Properties

        public IPursueDetector PursueDetector { get; set; }

        #endregion

        #region Public methods

        public GazeParser()
        {
            iPointsTimer.Interval = 30;
            iPointsTimer.Tick += PointsTimer_Tick;
        }

        public void start()
        {
            if (PursueDetector != null)
            {
                PursueDetector.start();
            }

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

        #endregion

        #region Internal methods

        private void EnsureSmoothPursuit(Point aPoint)
        {
            if (!iLastPoint.IsEmpty && PursueDetector != null)
            {
                int dx = aPoint.X - iLastPoint.X;
                int dy = aPoint.Y - iLastPoint.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist > MIN_FIX_DIST)
                {
                    PursueDetector.saccade();
                }
            }

            iLastPoint = aPoint;
        }

        private void ProcessNewPoint(int aTimestamp, Point aPoint)
        {
            EnsureSmoothPursuit(aPoint);

            if (PursueDetector != null)
            {
                PursueDetector.addGazePoint(aTimestamp, aPoint);
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

        #endregion
    }
}
