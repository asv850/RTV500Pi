using AlxCommonLib;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;

namespace RadioLib
{
    public class RadioVM : VMBase
    {
        private Radio _model = null;
        private double[,] _EquaCoeffs = new double[4, 4]    { { 3.5907e-5 , -9.047e-3 , -0.6634396 ,  355.195749 }, { 7.1351e-5 ,  9.9946e-4, -9.1815516 , 1740.68967 }, { 1.1528e-6,-2.091e-4,-0.0577759,16.7022185 }, { 5.1197e-7,-2.452e-4,-0.0865183,109.528576 } };
        private string[] _Arrondis = new string[4] { "N0", "N0", "N1", "N1" };
        private string[] _Unites = new string[4] { "kHz", "kHz", "MHz", "MHz" };
        private ObservableCollection<RadioStationVM>[] _radios = new ObservableCollection<RadioStationVM>[4] { new ObservableCollection<RadioStationVM>(), new ObservableCollection<RadioStationVM>(), new ObservableCollection<RadioStationVM>(), new ObservableCollection<RadioStationVM>() };
        private async void _model_OnStationChanged() { await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { OnPropertyChanged("UrlPlayed"); }); }
        private async void _model_OnTuneChanged()
        {        
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {                
                OnPropertyChanged("TunePosition");
                OnPropertyChanged("FrequencyHz");
            });
        }
        private async void _model_OnBandChanged()
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
            {
                OnPropertyChanged("Band");
                OnPropertyChanged("FrequencyHz");
            });
        }
        private void CreateStationRadioViewModels()
        {
            foreach (RadioStation station in _model.RadiosLW)
                _radios[0].Add(new RadioStationVM(station, station.DisplayRow));
            foreach (RadioStation station in _model.RadiosMW)
                _radios[1].Add(new RadioStationVM(station, station.DisplayRow));
            foreach (RadioStation station in _model.RadiosKW)
                _radios[2].Add(new RadioStationVM(station, station.DisplayRow));
            foreach (RadioStation station in _model.RadiosUKW)
                _radios[3].Add(new RadioStationVM(station, station.DisplayRow));
        }
        private double Equa3(byte idx, byte freq) { return Math.Pow(freq, 3) * _EquaCoeffs[idx, 0] + Math.Pow(freq, 2) * _EquaCoeffs[idx, 1] + freq * _EquaCoeffs[idx, 2] + _EquaCoeffs[idx, 3]; }

        public ObservableCollection<RadioStationVM> RadiosLW { get { return _radios[0]; } }
        public ObservableCollection<RadioStationVM> RadiosMW { get { return _radios[1]; } }
        public ObservableCollection<RadioStationVM> RadiosKW { get { return _radios[2]; } }
        public ObservableCollection<RadioStationVM> RadiosUKW { get { return _radios[3]; } }
        public Uri UrlPlayed { get { return _model.UrlPlayed; } }
        public string FrequencyHz
        {
            get
            {
                if (Band == WaveBand.wbNone)
                    return "";
                else
                {
                    byte idx = (byte)(Band - 1);
                    return Equa3(idx, TunePosition).ToString(_Arrondis[idx]) + " " + _Unites[idx];
                }
            }
        }
        public byte TunePosition
        {
            get { return _model.TunePosition; }
            set { _model.TunePosition = value; }
        }
        public WaveBand Band
        {
            get { return _model.Band; }
            set { _model.Band = value; }
        }
        public RadioVM(Radio modele)
        {
            _model = modele;
            _model_OnTuneChanged();
            _model_OnStationChanged();
            CreateStationRadioViewModels();
            _model.OnTuneChanged += _model_OnTuneChanged; ;
            _model.OnStationChanged += _model_OnStationChanged;
            _model.OnBandChanged += _model_OnBandChanged;
        }
        public void UpdateTunePosition() { OnPropertyChanged("TunePosition"); }
    }
}
