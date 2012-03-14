using System;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MakeAuto
{    
    static class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createNew = true;
            using (Mutex mutex = new Mutex(true, "MakeAuto", out createNew))
            {
                if (createNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new frmMakeAuto());
                }
                else
                {
                    Process[] current = Process.GetProcessesByName("MakeAuto");
                    System.IntPtr mainWindowHandle = current[0].MainWindowHandle;
                    if (mainWindowHandle != IntPtr.Zero)
                    {
                        SetForegroundWindow(mainWindowHandle);
                    }
                    
                }
            }
        }
    }
}
