using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume
{
    public interface IGazeControllable
    {
        void invalidate();
        void addGazePoint(int aTimestamp, Point aPoint);
    }
}
