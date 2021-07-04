using Base;
using System;
using System.Threading.Tasks;

namespace Peripherals
{
    public enum DriveMethod { TwoWheelDrive, FourWheelDrive, DualTracks }
    public enum Direction { Forward, Backwards, TurnLeft, TurnRight, RotateLeft, RotateRight }

    public interface IMovementInterface: ITinkerBase
    {
        bool IsMoving { get; }
        bool IsReady { get; }
        bool ReverseMotorOrientation { get; set; }
        DriveMethod driveMethod { get; }
        bool Init(Meadow.Hardware.IPin HBridge1PinA, Meadow.Hardware.IPin HBridge1Pinb, Meadow.Hardware.IPin HBridge1PinEnable,
            Meadow.Hardware.IPin HBridge2PinA, Meadow.Hardware.IPin HBridge2Pinb, Meadow.Hardware.IPin HBridge2PinEnable);
        void Move(Direction direction, float power, TimeSpan? movementDuration = null);
        bool MoveManual(float leftPower, float rightPower, TimeSpan? movementDuration = null);
        bool MoveManual(float leftFrontPower, float rightFrontPower, float leftRearPower, float rightRearPower, TimeSpan? movementDuration = null);
        void Stop();

    }
}
