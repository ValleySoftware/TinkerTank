using Base;
using Enumerations;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkerTank;

namespace Servos
{
    public class PCA9685 : TinkerBase, ITinkerBase
    {
        private Pca9685 pca9685;
        readonly int PWMFrequency = 50;
        public PanTiltBase DriveCameraMovement;
        public PanTiltBase PeriscopeCameraMovement;
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
            
            pca9685 = new Pca9685(Sharedi2cBus, 0x40, PWMFrequency);
            pca9685.Initialize();

            DriveCameraMovement = new 
                PanTiltBase(
                    _appRoot, 
                    pca9685.CreatePwmPort((byte)0), 
                    pca9685.CreatePwmPort((byte)1), 
                    "DriveCamera", 
                    ServoType.SG90Standard);

            PanTilts.Add(DriveCameraMovement);
            DriveCameraMovement.Init();
            DriveCameraMovement.DefaultPan = 110;
            DriveCameraMovement.DefaultTilt = 100;

            PeriscopeCameraMovement = new 
                PanTiltBase(
                    _appRoot, 
                    pca9685.CreatePwmPort((byte)2), 
                    pca9685.CreatePwmPort((byte)3), 
                    "PeriscopeCamera", 
                    ServoType.SG90Standard);

            PanTilts.Add(PeriscopeCameraMovement);
            PeriscopeCameraMovement.Init();
            PeriscopeCameraMovement.DefaultPan = 20;
            PeriscopeCameraMovement.DefaultTilt = 80;
            PeriscopeCameraMovement.GoToDefault();

            Miscservos.Add(new Servo(pca9685.CreatePwmPort((byte)15), PanTiltBase.Create996rConfig()));
            Miscservos[0].RotateTo(new Meadow.Units.Angle(40));

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
