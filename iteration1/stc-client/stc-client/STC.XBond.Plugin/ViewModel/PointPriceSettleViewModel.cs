using STC.Common.Data;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.View.Order;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Order;
using STC.XBond.Plugin.ViewModel.TradeBookHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 点价设置界面PointPriceSettleView的数据内容
    /// </summary>
    public class PointPriceSettleViewModel : BaseViewModel
    {
        private Window _myWindow = null;
        private PriceMatchParam _priceMatchParam;

        #region 界面属性

        private string _pointPriceInfo;
        public string PointPriceInfo
        {
            get
            {
                return _pointPriceInfo;
            }

            set
            {
                _pointPriceInfo = value;

                this.OnPropertyChanged();
            }
        }

        private List<ListBoxItemData> _settleItems;
        public List<ListBoxItemData> SettleItems
        {
            get
            {
                return _settleItems;
            }

            set
            {
                _settleItems = value;
                this.OnPropertyChanged();
            }
        }

        private int _selectListBoxIndex = -1;
        public int SelectListBoxIndex { get { return _selectListBoxIndex; } set { _selectListBoxIndex = value; this.OnPropertyChanged(); } }

        public ICommand CancelCommand { get { return new RelayCommand(OnCancel); } }
        public ICommand OkCommand { get { return new RelayCommand(OnOk); } }

        #endregion

        private void OnCancel()
        {
            _myWindow.Close();
        }

        private void OnOk()
        {
            bool isQuick = QuickOrderHandler.Instance.QuickOrder(null, true, _priceMatchParam, _priceMatchParam.OperationType == "3");

            if (!isQuick)
            {
                //SendPointPriceView sendPointPriceView = new SendPointPriceView(_priceMatchParam.Symbol,
                //            _priceMatchParam.Side.Equals("1"), _priceMatchParam.Price, _priceMatchParam.CustomFirmAmount,
                //            GetSelectSettle(), _priceMatchParam.NetPrice, _priceMatchParam.Terminal,
                //           _priceMatchParam.WholeFlag, _priceMatchParam.IsFirm, _priceMatchParam.Id, _priceMatchParam.TrsactTime,
                //           _priceMatchParam.DeliveryType, _priceMatchParam.ClearingMethod, _priceMatchParam.QuoteType);

                SendPointPriceView sendPointPriceView = new SendPointPriceView(_priceMatchParam.Symbol, _priceMatchParam.Side.Equals("1"), _priceMatchParam.Price, _priceMatchParam.CustomFirmAmount
                    , GetSelectSettle(), _priceMatchParam.NetPrice, _priceMatchParam.Terminal, _priceMatchParam.WholeFlag
                , _priceMatchParam.IsFirm, _priceMatchParam.Id, _priceMatchParam.TrsactTime,_priceMatchParam.DeliveryType, _priceMatchParam.ClearingMethod, _priceMatchParam.QuoteType, _priceMatchParam.Text, string.Empty , null);

                // sendPointPriceView.Closed += SendPointPriceView_Closed;
                sendPointPriceView.Owner = Application.Current.MainWindow;
                OnCancel();
                sendPointPriceView.Show();
            }
        }

        private void SendPointPriceView_Closed(object sender, EventArgs e)
        {
            SendPointPriceView sendPointPriceView = sender as SendPointPriceView;
            try
            {
                sendPointPriceView.Closed -= SendPointPriceView_Closed;

                if (sendPointPriceView.Owner != null)
                    sendPointPriceView.Owner.Focus();
            }
            catch { }
        }

        private string GetSelectSettle()
        {
            string result = string.Empty;

            ListBoxItemData selectData = SettleItems[SelectListBoxIndex];

            if (selectData != null)
            {
                result = ComboSourceUtil.Instance.FindEnum(selectData.Name, "settletype_revert"); ;
            }

            return result;
        }

        public void Init(Window myWindow, string priceInfo, string settle, PriceMatchParam priceMatchParam)
        {
            _myWindow = myWindow;

            _priceMatchParam = priceMatchParam;
            PointPriceInfo = priceInfo;
            List<ListBoxItemData> settleItems = new List<ListBoxItemData>();
            string[] arr = settle.Split(new char[] { ',' });
            bool haveOne = false;
            bool haveTwo = false;
            foreach (string str in arr)
            {
                string tem = str;
                ListBoxItemData itemData = new ListBoxItemData();
                if (str == Constants.T2)
                {
                    tem = Constants.T2_PART;
                    haveOne = true;
                    itemData.Seq = 1;
                }
                else if (str == Constants.T3)
                {
                    tem = Constants.T3_PART;
                    haveTwo = true;
                    itemData.Seq = 2;
                }
                else
                    itemData.Seq = 4;

                itemData.Name = tem;

                settleItems.Add(itemData);
            }

            if (haveOne && haveTwo)
            {
                ListBoxItemData itemData = new ListBoxItemData();
                itemData.Name = Constants.T4_PART;
                itemData.Seq = 3;
                settleItems.Add(itemData);
            }

            settleItems.Sort(new ListBoxItemSort());
            SettleItems = settleItems;

            if (SettleItems.Count > 0)
                SelectListBoxIndex = (haveOne && haveTwo)?2:0;
        }

    }
}
