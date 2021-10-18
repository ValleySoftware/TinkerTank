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
        private TinkerServoBase servoPan;
        private TinkerServoBase servoTilt;
        private string _name;
        protected PCA9685 _servoControllerDevice;

        bool _stopRequested = false;

        public PanTiltBase(MeadowApp appRoot, PCA9685 servoControllerDevice, string name)
        {
            _appRoot = appRoot;
            _servoControllerDevice = servoControllerDevice;
            _name = name;
        }

        public bool StopRequested
        {
            get => _stopRequested;
            set => SetProperty(ref _stopRequested, value);
        }

        public virtual void Init(int panPwmPort, int tiltPwmPort, ServoType servoType = ServoType.SG90)
        {
            servoPan = new TinkerServoBase(_appRoot, _servoControllerDevice, panPwmPort, servoType, null, null, "Pan");

            _appRoot.DebugDisplayText("Pantilt " + Name + " registered and ready");
            Status = ComponentStatus.Ready;
        }

        public Angle? CurrentPanPosition
        {
            get
            {
                return servoPan.CurrentAngle;
            }
        }

        public Angle? CurrentTiltPosition
        {
            get
            {
                return servoTilt.CurrentAngle;
            }
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Angle? DefaultPan
        {
            get => servoPan.DefaultAngle;
            set
            {
                servoPan.DefaultAngle = value;
            }
        }

        public Angle? DefaultTilt
        {
            get => servoTilt.DefaultAngle;
            set
            {
                servoTilt.DefaultAngle = value;                
            }
        }

        public Task PanTo(Angle? newAngle, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {
            if (newAngle == null)
            {
                newAngle = new Angle(90);
            }
            
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
                            servoPan.SafeIshRotate(newAngle);
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

                            Angle? newPos = new Angle(CurrentPanPosition.Value.Degrees);

                            _appRoot.DebugDisplayText("Pan from " + newPos.Value.Degrees + " to " + newAngle.Value.Degrees + " with " + millisecondDelay);

                            if (newPos.Value.Degrees > newAngle.Value.Degrees)
                            {

                                while (newPos.Value.Degrees > newAngle.Value.Degrees)
                                {
                                    newPos = new Angle(newPos.Value.Degrees - 1);
                                    servoPan.SafeIshRotate(newPos);
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
                                    newPos = new Angle(newPos.Value.Degrees + 1);
                                    servoPan.SafeIshRotate(newPos);
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

        public void TiltTo(Angle? newAngle, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {
            if (newAngle == null)
            {
                newAngle = new Angle(90);
            }

            _appRoot.DebugDisplayText("Tilt requested");

            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {

                var t = Task.Run(() =>
                {
                    Status = ComponentStatus.Action;
                    servoTilt.SafeIshRotate(newAngle);
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

        public void ErrorEncountered()
        {

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

                    _appRoot.DebugDisplayText("Pan to Min (" + servoPan.MinAngle.Value.Degrees + ")");

                    var t = PanTo(servoPan.MinAngle, speed);
                    t.Wait();


                    if (StopRequested)
                    {
                        break;
                    }
                    _appRoot.DebugDisplayText("Pan to Max (" + servoPan.MaxAngle.Value.Degrees + ")");

                    t = PanTo(servoPan.MaxAngle, speed);
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
