using MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp;

namespace BurthaRemote.ViewModels
{
    public class MainViewModel : ValleyBaseViewModel
    {
        private ObservableBluetoothLEDevice _currentDevice;
        private ObservableGattDeviceService _currentService;
        private ObservableGattCharacteristics _currentCharacteristic;
        public BluetoothLEHelper bluetoothLEHelper = BluetoothLEHelper.Context;

        public MainViewModel()
        {
            bluetoothLEHelper.EnumerationCompleted += BluetoothLEHelper_EnumerationCompleted;
        }

        public async void BluetoothLEHelper_EnumerationCompleted(object sender, EventArgs e)
        {
            await App.dispatcherQueue.EnqueueAsync(() =>
            {
                bluetoothLEHelper.StopEnumeration();

                Thinking = false;
            });
        }

        public bool IsNotNull(object toCheck)
        {
            return (toCheck != null);
        }

        public bool CountGreaterThanZero(ObservableCollection<ObservableBluetoothLEDevice> toCheck)
        {
            return (toCheck != null && toCheck.Count() > 0);
        }

        public bool CountGreaterThanZero(ObservableCollection<ObservableGattDeviceService> toCheck)
        {
            return (toCheck != null && toCheck.Count() > 0);
        }

        public ObservableGattCharacteristics CurrentCharacteristic
        {
            get => _currentCharacteristic;
            set => SetProperty(ref _currentCharacteristic, value);
        }

        public ObservableBluetoothLEDevice CurrentDevice
        {
            get => _currentDevice;
            set => SetProperty(ref _currentDevice, value);
        }

        public ObservableGattDeviceService CurrentService
        {
            get => _currentService;
            set => SetProperty(ref _currentService, value);
        }

        public void ListAvailableBluetoothDevices()
        {
            if (!Thinking)
            {
                Thinking = true;
                // Start the Enumeration
                bluetoothLEHelper.StartEnumeration();
            }
        }

        public async void ConnectToBTEDevice(ObservableBluetoothLEDevice deviceToConnectTo)
        {            
            try
            {
                if (deviceToConnectTo != null)
            {
                bluetoothLEHelper.StopEnumeration();
                Thinking = false;

                await deviceToConnectTo.ConnectAsync();

                    if (deviceToConnectTo.IsConnected)
                    {
                        CurrentDevice = deviceToConnectTo;
                        if (!deviceToConnectTo.IsPaired)
                        {
                            await deviceToConnectTo.DoInAppPairingAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public void SendMessage()
        {

        }
    }
}
