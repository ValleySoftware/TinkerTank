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

        public async Task Move(PanTiltAxis axis, Angle? destinationAngle, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {
            TinkerServoBase servoToUse = null; ;

            if (axis == PanTiltAxis.pan)
            {
                servoToUse = servoPan;

                if (destinationAngle == null)
                {
                    destinationAngle = DefaultPan;
                }
            }
            else
            {
                servoToUse = servoTilt;

                if (destinationAngle == null)
                {
                    destinationAngle = DefaultTilt;
                }
            }

            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {
                StopRequested = false;

                //var t = Task.Run(() =>
                //{
                    //Status = ComponentStatus.Action;
                    _appRoot.DebugDisplayText("Pan Running");

                    if (movementSpeed == ServoMovementSpeed.Stop)
                    {
                        StopRequested = true;
                    }
                    else
                    {
                        if (movementSpeed == ServoMovementSpeed.Flank)
                        {
                            _appRoot.DebugDisplayText("Pan speed flank");
                            servoToUse.SafeIshRotate(destinationAngle);
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

                            Angle? stepPos = new Angle(CurrentPanPosition.Value.Degrees);

                            _appRoot.DebugDisplayText("servo move from " + stepPos.Value.Degrees + " to " + destinationAngle.Value.Degrees + " with " + millisecondDelay);

                        var incriment = 1;

                        if (stepPos.Value.Degrees < destinationAngle.Value.Degrees)
                        {
                            incriment = -1;
                        }

                                while (Math.Round(stepPos.Value.Degrees) != Math.Round(destinationAngle.Value.Degrees))
                                {
                                    stepPos = new Angle(stepPos.Value.Degrees + incriment);
                                    var servoMovedTo = servoToUse.SafeIshRotate(stepPos);
                                    Thread.Sleep(millisecondDelay);

                                    if (StopRequested ||
                                        servoMovedTo == -1)
                                    {
                                        break;
                                    }
                                }
                        }
                    }
                    Status = ComponentStatus.Ready;
                //});

                //return t;
            }
            //return null;
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

                    var t = Move( PanTiltAxis.pan, servoPan.MinAngle, speed);
                    t.Wait();


                    if (StopRequested)
                    {
                        break;
                    }
                    _appRoot.DebugDisplayText("Pan to Max (" + servoPan.MaxAngle.Value.Degrees + ")");

                    t = Move(PanTiltAxis.pan ,servoPan.MaxAngle, speed);
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
                Move(PanTiltAxis.pan, DefaultPan);
                Move(PanTiltAxis.tilt, DefaultTilt);
            });
        }
    }
}
