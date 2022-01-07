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
using Meadow.Gateways.Bluetooth;

namespace Servos
{
    public partial class PanTiltBase: TinkerBase, ITinkerBase
    {
        private TinkerServoBase servoPan;
        private TinkerServoBase servoTilt;
        private string _name;
        protected PCA9685 _servoControllerDevice;

        bool _stopRequested = false;

        private Characteristic _characteristic;

        public PanTiltBase(PCA9685 servoControllerDevice, string name)
        {
            _appRoot = MeadowApp.Current;
            _servoControllerDevice = servoControllerDevice;
            _name = name;
        }

        public bool StopRequested
        {
            get => _stopRequested;
            set => _stopRequested = value;
        }

        public virtual void Init(int panPwmPort, int tiltPwmPort, ServoType servoType = ServoType.SG90)
        {
            servoPan = new TinkerServoBase(_servoControllerDevice, panPwmPort, servoType, null, null, "Pan");
            servoPan.InitServo();
            servoTilt = new TinkerServoBase(_servoControllerDevice, tiltPwmPort, servoType, null, null, "Tilt");
            servoTilt.InitServo();

            _appRoot.DebugDisplayText("PanTilt " + Name + " registered and ready on ports " + panPwmPort + " and " + tiltPwmPort);
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

        public bool AssignBluetoothCharacteristicToUpdate(Characteristic characteristicString)
        {
            _characteristic = characteristicString;

            UpdateBleValue();

            return _characteristic != null;
        }

        public void UpdateBleValue()
        {
                try
                {

                    if (_characteristic != null)
                    {
                         var newValue = CurrentPanPosition.Value.Degrees + "-" + CurrentTiltPosition.Value.Degrees;                        

                        _characteristic.SetValue(Convert.ToString(newValue));
                    }
                }
                catch (Exception)
                {

                }
        }

        public string Name
        {
            get => _name;
            set => _name = value;
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

        public void Move(Angle? newPanAngle, Angle? newTiltAngle, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {

            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {
                if (newPanAngle != null)
                {
                    servoPan.a
                }

                StopRequested = false;

                    Status = ComponentStatus.Action;
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

                UpdateBleValue();
            }
        }

        public void RefreshStatus()
        {
            UpdateBleValue();
        }

        public void Test()
        {
        }

        public virtual void ErrorEncountered()
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

                    Move( PanTiltAxis.pan, servoPan.MinAngle, speed);

                    if (StopRequested)
                    {
                        break;
                    }
                    _appRoot.DebugDisplayText("Pan to Max (" + servoPan.MaxAngle.Value.Degrees + ")");

                    Move(PanTiltAxis.pan ,servoPan.MaxAngle, speed);

                    if (StopRequested)
                    {
                        break;
                    }
                }
            });

        }

        public void GoToDefault()
        {
            //var t = Task.Run(() =>
            //{
                Move(PanTiltAxis.pan, DefaultPan);
                Move(PanTiltAxis.tilt, DefaultTilt);
                UpdateBleValue();
            //});
        }
    }
}
