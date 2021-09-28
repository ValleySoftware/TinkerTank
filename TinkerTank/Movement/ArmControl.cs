using Base;
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
            BasePanServo = new TinkerServoBase(_servoControllerDevice, 10, TinkerServoBase.ServoType.MG996R, null, null);
            BaseTiltServo = new TinkerServoBase(_servoControllerDevice, 11, TinkerServoBase.ServoType.MG996R, null, null);
            ShoulderServo = new TinkerServoBase(_servoControllerDevice, 12, TinkerServoBase.ServoType.MG996R, null, null);
            ElbowServo = new TinkerServoBase(_servoControllerDevice, 13, TinkerServoBase.ServoType.MG996R, null, null);
            WristServo = new TinkerServoBase(_servoControllerDevice, 14, TinkerServoBase.ServoType.MG996R, null, null);
            ClawServo = new TinkerServoBase(_servoControllerDevice, 15, TinkerServoBase.ServoType.MG996R, null, null);
            Status = Enumerations.ComponentStatus.Ready;
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

        public void RefreshStatus()
        {

        }

        public void Test()
        {

        }
    }
}
