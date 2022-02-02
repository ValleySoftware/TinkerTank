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
        protected PCA9685 _servoControllerDevice;

        bool _stopRequested = false;

        private Characteristic _characteristic;

        public PanTiltBase(PCA9685 servoControllerDevice, string name)
        {
            _appRoot = MeadowApp.Current;
            _servoControllerDevice = servoControllerDevice;
            Name = name;
        }

        public bool StopRequested
        {
            get => _stopRequested;
            set => _stopRequested = value;
        }

        public virtual void Init(int panPwmPort, int tiltPwmPort, ServoType servoType = ServoType.SG90)
        {
            servoPan = new TinkerServoBase(_servoControllerDevice, panPwmPort, servoType, null, null, Name + "Pan");
            servoPan.InitServo(true);
            servoTilt = new TinkerServoBase(_servoControllerDevice, tiltPwmPort, servoType, null, null, Name + "Tilt");
            servoTilt.InitServo(true);

            _appRoot.DebugDisplayText(Name + " registered and ready on ports " + panPwmPort + " and " + tiltPwmPort);
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

        public bool IsServoReversed (PanTiltServos servoToGet)
        {
            var result = false;

            if (servoToGet == PanTiltServos.servoPan)
            {
                result = servoPan.ReverseDirection;
            }

            if (servoToGet == PanTiltServos.servoTilt)
            {
                result = servoTilt.ReverseDirection;
            }

            return result;
        }

        public bool SetServoIsReversed(PanTiltServos servoToSet, bool newIsReversed)
        {
            var result = false;

            if (servoToSet == PanTiltServos.servoPan)
            {
                servoPan.ReverseDirection = newIsReversed;
                result = servoPan.ReverseDirection;
            }

            if (servoToSet == PanTiltServos.servoTilt)
            {
                servoTilt.ReverseDirection = newIsReversed;
                result = servoTilt.ReverseDirection;
            }

            return result;
        }

        public bool AssignBluetoothCharacteristicToUpdate(Characteristic characteristicString)
        {
            MeadowApp.Current.DebugDisplayText("Assign BLE characteristic to PanTilt", LogStatusMessageTypes.Debug);

            _characteristic = characteristicString;

            //UpdateBleValue();

            return _characteristic != null;
        }

        public void UpdateBleValue()
        {
                try
            {
                MeadowApp.Current.DebugDisplayText("Update PanTilt BLE Value", LogStatusMessageTypes.Debug);

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

        public void Move(Angle? destinationPanAngle, Angle? destinationTiltAngle, ServoMovementSpeed movementSpeed = ServoMovementSpeed.Flank)
        {

            if (Status != ComponentStatus.Error &&
                Status != ComponentStatus.UnInitialised)
            {

                StopRequested = false;

                    Status = ComponentStatus.Action;
                    _appRoot.DebugDisplayText("Pan/Tilt Running", LogStatusMessageTypes.Debug);

                if (movementSpeed == ServoMovementSpeed.Stop)
                {
                    StopRequested = true;
                }
                else
                {
                    if (movementSpeed == ServoMovementSpeed.Flank)
                    {
                        try
                        {
                            _appRoot.DebugDisplayText("Pan speed flank", LogStatusMessageTypes.Debug);

                            if (destinationPanAngle != null)
                            {
                                _ = servoPan.SafeIshRotate(destinationPanAngle);
                            }

                            if (destinationTiltAngle != null)
                            {
                                _ = servoTilt.SafeIshRotate(destinationTiltAngle);
                            }

                            _appRoot.DebugDisplayText("Pan/Tilt at flank finished", LogStatusMessageTypes.Debug);
                        }
                        catch (Exception PanTiltFlankEx)
                        {
                            _appRoot.DebugDisplayText("Pan/Tilt flank " + PanTiltFlankEx.Message, LogStatusMessageTypes.Error);
                        }
                    }
                    else
                    {
                        _appRoot.DebugDisplayText("Slow Pan/Tilt requested.", LogStatusMessageTypes.Debug);

                        int millisecondDelay = 0;

                        switch (movementSpeed)
                        {
                            case ServoMovementSpeed.Slow: millisecondDelay = 500; break;
                            case ServoMovementSpeed.Medium: millisecondDelay = 250; break;
                            case ServoMovementSpeed.Fast: millisecondDelay = 125; break;
                            default: millisecondDelay = 250; break;
                        }

                        Angle? panStepPos = new Angle(CurrentPanPosition.Value.Degrees);
                        Angle? tiltStepPos = new Angle(CurrentTiltPosition.Value.Degrees);

                        _appRoot.DebugDisplayText(
                                "servo move from " +
                                panStepPos.Value.Degrees +
                                "/" +
                                tiltStepPos.Value.Degrees +
                                " to " +
                                destinationPanAngle.Value.Degrees +
                                "/" +
                                destinationTiltAngle.Value.Degrees +
                                " with " +
                                millisecondDelay, LogStatusMessageTypes.Debug);

                        bool continuePanLooping = true;
                        bool continueTiltLooping = true;
                        var panIncriment = 1;
                        var tiltIncriment = 1;

                        if (panStepPos.Value.Degrees < destinationPanAngle.Value.Degrees)
                        {
                            panIncriment = -1;
                        }

                        if (tiltStepPos.Value.Degrees < destinationTiltAngle.Value.Degrees)
                        {
                            tiltIncriment = -1;
                        }

                        while (
                            continuePanLooping ||
                            continueTiltLooping)
                        {
                            double panMoveResult = -1;
                            double tiltMoveResult = -1;

                            if (Math.Round(panStepPos.Value.Degrees) != Math.Round(destinationPanAngle.Value.Degrees))
                            {
                                panStepPos = new Angle(panStepPos.Value.Degrees + panIncriment);
                                panMoveResult = servoPan.SafeIshRotate(panStepPos);
                            }
                            else
                            {
                                continuePanLooping = false;
                            }

                            if (Math.Round(tiltStepPos.Value.Degrees) != Math.Round(destinationTiltAngle.Value.Degrees))
                            {
                                tiltStepPos = new Angle(tiltStepPos.Value.Degrees + tiltIncriment);
                                tiltMoveResult = servoPan.SafeIshRotate(tiltStepPos);
                            }
                            else
                            {
                                continueTiltLooping = false;
                            }

                            Thread.Sleep(millisecondDelay);

                            if (StopRequested ||
                                panMoveResult == -1 ||
                                tiltMoveResult == -1)
                            {
                                break;
                            }
                        }

                    }
                    Status = ComponentStatus.Ready;

                    //UpdateBleValue();
                }
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

                    _appRoot.DebugDisplayText("Pan to Min (" + servoPan.MinAngle.Value.Degrees + ")", LogStatusMessageTypes.Debug);

                    Move(servoPan.MinAngle, servoTilt.CurrentAngle, speed);

                    if (StopRequested)
                    {
                        break;
                    }

                    _appRoot.DebugDisplayText("Pan to Max (" + servoPan.MaxAngle.Value.Degrees + ")", LogStatusMessageTypes.Debug);

                    Move(servoPan.MaxAngle, servoTilt.CurrentAngle, speed);

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
                Move(DefaultPan, DefaultTilt);
                //UpdateBleValue();
            //});
        }
    }
}
