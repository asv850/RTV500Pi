using AlxCommonLib;

namespace RadioLib
{
    public class WaveBandSelectorVM : VMBase
    {
        private WaveBandSelector _model = null;
        private bool[] _isOn = new bool[4] { false, false, false, false };       
        private void AllOff()
        {
            for(byte i=0; i<4; i++)
                _isOn[i] = false;
        }
        private void RaisePropertyChanged()
        {
            OnPropertyChanged("IsLWOn");
            OnPropertyChanged("IsMWOn");
            OnPropertyChanged("IsKWOn");
            OnPropertyChanged("IsUKWOn");
        }
        private void UpdateSelector(WaveBand band, bool val)
        {
            byte b = (byte)(band - 1);
            if (_isOn[b] != val)
            {
                if (val) AllOff();
                _isOn[b] = val;
                RaisePropertyChanged();
                UpdateModel();
            }
        }
        private void UpdateModel()
        {
            for (byte i = 0; i < 4; i++)
                if (_isOn[i])
                {
                    _model.Selected = (WaveBand)i + 1;
                    return;
                }
            _model.Selected = WaveBand.wbNone; ;
        }
        
        public bool IsLWOn
        {
            get { return _isOn[0]; }
            set { UpdateSelector(WaveBand.wbLW, value); }
        }
        public bool IsMWOn
        {
            get { return _isOn[1]; }
            set { UpdateSelector(WaveBand.wbMW, value);  }
        }
        public bool IsKWOn
        {
            get { return _isOn[2]; }
            set { UpdateSelector(WaveBand.wbKW, value); }
        }
        public bool IsUKWOn
        {
            get { return _isOn[3]; }
            set { UpdateSelector(WaveBand.wbUKW, value); }
        }
        public WaveBandSelectorVM(WaveBandSelector model)
        {
            _model = model;       
        }
    }
}
