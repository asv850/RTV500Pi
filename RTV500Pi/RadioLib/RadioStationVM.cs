using AlxCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Text;

namespace RadioLib
{
    public class RadioStationVM : VMBase
    {
        private RadioStation _model = null;
        private async void _model_OnPlayingChanged()
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged("BoldCars");
                OnPropertyChanged("Lecture");
            });
        }

        public string Name { get { return _model.Name; } }
        public byte Frequency { get { return _model.Frequency; } }
        public byte DisplayRow { get; private set; }
        public FontWeight BoldCars { get { return _model.IsPlaying ? FontWeights.Bold : FontWeights.Normal; } }
        public Uri SourceHttp { get { return _model.WebAddress; } }
        public bool Playing { get { return _model.IsPlaying; } }
        public RadioStationVM(RadioStation model, byte displayRow = 0)
        {
            _model = model;
            _model.OnPlayingChanged += _model_OnPlayingChanged; ;
            DisplayRow = displayRow;
        }        
    }
}
