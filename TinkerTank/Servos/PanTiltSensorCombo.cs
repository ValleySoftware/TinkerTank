using Enumerations;
using Meadow.Gateways.Bluetooth;
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

        private readonly bool _reversePan = true;
        private readonly bool _reverseTilt = true;

        public PanTiltSensorCombo(PCA9685 servoControllerDevice, string name) :
            base(servoControllerDevice, name)
        {
        }

        public void Init(int panPwmPort, int tiltPwmPort, Dist53l0 distanceSensor, ServoType servoType = ServoType.SG90)
        {
            base.Init(panPwmPort, tiltPwmPort, servoType);
            Sensor = distanceSensor;

            SetServoIsReversed(PanTiltServos.servoPan, _reversePan);
            SetServoIsReversed(PanTiltServos.servoTilt, _reverseTilt);
        }


        public override void ErrorEncountered()
        {
            base.ErrorEncountered();
        }

    }
}
