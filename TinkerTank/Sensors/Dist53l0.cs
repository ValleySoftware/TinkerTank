using Base;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using System;
using System.Threading;

namespace TinkerTank.Sensors
{
    public class Dist53l0 : BLESensorBase, ITinkerBase, ISensor
    {
        private Vl53l0x distanceSensor;
        public IPin LaserPin { get; set; }
        private IDigitalOutputPort _laserDigitaPort;
        public int UpdateIntervalInMS = 500;
        private II2cBus _bus;

        public Dist53l0(IPin laserPin, II2cBus bus, string name)
            :base(name)
        {
            LaserPin = laserPin;
            _bus = bus;
        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;
            
            try
            {
                _appRoot.DebugDisplayText("dist sensor init method started.", LogStatusMessageTypes.Debug);
                distanceSensor = new Vl53l0x(_device, _bus);

                LaserOn();

                _appRoot.DebugDisplayText("dist sensor init method complete, setting ready.", LogStatusMessageTypes.Debug);
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("dist " + Name + " - " + ex.Message, LogStatusMessageTypes.Error);
                Status = ComponentStatus.Error;
            }

        }

        public void BeginPolling()
        {
            distanceSensor.Updated += DistanceSensor_Updated;
            distanceSensor.StartUpdating(TimeSpan.FromMilliseconds(UpdateIntervalInMS));
            ErrorCount = 0;
        }        

        private void DistanceSensor_Updated(object sender, Meadow.IChangeResult<Meadow.Units.Length> e)
        {
            //LaserOn();

            if (e == null || 
                e.New == null ||
                e.New.Millimeters == -1
                )
            {
                ErrorCount++;
                _appRoot.DebugDisplayText("Error (null) reading distance from " + Name + " error no. " + ErrorCount, LogStatusMessageTypes.Error);
                return;
            }

            try
            {
                SensorValue = Convert.ToInt32(Math.Round(e.New.Millimeters));
                ErrorCount = 0;
            }
            catch
            {
                ErrorCount++;
                _appRoot.DebugDisplayText("Error reading distance from " + Name + " error no. " + ErrorCount, LogStatusMessageTypes.Error);
            }
        }

        public void LaserOn()
        {
            if (LaserPin != null)
            {
                if (_laserDigitaPort == null)
                {
                    _laserDigitaPort = _device.CreateDigitalOutputPort(LaserPin);
                }

                _laserDigitaPort.State = true;
            }
        }

        public void LaserOff()
        {
            if (LaserPin != null)
            {
                if (_laserDigitaPort == null)
                {
                    _laserDigitaPort = MeadowApp.Device.CreateDigitalOutputPort(LaserPin);
                }

                _laserDigitaPort.State = false;
            }
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
