using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation.Metadata;

namespace RTV500PiLib.Hardware.I2c
{
    public class I2cException : Exception
    {
        public I2cException(string message) : base(message) { }
        public I2cException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class DeviceNotInitializedException : I2cException { public DeviceNotInitializedException() : base("The I2C device has not been initialized.") { } }
    public class InvalidAddressException : I2cException { public InvalidAddressException() : base("Slave address was not acknowledged.") { } }
    public class NoI2cControllerException : I2cException { public NoI2cControllerException() : base("No I2C controllers were found on the system.") { } }
    public class DeviceNotFoundException : I2cException { public DeviceNotFoundException() : base("The I2c device was not found.") { } }
    public class DeviceInUseException : I2cException { public DeviceInUseException() : base("Slave address on I2C Controller is currently in use by another application. Please ensure that no other applications are using I2C..") { } }

    public class I2c : IDisposable
    {
        private bool _initialized = false;
        private byte _deviceAddress = 0;
        private I2cBusSpeed _busSpeed = I2cBusSpeed.FastMode;
        private I2cDevice _device = null;

        protected virtual void OnDispose() { }
        protected virtual I2cDevice Device          /// A reference to the I2cDevice retrieved from a call to I2cDevice.FromIdAsync.
        {
            get { return _device; }
            set { _device = value; }
        }
        protected virtual void OnInitialize()
        {
            byte[] readBuffer = new byte[2];
            this.Read(readBuffer);                       // *** The default behavior is to attempt a read
        }
        protected virtual Task OnResetAsync()
        {
            return Task.FromResult(0);
        }

        public byte DeviceAddress                   /// Gets the device address used when this instance of the was created.
        {
            get { return _deviceAddress; }
            private set { _deviceAddress = value; }
        }
        public I2cBusSpeed BusSpeed                 /// Gets the I2C bus speed set when this instance of the device was created.
        {
            get { return _busSpeed; }
            private set { _busSpeed = value; }
        }
        public bool IsInitialized                           /// Gets a value that indicates if the device was initialized successfully or not. True indicates that Initialize() was successfully called.
        {
            get { return _initialized; }
            private set { _initialized = value; }
        }
        public I2c(byte deviceAddress)
        {
            this.DeviceAddress = deviceAddress;
            this.BusSpeed = I2cBusSpeed.StandardMode;
        }
        public I2c(byte deviceAddress, I2cBusSpeed busSpeed)
        {
            this.DeviceAddress = deviceAddress;
            this.BusSpeed = busSpeed;
        }
        public async Task InitializeAsync()               /// Initializes the I2C device. Returns an InitializationResult value indicating if the initialization was success or not.
        {
            string aqs = I2cDevice.GetDeviceSelector();                                     // *** Get a selector string that will return all I2C controllers on the system
            DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs).AsTask();                   // *** Find the I2C bus controller device with our selector string
            if (dis.Count > 0)
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(this.DeviceAddress);
                settings.BusSpeed = this.BusSpeed;

                this.Device = await I2cDevice.FromIdAsync(dis[0].Id, settings);             // *** Create an I2cDevice with our selected bus controller and I2C settings
                if (this.Device != null)
                {
                    IsInitialized = true;
                    try
                    {
                        this.OnInitialize();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Slave address was not acknowledged"))                                  // *** Looking for: "The system cannot find the file specified. Slave address was not acknowledged."
                            throw new InvalidAddressException();
                        else
                            throw;
                    }
                }
                else
                    throw new DeviceInUseException();
            }
            else
                throw new NoI2cControllerException();
        }
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            if (this.IsInitialized)
                this.Device.WriteRead(writeBuffer, readBuffer);
            else
                throw new DeviceNotInitializedException();
        }
        public void Write(byte[] writeBuffer)
        {
            if (this.IsInitialized)
                this.Device.Write(writeBuffer);
            else
                throw new DeviceNotInitializedException();
        }
        public void Read(byte[] readBuffer)
        {
            if (this.IsInitialized)
                this.Device.Read(readBuffer);
            else
                throw new DeviceNotInitializedException();
        }
        public byte[] ReadBytes(int bufferSize)
        {
            byte[] readBuffer = new byte[bufferSize];
            this.Read(readBuffer);
            return readBuffer;
        }
        public byte[] ReadRegisterBytes(byte registerId, int bufferSize)
        {
            byte[] readBuffer = new byte[bufferSize];
            this.WriteRead(new byte[] { registerId }, readBuffer);
            return readBuffer;
        }
        public async Task ResetAsync()
        {
            await this.OnResetAsync();
        }
        public static bool IsAvailable()
        {
            return ApiInformation.IsTypePresent(typeof(I2cDevice).FullName);
        }
        public void Dispose()
        {
            try
            {
                this.OnDispose();
            }
            finally
            {
                if (_device != null) _device.Dispose();
            }
        }
    }
}
