using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Common;
using Iot.Device.Ft232H;
using Iot.Device.FtCommon;
using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Threading;
using UnitsNet;
using System.Device.Gpio;

namespace samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello FT232H");
            var devices = FtCommon.GetDevices();
            Console.WriteLine($"{devices.Count} available device(s)");
            foreach (var device in devices)
            {
                Console.WriteLine($"  {device.Description}");
                Console.WriteLine($"    Flags: {device.Flags}");
                Console.WriteLine($"    Id: {device.Id}");
                Console.WriteLine($"    LocId: {device.LocId}");
                Console.WriteLine($"    Serial number: {device.SerialNumber}");
                Console.WriteLine($"    Type: {device.Type}");
            }

            if (devices.Count == 0)
            {
                Console.WriteLine("No device connected");
                return;
            }

            TestSpi(devices);
        }

        static void TestSpi(List<FtDevice> devices)
        {
            SpiConnectionSettings settings = new(0, 3) { ClockFrequency = 1_000_000, DataBitLength = 8, ChipSelectLineActiveState = PinValue.Low };
            var spi = new Ft232HSpi(settings, new Ft232HDevice(devices[0]));
            Span<byte> toSend = stackalloc byte[10] { 0x12, 0x42, 0xFF, 0x00, 0x23, 0x98, 0x87, 0x65, 0x21, 0x34 };
            Span<byte> toRead = stackalloc byte[10];
            spi.TransferFullDuplex(toSend, toRead);
            for (int i = 0; i < toRead.Length; i++)
            {
                Console.Write($"{toRead[i]:X2} ");
            }

            Console.WriteLine();
        }

        static void TestI2c(List<FtDevice> devices)
        {
            // set this to the current sea level pressure in the area for correct altitude readings
            Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;
            Length stationHeight = Length.FromMeters(640); // Elevation of the sensor
            var ftI2cBus = new Ft232HDevice(devices[0]).CreateI2cBus();
            var i2cDevice = ftI2cBus.CreateDevice(Bmp280.SecondaryI2cAddress);
            using var i2CBmp280 = new Bmp280(i2cDevice);

            while (true)
            {
                // set higher sampling
                i2CBmp280.TemperatureSampling = Sampling.LowPower;
                i2CBmp280.PressureSampling = Sampling.UltraHighResolution;

                // Perform a synchronous measurement
                var readResult = i2CBmp280.Read();

                // Print out the measured data
                Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
                Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");

                // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
                // double altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                i2CBmp280.TryReadAltitude(out var altValue);

                Console.WriteLine($"Calculated Altitude: {altValue.Meters:0.##}m");
                Thread.Sleep(1000);

                // change sampling rate
                i2CBmp280.TemperatureSampling = Sampling.UltraHighResolution;
                i2CBmp280.PressureSampling = Sampling.UltraLowPower;
                i2CBmp280.FilterMode = Bmx280FilteringMode.X4;

                // Perform an asynchronous measurement
                readResult = i2CBmp280.ReadAsync().GetAwaiter().GetResult();

                // Print out the measured data
                Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
                Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");

                // This time use altitude calculation
                if (readResult.Temperature != null && readResult.Pressure != null)
                {
                    altValue = WeatherHelper.CalculateAltitude((Pressure)readResult.Pressure, defaultSeaLevelPressure, (Temperature)readResult.Temperature);
                    Console.WriteLine($"Calculated Altitude: {altValue.Meters:0.##}m");
                }

                // Calculate the barometric (corrected) pressure for the local position.
                // Change the stationHeight value above to get a correct reading, but do not be tempted to insert
                // the value obtained from the formula above. Since that estimates the altitude based on pressure,
                // using that altitude to correct the pressure won't work.
                if (readResult.Temperature != null && readResult.Pressure != null)
                {
                    var correctedPressure = WeatherHelper.CalculateBarometricPressure((Pressure)readResult.Pressure, (Temperature)readResult.Temperature, stationHeight);
                    Console.WriteLine($"Pressure corrected for altitude {stationHeight:F0}m (with average humidity): {correctedPressure.Hectopascals:0.##} hPa");
                }

                Thread.Sleep(5000);
            }
        }
    }
}
