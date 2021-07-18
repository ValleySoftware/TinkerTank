using Base;
using Meadow.Foundation.Servos;
//using Meadow.Foundation.Servos;
using System;
using TinkerTank;

namespace Servos
{
    public class PanTiltBase: TinkerBase, ITinkerBase
    {
        Meadow.Hardware.IPwmPort Pca9685Servo0;
        Meadow.Hardware.IPwmPort Pca9685Servo1;

        public Servo servo0;
        public Servo servo1;


        public PanTiltBase(MeadowApp appRoot)
        {
            _appRoot = appRoot;
        }

        public void Init()
        {
            Pca9685Servo0 = _appRoot.i2CPWMController.pca9685.CreatePwmPort(0, 0.05f);
            var servoConfig = NamedServoConfigs.SG90;
            servo0 = new Servo(Pca9685Servo0, servoConfig);
            Pca9685Servo1 = _appRoot.i2CPWMController.pca9685.CreatePwmPort(1, 0.05f);
            servo1 = new Servo(Pca9685Servo1, servoConfig);
        }

        public void PanLeft(int newAngle = 90)
        {
            servo0.RotateTo(new Meadow.Units.Angle(newAngle));
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
