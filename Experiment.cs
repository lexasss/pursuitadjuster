using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;

namespace SmoothPursuit
{
    public class Experiment
    {
        #region Declarations

        public class Trial
        {
            private static Random iRand = new Random();

            private bool IS_GREYCOLOR = true;
            private int COLOR_EDGE_GAP = 30;
            private int COLOR_START_GAP = 30;
            private int START_VALUE = 128;

            private long iStartTimestamp = 0;
            private HiResTimestamp iHRTimestamp = new HiResTimestamp();
            
            public int ColorComponentIndex { get; private set; }
            public int TargetValue { get; private set; }
            public int StartValue { get; private set; }
            public Color Target { get; private set; }
            public Color Start { get; private set; }
            public Color Result { get; private set; }
            public long  Duration { get; private set; }

            public Trial()
            {
                ColorComponentIndex = iRand.Next(3);
                StartValue = START_VALUE;

                do
                {
                    TargetValue = COLOR_EDGE_GAP + iRand.Next(256 - 2 * COLOR_EDGE_GAP);
                } while (START_VALUE - COLOR_START_GAP < TargetValue && TargetValue < START_VALUE + COLOR_START_GAP);

                Target = CreateColor(ColorComponentIndex, TargetValue);
                Start = CreateColor(ColorComponentIndex, StartValue);
            }

            public Color createColor(int aMainComponent)
            {
                return CreateColor(ColorComponentIndex, aMainComponent);
            }

            public void start()
            {
                iStartTimestamp = iHRTimestamp.Milliseconds;
            }

            public void stop(Color aColor)
            {
                Result = aColor;
                Duration = iHRTimestamp.Milliseconds - iStartTimestamp;
            }

            public override string ToString()
            {
                int[] targetComponents = new int[3] { Target.R, Target.G, Target.B };
                int[] resultComponents = new int[3] { Result.R, Result.G, Result.B };

                int diff = resultComponents[ColorComponentIndex] - targetComponents[ColorComponentIndex];
                if (diff == 0)  // probably, the difference should be estimated from non-main component
                {
                    int componentIndex = ColorComponentIndex + 1;
                    if (componentIndex > 2)
                        componentIndex = 0;
                    diff = resultComponents[componentIndex] - targetComponents[componentIndex];
                }

                return new StringBuilder().
                    AppendFormat("\t{0},{1},{2}", Target.R, Target.G, Target.B).
                    AppendFormat("\t{0},{1},{2}", Result.R, Result.G, Result.B).
                    AppendFormat("\t{0}", diff).
                    AppendFormat("\t{0}", Duration).
                    ToString();
            }

            private Color CreateColor(int aMainComponentIndex, int aMainComponent)
            {
                int mainComp = aMainComponent < 128 ? 2 * aMainComponent : 255;
                int restComp = aMainComponent < 128 ? 0 : 2 * (aMainComponent - 128);
                int[] components = new int[3];
                for (int compIndex = 0; compIndex < 3; compIndex++)
                {
                    if (IS_GREYCOLOR)
                    {
                        components[compIndex] = aMainComponent;
                    }
                    else
                    {
                        components[compIndex] = compIndex == aMainComponentIndex ? mainComp : restComp;
                    }
                }

                return Color.FromArgb(components[0], components[1], components[2]);
            }
        }

        #endregion

        #region Internal members

        private int iTrialCount;
        private int iTrialIndex;
        private List<Trial> iTrials = new List<Trial>();

        #endregion

        #region Events

        public class NextTrialArgs : EventArgs
        {
            public Color TargetColor { get; private set; }
            public Color StartColor { get; private set; }
            public int TargetValue { get; private set; }
            public int StartValue { get; private set; }

            public NextTrialArgs(Trial aTrial)
            {
                TargetColor = aTrial.Target;
                TargetValue = aTrial.TargetValue;
                
                StartColor = aTrial.Start;
                StartValue = aTrial.StartValue;
            }
        }
        public delegate void NextTrialHandler(object aSender, NextTrialArgs aArgs);
        public event NextTrialHandler OnNextTrial = delegate { };
        public event EventHandler OnFinished = delegate { };

        #endregion

        #region Properties

        public Trial CurrentTrial { get { return iTrialIndex < 0 ? null : iTrials[iTrialIndex]; } }

        #endregion

        #region Public methods

        public Experiment(int aTrialCount)
        {
            iTrialCount = aTrialCount;
            iTrialIndex = -1;
        }

        public void start()
        {
            CreateTrials();

            iTrialIndex = 0;
            StartNextTrial();
        }

        public void stop()
        {
            iTrialIndex = -1;
        }

        public void next(Color aSelectedColor)
        {
            if (iTrialIndex < 0)
                return;

            iTrials[iTrialIndex].stop(aSelectedColor);
            
            iTrialIndex++;
            if (iTrialIndex == iTrialCount)
            {
                stop();
                OnFinished(this, new EventArgs());
            }
            else
            {
                StartNextTrial();
            }
        }

        public void save(string aFileName)
        {
            using (TextWriter writer = new StreamWriter(aFileName))
            {
                for (int i = 0; i < iTrials.Count; i++)
                {
                    writer.WriteLine(iTrials[i]);
                    Console.WriteLine(iTrials[i]);
                }
            }
        }

        #endregion

        #region Internal methods

        private void CreateTrials()
        {
            iTrials.Clear();
            for (int i = 0; i < iTrialCount; i++)
            {
                iTrials.Add(new Trial());
            }
        }

        private void StartNextTrial()
        {
            Trial trial = iTrials[iTrialIndex];
            OnNextTrial(this, new NextTrialArgs(trial));
            trial.start();
        }

        #endregion
    }
}
