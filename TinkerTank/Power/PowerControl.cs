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
                Status = ComponentStatus.Error;
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
            while (true)
            {
                Connect();
                
                Thread.Sleep(5000);

                Disconnect();
                Thread.Sleep(5000);
            }
        }

        public void ErrorEncountered()
        {

        }
    }
}
