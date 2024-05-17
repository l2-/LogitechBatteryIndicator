using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LogitechBatteryIndicator.helpers
{
    internal partial class WindowHelpers
    {
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);
        [LibraryImport("user32.dll", SetLastError = true)]

        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsWindowVisible(IntPtr hWnd);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial int SetForegroundWindow(IntPtr hwnd);

        [LibraryImport("user32.dll")]
        private static partial IntPtr FindWindowA([MarshalAs(UnmanagedType.LPStr)] string? lpClassName, [MarshalAs(UnmanagedType.LPStr)] string? lpWindowName);

        [LibraryImport("kernel32.dll")]
        public static partial uint GetLastError();

        [LibraryImport("kernel32.dll")]
        public static partial void SetLastError(uint err);

        public static IntPtr WindowHandle(string title)
        {
            return FindWindowA(null, title);
        }

        public static void RestoreWindow(IntPtr windowHandle)
        {
            ShowWindow(windowHandle, ShowWindowEnum.Restore);
        }

        public static bool BringMainWindowToFront(IntPtr windowHandle)
        {
            return SetForegroundWindow(windowHandle) > 0;
        }

        public static bool BringMainWindowToFront(string windowTitle)
        {
            var windowHandle = WindowHandle(windowTitle);
            return BringMainWindowToFront(windowHandle);
        }

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            Maximize = 3,
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
