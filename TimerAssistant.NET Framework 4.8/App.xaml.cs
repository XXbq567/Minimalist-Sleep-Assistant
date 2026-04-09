using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace TimerAssistant
{
    public partial class App : Application
    {
        private static Mutex _mutex;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            const string mutexName = "Global\\MinimalistSleepMutex_2026";
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                IntPtr hWnd = FindWindow(null, "极简睡眠");
                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd);
                }
                Current.Shutdown();
                return;
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}