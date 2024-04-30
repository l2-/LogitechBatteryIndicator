using logi.hidppio;
using LogitechBatteryIndicator.components;
using LogitechBatteryIndicator.models;

namespace LogitechBatteryIndicator.controller
{
    internal class DeviceEngine : IDisposable
    {
        private readonly List<IMouseUpdateListener> mouseUpdateListeners = [TrayIcon.Instance];

        public static DeviceEngine Instance { get; } = new DeviceEngine();
        private readonly List<HidppDevice> devices = new();
        private readonly List<Mouse> mices = new();
        private readonly HidppDeviceProvider deviceProviderInstance = new HidppDeviceProvider();
        private Mouse? selectedMouse;
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
            deviceProviderInstance.DeviceAdded += (dev) =>
            {
                devices.Add(dev);
                if (dev.DeviceType is HidppDeviceType.MOUSE)
                {
                    var mouse = new Mouse(dev);
                    Task.Factory.StartNew(async () => { await OnMouseAdded(mouse); });
                }
            };
            deviceProviderInstance.DeviceRemoved += (dev) =>
            {
                var mouse = mices.FirstOrDefault(m => m.handle.Id == dev.Id);
                devices.RemoveAll(d => d.Id == dev.Id);
                if (mouse is not null)
                {
                    Task.Factory.StartNew(async () => { await OnMouseRemoved(mouse); });
                }

            };
            deviceProviderInstance.Start();

            mouseUpdateListeners.ForEach(listener => { MouseUpdate += listener.OnMouseUpdate; });
        }

        private async Task OnMouseRemoved(Mouse m)
        {
            mices.RemoveAll(_m => _m.handle.Id == m.handle.Id);
            Console.WriteLine("Disconnected {0}", m.Name);
            await OnConnectionChanged();
        }

        private async Task OnMouseAdded(Mouse m)
        {
            mices.Add(m);
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
                if (selectedMouse is null || !selectedMouse.handle.IsConnected || !mices.Any(m => m.handle.Id == selectedMouse.handle.Id))
                {
                    var newM = mices.First(m => m.handle.IsConnected);
                    await SetSelectedMouse(newM);
                }
            }
            await Debounce(action, 50).Invoke();
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
            deviceProviderInstance.Dispose();
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
