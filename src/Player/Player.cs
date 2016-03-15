using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace GazeInSimSpace.Player
{
    [Serializable]
    public abstract class Player
    {
        #region Internal members

        private Sound.PlayRequestEvent SoundPlayRequestEventHandler;

        protected List<Sound> iSounds = new List<Sound>();
        protected string iSoundsFolder = "sounds";

        protected static List<Device> iDevices = new List<Device>();

        #endregion

        #region Public members

        public static Device[] Devices
        {
            get
            {
                return iDevices != null ? iDevices.ToArray() : new Device[] { };
            }
        }

        [XmlIgnore]
        public string[] Files
        {
            get
            {
                List<string> result = new List<string>(iSounds.Count);
                foreach (Sound sound in iSounds)
                {
                    result.Add(sound.FileName);
                }
                return result.ToArray(); 
            }
        }

        public string SoundsFolder
        {
            get { return iSoundsFolder; }
            set { ReadFolder(value); }
        }

        #endregion

        #region Public methods

        public Player()
        {
            SoundPlayRequestEventHandler = new Sound.PlayRequestEvent(Sound_PlayRequest);
            EnumDevices();
        }

        public abstract void getVolume(int aDeviceIndex, out uint aLeft, out uint aRight);
        public abstract void setVolume(int aDeviceIndex, uint aLeft, uint aRight);

        public void init()
        {
            if (iSounds.Count == 0)
            {
                ReadFolder(iSoundsFolder);
            }
            else
            {
                for (int i = 0; i < iSounds.Count; i++)
                {
                    Sound sound = iSounds[i];
                    sound.Index = (uint)i;
                    sound.OnPlayRequest += SoundPlayRequestEventHandler;

                    AddSound(sound);
                }
            }
        }

        public void play(string aFileName, string aDeviceName, uint aDelay)
        {
            foreach (Sound sound in iSounds)
            {
                if (sound.FileName == aFileName)
                {
                    sound.DeviceName = aDeviceName;
                    sound.Delay = aDelay;
                    if (aDelay > 0)
                    {
                        sound.playPostpone();
                    }
                    else
                    {
                        Play(sound);
                    }
                    break;
                }
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Plays the sound file
        /// </summary>
        /// <param name="aSound">The sound to play</param>
        protected abstract void Play(Sound aSound);

        /// <summary>
        /// Populates the iSounds list
        /// </summary>
        /// <param name="aFiles">The list of files to add</param>
        protected abstract void AddFiles(string[] aFiles);

        /// <summary>
        /// Makes extra actions when the sound is already added to iSounds list, is necessary
        /// </summary>
        /// <param name="aSound">The sound to process</param>
        protected abstract void AddSound(Sound aSound);

        /// <summary>
        /// Populates the iDevices list
        /// </summary>
        protected abstract void EnumDevices();

        protected void Sound_PlayRequest(object aSender, EventArgs aArgs)
        {
            Sound sound = (Sound)aSender;
            Play(sound);
        }

        protected void ReadFolder(string aFolder)
        {
            iSoundsFolder = aFolder;
            iSounds.Clear();

            string[] files = Directory.GetFiles(iSoundsFolder);
            AddFiles(files);

            foreach (Sound sound in iSounds)
            {
                sound.OnPlayRequest += SoundPlayRequestEventHandler;
            }
        }

        #endregion
    }
}
