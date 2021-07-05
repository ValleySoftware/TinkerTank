using Base;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkerTank;

namespace Servos
{
    public class PCA9685 : TinkerBase, ITinkerBase
    {
        public Pca9685 pca9685;
        readonly int PWMFrequency = 50;

        public PCA9685(MeadowApp appRoot)
        {
            _appRoot = appRoot;
        }

        public void Init()
        {
            _appRoot.DebugDisplayText("Init PCA9685 Device");
            //Servo control using an intermediary IC
            var i2CBus = MeadowApp.Device.CreateI2cBus(I2cBusSpeed.FastPlus); //i2c buss is D07 for data, d08 for clock
            pca9685 = new Pca9685(i2CBus, 0x40, PWMFrequency);
            pca9685.Initialize();
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
