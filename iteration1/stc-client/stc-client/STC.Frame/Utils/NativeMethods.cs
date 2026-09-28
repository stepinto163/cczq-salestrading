using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
    public static class NativeMethods
    {
        //[DllImport("user32.dll")]
        //public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //public const int SH_SHOW = 1;

        //Import the SetForeground API to activate it
        [DllImport("User32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        public static void ProcessMainWindowToFront()
        {
            Process me = Process.GetCurrentProcess();
            if (me.MainWindowHandle != IntPtr.Zero)
            {
                NativeMethods.SetForegroundWindow(me.MainWindowHandle);
            }
        }
    }
}
