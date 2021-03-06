﻿using System;
using System.Drawing;

namespace SmoothPursuit.Rotation
{
    public sealed class Knob : IGazeControl
    {
        #region Consts

        private const int INDICATOR_OFFSET = 202;       // pixels from the center
        private const float MIN_ANGLE = -135;           // degrees
        private const float MAX_ANGLE = 135;            // degrees
        private const double TARGET_SPEED = 1.6;          // degrees per step
        
        #endregion

        #region Internal members

        private Image iIndicator;
        private Point iIndicatorLocation;

        #endregion

        #region Public methods

        public Knob()
        {
            iImage = global::SmoothPursuit.Properties.Resources.knob;

            iIncrease = new Cue(global::SmoothPursuit.Properties.Resources.increase, TARGET_SPEED, iImage.Size);
            iIncrease.OnVisibilityChanged += (s, e) => { FireRedraw(e); };

            iDecrease = new Cue(global::SmoothPursuit.Properties.Resources.decrease, -TARGET_SPEED, iImage.Size);
            iDecrease.OnLocationChanged += (s, e) => { FireRedraw(e); };
            iDecrease.OnVisibilityChanged += (s, e) => { FireRedraw(e); };

            iIndicator = new Bitmap(global::SmoothPursuit.Properties.Resources.indicator);
            iIndicatorLocation = new Point(-iIndicator.Width / 2, -INDICATOR_OFFSET);

            reset();

            setPursueDetectorType(Detectors.Type.OffsetXY);
        }

        public override void draw(Graphics aGraphics)
        {
            var container = aGraphics.BeginContainer();
            aGraphics.TranslateTransform(iImage.Width / 2, iImage.Height / 2);
            aGraphics.RotateTransform(MIN_ANGLE + (float)(Value * (MAX_ANGLE - MIN_ANGLE) / MAX_VALUE));
            aGraphics.DrawImage(iIndicator, iIndicatorLocation);
            aGraphics.EndContainer(container);

            base.draw(aGraphics);
        }

        public override string ToString()
        {
            return "KNOB";
        }

        #endregion
    }
}
