using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SmoothPursuit
{
    public partial class Options : Form
    {
        private Dictionary<RadioButton, Detectors.Type> iPursueDetectors = new Dictionary<RadioButton, Detectors.Type>();
        private Dictionary<RadioButton, GazeControlType> iWidgets = new Dictionary<RadioButton, GazeControlType>();

        public GazeControlType Widget
        {
            get { return iWidgets[gpbWidget.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked)]; }
            set { iWidgets.FirstOrDefault(n => n.Value == value).Key.Checked = true; }
        }

        public Detectors.Type PursueDetector
        {
            get { return iPursueDetectors[gpbPursueDetector.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked)]; }
            set { iPursueDetectors.FirstOrDefault(n => n.Value == value).Key.Checked = true; }
        }

        public Options()
        {
            InitializeComponent();

            iPursueDetectors.Add(rdbPursueDetector_OffsetXY, Detectors.Type.OffsetXY);
            iPursueDetectors.Add(rdbPursueDetector_OffsetDist, Detectors.Type.OffsetDist);

            iWidgets.Add(rdbWidgetKnob, GazeControlType.Knob);
            iWidgets.Add(rdbWidgetScrollbar, GazeControlType.Scrollbar);
            iWidgets.Add(rdbWidgetStatic, GazeControlType.Static);
        }
    }
}
