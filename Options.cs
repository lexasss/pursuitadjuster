using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmoothPursuit
{
    public partial class Options : Form
    {
        private Dictionary<RadioButton, Detectors.Type> iPursueDetectors = new Dictionary<RadioButton, Detectors.Type>();
        private Dictionary<RadioButton, int> iWidgets = new Dictionary<RadioButton, int>();

        public int Widget
        {
            get { return iWidgets[gpbWidget.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked)]; }
            set { if (value >= 0 && value < iWidgets.Count) iWidgets.FirstOrDefault(n => n.Value == value).Key.Checked = true; }
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

            iWidgets.Add(rdbWidgetKnob, 0);
            iWidgets.Add(rdbWidgetScrollbar, 1);
            iWidgets.Add(rdbWidgetStatic, 2);
        }
    }
}
