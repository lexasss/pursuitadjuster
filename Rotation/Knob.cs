using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;

namespace SmoothVolume.Rotation
{
    public sealed class Knob : IGazeControl
    {
        #region Consts

        private const int INDICATOR_OFFSET = 150;       // pixels
        private const float MIN_ANGLE = -135;           // degrees
        private const float MAX_ANGLE = 135;            // degrees
        private const double TARGET_SPEED = 1.4;        // degrees per step
        private const double MAX_VALUE = 100;
        
        private readonly int iIndicatorWidth;
        private readonly int iIndicatorHeight;

        #endregion

        #region Internal members

        private double iValue;
        private Image iIndicator;
        private Point iIndicatorLocation;

        private Cue iIncrease;
        private Cue iDecrease;

        #endregion

        #region Events

        public override event ValueChangedHandler OnValueChanged = delegate { };
        public override event SoundPlayRequestHandler OnSoundPlayRequest = delegate { };
        public override event EventHandler OnRedraw = delegate { };

        #endregion

        #region Properties

        public override double Value
        {
            get { return iValue; }
            protected set {
                double prev = iValue;
                iValue = Math.Max(0, Math.Min(MAX_VALUE, value));

                if (prev != iValue)
                {
                    OnValueChanged(this, new ValueChangedArgs(prev, iValue));
                    RequestSound(prev);
                }
            }
        }

        #endregion

        #region Public methods

        public Knob()
        {
            Value = 50;

            iImage = global::SmoothVolume.Properties.Resources.knob;

            iIncrease = new Cue(global::SmoothVolume.Properties.Resources.increase, iImage.Size, TARGET_SPEED);
            iIncrease.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iDecrease = new Cue(global::SmoothVolume.Properties.Resources.decrease, iImage.Size, -TARGET_SPEED);
            iDecrease.OnLocationChanged += (s, e) => { OnRedraw(this, e); };
            iDecrease.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iIndicator = new Bitmap(global::SmoothVolume.Properties.Resources.indicator);
            iIndicatorWidth = iIndicator.Width;
            iIndicatorHeight = iIndicator.Height;

            iIndicatorLocation = new Point(-iIndicatorWidth / 2, -INDICATOR_OFFSET);

            PursueDetector pd = new PursueDetector(iImage.Width / 2, iImage.Height / 2, iIncrease.Radius, iIncrease.Speed);
            pd.OnAngleChanged += (s, e) => { Value += e.AngleChange; };

            iPursueDetector = pd;
        }

        public override void start()
        {
            iIncrease.show();
            iDecrease.show();
        }

        public override void stop()
        {
            iIncrease.hide();
            iDecrease.hide();
        }

        public override void draw(Graphics aGraphics)
        {
            var container = aGraphics.BeginContainer();
            aGraphics.TranslateTransform(iImage.Width / 2, iImage.Height / 2);
            aGraphics.RotateTransform(MIN_ANGLE + (float)(Value * (MAX_ANGLE - MIN_ANGLE) / MAX_VALUE));
            aGraphics.DrawImage(iIndicator, iIndicatorLocation);
            aGraphics.EndContainer(container);

            aGraphics.DrawImage(iIncrease.Bitmap, iIncrease.Location);
            aGraphics.DrawImage(iDecrease.Bitmap, iDecrease.Location);
        }

        #endregion

        #region Internal members

        private void RequestSound(double aPrevValue)
        {
            if ((int)(aPrevValue / 3) != (int)(iValue / 3))
            {
                uint volume = 1 + (uint)Math.Round(15 * iValue / MAX_VALUE);
                OnSoundPlayRequest(this, new SoundPlayRequestArgs(volume));
            }
        }

        #endregion
    }
}
