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
using Servos;

namespace Peripherals
{
    public class TrackControl : TinkerBase, IMovementInterface
    {
        private DriveMethod _driveMethod;
        private HBridgeMotor motorLeft;
        private HBridgeMotor motorRight;
        private bool _reverseMotorOrientation = false;
        private int reverseMotorOrientationMultiplier = 1; //This is changed in the public property if the motor controller is backwards.
        private static double opposingActionMultiplier = 1.5; //If the motors are going opposite ways, apply more power as they face more resistance.
        private double _defaultPower = 50;
        PCA9685 _pcaDevice;
        F7Micro _device;
        bool _stopRequested = false;

        public TrackControl(F7Micro device, PCA9685 pcaDevice, MeadowApp appRoot)
        {
            _appRoot = appRoot;
            _pcaDevice = pcaDevice;
            _device = device;

            ErrorResponse = AutomaticErrorResponse.DisableMotorPower;
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

                
                motorLeft = new HBridgeMotor(MeadowApp.Device,
                    a1Pin: HBridge1PinA,
                    a2Pin: HBridge1Pinb,
                    enablePin: HBridge1PinEnable);

                motorLeft.IsNeutral = true;

                motorRight = new HBridgeMotor(MeadowApp.Device,
                    a1Pin: HBridge2PinA,
                    a2Pin: HBridge2Pinb,
                    enablePin: HBridge2PinEnable);

                motorRight.IsNeutral = true;
                
                _appRoot.DebugDisplayText("Setting Motor Controller Ready");
                Status = ComponentStatus.Ready;
            }
            catch (Exception)
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

        public void Move(Direction direction, int power, TimeSpan movementDuration, bool safeMove = true, bool smoothPowerTranstion = false)
        {
            try
            { 
            if (power == 0)
            {
                power = (int)Math.Round(_defaultPower);
            }

            //if (_appRoot.HasDrivePower)
            {
                Status = ComponentStatus.Action;
                StopRequested = false;

                Task.Run(async () =>
                {
                    switch (direction)
                    {
                        case Direction.Forward:
                            Forward(power, safeMove);
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
                });

                if (movementDuration.Equals(TimeSpan.Zero))
                {
                    Task.Run(async () =>
                    {
                        try
                        { 
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        _appRoot.DebugDisplayText("Backup Stop", DisplayStatusMessageTypes.Important);
                        StopRequested = true;
                        Stop();
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        try
                        { 
                        await Task.Delay(movementDuration);
                        StopRequested = true;
                        Stop();
                        _appRoot.DebugDisplayText("Timer Stop", DisplayStatusMessageTypes.Debug);
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
                }
            }
            catch (Exception exo)
            {

            }
        }

        public bool StopRequested
        {
            get => _stopRequested;
            set => _stopRequested = value;
        }

        public void Stop(bool smoothPowerTranstion = false)
        {
            StopRequested = true;

            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;

            //float powerSetting = 0;


            //while (powerSetting >= 0)
            //{
                motorLeft.Power = 0;
                motorRight.Power = 0;
                //powerSetting = powerSetting - (float)0.1;
                //Thread.Sleep(100);
           // }

            Status = ComponentStatus.Ready;
            _appRoot.DebugDisplayText("Stop Completed", DisplayStatusMessageTypes.Important);
        }

        public void BreakAndHold(bool smoothPowerTranstion = false)
        {
            try { 
            StopRequested = true;

            motorLeft.IsNeutral = false;
            motorRight.IsNeutral = false;

            //float powerSetting = 0;

            //if (smoothPowerTranstion)
            //{
            //    powerSetting = motorLeft.Power;
            //}

           // while (powerSetting >= 0)
            //{
                motorLeft.Power = 0;
                motorRight.Power = 0;
                //powerSetting = powerSetting - (float)0.1;
                //Thread.Sleep(100);
            //}

            Status = ComponentStatus.Ready;

            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
            }
        }

        private void Forward(float power, bool safeMove = true, bool smoothPowerTranstion = false)
        {
            try
            {
                //testing
                //smoothPowerTranstion = true;

                double useThisPower = _defaultPower;

                useThisPower = power * reverseMotorOrientationMultiplier;

                SanityCheckPower(ref useThisPower, ref useThisPower);

                motorLeft.IsNeutral = true;
                motorRight.IsNeutral = true;

                float powerSetting = 0;

                if (!smoothPowerTranstion)
                {
                    powerSetting = (float)useThisPower;
                }

                while (powerSetting <= useThisPower)
                {
                    if (safeMove &&
                        Convert.ToInt32(_appRoot.distController.FixedFrontDistance.SensorValue) < 50)
                    {
                        StopRequested = true;
                        _appRoot.DebugDisplayText("SafeStop due to distance", DisplayStatusMessageTypes.Debug);
                    }

                    if (StopRequested)
                    {
                        Stop();
                        break;
                    }
                    motorLeft.Power = powerSetting;
                    motorRight.Power = powerSetting;
                    powerSetting = powerSetting + (float)0.2;
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
            }

            Status = ComponentStatus.Action;
        }

        private void Backwards(float power, bool smoothPowerTranstion = false)
        {

            try
            {
                //testing
                //smoothPowerTranstion = true;

                _appRoot.DebugDisplayText("a - " + power, DisplayStatusMessageTypes.Debug);

                double useThisPower = _defaultPower * -1;

                _appRoot.DebugDisplayText("b - " + useThisPower, DisplayStatusMessageTypes.Debug);

                useThisPower = power * reverseMotorOrientationMultiplier;

                SanityCheckPower(ref useThisPower, ref useThisPower);

                motorLeft.IsNeutral = true;
                motorRight.IsNeutral = true;

                float powerSetting = 0;

                if (!smoothPowerTranstion)
                {
                    powerSetting = (float)useThisPower;
                }

                while (powerSetting >= useThisPower)
                {
                    if (StopRequested)
                    {
                        Stop();
                        break;
                    }

                    motorLeft.Power = powerSetting;
                    motorRight.Power = powerSetting;
                    powerSetting = powerSetting - (float)0.2;
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
            }

            Status = ComponentStatus.Action;
        }

        private void TurnLeft(float power, bool smoothPowerTranstion = false)
        {
            double leftPower = _defaultPower;
            double rightPower = _defaultPower;

            leftPower = 0;
            rightPower = power * reverseMotorOrientationMultiplier * opposingActionMultiplier;

            SanityCheckPower(ref leftPower, ref rightPower);

            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = (float)leftPower;
            motorRight.Power = (float)rightPower;
            Status = ComponentStatus.Action;
        }

        private static void SanityCheckPower(ref double leftPower, ref double rightPower)
        {
            if (leftPower > 100)
            {
                leftPower = 100;
            }
            if (leftPower < -100)
            {
                leftPower = -100;
            }
            if (rightPower > 100)
            {
                rightPower = 100;
            }
            if (rightPower < -100)
            {
                rightPower = -100;
            }
        }

        private void TurnRight(float power, bool smoothPowerTranstion = false)
        {
            double leftPower = _defaultPower;
            double rightPower = _defaultPower;

            leftPower = power * reverseMotorOrientationMultiplier * opposingActionMultiplier;
            rightPower = 0;

            SanityCheckPower(ref leftPower, ref rightPower);

            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = (float)leftPower;
            motorRight.Power = (float)rightPower;
            Status = ComponentStatus.Action;
        }

        private void RotateLeft(float power, bool smoothPowerTranstion = false)
        {
            double leftPower = _defaultPower;
            double rightPower = _defaultPower;

            leftPower = power * -1 * reverseMotorOrientationMultiplier * opposingActionMultiplier;
            rightPower = power * reverseMotorOrientationMultiplier * opposingActionMultiplier;

            SanityCheckPower(ref leftPower, ref rightPower);

            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = (float)leftPower;
            motorRight.Power = (float)rightPower;
            Status = ComponentStatus.Action;
        }

        private void RotateRight(float power, bool smoothPowerTranstion = false)
        { 
            double leftPower = _defaultPower;
            double rightPower = _defaultPower;

            leftPower = power * reverseMotorOrientationMultiplier * opposingActionMultiplier;
            rightPower = power * -1 * reverseMotorOrientationMultiplier * opposingActionMultiplier;

            SanityCheckPower(ref leftPower, ref rightPower);

            motorLeft.IsNeutral = true;
            motorRight.IsNeutral = true;
            motorLeft.Power = (float)leftPower;
            motorRight.Power = (float)rightPower;
            Status = ComponentStatus.Action;
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
