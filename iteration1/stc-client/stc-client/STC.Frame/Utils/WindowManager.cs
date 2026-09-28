using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.XBond.Plugin.Utils
{
    /// <summary>
    /// 窗口管理类
    /// </summary>
    public class WindowManager
    {
        private static WindowManager _windowManager = new WindowManager();

        public static WindowManager Instance { get { return _windowManager; } }

        public ConcurrentDictionary<string, Window> _UniqueKeyMapWindowDic = new ConcurrentDictionary<string, Window>();
        private ConcurrentDictionary<int, string> _WinHashCodeMapUniqueKeyDic = new ConcurrentDictionary<int, string>();

        private object lockobj = new object();
        private WindowManager()
        { }

        /// <summary>
        /// 判断窗口是否存在
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <returns></returns>
        public bool IsExist(string uniqueKey)
        {
            if (string.IsNullOrEmpty(uniqueKey) == false)
            {
                if (_UniqueKeyMapWindowDic.ContainsKey(uniqueKey))
                {
                    _UniqueKeyMapWindowDic[uniqueKey].Activate();
                    return true;
                }
            }
            return false;
        }

        public void CloseAllWindow()
        {
            foreach (KeyValuePair<string, Window> kv in _UniqueKeyMapWindowDic)
            {
                kv.Value.Close();
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="uniqueKey"></param>
        public void CloseWindow(string uniqueKey)
        {
            if (string.IsNullOrEmpty(uniqueKey) == false)
            {
                if (_UniqueKeyMapWindowDic.ContainsKey(uniqueKey))
                {
                    _UniqueKeyMapWindowDic[uniqueKey].Close();
                }
            }
        }

        /// <summary>
        /// 只能打开一个窗口
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <param name="win"></param>
        /// <param name="owner"></param>
        public void OpenOnlyOneWindow(string uniqueKey, Window win, Window owner = null)
        {
            if (string.IsNullOrEmpty(uniqueKey) || win == null)
            {
                return;
            }

            lock(lockobj)
            {
                if (_UniqueKeyMapWindowDic.ContainsKey(uniqueKey))
                {
                    _UniqueKeyMapWindowDic[uniqueKey].Activate();
                }
                else
                {
                    win.Closed += Win_Closed;
                    _UniqueKeyMapWindowDic[uniqueKey] = win;
                    _WinHashCodeMapUniqueKeyDic[win.GetHashCode()] = uniqueKey;
                    //win.Owner = owner ?? Application.Current.MainWindow;
                    win.Tag = uniqueKey;
                    if (win.Owner == null)
                        win.Owner = Application.Current.MainWindow;

                    win.Show();
                }
            }
        }

        /// <summary>
        /// 只能打开一个窗口
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <param name="win"></param>
        /// <param name="owner"></param>
        public void OpenWindow(string uniqueKey, Window win)
        {
            if (string.IsNullOrEmpty(uniqueKey) || win == null)
            {
                return;
            }

            lock (lockobj)
            {
                if (_UniqueKeyMapWindowDic.ContainsKey(uniqueKey))
                {
                    _UniqueKeyMapWindowDic[uniqueKey].Activate();
                }
                else
                {
                    win.Closed += Win_Closed;
                    _UniqueKeyMapWindowDic[uniqueKey] = win;
                    _WinHashCodeMapUniqueKeyDic[win.GetHashCode()] = uniqueKey;
                    win.Tag = uniqueKey;
                    //if (win.Owner == null)
                    //    win.Owner = Application.Current.MainWindow;

                    win.Show();
                }
            }
        }

        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Win_Closed(object sender, EventArgs e)
        {
            if (sender != null && sender is Window)
            {
                lock (lockobj)
                {
                    Window win = sender as Window;
                    int hashCode = win.GetHashCode();
                    string uniqueKey = string.Empty;
                    bool isSuccess = _WinHashCodeMapUniqueKeyDic.TryRemove(hashCode, out uniqueKey);
                    if (isSuccess)
                    {
                        Window removeWin;

                        isSuccess = _UniqueKeyMapWindowDic.TryRemove(uniqueKey, out removeWin);
                        if (isSuccess && removeWin != null && removeWin.Owner != null)
                            removeWin.Owner.Focus();

                        //OnWinClose(uniqueKey, removeWin.Height, removeWin.Width, removeWin.Left, removeWin.Top);

                        removeWin = null;
                        
                    }
                }
            }
        }

        #region 窗口记忆

        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        private readonly string[] WinKeyList = new string[] { "MyOrders", "MyExecutions", "MarketExecution" };

        private void OnWinClose(string winKey, double height, double width, double left, double top)
        {
            if (WinKeyList.Contains(winKey))
            {
                IniFileHelper.WriteValue($"{winKey}_{ LocalEnv.UserInfo.UserName}", WinHeight, height + "");
                IniFileHelper.WriteValue($"{winKey}_{ LocalEnv.UserInfo.UserName}", WinWidth, width + "");

                IniFileHelper.WriteValue($"{winKey}_{ LocalEnv.UserInfo.UserName}", PositionX, left + "");
                IniFileHelper.WriteValue($"{winKey}_{ LocalEnv.UserInfo.UserName}", PositionY, top + "");
            }
        }

        #endregion


        public void ChangWindowsDict(string oldSecurityId, string newSecurityId, Window myTradingPopupWin)
        {
            // 修改记忆行情小窗窗口名称，便于保存记忆窗口
            string oldUniquekey = $"QuotesMiniWindows-{oldSecurityId}";
            string newUniquekey = $"QuotesMiniWindows-{newSecurityId}";

            lock (lockobj)
            {
                if (WindowManager.Instance.IsExist(newUniquekey) == false)
                {
                    Window _mywin;
                    string _mywinHash;
                    _UniqueKeyMapWindowDic.TryGetValue(oldUniquekey, out _mywin);
                    _mywin.Closed -= Win_Closed;
                    bool isremoveok =  _UniqueKeyMapWindowDic.TryRemove(oldUniquekey, out _mywin);

                    int trytime = 0;
                    while ((!isremoveok) && trytime < 3)
                    {
                        isremoveok = _UniqueKeyMapWindowDic.TryRemove(oldUniquekey, out _mywin);
                        trytime++;
                        Thread.Sleep(20);
                    }

                    _WinHashCodeMapUniqueKeyDic.TryGetValue(_mywin.GetHashCode(), out _mywinHash);
                    //if (!WindowManager.Instance._UniqueKeyMapWindowDic.ContainsKey(newUniquekey))
                    {
                        _mywin.Closed += Win_Closed;
                        _UniqueKeyMapWindowDic[newUniquekey] = _mywin;
                        _WinHashCodeMapUniqueKeyDic[_mywin.GetHashCode()] = newUniquekey;
                    }
                }
            }
        }
    }
}
