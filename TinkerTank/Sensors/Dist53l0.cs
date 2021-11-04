using Base;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using System;
using System.Threading;
using static Meadow.Foundation.Sensors.Distance.Vl53l0x;

namespace TinkerTank.Sensors
{
    public class Dist53l0 : TinkerBase, ITinkerBase, ISensor
    {
        private Vl53l0x distanceSensor;
        private DistanceSensorController _controller;
        int _distanceInMillimeters;
        F7Micro _device;
        public IPin LaserPin { get; set; }
        private IDigitalOutputPort laserDigitaPort;
        public int UpdateIntervalInMS = 250;
        private IPin XShutPin { get; set; }
        private IDigitalOutputPort xShutPort;


        public Dist53l0(F7Micro device, MeadowApp appRoot, ref DistanceSensorController controller, IPin laserPin, IPin xShutPin)
        {
            _appRoot = appRoot;
            _controller = controller;
            _device = device;
            LaserPin = laserPin;
            XShutPin = xShutPin;
        }

        public int DistanceInMillimeters
        {
            get => _distanceInMillimeters;
            set
            {
                _distanceInMillimeters = value;
                //SetProperty(ref _distanceInMillimeters, value);
                _appRoot.communications.RequestUpdateDistance(_distanceInMillimeters);
            }

        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;
            
            try
            {
                _appRoot.DebugDisplayText("dist sensor init method started.", DisplayStatusMessageTypes.Debug);
                distanceSensor = new Vl53l0x(_device, _controller.DistanceSensori2cBus, XShutPin);
                distanceSensor.Updated += DistanceSensor_Updated;
                distanceSensor.StartUpdating(TimeSpan.FromMilliseconds(UpdateIntervalInMS));

                _appRoot.DebugDisplayText("dist sensor init method complete, setting ready.", DisplayStatusMessageTypes.Debug);
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("dist " + ex.Message, DisplayStatusMessageTypes.Error);
                Status = ComponentStatus.Error;
            }

        }
        
        public void ToggleXShut(bool newState)
        {
            distanceSensor.ShutDown(newState);
        }
        
        public bool ChangeAddress(byte newAddress)
        {
            var result = false;

            ToggleXShut(true);
            ToggleXShut(false);
            Thread.Sleep(500);

            distanceSensor.SetAddress(newAddress);

            return result;
        }

        private void DistanceSensor_Updated(object sender, Meadow.IChangeResult<Meadow.Units.Length> e)
        {
            LaserOn();

            if (e == null || 
                e.New == null)
            {
                return;
            }
            DistanceInMillimeters = Convert.ToInt32(Math.Round(e.New.Millimeters));
            _appRoot.DebugDisplayText($"{DistanceInMillimeters}mm", DisplayStatusMessageTypes.Debug);

            LaserOff();
        }

        public void LaserOn()
        {
            if (LaserPin != null)
            {
                if (laserDigitaPort == null)
                {
                    laserDigitaPort = MeadowApp.Device.CreateDigitalOutputPort(LaserPin);
                }

                laserDigitaPort.State = true;
            }
        }

        public void LaserOff()
        {
            if (LaserPin != null)
            {
                if (laserDigitaPort == null)
                {
                    laserDigitaPort = MeadowApp.Device.CreateDigitalOutputPort(LaserPin);
                }

                laserDigitaPort.State = false;
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
