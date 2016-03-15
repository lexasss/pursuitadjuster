using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GazeInSimSpace.Player
{
    [Serializable]
    public class Sound
    {
        #region Events

        public delegate void PlayRequestEvent(object aSender, EventArgs aArgs);

        public event PlayRequestEvent OnPlayRequest;

        #endregion

        #region Members

        private uint iIndex;
        private string iFileName;
        private uint iDeviceID;
        private uint iDelay = 0;

        private System.Timers.Timer iTimer;

        [XmlAttribute]
        public string FileName
        {
            get { return iFileName; }
            set { iFileName = value; }
        }

        [XmlAttribute]
        public uint DeviceID
        {
            get { return iDeviceID; }
            set { iDeviceID = value; }
        }

        [XmlIgnore]
        public string DeviceName
        {
            get
            {
                string result = "";
                foreach (Device wavDevice in WavPlayer.Devices)
                {
                    if (wavDevice.ID == iDeviceID)
                    {
                        result = wavDevice.Name;
                        break;
                    }
                }
                return result;
            }
            set
            {
                foreach (Device wavDevice in WavPlayer.Devices)
                {
                    if (wavDevice.Name == value)
                    {
                        iDeviceID = wavDevice.ID;
                        break;
                    }
                }
            }
        }

        [XmlAttribute]
        public uint Index
        {
            get { return iIndex; }
            set { iIndex = value; }
        }

        [XmlAttribute]
        public uint Delay
        {
            get { return iDelay; }
            set { iDelay = value; }
        }

        #endregion

        #region Public methods

        public Sound() 
        {
            Init();
        }

        public Sound(uint aIndex, String aPath, uint aDeviceID)
        {
            iIndex = aIndex;
            iFileName = aPath;
            iDeviceID = aDeviceID;

            Init();
        }

        public override string ToString()
        {
            return System.IO.Path.GetFileNameWithoutExtension(FileName);
        }

        public void playPostpone()
        {
            if (iDelay > 0)
            {
                iTimer.Interval = iDelay;
                iTimer.Start();
            }
            else if (OnPlayRequest != null)
            {
                OnPlayRequest(this, new EventArgs());
            }
        }

        #endregion

        #region Internal methods

        private void Init()
        {
            iTimer = new System.Timers.Timer();
            iTimer.Enabled = false;
            iTimer.AutoReset = false;
            iTimer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
        }

        private void Timer_Elapsed(object aSender, System.Timers.ElapsedEventArgs aArgs)
        {
            if (OnPlayRequest != null)
            {
                OnPlayRequest(this, new EventArgs());
            }
        }

        #endregion
    }
}
