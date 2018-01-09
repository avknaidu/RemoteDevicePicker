using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System.RemoteSystems;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace RemoteDevicePicker.Control
{
    public sealed class RemoteDevicePicker : ContentDialog
    {
        private Dictionary<string, RemoteSystem> DeviceMap { get; set; }
        private ListView _listDevices;
        private ComboBox _listDeviceTypes;
        private ProgressRing _progressRing;

        public ObservableCollection<RemoteSystem> RemoteSystems { get; private set; }

        public Brush HeaderLineColor
        {
            get { return (Brush)GetValue(HeaderLineColorProperty); }
            set { SetValue(HeaderLineColorProperty, value); }
        }

        public static readonly DependencyProperty HeaderLineColorProperty = DependencyProperty.Register(nameof(HeaderLineColor), typeof(Brush), typeof(RemoteDevicePicker), new PropertyMetadata(Application.Current.Resources["SystemControlBackgroundAccentBrush"]));

        public ListViewSelectionMode DeviceListSelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(DeviceListSelectionModeProperty); }
            set { SetValue(DeviceListSelectionModeProperty, value); }
        }

        public static readonly DependencyProperty DeviceListSelectionModeProperty = DependencyProperty.Register(nameof(DeviceListSelectionMode), typeof(ListViewSelectionMode), typeof(RemoteDevicePicker), new PropertyMetadata(ListViewSelectionMode.Single));

        public event EventHandler<RemoteDevicePickerEventArgs> RemoteDevicePickerClosed;

        public RemoteDevicePicker()
        {
            this.DefaultStyleKey = typeof(RemoteDevicePicker);
            this.SecondaryButtonText = "Done";
            this.SecondaryButtonClick += RemoteDevicePicker_SecondaryButtonClick;
            this.Loading += RemoteDevicePicker_Loading;
            this.Loaded += RemoteDevicePicker_Loaded;
        }

        private void RemoteDevicePicker_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ObservableCollection<RemoteSystem> selectedItems = new ObservableCollection<RemoteSystem>();
            foreach (RemoteSystem sys in _listDevices.SelectedItems)
            {
                selectedItems.Add(sys);
            }
            RemoteDevicePickerEventArgs eventArgs = new RemoteDevicePickerEventArgs(selectedItems);
            RemoteDevicePickerClosed?.Invoke(this, eventArgs);
            this.Hide();
        }

        protected override void OnApplyTemplate()
        {
            _listDevices = GetTemplateChild("PART_LISTDEVICES") as ListView;
            _listDeviceTypes = GetTemplateChild("PART_LISTDEVICETYPES") as ComboBox;
            _progressRing = GetTemplateChild("PART_PROGRESS") as ProgressRing;

            var _enumval = Enum.GetValues(typeof(DeviceType)).Cast<DeviceType>();
            _listDeviceTypes.ItemsSource = _enumval.ToList();
            _listDeviceTypes.SelectedIndex = 0;
            base.OnApplyTemplate();
        }

        private void _listDeviceTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void RemoteDevicePicker_Loaded(object sender, RoutedEventArgs e)
        {
            RemoteSystems = new ObservableCollection<RemoteSystem>();
            DeviceMap = new Dictionary<string, RemoteSystem>();
            _listDeviceTypes.SelectionChanged += _listDeviceTypes_SelectionChanged;
        }

        private async void RemoteDevicePicker_Loading(FrameworkElement sender, object args)
        {
            RemoteSystemAccessStatus accessStatus = await RemoteSystem.RequestAccessAsync();
            if (accessStatus == RemoteSystemAccessStatus.Allowed)
            {
                RemoteSystemWatcher m_remoteSystemWatcher = RemoteSystem.CreateWatcher();
                m_remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcher_RemoteSystemAdded;
                m_remoteSystemWatcher.RemoteSystemRemoved += RemoteSystemWatcher_RemoteSystemRemoved;
                m_remoteSystemWatcher.RemoteSystemUpdated += RemoteSystemWatcher_RemoteSystemUpdated;
                m_remoteSystemWatcher.Start();
            }
            _progressRing.IsActive = true;
            UpdateList();
        }

        private async void RemoteSystemWatcher_RemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _progressRing.IsActive = true;
                if (DeviceMap.ContainsKey(args.RemoteSystem.Id))
                {
                    RemoteSystems.Remove(DeviceMap[args.RemoteSystem.Id]);
                    DeviceMap.Remove(args.RemoteSystem.Id);
                }
                RemoteSystems.Add(args.RemoteSystem);
                DeviceMap.Add(args.RemoteSystem.Id, args.RemoteSystem);
                UpdateList();
            });
        }

        internal void UpdateList()
        {
            ObservableCollection<RemoteSystem> bindingList = new ObservableCollection<RemoteSystem>();
            if (RemoteSystems != null)
            {
                foreach (RemoteSystem sys in RemoteSystems)
                {
                    if (_listDeviceTypes.SelectedValue.ToString().Equals(DeviceType.All.ToString()))
                    {
                        bindingList = RemoteSystems;
                    }
                    else if (_listDeviceTypes.SelectedValue.ToString().Equals(sys.Kind))
                    {
                        bindingList.Add(sys);
                    }
                }
                _progressRing.IsActive = false;
            }
            _listDevices.ItemsSource = bindingList;
        }

        private async void RemoteSystemWatcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _progressRing.IsActive = true;
                if (DeviceMap.ContainsKey(args.RemoteSystemId))
                {
                    RemoteSystems.Remove(DeviceMap[args.RemoteSystemId]);
                    DeviceMap.Remove(args.RemoteSystemId);
                }
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _progressRing.IsActive = true;
                RemoteSystems.Add(args.RemoteSystem);
                DeviceMap.Add(args.RemoteSystem.Id, args.RemoteSystem);
            });
        }
    }

    public class KindToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string finalvalue = string.Empty;
            switch ((string)value)
            {
                case "Desktop":
                    finalvalue = "\xE770";
                    break;
                case "Phone":
                case "Unknown":
                    finalvalue = "\xE8EA";
                    break;
                case "Xbox":
                    finalvalue = "\xE990";
                    break;
                case "Tablet":
                    finalvalue = "\xE70A";
                    break;
                case "Laptop":
                    finalvalue = "\xE7F8";
                    break;
                default:
                    finalvalue = "\xE770";
                    break;
            }
            return finalvalue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public enum DeviceType
    {
        All,
        Desktop,
        Laptop,
        Phone,
        Xbox,
        Tablet,
        Unknown
    }
}
