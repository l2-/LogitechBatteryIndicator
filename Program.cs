using LogitechBatteryIndicator.components;
using LogitechBatteryIndicator.controller;
using LogitechBatteryIndicator.helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LogitechBatteryIndicator
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool startMinimized = false;
            if (args.Contains("--console"))
            {
                AllocConsole();
            }
            if (args.Contains("--start-minimized"))
            {
                startMinimized = true;
            }

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Process currentProcess = Process.GetCurrentProcess();
            string productName = currentProcess.MainModule.FileVersionInfo.ProductName;
            string name = executingAssembly.GetName().Name;

            foreach (Process process in Process.GetProcesses())
            {
                if (!(process.MainWindowHandle == IntPtr.Zero))
                {
                    if (process.Id != currentProcess.Id)
                    {
                        try
                        {
                            if (process.MainModule.FileVersionInfo.ProductName == productName)
                            {
                                if (WindowHelpers.BringMainWindowToFront(process))
                                    return;
                            }
                        }
                        catch (Win32Exception ex)
                        {
                        }
                    }
                }
            }
            bool createdNew;
            using (new Mutex(true, "Global\\" + name, out createdNew))
            {
                if (!createdNew)
                    return;
                string str = Path.Combine(Path.GetTempPath(), name);
                using (new AssemblyLoader(str))
                {
                    ApplicationConfiguration.Initialize();
                    var form = LogitechBatteryIndicator.Instance;
                    form.StartMinimized = startMinimized;
                    form.Load += (sender, e) =>
                    {
                        form.RegisterBatteryUpdateListeners(ref DeviceEngine.Instance.BatteryUpdate);
                        DeviceEngine.Instance.Init();
                    };
                    Application.Run(form);
                    DeviceEngine.Instance.Dispose();
                    TrayIcon.Instance.Dispose();
                }
            }
        }
    }
}