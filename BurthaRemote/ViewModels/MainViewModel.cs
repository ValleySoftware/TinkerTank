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
        BluetoothLEHelper bluetoothLEHelper;
        public ObservableCollection<ObservableBluetoothLEDevice> bluetoothDevices = new ObservableCollection<ObservableBluetoothLEDevice>();
        public ObservableBluetoothLEDevice Current;

        public MainViewModel()
        {
            bluetoothLEHelper = BluetoothLEHelper.Context;
            bluetoothLEHelper.EnumerationCompleted += BluetoothLEHelper_EnumerationCompleted;
            bluetoothDevices = new ObservableCollection<ObservableBluetoothLEDevice>();
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
            if (!Thinking)
            {
                try
                {
                    Thinking = true;
                    if (deviceToConnectTo != null)
                    {
                        await deviceToConnectTo.ConnectAsync();

                        if (deviceToConnectTo.IsConnected)
                        {
                            Current = deviceToConnectTo;
                            var d = new ContentDialog();
                            d.Title = "Bluetooth Connected";
                            d.Content = "Connection to " + Current.Name + " was successful.";
                            d.PrimaryButtonText = "ok";
                            d.IsPrimaryButtonEnabled = true;
                            await d.ShowAsync();
                        }
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    Thinking = false;
                }
            }
        }

        public void BluetoothLEHelper_EnumerationCompleted(object sender, EventArgs e)
        {
                App.dispatcherQueue.EnqueueAsync(() =>
                {
                    bluetoothDevices.Clear();

                    foreach (var element in bluetoothLEHelper.BluetoothLeDevices)
                    {
                        bluetoothDevices.Add(element);
                    }

                    Thinking = false;
                });
        }
    }
}
