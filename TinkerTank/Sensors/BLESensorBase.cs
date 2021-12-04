using Base;
using Meadow.Devices;
using Meadow.Gateways.Bluetooth;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinkerTank.Sensors
{
    public class BLESensorBase : TinkerBase
    {
        private string _name;
        protected F7Micro device;
        private Characteristic _characteristic;
        private object _sensorValue;

        public BLESensorBase(F7Micro device, MeadowApp appRoot, string name)
        {
            _appRoot = appRoot;
            this.device = device;
            _name = name;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public bool AssignBluetoothCharacteristicToUpdate(Characteristic characteristicString)
        {
            _characteristic = characteristicString;

            return _characteristic != null;
        }

        public void UpdateBleValue(object newValue)
        {
            try
            {

                if (_characteristic != null)
                {
                    if (newValue == null)
                    {
                        newValue = SensorValue;
                    }

                    _characteristic.SetValue(Convert.ToString(newValue));
                    //_appRoot.DebugDisplayText("dist updated. " + newDistance, DisplayStatusMessageTypes.Important);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public object SensorValue
        {
            get => _sensorValue;
            set
            {
                _sensorValue = value;
                UpdateBleValue(value);
            }
        }
    }
}
