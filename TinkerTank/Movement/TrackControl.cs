using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors;
using System.Threading;
using TinkerTank;
using Base;
using Enumerations;

namespace Peripherals
{
    public class TrackControl : TinkerBase, IMovementInterface
    {
        private DriveMethod _driveMethod;
        private HBridgeMotor motorLeft;
        private HBridgeMotor motorRight;
        private bool _reverseMotorOrientation = false;
        private int reverseMotorOrientationMultiplier = 1;
        private int _defaultPower = 50;

        public TrackControl(MeadowApp appRoot)
        {
            _appRoot = appRoot;
        }

        public ComponentStatus Init(
            Meadow.Hardware.IPin HBridge1PinA, Meadow.Hardware.IPin HBridge1Pinb, Meadow.Hardware.IPin HBridge1PinEnable,
            Meadow.Hardware.IPin HBridge2PinA, Meadow.Hardware.IPin HBridge2Pinb, Meadow.Hardware.IPin HBridge2PinEnable)
        {
            Status = ComponentStatus.UnInitialised;

            try
            {
                _appRoot.DebugDisplayText("Init Motor Controller");

                _driveMethod = DriveMethod.TwoWheelDrive;

                motorLeft = new HBridgeMotor(
                    a1Pin: MeadowApp.Device.CreatePwmPort(HBridge1PinA),
                    a2Pin: MeadowApp.Device.CreatePwmPort(HBridge1Pinb),
                    enablePin: MeadowApp.Device.CreateDigitalOutputPort(HBridge1PinEnable));

                motorLeft.IsNeutral = true;

                motorRight = new HBridgeMotor(
                    a1Pin: MeadowApp.Device.CreatePwmPort(HBridge2PinA),
                    a2Pin: MeadowApp.Device.CreatePwmPort(HBridge2Pinb),
                    enablePin: MeadowApp.Device.CreateDigitalOutputPort(HBridge2PinEnable));

                motorRight.IsNeutral = true;

                _appRoot.DebugDisplayText("Setting Motor Controller Ready");
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
            }

            return Status;
        }

        public DriveMethod driveMethod { get => _driveMethod; }


        public void SetDefaultPower(int defaultPower)
        {
            if (defaultPower > 100)
            {
                _defaultPower = 100;
            }
            else
            {
                if (defaultPower <=0)
                {
                    _defaultPower = 0;
                }
                else
                {
                    _defaultPower = defaultPower;
                }
            }
        }


        public bool ReverseMotorOrientation
        {
            get => _reverseMotorOrientation;
            set
            {
                _reverseMotorOrientation = value;
                if (value)
                {
                    reverseMotorOrientationMultiplier = -1;
                }
                else
                {
                    reverseMotorOrientationMultiplier = 1;
                }
            }
        }

        public void Move(Direction direction, int power, TimeSpan? movementDuration = null)
        {
            if (power == 0)
            {
                power = _defaultPower;
            }

            //if (_appRoot.HasDrivePower)
            {
                Status = ComponentStatus.Action;

                switch (direction)
                {
                    case Direction.Forward:
                        Forward(power);
                        break;
                    case Direction.Backwards:
                        Backwards(power);
                        break;
                    case Direction.TurnLeft:
                        TurnLeft(power);
                        break;
                    case Direction.TurnRight:
                        TurnRight(power);
                        break;
                    case Direction.RotateLeft:
                        RotateLeft(power);
                        break;
                    case Direction.RotateRight:
                        RotateRight(power);
                        break;
                    default:
                        Stop();
                        break;
                }

                if (movementDuration != null)
                {
                    var stopTimerThread = new Thread(() =>
                    {
                        Thread.Sleep((int)movementDuration.Value.TotalMilliseconds);
                        Stop();

                    });

                    stopTimerThread.Start();
                }
            }
        }

        public bool MoveManual(float leftPower, float rightPower, TimeSpan? movementDuration = null)
        {
            //if (_appRoot.HasDrivePower)
            {
                motorLeft.IsNeutral = true;
                motorRight.IsNeutral = true;
                motorLeft.Power = leftPower * reverseMotorOrientationMultiplier;
                motorRight.Power = rightPower * reverseMotorOrientationMultiplier;
                Status = ComponentStatus.Action;
            }

            return true;
        }

        public bool MoveManual(float leftFrontPower, float rightFrontPower, float leftRearPower, float rightRearPower, TimeSpan? movementDuration = null)
        {
            //if (_appRoot.HasDrivePower)
            {
                motorLeft.IsNeutral = true;
                motorRight.IsNeutral = true;
                motorLeft.Power = leftFrontPower * reverseMotorOrientationMultiplier;
                motorRight.Power = rightFrontPower * reverseMotorOrientationMultiplier;
                Status = ComponentStatus.Action;
            }

            return true;
        }

        public void Stop()
        {
            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = 0;
            motorRight.Power = 0;
            Status = ComponentStatus.Ready;
        }

        public void BreakAndHold()
        {
            motorLeft.IsNeutral = false;
            motorRight.IsNeutral = false;
            motorLeft.Power = 0;
            motorRight.Power = 0;
            Status = ComponentStatus.Ready;
        }

        private void Forward(float power)
        {
            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * reverseMotorOrientationMultiplier;
            motorRight.Power = power * reverseMotorOrientationMultiplier;
            Status = ComponentStatus.Action;
        }

        private void Backwards(float power)
        {
            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * -1 * reverseMotorOrientationMultiplier;
            motorRight.Power = power * -1 * reverseMotorOrientationMultiplier;
            Status = ComponentStatus.Action;
        }

        private void TurnLeft(float power)
        {
            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = 0;
            motorRight.Power = power * reverseMotorOrientationMultiplier * 2;
            Status = ComponentStatus.Action;
        }

        private void TurnRight(float power)
        {
            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * reverseMotorOrientationMultiplier * 2;
            motorRight.Power = 0;
            Status = ComponentStatus.Action;
        }

        private void RotateLeft(float power)
        {
            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * -1 * reverseMotorOrientationMultiplier;
            motorRight.Power = power * reverseMotorOrientationMultiplier;
            Status = ComponentStatus.Action;
        }

        private void RotateRight(float power)
        {
            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * reverseMotorOrientationMultiplier;
            motorRight.Power = power * -1 * reverseMotorOrientationMultiplier;
            Status = ComponentStatus.Action;
        }

        public void RefreshStatus()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            throw new NotImplementedException();
        }
    }
}
