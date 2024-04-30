using LogitechBatteryIndicator.models;

namespace LogitechBatteryIndicator.components
{
    public sealed class TrayIcon : IDisposable, IBatteryUpdateListener, IMouseUpdateListener
    {
        private static readonly uint _100p_rgb = 0xFF0ED145;
        private static readonly uint _40p_rgb = 0xFFF57F17;
        private static readonly uint _20p_rgb = 0xFFB71C1C;
        private static readonly Color _100p_color = Color.FromArgb((int)_100p_rgb);
        private static readonly Color _40p_color = Color.FromArgb((int)_40p_rgb);
        private static readonly Color _20p_color = Color.FromArgb((int)_20p_rgb);

        private readonly string title_text = "Connected: {0}";
        private readonly string sub_text = "Battery status: {0} {1}%";
        public static TrayIcon Instance { get; } = new TrayIcon();
        private readonly NotifyIcon notifyIcon;
        private string mouseName = string.Empty;
        private int batteryPercentage = 0;
        private BatteryMode batteryMode = BatteryMode.Discharging;

        private Bitmap IconBitmap { get; set; }
        private Icon Icon { get => Icon.FromHandle(IconBitmap.GetHicon()); }

        private TrayIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.MouseDoubleClick += new MouseEventHandler(NotifyIconMouseDoubleClick);
            notifyIcon.Text = CreateText();

            IconBitmap = new Bitmap(16, 16);
            DrawIcon(20);

            Icon icon = Icon;
            notifyIcon.Icon = icon;
            notifyIcon.Visible = true;
        }

        public void DrawIcon(int percentage)
        {
            var batteryWidth = 16;
            var batteryHeight = 16;

            var color = _100p_color;
            if (percentage < 21) color = _20p_color;
            else if (percentage < 41) color = _40p_color;
            using Graphics g = Graphics.FromImage(IconBitmap);
            Font font = new("Microsoft Sans Serif", 12, FontStyle.Regular, GraphicsUnit.Pixel);
            g.Clear(Color.Transparent);
            var height = batteryHeight * (percentage / 100.0f);
            g.FillRectangle(new SolidBrush(color), 0, batteryHeight - height, batteryWidth, batteryHeight);
            g.DrawRectangle(new Pen(Color.Black, 1), new Rectangle(0, 0, batteryWidth - 1, batteryHeight - 1));
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            var text = percentage.ToString();
            var metrics = TextRenderer.MeasureText(text, font);
            var text_x = batteryWidth / 2 - metrics.Width / 2 + 1;
            var text_y = batteryHeight / 2 - metrics.Height / 2 - 1;
            g.DrawString(text, font, new SolidBrush(Color.Black), new Point(text_x, text_y));
        }

        void NotifyIconMouseDoubleClick(object? sender, MouseEventArgs e)
        {
            LogitechBatteryIndicator.Instance.ShowInTaskbar = true;
            LogitechBatteryIndicator.Instance.Show();
            LogitechBatteryIndicator.Instance.WindowState = FormWindowState.Normal;
        }

        public void Dispose()
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        private string CreateText()
        {
            var text = string.Empty;
            text += string.Format(title_text, mouseName);
            text += "\n";
            text += string.Format(sub_text, batteryMode, batteryPercentage);
            return text;
        }

        public void OnMouseUpdate(object? sender, MouseUpdateEvent e)
        {
            mouseName = e.Mouse?.Name ?? string.Empty;
            notifyIcon.Text = CreateText();
        }

        public void OnBatteryUpdate(object? sender, BatteryUpdateEvent e)
        {
            batteryMode = e.State;
            batteryPercentage = e.Percentage;
            notifyIcon.Text = CreateText();
            DrawIcon(batteryPercentage);
            notifyIcon.Icon = Icon;
        }
    }
}
