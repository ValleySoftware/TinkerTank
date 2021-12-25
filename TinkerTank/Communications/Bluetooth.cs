using Base;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Gateways;
using Meadow.Gateways.Bluetooth;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkerTank;

namespace Communications
{
    public class BlueTooth : TinkerBase, ITinkerBase, ICommunication
    {

        public const string definitionName = "BerthaDefinition";
        public const string serviceName = "BerthaService";
        public const ushort serviceUuid = 41;

        public enum CharacteristicsNames { Stop, PanTilt, Power, AdvancedMove, PanSweep, ForwardDistance, PanTiltDistance, Lights};
        private enum CharacteristicsUUID { UUIDStop, UUIDPanTilt, UUIDPower, UUIDAdvancedMove, UUIDPanSweep, UUIDDistance, UUIDLights };

        private const string UUIDStop = @"017e99d6-8a61-11eb-8dcd-0242ac1a5100";
        private const string UUIDPanTilt = @"017e99d6-8a61-11eb-8dcd-0242ac1a5102";
        private const string UUIDPower = @"017e99d6-8a61-11eb-8dcd-0242ac1a5103";
        private const string UUIDAdvancedMove = @"017e99d6-8a61-11eb-8dcd-0242ac1a5104";
        private const string UUIDPanSweep = @"017e99d6-8a61-11eb-8dcd-0242ac1a5105";
        private const string UUIDForwardDistance = @"017e99d6-8a61-11eb-8dcd-0242ac1a5106";
        private const string UUIDPanTiltDistance = @"017e99d6-8a61-11eb-8dcd-0242ac1a5107";
        private const string UUIDLights = @"017e99d6-8a61-11eb-8dcd-0242ac1a5108";

        private Definition PrimaryControlDefinition;
        private Service primaryControlService;
        private CharacteristicInt32  charStop;
        public CharacteristicString charPanTilt;
        private CharacteristicString charPower;
        private CharacteristicString charAdvancedMove;
        private CharacteristicString charPanSweep;
        public CharacteristicString charForwardDistance;
        public CharacteristicString charPanTiltDistance;
        public CharacteristicString charLights;
        F7Micro _device;

        private readonly bool UseExternalAntenna = false;

        public BlueTooth(F7Micro device, MeadowApp appRoot)
        {
            _appRoot = appRoot;
            _device = device;
            Status = ComponentStatus.UnInitialised;
        }

        public ComponentStatus Init()
        {
            Status = ComponentStatus.UnInitialised;

            try
            {
                //This kills the bluetooth process.... to be investigated
                if (UseExternalAntenna)
                {
                    _appRoot.DebugDisplayText("Toggling on the external antenna.", DisplayStatusMessageTypes.Debug);
                    _device.SetAntenna(AntennaType.External);
                    _appRoot.DebugDisplayText("External antenna enabled.", DisplayStatusMessageTypes.Debug);
                }
                PrepareCharacteristics();

                PrepareDefinition();

                _device.BluetoothAdapter.StartBluetoothServer(PrimaryControlDefinition);

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
            try
            {
                _appRoot.DebugDisplayText("Request Power received with: " + payload, DisplayStatusMessageTypes.Debug);

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
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("Request Power exception: Payload = " + payload, DisplayStatusMessageTypes.Error);
            }
        }

        private void RequestPanTilt(string payload)
        {
            //PanTo-TiltTo-Speed
            //000-000-0

            try
            {
                var sp = payload.Split("-");

                if (sp.Count() >= 2)
                {

                    int pan = Convert.ToInt32(sp[1]);
                    int tilt = Convert.ToInt32(sp[2]);
                    ServoMovementSpeed speed = ServoMovementSpeed.Flank;
                    if (sp.Count() == 4)
                    {
                        int s = Convert.ToInt32(sp[3]);
                        speed = (ServoMovementSpeed)s;
                    }

                    _appRoot.DebugDisplayText(pan.ToString() + " " + tilt.ToString() + " " + speed.ToString(), DisplayStatusMessageTypes.Important);

                        _appRoot.panTiltSensorCombo.Move(PanTiltAxis.pan, new Angle(pan), speed);
                        _appRoot.panTiltSensorCombo.Move(PanTiltAxis.tilt , new Angle(tilt), speed);
                }
            }
            catch (Exception decipherPanTiltEx)
            {
                _appRoot.DebugDisplayText("DecyipherError: Payload = " + payload, DisplayStatusMessageTypes.Error);
                _appRoot.DebugDisplayText("DecyipherError: Exception= " + decipherPanTiltEx, DisplayStatusMessageTypes.Error);
            }
        }

        private void RequestPanSweep(string payload)
        {
            //Device-Speed
            //0

            try
            {

                    int device = Convert.ToInt32(payload);
                    ServoMovementSpeed speed = ServoMovementSpeed.Flank;

                        int s = Convert.ToInt32(payload);
                        speed = (ServoMovementSpeed)s;

                    _appRoot.DebugDisplayText(device.ToString() + " Pan Sweep " + speed.ToString(), DisplayStatusMessageTypes.Important);

                        _appRoot.panTiltSensorCombo.AutoPanSweep(speed);
                
            }
            catch (Exception decipherPanTiltEx)
            {
                _appRoot.DebugDisplayText("DecyipherError: Payload = " + payload, DisplayStatusMessageTypes.Error);
                _appRoot.DebugDisplayText("DecyipherError: Exception= " + decipherPanTiltEx, DisplayStatusMessageTypes.Error);
            }
        }

        private void RequestStop()
        {
            try
            {
                _appRoot.movementController.Move(Direction.Stop, 0, TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("Request Stop exception: " + ex.Message, DisplayStatusMessageTypes.Error);
            }
        }

        private void RequestAdvancedMove(string payload)
        {
            //movementDirection-powerPercent-durationInMilliseconds
            //00-000-00000

            var sp = payload.Split("-");

            if (sp.Count() == 3)
            {
                try
                {
                    int direction = Convert.ToInt32(sp[0]);
                    int power = Convert.ToInt32(sp[1]);
                    int duration = Convert.ToInt32(sp[2]);

                    _appRoot.movementController.Move((Direction)direction, power, TimeSpan.FromMilliseconds(duration));
                }
                catch (Exception ex)
                {
                    _appRoot.DebugDisplayText("Request Advanced Move exception: " + ex.Message, DisplayStatusMessageTypes.Error);
                }
            }
        }

        /*
        public void RequestUpdatePanTiltDistance(int newDistance = -1)
        {
            try
            {
                if (newDistance == -1)
                {
                    newDistance = _appRoot.distController.PeriscopeDistance.DistanceInMillimeters;
                }

                charPanTiltDistance.SetValue(newDistance.ToString());
                //_appRoot.DebugDisplayText("dist updated. " + newDistance, DisplayStatusMessageTypes.Important);
            }
            catch (Exception ex)
            {

            }
        }
        */
        private void PrepareDefinition()
        {
            _appRoot.DebugDisplayText("PrepBLEDefinitions", DisplayStatusMessageTypes.Debug);

            primaryControlService =
                new Service(
                    serviceName,
                    serviceUuid,
                    charStop,
                    charPanTilt,
                    charPower,
                    charAdvancedMove,
                    charPanSweep,
                    charForwardDistance,
                    charPanTiltDistance,
                    charLights
                    );

            foreach (var element in primaryControlService.Characteristics)
            {
                //Loop through and set the 'read' property to the characteristics name.
                try
                {
                    element.SetValue(element.Name);
                }
                catch (Exception ex)
                {
                    _appRoot.DebugDisplayText("BLE Prepare Exception: " + ex.Message, DisplayStatusMessageTypes.Error);
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
            charAdvancedMove = new CharacteristicString( //00-000-00000
                            name: CharacteristicsNames.AdvancedMove.ToString(),
                            uuid: UUIDAdvancedMove,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(UUIDAdvancedMove, CharacteristicsNames.AdvancedMove.ToString())
                            );
            charPanSweep = new CharacteristicString(
                            name: CharacteristicsNames.PanSweep.ToString(),
                            uuid: UUIDPanSweep,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(UUIDPanSweep, CharacteristicsNames.PanSweep.ToString())
                            );
            charForwardDistance = new CharacteristicString(
                            name: CharacteristicsNames.ForwardDistance.ToString(),
                            uuid: UUIDForwardDistance,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(UUIDForwardDistance, CharacteristicsNames.ForwardDistance.ToString())
                            );
            charPanTiltDistance = new CharacteristicString(
                            name: CharacteristicsNames.PanTiltDistance.ToString(),
                            uuid: UUIDPanTiltDistance,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(UUIDPanTiltDistance, CharacteristicsNames.PanTiltDistance.ToString())
                            );
            charLights = new CharacteristicString(
                            name: CharacteristicsNames.Lights.ToString(),
                            uuid: UUIDLights,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(UUIDLights, CharacteristicsNames.Lights.ToString())
                            );
        }

        private void PrepareCharacteristicEventHandlers()
        {
            _appRoot.DebugDisplayText("PrepBLEHandlers", DisplayStatusMessageTypes.Debug);

            foreach (var characteristic in primaryControlService.Characteristics)
            {
                characteristic.ValueSet += (c, d) =>
                {
                    
                    _appRoot.DebugDisplayText("Received ble msg", DisplayStatusMessageTypes.Debug);

                    string payload = string.Empty;

                    try
                    {
                        payload = d.ToString();
                    }
                    catch (Exception dataEx)
                    {
                        _appRoot.DebugDisplayText("error at BT receive" + dataEx.Message, DisplayStatusMessageTypes.Debug);
                    }

                    _appRoot.DebugDisplayText("Received " + c.Name + " with " + payload, DisplayStatusMessageTypes.Important);

                    try
                    {      
                        switch (c.Name)
                        {
                            case "Stop": RequestStop(); break;
                            case "PanTilt": RequestPanTilt(payload); break;
                            case "Power": RequestPower(payload); break;
                            case "AdvancedMove": RequestAdvancedMove(payload); break;
                            case "PanSweep": RequestPanSweep(payload); break;
                            case "ForwardDistance": RequestFrontDistanceUpdate(); break;
                            case "PanTiltDistance": RequestPanTiltDistanceUpdate(); break;
                            case "Lights": RequestLights(payload); break;
                            default: RequestStop(); break;
                        }

                        Status = ComponentStatus.Ready;
                    }
                    catch (Exception ex)
                    {
                        Status = ComponentStatus.Error;
                        _appRoot.DebugDisplayText("BT Error " + ex.Message, DisplayStatusMessageTypes.Error);
                    }
                };

                _appRoot.DebugDisplayText(characteristic.Uuid + " registered", DisplayStatusMessageTypes.Debug);
            }

            _appRoot.DebugDisplayText("BT receivers registered", DisplayStatusMessageTypes.Important);
        }

        private void RequestLights(string payload)
        {
            try
            {
                _appRoot.LightsController.RequestLightsDo(payload);
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("BT Error " + ex.Message, DisplayStatusMessageTypes.Error);
            }
        }

        private void RequestFrontDistanceUpdate()
        {
            try
            {
                _appRoot.distController.FixedFrontDistance.UpdateBleValue(null);
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("BT Error " + ex.Message, DisplayStatusMessageTypes.Error);
            }
        }

        private void RequestPanTiltDistanceUpdate()
        {
            try
            {
                _appRoot.distController.PeriscopeDistance.UpdateBleValue(null);
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("BT Error " + ex.Message, DisplayStatusMessageTypes.Error);
            }
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
