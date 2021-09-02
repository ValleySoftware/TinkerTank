using Base;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using static Meadow.Foundation.Sensors.Distance.Vl53l0x;

namespace TinkerTank.Sensors
{
    public class Dist53l0 : TinkerBase, ITinkerBase, ISensor
    {
        private Vl53l0x distanceSensor;
        II2cBus sharedBus;
        double _distanceInMillimeters;
        F7Micro _device;

        public Dist53l0(F7Micro device, MeadowApp appRoot, ref II2cBus i2cBus)
        {
            _appRoot = appRoot;
            sharedBus = i2cBus;
            _device = device;
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
                distanceSensor.StartUpdating(new TimeSpan(0,0,0,0,500));

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
            if (e.New == null)
            {
                return;
            }
            DistanceInMillimeters = e.New.Millimeters;
            Console.WriteLine($"{e.New.Millimeters}mm");
        }

        public void RefreshStatus()
        {

        }

        public void Test()
        {

        }
    }
}
