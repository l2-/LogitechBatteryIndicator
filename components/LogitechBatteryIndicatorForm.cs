using LogitechBatteryIndicator.components;
using LogitechBatteryIndicator.models;
using Label = System.Windows.Forms.Label;

namespace LogitechBatteryIndicator
{
    public partial class LogitechBatteryIndicator : Form
    {
        public static LogitechBatteryIndicator Instance { get; } = new LogitechBatteryIndicator();
        private readonly List<IBatteryUpdateListener> batteryUpdateListeners = [BatteryStatusLabel.Instance, BatteryIcon.Instance, TrayIcon.Instance];
        public bool StartMinimized { get; set; }

        private LogitechBatteryIndicator()
        {
            Icon = Properties.Resources.battery_default;
            InitializeComponent();
            Resize += OnResized;
            Load += OnLoad;
        }

        private void OnLoad(object? sender, EventArgs e)
        {
            if (StartMinimized)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        private void OnResized(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
            }
        }

        private static BatteryIcon CreateCanvas()
        {
            return BatteryIcon.Instance;
        }

        private void LogitechBatteryIndicator_Load(object sender, EventArgs e)
        {
            var panel = new TableLayoutPanel
            {
                Width = ClientSize.Width,
                Height = ClientSize.Height
            };
            Resize += (sender, e) => { panel.Width = ClientSize.Width; panel.Height = ClientSize.Height; };
            Controls.Add(panel);
            var status = BatteryStatusLabel.Instance;
            status.Anchor = AnchorStyles.Top;
            panel.Controls.Add(status);
            var paint = CreateCanvas();
            paint.Anchor = AnchorStyles.Top;
            panel.Controls.Add(paint);
            var toggleStartupControl = ToggleStartupControl.Instance;
            toggleStartupControl.Anchor = AnchorStyles.Top;
            panel.Controls.Add(toggleStartupControl);
        }

        public void RegisterBatteryUpdateListeners(ref EventHandler<BatteryUpdateEvent>? eventHandler)
        {
            eventHandler += (sender, e) =>
            {
                Console.WriteLine("Event {0}", e);
            };
            foreach (var item in batteryUpdateListeners)
            {
                eventHandler += item.OnBatteryUpdate;
            }
        }
    }
}
