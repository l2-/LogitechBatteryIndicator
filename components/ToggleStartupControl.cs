using IWshRuntimeLibrary;
using File = System.IO.File;

namespace LogitechBatteryIndicator.components
{
    public sealed class ToggleStartupControl : CheckBox
    {
        private static readonly string app_name = "LogitechBatteryIndicator";
        public static ToggleStartupControl Instance { get; } = new ToggleStartupControl();

        private ToggleStartupControl()
        {
            Text = "Start this app on Windows startup";
            AutoSize = true;
            ForeColor = Color.White;
            Checked = StartupFileExists();
            CheckedChanged += OnCheckChanged;
        }

        private void OnCheckChanged(object? sender, EventArgs e)
        {
            if (Checked) AddToStartup();
            else RemoveFromStartup();
        }

        private static string StartupFilePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\" + app_name + ".lnk";
        }

        private static bool StartupFileExists()
        {
            return File.Exists(StartupFilePath());
        }

        private static void AddToStartup()
        {
            WshShell shell = new();
            string shortcutAddress = StartupFilePath();

            if (StartupFileExists()) { File.Delete(shortcutAddress); }

            System.Reflection.Assembly curAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            IWshShortcut shortcut = shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Logitech Battery Indicator";
            shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var targetPath = curAssembly.Location.Replace(".dll", ".exe");
            if (targetPath is null || targetPath.Equals(string.Empty))
            {
                targetPath = Application.ExecutablePath;
            }
            shortcut.TargetPath = targetPath;
            shortcut.Arguments = "--start-minimized";
            shortcut.Save();
        }

        private static void RemoveFromStartup()
        {
            if (StartupFileExists()) { File.Delete(StartupFilePath()); }
        }
    }
}
