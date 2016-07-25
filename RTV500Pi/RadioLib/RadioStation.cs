using System;

namespace RadioLib
{
    public delegate void PlayingChange();

    public class RadioStation
    {
        private byte _frequency = 0;
        private Uri _uri = null;
        private bool _stereo = false;
        private bool _playing = false;
        private int MaxIndex(double[] vals)
        {
            double maxVal = vals[0];
            int res = 0;
            for (int i = 1; i < vals.Length; i++)
                if (vals[i] > maxVal)
                {
                    maxVal = vals[i];
                    res = i;
                }
            return res;
        }

        public string Name { get; private set; }
        public byte Frequency { get { return _frequency; } }
        public Uri WebAddress { get { return _uri; } }
        public bool Stereo { get { return _stereo; } }
        public bool IsPlaying
        {
            get { return _playing; }
            set
            {
                if (value != _playing)
                {
                    _playing = value;
                    OnPlayingChanged?.Invoke();
                }
            }
        }
        public byte DisplayRow { get; private set; }
        public double[] ReceipValues { get; private set; }
        public int IndexOfMaxReceipValue { get; private set; }
        public RadioStation(string name, byte frequency, string address, bool stereo, byte displayRow, double[] ReceiptValues)
        {
            Name = name;
            _frequency = frequency;
            _uri = new Uri(address);
            _stereo = stereo;
            DisplayRow = displayRow;
            ReceipValues = ReceiptValues;
            IndexOfMaxReceipValue = MaxIndex(ReceiptValues);
        }

        public event PlayingChange OnPlayingChanged;
    }
}
