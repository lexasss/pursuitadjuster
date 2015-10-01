using System;
using System.Collections.Generic;
using System.Drawing;

namespace SmoothPursuit
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

        public static int SAMPLE_INTERVAL { get { return 30; } }

        private const bool ENSURE_SMOOTH_PURSUIT = false;
        private const float ALPHA = 1f;
        private const double MIN_FIX_DIST = 70;     // pixels
        private const int MAX_OFFSET = 0;         // pixels
        
        private readonly int OFFSET_X = 0;          // pixels
        private readonly int OFFSET_Y = 0;          // pixels

        #endregion

        #region Internal members

        private PointF iLastPoint = PointF.Empty;

        private Queue<GazePoint> iPointBuffer = new Queue<GazePoint>();
        private System.Windows.Forms.Timer iPointsTimer = new System.Windows.Forms.Timer();

        #endregion

        #region Properties

        public IPursueDetector PursueDetector { get; set; }

        #endregion

        #region Public methods

        public GazeParser()
        {
            Random rand = new Random();
            if (MAX_OFFSET > 0)
            {
                OFFSET_X = rand.Next(2 * MAX_OFFSET) - MAX_OFFSET;
                OFFSET_Y = rand.Next(2 * MAX_OFFSET) - MAX_OFFSET;
            }

            iPointsTimer.Interval = SAMPLE_INTERVAL;
            iPointsTimer.Tick += PointsTimer_Tick;
        }

        public void start()
        {
            iLastPoint = PointF.Empty;

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
            Point gazePoint = new Point(aPoint.X + OFFSET_X, aPoint.Y + OFFSET_Y);
            lock (iPointBuffer)
            {
                iPointBuffer.Enqueue(new GazePoint(aTimestamp, gazePoint));
            }
        }

        #endregion

        #region Internal methods

        private void EnsureSmoothPursuit(Point aPoint)
        {
            PointF smoothedPoint = new PointF(aPoint.X, aPoint.Y);
            if (!iLastPoint.IsEmpty && PursueDetector != null)
            {
                smoothedPoint = new PointF(
                    (aPoint.X + ALPHA * iLastPoint.X) / (1.0f + ALPHA),
                    (aPoint.Y + ALPHA * iLastPoint.Y) / (1.0f + ALPHA)
                );

                double dx = smoothedPoint.X - iLastPoint.X;
                double dy = smoothedPoint.Y - iLastPoint.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                Console.WriteLine("{0:N0}", distance);
                if (distance > MIN_FIX_DIST)
                {
                    PursueDetector.saccade();
                }
            }

            iLastPoint = smoothedPoint;
        }

        private void ProcessNewPoint(int aTimestamp, Point aPoint)
        {
            if (ENSURE_SMOOTH_PURSUIT)
            {
                EnsureSmoothPursuit(aPoint);
            }

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
