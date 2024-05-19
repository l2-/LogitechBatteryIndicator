using logi.hidppio;
using LogitechBatteryIndicator.controller;
using System.Windows.Forms;

namespace LogitechBatteryIndicator.models
{
    public sealed class Mouse(HidppDevice device)
    {
        public HidppDevice handle = device;

        public Version? ProtocolVersion { get; private set; }
        public long ProductId { get; private set; }
        public string Name { get; private set; } = string.Empty;

        private Action<Feature1004.BatteryStatus>? batteryStatusBroadcastListener;
        private Action<Feature1001.BatteryInfo>? batteryInfoBroadcastListener;
        private Action<Feature1000.BatteryLevelStatus>? batteryLevelStatusBroadcastListener;

        public async Task LoadInfo()
        {
            try
            {
                ProtocolVersion = await handle.ProtocolVersion;
                if (ProtocolVersion.Major >= 2)
                {
                    ProductId = (await handle.GetFeature<Feature0003>().GetDeviceInfo()).model_id;
                    Name = await handle.GetFeature<Feature0005>().GetDeviceName();
                }
                else
                {
                    ProductId = (long)handle.ProductId;
                }

                if (Name.Equals(string.Empty))
                {
                    Name = MouseProductId.GetDeviceNameOverride(ProductId);
                }
            }
            catch (TimeoutException) { }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", handle.Id, ex);
            }
        }

        public async Task SubscribeToBatteryEvents()
        {
            try
            {
                if (await handle.HasFeature(4100))
                {
                    Feature1004 feature = handle.GetFeature<Feature1004>();
                    if (batteryStatusBroadcastListener is null)
                    {
                        batteryStatusBroadcastListener = (broadcast) =>
                        {
                            DeviceEngine.Instance.BatteryUpdate?.Invoke(this, Battery.EventFromF1004Broadcast(broadcast));
                        };
                        feature.BatteryStatusBroadcast += batteryStatusBroadcastListener;
                    }
                    Feature1004.BatteryStatus status = feature.GetStatus();
                    DeviceEngine.Instance.BatteryUpdate?.Invoke(this, Battery.EventFromF1004Broadcast(status));
                }
                else if (await handle.HasFeature(4097))
                {
                    Feature1001 feature = handle.GetFeature<Feature1001>();
                    if (batteryInfoBroadcastListener is null)
                    {
                        batteryInfoBroadcastListener = (broadcast) =>
                        {
                            DeviceEngine.Instance.BatteryUpdate?.Invoke(this, Battery.EventFromF1001Broadcast(broadcast, ProductId));
                        };
                        feature.BatteryInfoBroadcast += batteryInfoBroadcastListener;
                    }
                    Feature1001.BatteryInfo batteryInfo = feature.GetBatteryInfo();
                    DeviceEngine.Instance.BatteryUpdate?.Invoke(this, Battery.EventFromF1001Broadcast(batteryInfo, ProductId));
                }
                else
                {
                    if (!await handle.HasFeature(4096))
                        return;
                    Feature1000 feature = handle.GetFeature<Feature1000>();
                    if (batteryLevelStatusBroadcastListener is null)
                    {
                        batteryLevelStatusBroadcastListener = (broadcast) =>
                        {
                            DeviceEngine.Instance.BatteryUpdate?.Invoke(this, Battery.EventFromF1000Broadcast(broadcast));
                        };
                        feature.BatteryLevelStatusBroadcast += batteryLevelStatusBroadcastListener;
                    }
                    Feature1000.BatteryLevelStatus batteryLevelStatus = feature.GetBatteryLevelStatus();
                    DeviceEngine.Instance.BatteryUpdate?.Invoke(this, Battery.EventFromF1000Broadcast(batteryLevelStatus));
                }
            }
            catch (TimeoutException) { }
            catch (Exception ex) 
            { 
                Console.WriteLine("{0} {1}", handle.Id, ex); 
            }
        }

        public async Task UnsubscribeToBatteryEvents()
        {
        }
    }
}
