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
using TinkerTank.Data;

namespace Communications
{
    public class BlueTooth : TinkerBase, ITinkerBase, ICommunication
    {

        private Definition PrimaryControlDefinition;
        private Service primaryControlService;
        private CharacteristicString charStop;
        public CharacteristicString charPanTilt;
        private CharacteristicString charPower;
        private CharacteristicString charAdvancedMove;
        private CharacteristicString charPanSweep;
        public CharacteristicString charForwardDistance;
        public CharacteristicString charPanTiltDistance;
        public CharacteristicString charLights;
        public CharacteristicString charLogging;
        F7Micro _device;

        private readonly bool UseExternalAntenna = false;

        public BlueTooth()
        {
            _appRoot = MeadowApp.Current;
            _device = MeadowApp.Device;
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
                    _appRoot.DebugDisplayText("Toggling on the external antenna.", LogStatusMessageTypes.Debug);
                    _device.SetAntenna(AntennaType.External);
                    _appRoot.DebugDisplayText("External antenna enabled.", LogStatusMessageTypes.Information);
                }
                PrepareCharacteristics();

                PrepareDefinition();

                _device.BluetoothAdapter.StartBluetoothServer(PrimaryControlDefinition);

                _appRoot.DebugDisplayText("BT Service started", LogStatusMessageTypes.Information);

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

        private void PrepareDefinition()
        {
            _appRoot.DebugDisplayText("PrepBLEDefinitions", LogStatusMessageTypes.Debug);

            primaryControlService =
                new Service(
                    BLEConstants.serviceName,
                    BLEConstants.serviceUuid,
                    charStop,
                    charPanTilt,
                    charPower,
                    charAdvancedMove,
                    charPanSweep,
                    charForwardDistance,
                    charPanTiltDistance,
                    charLights,
                    charLogging
                    );

            _appRoot.DebugDisplayText("BLE ControlService created", LogStatusMessageTypes.Debug);

            foreach (var element in primaryControlService.Characteristics)
            {
                //Loop through and set the 'read' property to the characteristics name.
                try
                {
                    if (element is CharacteristicString)
                    {
                        UpdateCharacteristicValue(element, element.Name);
                    }
                }
                catch (Exception ex)
                {
                    _appRoot.DebugDisplayText("BLE Prepare Exception: " + ex.Message, LogStatusMessageTypes.Error);
                }
            }

            _appRoot.DebugDisplayText("Read properties set on BLE Chars", LogStatusMessageTypes.Debug);

            PrimaryControlDefinition = new Definition(
                BLEConstants.definitionName,
                primaryControlService
                );

            _appRoot.DebugDisplayText("BLEDefinition init complete", LogStatusMessageTypes.Debug);
        }

        private void PrepareCharacteristics()
        {

            _appRoot.DebugDisplayText("PrepBLECharacturistics", LogStatusMessageTypes.Information);

            charStop = new CharacteristicString(
                            name: CharacteristicsNames.Stop.ToString(),
                            uuid: BLEConstants.UUIDStop,
                            maxLength: 12,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            descriptors: new Descriptor(BLEConstants.UUIDStop, CharacteristicsNames.Stop.ToString())
                            );
            charPanTilt = new CharacteristicString(
                            name: CharacteristicsNames.PanTilt.ToString(),
                            uuid: BLEConstants.UUIDPanTilt,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(BLEConstants.UUIDPanTilt, CharacteristicsNames.PanTilt.ToString())
                            );
            charPower = new CharacteristicString(
                            name: CharacteristicsNames.Power.ToString(),
                            uuid: BLEConstants.UUIDPower,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(BLEConstants.UUIDPower, CharacteristicsNames.Power.ToString())
                            );
            charAdvancedMove = new CharacteristicString( //00-000-00000
                            name: CharacteristicsNames.AdvancedMove.ToString(),
                            uuid: BLEConstants.UUIDAdvancedMove,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(BLEConstants.UUIDAdvancedMove, CharacteristicsNames.AdvancedMove.ToString())
                            );
            charPanSweep = new CharacteristicString(
                            name: CharacteristicsNames.PanSweep.ToString(),
                            uuid: BLEConstants.UUIDPanSweep,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(BLEConstants.UUIDPanSweep, CharacteristicsNames.PanSweep.ToString())
                            );
            charForwardDistance = new CharacteristicString(
                            name: CharacteristicsNames.ForwardDistance.ToString(),
                            uuid: BLEConstants.UUIDForwardDistance,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(BLEConstants.UUIDForwardDistance, CharacteristicsNames.ForwardDistance.ToString())
                            );
            charPanTiltDistance = new CharacteristicString(
                            name: CharacteristicsNames.PanTiltDistance.ToString(),
                            uuid: BLEConstants.UUIDPanTiltDistance,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(BLEConstants.UUIDPanTiltDistance, CharacteristicsNames.PanTiltDistance.ToString())
                            );
            charLights = new CharacteristicString(
                            name: CharacteristicsNames.Lights.ToString(),
                            uuid: BLEConstants.UUIDLights,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 12,
                            descriptors: new Descriptor(BLEConstants.UUIDLights, CharacteristicsNames.Lights.ToString())
                            );
            charLogging = new CharacteristicString(
                            name: CharacteristicsNames.Logging.ToString(),
                            uuid: BLEConstants.UUIDLogging,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 128,
                            descriptors: new Descriptor(BLEConstants.UUIDLogging, CharacteristicsNames.Logging.ToString())
                            );
        }

        private void PrepareCharacteristicEventHandlers()
        {
            _appRoot.DebugDisplayText("PrepBLEHandlers", LogStatusMessageTypes.Debug);

            foreach (var characteristic in primaryControlService.Characteristics)
            {
                characteristic.ValueSet += (c, d) =>
                {
                    
                    _appRoot.DebugDisplayText("Received ble msg", LogStatusMessageTypes.Information);

                    string payload = string.Empty;

                    try
                    {
                        payload = d.ToString();
                    }
                    catch (Exception dataEx)
                    {
                        _appRoot.DebugDisplayText("error at BT receive" + dataEx.Message, LogStatusMessageTypes.Error);
                    }

                    _appRoot.DebugDisplayText("Received " + c.Name + " with " + payload, LogStatusMessageTypes.BLERecord);

                    string[] payloadSplit = payload.Split(BLEConstants.BLEMessageDivider);

                    try
                    {      
                        switch (c.Name)
                        {
                            case "Stop": RequestStop(payloadSplit); break;
                            case "PanTilt": RequestPanTilt(payloadSplit); break;
                            case "Power": RequestPower(payloadSplit); break;
                            case "AdvancedMove": RequestAdvancedMove(payloadSplit); break;
                            case "PanSweep": RequestPanSweep(payloadSplit); break;
                            case "ForwardDistance": RequestFrontDistanceUpdate(payloadSplit); break;
                            case "PanTiltDistance": RequestPanTiltDistanceUpdate(payloadSplit); break;
                            case "Lights": RequestLights(payloadSplit); break;
                            case "Logging": RequestLogUpdate(payloadSplit); break;
                            default: RequestStop(payloadSplit); break;
                        }

                        Status = ComponentStatus.Ready;
                    }
                    catch (Exception ex)
                    {
                        Status = ComponentStatus.Error;
                        _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error);
                    }
                };

                _appRoot.DebugDisplayText(characteristic.Uuid + " registered", LogStatusMessageTypes.Debug);
            }

            _appRoot.DebugDisplayText("BT receivers registered", LogStatusMessageTypes.Information);
        }

        private void RequestPower(string[] payloadSplit)
        {
            try
            {
                _appRoot.DebugDisplayText("Request Power received with: " + payloadSplit[1], LogStatusMessageTypes.BLERecord, payloadSplit[0]);

                var valueAsInt = Convert.ToInt32(payloadSplit[1]);

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
                _appRoot.DebugDisplayText("Request Power exception: Payload = " + payloadSplit[1] + " - Error details " + ex.Message, LogStatusMessageTypes.Error, payloadSplit[0]);
            }
        }

        private void RequestPanTilt(string[] payloadSplit)
        {
            //RemoteMsgID-0-PanTo-TiltTo-Speed
            //xxxxxxxx-0-000-000-0

            try
            {        
                if (payloadSplit.Count() >= 4)
                {

                    int pan = Convert.ToInt32(payloadSplit[2]);
                    int tilt = Convert.ToInt32(payloadSplit[3]);
                    ServoMovementSpeed speed = ServoMovementSpeed.Flank;
                    if (payloadSplit.Count() == 5)
                    {
                        int s = Convert.ToInt32(payloadSplit[4]);
                        speed = (ServoMovementSpeed)s;
                    }

                    _appRoot.DebugDisplayText(pan.ToString() + " " + tilt.ToString() + " " + speed.ToString(), LogStatusMessageTypes.BLERecord, payloadSplit[0]);

                    _appRoot.panTiltSensorCombo.Move(new Angle(pan), new Angle(tilt), speed);
                }
            }
            catch (Exception decipherPanTiltEx)
            {
                _appRoot.DebugDisplayText("DecyipherError: Payload = " + payloadSplit[1], LogStatusMessageTypes.Error);
                _appRoot.DebugDisplayText("DecyipherError: Exception= " + decipherPanTiltEx, LogStatusMessageTypes.Error);
            }
        }

        private void RequestPanSweep(string[] payloadSplit)
        {
            //Device-Speed
            //0-0

            try
            {
                int device = Convert.ToInt32(payloadSplit[1]);
                ServoMovementSpeed speed = ServoMovementSpeed.Flank;

                int s = Convert.ToInt32(payloadSplit[2]);
                speed = (ServoMovementSpeed)s;

                _appRoot.DebugDisplayText(device.ToString() + " Pan Sweep " + speed.ToString(), LogStatusMessageTypes.BLERecord, payloadSplit[0]);

                _appRoot.panTiltSensorCombo.AutoPanSweep(speed);

            }
            catch (Exception decipherPanTiltEx)
            {
                _appRoot.DebugDisplayText("DecyipherError: Payload = " + payloadSplit[1], LogStatusMessageTypes.Error);
                _appRoot.DebugDisplayText("DecyipherError: Exception= " + decipherPanTiltEx, LogStatusMessageTypes.Error);
            }
        }

        private void RequestStop(string[] payloadSplit)
        {
            try
            {
                _appRoot.movementController.Move(Direction.Stop, 0, TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("Request Stop exception: " + ex.Message, LogStatusMessageTypes.Error, payloadSplit[0]);
            }
        }

        private void RequestAdvancedMove(string[] payloadSplit)
        {
            //movementDirection-powerPercent-durationInMilliseconds
            //00-000-00000

            if (payloadSplit.Count() == 4)
            {
                try
                {
                    int direction = Convert.ToInt32(payloadSplit[1]);
                    int power = Convert.ToInt32(payloadSplit[2]);
                    int duration = Convert.ToInt32(payloadSplit[3]);

                    _appRoot.movementController.Move((Direction)direction, power, TimeSpan.FromMilliseconds(duration));
                }
                catch (Exception ex)
                {
                    _appRoot.DebugDisplayText("Request Advanced Move exception: " + ex.Message, LogStatusMessageTypes.Error, payloadSplit[0]);
                }
            }
        }

        private void RequestLogUpdate(string[] payloadSplit)
        {
            try
            {
                UpdateCharacteristicValue(charLogging, _appRoot.Logger.CurrentLog.Text);
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error, payloadSplit[0]);
            }
        }

        private void RequestLights(string[] payloadSplit)
        {
            try
            {
                _appRoot.LightsController.RequestLightsDo(payloadSplit[1]);
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error, payloadSplit[0]);
            }
        }

        private void RequestFrontDistanceUpdate(string[] payloadSplit)
        {
            try
            {
                _appRoot.distController.FixedFrontDistance.UpdateBleValue(null);
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error, payloadSplit[0]);
            }
        }

        private void RequestPanTiltDistanceUpdate(string[] payloadSplit)
        {
            try
            {
                _appRoot.distController.PeriscopeDistance.UpdateBleValue(null);
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error, payloadSplit[0]);
            }
        }

        public bool UpdateCharacteristicValue(ICharacteristic charToUpdate, object newValue)
        {
            var success = false;

            try
            {
                if (charToUpdate is CharacteristicString)
                {
                    string s = null;
                    if (newValue is string)
                    {
                        s = Convert.ToString(newValue);
                    }
                    if (newValue is DebugLogEntryModel)
                    {
                        s = ((DebugLogEntryModel)newValue).Text.ToString();
                    }

                    if (!string.IsNullOrEmpty(s))
                    {
                        if (s.Length >= charToUpdate.MaxLength)
                        {
                            s = s.Substring(0, charToUpdate.MaxLength - 1);
                        }
                        charToUpdate.SetValue(s);
                        success = true;
                    }
                }
                else
                {
                    UpdateCharacteristicValue(charToUpdate,newValue);
                }
            }
            catch (FormatException fe)
            {
                _appRoot.DebugDisplayText(fe.Message, LogStatusMessageTypes.Error);
            }

            return success;
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
