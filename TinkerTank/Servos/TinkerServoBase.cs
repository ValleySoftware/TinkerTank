using Base;
using Meadow.Foundation.Servos;
using Meadow.Units;
using Servos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        public double SafeIshRotate(Angle desiredAngle)
        {
            _appRoot.DebugDisplayText(Name + " - safeishrotate to " + desiredAngle.Degrees);
            Status = Enumerations.ComponentStatus.Action;

            double result = -1;

            bool Completed = false;

            var oldAngle = _servo.Angle;

            var rotateTask = Task.Run(() =>
            {
                _servo.RotateTo(desiredAngle);
            });

            var monitorTask = Task.Run(async () =>
            {
                while (!Completed)
                {
                    await Task.Delay(250);

                    if (desiredAngle.Equals(_servo.Angle))
                    {
                        Completed = true;
                        _servo.Stop();
                        //Status = Enumerations.ComponentStatus.Error;
                        result = _servo.Angle.Value.Degrees;
                        Status = Enumerations.ComponentStatus.Ready;
                        _appRoot.DebugDisplayText(Name + " - safeishrotate success");
                    }

                    if (oldAngle == _servo.Angle)
                    {
                        Completed = true;
                        _servo.Stop();
                        _appRoot.DebugDisplayText(Name + " - safeishrotate incomplete");
                        //Status = Enumerations.ComponentStatus.Error;
                        result = -1;

                        if (desiredAngle.Degrees < _servo.Angle.Value.Degrees)
                        {
                            SetNewMinimum(_servo.Angle);
                            _appRoot.DebugDisplayText(Name + " - new min = " + _servo.Angle.Value.Degrees);
                        }
                        else
                        {
                            SetNewMaximum(_servo.Angle);
                            _appRoot.DebugDisplayText(Name + " - new max = " + _servo.Angle.Value.Degrees);
                        }

                    }

                    oldAngle = _servo.Angle;
                }
            });

            

            result = _servo.Angle.Value.Degrees;
            Status = Enumerations.ComponentStatus.Ready;

            return result;
        }

        public void InitServo(bool GoToDefaultOnStart = false)
        {
            //_appRoot.DebugDisplayText(Name + " - 0");
            if (_servo != null)
            {
                _servo.Stop();
            }

            //_appRoot.DebugDisplayText(Name + " - 1");
            if (_minAngle == null)
            {
                _minAngle = new Angle(0, Angle.UnitType.Degrees);
            }

            //_appRoot.DebugDisplayText(Name + " - 2");
            if (_maxAngle == null)
            {
                _maxAngle = new Angle(180, Angle.UnitType.Degrees);
            }

            //_appRoot.DebugDisplayText(Name + " - 2a");
            ServoConfig config = null;

            //_appRoot.DebugDisplayText(Name + " - 3");
            switch (_servoType)
            {
                case ServoType.SG90: config = NamedServoConfigs.SG90; break;
                case ServoType.SG90Continuous: config = NamedServoConfigs.IdealContinuousRotationServo; break;
                case ServoType.MG996R: _maxAngle = new Angle(270, Angle.UnitType.Degrees); config = Create996rConfig(_minAngle, _maxAngle); break;
                default: config = NamedServoConfigs.SG90; break;
            }

            //_appRoot.DebugDisplayText(Name + " - 4");
            _servo = new Servo(_servoControllerDevice.GetPwmPort(_portIndex), config);

            //_appRoot.DebugDisplayText(Name + " - 5");
            if (GoToDefaultOnStart)
            {
                GoToDefaultPosition();
            }
            //_appRoot.DebugDisplayText(Name + " - 6");

            _appRoot.DebugDisplayText(Name + " - ready");
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

        public async void GoToDefaultPosition()
        {
            if (await SafeIshRotate(DefaultAngle) == -1)
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
