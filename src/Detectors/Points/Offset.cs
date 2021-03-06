﻿using System.Drawing;
using System.Text;

namespace SmoothPursuit.Detectors.Points
{
    public class Offset : Gaze
    {
        public Point OffsetIncrease { get; private set; }
        public Point OffsetDecrease { get; private set; }

        public Offset(int aTimestamp, Point aLocation, Point aOffsetIncrease, Point aOffsetDecrease)
            : base(aTimestamp, aLocation)
        {
            OffsetIncrease = aOffsetIncrease;
            OffsetDecrease = aOffsetDecrease;
        }

        public override string ToString()
        {
            return new StringBuilder(base.ToString()).
                AppendFormat("\t{0},{1}", OffsetIncrease.X, OffsetIncrease.Y).
                AppendFormat("\t{0},{1}", OffsetDecrease.X, OffsetDecrease.Y).
                ToString();
        }
    }

}
