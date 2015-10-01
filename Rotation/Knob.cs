using System;
using System.Drawing;

namespace SmoothPursuit.Rotation
{
    public sealed class Knob : IGazeControl
    {
        #region Consts

        private const int INDICATOR_OFFSET = 150;       // pixels
        private const float MIN_ANGLE = -135;           // degrees
        private const float MAX_ANGLE = 135;            // degrees
        private const double TARGET_SPEED = 1.4;        // degrees per step
        private const double MAX_VALUE = 255;
        private const uint VOLUME_MIN = 1;
        private const uint VOLUME_MAX = 16;
        
        #endregion

        #region Internal members

        private double iValue = -1;
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
            iImage = global::SmoothPursuit.Properties.Resources.knob;

            iIncrease = new Cue(global::SmoothPursuit.Properties.Resources.increase, TARGET_SPEED, iImage.Size);
            iIncrease.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iDecrease = new Cue(global::SmoothPursuit.Properties.Resources.decrease, -TARGET_SPEED, iImage.Size);
            iDecrease.OnLocationChanged += (s, e) => { OnRedraw(this, e); };
            iDecrease.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iIndicator = new Bitmap(global::SmoothPursuit.Properties.Resources.indicator);

            iIndicatorLocation = new Point(-iIndicator.Width / 2, -INDICATOR_OFFSET);

            Value = (int)(MAX_VALUE / 2);

            //PursueDetector pd = new PursueDetector(iImage.Width / 2, iImage.Height / 2, iIncrease.Radius, iIncrease.Speed * Math.PI / 180);
            OffsetPursueDetector pd = new OffsetPursueDetector(iIncrease, iDecrease);
            pd.OnValueChangeRequest += (s, e) => { Value += e.Direction == IPursueDetector.Direction.Increase ? iValueChangeStep : -iValueChangeStep; };

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
                uint volume = VOLUME_MIN + (uint)Math.Round((VOLUME_MAX - VOLUME_MIN) * iValue / MAX_VALUE);
                OnSoundPlayRequest(this, new SoundPlayRequestArgs(volume));
            }
        }

        #endregion
    }
}
