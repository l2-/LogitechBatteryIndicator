using logi.hidppio;
using LogitechBatteryIndicator.components;
using LogitechBatteryIndicator.models;
using System.Collections.Generic;

namespace LogitechBatteryIndicator.controller
{
    internal class DeviceEngine : IDisposable
    {
        private readonly List<IMouseUpdateListener> mouseUpdateListeners = [TrayIcon.Instance];

        public static DeviceEngine Instance { get; } = new DeviceEngine();
        private readonly List<HidppDevice> devices = [];
        private readonly List<Mouse> mices = [];
        private HidppDeviceProvider? deviceProviderInstance;
        private volatile Mouse? selectedMouse;
        private bool isInit = false;

        public EventHandler<BatteryUpdateEvent>? BatteryUpdate;
        public EventHandler<MouseUpdateEvent>? MouseUpdate;

        private readonly Action<HidppDevice, bool> onConnectionChangedListener = async (sender, e) =>
        {
            await Instance.OnConnectionChanged();
        };

        public void Init()
        {
            if (!isInit) { isInit = true; } else return;
            Console.WriteLine("Init logitech device engine");
            devices.Clear();
            deviceProviderInstance ??= new HidppDeviceProvider();
            deviceProviderInstance.DeviceAdded += OnDeviceAdded;
            deviceProviderInstance.DeviceRemoved += OnDeviceRemoved;
            deviceProviderInstance.Start();
            Task.Delay(500).ContinueWith(_ =>
            {
                if (selectedMouse is null)
                {
                    var m_devices = deviceProviderInstance.GetType().GetField("m_devices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                        .GetValue(deviceProviderInstance) as Dictionary<string, HidppDevice> ?? [];
                    foreach(var m_device in m_devices.Values)
                    {
                        OnDeviceAdded(m_device);
                    }
                }
            });

            mouseUpdateListeners.ForEach(listener => { MouseUpdate += listener.OnMouseUpdate; });
        }

        private void OnDeviceAdded(HidppDevice dev)
        {
            lock (devices)
            {
                devices.Add(dev);
            }
            if (dev.DeviceType is HidppDeviceType.MOUSE)
            {
                var mouse = new Mouse(dev);
                Task.Factory.StartNew(async () => { await OnMouseAdded(mouse); });
            }
        }

        private void OnDeviceRemoved(HidppDevice dev)
        {
            Mouse? mouse;
            lock (mices)
            {
                mouse = mices.FirstOrDefault(m => m.handle.Id == dev.Id);
            }
            lock (devices)
            {
                devices.RemoveAll(d => d.Id == dev.Id);
            }
            if (mouse is not null)
            {
                Task.Factory.StartNew(async () => { await OnMouseRemoved(mouse); });
            }
        }

        private async Task OnMouseRemoved(Mouse m)
        {
            lock (mices)
            {
                mices.RemoveAll(_m => _m.handle.Id == m.handle.Id);
            }
            Console.WriteLine("Disconnected {0}", m.Name);
            await OnConnectionChanged();
        }

        private async Task OnMouseAdded(Mouse m)
        {
            lock (mices)
            {
                mices.Add(m);
            }
            if (m.handle.IsConnected)
            {
                await m.LoadInfo();
                Console.WriteLine("Connected {0}", m.Name);
                await OnConnectionChanged();
            }
        }

        private async Task OnConnectionChanged()
        {
            async Task action()
            {
                Mouse? newMouse;
                bool lostSelectedMouse;
                lock (mices)
                {
                    lostSelectedMouse = selectedMouse is null || !selectedMouse.handle.IsConnected || !mices.Any(m => m.handle.Id == selectedMouse?.handle.Id);
                    newMouse = mices.First(m => m.handle.IsConnected);
                }
                if (lostSelectedMouse)
                {
                    await SetSelectedMouse(newMouse);
                }
            }
            await Debounce(action, 500).Invoke();
        }

        private async Task SetSelectedMouse(Mouse? m)
        {
            if (selectedMouse is not null)
            {
                await selectedMouse.UnsubscribeToBatteryEvents();
                selectedMouse.handle.ConnectionChanged -= onConnectionChangedListener;
                if (m is not null) Console.WriteLine("Swapped {0}", m.Name);
            }
            selectedMouse = m;
            MouseUpdate?.Invoke(this, new MouseUpdateEvent(selectedMouse));
            if (selectedMouse is not null)
            {
                selectedMouse.handle.ConnectionChanged += onConnectionChangedListener;
                _ = selectedMouse.SubscribeToBatteryEvents();
            }
        }

        public void Dispose()
        {
            deviceProviderInstance?.Dispose();
        }

        public static Func<Task> Debounce<T>(Func<T> func, int milliseconds = 300)
        {
            CancellationTokenSource? cancelTokenSource = null;

            return () =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                return Task.Delay(milliseconds, cancelTokenSource.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            func();
                        }
                    }, TaskScheduler.Default);
            };
        }
    }
}
