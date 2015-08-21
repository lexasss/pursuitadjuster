using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GazeInSimSpace.Player
{
    public class WavPlayer : Player
    {
        #region Imported methods

        private delegate void EnumDevicesCallback(String aname);

        private sealed class Wav
        {
            [DllImport("WavPlayer.dll")]
            public static extern void enumDevices(EnumDevicesCallback aCallback);

            [DllImport("WavPlayer.dll")]
            public static extern uint add(String aFileName, uint aDeviceID);

            [DllImport("WavPlayer.dll")]
            public static extern void play(uint aIndex);

            [DllImport("WavPlayer.dll")]
            public static extern void stop(uint aIndex);

            [DllImport("WavPlayer.dll")]
            public static extern uint canAdjustVolume(uint aDeviceID);

            [DllImport("WavPlayer.dll")]
            public static extern uint getVolume(uint aDeviceID);

            [DllImport("WavPlayer.dll")]
            public static extern void setVolume(uint aDeviceID, uint aValue);

            [DllImport("WavPlayer.dll")]
            public static extern uint getDevice(uint aIndex);

            [DllImport("WavPlayer.dll")]
            public static extern void setDevice(uint aIndex, uint aValue);

            [DllImport("WavPlayer.dll")]
            public static extern void clear();
        }

        private sealed class Midi
        {
            [DllImport("MidiPlayer.dll")]
            public static extern void enumDevices(EnumDevicesCallback aCallback);

            [DllImport("MidiPlayer.dll")]
            public static extern bool open(uint aDeviceID);

            [DllImport("MidiPlayer.dll")]
            public static extern void play(uint aChannel, uint aNote, uint aRelativeVolume, uint aDuration);

            [DllImport("MidiPlayer.dll")]
            public static extern void stop(uint aChannel = 0xFFFFFFFF);

            [DllImport("MidiPlayer.dll")]
            public static extern uint canAdjustVolume(uint aDeviceID);

            [DllImport("MidiPlayer.dll")]
            public static extern uint getVolume(uint aDeviceID);

            [DllImport("MidiPlayer.dll")]
            public static extern void setVolume(uint aDeviceID, uint aValue);

            [DllImport("MidiPlayer.dll")]
            public static extern uint getPitch(uint aChannel);

            [DllImport("MidiPlayer.dll")]
            public static extern void setPitch(uint aChannel, uint aValue);

            [DllImport("MidiPlayer.dll")]
            public static extern void checkForStop();

            public enum Duration
            {
                ThirtySecond = 1,
                Sixteenth = 2,
                Eighth = 4,
                Quarter = 8,
                Half = 16,
                Whole = 32
            }

            public enum PatchID
            {
                AcousticGrandPiano,
                BrightAcousticPiano,
                ElectricGrandPiano,
                HonkytonkPiano,
                RhodesPiano,
                ChorusPiano,
                Harpsichord,
                Clavinet,
                Celesta,
                Glockenspiel,
                MusicBox,
                Vibraphone,
                Marimba,
                Xylophone,
                TubularBells,
                Dulcimer,
                HammondOrgan,
                PercussOrgan,
                RockOrgan,
                ChurchOrgan,
                ReedOrgan,
                Accordion,
                Harmonica,
                TangoAccordion,
                AcousticGuitarNnylon,
                AcousticGuitarSsteel,
                ElectricGuitarJjazz,
                ElectricGuitarClean,
                ElectricGuitarMuted,
                OverdrivenGuitar,
	            DistortionGuitar,
                GuitaHarmonics,
                AcousticBass,
                ElectricBassFinger,
                ElectricBassPick,
                FretlessBass,
                SlapBass1,
                SlapBass2,
                SynthBass1,
                SynthBass2,
                Violin,
                Viola,
                Cello,
                ContraBass,
                TremoloStrings,
                PizzicatoStrings,
                OrchestralHarp,
                Timpani,
                StringEnsemble1,
                StringEnsemble2,
                SynthStrings1,
                SynthStrings2,
                ChoirAahs,
                VoiceOohs,
                SynthVoice,
	            OrchestraHit,
                Trumpet,
                Trombone,
                Tuba,
                MutedTrumpet,
                FrenchHorn,
                BrassSection,
                SynthBrass1,
                SynthBrass2,
                SopranoSax,
                AltoSax,
                TenorSax,
                BaritoneSax,
                Oboe,
                EnglishHorn,
                Bassoon,
                Clarinet,
                Piccolo,
                Flute,
                Recorder,
                PanFlute,
                BottleBlow,
                Shakuhachi,
                Whistle,
                Ocarina,
	            Lead1Square,
                Lead2SawTooth,
                Lead3CalliopeLead,
                Lead4ChiffLead,
                Lead5Charang,
                Lead6Voice,
                Lead7Fifths,
                Lead8BassLead,
                Pad1NewAge,
                Pad2Warm,
                Pad3PolySynth,
                Pad4Choir,
                Pad5Bowed,
                Pad6Metallic,
                Pad7Halo,
                Pad8Sweep,
                FX1Rain,
                FX2SoundTrack,
                FX3Crystal,
                FX4Atmosphere,
                FX5Bright,
                FX6Goblins,
                FX7Echoes,
                FX8Scifi,
                Sitar,
	            Banjo,
                Shamisen,
                Koto,
                Kalimba,
                Bagpipe,
                Fiddle,
                Shanai,
                TinkleBell,
                Agogo,
                SteelDrums,
                WoodBlock,
                TaikoDrum,
                MelodicTom,
                SynthDrum,
                ReverseCymbal,
                GuitarFretNoise,
                BreathNoise,
                Seashore,
                BirdTweet,
                TelephoneRinging,
                Helicopter,
                Applause,
                Gunshot
            }

            public class Patch
            {
                public PatchID ID;
                public uint NoteFrom;
                public uint NoteTo;

                public Patch(PatchID aID, uint aNoteFrom, uint aNoteTo)
                {
                    ID = aID;
                    NoteFrom = aNoteFrom;
                    NoteTo = aNoteTo;
                }
            }

            public static Patch[] Patches = new Patch[] {
                new Patch( PatchID.AcousticGrandPiano, 21, 108 ),
                new Patch( PatchID.BrightAcousticPiano, 21, 108 ),
                new Patch( PatchID.ElectricGrandPiano, 21, 108 ),
                new Patch( PatchID.HonkytonkPiano, 21, 108 ),
                new Patch( PatchID.RhodesPiano, 21, 108 ),
                new Patch( PatchID.ChorusPiano, 21, 108 ),
                new Patch( PatchID.Harpsichord, 29, 89 ),
                new Patch( PatchID.Clavinet, 29, 89 ),
                new Patch( PatchID.Celesta, 60, 108 ),
                new Patch( PatchID.Glockenspiel, 79, 108 ),
                new Patch( PatchID.MusicBox, 79, 108 ),
                new Patch( PatchID.Vibraphone, 53, 89 ),
                new Patch( PatchID.Marimba, 36, 96 ),
                new Patch( PatchID.Xylophone, 79, 108 ),
                new Patch( PatchID.TubularBells, 72, 91 ),
                new Patch( PatchID.Dulcimer, 40, 76 ),
                new Patch( PatchID.HammondOrgan, 36, 96 ),
                new Patch( PatchID.PercussOrgan, 36, 96 ),
                new Patch( PatchID.RockOrgan, 36, 96 ),
                new Patch( PatchID.ChurchOrgan, 36, 96 ),
                new Patch( PatchID.ReedOrgan, 36, 96 ),
                new Patch( PatchID.Accordion, 36, 96 ),
                new Patch( PatchID.Harmonica, 36, 96 ),
                new Patch( PatchID.TangoAccordion, 36, 96 ),
                new Patch( PatchID.AcousticGuitarNnylon, 40, 76 ),
                new Patch( PatchID.AcousticGuitarSsteel, 40, 76 ),
                new Patch( PatchID.ElectricGuitarJjazz, 40, 76 ),
                new Patch( PatchID.ElectricGuitarClean, 40, 76 ),
                new Patch( PatchID.ElectricGuitarMuted, 40, 76 ),
                new Patch( PatchID.OverdrivenGuitar, 40, 76 ),
                new Patch( PatchID.DistortionGuitar, 40, 76 ),
                new Patch( PatchID.GuitaHarmonics, 40, 76 ),
                new Patch( PatchID.AcousticBass, 24, 60 ),
                new Patch( PatchID.ElectricBassFinger, 24, 60 ),
                new Patch( PatchID.ElectricBassPick, 24, 60 ),
                new Patch( PatchID.FretlessBass, 24, 60 ),
                new Patch( PatchID.SlapBass1, 24, 60 ),
                new Patch( PatchID.SlapBass2, 24, 60 ),
                new Patch( PatchID.SynthBass1, 24, 60 ),
                new Patch( PatchID.SynthBass2, 24, 60 ),
                new Patch( PatchID.Violin, 55, 105 ),
                new Patch( PatchID.Viola, 48, 88 ),
                new Patch( PatchID.Cello, 36, 84 ),
                new Patch( PatchID.ContraBass, 24, 60 ),
                new Patch( PatchID.TremoloStrings, 36, 105 ),
                new Patch( PatchID.PizzicatoStrings, 36, 105 ),
                new Patch( PatchID.OrchestralHarp, 23, 104 ),
                new Patch( PatchID.Timpani, 36, 57 ),
                new Patch( PatchID.StringEnsemble1, 36, 105 ),
                new Patch( PatchID.StringEnsemble2, 36, 105 ),
                new Patch( PatchID.SynthStrings1, 36, 105 ),
                new Patch( PatchID.SynthStrings2, 36, 105 ),
                new Patch( PatchID.ChoirAahs, 41, 84 ),
                new Patch( PatchID.VoiceOohs, 41, 84 ),
                new Patch( PatchID.SynthVoice, 41, 84 ),
                new Patch( PatchID.OrchestraHit, 24, 108 ),
                new Patch( PatchID.Trumpet, 54, 86 ),
                new Patch( PatchID.Trombone, 40, 77 ),
                new Patch( PatchID.Tuba, 19, 72 ),
                new Patch( PatchID.MutedTrumpet, 54, 86 ),
                new Patch( PatchID.FrenchHorn, 38, 60 ),
                new Patch( PatchID.BrassSection, 38, 86 ),
                new Patch( PatchID.SynthBrass1, 38, 86 ),
                new Patch( PatchID.SynthBrass2, 38, 86 ),
                new Patch( PatchID.SopranoSax, 56, 89 ),
                new Patch( PatchID.AltoSax, 52, 88 ),
                new Patch( PatchID.TenorSax, 46, 77 ),
                new Patch( PatchID.BaritoneSax, 40, 74 ),
                new Patch( PatchID.Oboe, 58, 93 ),
                new Patch( PatchID.EnglishHorn, 58, 93 ),
                new Patch( PatchID.Bassoon, 34, 75 ),
                new Patch( PatchID.Clarinet, 50, 106 ),
                new Patch( PatchID.Piccolo, 74, 110 ),
                new Patch( PatchID.Flute, 60, 98 ),
                new Patch( PatchID.Recorder, 60, 98 ),
                new Patch( PatchID.PanFlute, 60, 98 ),
                new Patch( PatchID.BottleBlow, 60, 98 ),
                new Patch( PatchID.Shakuhachi, 60, 98 ),
                new Patch( PatchID.Whistle, 60, 98 ),
                new Patch( PatchID.Ocarina, 60, 98 ),
                new Patch( PatchID.Lead1Square, 0, 127 ),
                new Patch( PatchID.Lead2SawTooth, 0, 127 ),
                new Patch( PatchID.Lead3CalliopeLead, 0, 127 ),
                new Patch( PatchID.Lead4ChiffLead, 0, 127 ),
                new Patch( PatchID.Lead5Charang, 0, 127 ),
                new Patch( PatchID.Lead6Voice, 0, 127 ),
                new Patch( PatchID.Lead7Fifths, 0, 127 ),
                new Patch( PatchID.Lead8BassLead, 0, 127 ),
                new Patch( PatchID.Pad1NewAge, 0, 127 ),
                new Patch( PatchID.Pad2Warm, 0, 127 ),
                new Patch( PatchID.Pad3PolySynth, 0, 127 ),
                new Patch( PatchID.Pad4Choir, 0, 127 ),
                new Patch( PatchID.Pad5Bowed, 0, 127 ),
                new Patch( PatchID.Pad6Metallic, 0, 127 ),
                new Patch( PatchID.Pad7Halo, 0, 127 ),
                new Patch( PatchID.Pad8Sweep, 0, 127 ),
                new Patch( PatchID.FX1Rain, 0, 127 ),
                new Patch( PatchID.FX2SoundTrack, 0, 127 ),
                new Patch( PatchID.FX3Crystal, 0, 127 ),
                new Patch( PatchID.FX4Atmosphere, 0, 127 ),
                new Patch( PatchID.FX5Bright, 0, 127 ),
                new Patch( PatchID.FX6Goblins, 0, 127 ),
                new Patch( PatchID.FX7Echoes, 0, 127 ),
                new Patch( PatchID.FX8Scifi, 0, 127 ),
                new Patch( PatchID.Sitar, 0, 127 ),
                new Patch( PatchID.Banjo, 48, 69 ),
                new Patch( PatchID.Shamisen, 0, 127 ),
                new Patch( PatchID.Koto, 0, 127 ),
                new Patch( PatchID.Kalimba, 0, 127 ),
                new Patch( PatchID.Bagpipe, 0, 127 ),
                new Patch( PatchID.Fiddle, 0, 127 ),
                new Patch( PatchID.Shanai, 0, 127 ),
                new Patch( PatchID.TinkleBell, 0, 127 ),
                new Patch( PatchID.Agogo, 0, 127 ),
                new Patch( PatchID.SteelDrums, 0, 127 ),
                new Patch( PatchID.WoodBlock, 0, 127 ),
                new Patch( PatchID.TaikoDrum, 0, 127 ),
                new Patch( PatchID.MelodicTom, 0, 127 ),
                new Patch( PatchID.SynthDrum, 0, 127 ),
                new Patch( PatchID.ReverseCymbal, 0, 127 ),
                new Patch( PatchID.GuitarFretNoise, 0, 127 ),
                new Patch( PatchID.BreathNoise, 0, 127 ),
                new Patch( PatchID.Seashore, 0, 127 ),
                new Patch( PatchID.BirdTweet, 0, 127 ),
                new Patch( PatchID.TelephoneRinging, 0, 127 ),
                new Patch( PatchID.Helicopter, 0, 127 ),
                new Patch( PatchID.Applause, 0, 127 ),
                new Patch( PatchID.Gunshot, 0, 127 )
            };
        }

        #endregion

        #region Members

        //private PlayerOptions iOptionsDialog = null;

        //private static List<String> iMidiDevices = null;

        /*public static string[] MidiDevices
        {
            get
            {
                EnumMidiDevices();
                return iMidiDevices.ToArray();
            }
        }*/

        #endregion

        #region Public methods

        public WavPlayer()
        {
            //WavPlayer.EnumMidiDevices();
        }

        /*
        public static string getWavDeviceName(uint aID)
        {
            int index = (int)(aID + 1);
            return index >= 0 && index < iDevices.Count ? iDevices[index] : "";
        }

        public static String getMidiDeviceName(uint aID)
        {
            return aID < iMidiDevices.Count ? iMidiDevices[(int)aID] : "";
        }

        public void showOption()
        {
            iOptionsDialog = new PlayerOptions();

            List<String> files = new List<string>();
            foreach (Sound sound in iFiles)
            {
                files.Add(Path.GetFileNameWithoutExtension(sound.Path));
            }

            iOptionsDialog.setFiles(files.ToArray());
            iOptionsDialog.setWavDevices(WavPlayer.WavDevices);
            iOptionsDialog.setMidiDevices(WavPlayer.MidiDevices, Enum.GetNames(typeof(Midi.PatchID)), Enum.GetNames(typeof(Midi.Duration)));
            iOptionsDialog.OnFileChanged += new PlayerOptions.FileChangedEvent(Options_FileChanged);
            iOptionsDialog.OnDeviceChanged += new PlayerOptions.DeviceChangedEvent(Options_DeviceChanged);
            iOptionsDialog.OnFileTestRequest += new PlayerOptions.FileTestEvent(Options_FileTestRequest);
            iOptionsDialog.OnReloadFilesRequest += new PlayerOptions.ReloadFilesEvent(Options_ReloadFilesRequest);
            iOptionsDialog.OnWavVolumeDeviceChanged += new PlayerOptions.VolumeDeviceChangedEvent(Options_WavDeviceVolumeChanged);
            iOptionsDialog.OnWavVolumeChanged += new PlayerOptions.VolumeChangedEvent(Options_WavVolumeChanged);
            iOptionsDialog.OnMidiVolumeDeviceChanged += new PlayerOptions.VolumeDeviceChangedEvent(Options_MidiDeviceVolumeChanged);
            iOptionsDialog.OnMidiVolumeChanged += new PlayerOptions.VolumeChangedEvent(Options_MidiVolumeChanged);
            iOptionsDialog.OnNoteTestRequest += new PlayerOptions.NoteTestEvent(Options_NoteTestRequest);
            iOptionsDialog.OnPatchChanged += new PlayerOptions.PatchChangedEvent(Options_PatchChanged);

            iOptionsDialog.ShowDialog();

            iOptionsDialog = null;
        }

        public void updateFileDevices()
        {
            for (int i = 0; i < iFiles.Count; i++ )
            {
                Wav.setDevice((uint)i, iFiles[i].DeviceID);
            }
        }

        public void getMidiVolume(uint aID, out uint aLeft, out uint aRight)
        {
            uint volume = Midi.getVolume(aID);
            aLeft = ((volume & 0xFFFF) + 1) >> 12;
            aRight = ((volume >> 16) + 1) >> 12;
        }

        public void setMidiVolume(uint aID, uint aLeft, uint aRight)
        {
            uint value = ((aLeft << 12) - 1) | (((aRight << 12) - 1) << 16);
            Midi.setVolume(aID, value);
        }

        public void play(uint aIndex)
        {
            Wav.play(aIndex);
        }

        public void play(string aPath, uint aDeviceID)
        {
            foreach (Sound sound in iFiles)
            {
                if (sound.Path == aPath && sound.DeviceID == aDeviceID)
                {
                    Wav.play(sound.Index);
                    break;
                }
            }
        }*/

        public override void getVolume(int aDeviceIndex, out uint aLeft, out uint aRight)
        {
            uint volume = Wav.getVolume((uint)(aDeviceIndex - 1));
            aLeft = ((volume & 0xFFFF) + 1) >> 12;
            aRight = ((volume >> 16) + 1) >> 12;
        }

        public override void setVolume(int aDeviceIndex, uint aLeft, uint aRight)
        {
            uint value = ((aLeft << 12) - 1) | (((aRight << 12) - 1) << 16);
            Wav.setVolume((uint)(aDeviceIndex - 1), value);
        }

        #endregion

        #region Private methods

        protected override void Play(Sound aSound)
        {
            Wav.setDevice(aSound.Index, aSound.DeviceID);
            Wav.play(aSound.Index);
        }

        protected override void AddSound(Sound aSound)
        {
            aSound.DeviceID = uint.MaxValue;
            aSound.Index = Wav.add(aSound.FileName, aSound.DeviceID);
        }

        protected override void AddFiles(string[] aFiles)
        {
            Wav.clear();

            foreach (string file in aFiles)
            {
                uint index = Wav.add(file, uint.MaxValue);
                if (index < uint.MaxValue)
                {
                    Sound sound = new Sound(index, file, uint.MaxValue);
                    iSounds.Add(sound);
                }
                else
                {
                    Debug.WriteLine("failed to add " + file, "WP");
                }
            }
        }

        protected override void EnumDevices()
        {
            if (iDevices.Count == 0)
            {
                EnumDevicesCallback callback = new EnumDevicesCallback(EnumWavDevicesHandler);
                Wav.enumDevices(callback);
            }
        }

        private void EnumWavDevicesHandler(String aName)
        {
            Regex re = new Regex(@"(\()(?<name>(.)+(?<!\)))(\))?");
            Match match = re.Match(aName);
            Group group = match.Groups["name"];
            iDevices.Add(new Device(group.Length > 0 ? group.ToString() : aName, (uint)(iDevices.Count - 1)));
        }

        /*
        private static void EnumMidiDevices()
        {
            if (iMidiDevices == null)
            {
                iMidiDevices = new List<string>();
                EnumDevicesCallback callback = new EnumDevicesCallback(EnumMidiDevicesHandler);
                Midi.enumDevices(callback);
            }
        }

        private static void EnumMidiDevicesHandler(String aName)
        {
            iMidiDevices.Add(aName);
        }

        private void Options_FileChanged(int aIndex)
        {
            iOptionsDialog.setDeviceForCurrent((int)(iFiles[aIndex].DeviceID + 1));
        }

        private void Options_DeviceChanged(int aFileIndex, int aDeviceIndex)
        {
            iFiles[aFileIndex].DeviceID = (uint)(aDeviceIndex - 1);
            Wav.setDevice((uint)aFileIndex, iFiles[aFileIndex].DeviceID);
        }

        private void Options_FileTestRequest(int aIndex)
        {
            Wav.play(iFiles[aIndex].Index);
        }

        private void Options_ReloadFilesRequest(int aDeviceIndex)
        {
            readFromFolder(iSoundsFolder, (uint)(aDeviceIndex - 1));

            if (iOptionsDialog != null)
            {
                List<String> files = new List<string>();
                foreach (Sound sound in iFiles)
                {
                    files.Add(Path.GetFileNameWithoutExtension(sound.Path));
                }
                iOptionsDialog.setFiles(files.ToArray());
                iOptionsDialog.setDeviceForCurrent(-1);
            }
        }

        private void Options_WavDeviceVolumeChanged(int aIndex)
        {
            uint left, right;
            getWavVolume((uint)(aIndex - 1), out left, out right);
            iOptionsDialog.setWavVolumeForCurrent((int)left, (int)right);
        }

        private void Options_MidiDeviceVolumeChanged(int aIndex)
        {
            uint left, right;
            getMidiVolume((uint)aIndex, out left, out right);
            iOptionsDialog.setMidiVolumeForCurrent((int)left, (int)right);
        }

        private void Options_WavVolumeChanged(int aIndex, int aLeft, int aRight)
        {
            setWavVolume((uint)(aIndex - 1), (uint)aLeft, (uint)aRight);
        }

        private void Options_MidiVolumeChanged(int aIndex, int aLeft, int aRight)
        {
            setMidiVolume((uint)aIndex, (uint)aLeft, (uint)aRight);
        }

        public void Options_NoteTestRequest(int aDeviceIndex, int aPatch, int aNote, int aDuration)
        {
            if (Midi.open((uint)aDeviceIndex))
            {
                Midi.play((uint)aPatch, (uint)aNote, 0, (uint)aDuration);
            }
        }

        public void Options_PatchChanged(int aIndex)
        {
            Midi.Patch patch = Midi.Patches[aIndex];
            iOptionsDialog.setNoteRange(patch.NoteFrom, patch.NoteTo);
        }*/

        #endregion
    }
}
