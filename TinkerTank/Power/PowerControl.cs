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

        public ComponentSatus Init(IPin motorPowerRelayPin)
        {
            _appRoot.DebugDisplayText("Init Power Controller");

            Status = ComponentSatus.UnInitialised;

            try
            {
                motorPowerRelayPort = MeadowApp.Device.CreateDigitalOutputPort(motorPowerRelayPin);
                Status = ComponentSatus.Ready;
                _appRoot.DebugDisplayText("Power Controller Ready");
            }
            catch (Exception ex)
            {
                Status = ComponentSatus.Error;
            }

            return Status;
        }

        public void RefreshStatus()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            while (true)
            {
                motorPowerRelayPort.State = false;
                Thread.Sleep(5000);

                motorPowerRelayPort.State = true;
                Thread.Sleep(5000);
            }
        }
    }
}
