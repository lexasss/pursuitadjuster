using System;
using System.Drawing;

namespace SmoothPursuit
{
    public abstract class IGazeControl
    {
        #region Consts

        protected const double MAX_VALUE = 255;
        private const uint VOLUME_MIN = 1;
        private const uint VOLUME_MAX = 16;
        private const uint VOLUME = 8;          // VOLUME_MIN..VOLUME_MAX, or 0 for the value-based

        #endregion

        #region Internal members

        protected Image iImage;
        protected IPursueDetector iPursueDetector;
        protected double iValueChangeStep = 1;

        #endregion

        #region Properties

        public abstract double Value { get; protected set; }
        public IPursueDetector PursueDetector { get { return iPursueDetector; } }
        public Image Image { get { return iImage; } }

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

        #region Interface

        public abstract void start();
        public abstract void stop();
        public abstract void draw(Graphics aGraphics);

        public void reset()
        {
            Value = (int)(MAX_VALUE / 2);
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
