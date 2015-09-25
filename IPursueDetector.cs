using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothVolume
{
    public interface IPursueDetector
    {
        void start();
        void saccade();
        void addGazePoint(int aTimestamp, Point aPoint);
    }
}
