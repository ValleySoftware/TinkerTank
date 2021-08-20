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
using BurthaRemote.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BurthaRemote.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {

        MainViewModel mainViewModel => App.mainViewModel;

        public MainView()
        {
            this.InitializeComponent();// Get a local copy of the context for easier reading
        }

        private void scanForBluetoothButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.ListAvailableBluetoothDevices();
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
