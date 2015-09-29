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
        private IGazeControl iGazeControl;  // set only using SetGazeControl

        private Rotation.Knob iKnob;
        private Scrolling.Bar iScrollbar;

        private TheCodeKing.ActiveButtons.Controls.IActiveMenu iMenu;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnOptions;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnCalibrate;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnToggleTracking;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnToggleStimuli;

        public MainForm()
        {
            InitializeComponent();

            iETUDriver = new CoETUDriver();
            iETUDriver.OptionsFile = Application.StartupPath + "\\etudriver.ini";
            iETUDriver.OnRecordingStart += ETUDriver_OnRecordingStart;
            iETUDriver.OnRecordingStop += ETUDriver_OnRecordingStop;
            iETUDriver.OnCalibrated += ETUDriver_OnCalibrated;
            iETUDriver.OnDataEvent += ETUDriver_OnDataEvent;

            CreateMenu();

            iKnob = new Rotation.Knob();
            //iKnob.OnValueChanged += GazeControl_OnValueChnaged;
            iKnob.OnSoundPlayRequest += GazeControl_OnSoundPlayRequest;
            iKnob.OnRedraw += GazeControl_OnRedraw;

            iScrollbar = new Scrolling.Bar();
            iScrollbar.OnSoundPlayRequest += GazeControl_OnSoundPlayRequest;
            iScrollbar.OnRedraw += GazeControl_OnRedraw;

            iParser = new GazeParser();

            SetGazeControl(iKnob);

            iPlayer = new Utils.WavPlayer();
            iPlayer.init();
            
            EnabledMenuButtons();
        }

        private void SetGazeControl(IGazeControl aGazeControl)
        {
            iGazeControl = aGazeControl;
            iParser.PursueDetector = iGazeControl.PursueDetector;
            pcbControl.Image = iGazeControl.Image;

            mbnToggleStimuli.Text = iGazeControl is Rotation.Knob ? "Switch to SCROLLBAR"  : "Switch to KNOB";
        }

        private void EnabledMenuButtons()
        {
            mbnOptions.Enabled = iETUDriver.DeviceCount > 0 && iETUDriver.Active == 0;
            mbnCalibrate.Enabled = iETUDriver.Ready != 0 && iETUDriver.Active == 0;
            mbnToggleTracking.Enabled = iETUDriver.Ready != 0 && iETUDriver.Calibrated != 0;
            mbnToggleStimuli.Enabled = iETUDriver.Active == 0;
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

            mbnToggleTracking = new TheCodeKing.ActiveButtons.Controls.ActiveButton();
            mbnToggleTracking.Text = "Start";
            mbnToggleTracking.Click += (s, e) =>
            {
                if (iETUDriver.Active == 0)
                {
                    //pcbControl.Show();
                    iETUDriver.startTracking();
                }
                else
                {
                    iETUDriver.stopTracking();
                    //pcbControl.Hide();
                }
            };

            mbnToggleStimuli = new TheCodeKing.ActiveButtons.Controls.ActiveButton();
            mbnToggleStimuli.Click += (s, e) =>
            {
                SetGazeControl(iGazeControl is Rotation.Knob ? (IGazeControl)iScrollbar : (IGazeControl)iKnob);
            };

            iMenu.Items.Add(mbnToggleStimuli);
            iMenu.Items.Add(mbnToggleTracking);
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
            iGazeControl.start();

            SiETUDFloatPoint offset = new SiETUDFloatPoint();
            Rectangle r = this.ClientRectangle;
            r = this.RectangleToScreen(r);
            offset.X = r.Left;
            offset.Y = r.Top;
            iETUDriver.set_Offset(ref offset);
            
            EnabledMenuButtons();
            mbnToggleTracking.Text = "Stop";
        }

        private void ETUDriver_OnRecordingStop()
        {
            EnabledMenuButtons();
            mbnToggleTracking.Text = "Start";

            iParser.stop();
            iGazeControl.stop();
        }

        private void ETUDriver_OnDataEvent(EiETUDGazeEvent aEventID, ref int aData, ref int aResult)
        {
            if (aEventID == EiETUDGazeEvent.geSample)
            {
                SiETUDSample smp = iETUDriver.LastSample;
                Point pt = pcbControl.PointToClient(new Point((int)smp.X[0], (int)smp.Y[0]));
                iParser.feed(smp.Time, pt);
            }
        }

        public void GazeControl_OnRedraw(object sender, EventArgs e)
        {
            pcbControl.Invoke(new Action(pcbControl.Refresh));
        }

        private void GazeControl_OnSoundPlayRequest(object sender, Rotation.Knob.SoundPlayRequestArgs e)
        {
            iPlayer.setVolume(0, e.Volume, e.Volume);
            iPlayer.play("sounds\\click.wav", "", 0);
        }

        private void pcbControl_Paint(object sender, PaintEventArgs e)
        {
            iGazeControl.draw(e.Graphics);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (iETUDriver.Active != 0)
            {
                e.Cancel = true;
            }
        }
    }
}
