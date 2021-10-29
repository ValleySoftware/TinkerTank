﻿using Base;
using Enumerations;
using Meadow.Foundation.Servos;
using Meadow.Units;
using Servos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TinkerTank.Servos
{
    public class TinkerServoBase : TinkerBase, ITinkerBase
    {

        private PCA9685 _servoControllerDevice;
        private Servo _servo;
        private ServoType _servoType;
        private int _portIndex;
        private Angle? _minAngle;
        private Angle? _maxAngle;
        private string _name;
        private Angle? _defaultAngle;

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

            Status = ComponentStatus.UnInitialised;
            _name = name;
            _portIndex = portIndex;
            _servoType = typeOfServo;
            _servoControllerDevice = servoControllerDevice;
            _minAngle = minAngle;
            _maxAngle = maxAngle;
            _defaultAngle = new Angle(90, Angle.UnitType.Degrees);
        }

        public double SafeIshRotate(Angle? desiredAngle)
        {
            _appRoot.DebugDisplayText(Name + " - safeishrotate to " + desiredAngle.Value.Degrees, DisplayStatusMessageTypes.Debug);
            Status = ComponentStatus.Action;

            double result = -1;

            _appRoot.DebugDisplayText(Name + " - safeishrotate prep", DisplayStatusMessageTypes.Debug);

            bool Completed = false;
            _appRoot.DebugDisplayText(Name + " - safeishrotate servo not null = " + (_servo != null), DisplayStatusMessageTypes.Important);

            //var oldAngle = _servo.Angle;
            //_appRoot.DebugDisplayText(Name + " - safeishrotate old angle = " + oldAngle.Value.Degrees, DisplayStatusMessageTypes.Debug);

            //Try changing this to Thread rather than Task???            
            //var rotateTask = Task.Run(() =>
            //{
                _appRoot.DebugDisplayText(Name + " - safeishrotate action thread started", DisplayStatusMessageTypes.Debug);
                _servo.RotateTo(desiredAngle.Value);
            //});

            /*
            var monitorTask = Task.Run(async () =>
            {
                _appRoot.DebugDisplayText(Name + " - safeishrotate monitor thread started", DisplayStatusMessageTypes.Debug);

                while (!Completed)
                {
                    await Task.Delay(250);

                    if (desiredAngle.Equals(_servo.Angle))
                    {
                        Completed = true;
                        _servo.Stop();
                        result = _servo.Angle.Value.Degrees;
                        Status = ComponentStatus.Ready;
                        _appRoot.DebugDisplayText(Name + " - safeishrotate success", DisplayStatusMessageTypes.Important);
                    }

                    if (oldAngle == _servo.Angle)
                    {
                        Completed = true;
                        _servo.Stop();
                        _appRoot.DebugDisplayText(Name + " - safeishrotate incomplete", DisplayStatusMessageTypes.Important);
                        //Status = Enumerations.ComponentStatus.Error;
                        result = -1;

                        if (desiredAngle.Value.Degrees < _servo.Angle.Value.Degrees)
                        {
                            SetNewMinimum(_servo.Angle);
                            _appRoot.DebugDisplayText(Name + " - new min = " + _servo.Angle.Value.Degrees, DisplayStatusMessageTypes.Important);
                        }
                        else
                        {
                            SetNewMaximum(_servo.Angle);
                            _appRoot.DebugDisplayText(Name + " - new max = " + _servo.Angle.Value.Degrees, DisplayStatusMessageTypes.Important);
                        }

                    }

                    oldAngle = _servo.Angle;
                }
            });
                     */

            result = _servo.Angle.Value.Degrees;
            Status = ComponentStatus.Ready;

            return result;
        }

        public void InitServo(bool GoToDefaultOnStart = false)
        {
            if (_servo != null)
            {
                _servo.Stop();
            }

            if (_minAngle == null)
            {
                _minAngle = new Angle(0, Angle.UnitType.Degrees);
            }

            if (_maxAngle == null)
            {
                _maxAngle = new Angle(180, Angle.UnitType.Degrees);
            }

            ServoConfig config = null;

            switch (_servoType)
            {
                case ServoType.SG90: config = NamedServoConfigs.SG90; break;
                case ServoType.SG90Continuous: config = NamedServoConfigs.IdealContinuousRotationServo; break;
                case ServoType.MG996R: _maxAngle = new Angle(270, Angle.UnitType.Degrees); config = Create996rConfig(_minAngle, _maxAngle); break;
                default: config = NamedServoConfigs.SG90; break;
            }

            _servo = new Servo(_servoControllerDevice.GetPwmPort(_portIndex), config);
            _appRoot.DebugDisplayText(Name + " - instantiated on port " + _portIndex, Enumerations.DisplayStatusMessageTypes.Important);

            if (GoToDefaultOnStart)
            {
                GoToDefaultPosition();
            }

            _appRoot.DebugDisplayText(Name + " - ready");
            Status = ComponentStatus.Ready;
        }

        private void SetNewMinimum(Angle? newValue)
        {
            Status = ComponentStatus.UnInitialised;
            _minAngle = newValue;
            InitServo();
        }

        private void SetNewMaximum(Angle? newValue)
        {
            Status = ComponentStatus.UnInitialised;
            _maxAngle = newValue;
            InitServo();
        }

        public Angle? MinAngle
        {
            get => _minAngle;
        }

        public Angle? MaxAngle
        {
            get => _maxAngle;
        }

        public Angle? DefaultAngle
        {
            get => _defaultAngle;
            set { _defaultAngle = value; }
        }

        public Angle? CurrentAngle
        {
            get
            {
                return _servo.Angle;
            }
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

        public void ErrorEncountered()
        {

        }
    }
}
