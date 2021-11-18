using Base;
using Enumerations;
using Meadow.Devices;
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

        public Dist53l0 FrontDistance;
        public Dist53l0 RearDistance;
        public Dist53l0 PeriscopeDistance;

        public II2cBus DistanceSensori2cBus;

        public static Byte[] distanceAddresses = new Byte[5] { 0x42, 0x43, 0x44, 0x4, 0x46 };

        public DistanceSensorController(F7Micro device, MeadowApp appRoot, II2cBus i2cbus)
        {
            Status = ComponentStatus.UnInitialised;

            DistanceSensori2cBus = i2cbus;

            _appRoot = appRoot;
            _device = device;
        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;

            _appRoot.DebugDisplayText("Init sensors");

            //InitNewSensor(null, _device.Pins.D03, DistanceSensorLocation.front);
            //InitNewSensor(null, _device.Pins.D04, DistanceSensorLocation.rear);
            InitNewSensor(_device.Pins.D01, _device.Pins.D00, DistanceSensorLocation.periscope);

            Status = ComponentStatus.Ready;
        }

        private Dist53l0 InitNewSensor(IPin LaserPin, IPin XShutPin, DistanceSensorLocation sensorLocation)
        {
            var sensor = new Dist53l0(MeadowApp.Device, _appRoot, this, LaserPin, XShutPin);
            sensor.Init();

            Byte newAddress = Convert.ToByte(82);

            switch (sensorLocation)
            {
                case DistanceSensorLocation.front: FrontDistance = sensor; newAddress = distanceAddresses[0]; break;
                case DistanceSensorLocation.rear: RearDistance = sensor; newAddress = distanceAddresses[1]; break;
                case DistanceSensorLocation.periscope: PeriscopeDistance = sensor; newAddress = distanceAddresses[2]; break;
            }
             
            //SetAddress(sensor, newAddress);

            sensor.BeginPolling();

            return sensor;
        }

        public bool SetAddress(Dist53l0 sensorToChange, Byte newAddress, bool unShutAllAfterChange = true)
        {
            var result = false;

            if (sensorToChange != null)
            {

                _appRoot.DebugDisplayText("Shutting down all sensors ", DisplayStatusMessageTypes.Debug);
                
                if (FrontDistance != null) { FrontDistance.ToggleXShut(true); };
                if (RearDistance != null) { FrontDistance.ToggleXShut(true); };
                if (PeriscopeDistance != null) { FrontDistance.ToggleXShut(true); };

                Thread.Sleep(500);

                _appRoot.DebugDisplayText("Attempting address change " + newAddress, DisplayStatusMessageTypes.Debug);
                sensorToChange.ToggleXShut(false);

                Thread.Sleep(500);

                result = sensorToChange.ChangeAddress(newAddress);
                _appRoot.DebugDisplayText("Address change result = " + result, DisplayStatusMessageTypes.Debug);

                //sensorToChange.ToggleXShut(false);
                /*if (unShutAllAfterChange)
                {
                    _appRoot.DebugDisplayText("re-enabling all sensors " + distanceAddresses[Items.Count - 1], DisplayStatusMessageTypes.Debug);

                    foreach (var element in Items)
                    {
                            element.ToggleXShut(false);
                    }
                }*/

            }

            return result;
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
