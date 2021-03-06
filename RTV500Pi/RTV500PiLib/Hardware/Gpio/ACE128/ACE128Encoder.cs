﻿using Windows.Devices.Gpio;

namespace RTV500PiLib.Hardware.Gpio.ACE128
{
    public delegate void EncoderValueChanged(byte newValue);

    public class ACE128Encoder
    {
        private static readonly byte[] _pinValuesToPosition = new byte[256] { 255, 56, 40, 55, 24, 255, 39, 52, 8, 57, 255, 255, 23, 255, 36, 13, 120, 255, 41, 54, 255, 255, 255, 53, 7, 255, 255, 255, 20, 19, 125, 18, 104, 105, 255, 255, 25, 106, 38, 255, 255, 58, 255, 255, 255, 255, 37, 14, 119, 118, 255, 255, 255, 107, 255, 255, 4, 255, 3, 255, 109, 108, 2, 1, 88, 255, 89, 255, 255, 255, 255, 51, 9, 10, 90, 255, 22, 11, 255, 12, 255, 255, 42, 43, 255, 255, 255, 255, 255, 255, 255, 255, 21, 255, 126, 127, 103, 255, 102, 255, 255, 255, 255, 255, 255, 255, 91, 255, 255, 255, 255, 255, 116, 117, 255, 255, 115, 255, 255, 255, 93, 94, 92, 255, 114, 95, 113, 0, 72, 71, 255, 68, 73, 255, 255, 29, 255, 70, 255, 69, 255, 255, 35, 34, 121, 255, 122, 255, 74, 255, 255, 30, 6, 255, 123, 255, 255, 255, 124, 17, 255, 255, 255, 67, 26, 255, 27, 28, 255, 59, 255, 255, 255, 255, 255, 15, 255, 255, 255, 255, 255, 255, 255, 255, 5, 255, 255, 255, 110, 255, 111, 16, 87, 84, 255, 45, 86, 85, 255, 50, 255, 255, 255, 46, 255, 255, 255, 33, 255, 83, 255, 44, 75, 255, 255, 31, 255, 255, 255, 255, 255, 255, 255, 32, 100, 61, 101, 66, 255, 62, 255, 49, 99, 60, 255, 47, 255, 255, 255, 48, 77, 82, 78, 65, 76, 63, 255, 64, 98, 81, 79, 80, 97, 96, 112, 255 };
        private GpioPin[] _pins = new GpioPin[8];
        private static byte GetPinValue(GpioPin pin)        { return (pin.Read() == GpioPinValue.High) ? (byte)1 : (byte)0; }
        private GpioPin CreatePin(int no)
        {
            GpioPin pin = GpioController.GetDefault().OpenPin(no);
            pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            pin.ValueChanged += pinValueChanged;
            return pin;
        }
        private void pinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            byte newValue = GetEncoderValue();
            if (EncoderValue != newValue)
            {
                EncoderValue = newValue;
                OnEncoderValueChanged?.Invoke(newValue);
            }
        }
        private byte GetEncoderValue()
        {
            byte valeurBrute = GetPinValue(_pins[7]);
            for (short i = 6; i >= 0; i--)
                valeurBrute = (byte)(valeurBrute << 1 | GetPinValue(_pins[i]));
            EncoderScratchValue = valeurBrute;
            return _pinValuesToPosition[valeurBrute];
        }

        public GpioPin[] EncoderPins        { get { return _pins; } }
        public byte EncoderValue            { get; private set; }
        public byte EncoderScratchValue     { get; private set; }
        public ACE128Encoder(byte[] pinsNo)
        {
            for (byte i = 0; i <= 7; i++)
                _pins[i] = CreatePin(pinsNo[i]);
            pinValueChanged(null, null);
        }

        public event EncoderValueChanged OnEncoderValueChanged;
    }
}
