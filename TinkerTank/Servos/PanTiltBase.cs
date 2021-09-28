using Base;
using Enumerations;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;
using TinkerTank;
using Meadow.Units;
using TinkerTank.Servos;

namespace Servos
{
    public partial class PanTiltBase: TinkerBase, ITinkerBase
    {
        private IPwmPort PwmPan;
        private IPwmPort PwmTilt;
        private ServoType _servoType;

        private Servo servoPan;
        private Servo servoTilt;
        private string _name;

        private int _defaultPan = 90;
        private int _defaultTilt = 90;

        bool _stopRequested = false;

        public PanTiltBase(MeadowApp appRoot, IPwmPort panPwmPort, IPwmPort tiltPwmPort, string name, ServoType servoType = ServoType.SG90Standard)
        {
            _appRoot = appRoot;
            PwmPan = panPwmPort;
            PwmTilt = tiltPwmPort;
            _servoType = servoType;
            _name = name;
        }

        public bool StopRequested
        {
            get => _stopRequested;
            set => SetProperty(ref _stopRequested, value);
        }

        public virtual void Init()
        {
            ServoConfig conf = null;

            switch (_servoType)
            {
                case ServoType.SG90Standard: conf = NamedServoConfigs.SG90; break;
                case ServoType.MG996RStandard: conf = TinkerServoBase.Create996rConfig(null, null); break;
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

        public int DefaultPan
        {
            get => _defaultPan;
            set => SetProperty(ref _defaultPan, value);
        }

        public int DefaultTilt
        {
            get => _defaultTilt;
            set => SetProperty(ref _defaultTilt, value);
        }

        public Task PanTo(int newAngle = 90, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {
            _appRoot.DebugDisplayText("Pan requested");

            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {
                StopRequested = false;

                var t = Task.Run(() =>
                {
                    Status = ComponentStatus.Action;
                    _appRoot.DebugDisplayText("Pan Task Running");

                    if (movementSpeed == ServoMovementSpeed.Stop)
                    {
                        StopRequested = true;
                    }
                    else
                    {
                        if (movementSpeed == ServoMovementSpeed.Flank)
                        {
                            _appRoot.DebugDisplayText("Pan speed flank");
                            ServoRotateTo(servoPan, newAngle);
                            _appRoot.DebugDisplayText("Pan at flank finished");

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

                            int newPos = (int)Math.Round(CurrentPanPosition.Value.Degrees);

                            _appRoot.DebugDisplayText("Pan from " + newPos + " to " + newAngle + " with " + millisecondDelay);

                            if (newPos > newAngle)
                            {

                                while (newPos > newAngle)
                                {
                                    newPos = newPos - 1;
                                    _appRoot.DebugDisplayText("Pan Decrease Step to " + newPos);
                                    ServoRotateTo(servoPan, newPos);
                                    Thread.Sleep(millisecondDelay);

                                    if (StopRequested)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                while (newPos < newAngle)
                                {
                                    newPos = newPos + 1;
                                    _appRoot.DebugDisplayText("Pan Increase Step to " + newPos);
                                    ServoRotateTo(servoPan, newPos);
                                    Task.Delay(millisecondDelay);

                                    if (StopRequested)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    Status = ComponentStatus.Ready;
                });

                return t;
            }
            return null;
        }

        public void TiltTo(int newAngle = 90, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {
            _appRoot.DebugDisplayText("Tilt requested");

            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {

                var t = Task.Run(() =>
                {
                    Status = ComponentStatus.Action;
                    _appRoot.DebugDisplayText("Tilt Task Running");
                    ServoRotateTo(servoTilt, newAngle);
                    Status = ComponentStatus.Ready;
                });
            }
        }

        public void RefreshStatus()
        {
        }

        public void Test()
        {
        }

        private void ServoRotateTo(Servo servoToRotate, int newAngle)
        {
            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {
                if (servoToRotate != null &&
                    newAngle >= servoToRotate.Config.MinimumAngle.Degrees &&
                    newAngle <= servoToRotate.Config.MaximumAngle.Degrees)
                {
                    //var t = Task.Run(() =>
                    //{
                        servoToRotate.RotateTo(new Meadow.Units.Angle(newAngle, Meadow.Units.Angle.UnitType.Degrees));                
                    //});
                    //return t;
                }

            }
            //return null;
        }

        public void AutoPanSweep(ServoMovementSpeed speed)
        {
            Task.Run(() =>
            {
                if (speed == ServoMovementSpeed.Stop)
                {
                    StopRequested = true;
                    return;
                }

                StopRequested = false;

                while (!StopRequested)
                {

                    _appRoot.DebugDisplayText("Pan to 25");

                    var t = PanTo(25, speed);
                    t.Wait();


                    if (StopRequested)
                    {
                        break;
                    }
                    _appRoot.DebugDisplayText("Pan to 160");

                    t = PanTo(160, speed);
                    t.Wait();

                    if (StopRequested)
                    {
                        break;
                    }
                }
            });

        }

        public void GoToDefault()
        {
            var t = Task.Run(() =>
            { 
                PanTo(DefaultPan);
                TiltTo(DefaultTilt);
            });
        }
    }
}
