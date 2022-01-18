using System;
using System.Collections.Generic;
using System.Text;

namespace Enumerations
{ 
    public enum ComponentStatus { Error, UnInitialised, Ready, Action, None };
    public enum AutomaticErrorResponse { DoNothing, TryReload, Warn, DisableComponent, DisableMotorPower }
    public enum DriveMethod { SingleDrive, DualDrive, QuadDrive, HexDrive, AdjustableHexDrive }
    public enum Direction { Forward, Backwards, TurnLeft, TurnRight, RotateLeft, RotateRight, Stop }
    public enum LogStatusMessageTypes {Debug, BLERecord, Information, Important, Error, CriticalError };
    public enum ServoMovementSpeed { Slow, Medium, Fast, Flank, Stop };
    public enum ServoType { SG90, SG90Continuous, MG996R };
    public enum I2CExpanderChannel { sensorZero, fixedForwardDistance, periscopeDistance }
    public enum CharacteristicsNames { Stop, PanTilt, Power, AdvancedMove, PanSweep, ForwardDistance, PanTiltDistance, Lights, Logging };
    public enum MovementAutoStopMode { None, Proximity, Timespan, Distance }

    public class BLEConstants
    {

        public const string definitionName = "BerthaDefinition";
        public const string serviceName = "BerthaService";
        public const ushort serviceUuid = 41;

        public const string UUIDStop = @"017e99d6-8a61-11eb-8dcd-0242ac1a5100";
        public const string UUIDPanTilt = @"017e99d6-8a61-11eb-8dcd-0242ac1a5102";
        public const string UUIDPower = @"017e99d6-8a61-11eb-8dcd-0242ac1a5103";
        public const string UUIDAdvancedMove = @"017e99d6-8a61-11eb-8dcd-0242ac1a5104";
        public const string UUIDPanSweep = @"017e99d6-8a61-11eb-8dcd-0242ac1a5105";
        public const string UUIDForwardDistance = @"017e99d6-8a61-11eb-8dcd-0242ac1a5106";
        public const string UUIDPanTiltDistance = @"017e99d6-8a61-11eb-8dcd-0242ac1a5107";
        public const string UUIDLights = @"017e99d6-8a61-11eb-8dcd-0242ac1a5108";
        public const string UUIDLogging = @"017e99d6-8a61-11eb-8dcd-0242ac1a5109";
    }
}
