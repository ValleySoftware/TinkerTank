using BurthaRemote.ViewModels;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BurthaRemote.UserControls
{
    public sealed partial class BluetoothUserControl : UserControl
    {
        MainViewModel mainViewModel => App.mainViewModel;

        public BluetoothUserControl()
        {
            this.InitializeComponent();
        }

        private void scanForBluetoothButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.ListAvailableBluetoothDevices();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.SendUtf8Message(mainViewModel.CurrentCharacteristic, messageTextBox.Text);
        }

        private void connectToDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            var fe = e.OriginalSource as FrameworkElement;
            var vm = fe.DataContext as ObservableBluetoothLEDevice;

            if (vm != null)
            {
                App.dispatcherQueue.EnqueueAsync(() =>
                {
                    mainViewModel.ConnectToBTEDevice(vm);
                });
            }
        }
    }
}
