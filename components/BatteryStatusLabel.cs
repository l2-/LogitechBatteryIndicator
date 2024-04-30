using LogitechBatteryIndicator.models;

namespace LogitechBatteryIndicator.components
{
    internal class BatteryStatusLabel : Label, IBatteryUpdateListener
    {
        private readonly string text = "Battery status: {0} {1}%";

        public static BatteryStatusLabel Instance { get; } = new BatteryStatusLabel();

        private BatteryStatusLabel()
        {
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold);
            ForeColor = Color.White;
            Text = string.Format(text, BatteryMode.Discharging, 0);
            AutoSize = true;
            Invalidate();
        }

        public void SetStatus(BatteryMode mode, int percentage)
        {
            Text = string.Format(text, mode, percentage);
            Invalidate();
        }

        public void OnBatteryUpdate(object? sender, BatteryUpdateEvent e)
        {
            Invoke(() =>
            {
                Text = string.Format(text, e.State, e.Percentage);
                Invalidate();
            });
        }
    }
}
