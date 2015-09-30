using System;
using System.Drawing;

namespace SmoothPursuit
{
    public abstract class ICue
    {
        #region Consts

        protected const int ACCELERATION_STEPS = 20;  // number of steps taken to speed up to the full speed and to slow down to stop
        private const int STEP_DURATION = 25;         // ms

        #endregion

        #region Internal members

        protected readonly double iSpeed;       // per step
        protected int iStepCounter;

        private long iLocationX;
        private long iLocationY;
        
        //private long iStartTimestamp = 0;
        //private HiResTimestamp iHRTimestamp = new HiResTimestamp();
        
        // improved timer
        private MicroLib.MicroTimer iTimer = new MicroLib.MicroTimer();

        #endregion

        #region Events

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

        #endregion

        #region Properties

        public Bitmap Bitmap { get; protected set; }
        public Point Location
        {
            get
            {
                return new Point(
                    (int)System.Threading.Interlocked.Read(ref iLocationX),
                    (int)System.Threading.Interlocked.Read(ref iLocationY));
            }
            protected set
            {
                System.Threading.Interlocked.Exchange(ref iLocationX, value.X);
                System.Threading.Interlocked.Exchange(ref iLocationY, value.Y);
            }
        }

        // Something funny is here:
        //      if STEP_DURATION = 25, then the expected number of steps per second is 40
        //      However, it really makes only 32 steps... WTF!!
        //      MadFix - 1000 must be replaced by 800
        //      Better fix - controlling the duration since start and adjusting the timer interval
        //      Event better fix - find a good timer [IMPLEMENTED]
        public double Speed { get { return iSpeed * 1000 / STEP_DURATION; } }   // per second

        #endregion

        #region Public methods

        public ICue(Bitmap aBitmap, double aSpeed)
        {
            Bitmap = aBitmap;
            iSpeed = aSpeed;

            Location = new Point(-100, -100);

            iTimer.Interval = STEP_DURATION;
            iTimer.Tick += Timer_Tick;
        }

        public virtual void show()
        {
            iStepCounter = 0;

            OnVisibilityChanged(this, new EventArgs());

            SetInitialLocation();
            OnLocationChanged(this, new LocationChangedArgs(Location));

            iTimer.Start();

            //iStartTimestamp = iHRTimestamp.Milliseconds;
        }

        public virtual void hide()
        {
            iStepCounter = -ACCELERATION_STEPS;
        }

        #endregion

        #region Internal methods

        protected abstract void SetInitialLocation();
        protected abstract void UpdateLocation();

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
                UpdateLocation();
                OnLocationChanged(this, new LocationChangedArgs(Location));
            }

            // Improved timer
            /*
            iTimer.Stop();
            if (iStepCounter > 0)
            {
                long duration = iHRTimestamp.Milliseconds - iStartTimestamp;
                int delay = Math.Max((int)(duration - iStepCounter * STEP_DURATION), 0);
                iTimer.Interval = Math.Max(3, STEP_DURATION - delay);
            }
            else
            {
                iTimer.Interval = STEP_DURATION;
            }

            iTimer.Start();
             */
        }

        #endregion
    }
}
