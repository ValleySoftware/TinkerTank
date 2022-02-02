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
        protected F7Micro _device;
        private Characteristic _characteristic;
        private object _sensorValue;
        private DateTimeOffset _lastBleUpdate;
        private TimeSpan _minimumInterval;

        public BLESensorBase(string name)
        {
            _appRoot = MeadowApp.Current;
            _device = MeadowApp.Device;
            Name = name;
            _minimumInterval = TimeSpan.FromSeconds(1);
        }

        public bool AssignBluetoothCharacteristicToUpdate(Characteristic characteristicString)
        {
            _characteristic = characteristicString;
            _lastBleUpdate = DateTimeOffset.Now;

            return _characteristic != null;
        }

        public void UpdateBleValue(object newValue)
        {
            if (DateTimeOffset.Now.Subtract(_lastBleUpdate) > _minimumInterval)
            {
                try
                {
                    if (_characteristic != null)
                    {
                        if (newValue == null)
                        {
                            newValue = SensorValue;
                        }

                        _lastBleUpdate = DateTimeOffset.Now;

                        _appRoot.communications.UpdateCharacteristicValue(_characteristic, Convert.ToString(newValue));                        
                    }
                }
                catch (Exception)
                {

                }
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
