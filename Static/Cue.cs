using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SmoothPursuit.Static
{
    public class Cue : ICue
    {
        #region Internal members

        private Point iStaticLocation;

        #endregion

        #region Public methods

        public Cue(Bitmap aBitmap, Point aLocation)
            : base(aBitmap, 0)
        {
            iStaticLocation = new Point(aLocation.X  - aBitmap.Width / 2, aLocation.Y  - aBitmap.Height / 2);
            Location = iStaticLocation;
        }

        #endregion

        #region Internal methods

        protected override void SetInitialLocation()
        {
            Location = iStaticLocation;
        }

        protected override void UpdateLocation()
        {
        }

        #endregion
    }
}
