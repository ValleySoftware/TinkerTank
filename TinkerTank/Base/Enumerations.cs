using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerations
{
    public enum ComponentStatus { Error, Ready, Action, UnInitialised };
    public enum StatusMessageTypes { Debug, Important, Error };
    public enum DriveMethod { TwoWheelDrive, FourWheelDrive, DualTracks }
    public enum Direction { Forward, Backwards, TurnLeft, TurnRight, RotateLeft, RotateRight }
    public enum DisplayStatusMessageTypes { Debug, Important, Error };
    public enum ServoType { SG90Standard, MG996RStandard };
    public enum ServoMovementSpeed { Slow, Medium, Fast, Flank };


}
