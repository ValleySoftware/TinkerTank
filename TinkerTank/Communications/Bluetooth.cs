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
        private float testMotorSpeed = 0.75f;

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
                        203,
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
                            "Pan",
                            maxLength: 10,
                            uuid: "017e99d6-8a61-11eb-8dcd-0242ac1300cc",
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read
                            ),
                        new CharacteristicString(
                            "Tilt",
                            maxLength: 10,
                            uuid: "017e99d6-8a61-11eb-8dcd-0242ac1300dd",
                            permissions: CharacteristicPermission.Write | CharacteristicPermission.Read,
                            properties: CharacteristicProperty.Write | CharacteristicProperty.Read
                            ),
                        new CharacteristicString(
                            "Power",
                            maxLength: 10,
                            uuid: "017e99d6-8a61-11eb-8dcd-0242ac1300ee",
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
                        _appRoot.DebugDisplayText($"HEY, I JUST GOT THIS BLE DATA for Characteristic '{c.Name}' of type {d.GetType().Name}: {d}");
                        int receivedData = -1;

                        receivedData = Convert.ToInt32(d);

                        try
                        {
                            _appRoot.DebugDisplayText("Received " + c.Name + " with " + receivedData);

                            if (c.Name.Equals("Stop"))
                            {
                                _appRoot.movementController.Stop();
                            }

                            if (c.Name.Equals("Move"))
                            {

                                switch (receivedData)
                                {
                                    case -1:
                                        _appRoot.movementController.Stop();
                                        break;
                                    case (int)Direction.Forward:
                                        _appRoot.movementController.Move(Direction.Forward, testMotorSpeed, new TimeSpan(0, 0, 0, 0, 500));
                                        break;
                                    case (int)Direction.Backwards:
                                        _appRoot.movementController.Move(Direction.Backwards, testMotorSpeed, new TimeSpan(0, 0, 0, 0, 500));
                                        break;
                                    case (int)Direction.TurnLeft:
                                        _appRoot.movementController.Move(Direction.TurnLeft, testMotorSpeed, new TimeSpan(0, 0, 0, 0, 500));
                                        break;
                                    case (int)Direction.TurnRight:
                                        _appRoot.movementController.Move(Direction.TurnRight, testMotorSpeed, new TimeSpan(0, 0, 0, 0, 500));
                                        break;
                                    case (int)Direction.RotateLeft:
                                        _appRoot.movementController.Move(Direction.RotateLeft, testMotorSpeed, new TimeSpan(0, 0, 0, 0, 500));
                                        break;
                                    case (int)Direction.RotateRight:
                                        _appRoot.movementController.Move(Direction.RotateRight, testMotorSpeed, new TimeSpan(0, 0, 0, 0, 500));
                                        break;
                                    default:
                                        _appRoot.DebugDisplayText("Received STOP with " + receivedData);
                                        break;
                                }
                            }

                            if (c.Name.Equals("Pan"))
                            {
                                
                            }

                            if (c.Name.Equals("Tilt"))
                            {
                                
                            }

                            if (c.Name.Equals("Power"))
                            {
                                if (receivedData == 1)
                                {
                                    _appRoot.powerController.Connect();
                                }
                                else
                                {
                                    _appRoot.powerController.Disconnect();
                                }
                            }

                            Status = ComponentStatus.Ready;
                        }
                        catch (Exception ex)
                        {
                            Status = ComponentStatus.Error;
                            _appRoot.DebugDisplayText("BT Error " + ex.Message);
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
