using Base;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using System;
using static Meadow.Foundation.Sensors.Distance.Vl53l0x;

namespace TinkerTank.Sensors
{
    public class Dist53l0 : TinkerBase, ITinkerBase, ISensor
    {
        private Vl53l0x distanceSensor;
        II2cBus sharedBus;
        double _distanceInMillimeters;
        F7Micro _device;
        public IPin LaserPin { get; set; }
        private IDigitalOutputPort laserDigitaPort;


        public Dist53l0(F7Micro device, MeadowApp appRoot, ref II2cBus i2cBus, IPin laserPin)
        {
            _appRoot = appRoot;
            sharedBus = i2cBus;
            _device = device;
            LaserPin = laserPin;
        }

        public double DistanceInMillimeters
        {
            get => _distanceInMillimeters;
            set => SetProperty(ref _distanceInMillimeters, value); 
        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;
            
            try
            {
                distanceSensor = new Vl53l0x(_device, sharedBus);
                distanceSensor.Updated += DistanceSensor_Updated;
                //distanceSensor.StartUpdating(TimeSpan.FromMilliseconds(250));
                distanceSensor.StartUpdating(TimeSpan.FromMilliseconds(2000));

                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("dist " + ex.Message, DisplayStatusMessageTypes.Error, true);
                Status = ComponentStatus.Error;
            }

        }

        private void DistanceSensor_Updated(object sender, Meadow.IChangeResult<Meadow.Units.Length> e)
        {
            LaserOn();

            if (e == null || 
                e.New == null)
            {
                return;
            }
            DistanceInMillimeters = e.New.Millimeters;
            _appRoot.DebugDisplayText($"{DistanceInMillimeters}mm", DisplayStatusMessageTypes.Important);

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
    }
}
