using Windows.Devices.Gpio;

namespace RTV500PiLib.Hardware.Gpio.ACE128.TuneButton
{
    public class TuneButton
    {
        private ACE128Encoder _encoder = null;
        private GpioPin _extPin = null;
        private byte _switchedValue = 0;
        private byte _invertValue = 0;
        private void _encoder_OnValueChanged(byte newEncoderValue)
        {
            if ((newEncoderValue < _switchedValue) && (_extPin.Read() == GpioPinValue.High))
                Value = (byte)(newEncoderValue + 128);
            else
                Value = newEncoderValue;
            if (_invertValue != 0)
                Value = (byte)(_invertValue - Value);
            OnValueChanged?.Invoke(Value);
        }

        public ACE128Encoder Encoder            { get { return _encoder; } }
        public GpioPin ExtPin                   { get { return _extPin; } }
        public byte Value                       { get; private set; }
        public TuneButton(byte[] gpioPins, byte switchValue, byte invertValue)
        {
            _encoder = new ACE128Encoder(gpioPins);
            _extPin = GpioController.GetDefault().OpenPin(gpioPins[8]);
            _extPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            _switchedValue = switchValue;
            _invertValue = invertValue;
            _encoder_OnValueChanged(_encoder.EncoderValue);
            _encoder.OnEncoderValueChanged += _encoder_OnValueChanged;
        }

        public event EncoderValueChanged OnValueChanged;
    }
}
