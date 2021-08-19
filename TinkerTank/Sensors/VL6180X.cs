using System;
using System.Collections.Generic;
using System.Text;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Foundation;
using Meadow;
using System.Threading.Tasks;
using System.Threading;

namespace TinkerTank.Sensors
{

    public class VL6180X : ByteCommsSensorBase<Length>, IRangeFinder
    {
        //==== public properties and such
        public enum UnitType
        {
            mm,
            cm,
            inches
        }

        public const byte DefaultI2cAddress = 0x29;

        public bool IsShutdown
        {
            get
            {
                if (shutdownPort != null)
                {
                    return !shutdownPort.State;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates a distance sensor object with the specified i2c port.
        /// </summary>
        /// <param name="lengthUnit"></param>
        /// <param name="i2cBus"></param>
        /// <param name="address"></param>
        public VL6180X(
            IDigitalOutputController device,
            Length lengthUnit, 
            I2cBus i2cBus, 
            byte address = DefaultI2cAddress)
                : this(device, i2cBus, null, address)
        {

        }

        /// <param name="i2cBus">I2C bus</param>
        /// <param name="address">VL6180X address</param>
        /// <param name="units">Unit of measure</param>
        public VL6180X(
            IDigitalOutputController device, 
            II2cBus i2cBus, 
            IPin shutdownPin,
            byte address = DefaultI2cAddress)
                : base(i2cBus, address)
        {
            if (shutdownPin != null)
            {
                device.CreateDigitalOutputPort(shutdownPin, true);
            }
            Initialize().Wait();
        }

        /// <summary>
        /// The distance to the measured object.
        /// </summary>
        public Length? Distance { get; protected set; } = new Length(0);

        /// <summary>
        /// Minimum valid distance in mm.
        /// </summary>
        public Length MinimumDistance => new Length(30, Length.UnitType.Millimeters);

        /// <summary>
        /// Maximum valid distance in mm (CurrentDistance returns -1 if above).
        /// </summary>
        public Length MaximumDistance => new Length(2000, Length.UnitType.Millimeters);

        readonly IDigitalOutputPort shutdownPort;

        byte stopVariable;

        public event EventHandler<IChangeResult<Length>> DistanceUpdated;

        protected override Task<Length> ReadSensor()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the VL6180X
        /// </summary>
        protected async Task Initialize()
        {
            if (IsShutdown)
            {
                await ShutDown(false);
            }

            if (Read(0xC0) != 0xEE || Read(0xC1) != 0xAA || Read(0xC2) != 0x10)
            {
                throw new Exception("Failed to find expected ID register values");
            }

            Peripheral.WriteRegister(0x88, 0x00);
            Peripheral.WriteRegister(0x80, 0x01);
            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(0x00, 0x00);

            stopVariable = Read(0x91);

            Peripheral.WriteRegister(0x00, 0x01);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(0x80, 0x00);

            var configControl = ((byte)(Read(MsrcConfigControl) | 0x12));
            var signalRateLimit = 0.25f;

            Peripheral.WriteRegister(SystemSequenceConfig, 0xFF);
            var spadInfo = GetSpadInfo();
            int spadCount = spadInfo.Item1;
            bool spad_is_aperture = spadInfo.Item2;

            byte[] ref_spad_map = new byte[7];
            ref_spad_map[0] = GlobalConfigSpadEnablesRef0;

            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(DynamicSpadRefEnStartOffset, 0x00);
            Peripheral.WriteRegister(DynamicSpadNumRequestedRefSpad, 0x2C);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(GlobalConfigRefEnStartSelect, 0xB4);

            var first_spad_to_enable = (spad_is_aperture) ? 12 : 0;
            var spads_enabled = 0;

            for (int i = 0; i < 48; i++)
            {
                if (i < first_spad_to_enable || spads_enabled == spadCount)
                {
                    ref_spad_map[1 + (i / 8)] &= (byte)~(1 << (i % 8));
                }
                else if ((ref_spad_map[1 + (i / 8)] >> (byte)((i % 8)) & 0x1) > 0)
                {
                    spads_enabled += 1;
                }
            }

            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(0x00, 0x00);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(0x09, 0x00);
            Peripheral.WriteRegister(0x10, 0x00);
            Peripheral.WriteRegister(0x11, 0x00);
            Peripheral.WriteRegister(0x24, 0x01);
            Peripheral.WriteRegister(0x25, 0xFF);
            Peripheral.WriteRegister(0x75, 0x00);
            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(0x4E, 0x2C);
            Peripheral.WriteRegister(0x48, 0x00);
            Peripheral.WriteRegister(0x30, 0x20);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(0x30, 0x09);
            Peripheral.WriteRegister(0x54, 0x00);
            Peripheral.WriteRegister(0x31, 0x04);
            Peripheral.WriteRegister(0x32, 0x03);
            Peripheral.WriteRegister(0x40, 0x83);
            Peripheral.WriteRegister(0x46, 0x25);
            Peripheral.WriteRegister(0x60, 0x00);
            Peripheral.WriteRegister(0x27, 0x00);
            Peripheral.WriteRegister(0x50, 0x06);
            Peripheral.WriteRegister(0x51, 0x00);
            Peripheral.WriteRegister(0x52, 0x96);
            Peripheral.WriteRegister(0x56, 0x08);
            Peripheral.WriteRegister(0x57, 0x30);
            Peripheral.WriteRegister(0x61, 0x00);
            Peripheral.WriteRegister(0x62, 0x00);
            Peripheral.WriteRegister(0x64, 0x00);
            Peripheral.WriteRegister(0x65, 0x00);
            Peripheral.WriteRegister(0x66, 0xA0);
            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(0x22, 0x32);
            Peripheral.WriteRegister(0x47, 0x14);
            Peripheral.WriteRegister(0x49, 0xFF);
            Peripheral.WriteRegister(0x4A, 0x00);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(0x7A, 0x0A);
            Peripheral.WriteRegister(0x7B, 0x00);
            Peripheral.WriteRegister(0x78, 0x21);
            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(0x23, 0x34);
            Peripheral.WriteRegister(0x42, 0x00);
            Peripheral.WriteRegister(0x44, 0xFF);
            Peripheral.WriteRegister(0x45, 0x26);
            Peripheral.WriteRegister(0x46, 0x05);
            Peripheral.WriteRegister(0x40, 0x40);
            Peripheral.WriteRegister(0x0E, 0x06);
            Peripheral.WriteRegister(0x20, 0x1A);
            Peripheral.WriteRegister(0x43, 0x40);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(0x34, 0x03);
            Peripheral.WriteRegister(0x35, 0x44);
            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(0x31, 0x04);
            Peripheral.WriteRegister(0x4B, 0x09);
            Peripheral.WriteRegister(0x4C, 0x05);
            Peripheral.WriteRegister(0x4D, 0x04);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(0x44, 0x00);
            Peripheral.WriteRegister(0x45, 0x20);
            Peripheral.WriteRegister(0x47, 0x08);
            Peripheral.WriteRegister(0x48, 0x28);
            Peripheral.WriteRegister(0x67, 0x00);
            Peripheral.WriteRegister(0x70, 0x04);
            Peripheral.WriteRegister(0x71, 0x01);
            Peripheral.WriteRegister(0x72, 0xFE);
            Peripheral.WriteRegister(0x76, 0x00);
            Peripheral.WriteRegister(0x77, 0x00);
            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(0x0D, 0x01);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(0x80, 0x01);
            Peripheral.WriteRegister(0x01, 0xF8);
            Peripheral.WriteRegister(0xFF, 0x01);
            Peripheral.WriteRegister(0x8E, 0x01);
            Peripheral.WriteRegister(0x00, 0x01);
            Peripheral.WriteRegister(0xFF, 0x00);
            Peripheral.WriteRegister(0x80, 0x00);

            Peripheral.WriteRegister(SystemInterruptConfigGpio, 0x04);
            var gpio_hv_mux_active_high = Read(GpioHvMuxActiveHigh);
            Peripheral.WriteRegister(GpioHvMuxActiveHigh, (byte)(gpio_hv_mux_active_high & ~0x10));

            Peripheral.WriteRegister(GpioHvMuxActiveHigh, 0x01);
            Peripheral.WriteRegister(SystemSequenceConfig, 0xE8);

            Peripheral.WriteRegister(SystemSequenceConfig, 0x01);
            PerformSingleRefCalibration(0x40);
            Peripheral.WriteRegister(SystemSequenceConfig, 0x02);
            PerformSingleRefCalibration(0x00);

            Peripheral.WriteRegister(SystemSequenceConfig, 0xE8);
        }

        protected byte Read(byte address)
        {
            var result = Peripheral.ReadRegister(address);
            return result;
        }

        protected int Read16(byte address)
        {
            //var result = Peripheral.ReadRegisters(address, 2);
            Peripheral.ReadRegister(address, ReadBuffer.Span[0..2]);
            return (ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1];
        }

        /// <summary>
        /// Set the Shutdown state of the device
        /// </summary>
        /// <param name="state">true = off/shutdown. false = on</param>
        public async Task ShutDown(bool state)
        {
            if (shutdownPort == null)
            {
                return;
            }

            shutdownPort.State = !state;
            await Task.Delay(2).ConfigureAwait(false);

            if (state == false)
            {
                await Initialize();
                // TODO: is this still needed? the previous line wasn't awaited before.
                await Task.Delay(2).ConfigureAwait(false);
            }
        }
    }
}