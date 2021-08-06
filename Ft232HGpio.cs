
using Iot.Device.FtCommon;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Ft232H
{
    public class Ft232HGpio : GpioDriver
    {
        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ft232HDevice DeviceInformation { get; private set; }

        protected override int PinCount => Ft232HDevice.PinCountConst;

        internal Ft232HGpio(Ft232HDevice deviceInformation)
        {
            DeviceInformation = deviceInformation;
            // Open device
            DeviceInformation.GetHandle();
        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => pinNumber;

        protected override void OpenPin(int pinNumber)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCount))
            {
                throw new ArgumentException($"Pin number can only be between 0 and {PinCount - 1}");
            }

            if (DeviceInformation.IsI2cMode && (
                (pinNumber >= 0) && (pinNumber <= 2)))
            {
                throw new ArgumentException($"Can't open pin 0, 1 or 2 while I2C mode is on");
            }

            if (DeviceInformation.IsSpiMode && (
                (pinNumber >= 0) && (pinNumber <= 2)))
            {
                throw new ArgumentException($"Can't open pin 0, 1 or 2 while SPI mode is on");
            }

            if (DeviceInformation._connectionSettings.Where(m=> m.ChipSelectLine == pinNumber).Any())
            {
                throw new ArgumentException($"Pin already open or used as Chip Select");
            }

            DeviceInformation._PinOpen[pinNumber] = true;
        }

        protected override void ClosePin(int pinNumber)
        {
            DeviceInformation._PinOpen[pinNumber] = false;
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (pinNumber < 8)
            {

                if (mode != PinMode.Output)
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << pinNumber));
                    DeviceInformation.GPIO_Low_Dir &= mask;
                }
                else
                {
                    DeviceInformation.GPIO_Low_Dir |= (byte)(1 << pinNumber);
                }

                DeviceInformation.SetGpioValuesLow();
            }
            else
            {
                if (mode != PinMode.Output)
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << (pinNumber - 8)));
                    DeviceInformation.GPIO_High_Dir &= mask;
                }
                else
                {
                    DeviceInformation.GPIO_High_Dir |= (byte)(1 << (pinNumber - 8));
                }

                DeviceInformation.SetGpioValuesHigh();
            }
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            if (pinNumber < 8)
            {
                return ((DeviceInformation.GPIO_Low_Dir >> pinNumber) & 0x01) == 0x01 ? PinMode.Output : PinMode.Input;
            }

            return ((DeviceInformation.GPIO_High_Dir >> (pinNumber - 8)) & 0x01) == 0x01 ? PinMode.Output : PinMode.Input;
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            if ((mode == PinMode.InputPullDown) || (mode == PinMode.InputPullUp))
            {
                return false;
            }

            return true;
        }

        protected override PinValue Read(int pinNumber)
        {
            if (pinNumber < 8)
            {
                var val = DeviceInformation.GetGpioValuesLow();
                return (((val >> pinNumber) & 0x01) == 0x01) ? PinValue.High : PinValue.Low;
            }
            else
            {
                var valhigh = DeviceInformation.GetGpioValuesHigh();
                return (((valhigh >> (pinNumber - 8)) & 0x01) == 0x01) ? PinValue.High : PinValue.Low;
            }
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            if (pinNumber < 8)
            {
                if (value == PinValue.High)
                {
                    DeviceInformation.GPIO_Low_Dat |= (byte)(1 << pinNumber);
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << pinNumber));
                    DeviceInformation.GPIO_Low_Dat &= mask;
                }

                DeviceInformation.SetGpioValuesLow();
            }
            else
            {
                if (value == PinValue.High)
                {
                    DeviceInformation.GPIO_High_Dat |= (byte)(1 << (pinNumber - 8));
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << (pinNumber - 8)));
                    DeviceInformation.GPIO_High_Dat &= mask;
                }

                DeviceInformation.SetGpioValuesHigh();
            }
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }
    }
}
