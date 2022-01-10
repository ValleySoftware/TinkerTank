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
        private CharacteristicInt32  charStop;
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

        private void RequestPower(string payload)
        {
            try
            {
                _appRoot.DebugDisplayText("Request Power received with: " + payload, LogStatusMessageTypes.BLERecord);

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
                _appRoot.DebugDisplayText("Request Power exception: Payload = " + payload + " - Error details " + ex.Message, LogStatusMessageTypes.Error);
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

                    _appRoot.DebugDisplayText(pan.ToString() + " " + tilt.ToString() + " " + speed.ToString(), LogStatusMessageTypes.BLERecord);

                    _appRoot.panTiltSensorCombo.Move(new Angle(pan), new Angle(tilt), speed);
                }
            }
            catch (Exception decipherPanTiltEx)
            {
                _appRoot.DebugDisplayText("DecyipherError: Payload = " + payload, LogStatusMessageTypes.Error);
                _appRoot.DebugDisplayText("DecyipherError: Exception= " + decipherPanTiltEx, LogStatusMessageTypes.Error);
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

                _appRoot.DebugDisplayText(device.ToString() + " Pan Sweep " + speed.ToString(), LogStatusMessageTypes.BLERecord);

                _appRoot.panTiltSensorCombo.AutoPanSweep(speed);
                
            }
            catch (Exception decipherPanTiltEx)
            {
                _appRoot.DebugDisplayText("DecyipherError: Payload = " + payload, LogStatusMessageTypes.Error);
                _appRoot.DebugDisplayText("DecyipherError: Exception= " + decipherPanTiltEx, LogStatusMessageTypes.Error);
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
                _appRoot.DebugDisplayText("Request Stop exception: " + ex.Message, LogStatusMessageTypes.Error);
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
                    _appRoot.DebugDisplayText("Request Advanced Move exception: " + ex.Message, LogStatusMessageTypes.Error);
                }
            }
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

            PrimaryControlDefinition = new Definition(
                BLEConstants.definitionName,
                primaryControlService
                );
        }

        private void PrepareCharacteristics()
        {

            _appRoot.DebugDisplayText("PrepBLECharacturistics", LogStatusMessageTypes.Information);

            charStop = new CharacteristicInt32(
                            name: CharacteristicsNames.Stop.ToString(),
                            uuid: BLEConstants.UUIDStop,
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
                    
                    _appRoot.DebugDisplayText("Received ble msg", LogStatusMessageTypes.BLERecord);

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
                            case "Logging": RequestLogUpdate(payload); break;
                            default: RequestStop(); break;
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

        private void RequestLogUpdate(string payload)
        {
            try
            {
                UpdateCharacteristicValue(charLogging, _appRoot.Logger.CurrentLog.Text);
            }
            catch (Exception ex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error);
            }
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
                _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error);
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
                _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error);
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
                _appRoot.DebugDisplayText("BT Error " + ex.Message, LogStatusMessageTypes.Error);
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

                        UpdateCharacteristicValue(charToUpdate, s);
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
