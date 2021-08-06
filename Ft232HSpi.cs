using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Ft232H
{
    public class Ft232HSpi : SpiDevice
    {
        private readonly SpiConnectionSettings _settings;

        /// <inheritdoc/>
        public override SpiConnectionSettings ConnectionSettings => _settings;

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ft232HDevice DeviceInformation { get; internal set; }

        public Ft232HSpi(SpiConnectionSettings settings, Ft232HDevice deviceInformation)
        {
            DeviceInformation = deviceInformation;
            _settings = settings;
            if ((_settings.ChipSelectLine < 3) || (_settings.ChipSelectLine > Ft232HDevice.PinCountConst))
            {
                throw new ArgumentException($"Chip Select line has to be between 3 and {Ft232HDevice.PinCountConst - 1}");
            }

            if (DeviceInformation._connectionSettings.Where(m => m.ChipSelectLine == _settings.ChipSelectLine).Any())
            {
                throw new ArgumentException("Chip Select already in use");
            }

            // Open the device
            DeviceInformation._connectionSettings.Add(_settings);
            DeviceInformation.SpiInitialize();
        }

        public override void Read(Span<byte> buffer)
        {
            DeviceInformation.SpiRead(_settings, buffer);
        }

        public override byte ReadByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            DeviceInformation.SpiRead(_settings, buffer);
            return buffer[0];
        }

        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            DeviceInformation.SpiWriteRead(_settings, writeBuffer, readBuffer);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            DeviceInformation.SpiWrite(_settings, buffer);
        }

        public override void WriteByte(byte value)
        {
            DeviceInformation.SpiWrite(_settings, stackalloc byte[1] { value });
        }

        protected override void Dispose(bool disposing)
        {
            DeviceInformation._connectionSettings.Remove(_settings);
            DeviceInformation.SpiDeinitialize();
            base.Dispose(disposing);
        }
    }
}
