using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace STC.Broker.Plugin.Utils
{
    /// <summary>
    /// 解决popup中的输入框不能输入中文的问题
    /// </summary>
    public class TextBoxPopup : Popup
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);
        static TextBoxPopup()
        {
            EventManager.RegisterClassHandler(typeof(TextBoxPopup), Popup.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus), true);
        }

        private static void OnPreviewGotKeyboardFocus(Object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = e.NewFocus as TextBoxBase;
            if (textBox != null)
            {
                var hwndSource = PresentationSource.FromVisual(textBox) as HwndSource;
                if (hwndSource != null)
                {
                    SetActiveWindow(hwndSource.Handle);
                }
            }
        }

    }
}
