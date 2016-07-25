using System;
using Windows.Devices.Gpio;

namespace RTV500PiLib.Hardware.Gpio.Selectors
{
    [Flags]
    public enum RTV500FrontSelectorButtons { fsbNone = 0, fsbLW = 1, fsbMW = 2, fsbKW = 4, fsbUKW = 8, fsbTATB = 16, fsbMono = 32 };
    public delegate void ButtonsChanged(RTV500FrontSelectorButtons newValue);

    public class RTV500FrontSelector
    {
        public static readonly RTV500FrontSelectorButtons[] BUTTONS_VALUES = new RTV500FrontSelectorButtons[6] { RTV500FrontSelectorButtons.fsbLW, RTV500FrontSelectorButtons.fsbMW, RTV500FrontSelectorButtons.fsbKW, RTV500FrontSelectorButtons.fsbUKW, RTV500FrontSelectorButtons.fsbTATB, RTV500FrontSelectorButtons.fsbMono };
        private GpioPin[] _pins = new GpioPin[6];
        private GpioPin CreatePin(byte noPin)
        {
            GpioPin pin = GpioController.GetDefault().OpenPin(noPin);
            pin.DebounceTimeout = new TimeSpan(0, 0, 0, 0, 20);
            pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            pin.ValueChanged += Pin_ValueChanged;
            return pin;
        }
        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            RTV500FrontSelectorButtons newValue = GetButtonsValue();
            if (PressedButtons != newValue)
            {
                PressedButtons = newValue;
                OnButtonsChanged?.Invoke(newValue);
            }
        }
        private RTV500FrontSelectorButtons GetButtonsValue()
        {
            RTV500FrontSelectorButtons buttons = RTV500FrontSelectorButtons.fsbNone;
            for (byte i = 0; i < 6; i++)
                if (_pins[i].Read() == GpioPinValue.Low)
                    buttons |= BUTTONS_VALUES[i];
            return buttons;
        }

        public RTV500FrontSelectorButtons PressedButtons    { get; private set; }
        public GpioPin[] ButtonsPins                        { get { return _pins; } }
        public RTV500FrontSelector(byte[] pinsNo)
        {
            for (byte i = 0; i <= 5; i++)
                _pins[i] = CreatePin(pinsNo[i]);
            Pin_ValueChanged(null, null);
        }
        public event ButtonsChanged OnButtonsChanged;
    }
}
