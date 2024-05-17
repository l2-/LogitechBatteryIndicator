using LogitechBatteryIndicator.models;

namespace LogitechBatteryIndicator.components
{
    internal class BatteryIcon : Panel, IDisposable, IBatteryUpdateListener
    {
        private static readonly uint _100p_rgb = 0xFF0ED145;
        private static readonly uint _40p_rgb = 0xFFF57F17;
        private static readonly uint _20p_rgb = 0xFFB71C1C;
        private static readonly Color _100p_color = Color.FromArgb((int)_100p_rgb);
        private static readonly Color _40p_color = Color.FromArgb((int)_40p_rgb);
        private static readonly Color _20p_color = Color.FromArgb((int)_20p_rgb);

        public static BatteryIcon Instance { get; } = new BatteryIcon();

        private Icon DefaultIcon { get; set; }
        private Bitmap IconBitmap { get; set; }
        private Icon Icon { get => Icon.FromHandle(IconBitmap.GetHicon()); }

        public new int Width { get => Icon.Width; }
        public new int Height { get => Icon.Height; }

        private BatteryIcon()
        {
            var icon = Properties.Resources.battery_default1;
            if (icon is null) { throw new ArgumentNullException(nameof(icon)); }
            DefaultIcon = new Icon(icon, int.MaxValue, int.MaxValue);
            IconBitmap = new Bitmap(DefaultIcon.Width, DefaultIcon.Height);
            base.Width = DefaultIcon.Width;
            base.Height = DefaultIcon.Height;

            DrawIcon(20);

            Paint += (sender, e) =>
            {
                Draw(e.Graphics);
            };
        }

        public void DrawIcon(int percentage)
        {
            void action()
            {
                IconBitmap = new Bitmap(DefaultIcon.Width, DefaultIcon.Height);
                using (Graphics g = Graphics.FromImage(IconBitmap))
                {
                    g.Clear(Color.Transparent);
                    g.DrawIcon(DefaultIcon, 0, 0);
                }

                var min_x = int.MaxValue; var max_x = 0;
                var min_y = int.MaxValue; var max_y = 0;
                for (int y = 0; y < IconBitmap.Height; y++)
                {
                    for (int x = 0; x < IconBitmap.Width; x++)
                    {
                        if (IconBitmap.GetPixel(x, y).G > 100)
                        {
                            min_x = Math.Min(min_x, x);
                            max_x = Math.Max(max_x, x);
                            min_y = Math.Min(min_y, y);
                            max_y = Math.Max(max_y, y);
                        }
                    }
                }
                var batteryWidth = max_x - min_x;
                var batteryHeight = max_y - min_y;

                var width = batteryWidth * (percentage / 100.0f);
                var color = _100p_color;
                if (percentage < 21) color = _20p_color;
                else if (percentage < 41) color = _40p_color;
                for (int y = 0; y < IconBitmap.Height; y++)
                {
                    for (int x = 0; x < IconBitmap.Width; x++)
                    {
                        if (IconBitmap.GetPixel(x, y).G > 100)
                        {
                            IconBitmap.SetPixel(x, y, Color.Transparent);
                            if (x < width + min_x)
                            {
                                IconBitmap.SetPixel(x, y, color);
                            }
                        }
                    }
                }

                using (Graphics g = Graphics.FromImage(IconBitmap))
                {
                    var text = string.Format("{0}%", percentage);
                    var font = new Font(Font.FontFamily, 30, FontStyle.Bold);
                    var metrics = TextRenderer.MeasureText(text, font);
                    var text_x = Width / 2 - metrics.Width / 2;
                    var text_y = min_y + batteryHeight / 2 - metrics.Height / 2;
                    g.DrawString(text, font, new SolidBrush(Color.White), new Point(text_x, text_y));
                }

                Invalidate();
            }
            if (SynchronizationContext.Current is not null)
            {
                action();
            }
            else
            {
                Invoke(action);
            }
        }

        public void Draw(Graphics g)
        {
            g.DrawIcon(Icon, 0, 0);
        }

        public new void Dispose()
        {
            Icon.Dispose();
            DefaultIcon.Dispose();
            Dispose();
        }

        public void OnBatteryUpdate(object? sender, BatteryUpdateEvent e)
        {
            DrawIcon(e.Percentage);
        }
    }
}
