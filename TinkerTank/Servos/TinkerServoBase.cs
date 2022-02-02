using Base;
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
        private Angle? _defaultAngle;
        private Angle? _stowedAngle;
        private Angle? _readyAngle;
        private bool _reverseDirection;

        public static ServoConfig Create996rConfig(Angle? minAngle, Angle? maxAngle)
        {

            if (minAngle == null)
            {
                minAngle = new Angle(0, Angle.UnitType.Degrees);
            }

            if (maxAngle == null)
            {
                maxAngle = new Angle(245, Angle.UnitType.Degrees);
            }

            return new ServoConfig(
                    minAngle,
                    maxAngle,
                    500,
                    3000,
                    50);
        }

        public ServoBase servoDirectAccess
        {
            get => _servo;
        }

        public TinkerServoBase(PCA9685 servoControllerDevice, int portIndex, ServoType typeOfServo, Angle? minAngle, Angle? maxAngle, string name)
        {
            _appRoot = MeadowApp.Current;

            Status = ComponentStatus.UnInitialised;
            Name = name;
            _portIndex = portIndex;
            _servoType = typeOfServo;
            _servoControllerDevice = servoControllerDevice;
            _minAngle = minAngle;
            _maxAngle = maxAngle;
            _defaultAngle = new Angle(90, Angle.UnitType.Degrees);
        }

        public double AdjustRotationBy(double amountToRotate = 5)
        {
            var newDegrees = _servo.Angle.Value.Degrees + amountToRotate;
            var newAngle = new Angle(newDegrees, Angle.UnitType.Degrees);

            _servo.RotateTo(newAngle);

            return _servo.Angle.Value.Degrees;
        }

        public double SafeIshRotate(Angle? desiredAngle)
        {
            try
            {
                if (!desiredAngle.HasValue)
                {
                    _appRoot.DebugDisplayText(Name + " - safeishrotate requested without target angle.  Method exiting. ", LogStatusMessageTypes.Error);
                    return -1;
                }

                var NormalisedAngle = new Angle(Math.Abs(desiredAngle.GetValueOrDefault().Degrees), Angle.UnitType.Degrees);

                if (ReverseDirection)
                {
                    NormalisedAngle = new Angle(180 - NormalisedAngle.Degrees, Angle.UnitType.Degrees);
                }

                _appRoot.DebugDisplayText(Name + " - safeishrotate to " + NormalisedAngle.Degrees, LogStatusMessageTypes.Debug);
                Status = ComponentStatus.Action;
                _appRoot.DebugDisplayText("A");

                double result = -1;
                _appRoot.DebugDisplayText("B");

                if (NormalisedAngle.Degrees >= MinAngle.GetValueOrDefault().Degrees &&
                    NormalisedAngle.Degrees <= MaxAngle.GetValueOrDefault().Degrees)
                {
                    _appRoot.DebugDisplayText(Name + " - Rotating to " + NormalisedAngle.ToString(), LogStatusMessageTypes.Debug);
                    _appRoot.DebugDisplayText("C");
                    _servo.RotateTo(NormalisedAngle);
                }
                else
                {
                    _appRoot.DebugDisplayText("C");
                    _appRoot.DebugDisplayText(Name + " - angle is out of bounds.  No action taken. (" + NormalisedAngle.Degrees + ")", LogStatusMessageTypes.Error);
                }

                _appRoot.DebugDisplayText("D");
                result = _servo.Angle.GetValueOrDefault().Degrees;
                _appRoot.DebugDisplayText(Name + " - new angle is " + result, LogStatusMessageTypes.Debug);
                Status = ComponentStatus.Ready;

                return result;
            }
            catch (Exception e)
            {
                _appRoot.DebugDisplayText(Name + " - Error " + e.Message, LogStatusMessageTypes.Error);
            }

            return -1;
        }

        public void InitServo()
        {
            InitServo(false, false);
        }

        public void InitServo(bool GoToDefaultOnStart)
        {
            InitServo(GoToDefaultOnStart, false);
        }

        private void InitServo(bool GoToDefaultOnStart, bool ReverseServo)
        {
            ReverseDirection = ReverseServo;

            try
            {
                if (_servo != null)
                {
                    _servo.Stop();
                }
            }
            catch (Exception)
            {
                //01/feb/2022, for some reason the _servo != null line causes an exception to be raised ?? Very odd.
            }

            if (_minAngle == null &&
                _servoType != ServoType.MG996R)
            {
                _minAngle = new Angle(0, Angle.UnitType.Degrees);
            }

            if (_maxAngle == null &&
                _servoType != ServoType.MG996R)
            {
                _maxAngle = new Angle(245, Angle.UnitType.Degrees);
            }

            ServoConfig config = null;

            switch (_servoType)
            {
                case ServoType.SG90: config = NamedServoConfigs.SG90; break;
                case ServoType.SG90Continuous: config = NamedServoConfigs.IdealContinuousRotationServo; break;
                case ServoType.MG996R: config = Create996rConfig(_minAngle, _maxAngle); break;
                default: config = NamedServoConfigs.SG90; break;
            }

            _servo = new Servo(_servoControllerDevice.GetPwmPort(_portIndex), config);
            _appRoot.DebugDisplayText(Name + " (" + _servoType.ToString() + ") - instantiated on port " + _portIndex, LogStatusMessageTypes.Information);

            if (GoToDefaultOnStart)
            {
                GoToDefaultPosition();
            }

            _appRoot.DebugDisplayText(Name + " - ready", LogStatusMessageTypes.Debug);
            Status = ComponentStatus.Ready;
        }

        private void SetMinimum(Angle? newValue)
        {
            Status = ComponentStatus.UnInitialised;
            _minAngle = newValue;
            InitServo();
        }

        private void SetMaximum(Angle? newValue)
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

        public bool ReverseDirection
        {
            get => _reverseDirection;
            set { _reverseDirection = value; }
        }
        
        public Angle? StowedAngle
        {
            get => _stowedAngle;
            set { _stowedAngle = value; }
        }

        public Angle? ReadyAngle
        {
            get => _readyAngle;
            set { _readyAngle = value; }
        }

        public Angle? CurrentAngle
        {
            get
            {
                return _servo.Angle;
            }
        }

        public void GoToDefaultPosition()
        {
            _appRoot.DebugDisplayText(Name + " attempting to return to default position.", LogStatusMessageTypes.Debug);

            if (SafeIshRotate(DefaultAngle) == -1)
            {
                _appRoot.DebugDisplayText("Error going to default angle (" + Name + ") at " + Convert.ToString(_servo.Angle.Value) + " degrees.", LogStatusMessageTypes.Error);
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
