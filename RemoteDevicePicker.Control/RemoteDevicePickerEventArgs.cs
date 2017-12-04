using System;
using System.Collections.ObjectModel;
using Windows.System.RemoteSystems;

namespace RemoteDevicePicker.Control
{
    public class RemoteDevicePickerEventArgs : EventArgs
    {
        public ObservableCollection<RemoteSystem> Devices { get; }
        internal RemoteDevicePickerEventArgs(ObservableCollection<RemoteSystem> devices)
        {
            Devices = devices;
        }
    }
}
