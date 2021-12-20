using Base;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinkerTank;

namespace Utilities.Power
{
    public class PowerControl : TinkerBase, ITinkerBase
    {
        IDigitalOutputPort motorPowerRelayPort;

        public PowerControl(MeadowApp appRoot)
        {
            _appRoot = appRoot;
        }

        public ComponentStatus Init(IPin motorPowerRelayPin)
        {
            _appRoot.DebugDisplayText("Init Power Controller");

            Status = ComponentStatus.UnInitialised;

            try
            {
                motorPowerRelayPort = MeadowApp.Device.CreateDigitalOutputPort(motorPowerRelayPin);
                _appRoot.DebugDisplayText("Power Controller Ready");
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("Power Init Failed - " + ex.Message, DisplayStatusMessageTypes.Error);
                ErrorEncountered();
            }

            return Status;
        }

        public void RefreshStatus()
        {
        }

        public void Connect()
        {
            motorPowerRelayPort.State = true;
        }

        public void Disconnect()
        {
            motorPowerRelayPort.State = false;
        }

        public void Test()
        {
            try
            { 
                Connect();
                
                Thread.Sleep(5000);

                Disconnect();
                Thread.Sleep(5000);
            }
            catch (Exception e)
            {
                _appRoot.DebugDisplayText("Power Test Failed - " + e.Message, DisplayStatusMessageTypes.Error);
                ErrorEncountered();
            }
        }

        public void ErrorEncountered()
        {
            try
            {
                Status = ComponentStatus.Error;
                _appRoot.DebugDisplayText("Power ErrorEncountered method triggered", DisplayStatusMessageTypes.Error);
                Disconnect();
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("Power Safety Disconnect Failed - " + ex.Message, DisplayStatusMessageTypes.Error);
            }
        }
    }
}
