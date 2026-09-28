using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.Common.Utils
{
    /// 
    /// 热键管理器
    /// 
    public class HotKeyManager
    {
        /// 
        /// 热键消息
        /// 
        public const int WM_HOTKEY = 0x312;

        /// 
        /// 注册热键
        /// 
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifuers, int vk);

        /// 
        /// 注销热键
        /// 
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// 
        /// 向原子表中添加全局原子
        /// 
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        /// 
        /// 在表中搜索全局原子
        /// 
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalFindAtom(string lpString);

        /// 
        /// 在表中删除全局原子
        /// 
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);
    }
}
