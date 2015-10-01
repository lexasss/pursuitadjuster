﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothPursuit.Static
{
    public class DwellDetector : IPursueDetector
    {
        #region Declarations

        private class DwellArea
        {
            private const int GAZE_RESPONSIVE_AREA = 160;        // pixels
            private const int DWELL_TIME = 800;                  // ms
            private const int ACCUMULATOR_HIST_DURATION = 200;   // ms
            private const int MAX_ACCUMULATED_TIME = DWELL_TIME + ACCUMULATOR_HIST_DURATION;
            
            private Rectangle iRect;
            private int iTimeAccumulator = 0;

            public bool Activated { get { return iTimeAccumulator >= DWELL_TIME; } }

            public DwellArea(Point aCenter)
            {
                int radius = GAZE_RESPONSIVE_AREA / 2;
                iRect = new Rectangle(aCenter.X - radius, aCenter.Y - radius, GAZE_RESPONSIVE_AREA, GAZE_RESPONSIVE_AREA);
            }

            public void reset()
            {
                iTimeAccumulator = 0;
            }

            public void feed(Point aGazePoint)
            {
                if (iRect.Contains(aGazePoint))
                {
                    iTimeAccumulator += GazeParser.SAMPLE_INTERVAL;
                    if (iTimeAccumulator > MAX_ACCUMULATED_TIME)
                        iTimeAccumulator = MAX_ACCUMULATED_TIME;
                }
                else
                {
                    iTimeAccumulator -= GazeParser.SAMPLE_INTERVAL;
                    if (iTimeAccumulator < 0)
                        iTimeAccumulator = 0;
                }
            }
        }

        private class DwellTrack : Track
        {
            public DwellTrack()
                : base(null, null)
            {
            }

            public void updateState(bool aIsIncreaseActivated, bool aIsDecreaseActivated)
            {
                if (aIsIncreaseActivated)
                    State = IPursueDetector.State.Increase;
                else if (aIsDecreaseActivated)
                    State = IPursueDetector.State.Decrease;
                else
                    State = IPursueDetector.State.Unknown;
            }

            public override string ToString()
            {
                return new StringBuilder(base.ToString()).
                    AppendFormat("\t{0,8:N3}", 11.1).
                    ToString();
            }
        }

        #endregion

        #region Internal members

        private DwellArea iIncreaseArea;
        private DwellArea iDecreaseArea;
        private DwellTrack iTrack;

        #endregion

        #region Public methods

        public DwellDetector(Point aCueIncrease, Point aCueDecrease)
            : base()
        {
            iIncreaseArea = new DwellArea(aCueIncrease);
            iDecreaseArea = new DwellArea(aCueDecrease);
            iTrack = new DwellTrack();
        }

        #endregion

        protected override IPursueDetector.DataPoint CreateDataPoint(int aTimestamp, Point aPoint)
        {
            return new GazePoint(aTimestamp, aPoint);
        }

        protected override IPursueDetector.Track CreateTrack(IPursueDetector.DataPoint aFirstDataPoint, IPursueDetector.DataPoint aLastDataPoint)
        {
            GazePoint point = (GazePoint)aLastDataPoint;
            iIncreaseArea.feed(point.Location);
            iDecreaseArea.feed(point.Location);
            iTrack.updateState(iIncreaseArea.Activated, iDecreaseArea.Activated);
            return iTrack;
        }
    }
}
