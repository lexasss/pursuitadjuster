using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace GazeInSimSpace.Player
{
    public class NAudioPlayer : Player
    {
        #region Declarations

        private class FileData
        {
            private NAudio.Wave.AudioFileReader iReader;
            private string iFileName;

            public NAudio.Wave.AudioFileReader Reader { get { return iReader; } }
            public string FileName { get { return iFileName; } }

            public FileData(string aFileName)
            {
                iReader = new NAudio.Wave.AudioFileReader(aFileName);
                iFileName = aFileName;
            }
        }

        #endregion

        #region Internal members

        private static List<Guid> iDirectSoundDevices;
        private static NAudio.CoreAudioApi.MMDeviceCollection iWasapiDevices;

        private static string[] ListDevices(PlayerType aType)
        {
            List<string> result = new List<string>();
            Regex re;

            switch (aType)
            {
                case PlayerType.ASIO:
                    foreach (string name in NAudio.Wave.AsioOut.GetDriverNames())
                    {
                        result.Add(name);
                    }
                    break;

                case PlayerType.DirectSound:
                    iDirectSoundDevices = new List<Guid>();
                    re = new Regex(@"(\()(?<name>(.)+)(\))");
                    foreach (NAudio.Wave.DirectSoundDeviceInfo dsdi in NAudio.Wave.DirectSoundOut.Devices)
                    {
                        Match match = re.Match(dsdi.Description);
                        Group group = match.Groups["name"];
                        result.Add(group.Length > 0 ? group.ToString() : dsdi.Description);
                        iDirectSoundDevices.Add(dsdi.Guid);
                    }
                    break;

                case PlayerType.Wasapi:
                    iWasapiDevices = new NAudio.CoreAudioApi.MMDeviceEnumerator()
                        .EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.DeviceState.Active);
                    for (int i = 0; i < iWasapiDevices.Count; i++)
                    {
                        result.Add(iWasapiDevices[i].DeviceFriendlyName);
                    }
                    break;

                case PlayerType.WaveOut:
                    for (int i = 0; i < NAudio.Wave.WaveOut.DeviceCount; i++)
                    {
                        NAudio.Wave.WaveOutCapabilities woc = NAudio.Wave.WaveOut.GetCapabilities(i);
                        re = new Regex(@"(\()(?<name>(.)+(?<!\)))(\))?");
                        Match match = re.Match(woc.ProductName);
                        Group group = match.Groups["name"];
                        result.Add(group.Length > 0 ? group.ToString() : woc.ProductName);
                        //result.Add(woc.ProductName);
                    }
                    break;
            }

            return result.ToArray();
        }

        private PlayerType iType = PlayerType.WaveOut;
        private List<FileData> iFiles = new List<FileData>();
        private List<NAudio.Wave.IWavePlayer> iPlayers = new List<NAudio.Wave.IWavePlayer>();

        private PlayerType Type { get { return iType; } }

        #endregion

        #region Public methods

        public NAudioPlayer(int aLatency = 100)
        {
            string[] devices = NAudioPlayer.ListDevices(iType);
            //iDeviceName = devices[aDeviceIndex].ToString();

            for (int i = 0; i < devices.Length; i++)
            {
                NAudio.Wave.IWavePlayer player;
                switch (iType)
                {
                    case PlayerType.ASIO:
                        player = new NAudio.Wave.AsioOut(i);
                        ((NAudio.Wave.AsioOut)player).ChannelOffset = 0;
                        break;

                    case PlayerType.DirectSound:
                        player = new NAudio.Wave.DirectSoundOut(NAudioPlayer.iDirectSoundDevices[i], aLatency);
                        break;

                    case PlayerType.Wasapi:
                        player = new NAudio.Wave.WasapiOut(NAudioPlayer.iWasapiDevices[i],
                            NAudio.CoreAudioApi.AudioClientShareMode.Shared, false, aLatency);
                        break;

                    case PlayerType.WaveOut:
                        player = new NAudio.Wave.WaveOut();
                        ((NAudio.Wave.WaveOut)player).DeviceNumber = i;
                        ((NAudio.Wave.WaveOut)player).DesiredLatency = aLatency;
                        break;

                    default:
                        throw new Exception("Unknown player type");
                }

                player.PlaybackStopped += new EventHandler<NAudio.Wave.StoppedEventArgs>(OnPlayerStopped);
                iPlayers.Add(player);
            }
        }

        public void Dispose()
        {
            if (iPlayers != null)
            {
                foreach (NAudio.Wave.IWavePlayer player in iPlayers)
                {
                    player.Dispose();
                }
                iPlayers = null;
            }
            iFiles = null;
            //iDeviceName = "";
        }

        public override void getVolume(int aDeviceIndex, out uint aLeft, out uint aRight)
        {
            aLeft = (uint)(16 * iPlayers[aDeviceIndex].Volume);
            aRight = (uint)(16 * iPlayers[aDeviceIndex].Volume);
        }
        public override void setVolume(int aDeviceIndex, uint aLeft, uint aRight)
        {
            iPlayers[aDeviceIndex].Volume = (float)aLeft / 16;
        }

        #endregion

        #region Internal methods

        protected override void Play(Sound aSound)
        {
            NAudio.Wave.IWavePlayer player = null;
            int playerIndex = 0;
            foreach (Device device in iDevices)
            {
                if (device.ID == aSound.DeviceID)
                {
                    player = iPlayers[playerIndex];
                    break;
                }
                playerIndex++;
            }

            if (player != null)
            {
                foreach (FileData file in iFiles)
                {
                    if (file.FileName == aSound.FileName)
                    {
                        if (player.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
                        {
                            try
                            {
                                //if (iType == PlayerType.WaveOut)
                                // {
                                player.Init(file.Reader);
                                //}

                                file.Reader.Position = 0;
                                player.Play();
                            }
                            catch (NAudio.MmException ex) { Console.Out.WriteLine("NAudioPlayer EX MM: {0}", ex.Result); }
                            catch (Exception ex) { Console.Out.WriteLine("NAudioPlayer EX: {0}", ex.Message); }
                        }
                        break;
                    }
                }
            }
        }

        protected override void AddFiles(string[] aFiles)
        {
            uint index = 0;
            foreach (string filename in aFiles)
            {
                iFiles.Add(new NAudioPlayer.FileData(filename));
                iSounds.Add(new Sound(index, filename, uint.MaxValue));
                index++;
            }
        }

        protected override void AddSound(Sound aSound)
        {
            iFiles.Add(new FileData(aSound.FileName));
            /*
            if (iType != PlayerType.WaveOut)
            {
                iPlayer.Init(iFile.Reader);
            }*/
        }

        protected override void EnumDevices()
        {
            if (iDevices.Count == 0)
            {
                string[] device = ListDevices(iType);
                for (int i = 0; i < device.Length; i++)
                {
                    iDevices.Add(new Device(device[i], (uint)i));
                }
            }
        }

        private void OnPlayerStopped(object aSender, NAudio.Wave.StoppedEventArgs aArgs)
        {
            NAudio.Wave.IWavePlayer player = (NAudio.Wave.IWavePlayer)aSender;
            if (player != null)
            {
                player.Stop();
            }
            else
            {
                Debug.WriteLine("The player is empty already");
            }
        }

        #endregion
    }
}
