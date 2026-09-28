using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.Utils
{
    public static class MenuDropAlignmentHelper
    {
        private static FieldInfo _menuDropAlignmentField;
        public static void DisableSystemMenuAlignment()
        {
            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        private static void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EnsureStandardPopupAlignment();
        }

        private static void EnsureStandardPopupAlignment()
        {
            if (SystemParameters.MenuDropAlignment)
            {
                _menuDropAlignmentField?.SetValue(null, false);
            }
        }
    }

}
