using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ETUDriver;
using Utils = GazeInSimSpace.Player;

namespace SmoothPursuit
{
    public partial class MainForm : Form
    {
        #region Consts

        private const bool COLOR_VALUE_VISIBLE = false;
        private const bool SOUND_ENABLED = true;
        private const bool CONTROL_INVISIBLE_WHEN_NOT_TRACKING = false;
        private const int TRIAL_COUNT = 20;

        #endregion

        #region Internal members

        private CoETUDriver iETUDriver;
        private GazeParser iParser;
        private Utils.Player iPlayer;
        private IGazeControl iGazeControl;              // use SetGazeControl to set
        private Detectors.Type iPursueDetectorType;     // use SetPursueDetectorType to set
        private Experiment iExperiment;

        private Rotation.Knob iKnob;
        private Scrolling.Bar iScrollbar;
        private Static.Control iStaticControl;

        private Dictionary<GazeControlType, IGazeControl> iGazeControls = new Dictionary<GazeControlType, IGazeControl>();
        private GazeControlType iGazeControlType;

        private TheCodeKing.ActiveButtons.Controls.IActiveMenu iMenu;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnETUDOptions;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnCalibrate;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnToggleTracking;
        private TheCodeKing.ActiveButtons.Controls.ActiveButton mbnOptions;

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

            iExperiment = new Experiment(TRIAL_COUNT);
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

            iStaticControl = new Static.Control();
            iStaticControl.OnValueChanged += GazeControl_OnValueChnaged;
            iStaticControl.OnSoundPlayRequest += GazeControl_OnSoundPlayRequest;
            iStaticControl.OnRedraw += GazeControl_OnRedraw;

            iGazeControls.Add(GazeControlType.Knob, iKnob);
            iGazeControls.Add(GazeControlType.Scrollbar, iScrollbar);
            iGazeControls.Add(GazeControlType.Static, iStaticControl);

            iParser = new GazeParser();

            SetGazeControl();
            SetPursueDetectorType();

            iPlayer = new Utils.WavPlayer();
            iPlayer.init();
            
            EnabledMenuButtons();
        }

        #endregion

        #region Internal methods

        private void SetGazeControl(GazeControlType aType = GazeControlType.Knob)
        {
            iGazeControlType = aType;

            iGazeControl = iGazeControls[iGazeControlType];
            iParser.PursueDetector = iGazeControl.PursueDetector;
            iParser.OffsetEnabled = iGazeControl != iStaticControl;
            
            pcbControl.Image = iGazeControl.Image;
            pcbControl.Visible = !CONTROL_INVISIBLE_WHEN_NOT_TRACKING;
        }

        private void SetPursueDetectorType(Detectors.Type aType = Detectors.Type.OffsetXY)
        {
            iPursueDetectorType = aType;
            foreach (IGazeControl gazeControl in iGazeControls.Values)
            {
                gazeControl.setPursueDetectorType(iPursueDetectorType);
            }
        }

        private void EnabledMenuButtons()
        {
            mbnETUDOptions.Enabled = iETUDriver.DeviceCount > 0 && iETUDriver.Active == 0;
            mbnCalibrate.Enabled = iETUDriver.Ready != 0 && iETUDriver.Active == 0;
            mbnToggleTracking.Enabled = iETUDriver.Ready != 0 && iETUDriver.Calibrated != 0;
            mbnOptions.Enabled = iETUDriver.Active == 0;
        }

        private void CreateMenu()
        {
            iMenu = TheCodeKing.ActiveButtons.Controls.ActiveMenu.GetInstance(this);
            iMenu.Alighment = HorizontalAlignment.Center;

            mbnETUDOptions = new TheCodeKing.ActiveButtons.Controls.ActiveButton();
            mbnETUDOptions.Text = "ETUDriver";
            mbnETUDOptions.Click += (s, e) =>
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
                    pcbControl.Visible = !CONTROL_INVISIBLE_WHEN_NOT_TRACKING;
                }
            };

            mbnOptions = new TheCodeKing.ActiveButtons.Controls.ActiveButton();
            mbnOptions.Text = "Options";
            mbnOptions.Click += (s, e) =>
            {
                Options options = new Options();
                options.Widget = iGazeControlType;
                options.PursueDetector = iPursueDetectorType;
                if (options.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SetGazeControl(options.Widget);
                    SetPursueDetectorType(options.PursueDetector);
                }
            };

            iMenu.Items.Add(mbnOptions);
            iMenu.Items.Add(mbnToggleTracking);
            iMenu.Items.Add(mbnCalibrate);
            iMenu.Items.Add(mbnETUDOptions);
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
                string header = new StringBuilder().
                    Append(iParser).
                    AppendLine().
                    Append(iGazeControl).
                    ToString();
                iExperiment.save(sfdSaveData.FileName, header);
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
                if (trial != null)
                {
                    int value = (int)aArgs.Current;
                    ConfigLabel(lblColor, trial.createColor(value), value);
                }
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
                iGazeControl.PursueDetector.reset();
            }
        }

        #endregion
    }
}
