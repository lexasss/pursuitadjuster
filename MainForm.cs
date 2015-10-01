using System;
using System.Drawing;
using System.Windows.Forms;
using ETUDriver;
using Utils = GazeInSimSpace.Player;

namespace SmoothPursuit
{
    public partial class MainForm : Form
    {
        #region Consts

        private const bool COLOR_VALUE_VISIBLE = true;
        private const bool SOUND_ENABLED = true;
        private const bool CONTROL_ARE_INVISIBLE_WHEN_NOT_TRACKING = false;

        #endregion

        #region Internal members

        private CoETUDriver iETUDriver;
        private GazeParser iParser;
        private Utils.Player iPlayer;
        private IGazeControl iGazeControl;  // set only using SetGazeControl
        private Experiment iExperiment;

        private Rotation.Knob iKnob;
        private Scrolling.Bar iScrollbar;

        private TheCodeKing.ActiveButtons.Controls.IActiveMenu iMenu;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnOptions;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnCalibrate;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnToggleTracking;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnToggleStimuli;

        #endregion

        #region Public methods

        public MainForm()
        {
            InitializeComponent();

            iETUDriver = new CoETUDriver();
            iETUDriver.OptionsFile = Application.StartupPath + "\\etudriver.ini";
            iETUDriver.OnRecordingStart += ETUDriver_OnRecordingStart;
            iETUDriver.OnRecordingStop += ETUDriver_OnRecordingStop;
            iETUDriver.OnCalibrated += ETUDriver_OnCalibrated;
            iETUDriver.OnDataEvent += ETUDriver_OnDataEvent;

            iExperiment = new Experiment(3);
            iExperiment.OnNextTrial += Experiment_OnNextTrial;
            iExperiment.OnFinished += Experiment_OnFinished;

            CreateMenu();

            iKnob = new Rotation.Knob();
            iKnob.OnValueChanged += GazeControl_OnValueChnaged;
            iKnob.OnSoundPlayRequest += GazeControl_OnSoundPlayRequest;
            iKnob.OnRedraw += GazeControl_OnRedraw;

            iScrollbar = new Scrolling.Bar();
            iScrollbar.OnValueChanged += GazeControl_OnValueChnaged;
            iScrollbar.OnSoundPlayRequest += GazeControl_OnSoundPlayRequest;
            iScrollbar.OnRedraw += GazeControl_OnRedraw;

            iParser = new GazeParser();

            SetGazeControl(iKnob);

            iPlayer = new Utils.WavPlayer();
            iPlayer.init();
            
            EnabledMenuButtons();
        }

        #endregion

        #region Internal methods

        private void SetGazeControl(IGazeControl aGazeControl)
        {
            iGazeControl = aGazeControl;
            iParser.PursueDetector = iGazeControl.PursueDetector;
            
            pcbControl.Image = iGazeControl.Image;
            pcbControl.Visible = !CONTROL_ARE_INVISIBLE_WHEN_NOT_TRACKING;

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
                    pcbControl.Visible = true;
                    iExperiment.start();
                    iETUDriver.startTracking();
                }
                else
                {
                    iETUDriver.stopTracking();
                    iExperiment.stop();
                    pcbControl.Visible = !CONTROL_ARE_INVISIBLE_WHEN_NOT_TRACKING;
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

        private void ConfigLabel(Label aLabel, Color aColor, int aValue)
        {
            aLabel.BackColor = aColor;
            if (COLOR_VALUE_VISIBLE)
            {
                aLabel.Text = aValue.ToString();
                aLabel.ForeColor = aValue > 140 ? Color.Black : Color.White;
            }
        }

        #endregion

        #region Event handlers

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

        private void Experiment_OnFinished(object sender, EventArgs e)
        {
            iETUDriver.stopTracking();
            if (sfdSaveData.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                iExperiment.save(sfdSaveData.FileName);
            }
        }

        private void Experiment_OnNextTrial(object aSender, Experiment.NextTrialArgs aArgs)
        {
            ConfigLabel(lblTargetColor, aArgs.TargetColor, aArgs.TargetValue);
            ConfigLabel(lblColor, aArgs.StartColor, aArgs.StartValue);
            iGazeControl.reset();
        }

        private void GazeControl_OnRedraw(object sender, EventArgs e)
        {
            pcbControl.Invoke(new Action(pcbControl.Refresh));
        }

        private void GazeControl_OnValueChnaged(object aSender, IGazeControl.ValueChangedArgs aArgs)
        {
            this.Invoke(new Action(() => { 
                Experiment.Trial trial = iExperiment.CurrentTrial;
                int value = (int)aArgs.Current;
                ConfigLabel(lblColor,  trial.createColor(value), value);
            }));
        }

        private void GazeControl_OnSoundPlayRequest(object sender, Rotation.Knob.SoundPlayRequestArgs e)
        {
            if (SOUND_ENABLED)
            {
                iPlayer.setVolume(0, e.Volume, e.Volume);
                iPlayer.play("sounds\\click.wav", "", 0);
            }
        }

        #endregion

        #region GUI event handlers

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

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                iExperiment.next(lblColor.BackColor);
            }
        }

        #endregion
    }
}
