using Base;
using Enumerations;
using Meadow.Foundation.Servos;
using Servos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TinkerTank.Servos;

namespace TinkerTank.Movement
{
    public class ArmControl : TinkerBase, ITinkerBase
    {
        private List<TinkerServoBase> servos;
        private PCA9685 _servoControllerDevice;
        private TinkerServoBase _clawServo;
        private TinkerServoBase _wristServo;
        private TinkerServoBase _elbowServo;
        private TinkerServoBase _shoulderServo;
        private TinkerServoBase _basePanServo;
        private TinkerServoBase _baseTiltServo;

        public ArmControl(PCA9685 servoControllerDevice)
        {
            _appRoot = MeadowApp.Current;
            _servoControllerDevice = servoControllerDevice;
            Status = ComponentStatus.UnInitialised;
        }

        public void Init()
        {
            try
            {
                _appRoot.DebugDisplayText("Init Arm", LogStatusMessageTypes.Debug);

                if (servos == null)
                {
                    servos = new List<TinkerServoBase>();
                }

                servos.Clear();

                BasePanServo = new TinkerServoBase(_servoControllerDevice, 15, ServoType.MG996R, null, null, "Base Pan");//
                servos.Add(BasePanServo);
                BasePanServo.InitServo();

                BaseTiltServo = new TinkerServoBase(_servoControllerDevice, 7, ServoType.MG996R, null, null, "Base Tilt");//
                servos.Add(BaseTiltServo);
                BaseTiltServo.InitServo();

                ShoulderServo = new TinkerServoBase(_servoControllerDevice, 9, ServoType.MG996R, null, null, "Shoulder");//
                servos.Add(ShoulderServo);
                ShoulderServo.InitServo();

                ElbowServo = new TinkerServoBase(_servoControllerDevice, 8, ServoType.MG996R, null, null, "Elbow");//
                servos.Add(ElbowServo);
                ElbowServo.InitServo();

                WristServo = new TinkerServoBase(_servoControllerDevice, 10, ServoType.MG996R, null, null, "Wrist");
                servos.Add(WristServo);
                WristServo.InitServo();

                ClawServo = new TinkerServoBase(_servoControllerDevice, 6, ServoType.MG996R, null, null, "Claw");// higher = closed. Lower = open
                servos.Add(ClawServo);
                ClawServo.InitServo(true);

                Status = Enumerations.ComponentStatus.Ready;
                _appRoot.DebugDisplayText("arm - init complete", LogStatusMessageTypes.Information);
            }
            catch (Exception e)
            {
                _appRoot.DebugDisplayText("e - " + e.Message, Enumerations.LogStatusMessageTypes.Error);
                Status = ComponentStatus.Error;
            }
        }

        private TinkerServoBase ClawServo
        {
            get => _clawServo;
            set => _clawServo = value;
        }

        private TinkerServoBase WristServo
        {
            get => _wristServo;
            set => _wristServo = value;
        }

        private TinkerServoBase ElbowServo
        {
            get => _elbowServo;
            set => _elbowServo = value;
        }

        private TinkerServoBase ShoulderServo
        {
            get => _shoulderServo;
            set => _shoulderServo = value;
        }

        private TinkerServoBase BasePanServo
        {
            get => _basePanServo;
            set => _basePanServo = value;
        }

        private TinkerServoBase BaseTiltServo
        {
            get => _baseTiltServo;
            set => _baseTiltServo = value;
        }

        public void GoToDefaultPosition()
        {
            foreach (var element in servos)
            {
                element.GoToDefaultPosition();
            }
        }

        public void RefreshStatus()
        {

        }

        public void Test()
        {

        }

        public void ErrorEncountered()
        {

        }
    }
}
