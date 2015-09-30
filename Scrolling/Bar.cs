using System;
using System.Drawing;

namespace SmoothPursuit.Scrolling
{
    public sealed class Bar : IGazeControl
    {
        #region Consts

        private const int TARGET_SPEED = 3;     // pixels per step
        private const double MAX_VALUE = 255;
        private const uint VOLUME = 8;          // 1..16

        private const int SLIDER_X = 44;
        private const int SLIDER_Y = 447;
        private const int SLIDER_WIDTH = 411;
        private const int SLIDER_HEIGHT = 36;

        #endregion

        #region Internal members

        private double iValue = -1;
        private Image iThumb;
        private Point iThumbLocation;
        private int iThumbLength;

        private Cue iDecrease;
        private Cue iIncrease;

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
            protected set
            {
                double prev = iValue;
                iValue = Math.Max(0, Math.Min(MAX_VALUE, value));

                if (prev != iValue)
                {
                    iThumbLocation = new Point(
                        SLIDER_X + (int)(iValue / MAX_VALUE * (SLIDER_WIDTH - iThumbLength)),
                        SLIDER_Y
                    );
                    OnValueChanged(this, new ValueChangedArgs(prev, iValue));
                    RequestSound(prev);
                }
            }
        }

        #endregion

        #region Public methods

        public Bar()
        {
            iImage = global::SmoothPursuit.Properties.Resources.scrollbar;
            Rectangle cuePath = new Rectangle(SLIDER_X, SLIDER_Y, SLIDER_WIDTH, SLIDER_HEIGHT);

            iDecrease = new Cue(global::SmoothPursuit.Properties.Resources.left, -TARGET_SPEED, cuePath);
            iDecrease.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iIncrease = new Cue(global::SmoothPursuit.Properties.Resources.right, TARGET_SPEED, cuePath);
            iIncrease.OnLocationChanged += (s, e) => { OnRedraw(this, e); };
            iIncrease.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iThumb = new Bitmap(global::SmoothPursuit.Properties.Resources.thumb);
            iThumbLength = iThumb.Width;

            Value = (int)(MAX_VALUE / 2);

            //PursueDetector pd = new PursueDetector(new Rectangle(SLIDER_X, SLIDER_Y, SLIDER_WIDTH, SLIDER_HEIGHT), iIncrease.Speed);
            OffsetPursueDetector pd = new OffsetPursueDetector(iIncrease, iDecrease);
            pd.OnValueChangeRequest += (s, e) => { Value += e.ValueChange; };

            iPursueDetector = pd;
        }

        public override void start()
        {
            iDecrease.show();
            iIncrease.show();
        }

        public override void stop()
        {
            iDecrease.hide();
            iIncrease.hide();
        }

        public override void draw(Graphics aGraphics)
        {
            var container = aGraphics.BeginContainer();
            aGraphics.DrawImage(iThumb, iThumbLocation);
            aGraphics.DrawImage(iDecrease.Bitmap, iDecrease.Location);
            aGraphics.DrawImage(iIncrease.Bitmap, iIncrease.Location);
            aGraphics.EndContainer(container);
        }

        #endregion

        #region Internal members

        private void RequestSound(double aPrevValue)
        {
            if ((int)(aPrevValue / 3) != (int)(iValue / 3))
            {
                OnSoundPlayRequest(this, new SoundPlayRequestArgs(VOLUME));
            }
        }

        #endregion
    }
}
