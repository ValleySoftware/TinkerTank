using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerations
{

    public enum ComponentStatus { Error, Ready, Action, UnInitialised };
    public enum AutomaticErrorResponse { DoNothing, TryReload, Warn, DisableComponent, DisableMotorPower }
    public enum StatusMessageTypes { Debug, Important, Error };
    public enum DriveMethod { TwoWheelDrive, FourWheelDrive, DualTracks }
    public enum Direction { Forward, Backwards, TurnLeft, TurnRight, RotateLeft, RotateRight }
    public enum DisplayStatusMessageTypes { Debug, Important, Error };
    public enum ServoMovementSpeed { Slow, Medium, Fast, Flank, Stop };
    public enum ServoType { SG90, SG90Continuous, MG996R };
    public enum PanTiltAxis { pan, tilt };
    public enum BasePinType { digital, analogue, scl, sda };
    public enum DistanceSensorLocation { front, rear, periscope }
    public enum i2cBusIdentifier { sharedBus, distanceBus}
}
