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
        F7Micro _device;

        public Dist53l0 PeriscopeDistance;
        public Dist53l0 FixedFrontDistance;

        public Tca9548a _i2cExpander;

        public DistanceSensorController(F7Micro device, MeadowApp appRoot, Tca9548a i2cExpander)
        {
            appRoot.DebugDisplayText("Distance sensor controller Constructor");
            Status = ComponentStatus.UnInitialised;

            _i2cExpander = i2cExpander;

            _appRoot = appRoot;
            _device = device;
        }

        public void Init()
        {
            _appRoot.DebugDisplayText("Distance sensor controller Init method");
            Status = ComponentStatus.UnInitialised;

            PeriscopeDistance = InitNewSensor(_device.Pins.D01, _appRoot.Geti2cBus(DistanceSensorLocation.periscopeDistance), "pan", _appRoot.communications.charPanTiltDistance);
            FixedFrontDistance = InitNewSensor(null, _appRoot.Geti2cBus(DistanceSensorLocation.fixedForwardDistance), "fwd", _appRoot.communications.charForwardDistance);

            Status = ComponentStatus.Ready;
        }

        public Dist53l0 InitNewSensor(IPin LaserPin, II2cBus bus, string name, Characteristic charac)
        {
            var sensor = new Dist53l0(MeadowApp.Device, _appRoot, this, LaserPin, bus, name);
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
