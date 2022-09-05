using Base;
using Enumerations;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TinkerTank.Sensors
{
    public class DistanceSensorController : TinkerBase, ITinkerBase
    {
        F7FeatherV1 _device;

        public Dist53l0 PeriscopeDistance;
        public Dist53l0 FixedFrontDistance;

        public Tca9548a _i2cExpander;

        public DistanceSensorController(Tca9548a i2cExpander)
        {
            _appRoot = MeadowApp.Current;
            _device = MeadowApp.Device;

            _appRoot.DebugDisplayText("Distance sensor controller Constructor", LogStatusMessageTypes.Information);
            Status = ComponentStatus.UnInitialised;

            _i2cExpander = i2cExpander;
        }

        public void Init()
        {
            try
            {
                _appRoot.DebugDisplayText("Distance sensor controller Init method", LogStatusMessageTypes.Information);
                Status = ComponentStatus.UnInitialised;

                PeriscopeDistance = InitNewSensor(
                    _device.Pins.D03,
                    I2CExpanderChannel.periscopeDistance,
                    "pan",
                    _appRoot.communications.charPanTiltDistance);

                FixedFrontDistance = InitNewSensor(
                    null,
                    I2CExpanderChannel.fixedForwardDistance,
                    "fwd",
                    _appRoot.communications.charForwardDistance);

                Status = ComponentStatus.Ready;
            }
            catch (Exception e)
            {
                Status = ComponentStatus.Error;
            }
        }

        public Dist53l0 InitNewSensor(IPin LaserPin, I2CExpanderChannel chan , string name, Characteristic charac)
        {
            var sensor = new Dist53l0(LaserPin, _appRoot.Geti2cBus(chan), name);
            sensor.AssignBluetoothCharacteristicToUpdate(charac);
            sensor.Init();

            sensor.BeginPolling();

            return sensor;
        }

        public void RefreshStatus()
        {

        }

        public void Test()
        {

        }

        public void ErrorEncountered()
        {

        }
    }
}
