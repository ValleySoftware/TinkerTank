using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BurthaRemote.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        BluetoothLEHelper bluetoothLEHelper;
        public ObservableCollection<ObservableBluetoothLEDevice> bluetoothDevices = new ObservableCollection<ObservableBluetoothLEDevice>();
        private ObservableBluetoothLEDevice current;
        private Windows.System.DispatcherQueue dispatcherQueue;

        public MainView()
        {
            this.InitializeComponent();// Get a local copy of the context for easier reading

            bluetoothLEHelper = BluetoothLEHelper.Context;
            bluetoothLEHelper.EnumerationCompleted += BluetoothLEHelper_EnumerationCompleted;
            bluetoothDevices = new ObservableCollection<ObservableBluetoothLEDevice>();

            // From a UI thread, capture the DispatcherQueue once:
            dispatcherQueue = CoreApplication.MainView.DispatcherQueue;

        }

        public async void ListAvailableBluetoothDevices()
        {
                // Start the Enumeration
                bluetoothLEHelper.StartEnumeration();



            //bluetoothDevices = bluetoothLEHelper.BluetoothLeDevices;

                // At this point the user needs to select a device they want to connect to. This can be done by
                // creating a ListView and binding the bluetoothLEHelper collection to it. Once a device is found, 
                // the Connect() method can be called to connect to the device and start interacting with its services

                // Connect to a device if your choice
                //ObservableBluetoothLEDevice device = bluetoothLEHelper.BluetoothLeDevices[< Device you choose >];
                //await device.ConnectAsync();

                // At this point the device is connected and the Services property is populated.

                // See all the services
                //var services = device.Services;
        }

        private void ConnectToBTEDevice(ObservableBluetoothLEDevice deviceToConnectTo)
        {
            if (deviceToConnectTo != null)
            {
                deviceToConnectTo.ConnectAsync();

                if (deviceToConnectTo.IsConnected)
                {
                    current = deviceToConnectTo;
                    var d = new ContentDialog();
                    d.Title = "Bluetooth Connected";
                    d.Content = "Connection to " + current.Name + " was successful.";
                    d.ShowAsync();

                }
            }
        }

        private void BluetoothLEHelper_EnumerationCompleted(object sender, EventArgs e)
        {
            dispatcherQueue.EnqueueAsync(() =>            
            {
                bluetoothDevices.Clear();

                foreach (var element in bluetoothLEHelper.BluetoothLeDevices)
                {
                    bluetoothDevices.Add(element);
                }
            });
        }

        private void scanForBluetoothButton_Click(object sender, RoutedEventArgs e)
        {
            ListAvailableBluetoothDevices();
        }

        private void connectToDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            var fe = e.OriginalSource as FrameworkElement; 
            var vm = fe.DataContext as ObservableBluetoothLEDevice;

            if (vm != null)
            {
                dispatcherQueue.EnqueueAsync(() =>
                {
                    ConnectToBTEDevice(vm);
                });
            }
        }
    }
}
