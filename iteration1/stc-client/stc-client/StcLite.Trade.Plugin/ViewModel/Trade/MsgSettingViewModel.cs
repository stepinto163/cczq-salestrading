using STC.Common.Model;
using BDB.Price.Plugin.Handler;
using BDB.Price.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BDB.Price.Plugin.ViewModel.Trade
{
    public class MsgSettingViewModel : BaseViewModel
    {
        public ICommand CloseCommand { get { return new RelayCommand<Button>(OnCloseClick); } }

        public ICommand OkCommand { get { return new RelayCommand<Button>(OnOkClick); } }

        private MsgSettingItem _item;

        public MsgSettingItem Item
        {
            get { return _item; }
            set
            {
                _item = value;
                this.OnPropertyChanged();
            }
        }

        private string _symbolName;

        public string SymbolName
        {
            get { return _symbolName; }
            set
            {
                _symbolName = value;
                this.OnPropertyChanged();
            }
        }

        private void OnCloseClick(Button btn)
        {
            Window win = Window.GetWindow(btn);
            if (win != null)
            {
                win.Close();
            }
        }

        private async void OnOkClick(Button btn)
        {
            bool result = await MsgSettingHandler.Instance.SaveGlobalPara(this.Item, false);
            if (result == true)
            {
                Window win = Window.GetWindow(btn);
                if (win != null)
                {
                    win.Close();
                }
            }
        }
    }
}
