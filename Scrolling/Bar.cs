using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume.Scrolling
{
    public sealed class Bar : IGazeControl
    {
        #region Consts

        private const int TARGET_SPEED = 3;     // pixels per step
        private const double MAX_VALUE = 100;
        private const uint VOLUME = 8;          // 1..16

        private const int SLIDER_X = 447;
        private const int SLIDER_Y = 44;
        private const int SLIDER_WIDTH = 36;
        private const int SLIDER_HEIGHT = 411;

        #endregion

        #region Internal members

        private double iValue = -1;
        private Image iThumb;
        private Point iThumbLocation;

        private Cue iUp;
        private Cue iDown;

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
                        SLIDER_X,
                        SLIDER_Y + (int)(iValue / MAX_VALUE * (SLIDER_HEIGHT - iThumb.Height))
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
            iImage = global::SmoothVolume.Properties.Resources.scrollbar;
            Rectangle cuePath = new Rectangle(SLIDER_X, SLIDER_Y, SLIDER_WIDTH, SLIDER_HEIGHT);

            iUp = new Cue(global::SmoothVolume.Properties.Resources.up, -TARGET_SPEED, cuePath);
            iUp.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iDown = new Cue(global::SmoothVolume.Properties.Resources.down, TARGET_SPEED, cuePath);
            iDown.OnLocationChanged += (s, e) => { OnRedraw(this, e); };
            iDown.OnVisibilityChanged += (s, e) => { OnRedraw(this, e); };

            iThumb = new Bitmap(global::SmoothVolume.Properties.Resources.thumb);

            Value = MAX_VALUE / 2;

            PursueDetector pd = new PursueDetector(new Rectangle(SLIDER_X, SLIDER_Y, SLIDER_WIDTH, SLIDER_HEIGHT), iDown.Speed);
            pd.OnValueChangeRequest += (s, e) => { Value += e.ValueChange; };

            iPursueDetector = pd;
        }

        public override void start()
        {
            iUp.show();
            iDown.show();
        }

        public override void stop()
        {
            iUp.hide();
            iDown.hide();
        }

        public override void draw(Graphics aGraphics)
        {
            var container = aGraphics.BeginContainer();
            aGraphics.DrawImage(iThumb, iThumbLocation);
            aGraphics.DrawImage(iUp.Bitmap, iUp.Location);
            aGraphics.DrawImage(iDown.Bitmap, iDown.Location);
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
