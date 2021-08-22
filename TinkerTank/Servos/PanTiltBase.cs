using Base;
using Enumerations;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;
using TinkerTank;

namespace Servos
{
    public class PanTiltBase: TinkerBase, ITinkerBase
    {
        private IPwmPort PwmPan;
        private IPwmPort PwmTilt;
        private ServoType _servoType;

        private Servo servoPan;
        private Servo servoTilt;
        private string _name;

        private double _defaultPan = 90;
        private double _defaultTilt = 90;

        //private bool stopRequested = false;

        public PanTiltBase(MeadowApp appRoot, IPwmPort panPwmPort, IPwmPort tiltPwmPort, string name, ServoType servoType = ServoType.SG90Standard)
        {
            _appRoot = appRoot;
            PwmPan = panPwmPort;
            PwmTilt = tiltPwmPort;
            _servoType = servoType;
            _name = name;
        }

        public static ServoConfig Create996rConfig()
        {
            return new ServoConfig(
                    new Meadow.Units.Angle(0, Meadow.Units.Angle.UnitType.Degrees),
                    new Meadow.Units.Angle(270, Meadow.Units.Angle.UnitType.Degrees),
                    500,
                    2500,
                    50);
        }

        public void Init()
        {
            ServoConfig conf = null;

            switch (_servoType)
            {
                case ServoType.SG90Standard: conf = NamedServoConfigs.SG90; break;
                case ServoType.MG996RStandard: conf = Create996rConfig(); break;
                default: conf = NamedServoConfigs.SG90; break;
            }

            servoPan = new Servo(PwmPan, conf);
            servoTilt = new Servo(PwmTilt, conf);

            _appRoot.DebugDisplayText("Pantilt " + Name + " registered and ready");
            Status = ComponentStatus.Ready;
        }

        public Meadow.Units.Angle? CurrentPanPosition
        {
            get
            {
                return servoPan.Angle;
            }
        }

        public Meadow.Units.Angle? CurrentTiltPosition
        {
            get
            {
                return servoTilt.Angle;
            }
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public double DefaultPan
        {
            get => _defaultPan;
            set => SetProperty(ref _defaultPan, value);
        }

        public double DefaultTilt
        {
            get => _defaultTilt;
            set => SetProperty(ref _defaultTilt, value);
        }

        public void PanTo(double newAngle = 90, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {
            _appRoot.DebugDisplayText("Pan requested");

            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {
                var t = new Task(() =>
                {
                    Status = ComponentStatus.Action;
                    _appRoot.DebugDisplayText("Pan Task Running");
                    if (movementSpeed == ServoMovementSpeed.Flank)
                    {
                        ServoRotateTo(servoPan, newAngle);
                    }
                    else
                    {
                        int millisecondDelay = 0;

                        switch (movementSpeed)
                        {
                            case ServoMovementSpeed.Slow: millisecondDelay = 500; break;
                            case ServoMovementSpeed.Medium: millisecondDelay = 250; break;
                            case ServoMovementSpeed.Fast: millisecondDelay = 125; break;
                            default: millisecondDelay = 250; break;
                        }

                        var newPos = CurrentPanPosition.Value.Degrees;

                        _appRoot.DebugDisplayText("Pan from " + newPos + " to " + newAngle + " with " + millisecondDelay);

                        if (newPos > newAngle)
                        {

                            while (newPos > newAngle)
                            {
                                newPos--;
                                _appRoot.DebugDisplayText("Pan Decrease Step to " + newPos);
                                ServoRotateTo(servoPan, newPos);
                                Thread.Sleep(millisecondDelay);
                            }
                        }
                        else
                        {
                            while (newPos < newAngle)
                            {
                                newPos++;
                                _appRoot.DebugDisplayText("Pan Increase Step to " + newPos);
                                ServoRotateTo(servoPan, newPos);
                                Thread.Sleep(millisecondDelay);
                            }
                        }
                    }
                    Status = ComponentStatus.Ready;
                });

                t.Start();
            }
        }

        public void TiltTo(double newAngle = 90, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {
            _appRoot.DebugDisplayText("Tilt requested");

            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {

                var t = new Task(() =>
                {
                    Status = ComponentStatus.Action;
                    _appRoot.DebugDisplayText("Tilt Task Running");
                    ServoRotateTo(servoTilt, newAngle);
                    Status = ComponentStatus.Ready;
                });

                t.Start();
            }
        }

        public void RefreshStatus()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            throw new NotImplementedException();
        }

        private void ServoRotateTo(Servo servoToRotate, double newAngle)
        {
            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {
                if (servoToRotate != null &&
                newAngle >= servoToRotate.Config.MinimumAngle.Degrees &&
                newAngle <= servoToRotate.Config.MaximumAngle.Degrees
                )
                {
                    servoToRotate.RotateTo(new Meadow.Units.Angle(newAngle, Meadow.Units.Angle.UnitType.Degrees));
                }
            }
        }

        public void GoToDefault()
        {
            var t = new Task(() =>
            {
                PanTo(DefaultPan);
                TiltTo(DefaultTilt);
            });

            t.Start();

            }
    }
}
