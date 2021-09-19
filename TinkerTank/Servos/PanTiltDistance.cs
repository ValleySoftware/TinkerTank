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

        public  PanTiltDistance(MeadowApp appRoot, IPwmPort panPwmPort, IPwmPort tiltPwmPort, string name, ServoType servoType = ServoType.SG90Standard, IPin laserPin = null) : 
            base(appRoot, panPwmPort, tiltPwmPort, name,  ServoType.SG90Standard)
        {
            LaserPin = laserPin;
        }

        public override void Init() 
        {
            base.Init();

            Sensor = new Dist53l0(MeadowApp.Device, _appRoot, ref _appRoot.pcaBus, LaserPin);
            Sensor.Init();
        }


    }
}
