﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SmoothVolume
{
    public class GazeTarget
    {
        private const double RADIUS = 0.35;         // fraction of the min(width, height)
        private const double INITIAL_ANGLE = 90;    // degrees
        private const int ACCELERATION_STEPS = 20;  // number of steps taken to speed up o the full spped and slow down to stop
        private const int STEP_DURATION = 25;       // ms

        private readonly Point iCenter;
        private readonly double iRadius;
        private readonly double iSpeed;             // degrees per step

        private System.Windows.Forms.Timer iTimer = new System.Windows.Forms.Timer();
        private int iStepCounter;
        private Angle iAngle;                       // degrees

        // improved timer
        private long iStartTimestamp = 0;
        private HiResTimer iHRTimer = new HiResTimer();

        public class LocationChangedArgs: EventArgs
        {
            public Point Location { get; private set; }
            public LocationChangedArgs(Point aLocation)
            {
                Location = aLocation;
            }
        }
        public delegate void LocationChangedHandler(object aSender, LocationChangedArgs aArgs);
        public event LocationChangedHandler OnLocationChanged = delegate { };
        public event EventHandler OnVisibilityChanged = delegate { };

        public Bitmap Bitmap { get; private set; }
        public Point Location { get; private set; }

        // Something funny is here:
        //      if STEP_DURATION = 25, then the expected number of steps per second is 40
        //      However, it really makes only 32 steps... WTF!!
        //      MadFix: 1000 must be replaced by 800
        //      Better fix - controlling the duration since start and adjusting the timer interval
        public double Speed { get { return iSpeed * 1000 / STEP_DURATION; } }  // degrees per second
        public double Radius { get { return iRadius; } }

        public GazeTarget(Bitmap aBitmap, Size aKnobSize, double aSpeed)
        {
            Bitmap = aBitmap;
            iSpeed = aSpeed;

            Location = new Point(-100, -100);

            iTimer.Interval = STEP_DURATION;
            iTimer.Tick += Timer_Tick;

            iCenter = new Point(aKnobSize.Width / 2, aKnobSize.Height / 2);
            iRadius = Math.Min(aKnobSize.Width, aKnobSize.Height) * RADIUS;
        }

        public void show()
        {
            iStepCounter = 0;

            OnVisibilityChanged(this, new EventArgs());
            SetAngle(INITIAL_ANGLE + 2 * iSpeed);

            iTimer.Start();

            iStartTimestamp = iHRTimer.Milliseconds;
        }

        public void hide()
        {
            iStepCounter = -ACCELERATION_STEPS;
        }

        private void SetAngle(double aAngle)
        {
            iAngle = new Angle(aAngle, true);

            double dx = iRadius * Math.Cos(iAngle.Radians);
            double dy = iRadius * Math.Sin(iAngle.Radians);

            Location = new Point(
                (int)(iCenter.X - Bitmap.Width / 2 + dx), 
                (int)(iCenter.Y - Bitmap.Height / 2 + dy));

            OnLocationChanged(this, new LocationChangedArgs(Location));
        }

        private void Timer_Tick(object aSender, EventArgs e)
        {
            iStepCounter++;
            if (iStepCounter == 0)
            {
                iTimer.Stop();
                Location = new Point(-100, -100);
                OnVisibilityChanged(this, new EventArgs());
                return;
            }
            else
            {
                double speed = iSpeed;
                if (iStepCounter < ACCELERATION_STEPS)
                {
                    Angle angle = new Angle(90 * (Math.Abs((double)iStepCounter) / ACCELERATION_STEPS), true);
                    speed = iSpeed * Math.Sin(angle.Radians);
                }

                SetAngle(iAngle.Degrees + speed);
            }

            // Improving timer
            iTimer.Stop();
            if (iStepCounter > 0)
            {
                long timestamp = iHRTimer.Milliseconds - iStartTimestamp;
                int delay = Math.Max((int)(timestamp - iStepCounter * STEP_DURATION), 0);
                iTimer.Interval = Math.Max(3, STEP_DURATION - delay);
            }
            else
            {
                iTimer.Interval = STEP_DURATION;
            }

            iTimer.Start();
        }
    }
}
