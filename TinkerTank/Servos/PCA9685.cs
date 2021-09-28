using Base;
using Enumerations;
using Meadow;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkerTank;
using TinkerTank.Servos;

namespace Servos
{
    public class PCA9685 : TinkerBase, ITinkerBase
    {
        private Pca9685 pca9685;
        II2cBus Sharedi2cBus;

        public PCA9685(MeadowApp appRoot, ref II2cBus sharedi2cBus)
        {
            _appRoot = appRoot;
            Sharedi2cBus = sharedi2cBus;
        }

        public void Init()
        {
            _appRoot.DebugDisplayText("Init PCA9685 Device");
            
            pca9685 = new Pca9685(Sharedi2cBus, 0x40, _appRoot.PWMFrequency);
            pca9685.Initialize(); 

            Status = ComponentStatus.Ready;
        }

        public IPwmPort GetPwmPort(int portIndex)
        {
            return pca9685.CreatePwmPort(Convert.ToByte(portIndex));
        }

        public void RefreshStatus()
        {
            //
        }

        public void Test()
        {
        }


    }
}
