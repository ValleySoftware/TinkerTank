﻿using Base;
using Meadow.Foundation.Servos;
using Meadow.Units;
using Servos;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinkerTank.Servos
{
    public class TinkerServoBase : TinkerBase, ITinkerBase
    {

        public enum ServoType { SG90, SG90Continuous, MG996R };
        private PCA9685 _servoControllerDevice;
        private Servo _servo;
        private ServoType _servoType;
        private int _portIndex;
        private Angle? _minAngle;
        private Angle? _maxAngle;
        private string _name;
        private Angle _defaultAngle;

        public static ServoConfig Create996rConfig(Angle? minAngle, Angle? maxAngle)
        {

            if (minAngle == null)
            {
                minAngle = new Angle(0, Angle.UnitType.Degrees);
            }

            if (maxAngle == null)
            {
                maxAngle = new Angle(270, Angle.UnitType.Degrees);
            }

            return new ServoConfig(
                    minAngle,
                    maxAngle,
                    500,
                    2500,
                    50);
        }

        public TinkerServoBase(MeadowApp appRoot, PCA9685 servoControllerDevice, int portIndex, ServoType typeOfServo, Angle? minAngle, Angle? maxAngle, string name)
        {
            _appRoot = appRoot;

            Status = Enumerations.ComponentStatus.UnInitialised;
            _name = name;
            _portIndex = portIndex;
            _servoType = typeOfServo;
            _servoControllerDevice = servoControllerDevice;
            _minAngle = minAngle;
            _maxAngle = maxAngle;
            _defaultAngle = new Angle(90, Angle.UnitType.Degrees);
        }

        public int SafeIshRotate(Angle desiredAngle)
        {
            Status = Enumerations.ComponentStatus.Action;

            int result = -1;

            var oldAngle = _servo.Angle;

            _servo.RotateTo(desiredAngle);

            if (desiredAngle.Equals(_servo.Angle))
            {
                _servo.Stop();
                Status = Enumerations.ComponentStatus.Error;
                result = -1;

                if (desiredAngle.Degrees < _servo.Angle.Value.Degrees)
                {
                    SetNewMinimum(_servo.Angle);
                }
                else
                {
                    SetNewMaximum(_servo.Angle);
                }
            }
            else
            {
                result = ((int)_servo.Angle.Value.Degrees);
            }

            Status = Enumerations.ComponentStatus.Ready;
            return result;
        }

        public void InitServo(bool GoToDefaultOnStart = false)
        {
            _appRoot.DebugDisplayText("arm - 0");
            if (_servo != null)
            {
                _servo.Stop();
            }

            _appRoot.DebugDisplayText("arm - 1");
            if (_minAngle == null)
            {
                _minAngle = new Angle(0, Angle.UnitType.Degrees);
            }

            _appRoot.DebugDisplayText("arm - 2");
            if (_maxAngle == null)
            {
                _maxAngle = new Angle(180, Angle.UnitType.Degrees);
            }

            _appRoot.DebugDisplayText("arm - 2a");
            ServoConfig config = null;

            _appRoot.DebugDisplayText("arm - 3");
            switch (_servoType)
            {
                case ServoType.SG90: config = NamedServoConfigs.SG90; break;
                case ServoType.SG90Continuous: config = NamedServoConfigs.IdealContinuousRotationServo; break;
                case ServoType.MG996R: _maxAngle = new Angle(270, Angle.UnitType.Degrees); config = Create996rConfig(_minAngle, _maxAngle); break;
                default: config = NamedServoConfigs.SG90; break;
            }

            _appRoot.DebugDisplayText("arm - 4");
            _servo = new Servo(_servoControllerDevice.GetPwmPort(_portIndex), config);

            _appRoot.DebugDisplayText("arm - 5");
            if (GoToDefaultOnStart)
            {
                GoToDefaultPosition();
            }
            _appRoot.DebugDisplayText("arm - 6");

            Status = Enumerations.ComponentStatus.Ready;
        }

        private void SetNewMinimum(Angle? newValue)
        {
            Status = Enumerations.ComponentStatus.UnInitialised;
            _minAngle = newValue;
            InitServo();
        }

        private void SetNewMaximum(Angle? newValue)
        {
            Status = Enumerations.ComponentStatus.UnInitialised;
            _maxAngle = newValue;
            InitServo();
        }

        public Angle DefaultAngle
        {
            get => _defaultAngle;
            set => SetProperty(ref _defaultAngle, value);
        }

        private string Name 
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public void GoToDefaultPosition()
        {
            if (SafeIshRotate(DefaultAngle) == -1)
            {
                _appRoot.DebugDisplayText("Error going to default angle (" + Name + ") at " + Convert.ToString(_servo.Angle.Value) + " degrees.");
                DefaultAngle = _servo.Angle.Value;
            }
        }

        public void RefreshStatus()
        {
            
        }

        public void Test()
        {
            
        }
    }
}
