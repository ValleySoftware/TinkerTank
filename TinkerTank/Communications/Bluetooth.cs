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
        public BlueTooth(MeadowApp appRoot)
        {
            _appRoot = appRoot;
        }

        public ComponentStatus Init()
        {
            Status = ComponentStatus.UnInitialised; 

            try
            {
                var definition = new Definition(
                    "Bertha",
                    new Service(
                        "BerthaService",
                        41,
                        new CharacteristicInt32(
                            BluetoothCharacturistics.CharacteristicsNames.Stop.ToString(),
                            uuid: BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.Stop).Value,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            descriptors: new Descriptor(BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.Stop).Value, "Stop")
                            ),
                        new CharacteristicString(
                            BluetoothCharacturistics.CharacteristicsNames.Move.ToString(),
                            uuid: BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.Move).Value,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.Move).Value, "Move")
                            ),
                        new CharacteristicString(
                            BluetoothCharacturistics.CharacteristicsNames.PanTilt.ToString(),
                            uuid: BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.PanTilt).Value,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.PanTilt).Value, "PanTilt")
                            ),
                        new CharacteristicString(
                            BluetoothCharacturistics.CharacteristicsNames.Power.ToString(),
                            uuid: BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.Power).Value,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.Power).Value, "Power")
                            ),
                        new CharacteristicString(
                            BluetoothCharacturistics.CharacteristicsNames.AdvancedMove.ToString(),
                            uuid: BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.AdvancedMove).Value,
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read,
                            maxLength: 20,
                            descriptors: new Descriptor(BluetoothCharacturistics.Characteristics.FirstOrDefault(k => k.Key == BluetoothCharacturistics.CharacteristicsNames.AdvancedMove).Value, "AdvancedMove")
                            )
                        )
                    );

                _appRoot.DebugDisplayText("BT Service registered", DisplayStatusMessageTypes.Important);

                MeadowApp.Device.InitCoprocessor();
                MeadowApp.Device.BluetoothAdapter.StartBluetoothServer(definition);

                _appRoot.DebugDisplayText("BT Service started", DisplayStatusMessageTypes.Important);

                foreach (var characteristic in definition.Services[0].Characteristics)
                {
                    characteristic.ValueSet += (c, d) =>
                    {
                        _appRoot.DebugDisplayText("Received ble msg" , DisplayStatusMessageTypes.Important, false);

                        //int receivedData = -1;
                        string advancedData = string.Empty;

                        try
                        {
                            advancedData = d.ToString();
                            //receivedData = Convert.ToInt32(d);
                        }
                        catch (Exception dataEx)
                        {
                            _appRoot.DebugDisplayText("error at BT receive" + dataEx.Message, DisplayStatusMessageTypes.Debug, false, false);
                        }

                        _appRoot.DebugDisplayText("Received " + c.Name + " with " + advancedData, DisplayStatusMessageTypes.Debug, false);

                        try
                        {

                            if (c.Name.Equals(BluetoothCharacturistics.CharacteristicsNames.Stop.ToString()))
                            {
                                _appRoot.movementController.Stop();
                            }

                            var testMotorDuration = new TimeSpan(0, 0, 0, 0, 250);

                            if (c.Name.Equals(BluetoothCharacturistics.CharacteristicsNames.Move.ToString()))
                            {

                                switch (Convert.ToInt32(advancedData))
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

                            if (c.Name.Equals(BluetoothCharacturistics.CharacteristicsNames.AdvancedMove.ToString()))
                            {
                                //movementDirection-powerPercent-durationInseconds
                                //00-000-000

                                var sp = advancedData.Split("-");

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

                            if (c.Name.Equals(BluetoothCharacturistics.CharacteristicsNames.PanTilt.ToString()))
                            {
                                //Device-PanTo-TiltTo-Speed
                                //00-000-000-0

                                try
                                {
                                    var sp = advancedData.Split("-");

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

                            if (c.Name.Equals(BluetoothCharacturistics.CharacteristicsNames.Power.ToString()))
                            {

                                var valueAsInt = Convert.ToInt32(advancedData);

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

                Status = ComponentStatus.Ready;
            }
            catch (Exception bex)
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText(bex.Message);
            }

            return Status;
        }

        public void RefreshStatus()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            throw new NotImplementedException();
        }
    }
}
