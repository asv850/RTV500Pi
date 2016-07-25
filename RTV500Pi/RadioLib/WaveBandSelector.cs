namespace RadioLib
{
    public delegate void SelectorChanged(WaveBand newValue);

    public class WaveBandSelector
    {
        private WaveBand _selected = WaveBand.wbNone;

        public WaveBand Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnSelectorChanged?.Invoke(value);
                }
            }
        }

        public event SelectorChanged OnSelectorChanged;
    }
}
