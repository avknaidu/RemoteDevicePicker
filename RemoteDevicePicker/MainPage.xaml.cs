using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace App5
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RemoteDevicePicker.Control.RemoteDevicePicker remoteDevicePicker = new RemoteDevicePicker.Control.RemoteDevicePicker()
            {
                Title = "Pick Remote Device"
            };
            remoteDevicePicker.RemoteDevicePickerClosed += RemoteDevicePicker_RemoteDevicePickerClosed;
            await remoteDevicePicker.ShowAsync();
        }

        private async void RemoteDevicePicker_RemoteDevicePickerClosed(object sender, RemoteDevicePicker.Control.RemoteDevicePickerEventArgs e)
        {
            await new MessageDialog(e.Devices.Count.ToString()).ShowAsync();
        }
    }
}
