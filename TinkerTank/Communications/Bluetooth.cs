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
                    "MEADOWBOT",
                    new Service(
                        "ServiceA",
                        42,
                        new CharacteristicInt32(
                            "Stop",
                            uuid: "017e99d6-8a61-11eb-8dcd-0242ac1300aa",
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read
                            ),
                        new CharacteristicString(
                            "Move",
                            maxLength: 10,
                            uuid: "017e99d6-8a61-11eb-8dcd-0242ac1300bb",
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read
                            ),
                        new CharacteristicString(
                            "PanTilt",
                            maxLength: 10,
                            uuid: "017e99d6-8a61-11eb-8dcd-0242ac1300cc",
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read
                            ),
                        new CharacteristicString(
                            "Power",
                            maxLength: 10,
                            uuid: "017e99d6-8a61-11eb-8dcd-0242ac1300ee",
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read
                            ),
                        new CharacteristicString(
                            "AdvancedMove",
                            maxLength: 10,
                            uuid: "017e99d6-8a61-11eb-8dcd-0242ac1300ff",
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read
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

                        int receivedData = -1;
                        string advancedData = string.Empty;

                        try
                        {
                            advancedData = d.ToString();
                            receivedData = Convert.ToInt32(d);
                        }
                        catch (Exception dataEx)
                        {

                        }

                        try
                        {
                            _appRoot.DebugDisplayText("Received " + c.Name + " with " + advancedData, DisplayStatusMessageTypes.Debug, true);

                            if (c.Name.Equals("Stop"))
                            {
                                _appRoot.movementController.Stop();
                            }

                            var testMotorDuration = new TimeSpan(0, 0, 0, 0, 250);

                            if (c.Name.Equals("Move"))
                            {

                                switch (receivedData)
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

                            if (c.Name.Equals("AdvancedMove"))
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

                            if (c.Name.Equals("PanTilt"))
                            {
                                //Device-PanTo-TiltTo
                                //00-000-000

                                try
                                {
                                    var sp = advancedData.Split("-");

                                    if (sp.Count() == 3)
                                    {

                                        int device = Convert.ToInt32(sp[0]);
                                        int pan = Convert.ToInt32(sp[1]);
                                        int tilt = Convert.ToInt32(sp[2]);

                                        _appRoot.DebugDisplayText(device.ToString() + " " + pan.ToString() + " " + tilt.ToString(), DisplayStatusMessageTypes.Important);

                                        if (device == 0)
                                        {
                                            _appRoot.i2CPWMController.ServoRotateTo(0, pan);
                                            _appRoot.i2CPWMController.ServoRotateTo(1, tilt);
                                        }
                                    }
                                }
                                catch (Exception decipherPanTiltEx)
                                {

                                }
                            }

                            if (c.Name.Equals("Power"))
                            {
                                switch (receivedData)
                                {
                                    case 0: _appRoot.powerController.Disconnect(); break;
                                    case 1: _appRoot.powerController.Connect(); break;
                                    default:
                                        {
                                            if (receivedData >= 100)
                                            {
                                                _appRoot.movementController.SetDefaultPower(100);
                                            }
                                            else
                                            {
                                                if (receivedData <= 2)
                                                {
                                                    _appRoot.movementController.SetDefaultPower(0);
                                                }
                                                else
                                                {
                                                    _appRoot.movementController.SetDefaultPower(receivedData);
                                                }
                                            }
                                        }
                                        break;
                                }
                                if (receivedData == 1)
                                {
                                    
                                }
                                else
                                {
                                    
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
