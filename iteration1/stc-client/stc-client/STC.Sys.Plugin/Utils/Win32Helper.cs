using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace STC.Sys.Plugin.Utils
{
    /// <summary>
    /// Win32接口公共类
    /// </summary>
    public sealed class Win32Helper
    {
        public struct MEMORY_INFO
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public uint dwTotalPhys;
            public uint dwAvailPhys;
            public uint dwTotalPageFile;
            public uint dwAvailPageFile;
            public uint dwTotalVirtual;
            public uint dwAvailVirtual;
        }

        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MEMORY_INFO meminfo);
        public static decimal ConvertBytes(string b, int iteration)
        {
            long iter = 1;
            for (int i = 0; i < iteration; i++)
                iter *= 1024;
            return Math.Round((Convert.ToDecimal(b)) / Convert.ToDecimal(iter), 2, MidpointRounding.AwayFromZero);
        }

        public struct GRADIENT_RECT
        {
            public uint UpperLeft;
            public uint LowerRight;
            public GRADIENT_RECT(uint ul, uint lr)
            {
                this.UpperLeft = ul;
                this.LowerRight = lr;
            }
        }



        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, long wParam, long lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "PostMessage")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, long wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public extern static int GetWindowLong(IntPtr hWnd, int nlndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        public delegate Int32 CallBack(ref long a);
        [DllImport("kernel32")]
        public static extern Int32 SetUnhandledExceptionFilter(CallBack cb);
    }

}
