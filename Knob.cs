using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume
{
    public sealed class Knob
    {
        private readonly int IMAGE_WIDTH;
        private readonly int IMAGE_HEIGHT;
        private readonly int INDICATOR_WIDTH;
        private readonly int INDICATOR_HEIGHT;
        private const int INDICATOR_OFFSET = 150;
        private const float MIN_ANGLE = -135;
        private const float MAX_ANGLE = 135;
        private const double TARGET_SPEED = 1.4;
        private const double MIN_VALUE = 0;
        private const double MAX_VALUE = 100;

        private double iValue;
        private Image iIndicator;
        private Point iIndicatorLocation;

        private GazeTarget iIncrease;
        private GazeTarget iDecrease;
        
        public double MaxValue { get { return MAX_VALUE; } }
        public double Value   // 0-100
        {
            get { return iValue; }
            set {
                double prev = iValue;

                if (MIN_VALUE < MAX_VALUE)
                    iValue = Math.Max(0, Math.Min(100, value));
                else
                    iValue = value;

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

            IMAGE_WIDTH = aImageSize.Width;
            IMAGE_HEIGHT = aImageSize.Height;

            iIndicator = new Bitmap(global::SmoothVolume.Properties.Resources.indicator);
            INDICATOR_WIDTH = iIndicator.Width;
            INDICATOR_HEIGHT = iIndicator.Height;

            iIndicatorLocation = new Point(-INDICATOR_WIDTH / 2, -INDICATOR_OFFSET);
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
            aGraphics.TranslateTransform(IMAGE_WIDTH / 2, IMAGE_HEIGHT / 2);
            aGraphics.RotateTransform(MIN_ANGLE + ((float)Value * (MAX_ANGLE - MIN_ANGLE) / 100));
            aGraphics.DrawImage(iIndicator, iIndicatorLocation);
            aGraphics.EndContainer(container);

            aGraphics.DrawImage(iIncrease.Bitmap, iIncrease.Location);
            aGraphics.DrawImage(iDecrease.Bitmap, iDecrease.Location);
        }
    }
}
