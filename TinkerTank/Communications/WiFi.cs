using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinkerTank;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;
using Enumerations;
using Meadow.Gateway.WiFi;
using Meadow.Gateways;

namespace Communications
{
    public class WiFiComms : RequestHandlerBase, ITinkerBase, ICommunication
    {
        private int testMotorSpeed = 75;

        protected ComponentStatus _status = ComponentStatus.UnInitialised;
        protected MeadowApp _appRoot;
        private string _statusText = string.Empty;
        MapleServer server;
        F7Micro _device;

        public WiFiComms(F7Micro device, MeadowApp appRoot)
        {
            _appRoot = appRoot;
            _device = device;
        }

        public async Task InitWiFi(string networkName, string networkPassword)
        {
            _appRoot.DebugDisplayText("Initialize wifi...");

            // initialize the wifi adpater
            if (!MeadowApp.Device.InitWiFiAdapter().Result)
            {
                throw new Exception("Could not initialize the WiFi adapter.");
            }

            // connnect to the wifi network.
            Console.WriteLine($"Connecting to WiFi Network");
            var connectionResult = await MeadowApp.Device.WiFiAdapter.Connect(networkName, networkPassword); 
            if (connectionResult.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception($"Cannot connect to network: {connectionResult.ConnectionStatus}");
            }
            _appRoot.DebugDisplayText($"Connected. IP: {MeadowApp.Device.WiFiAdapter.IpAddress}");


            _appRoot.DebugDisplayText("Toggling on the external antenna.", DisplayStatusMessageTypes.Debug);
            _device.SetAntenna(AntennaType.External);
            _appRoot.DebugDisplayText("External antenna enabled.", DisplayStatusMessageTypes.Debug);

            // create our maple web server
            server = new MapleServer(
                MeadowApp.Device.WiFiAdapter.IpAddress,
                processMode: RequestProcessMode.Parallel
                );

            _appRoot.DebugDisplayText("wifi initialised.");

            server.Start();
            _appRoot.DebugDisplayText("wifi online.", DisplayStatusMessageTypes.Important);
        }

        [HttpGet]
        public void Stop()
        {
            _appRoot.movementController.Stop();
        }

        [HttpGet]
        public void Forward()
        {
            _appRoot.DebugDisplayText("ForwardReceived");

            var t = new Task(() =>
            {
                _appRoot.DebugDisplayText("ForwardRequested");
                _appRoot.movementController.Move(Direction.Forward, testMotorSpeed, TimeSpan.Zero);
                Thread.Sleep(250);
                _appRoot.movementController.Stop();
            });

            this.Context.Response.ContentType = ContentTypes.Application_Text;
            this.Context.Response.StatusCode = 200;
            this.Send("Forward").Wait();

            t.Start();
        }

        [HttpGet]
        public void Backward()
        {
            _appRoot.movementController.Move(Direction.Backwards, testMotorSpeed, TimeSpan.Zero);
            Thread.Sleep(250);
            Stop();
        }

        [HttpGet]
        public void RotateLeft()
        {
            _appRoot.movementController.Move(Direction.RotateLeft, testMotorSpeed, TimeSpan.Zero);
            Thread.Sleep(250);
            Stop();
        }

        [HttpGet]
        public void RotateRight()
        {
            _appRoot.movementController.Move(Direction.RotateRight, testMotorSpeed, TimeSpan.Zero);
            Thread.Sleep(250);
            Stop();
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


        public ComponentStatus Status
        {
            get => _status;
            set => _status = value;
        }

        public string StatusText
        {
            get => _statusText;
            set => _statusText = value;
        }
    }
}
