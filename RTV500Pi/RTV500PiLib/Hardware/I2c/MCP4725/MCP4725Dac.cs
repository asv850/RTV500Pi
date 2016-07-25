namespace RTV500PiLib.Hardware.I2c.MCP4725
{
    public enum PowerDownModes : byte { pwdNormal, pwd1K, pwd100K, pwd500K };
    public class MCP4725Registers
    {
        public bool EepromWriteStatusComplete { get; private set; }
        public bool PowerOnReset { get; private set; }
        public PowerDownModes CurrentPowerDownMode { get; private set; }
        public ushort DACValue { get; private set; }
        public PowerDownModes EepromPowerDownMode { get; private set; }
        public ushort EEpromValue { get; private set; }
        public MCP4725Registers(byte[] donneesLues)
        {
            EepromWriteStatusComplete = (donneesLues[0] & (byte)128) != 0;
            PowerOnReset = (donneesLues[0] & (byte)64) != 0;
            CurrentPowerDownMode = (PowerDownModes)((donneesLues[0] >> (byte)1) & 3);
            DACValue = (ushort)((donneesLues[1] << (byte)4) | (donneesLues[2] >> 4));
            EepromPowerDownMode = (PowerDownModes)((donneesLues[3] >> (byte)5) & 3);
            EEpromValue = (ushort)(((donneesLues[3] & 0xF) << 8) | (ushort)(donneesLues[4]));
        }
    }

    public class MCP4725Dac : I2c
    {
        public MCP4725Dac(byte i2cAddress) : base(i2cAddress) { }
        public MCP4725Registers GetValues()
        {
            byte[] donneesLues = ReadBytes(5);
            return new MCP4725Registers(donneesLues);
        }
        public void SetValues(ushort value, PowerDownModes powerDownMode, bool eepromToo = false)
        {
            byte[] donneesAEcrire = new byte[3];
            donneesAEcrire[0] = (byte)(((byte)(eepromToo ? 0x60 : 0x40)) | (byte)(((byte)powerDownMode << 1)));
            donneesAEcrire[1] = (byte)(value >> 4);
            donneesAEcrire[2] = (byte)(value << 4);
            Write(donneesAEcrire);
        }
    }
}
