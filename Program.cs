using LogitechBatteryIndicator.components;
using LogitechBatteryIndicator.controller;
using LogitechBatteryIndicator.helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LogitechBatteryIndicator
{
    internal static partial class Program
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool startMinimized = false;
            bool useConsole = false;
            if (args.Contains("--console"))
            {
                useConsole = true;
                AllocConsole();
            }
            if (args.Contains("--start-minimized"))
            {
                startMinimized = true;
            }

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Process currentProcess = Process.GetCurrentProcess();
            var productName = currentProcess.MainModule?.FileVersionInfo.ProductName;
            var name = executingAssembly.GetName().Name ?? string.Empty;

            using (new Mutex(true, "Global\\" + name, out bool createdNew))
            {
                if (!createdNew)
                {
                    var existingWindowHandle = WindowHelpers.WindowHandle(LogitechBatteryIndicator.Instance.Title);
                    if (existingWindowHandle != IntPtr.Zero)
                    {
                        WindowHelpers.RestoreWindow(existingWindowHandle);
                        var isInFront = WindowHelpers.BringMainWindowToFront(existingWindowHandle);
                        return;
                    }
                    MessageBox.Show("Another Logitech Battery Indicator process is already running. Please check the system tray and double click the icon there.", "Logitech Battery Indicator");
                    return;
                }
                string str = Path.Combine(Path.GetTempPath(), name);
                using (new AssemblyLoader(str))
                {
                    ApplicationConfiguration.Initialize();
                    var form = LogitechBatteryIndicator.Instance;
                    form.StartMinimized = startMinimized;
                    form.Load += (sender, e) =>
                    {
                        try
                        {
                            form.RegisterBatteryUpdateListeners(ref DeviceEngine.Instance.BatteryUpdate);
                            DeviceEngine.Instance.Init();
                        }
                        catch (Exception ex) { Console.Error.WriteLine(ex); }
                    };
                    Application.Run(form);
                    DeviceEngine.Instance.Dispose();
                    TrayIcon.Instance.Dispose();
                }
            }
        }
    }
}