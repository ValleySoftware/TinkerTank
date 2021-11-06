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

        public II2cBus DistanceSensori2cBus;

        public static Byte[] distanceAddresses = new Byte[5] { 0x42, 0x43, 0x44, 0x4, 0x46 };

        public DistanceSensorController(F7Micro device, MeadowApp appRoot, ref II2cBus i2cbus)
        {
            Status = ComponentStatus.UnInitialised;

            DistanceSensori2cBus = i2cbus;

            _appRoot = appRoot;
            _device = device;
        }

        public void Init()
        {
            Status = ComponentStatus.Ready;
        }

        private List<Dist53l0> _items = new List<Dist53l0>();

        public List<Dist53l0> Items
        {
            get => _items;
            private set => _items = value;
        }

        public void RemoveDistanceSensor(Dist53l0 sensorToRemove)
        {
            Items.Remove(sensorToRemove);
        }

        public Dist53l0 InitNewSensor(IPin LaserPin, IPin XShutPin)
        {
            var sensor = new Dist53l0(MeadowApp.Device, _appRoot, this, LaserPin, XShutPin);
            sensor.Init();
            AddDistanceSensor(sensor);
            Byte newAddress = distanceAddresses[0];
            SetAddress(sensor, newAddress);
            sensor.BeginPolling();
            _appRoot.DebugDisplayText("dist sensor re addressed as " + newAddress, DisplayStatusMessageTypes.Debug);

            return sensor;
        }

        private void AddDistanceSensor(Dist53l0 sensorToAdd)
        {
            Items.Add(sensorToAdd);
        }

        public bool SetAddress(Dist53l0 sensorToChange, Byte newAddress, bool unShutAllAfterChange = true)
        {
            var result = false;

            if (sensorToChange != null)
            {

                _appRoot.DebugDisplayText("Shutting down all sensors ", DisplayStatusMessageTypes.Debug);
                foreach (var element in Items)
                {
                    element.ToggleXShut(true);
                }

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
