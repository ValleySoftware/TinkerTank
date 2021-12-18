using Enumerations;
using Meadow.Hardware;
using Servos;
using System;
using System.Collections.Generic;
using System.Text;
using TinkerTank.Sensors;

namespace TinkerTank.Servos
{
    public class PanTiltSensorCombo : PanTiltBase
    {
        public Dist53l0 Sensor { get; set; }

        public PanTiltSensorCombo(MeadowApp appRoot, PCA9685 servoControllerDevice, string name) :
            base(appRoot, servoControllerDevice, name)
        {
        }

        public void Init(int panPwmPort, int tiltPwmPort, Dist53l0 distanceSensor, ServoType servoType = ServoType.SG90)
        {
            base.Init(panPwmPort, tiltPwmPort, servoType);
            Sensor = distanceSensor; 
        }


        public override void ErrorEncountered()
        {
            base.ErrorEncountered();
        }

    }
}
