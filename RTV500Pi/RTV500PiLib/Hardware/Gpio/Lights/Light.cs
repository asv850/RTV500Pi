using RadioLib;
using Windows.Devices.Gpio;

namespace RTV500PiLib.Hardware.Gpio.Lights
{
    public class Light : ILight
    {
        private GpioPin _gpioPin = null;
        private bool _isOn = true;

        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                if (_isOn != value)
                {
                    _gpioPin.Write(value ? GpioPinValue.High : GpioPinValue.Low);
                    _isOn = value;
                }

            }
        }
        public Light(byte pinNumber)
        {
            _gpioPin = GpioController.GetDefault().OpenPin(pinNumber);
            _gpioPin.SetDriveMode(GpioPinDriveMode.Output);
            IsOn = false;
        }
    }
}
