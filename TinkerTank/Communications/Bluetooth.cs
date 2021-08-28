using Base;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Gateways.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkerTank;

namespace Communications
{
    public class BlueTooth : TinkerBase, ITinkerBase
    {

        public const string definitionName = "BerthaDefinition";
        public const string serviceName = "BerthaService";
        public const ushort serviceUuid = 41;

        public enum CharacteristicsNames { Stop, Move, PanTilt, Power, AdvancedMove };
        private enum CharacteristicsUUID { UUIDStop, UUIDMove, UUIDPanTilt, UUIDPower, UUIDAdvancedMove };

        private const string UUIDStop = @"017e99d6-8a61-11eb-8dcd-0242ac1a5100";
        private const string UUIDMove = @"017e99d6-8a61-11eb-8dcd-0242ac1a5101";
        private const string UUIDPanTilt = @"017e99d6-8a61-11eb-8dcd-0242ac1a5102";
        private const string UUIDPower = @"017e99d6-8a61-11eb-8dcd-0242ac1a5103";
        private const string UUIDAdvancedMove = @"017e99d6-8a61-11eb-8dcd-0242ac1a5104";

        private Definition PrimaryControlDefinition;
        private Service primaryControlService;
        private CharacteristicInt32  charStop;
        private CharacteristicString charMove;
        private CharacteristicString charPanTilt;
        private CharacteristicString charPower;
        private CharacteristicString charAdvancedMove;

        public BlueTooth(MeadowApp appRoot)
        {
            _appRoot = appRoot;
            Status = ComponentStatus.UnInitialised;
        }

        public ComponentStatus Init()
        {
            Status = ComponentStatus.UnInitialised;

            try
            {
                PrepareCharacteristics();

                PrepareDefinition();

                MeadowApp.Device.InitCoprocessor();
                MeadowApp.Device.BluetoothAdapter.StartBluetoothServer(PrimaryControlDefinition);

                _appRoot.DebugDisplayText("BT Service started", DisplayStatusMessageTypes.Important);

                PrepareCharacteristicEventHandlers();              

                Status = ComponentStatus.Ready;
            }
            catch (Exception bex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText(bex.Message);
            }

            return Status;
        }

        private void RequestPower(string payload)
        {

            var valueAsInt = Convert.ToInt32(payload);

            switch (valueAsInt)
            {
                case 0: _appRoot.powerController.Disconnect(); break;
                case 1: _appRoot.powerController.Connect(); break;
                default:
                    {
                        if (valueAsInt >= 100)
                        {
                            _appRoot.movementController.SetDefaultPower(100);
                        }
                        else
                        {
                            if (valueAsInt <= 2)
                            {
                                _appRoot.movementController.SetDefaultPower(0);
                            }
                            else
                            {
                                _appRoot.movementController.SetDefaultPower(valueAsInt);
                            }
                        }
                    }
                    break;
            }
        }

        private void RequestPanTilt(string payload)
        {
            //Device-PanTo-TiltTo-Speed
            //00-000-000-0

            try
            {
                var sp = payload.Split("-");

                if (sp.Count() >= 3)
                {

                    int device = Convert.ToInt32(sp[0]);
                    int pan = Convert.ToInt32(sp[1]);
                    int tilt = Convert.ToInt32(sp[2]);
                    ServoMovementSpeed speed = ServoMovementSpeed.Flank;
                    if (sp.Count() == 4)
                    {
                        int s = Convert.ToInt32(sp[3]);
                        speed = (ServoMovementSpeed)s;
                    }



                    _appRoot.DebugDisplayText(device.ToString() + " " + pan.ToString() + " " + tilt.ToString() + " " + speed.ToString(), DisplayStatusMessageTypes.Important);

                    if (device < _appRoot.i2CPWMController.PanTilts.Count)
                    {
                        _appRoot.i2CPWMController.PanTilts[device].PanTo(pan, speed);
                        _appRoot.i2CPWMController.PanTilts[device].TiltTo(tilt, speed);
                    }
                }
            }
            catch (Exception decipherPanTiltEx)
            {
                _appRoot.DebugDisplayText("DecyipherError: Payload = " + payload, DisplayStatusMessageTypes.Error);
                _appRoot.DebugDisplayText("DecyipherError: Exception= " + decipherPanTiltEx, DisplayStatusMessageTypes.Error);
            }
        }

        private void RequestStop()
        {
            _appRoot.movementController.Stop();
        }

        private void RequestMove(string payload)
        {
            var testMotorDuration = new TimeSpan(0, 0, 0, 0, 250);

            switch (Convert.ToInt32(payload))
            {
                case -1:
                    _appRoot.movementController.Stop();
                    break;
                case (int)Direction.Forward: //0
                    _appRoot.movementController.Move(Direction.Forward, 0, testMotorDuration);
                    break;
                case (int)Direction.Backwards://1
                    _appRoot.movementController.Move(Direction.Backwards, 0, testMotorDuration);
                    break;
                case (int)Direction.TurnLeft://2
                    _appRoot.movementController.Move(Direction.TurnLeft, 0, testMotorDuration);
                    break;
                case (int)Direction.TurnRight://3
                    _appRoot.movementController.Move(Direction.TurnRight, 0, testMotorDuration);
                    break;
                case (int)Direction.RotateLeft://4
                    _appRoot.movementController.Move(Direction.RotateLeft, 0, testMotorDuration);
                    break;
                case (int)Direction.RotateRight://5
                    _appRoot.movementController.Move(Direction.RotateRight, 0, testMotorDuration);
                    break;
                default:
                    break;
            }
        }

        private void RequestAdvancedMove(string payload)
        {
            //movementDirection-powerPercent-durationInMilliseconds
            //00-000-000

            var sp = payload.Split("-");

            if (sp.Count() == 3)
            {
                try
                {
                    int direction = Convert.ToInt32(sp[0]);
                    int power = Convert.ToInt32(sp[1]);
                    double duration = Convert.ToDouble(sp[2]);

                    _appRoot.movementController.Move((Direction)direction, power, TimeSpan.FromMilliseconds(duration));
                }
                catch (Exception decipherAdvancedMoveEx)
                {

                }
            }
        }


        private void PrepareDefinition()
        {
            _appRoot.DebugDisplayText("PrepBLEDefinitions", DisplayStatusMessageTypes.Debug);

            primaryControlService =
                new Service(
                    serviceName,
                    serviceUuid,
                    charStop,
                    charMove,
                    charPanTilt,
                    charPower,
                    charAdvancedMove
                    );

            foreach (var element in primaryControlService.Characteristics)
            {
                //Loop through and set the 'read' property to the characteristics name.
                try
                {
                    element.SetValue(element.Name);
                }
                catch (Exception iDontKnowHowToCheckCharactiristicTypeSoThisWillCatchBoolOrInt)
                {

                }
            }

            PrimaryControlDefinition = new Definition(
                definitionName,
                primaryControlService
                );
        }

        private void PrepareCharacteristics()
        {

            _appRoot.DebugDisplayText("PrepBLECharacturistics", DisplayStatusMessageTypes.Important);

            charStop = new CharacteristicInt32(
                            name: CharacteristicsNames.Stop.ToString(),
                            uuid: UUIDStop,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            descriptors: new Descriptor(UUIDStop, CharacteristicsNames.Stop.ToString())
                            );
            charMove = new CharacteristicString(
                            name: CharacteristicsNames.Move.ToString(),
                            uuid: UUIDMove,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(UUIDMove, CharacteristicsNames.Move.ToString())
                            );
            charPanTilt = new CharacteristicString(
                            name: CharacteristicsNames.PanTilt.ToString(),
                            uuid: UUIDPanTilt,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(UUIDPanTilt, CharacteristicsNames.PanTilt.ToString())
                            );
            charPower = new CharacteristicString(
                            name: CharacteristicsNames.Power.ToString(),
                            uuid: UUIDPower,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(UUIDPower, CharacteristicsNames.Power.ToString())
                            );
            charAdvancedMove = new CharacteristicString(
                            name: CharacteristicsNames.AdvancedMove.ToString(),
                            uuid: UUIDAdvancedMove,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(UUIDAdvancedMove, CharacteristicsNames.AdvancedMove.ToString())
                            );
        }

        private void PrepareCharacteristicEventHandlers()
        {
            _appRoot.DebugDisplayText("PrepBLEHandlers", DisplayStatusMessageTypes.Debug);

            foreach (var characteristic in primaryControlService.Characteristics)
            {
                characteristic.ValueSet += (c, d) =>
                {

                    _appRoot.DebugDisplayText("Received ble msg", DisplayStatusMessageTypes.Debug, false);

                    string payload = string.Empty;

                    try
                    {
                        payload = d.ToString();
                    }
                    catch (Exception dataEx)
                    {
                        _appRoot.DebugDisplayText("error at BT receive" + dataEx.Message, DisplayStatusMessageTypes.Debug, false, false);
                    }

                    _appRoot.DebugDisplayText("Received " + c.Name + " with " + payload, DisplayStatusMessageTypes.Important, false);

                    try
                    {      
                        switch (c.Name)
                        {
                            case "Stop": RequestStop(); break;
                            case "Move": RequestMove(payload); break;
                            case "PanTilt": RequestPanTilt(payload); break;
                            case "Power": RequestPower(payload); break;
                            case "AdvancedMove": RequestAdvancedMove(payload); break;
                            default: RequestStop(); break;
                        }

                        Status = ComponentStatus.Ready;
                    }
                    catch (Exception ex)
                    {
                        Status = ComponentStatus.Error;
                        _appRoot.DebugDisplayText("BT Error " + ex.Message, DisplayStatusMessageTypes.Error, true);
                    }
                };

                _appRoot.DebugDisplayText(characteristic.Uuid + " registered", DisplayStatusMessageTypes.Debug);
            }

            _appRoot.DebugDisplayText("BT receivers registered", DisplayStatusMessageTypes.Important);
        }

        public void RefreshStatus()
        {
        }

        public void Test()
        {
        }
    }
}
