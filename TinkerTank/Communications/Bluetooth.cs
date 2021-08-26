using Base;
using Enumerations;
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

        private const string UUIDStop = "017e99d6-8a61-11eb-8dcd-0242ac1a5100";
        private const string UUIDMove = "017e99d6-8a61-11eb-8dcd-0242ac1a5101";
        private const string UUIDPanTilt = "017e99d6-8a61-11eb-8dcd-0242ac1a5102";
        private const string UUIDPower = "017e99d6-8a61-11eb-8dcd-0242ac1a5103";
        private const string UUIDAdvancedMove = "017e99d6-8a61-11eb-8dcd-0242ac1a5104";

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

                PrepareDefinition();

                PrepareCharacteristics();

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
                    int speed = (int)ServoMovementSpeed.Flank;
                    if (sp.Count() == 4)
                    {
                        speed = Convert.ToInt32(sp[3]);
                    }

                    _appRoot.DebugDisplayText(device.ToString() + " " + pan.ToString() + " " + tilt.ToString() + " " + speed.ToString(), DisplayStatusMessageTypes.Important);

                    if (device < _appRoot.i2CPWMController.PanTilts.Count)
                    {
                        _appRoot.i2CPWMController.PanTilts[device].PanTo(pan, (ServoMovementSpeed)speed);
                        _appRoot.i2CPWMController.PanTilts[device].TiltTo(tilt, (ServoMovementSpeed)speed);
                    }
                }
            }
            catch (Exception decipherPanTiltEx)
            {

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
            //movementDirection-powerPercent-durationInseconds
            //00-000-000

            var sp = payload.Split("-");

            if (sp.Count() == 3)
            {
                try
                {

                    int direction = Convert.ToInt32(sp[0]);
                    int power = Convert.ToInt32(sp[1]);
                    double duration = Convert.ToDouble(sp[2]);

                    _appRoot.movementController.Move((Direction)direction, power, TimeSpan.FromSeconds(duration));
                }
                catch (Exception decipherAdvancedMoveEx)
                {

                }
            }
        }


        private void PrepareDefinition()
        {
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

            foreach(var element in primaryControlService.Characteristics)
            {
                //Loop through and set the 'read' property to the characteristics name.
                element.SetValue(element.Name);
            }

            PrimaryControlDefinition = new Definition(
                definitionName,
                primaryControlService
                );
        }

        private void PrepareCharacteristics()
        {

            _appRoot.DebugDisplayText("BT Service setup", DisplayStatusMessageTypes.Important);

            charStop = new CharacteristicInt32(
                            Convert.ToString(CharacteristicsNames.Stop),
                            uuid: UUIDStop,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            descriptors: new Descriptor(UUIDStop, CharacteristicsNames.Stop.ToString())
                            );
            charMove = new CharacteristicString(
                            Convert.ToString(CharacteristicsNames.Move),
                            uuid: UUIDMove,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(UUIDMove, CharacteristicsNames.Move.ToString())
                            );
            charPanTilt = new CharacteristicString(
                            Convert.ToString(CharacteristicsNames.PanTilt),
                            uuid: UUIDPanTilt,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(UUIDPanTilt, CharacteristicsNames.PanTilt.ToString())
                            );
            charPower = new CharacteristicString(
                            Convert.ToString(CharacteristicsNames.Power),
                            uuid: UUIDPower,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(UUIDPower, CharacteristicsNames.Power.ToString())
                            );
            charAdvancedMove = new CharacteristicString(
                            Convert.ToString(CharacteristicsNames.AdvancedMove),
                            uuid: UUIDAdvancedMove,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(UUIDAdvancedMove, CharacteristicsNames.AdvancedMove.ToString())
                            );
        }

        private void PrepareCharacteristicEventHandlers()
        {
            foreach (var characteristic in primaryControlService.Characteristics)
            {
                characteristic.ValueSet += (c, d) =>
                {

                    _appRoot.DebugDisplayText("Received ble msg", DisplayStatusMessageTypes.Important, false);

                    string payload = string.Empty;

                    try
                    {
                        payload = d.ToString();
                    }
                    catch (Exception dataEx)
                    {
                        _appRoot.DebugDisplayText("error at BT receive" + dataEx.Message, DisplayStatusMessageTypes.Debug, false, false);
                    }

                    _appRoot.DebugDisplayText("Received " + c.Name + " with " + payload, DisplayStatusMessageTypes.Debug, false);

                    try
                    {
                        int ci = -1; 

                        try
                        {
                            ci = Convert.ToInt32(c.Name);
                        }
                        catch (Exception e)
                        {

                        }

                        if (ci == -1)
                        {
                            return;
                        }

                        CharacteristicsNames requestedCharacteristic = (CharacteristicsNames)ci;

                        switch (requestedCharacteristic)
                        {
                            case CharacteristicsNames.Stop: RequestStop(); break;
                            case CharacteristicsNames.Move: RequestMove(payload); break;
                            case CharacteristicsNames.PanTilt: RequestPanTilt(payload); break;
                            case CharacteristicsNames.Power: RequestPower(payload); break;
                            case CharacteristicsNames.AdvancedMove: RequestAdvancedMove(payload); break;
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

                _appRoot.DebugDisplayText(characteristic.Uuid + " registered", DisplayStatusMessageTypes.Important);
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
