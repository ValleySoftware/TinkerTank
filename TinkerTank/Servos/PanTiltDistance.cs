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

        private IDigitalOutputPort laserDigitaPort;
        IPin LaserPin;

        public PanTiltDistance(MeadowApp appRoot, PCA9685 servoControllerDevice, string name, IPin laserPin = null) :
            base(appRoot, servoControllerDevice, name)
        {
            LaserPin = laserPin;
        }

        public override void Init(int panPwmPort, int tiltPwmPort, ServoType servoType = ServoType.SG90)
        {
            base.Init(panPwmPort, tiltPwmPort, servoType);

            _appRoot.DebugDisplayText("Init distance sensor.");
            Sensor = new Dist53l0(MeadowApp.Device, _appRoot, ref _appRoot.i2CBus, LaserPin);
            Sensor.Init();
        }


        public void ErrorEncountered()
        {

        }

    }
}
