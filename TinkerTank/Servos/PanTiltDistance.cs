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
        IPin XShutPin;
        II2cBus sharedBus;
        DistanceSensorController _controller;

        public PanTiltDistance(MeadowApp appRoot, PCA9685 servoControllerDevice, string name, ref DistanceSensorController controller, IPin laserPin = null, IPin xShutPin = null) :
            base(appRoot, servoControllerDevice, name)
        {
            LaserPin = laserPin;
            _controller = controller;
            XShutPin = xShutPin;
        }

        public override void Init(int panPwmPort, int tiltPwmPort, ServoType servoType = ServoType.SG90)
        {
            base.Init(panPwmPort, tiltPwmPort, servoType);

            _appRoot.DebugDisplayText("Init distance sensor.");
            Sensor = new Dist53l0(MeadowApp.Device, _appRoot, ref _controller, LaserPin, XShutPin);
            Sensor.Init();
            _controller.AddDistanceSensor(Sensor);
        }


        public override void ErrorEncountered()
        {
            base.ErrorEncountered();
        }

    }
}
