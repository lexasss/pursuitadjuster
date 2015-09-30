using System;

namespace SmoothPursuit
{
    public class Angle
    {
        #region Consts

        private const double CYCLE = 2 * Math.PI;

        #endregion

        #region Internal members

        private int iCycles = 0;

        #endregion

        #region Properties

        public int Cycles { get { return iCycles; } }
        public double Radians { get; set; }
        public double Degrees
        {
            get { return Radians * 180.0 / Math.PI; }
            set { Radians = value * Math.PI / 180.0; }
        }

        #endregion

        #region Public methods

        public Angle()
        {
            Radians = 0.0;
        }

        public Angle(double aRadians)
        {
            Radians = aRadians;
            iCycles = CalcCycles();
        }

        public Angle(double aRadians, int aCycles)
        {
            Radians = aRadians;
            iCycles = aCycles;
        }

        public Angle(double aValue, bool aIsValueInDegrees)
        {
            if (aIsValueInDegrees)
                Degrees = aValue;
            else
                Radians = aValue;
            
            iCycles = CalcCycles();
        }

        public static Angle operator -(Angle aAngle1, Angle aAngle2)
        {
            return new Angle(aAngle1.Radians - aAngle2.Radians);
        }

        public static Angle operator +(Angle aAngle1, Angle aAngle2)
        {
            return new Angle(aAngle1.Radians + aAngle2.Radians);
        }

        public Angle rotateBy(int aCycles)
        {
            iCycles += aCycles;
            Radians += aCycles * CYCLE;
            return this;
        }

        public Angle keepCloseTo(Angle aRef, ref int aCycles)
        {
            if ((Radians - aRef.Radians) > Math.PI)
            {
                aCycles--;
                Radians -= CYCLE;
            }
            else if ((Radians - aRef.Radians) < -Math.PI)
            {
                aCycles++;
                Radians += CYCLE;
            }
            return this;
        }

        public Angle normalize()
        {
            iCycles = 0;

            while (Radians >= CYCLE)
                Radians -= CYCLE;
            while (Radians <= -CYCLE)
                Radians += CYCLE;

            return this;
        }

        public override string ToString()
        {
            return String.Format("D: {0:N1}, R: {1:N3}", Degrees, Radians);
        }

        #endregion

        #region Internal methods

        private int CalcCycles()
        {
            int result = 0;
            double radians = Radians;

            while (radians >= CYCLE)
            {
                radians -= CYCLE;
                result++;
            }
            while (radians <= -CYCLE)
            {
                radians += CYCLE;
                result--;
            }
            return result;
        }

        #endregion
    }
}
