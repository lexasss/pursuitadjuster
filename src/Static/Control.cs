using System.Drawing;

namespace SmoothPursuit.Static
{
    public class Control : IGazeControl
    {
        public Control()
        {
            iImage = new Bitmap(500, 500);

            iDecrease = new Cue(global::SmoothPursuit.Properties.Resources.decrease,
                new Point((int)(0.2 * iImage.Width), iImage.Height / 2));
            iDecrease.OnLocationChanged += (s, e) => { FireRedraw(e); };
            iDecrease.OnVisibilityChanged += (s, e) => { FireRedraw(e); };

            iIncrease = new Cue(global::SmoothPursuit.Properties.Resources.increase,
                new Point((int)(0.8 * iImage.Width), iImage.Height / 2));
            iIncrease.OnLocationChanged += (s, e) => { FireRedraw(e); };
            iIncrease.OnVisibilityChanged += (s, e) => { FireRedraw(e); };

            reset();

            Detectors.Dwell dwellDetector = new Detectors.Dwell(iIncrease.Center, iDecrease.Center);
            dwellDetector.OnValueChangeRequest += (s, e) => { Value += e.Direction == Detectors.IPursueDetector.Direction.Increase ? VALUE_CHANGE_STEP : -VALUE_CHANGE_STEP; };

            iPursueDetector = dwellDetector;
        }

        public override void setPursueDetectorType(Detectors.Type aType)
        {
        }

        public override string ToString()
        {
            return "STATIC";
        }
    }
}
