using Base;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinkerTank.Sensors
{
    public class DistanceSensorController : TinkerBase, ITinkerBase
    {
        private IPin _sclPin;
        private IPin _sdaPin;
        F7Micro _device;

        public II2cBus DistanceSensori2cBus;

        public DistanceSensorController(F7Micro device, MeadowApp appRoot, II2cBus i2cbus)
        {
            DistanceSensori2cBus = i2cbus;

            _appRoot = appRoot;
            _device = device;
        }

        public void Init()
        {

        }

        private List<Dist53l0> _items;

        private List<Dist53l0> Items
        {
            get => _items;
            set => _items = value;
        }

        public void RemoveDistanceSensor(Dist53l0 sensorToRemove)
        {
            Items.Remove(sensorToRemove);
        }

        public void AddDistanceSensor(Dist53l0 sensorToAdd)
        {
            Items.Remove(sensorToAdd);
        }

        public bool SetAddress(Dist53l0 sensorToChange, byte newAddress, bool unShutAllAfterChange = true)
        {
            var result = false;

            if (sensorToChange != null)
            {

                foreach (var element in Items)
                {
                    element.ToggleXShut(true);
                }

                result = sensorToChange.ChangeAddress(newAddress);

                if (unShutAllAfterChange)
                {
                    foreach (var element in Items)
                    {
                        if (element != sensorToChange)
                        {
                            element.ToggleXShut(false);
                        }
                    }
                }

            }

            return result;
        }

        public void RefreshStatus()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            throw new NotImplementedException();
        }

        public void ErrorEncountered()
        {
            throw new NotImplementedException();
        }
    }
}
