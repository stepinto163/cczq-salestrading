using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public static class Win32Helper
    {
        /// <summary>
        /// true:设置成左手; false:设置成右手
        /// </summary>
        /// <param name="fSwap"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public extern static bool SwapMouseButton(bool fSwap);

        /// <summary>
        /// index:23 获取当前鼠标设置状态
        /// </summary>
        /// <param name="index"></param>
        /// <returns>0:右手习惯  1:左手习惯</returns>
        [DllImport("user32.dll")]
        public extern static int GetSystemMetrics(int index);

        #region window任务栏加菜单

        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_STRING = 0x0;
        public const Int32 IDM_ABOUT = 10000;

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern bool AppendMenu(IntPtr hMenu, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);
        #endregion
    }
}
