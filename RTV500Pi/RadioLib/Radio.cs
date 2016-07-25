using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media.Playback;
using Windows.Storage;

namespace RadioLib
{  
    public delegate void MediaPlayerStateChange();
    public delegate void TuneChange();
    public delegate void StationChange();
    public delegate void BandChange();

    public struct RadioPosition
    {
        public double ReceipValue;
        public RadioStation Station;
    }
    public enum WaveBand { wbNone, wbLW, wbMW, wbKW, wbUKW };
    public class Radio
    {
        private Collection<RadioStation>[] _radios = new Collection<RadioStation>[4] { new Collection<RadioStation>(), new Collection<RadioStation>(), new Collection<RadioStation>(), new Collection<RadioStation>() };
        private RadioPosition[,] _tabRadios = new RadioPosition[4, 201];
        private RadioStation _currentStation = null;
        private StorageFile[] _bandNoisesFiles = new StorageFile[4];
        private byte _tunePosition = 3;
        private WaveBand _band = WaveBand.wbNone;
        private object _lockBand = new object();
        private object _lockTunePosition = new object();
        private void Current_CurrentStateChanged(MediaPlayer sender, object args) { OnMediaPlayerStateChanged?.Invoke(); }
        private void SetGalvanometer(double val) { if (Galva != null) Galva.Value = val; }
        private void SetBackgroundVolume(double val) { BackgroundMediaPlayer.Current.Volume = Math.Max(-val / 4 + 1, 0); }
        private void SetStereoLight(bool on) { if (StereoLight != null) StereoLight.IsOn = on; }
        private void UpdateStation(bool newFreq, bool newSource)
        {
            if ((_band == WaveBand.wbNone))
            {
                SetGalvanometer(0);
                SetBackgroundVolume(5);
                SetStereoLight(false);
                if (_currentStation != null) _currentStation.IsPlaying = false;
                _currentStation = null;
                UrlPlayed = null;
                OnStationChanged?.Invoke();
            }
            else
            {
                if (newSource) BackgroundMediaPlayer.Current.SetFileSource(_bandNoisesFiles[(int)_band - 1]);
                bool stationchange = false;
                lock (_lockTunePosition) lock (_lockBand)
                    {
                        RadioPosition pRadio = _tabRadios[(int)_band - 1, _tunePosition];
                        SetGalvanometer(pRadio.ReceipValue);
                        SetBackgroundVolume(pRadio.ReceipValue);
                        SetStereoLight(pRadio.ReceipValue > 2.5 && pRadio.Station.Stereo);
                        if (_currentStation != pRadio.Station)
                        {
                            if (pRadio.Station == null)
                                UrlPlayed = null;
                            else
                            {
                                pRadio.Station.IsPlaying = true;
                                UrlPlayed = pRadio.Station.WebAddress;
                            }
                            if (_currentStation != null) _currentStation.IsPlaying = false;
                            _currentStation = pRadio.Station;
                            stationchange = true;
                        }
                    }
                if (stationchange) OnStationChanged?.Invoke();
            }
            if (newFreq) OnTuneChanged?.Invoke();
            if (newSource) OnBandChanged?.Invoke();
        }

        public byte TunePosition
        {
            get { return _tunePosition; }
            set
            {
                bool change = false;
                lock (_lockTunePosition)
                    if (_tunePosition != value)
                    {
                        change = true;
                        _tunePosition = value;
                    }
                if (change) UpdateStation(true, false);
            }
        }
        public Collection<RadioStation> RadiosLW { get { return _radios[0]; } }
        public Collection<RadioStation> RadiosMW { get { return _radios[1]; } }
        public Collection<RadioStation> RadiosKW { get { return _radios[2]; } }
        public Collection<RadioStation> RadiosUKW { get { return _radios[3]; } }
        public Uri UrlPlayed { get; private set; }
        public IGalvanometer Galva { get; set; }
        public ILight StereoLight { get; set; }
        public WaveBand Band
        {
            get { return _band; }
            set
            {
                bool change = false;
                lock (_lockBand)
                    if (_band != value)
                    {
                        change = true;
                        _band = value;
                    }
                if (change) UpdateStation(false, true);
            }
        }
        public Radio() { }
        public async Task Initialize(string[] NoisesFileNames)
        {
            _bandNoisesFiles[0] = await Package.Current.InstalledLocation.GetFileAsync(NoisesFileNames[0]);
            _bandNoisesFiles[1] = await Package.Current.InstalledLocation.GetFileAsync(NoisesFileNames[1]);
            _bandNoisesFiles[2] = await Package.Current.InstalledLocation.GetFileAsync(NoisesFileNames[2]);
            _bandNoisesFiles[3] = await Package.Current.InstalledLocation.GetFileAsync(NoisesFileNames[3]);
            BackgroundMediaPlayer.Current.IsLoopingEnabled = true;
            UpdateStation(true, true);
            BackgroundMediaPlayer.Current.Play();
        }
        public RadioStation AddStation(string name, WaveBand band, byte frequency, string address, bool stereo, byte displayRow, double[] ReceipValues)
        {
            RadioStation res = new RadioStation(name, frequency, address, stereo, displayRow, ReceipValues);
            _radios[(int)band - 1].Add(res);

            for (byte i = 0; i < ReceipValues.Length; i++)
            {
                _tabRadios[(int)band - 1, frequency - res.IndexOfMaxReceipValue + i].ReceipValue = ReceipValues[i];
                _tabRadios[(int)band - 1, frequency - res.IndexOfMaxReceipValue + i].Station = res;
            }
            return res;
        }

        public event MediaPlayerStateChange OnMediaPlayerStateChanged;
        public event TuneChange OnTuneChanged;
        public event StationChange OnStationChanged;
        public event BandChange OnBandChanged;

        public static void AddParisRadios(Radio radio, WaveBand band)
        {
            radio.AddStation("France Inter", band, 190, "http://audio.scdn.arkena.com/11008/franceinter-midfi128.mp3", true, 0, new double[] { 1, 2, 3, 4, 3, 2, 1 });
            radio.AddStation("RFI", band, 180, "http://rfi-monde-64k.scdn.arkena.com/rfimonde.mp3", true, 0, new double[] { 1, 2, 3, 4, 3, 2, 1 });
            radio.AddStation("Nostalgie", band, 169, "http://cdn.nrjaudio.fm/audio1/fr/30601/mp3_128.mp3?origine=listenlive", true, 0, new double[] { 1, 2, 3, 4, 3, 2, 1 });
            radio.AddStation("Chérie FM", band, 162, "http://cdn.nrjaudio.fm/audio1/fr/30201/mp3_128.mp3?origine=listenlive", true, 1, new double[] { 1, 4, 3, 2, 1 });
            radio.AddStation("France Musique", band, 158, "http://audio.scdn.arkena.com/11012/francemusique-midfi128.mp3", true, 0, new double[] { 2, 4.5, 4, 2 });
            radio.AddStation("Le Mouv'", band, 155, "http://audio.scdn.arkena.com/11014/mouv-midfi128.mp3", true, 2, new double[] { 1, 2, 3, 4, 2 });
            radio.AddStation("France Culture", band, 144, "http://audio.scdn.arkena.com/11010/franceculture-midfi128.mp3", true, 0, new double[] { 1, 2, 3, 4, 3, 2, 1 });
            radio.AddStation("Skyrock", band, 124, "http://icecast.skyrock.net/s/natio_aac_64k?tvr_name=yourplayer&tvr_section1=64", true, 0, new double[] { 2, 4, 3, 2, 1 });
            radio.AddStation("BFM", band, 121, "http://bfmbusiness.scdn.arkena.com/bfmbusiness.mp3", true, 1, new double[] { 1, 2, 3, 4, 2 });
            radio.AddStation("Rires & Chansons", band, 113, "http://cdn.nrjaudio.fm/audio1/fr/30401/mp3_128.mp3?origine=listenlive", true, 0, new double[] { 1, 2, 3, 4, 3, 2, 1 });
            radio.AddStation("NRJ", band, 89, "http://cdn.nrjaudio.fm/audio1/fr/30001/mp3_128.mp3?origine=listenlive", true, 0, new double[] { 1, 2, 3, 4, 3, 2, 1 });
            radio.AddStation("Radio Classique", band, 82, "http://broadcast.infomaniak.net:80/radioclassique-high.mp3", true, 1, new double[] { 1, 2, 3, 4, 3, 2, 1 });
            radio.AddStation("FUN Radio", band, 75, "http://ais.rtl.fr:80/fun-1-44-128", true, 0, new double[] { 1, 2, 4, 3, 2, 1 });
            radio.AddStation("Oui FM", band, 71, "http://ice5.infomaniak.ch:80/ouifm-high.mp3", true, 1, new double[] { 1, 4, 1 });
            radio.AddStation("MFM", band, 68, "http://mfm.ice.infomaniak.ch:80/mfm-128", true, 0, new double[] { 2, 4, 1 });
            radio.AddStation("RMC", band, 64, "http://rmc.scdn.arkena.com/rmc.mp3", true, 1, new double[] { 2, 4, 3, 2 });
            radio.AddStation("Virgin Radio", band, 61, "http://mp3lg4.tdf-cdn.com/9243/lag_164753.mp3", true, 0, new double[] { 2, 4, 2 });
            radio.AddStation("RFM", band, 57, "http://rfm-live-mp3-128.scdn.arkena.com/rfm.mp3", true, 1, new double[] { 2, 4, 3, 2 });
            radio.AddStation("RTL", band, 53, "http://ais.rtl.fr:80/rtl-1-44-128", true, 0, new double[] { 2, 4, 3, 2 });
            radio.AddStation("Europe 1", band, 50, "http://mp3lg4.tdf-cdn.com/9240/lag_180945.mp3", true, 1, new double[] { 2, 4, 2 });
            radio.AddStation("FIP", band, 46, "http://audio.scdn.arkena.com/11016/fip-midfi128.mp3", true, 0, new double[] { 2, 4, 3, 2 });
            radio.AddStation("France Info", band, 42, "http://audio.scdn.arkena.com/11006/franceinfo-midfi128.mp3", false, 1, new double[] { 2, 4, 3, 2 });
            radio.AddStation("RTL 2", band, 38, "http://ais.rtl.fr:80/rtl2-1-44-128", true, 0, new double[] { 1, 2, 3, 4, 3, 2 });
            radio.AddStation("Beur FM", band, 30, "http://ice14.infomaniak.ch:80/beurfm-high.mp3", true, 0, new double[] { 2, 4, 3, 2, 1 });
            radio.AddStation("France Bleu", band, 26, "http://audio.scdn.arkena.com/11719/fb1071-midfi128.mp3", true, 1, new double[] { 1, 2, 3, 4, 3, 2 });
            radio.AddStation("Autoroute Info", band, 20, "http://media.autorouteinfo.fr:8000/direct_sud.mp3", true, 0, new double[] { 1, 2, 3, 4, 3, 2, 1 });
        }
        public static void AddAddictRadios(Radio radio, WaveBand band)
        {
            radio.AddStation("Addict Radio Alternative", band, 100, "http://stream6.addictradio.net/addictalternative.mp3", true, 0, new double[] { 1, 2, 3, 4, 5, 4, 3, 2, 1 });
            radio.AddStation("Addict Radio Lounge", band, 120, "http://stream8.addictradio.net/addictlounge.mp3", true, 1, new double[] { 1, 2, 3, 4, 5, 4, 3, 2, 1 });
            radio.AddStation("Addict Radio Star", band, 140, "http://stream6.addictradio.net/addictstar.mp3", true, 0, new double[] { 1, 2, 3, 4, 5, 4, 3, 2, 1 });
            radio.AddStation("Addict Radio Rock", band, 160, "http://stream8.addictradio.net/addictrock.mp3", true, 1, new double[] { 1, 2, 3, 4, 5, 4, 3, 2, 1 });
            radio.AddStation("Addict Radio by La Détente Générale",band, 180, "http://stream9.addictradio.net:80/addictldg", true, 0,new double[] { 1, 2, 3, 4, 5, 4, 3, 2, 1 });
        }
        public static void AddFrenchLWRadios(Radio radio)
        {
            radio.AddStation("FRANCE INTER", WaveBand.wbLW, 163, "http://audio.scdn.arkena.com/11008/franceinter-midfi128.mp3", false, 0, new double[] { 1,2,3,4,4.5,5,4.5,4,3,2,1});
            radio.AddStation("EUROPE 1", WaveBand.wbLW, 141, "http://mp3lg4.tdf-cdn.com/9240/lag_180945.mp3", false, 0, new double[] { 1, 2, 3, 4, 4.5, 5, 4.5, 4, 3, 2, 1 });
            radio.AddStation("RADIO MONTE CARLO", WaveBand.wbLW, 114, "http://rmc.scdn.arkena.com/rmc.mp3", false, 1, new double[] { 1, 2, 3, 4, 4.5, 5, 4.5, 4, 3, 2, 1 });
            radio.AddStation("LUXEMBOURG", WaveBand.wbLW, 100, "http://ais.rtl.fr:80/rtl-1-44-128", false, 0, new double[] { 1, 2, 3, 4, 4.5, 5, 4.5, 4, 3, 2, 1 });
        }
    }

    public interface IGalvanometer
    {
        double Value { get; set; }
        bool Enabled { get; set; }
    }
    public interface ILight
    {
        bool IsOn { get; set; }
    }
}
