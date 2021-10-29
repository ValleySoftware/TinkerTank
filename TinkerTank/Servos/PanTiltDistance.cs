using Enumerations;
using Meadow.Hardware;
using Servos;
using System;
using System.Collections.Generic;
using System.Text;
using TinkerTank.Sensors;

namespace TinkerTank.Servos
{
    public class PanTiltDistance : PanTiltBase
    {
        public Dist53l0 Sensor { get; set; }

        IPin LaserPin;
        II2cBus sharedBus;

        public PanTiltDistance(MeadowApp appRoot, PCA9685 servoControllerDevice, string name, ref II2cBus sharedi2cBus, IPin laserPin = null) :
            base(appRoot, servoControllerDevice, name)
        {
            LaserPin = laserPin;
            sharedBus = sharedi2cBus;
        }

        public override void Init(int panPwmPort, int tiltPwmPort, ServoType servoType = ServoType.SG90)
        {
            base.Init(panPwmPort, tiltPwmPort, servoType);

            _appRoot.DebugDisplayText("Init distance sensor.");
            Sensor = new Dist53l0(MeadowApp.Device, _appRoot, ref sharedBus, LaserPin);
            Sensor.Init();
        }


        public override void ErrorEncountered()
        {
            base.ErrorEncountered();
        }

    }
}
