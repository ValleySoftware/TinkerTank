using Base;
using Enumerations;
using Meadow.Foundation.Servos;
using Servos;
using System;
using System.Collections.Generic;
using System.Text;
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

        public ArmControl(MeadowApp appRoot, PCA9685 servoControllerDevice)
        {
            _appRoot = appRoot;
            _servoControllerDevice = servoControllerDevice;
            Status = Enumerations.ComponentStatus.UnInitialised;
        }

        public void Init()
        {
            try
            {
                _appRoot.DebugDisplayText("Init Arm");

                if (servos == null)
                {
                    servos = new List<TinkerServoBase>();
                }

                servos.Clear();

                //_appRoot.DebugDisplayText("arm - base pan");
                BasePanServo = new TinkerServoBase(_appRoot, _servoControllerDevice, 6, ServoType.MG996R, null, null, "Base Pan");
                servos.Add(BasePanServo);
                BasePanServo.InitServo();

                //_appRoot.DebugDisplayText("arm - base tilt");
                BaseTiltServo = new TinkerServoBase(_appRoot, _servoControllerDevice, 7, ServoType.MG996R, null, null, "Base Tilt");
                servos.Add(BaseTiltServo);
                BaseTiltServo.InitServo();

                //_appRoot.DebugDisplayText("arm - Shoulder");
                ShoulderServo = new TinkerServoBase(_appRoot, _servoControllerDevice, 8, ServoType.MG996R, null, null, "Shoulder");
                servos.Add(ShoulderServo);
                ShoulderServo.InitServo();

                //_appRoot.DebugDisplayText("arm - Elbow");
                ElbowServo = new TinkerServoBase(_appRoot, _servoControllerDevice, 9, ServoType.MG996R, null, null, "Elbow");
                servos.Add(ElbowServo);
                ElbowServo.InitServo();

                //_appRoot.DebugDisplayText("arm - Wrist");
                WristServo = new TinkerServoBase(_appRoot, _servoControllerDevice, 10, ServoType.MG996R, null, null, "Wrist");
                servos.Add(WristServo);
                WristServo.InitServo();

                ClawServo = new TinkerServoBase(_appRoot, _servoControllerDevice, 15, ServoType.MG996R, null, null, "Claw");
                servos.Add(ClawServo);
                ClawServo.InitServo();
                //ClawServo.SafeIshRotate(new Meadow.Units.Angle(60, Meadow.Units.Angle.UnitType.Degrees));

                //ClawServo.DefaultAngle = new Meadow.Units.Angle(50, Meadow.Units.Angle.UnitType.Degrees);

                Status = Enumerations.ComponentStatus.Ready;
                _appRoot.DebugDisplayText("arm - init complete");
            }
            catch (Exception e)
            {
                _appRoot.DebugDisplayText("e - " + e.Message, Enumerations.DisplayStatusMessageTypes.Error);
                Status = Enumerations.ComponentStatus.Error;
            }
        }

        private TinkerServoBase ClawServo
        {
            get => _clawServo;
            set => SetProperty(ref _clawServo, value);
        }

        private TinkerServoBase WristServo
        {
            get => _wristServo;
            set => SetProperty(ref _wristServo, value);
        }

        private TinkerServoBase ElbowServo
        {
            get => _elbowServo;
            set => SetProperty(ref _elbowServo, value);
        }

        private TinkerServoBase ShoulderServo
        {
            get => _shoulderServo;
            set => SetProperty(ref _shoulderServo, value);
        }

        private TinkerServoBase BasePanServo
        {
            get => _basePanServo;
            set => SetProperty(ref _basePanServo, value);
        }

        private TinkerServoBase BaseTiltServo
        {
            get => _baseTiltServo;
            set => SetProperty(ref _baseTiltServo, value);
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
