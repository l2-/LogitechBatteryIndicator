using logi.hidppio;

namespace LogitechBatteryIndicator.models
{
    public class Battery
    {
        public static BatteryUpdateEvent EventFromF1004Broadcast(Feature1004.BatteryStatus info)
        {
            var state = info.charging_status is Feature1004.CHARGING_STATUS.CHARGING ? BatteryMode.Charging : BatteryMode.Discharging;
            int stateOfCharge = info.state_of_charge;
            return new BatteryUpdateEvent(stateOfCharge, state);
        }

        public static BatteryUpdateEvent EventFromF1001Broadcast(Feature1001.BatteryInfo info, ulong productId)
        {
            var state = info.ext_power ? BatteryMode.Charging : BatteryMode.Discharging;
            int percent;
            if (info.charge_status == Feature1001.BATTERY_INFO.END_OF_CHARGE)
            {
                percent = 100;
            }
            else if (info.critical && state == BatteryMode.Discharging)
            {
                percent = 1;
            }
            else
            {
                float num1;
                float num2;
                float num3;
                float num4;
                float num5;
                float num6;
                float num7;
                float num8;
                float num9;
                float num10;
                switch ((ulong)productId)
                {
                    case 16511:
                    case 49281:
                    case 49282:
                    case 49287:
                    case 4649896869286117376:
                        num1 = 3.632f;
                        num2 = 2837.09f;
                        num3 = -2738.1084f;
                        num4 = 26431.2852f;
                        num5 = -85045.12f;
                        num6 = 94026.48f;
                        num7 = -18016.2539f;
                        num8 = 219290.1f;
                        num9 = -892579.75f;
                        num10 = 1215175.88f;
                        break;
                    default:
                        num1 = 3.655f;
                        num2 = 902.34f;
                        num3 = -15695.751f;
                        num4 = 166612.547f;
                        num5 = -589698.938f;
                        num6 = 696795.9f;
                        num7 = 8350.133f;
                        num8 = -95306.16f;
                        num9 = 360382.25f;
                        num10 = -450903.6f;
                        break;
                }
                float x = (float)info.battery_voltage / 1000f;
                double num11 = (double)x < (double)num1 ? (double)num3 : (double)num7;
                float num12 = (double)x < (double)num1 ? num4 : num8;
                float num13 = (double)x < (double)num1 ? num5 : num9;
                float num14 = (double)x < (double)num1 ? num6 : num10;
                double num15 = Math.Pow((double)x, 3.0);
                float num16 = (float)(num11 * num15 + (double)num12 * Math.Pow((double)x, 2.0) + (double)num13 * (double)x) + num14;
                int num17 = (int)float.Clamp(((float)(100.0 * ((double)(num2 - num16) / (double)num2))), 0.0f, 100f);
                percent = num17;
            }
            return new BatteryUpdateEvent(percent, state);
        }

        public static BatteryUpdateEvent EventFromF1000Broadcast(Feature1000.BatteryLevelStatus info)
        {
            var state = BatteryMode.Discharging;
            int batteryDischargeLevel = info.battery_discharge_level;
            var percent = batteryDischargeLevel;
            return new BatteryUpdateEvent(percent, state);
        }
    }
}
