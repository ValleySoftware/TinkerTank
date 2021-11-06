﻿using Base;
using Communications;
using Display;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Units;
using Peripherals;
using Servos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinkerTank.Movement;
using TinkerTank.Sensors;
using TinkerTank.Servos;
using Utilities.Power;

namespace TinkerTank
{
    public class PinAssignment
    {
        public int PinIndex { get; set; }
        public IPin PinLink { get; set; }
        public bool IsInUse { get; }
        public ITinkerBase UsedFor { get; set; }
        public BasePinType TypeOfPin { get; set; }
    }

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public List<PinAssignment> PinRegister;

        public IMovementInterface movementController;
        public PowerControl powerController;

        public LCDDisplay_ST7789 lcd;

        public BlueTooth communications;

        public PCA9685 i2CPWMController;

        public PushButton button;

        public readonly int PWMFrequency = 60;//50;

        public ArmControl Arm;

        public DistanceSensorController distController;

        public List<PanTiltBase> PanTilts = new List<PanTiltBase>();
        public PanTiltDistance DistancePanTilt;
        public PanTiltBase CameraPanTilt;

        public II2cBus i2CBus;

        IDigitalOutputPort blueLED;
        IDigitalOutputPort greenLED;
        IDigitalOutputPort redLED;
        ComponentStatus _status;

        private List<TinkerBase> TBObjects;
        private System.Timers.Timer _statusPoller;

        private PushButton nextLogButton;
        private PushButton previousLogButton;

        private bool EnableDistanceSensors = true;
        private bool EnablePanTiltDistance = true;
        private bool EnablePanTiltCamera = true;
        private bool EnableDisplay = false;
        private bool EnableArm = true;
        private bool EnableDisplayLogButtons = false;
        private bool EnablePCA9685 = true;

        public MeadowApp()
        {
            Init();
        }

        void Init()
        {
            try
            {
                TBObjects = new List<TinkerBase>();

                //InitialisePinRegister();

                //Display And Logging

                //Indicators to see what's going on
                blueLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
                greenLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen);
                redLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed);

                if (EnableDisplay)
                {
                    try
                    {
                        DebugDisplayText("start lcd", DisplayStatusMessageTypes.Important);
                        lcd = new LCDDisplay_ST7789(this);
                        TBObjects.Add(lcd);
                        lcd.Init();
                    }
                    catch (Exception broadLCDException)
                    {

                    }
                }

                if (EnableDisplayLogButtons)
                {
                    try
                    {
                        //nextLogButton = new PushButton(Device.CreateDigitalInputPort());
                        //previousLogButton = new PushButton();

                    }
                    catch (Exception broadLCDException)
                    {

                    }
                }

                //Communications and control

                DebugDisplayText("start communications controller", DisplayStatusMessageTypes.Important);
                communications = new BlueTooth(Device as F7Micro, this);
                TBObjects.Add(communications);
                communications.Init();

                //Shared

                DebugDisplayText("Init i2c");
                i2CBus = Device.CreateI2cBus();

                if (EnableDistanceSensors)
                {
                    DebugDisplayText("Init distance sensor controller");

                    distController = new DistanceSensorController(Device, this, ref i2CBus);

                    TBObjects.Add(distController);
                    distController.Init();
                }

                if (EnablePCA9685)
                {
                    DebugDisplayText("start pca9685", DisplayStatusMessageTypes.Important);
                    i2CPWMController = new PCA9685(this, ref i2CBus);
                    TBObjects.Add(i2CPWMController);
                    i2CPWMController.Init();

                    //Movement and power            
                }

                DebugDisplayText("start power controller", DisplayStatusMessageTypes.Important);
                powerController = new PowerControl(this);
                TBObjects.Add(powerController);
                powerController.Init(Device.Pins.D10);

                DebugDisplayText("start motor controller");
                movementController = new TrackControl(Device, i2CPWMController, this);
                TBObjects.Add((TinkerBase)movementController);

                movementController.Init(
                    Device.Pins.D13, Device.Pins.D12, Device.Pins.D11,
                    Device.Pins.D05, Device.Pins.D06, Device.Pins.D02);

                if (EnableArm)
                {
                    try
                    {
                        DebugDisplayText("Start Arm");
                        Arm = new ArmControl(this, i2CPWMController);
                        TBObjects.Add((TinkerBase)Arm);
                        Arm.Init();

                    }
                    catch (Exception broadLCDException)
                    {

                    }
                }   


                DebugDisplayText("Begining Camera and Sensor init", DisplayStatusMessageTypes.Important);
                //Camera and Sensor

                if (EnablePanTiltDistance)
                {
                    try
                    {
                        DistancePanTilt = new
                            PanTiltDistance(
                                this,
                                i2CPWMController,
                                "Range Finder");

                        PanTilts.Add(DistancePanTilt);
                        DistancePanTilt.Init(2, 3, distController.PeriscopeDistance);
                        DistancePanTilt.DefaultPan = new Angle(75); //Higher = Left
                        DistancePanTilt.DefaultTilt = new Angle(40); //?
                        DistancePanTilt.GoToDefault();
                    }
                    catch (Exception e)
                    {
                        DebugDisplayText("Distance Pan Tilt broad exception: " + e.Message, DisplayStatusMessageTypes.Error);
                    }
                }

                if (EnablePanTiltCamera)
                {
                    try
                {
                    CameraPanTilt = new
                        PanTiltBase(
                            this,
                            i2CPWMController,
                            "Forward Pan Tilt Camera");

                        PanTilts.Add(CameraPanTilt);
                        CameraPanTilt.Init(0, 1);
                        CameraPanTilt.DefaultPan = new Angle(110); //Higher = Left
                        CameraPanTilt.DefaultTilt = new Angle(120);  //Higher = down
                        CameraPanTilt.GoToDefault();
                    }
                    catch (Exception e)
                    {
                        DebugDisplayText("Camera Pan Tilt broad exception: " + e.Message, DisplayStatusMessageTypes.Error);
                    }
                }

                //Final

                DebugDisplayText("Begining regular polling", DisplayStatusMessageTypes.Important);
                _statusPoller = new System.Timers.Timer(2000);
                _statusPoller.Elapsed += _statusPoller_Elapsed;
                _statusPoller.AutoReset = true;
                _statusPoller.Enabled = true;

                DebugDisplayText("Startup Complete", DisplayStatusMessageTypes.Important);
            }
            catch (Exception iex)
            {
                DebugDisplayText("Main Init Exception: " + iex.Message, DisplayStatusMessageTypes.Error);
            }
        }

        private void InitialisePinRegister()
        {
            PinRegister = new List<PinAssignment>();

            PinRegister.Add(new PinAssignment() { PinIndex = 0 , TypeOfPin = BasePinType.analogue, PinLink = Device.Pins.A00 });
            PinRegister.Add(new PinAssignment() { PinIndex = 1, TypeOfPin = BasePinType.analogue, PinLink = Device.Pins.A01 });
            PinRegister.Add(new PinAssignment() { PinIndex = 2, TypeOfPin = BasePinType.analogue, PinLink = Device.Pins.A02 });
            PinRegister.Add(new PinAssignment() { PinIndex = 3, TypeOfPin = BasePinType.analogue, PinLink = Device.Pins.A03 });
            PinRegister.Add(new PinAssignment() { PinIndex = 4, TypeOfPin = BasePinType.analogue, PinLink = Device.Pins.A04 });
            PinRegister.Add(new PinAssignment() { PinIndex = 5, TypeOfPin = BasePinType.analogue, PinLink = Device.Pins.A05 });
            PinRegister.Add(new PinAssignment() { PinIndex = 0, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D00 });
            PinRegister.Add(new PinAssignment() { PinIndex = 1, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D01 });
            PinRegister.Add(new PinAssignment() { PinIndex = 2, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D02 });
            PinRegister.Add(new PinAssignment() { PinIndex = 3, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D03 });
            PinRegister.Add(new PinAssignment() { PinIndex = 4, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D04 });
            PinRegister.Add(new PinAssignment() { PinIndex = 5, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D05 });
            PinRegister.Add(new PinAssignment() { PinIndex = 6, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D06 });
            PinRegister.Add(new PinAssignment() { PinIndex = 7, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D07 });
            PinRegister.Add(new PinAssignment() { PinIndex = 8, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D08 });
            PinRegister.Add(new PinAssignment() { PinIndex = 9, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D09 });
            PinRegister.Add(new PinAssignment() { PinIndex = 10, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D10 });
            PinRegister.Add(new PinAssignment() { PinIndex = 11, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D11 });
            PinRegister.Add(new PinAssignment() { PinIndex = 12, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D12 });
            PinRegister.Add(new PinAssignment() { PinIndex = 13, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D13 });
            PinRegister.Add(new PinAssignment() { PinIndex = 14, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D14 });
            PinRegister.Add(new PinAssignment() { PinIndex = 15, TypeOfPin = BasePinType.digital, PinLink = Device.Pins.D15 });
            PinRegister.Add(new PinAssignment() { PinIndex = -1, TypeOfPin = BasePinType.scl, PinLink = Device.Pins.I2C_SCL });
            PinRegister.Add(new PinAssignment() { PinIndex = -1, TypeOfPin = BasePinType.sda, PinLink = Device.Pins.I2C_SDA });
        }

        private void _statusPoller_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshStatus();
        }

        public void RefreshStatus()
        {
            foreach (var element in TBObjects)
            {
                SetStatus(element.Status);

                if (element.Status == ComponentStatus.Error ||
                    element.Status == ComponentStatus.UnInitialised)
                {
                    switch (element.ErrorResponse)
                    {
                        case AutomaticErrorResponse.DoNothing: break;
                        case AutomaticErrorResponse.TryReload: break;
                        case AutomaticErrorResponse.Warn: break;
                        case AutomaticErrorResponse.DisableComponent: break;
                        case AutomaticErrorResponse.DisableMotorPower: try { powerController.Disconnect(); } catch (Exception) { }; break;
                    }

                    DebugDisplayText(element.GetType().ToString() + " inopperable.  : ", DisplayStatusMessageTypes.Error);
                    
                    break;
                }
            }
        }

        private void SetStatus(ComponentStatus newStatus)
        {
            if (newStatus != _status)
            {
                _status = newStatus;
        
                blueLED.State = false;
                greenLED.State = false;
                redLED.State = false;
        
                switch (newStatus)
                {
                    case ComponentStatus.Error:
                        redLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Error);
                        break;
                    case ComponentStatus.Action:
                        blueLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Important);
                        break;
                    case ComponentStatus.Ready:
                        greenLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Important);
                        break;
                    default:
                        break;
                }
        
        
            }
        }

        public void NextDebugMessage()
        {
            if (lcd != null && lcd.Log.Count > 0)
            {
                var oldIndex = lcd.Log.IndexOf(lcd.CurrentLog);

                if (oldIndex >= 0)
                {
                    ShowDebugMessage(oldIndex - 1);
                }
            }
        }

        public void PreviousDebugMessage()
        {
            if (lcd != null && lcd.Log.Count > 0)
            {
                var oldIndex = lcd.Log.IndexOf(lcd.CurrentLog);

                if  (oldIndex >= 0)
                {
                    ShowDebugMessage(oldIndex + 1);
                }
            }
        }

        public void FirstMessage()
        {
            if (lcd != null && lcd.Log.Count > 0)
            {
                lcd.CurrentLog = lcd.Log[lcd.Log.Count - 1];
            }
        }

        public void LastMessage()
        {
            if (lcd != null && lcd.Log.Count > 0)
            {
                lcd.CurrentLog = lcd.Log[0];
            }
        }

        public void ShowDebugMessage(int messageIndex)
        {
            if (lcd != null && lcd.Log.Count > messageIndex)
            {

            }
        }

        public void DebugDisplayText(string newText, DisplayStatusMessageTypes statusType = DisplayStatusMessageTypes.Debug)
        {
                var t = new Task(() =>
                {
                    Console.WriteLine(newText);
        
                    if (lcd != null)
                    {
                        try
                        {
                            lcd.AddMessage(newText, statusType);
                        }
                        catch (Exception)
                        {
                            //Display add process went through,but not talking.  Is it plugged in?
                        }
                    }
                    });
                t.Start();
        }
    }
}
