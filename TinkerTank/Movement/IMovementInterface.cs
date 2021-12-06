using Base;
using Enumerations;
using System;
using System.Threading.Tasks;

namespace Peripherals
{

    public interface IMovementInterface: ITinkerBase
    {
        bool ReverseMotorOrientation { get; set; }
        DriveMethod driveMethod { get; }
        ComponentStatus Init(
            Meadow.Hardware.IPin HBridge1PinA, Meadow.Hardware.IPin HBridge1Pinb, Meadow.Hardware.IPin HBridge1PinEnable,
            Meadow.Hardware.IPin HBridge2PinA, Meadow.Hardware.IPin HBridge2Pinb, Meadow.Hardware.IPin HBridge2PinEnable);
        void Move(Direction direction, int power, TimeSpan movementDuration, bool safeMove, bool smoothPowerTranstion = false);
        void SetDefaultPower(int defaultPower);
        void Stop(bool smoothPowerTranstion = false);
        bool StopRequested { get; set; }

    }
}
