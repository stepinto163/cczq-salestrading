using STC.Common.Data;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.View.popSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace STC.XBond.Plugin
{
    public class FindSecurityPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 160;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.FindSecurityPlugin; }
        }

        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }

        private bool isFullScreen = false;
        public override bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }

            set
            {
                isFullScreen = value;
            }
        }

        public override string PluginName
        {
            get { return "快捷键查询债券插件"; }
        }

        public override string Description
        {
            get { return "快捷键查询债券插件"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
        }

        #endregion

        private Popup popup = null;
        private Window mainWin = null;
        protected override void load()
        {
            DesignHandler.Instance.OnKeyDown += Instance_OnKeyDown;

            popup = new Popup();
            if (System.Windows.Application.Current.MainWindow != null)
            {
                popup.PlacementTarget = mainWin = System.Windows.Application.Current.MainWindow;
            }

            popup.Width = 360;
            popup.Height = 300;
            FindSecurityView ui = new FindSecurityView();
            if (popup.Child != null)
                popup.Child = null;
            popup.Child = ui;
            popup.StaysOpen = false;
        }

        
        private void Instance_OnKeyDown(System.Windows.Input.KeyEventArgs obj)
        {
            if (!popup.IsOpen)
            {
                ShowSearch(obj.Key, obj.ImeProcessedKey);
            }
        }

        private void ShowSearch(Key key, Key imeKey)
        {
            Key tmpKey = key;
            if (key == Key.ImeProcessed)
                tmpKey = imeKey;



            if ((tmpKey >= Key.A && tmpKey <= Key.Z) ||
              (tmpKey >= Key.D0 && tmpKey <= Key.D9) ||
              tmpKey == Key.Decimal || tmpKey == Key.Space ||
              (tmpKey >= Key.NumPad0 && tmpKey <= Key.NumPad9))
            {
                FindSecurityView ui = popup.Child as FindSecurityView;
                //if (key == Key.ImeProcessed && ((tmpKey >= Key.D0 && tmpKey <= Key.D9) || tmpKey == Key.Decimal ||
                //    (tmpKey >= Key.NumPad0 && tmpKey <= Key.NumPad9)))
                if ((tmpKey >= Key.D0 && tmpKey <= Key.D9) || tmpKey == Key.Decimal ||
                    (tmpKey >= Key.NumPad0 && tmpKey <= Key.NumPad9) || (tmpKey >= Key.A && tmpKey <= Key.Z))
                {
                    ui.tbSearch.Text = GetKeyString(tmpKey);
                    ui.tbSearch.SelectionStart = ui.tbSearch.Text.Length;
                }

                popup.Child = ui;
                popup.Placement = PlacementMode.Relative;
                if (mainWin != null)
                {
                    popup.VerticalOffset = mainWin.ActualHeight - popup.Height;
                    popup.HorizontalOffset = mainWin.ActualWidth - popup.Width;
                }

                popup.IsOpen = true;
            }
        }

        private string GetKeyString(Key key)
        {
            string skey = "";
            if (key >= Key.A && key <= Key.Z)
                skey = (key + "");//.ToLower();
            else if (key >= Key.NumPad0 && key <= Key.NumPad9)
                skey = ((int)key - 74) + "";
            else if (key >= Key.D0 && key <= Key.D9)
                skey = ((int)key - 34) + "";
            else if (key == Key.Decimal)
                skey = ".";
            return skey;
        }
    }
}
