using System;
using System.Drawing;

namespace SmoothPursuit.Scrolling
{
    public sealed class Bar : IGazeControl
    {
        #region Consts

        private const int TARGET_SPEED = 3;     // pixels per step

        private const int SLIDER_X = 44;
        private const int SLIDER_Y = 337;
        private const int SLIDER_WIDTH = 411;
        private const int SLIDER_HEIGHT = 40;

        #endregion

        #region Internal members

        private double iValue = -1;
        private Image iThumb;
        private Point iThumbLocation;
        private int iThumbLength;

        private Cue iDecrease;
        private Cue iIncrease;

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
                    FireValueChanged(new ValueChangedArgs(prev, iValue));
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

            iDecrease = new Cue(global::SmoothPursuit.Properties.Resources.decrease, -TARGET_SPEED, cuePath);
            iDecrease.OnVisibilityChanged += (s, e) => { FireRedraw(e); };

            iIncrease = new Cue(global::SmoothPursuit.Properties.Resources.increase, TARGET_SPEED, cuePath);
            iIncrease.OnLocationChanged += (s, e) => { FireRedraw(e); };
            iIncrease.OnVisibilityChanged += (s, e) => { FireRedraw(e); };

            iThumb = new Bitmap(global::SmoothPursuit.Properties.Resources.thumb);
            iThumbLength = iThumb.Width;

            reset();

            //PursueDetector pd = new PursueDetector(new Rectangle(SLIDER_X, SLIDER_Y, SLIDER_WIDTH, SLIDER_HEIGHT), iIncrease.Speed);
            OffsetPursueDetector pd = new OffsetPursueDetector(iIncrease, iDecrease);
            pd.OnValueChangeRequest += (s, e) => { Value += e.Direction == IPursueDetector.Direction.Increase ? iValueChangeStep : -iValueChangeStep; };

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
    }
}
