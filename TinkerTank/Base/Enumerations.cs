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

    public class BluetoothCharacturistics
    {
        public enum CharacteristicsNames { Stop, Move, PanTilt, Power, AdvancedMove };
        private enum CharacteristicsUUID { UUIDStop, UUIDMove, UUIDPanTilt, UUIDPower, UUIDAdvancedMove };

        private const string UUIDStop = "017e99d6-8a61-11eb-8dcd-0242ac1a5109";
        private const string UUIDMove = "017e99d6-8a61-11eb-8dcd-0242ac1f407e";
        private const string UUIDPanTilt = "017e99d6-8a61-11eb-8dcd-0242ac1fa9a4";
        private const string UUIDPower = "017e99d6-8a61-11eb-8dcd-0242ac1f904e";
        private const string UUIDAdvancedMove = "017e99d6-8a61-11eb-8dcd-0242ac1fad40";

        public static readonly List<KeyValuePair<CharacteristicsNames, string>> Characteristics = 
            new List<KeyValuePair<CharacteristicsNames, string>>() 
            {
                new KeyValuePair<CharacteristicsNames,string>(CharacteristicsNames.Stop,UUIDStop),
                new KeyValuePair<CharacteristicsNames,string>(CharacteristicsNames.Move,UUIDMove),
                new KeyValuePair<CharacteristicsNames,string>(CharacteristicsNames.PanTilt,UUIDPanTilt),
                new KeyValuePair<CharacteristicsNames,string>(CharacteristicsNames.Power,UUIDPower),
                new KeyValuePair<CharacteristicsNames,string>(CharacteristicsNames.AdvancedMove,UUIDAdvancedMove)
            };
    }

}
