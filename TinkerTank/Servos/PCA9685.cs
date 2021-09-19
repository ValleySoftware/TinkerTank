using Base;
using Enumerations;
using Meadow;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkerTank;
using TinkerTank.Servos;

namespace Servos
{
    public class PCA9685 : TinkerBase, ITinkerBase
    {
        private Pca9685 pca9685;
        public PanTiltBase DriveCameraMovement;
        public PanTiltDistance PeriscopeCameraMovement;
        public List<Servo> Miscservos = new List<Servo>();
        public List<PanTiltBase> PanTilts = new List<PanTiltBase>();
        II2cBus Sharedi2cBus;

        public PCA9685(MeadowApp appRoot, ref II2cBus sharedi2cBus)
        {
            _appRoot = appRoot;
            Sharedi2cBus = sharedi2cBus;
        }

        public void Init()
        {
            _appRoot.DebugDisplayText("Init PCA9685 Device");
            
            pca9685 = new Pca9685(Sharedi2cBus, 0x40, _appRoot.PWMFrequency);
            pca9685.Initialize(); 

             DriveCameraMovement = new 
                PanTiltBase(
                    _appRoot, 
                    pca9685.CreatePwmPort(0), 
                    pca9685.CreatePwmPort(1), 
                    "DriveCamera", 
                    ServoType.SG90Standard);

            PanTilts.Add(DriveCameraMovement);
            DriveCameraMovement.Init();
            DriveCameraMovement.DefaultPan = 110;
            DriveCameraMovement.DefaultTilt = 100;
            DriveCameraMovement.GoToDefault();

            PeriscopeCameraMovement = new 
                PanTiltDistance(
                    _appRoot, 
                    pca9685.CreatePwmPort(2), 
                    pca9685.CreatePwmPort(3), 
                    "PeriscopeCamera", 
                    ServoType.SG90Standard,
                    MeadowApp.Device.Pins.D09);

            PanTilts.Add(PeriscopeCameraMovement);
            PeriscopeCameraMovement.Init();
            PeriscopeCameraMovement.DefaultPan = 55;
            PeriscopeCameraMovement.DefaultTilt = 40;
            PeriscopeCameraMovement.GoToDefault();

            Miscservos.Add(new Servo(pca9685.CreatePwmPort(15), PanTiltBase.Create996rConfig()));
            Miscservos[0].RotateTo(new Meadow.Units.Angle(10));

            Status = ComponentStatus.Ready;
        }

        public void RefreshStatus()
        {
            //
        }

        public void Test()
        {
        }


    }
}
