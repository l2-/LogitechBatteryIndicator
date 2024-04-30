using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace LogitechBatteryIndicator.helpers
{
    internal class WindowHelpers
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, WindowHelpers.ShowWindowEnum flags);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        public static bool BringMainWindowToFront(Process bProcess)
        {
            if (bProcess.MainWindowHandle == IntPtr.Zero)
                WindowHelpers.ShowWindow(bProcess.Handle, WindowHelpers.ShowWindowEnum.Restore);
            return WindowHelpers.SetForegroundWindow(bProcess.MainWindowHandle) > 0;
        }

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10, // 0x0000000A
            ForceMinimized = 11, // 0x0000000B
        }
    }
}
