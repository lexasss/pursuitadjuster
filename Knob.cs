using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume
{
    public sealed class Knob
    {
        private const int INDICATOR_OFFSET = 150;       // pixels
        private const float MIN_ANGLE = -135;           // degrees
        private const float MAX_ANGLE = 135;            // degrees
        private const double TARGET_SPEED = 1.4;        // degrees per step
        private const double MAX_VALUE = 100;
        
        private readonly int iImageWidth;
        private readonly int iImageHeight;
        private readonly int iIndicatorWidth;
        private readonly int iIndicatorHeight;

        private double iValue;
        private Image iIndicator;
        private Point iIndicatorLocation;

        private GazeTarget iIncrease;
        private GazeTarget iDecrease;
        
        public double MaxValue { get { return MAX_VALUE; } }
        public double Value
        {
            get { return iValue; }
            set {
                double prev = iValue;
                iValue = Math.Max(0, Math.Min(MAX_VALUE, value));

                if (prev != iValue)
                    OnValueChanged(this, new ValueChangedArgs(prev, iValue)); 
            }
        }

        public double TargetSpeed { get { return iIncrease.Speed; } }
        public double TargetRadius { get { return iIncrease.Radius; } }

        public class ValueChangedArgs: EventArgs
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
        public event EventHandler OnRedraw = delegate { };

        public Knob(Size aImageSize)
        {
            Value = 50;

            iIncrease = new GazeTarget(global::SmoothVolume.Properties.Resources.increase, aImageSize, TARGET_SPEED);
            iIncrease.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };
            
            iDecrease = new GazeTarget(global::SmoothVolume.Properties.Resources.decrease, aImageSize, -TARGET_SPEED);
            iDecrease.OnLocationChanged += (s, e) => { OnRedraw(this, e); };
            iDecrease.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iImageWidth = aImageSize.Width;
            iImageHeight = aImageSize.Height;

            iIndicator = new Bitmap(global::SmoothVolume.Properties.Resources.indicator);
            iIndicatorWidth = iIndicator.Width;
            iIndicatorHeight = iIndicator.Height;

            iIndicatorLocation = new Point(-iIndicatorWidth / 2, -INDICATOR_OFFSET);
        }

        public void start()
        {
            iIncrease.show();
            iDecrease.show();
        }

        public void stop()
        {
            iIncrease.hide();
            iDecrease.hide();
        }

        public void draw(Graphics aGraphics)
        {
            var container = aGraphics.BeginContainer();
            aGraphics.TranslateTransform(iImageWidth / 2, iImageHeight / 2);
            aGraphics.RotateTransform(MIN_ANGLE + (float)(Value * (MAX_ANGLE - MIN_ANGLE) / MAX_VALUE));
            aGraphics.DrawImage(iIndicator, iIndicatorLocation);
            aGraphics.EndContainer(container);

            aGraphics.DrawImage(iIncrease.Bitmap, iIncrease.Location);
            aGraphics.DrawImage(iDecrease.Bitmap, iDecrease.Location);
        }
    }
}
