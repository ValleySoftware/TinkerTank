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
        private II2cBus _bus;
        private string _name;


        public Dist53l0(F7Micro device, MeadowApp appRoot, DistanceSensorController controller, IPin laserPin, II2cBus bus, string name)
        {
            _appRoot = appRoot;
            _controller = controller;
            _device = device;
            LaserPin = laserPin;
            _bus = bus;
            _name = name;
        }

        public int DistanceInMillimeters
        {
            get => _distanceInMillimeters;
            set
            {
                _distanceInMillimeters = value;
                _appRoot.communications.RequestUpdateDistance(_distanceInMillimeters);
            }
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;
            
            try
            {
                _appRoot.DebugDisplayText("dist sensor init method started.", DisplayStatusMessageTypes.Debug);
                distanceSensor = new Vl53l0x(_device, _bus);

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
        
        public void ToggleXShut(bool newState)
        {
            distanceSensor.ShutDown(newState); //true = off/shutdown. false = on
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
            _appRoot.DebugDisplayText($"{Name} - {DistanceInMillimeters}mm", DisplayStatusMessageTypes.Debug);

            //LaserOff();
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
