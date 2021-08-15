using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BurthaRemote.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        BluetoothLEHelper bluetoothLEHelper;
        public ObservableCollection<ObservableBluetoothLEDevice> bluetoothDevices;

        public MainView()
        {
            this.InitializeComponent();// Get a local copy of the context for easier reading

            bluetoothLEHelper = BluetoothLEHelper.Context;
            bluetoothDevices = new ObservableCollection<ObservableBluetoothLEDevice>();
        }

        public async void ListAvailableBluetoothDevices()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Start the Enumeration
                bluetoothLEHelper.StartEnumeration();

                foreach (var element in bluetoothLEHelper.BluetoothLeDevices)
                {
                    bluetoothDevices.Add(element);
                }
            });

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

        private void scanForBluetoothButton_Click(object sender, RoutedEventArgs e)
        {
            ListAvailableBluetoothDevices();
        }
    }
}
