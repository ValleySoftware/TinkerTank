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

namespace Peripherals
{
    public class TrackControl : TinkerBase, IMovementInterface
    {
        private DriveMethod _driveMethod;
        private bool _isMoving;
        private bool _isReady;
        private HBridgeMotor motorLeft;
        private HBridgeMotor motorRight;
        private bool _reverseMotorOrientation = false;
        private int reverseMotorOrientationMultiplier = 1;

        public TrackControl(MeadowApp appRoot)
        {
            _appRoot = appRoot;
        }

        public bool Init(
            Meadow.Hardware.IPin HBridge1PinA, Meadow.Hardware.IPin HBridge1Pinb, Meadow.Hardware.IPin HBridge1PinEnable,
            Meadow.Hardware.IPin HBridge2PinA, Meadow.Hardware.IPin HBridge2Pinb, Meadow.Hardware.IPin HBridge2PinEnable)
        {
            IsReady = false;

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
                IsReady = true;
            }
            catch (Exception ex)
            {

            }

            return IsReady;
        }

        public DriveMethod driveMethod { get => _driveMethod; }

        public bool IsMoving
        {
            get => _isMoving;
            private set => _isMoving = value;
        }

        public bool IsReady
        {
            get => _isReady;
            private set => _isReady = value;
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

        public void Move(Direction direction, float power, TimeSpan? movementDuration = null)
        {

            //if (_appRoot.HasDrivePower)
            {
                IsMoving = true;

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
                motorRight.IsNeutral = true;
                motorRight.IsNeutral = true;
                motorLeft.Power = leftPower * reverseMotorOrientationMultiplier;
                motorRight.Power = rightPower * reverseMotorOrientationMultiplier;
                IsMoving = true;
            }

            return true;
        }

        public bool MoveManual(float leftFrontPower, float rightFrontPower, float leftRearPower, float rightRearPower, TimeSpan? movementDuration = null)
        {
            //if (_appRoot.HasDrivePower)
            {
                motorRight.IsNeutral = true;
                motorRight.IsNeutral = true;
                motorLeft.Power = leftFrontPower * reverseMotorOrientationMultiplier;
                motorRight.Power = rightFrontPower * reverseMotorOrientationMultiplier;
                IsMoving = true;
            }

            return true;
        }

        public void Stop()
        {
            motorRight.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = 0;
            motorRight.Power = 0;
            IsMoving = false;
        }

        public void BreakAndHold()
        {
            motorRight.IsNeutral = false;
            motorRight.IsNeutral = false;
            motorLeft.Power = 0;
            motorRight.Power = 0;
            IsMoving = false;
        }

        private void Forward(float power)
        {
            motorRight.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * reverseMotorOrientationMultiplier;
            motorRight.Power = power * reverseMotorOrientationMultiplier;
        }

        private void Backwards(float power)
        {
            motorRight.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * -1 * reverseMotorOrientationMultiplier;
            motorRight.Power = power * -1 * reverseMotorOrientationMultiplier;
        }

        private void TurnLeft(float power)
        {
            motorRight.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = 0;
            motorRight.Power = power * reverseMotorOrientationMultiplier * 2;
        }

        private void TurnRight(float power)
        {
            motorRight.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * reverseMotorOrientationMultiplier * 2;
            motorRight.Power = 0;
        }

        private void RotateLeft(float power)
        {
            motorRight.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * -1 * reverseMotorOrientationMultiplier;
            motorRight.Power = power * reverseMotorOrientationMultiplier;
        }

        private void RotateRight(float power)
        {
            motorRight.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = power * reverseMotorOrientationMultiplier;
            motorRight.Power = power * -1 * reverseMotorOrientationMultiplier;
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
