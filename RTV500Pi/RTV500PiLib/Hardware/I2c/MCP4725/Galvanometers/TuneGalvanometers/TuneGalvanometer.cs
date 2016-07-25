using RadioLib;
using System;
using System.Threading.Tasks;

namespace RTV500PiLib.Hardware.I2c.MCP4725.Galvanometers.TuneGalvanometers
{
    public class TuneGalvanometer : IGalvanometer
    {
        private MCP4725Dac _dac = null;
        private double _value = 0;
        private bool _enabled = true;
        private ushort ValueTo128Bits(double value)     { return (ushort)(12.4009324 * Math.Pow(value, 4) - 81.212121 * Math.Pow(value, 3) + 210.51282 * Math.Pow(value, 2) + 227.202797 * value + 4.89510489); }
        private void UpdateDac()
        {
            if (_dac.IsInitialized)
                _dac.SetValues(ValueTo128Bits(_value), _enabled ? PowerDownModes.pwdNormal : PowerDownModes.pwd500K);
        }

        public double Value
        {
            get { return _value; }
            set
            {
                double limitedValue = Math.Max(0, Math.Min(value, 5));
                if (limitedValue != _value)
                {
                    _value = limitedValue;
                    UpdateDac();
                }
            }
        }
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    UpdateDac();
                }
            }
        }
        public TuneGalvanometer(byte i2cAdress)     { _dac = new MCP4725Dac(i2cAdress); }
        public async Task Initialize()
        {
            if (!_dac.IsInitialized)
                await _dac.InitializeAsync();
            if (_dac.IsInitialized)
            {
                MCP4725Registers currentValues = _dac.GetValues();
                _value = currentValues.DACValue;
                _enabled = (currentValues.CurrentPowerDownMode == PowerDownModes.pwdNormal);
            }
        }
    }
}
