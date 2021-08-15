﻿using Base;
using Enumerations;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinkerTank.Sensors
{
    public class VL6180xDistance : TinkerBase, ITinkerBase
    {
        Vl53l0x sensor;

        public VL6180xDistance(MeadowApp appRoot)
        {
            _appRoot = appRoot;
        }

        public ComponentStatus Init()
        {
            Status = ComponentStatus.UnInitialised;

            try
            {

                var i2cBus = MeadowApp.Device.CreateI2cBus(I2cBusSpeed.Fast, (int)0x29);
                sensor = new Vl53l0x(MeadowApp.Device, i2cBus);

                sensor.Updated += Sensor_Updated; 
                sensor.StartUpdating();
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("Vl53l0x Error " + ex.Message, DisplayStatusMessageTypes.Error, true);
            }

            return Status;
        }

        private void Sensor_Updated(object sender, Meadow.IChangeResult<Meadow.Units.Length> e)
        {
            if (e.New == null)
            {
                return;
            }

            Console.WriteLine($"{e.New.Centimeters}mm");
        }

        public void RefreshStatus()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            throw new NotImplementedException();
        }
    }
}