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
    public class Dist53l0 : BLESensorBase, ITinkerBase, IValleySensor
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

                _appRoot.DebugDisplayText("dist sensor init method complete, setting ready.", LogStatusMessageTypes.Important);
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
            if (Status == ComponentStatus.Ready)
            {
                distanceSensor.Updated += DistanceSensor_Updated;
                distanceSensor.StartUpdating(TimeSpan.FromMilliseconds(UpdateIntervalInMS));
                ErrorCount = 0;
            }
        }   
        
        public void StopPolling()
        {
            if (distanceSensor != null)
            {
                distanceSensor.StopUpdating();
            }
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
                if (ErrorCount == 1)
                {
                    _appRoot.DebugDisplayText("Error (null) reading distance from " + Name + " error no. " + ErrorCount, LogStatusMessageTypes.Debug);
                }
                else
                {
                    _appRoot.DebugDisplayText("Error (null) reading distance from " + Name + " error no. " + ErrorCount, LogStatusMessageTypes.Error);
                }

                if (ErrorCount >= 5)
                {
                    _appRoot.DebugDisplayText("dist sensor " + Name + " has hit error limit.  Disabling now. " , LogStatusMessageTypes.Error);
                    Status = ComponentStatus.Error;
                    StopPolling();
                }
                return;
            }

            try
            {
                var r = Math.Round(e.New.Millimeters);
                SensorValue = Convert.ToInt32(r);
                ErrorCount = 0;
            }
            catch (Exception ex)
            {
                ErrorCount++;
                _appRoot.DebugDisplayText("Error reading distance from " + Name + " error no. " + ErrorCount + ex.Message, LogStatusMessageTypes.Error);
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
