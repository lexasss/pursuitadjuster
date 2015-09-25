using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume
{
    public abstract class IGazeControl
    {
        #region Internal members

        protected Image iImage;
        protected IPursueDetector iPursueDetector;

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
        public abstract event ValueChangedHandler OnValueChanged;

        public class SoundPlayRequestArgs : EventArgs
        {
            public uint Volume { get; private set; }
            public SoundPlayRequestArgs(uint aVolume)
            {
                Volume = aVolume;
            }
        }
        public delegate void SoundPlayRequestHandler(object aSender, SoundPlayRequestArgs aArgs);
        public abstract event SoundPlayRequestHandler OnSoundPlayRequest;
        
        public abstract event EventHandler OnRedraw;

        #endregion

        #region Interface

        public abstract void start();
        public abstract void stop();
        public abstract void draw(Graphics aGraphics);

        #endregion
    }
}
