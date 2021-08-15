using Base;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Servos;
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
        private Pca9685 pca9685;
        readonly int PWMFrequency = 50;
        private Servo CameraPan;
        private Servo CameraTilt;
        private Servo DistancePan;
        private Servo DistanceTilt;
        private List<Servo> servos = new List<Servo>();

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

            CameraPan = AddNewServoSG90(0);
            
            CameraTilt = AddNewServoSG90(1);

            DistancePan = AddNewServoSG90(2);

            DistanceTilt = AddNewServoSG90(3);

            Status = Enumerations.ComponentStatus.Ready;
        }

        public void RefreshStatus()
        {
            //
        }

        public Servo AddNewServoSG90(int portNo)
        {
            var newPort = pca9685.CreatePwmPort((byte)portNo);
            var newServo = new Servo(newPort, NamedServoConfigs.SG90);
            servos.Add(newServo);

            return newServo;
        }

        public void Test()
        {
            throw new NotImplementedException();
        }

        public bool ServoRotateTo(int deviceIndex, int newAngle)
        {
            var result = false;
            if (deviceIndex < servos.Count() && 
                deviceIndex >= 0 &&
                newAngle >= servos[deviceIndex].Config.MinimumAngle.Degrees &&
                newAngle <= servos[deviceIndex].Config.MaximumAngle.Degrees
                )
            {
                servos[deviceIndex].RotateTo(new Meadow.Units.Angle(newAngle, Meadow.Units.Angle.UnitType.Degrees));
                result = true;
            }

            return result;
        }


    }
}
