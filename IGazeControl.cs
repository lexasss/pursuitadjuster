using System;
using System.Drawing;

namespace SmoothPursuit
{
    public enum GazeControlType
    {
        Knob,
        Scrollbar,
        Static
    }

    public abstract class IGazeControl
    {
        #region Consts

        protected const double MAX_VALUE = 255;
        private const uint VOLUME_MIN = 1;
        private const uint VOLUME_MAX = 16;
        private const uint VOLUME = 8;          // VOLUME_MIN..VOLUME_MAX, or 0 for the value-based

        #endregion

        #region Internal members

        private double iValue = -1;

        protected double iValueChangeStep = 1;
        
        protected Image iImage;                     // must be set in the derived class
        protected Detectors.IPursueDetector iPursueDetector;  // must be set in the derived class

        protected ICue iIncrease;   // must be set in the derived class
        protected ICue iDecrease;   // must be set in the derived class
        
        #endregion

        #region Properties

        public Detectors.IPursueDetector PursueDetector { get { return iPursueDetector; } }
        public Image Image { get { return iImage; } }
        public double Value
        {
            get { return iValue; }
            protected set
            {
                double prev = iValue;
                iValue = Math.Max(0, Math.Min(MAX_VALUE, value));

                if (prev != iValue)
                {
                    FireValueChanged(new ValueChangedArgs(prev, iValue));
                    RequestSound(prev);
                }
            }
        }

        #endregion

        #region Events

        public class ValueChangedArgs : EventArgs
        {
            public double Prev { get; private set; }
            public double Current { get; private set; }
            public ValueChangedArgs(double aPrev, double aCurrent)
            {
                Prev = aPrev;
                Current = aCurrent;
            }
        }
        public delegate void ValueChangedHandler(object aSender, ValueChangedArgs aArgs);
        public event ValueChangedHandler OnValueChanged = delegate { };

        public class SoundPlayRequestArgs : EventArgs
        {
            public uint Volume { get; private set; }
            public SoundPlayRequestArgs(uint aVolume)
            {
                Volume = aVolume;
            }
        }
        public delegate void SoundPlayRequestHandler(object aSender, SoundPlayRequestArgs aArgs);
        public event SoundPlayRequestHandler OnSoundPlayRequest = delegate { };

        public event EventHandler OnRedraw = delegate { };

        #endregion

        #region Public methods

        public virtual void setPursueDetectorType(Detectors.Type aType)
        {
            Detectors.IPursueDetector pd;
            switch (aType)
            {
                case Detectors.Type.OffsetXY:
                    pd = new Detectors.OffsetXY(iIncrease, iDecrease);
                    break;
                case Detectors.Type.OffsetDist:
                    pd = new Detectors.OffsetDist(iIncrease, iDecrease);
                    break;
                default:
                    throw new NotSupportedException(this.ToString() + " do not support this detector");
            }
            pd.OnValueChangeRequest += (s, e) => { Value += e.Direction == Detectors.IPursueDetector.Direction.Increase ? iValueChangeStep : -iValueChangeStep; };
            iPursueDetector = pd;
        }
        
        public virtual void start()
        {
            iIncrease.show();
            iDecrease.show();
        }

        public virtual void stop()
        {
            iIncrease.hide();
            iDecrease.hide();
        }

        public virtual void draw(Graphics aGraphics)
        {
            if (iDecrease.Visible)
            {
                aGraphics.DrawImage(iDecrease.Bitmap, iDecrease.Location);
                aGraphics.DrawImage(iIncrease.Bitmap, iIncrease.Location);
            }
        }

        public void reset()
        {
            Value = (int)(MAX_VALUE / 2 + 0.5);
        }

        #endregion

        #region Internal members

        protected virtual void FireValueChanged(ValueChangedArgs aArgs)
        {
            OnValueChanged(this, aArgs);
        }

        protected virtual void FireSoundPlayRequest(SoundPlayRequestArgs aArgs)
        {
            OnSoundPlayRequest(this, aArgs);
        }

        protected virtual void FireRedraw(EventArgs aArgs)
        {
            OnRedraw(this, aArgs);
        }

        protected virtual void RequestSound(double aPrevValue)
        {
            if ((int)(aPrevValue / 3) != (int)(Value / 3))
            {
                uint volume;
                if (VOLUME > 0)
                {
                    volume = VOLUME;
                }
                else
                {
                    volume = VOLUME_MIN + (uint)Math.Round((VOLUME_MAX - VOLUME_MIN) * Value / MAX_VALUE);
                }

                FireSoundPlayRequest(new SoundPlayRequestArgs(volume));
            }
        }

        #endregion
    }
}
