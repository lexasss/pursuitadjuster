using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ETUDriver;
using Utils = GazeInSimSpace.Player;

namespace SmoothVolume
{
    public partial class MainForm : Form
    {
        private CoETUDriver iETUDriver;
        private GazeParser iParser;
        private Utils.Player iPlayer;
        private Knob iKnob;

        private TheCodeKing.ActiveButtons.Controls.IActiveMenu iMenu;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnOptions;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnCalibrate;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnToggle;

        public MainForm()
        {
            InitializeComponent();

            iETUDriver = new CoETUDriver();
            iETUDriver.OptionsFile = Application.StartupPath + "\\etudriver.ini";
            iETUDriver.OnRecordingStart += ETUDriver_OnRecordingStart;
            iETUDriver.OnRecordingStop += ETUDriver_OnRecordingStop;
            iETUDriver.OnCalibrated += ETUDriver_OnCalibrated;
            iETUDriver.OnDataEvent += ETUDriver_OnDataEvent;

            iKnob = new Knob(pcbKnob.Size);
            iKnob.OnValueChanged += Knob_OnValueChnaged;
            iKnob.OnRedraw += Knob_OnRedraw;

            RotationDetector rotationDetector = new RotationDetector(pcbKnob.Width / 2, pcbKnob.Height / 2, iKnob.TargetRadius, iKnob.TargetSpeed);
            rotationDetector.OnAngleChanged += (s, e) => { this.Invoke(new Action(() => { iKnob.Value += e.AngleChange; })); };

            iParser = new GazeParser();
            iParser.Control = rotationDetector;

            iPlayer = new Utils.WavPlayer();
            iPlayer.init();
            
            CreateMenu();
            EnabledMenuButtons();
        }

        private void EnabledMenuButtons()
        {
            mbnOptions.Enabled = iETUDriver.DeviceCount > 0 && iETUDriver.Active == 0;
            mbnCalibrate.Enabled = iETUDriver.Ready != 0 && iETUDriver.Active == 0;
            mbnToggle.Enabled = iETUDriver.Ready != 0 && iETUDriver.Calibrated != 0;
        }

        private void CreateMenu()
        {
            iMenu = TheCodeKing.ActiveButtons.Controls.ActiveMenu.GetInstance(this);
            iMenu.Alighment = HorizontalAlignment.Center;

            mbnOptions = new TheCodeKing.ActiveButtons.Controls.ActiveButton();
            mbnOptions.Text = "Options";
            mbnOptions.Click += (s, e) =>
            {
                iETUDriver.showRecordingOptions();
                EnabledMenuButtons();
            };

            mbnCalibrate = new TheCodeKing.ActiveButtons.Controls.ActiveButton();
            mbnCalibrate.Text = "Calibrate";
            mbnCalibrate.Click += (s, e) =>
            {
                iETUDriver.calibrate();
            };

            mbnToggle = new TheCodeKing.ActiveButtons.Controls.ActiveButton();
            mbnToggle.Text = "Start";
            mbnToggle.Click += (s, e) =>
            {
                if (iETUDriver.Active == 0)
                    iETUDriver.startTracking();
                else
                    iETUDriver.stopTracking();
            };

            iMenu.Items.Add(mbnToggle);
            iMenu.Items.Add(mbnCalibrate);
            iMenu.Items.Add(mbnOptions);
        }

        private void ETUDriver_OnCalibrated()
        {
            EnabledMenuButtons();
        }

        private void ETUDriver_OnRecordingStart()
        {
            iParser.start();
            iKnob.start();

            SiETUDFloatPoint offset = new SiETUDFloatPoint();
            Rectangle r = this.ClientRectangle;
            r = this.RectangleToScreen(r);
            offset.X = r.Left;
            offset.Y = r.Top;
            iETUDriver.set_Offset(ref offset);
            
            EnabledMenuButtons();
            mbnToggle.Text = "Stop";
        }

        private void ETUDriver_OnRecordingStop()
        {
            EnabledMenuButtons();
            mbnToggle.Text = "Start";

            iParser.stop();
            iKnob.stop();
        }

        private void ETUDriver_OnDataEvent(EiETUDGazeEvent aEventID, ref int aData, ref int aResult)
        {
            if (aEventID == EiETUDGazeEvent.geSample)
            {
                SiETUDSample smp = iETUDriver.LastSample;
                Point pt = pcbKnob.PointToClient(new Point((int)smp.X[0], (int)smp.Y[0]));
                iParser.feed(smp.Time, pt);
            }
        }

        public void Knob_OnRedraw(object sender, EventArgs e)
        {
            pcbKnob.Invoke(new Action(pcbKnob.Refresh));
        }

        private void Knob_OnValueChnaged(object sender, Knob.ValueChangedArgs e)
        {
            int prev = (int)Math.Round(e.Prev);
            int current = (int)Math.Round(e.Current);

            uint volume = 1 + (uint)Math.Round(15 * e.Current / iKnob.MaxValue);
            iPlayer.setVolume(0, volume, volume);

            if ((int)(prev / 3) != (int)(current / 3))
            {
                iPlayer.play("sounds\\click.wav", "", 0);
            }
        }

        private void pcbKnob_Paint(object sender, PaintEventArgs e)
        {
            iKnob.draw(e.Graphics);
        }
    }
}
