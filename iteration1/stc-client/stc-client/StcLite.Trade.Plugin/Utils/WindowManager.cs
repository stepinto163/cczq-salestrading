using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StcLite.Trade.Plugin.Utils
{
    public class WindowManager
    {
        private static WindowManager _windowManager = new WindowManager();

        public static WindowManager Instance { get { return _windowManager; } }

        private ConcurrentDictionary<string, Window> _UniqueKeyMapWindowDic = new ConcurrentDictionary<string, Window>();
        private ConcurrentDictionary<int, string> _WinHashCodeMapUniqueKeyDic = new ConcurrentDictionary<int, string>();

        private WindowManager()
        { }

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

        public void OpenOnlyOneWindow(string uniqueKey, Window win, Window owner = null)
        {
            if (string.IsNullOrEmpty(uniqueKey) || win == null)
            {
                return;
            }

            lock(uniqueKey)
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
                    win.Owner = owner ?? Application.Current.MainWindow;
                    win.Show();
                }
            }
        }

        private void Win_Closed(object sender, EventArgs e)
        {
            if (sender != null && sender is Window)
            {
                Window win = sender as Window;
                int hashCode = win.GetHashCode();
                string uniqueKey = string.Empty;
                bool isSuccess = _WinHashCodeMapUniqueKeyDic.TryRemove(hashCode, out uniqueKey);
                if (isSuccess)
                {
                    Window removeWin;
                    isSuccess = _UniqueKeyMapWindowDic.TryRemove(uniqueKey, out removeWin);

                    if(isSuccess && removeWin != null && removeWin.Owner != null)
                        removeWin.Owner.Focus();

                    removeWin = null;
                    
                }
            }
        }
    }
}
