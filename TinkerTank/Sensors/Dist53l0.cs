using Base;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using System;
using System.Threading;
using static Meadow.Foundation.Sensors.Distance.Vl53l0x;

namespace TinkerTank.Sensors
{
    public class Dist53l0 : BLESensorBase, ITinkerBase, ISensor
    {
        private Vl53l0x distanceSensor;
        private DistanceSensorController _controller;
        public IPin LaserPin { get; set; }
        private IDigitalOutputPort _laserDigitaPort;
        public int UpdateIntervalInMS = 500;
        private II2cBus _bus;


        public Dist53l0(F7Micro device, MeadowApp appRoot, DistanceSensorController controller, IPin laserPin, II2cBus bus, string name)
            :base(device, appRoot, name)
        {
            _controller = controller;
            LaserPin = laserPin;
            _bus = bus;

        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;
            
            try
            {
                _appRoot.DebugDisplayText("dist sensor init method started.", DisplayStatusMessageTypes.Debug);
                distanceSensor = new Vl53l0x(device, _bus);

                LaserOn();

                _appRoot.DebugDisplayText("dist sensor init method complete, setting ready.", DisplayStatusMessageTypes.Debug);
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("dist " + ex.Message, DisplayStatusMessageTypes.Error);
                Status = ComponentStatus.Error;
            }

        }

        public void BeginPolling()
        {
            distanceSensor.Updated += DistanceSensor_Updated;
            distanceSensor.StartUpdating(TimeSpan.FromMilliseconds(UpdateIntervalInMS));
        }        

        private void DistanceSensor_Updated(object sender, Meadow.IChangeResult<Meadow.Units.Length> e)
        {
            //LaserOn();

            if (e == null || 
                e.New == null ||
                e.New.Millimeters == -1
                )
            {
                return;
            }
            SensorValue = Convert.ToInt32(Math.Round(e.New.Millimeters));
            //_appRoot.DebugDisplayText($"{Name} - {DistanceInMillimeters}mm", DisplayStatusMessageTypes.Debug);

            //LaserOff();
        }

        public void LaserOn()
        {
            if (LaserPin != null)
            {
                if (_laserDigitaPort == null)
                {
                    _laserDigitaPort = device.CreateDigitalOutputPort(LaserPin);
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
