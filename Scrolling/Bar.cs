using System;
using System.Drawing;

namespace SmoothPursuit.Scrolling
{
    public sealed class Bar : IGazeControl
    {
        #region Consts

        private const double TARGET_SPEED = 3.7;    // pixels per step

        private const int SLIDER_X = 44;
        private const int SLIDER_Y = 330;
        private const int SLIDER_WIDTH = 411;
        private const int SLIDER_HEIGHT = 40;

        #endregion

        #region Internal members

        private Image iThumb;
        private Point iThumbLocation;
        private int iThumbLength;

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
            Detectors.OffsetA pd = new Detectors.OffsetA(iIncrease, iDecrease);
            pd.OnValueChangeRequest += (s, e) => { Value += e.Direction == Detectors.IPursueDetector.Direction.Increase ? iValueChangeStep : -iValueChangeStep; };

            iPursueDetector = pd;
        }

        public override void draw(Graphics aGraphics)
        {
            aGraphics.DrawImage(iThumb, iThumbLocation);
            base.draw(aGraphics);
        }

        public override string ToString()
        {
            return "SCROLLBAR";
        }

        #endregion

        #region Internal methods

        protected override void FireValueChanged(ValueChangedArgs aArgs)
        {
            iThumbLocation = new Point(
                SLIDER_X + (int)(Value / MAX_VALUE * (SLIDER_WIDTH - iThumbLength)),
                SLIDER_Y
            );
            
            base.FireValueChanged(aArgs);
        }

        #endregion
    }
}
