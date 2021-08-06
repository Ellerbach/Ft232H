using Iot.Device.FtCommon;
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// Ft232HI2cBus
    /// Commands are available in: 
    /// - AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf
    /// - AN_113_FTDI_Hi_Speed_USB_To_I2C_Example.pdf
    /// To understand the signal need: https://training.ti.com/sites/default/files/docs/slides-i2c-protocol.pdf
    /// </summary>
    internal class Ft232HI2cBus : I2cBus
    {
        private HashSet<int> _usedAddresses = new HashSet<int>();

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ft232HDevice DeviceInformation { get; private set; }

        public Ft232HI2cBus(Ft232HDevice deviceInformation)
        {
            DeviceInformation = deviceInformation;
            DeviceInformation.I2cInitialize();
        }

        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (!_usedAddresses.Add(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} is already open.", nameof(deviceAddress));
            }

            return new Ft232HI2c(this, deviceAddress);
        }

        public override void RemoveDevice(int deviceAddress)
        {
            if (!_usedAddresses.Remove(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} was not open.", nameof(deviceAddress));
            }
        }

        internal void Read(int deviceAddress, Span<byte> buffer)
        {
            DeviceInformation.I2cStart();
            var ack = DeviceInformation.I2cSendDeviceAddrAndCheckACK((byte)deviceAddress, true);
            if (!ack)
            {
                DeviceInformation.I2cStop();
                throw new IOException($"Error reading device while setting up address");
            }

            for (int i = 0; i < buffer.Length - 1; i++)
            {
                buffer[i] = DeviceInformation.I2CReadByte(true);
            }

            if (buffer.Length > 0)
            {
                buffer[buffer.Length - 1] = DeviceInformation.I2CReadByte(false);
            }

            DeviceInformation.I2cStop();
        }

        internal void Write(int deviceAddress, ReadOnlySpan<byte> buffer)
        {
            DeviceInformation.I2cStart();
            var ack = DeviceInformation.I2cSendDeviceAddrAndCheckACK((byte)deviceAddress, false);
            if (!ack)
            {
                DeviceInformation.I2cStop();
                throw new IOException($"Error writing device while setting up address");
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                ack = DeviceInformation.I2cSendByteAndCheckACK(buffer[i]);
                if (!ack)
                {
                    DeviceInformation.I2cStop();
                    throw new IOException($"Error writing device on byte {i}");
                }
            }

            DeviceInformation.I2cStop();
        }
    }
}
