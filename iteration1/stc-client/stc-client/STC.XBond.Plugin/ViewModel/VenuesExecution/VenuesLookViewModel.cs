using STC.XBond.Plugin.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 渠道执行查看VenuesLookView的数据内容
    /// </summary>
    public class VenuesLookViewModel : ViewModelBase
    {
        private Window _myWindow = null;
        private readonly string DEFAULT_TXT = "--";
        private readonly string EXEC_TITLE = "成交回报";
        private readonly string ORDER_TITLE = "订单信息";

        private readonly string CANCEL = "撤单原因";
        private readonly string EXECEPTION = "异常原因";

        #region 界面属性

        private ExecOrderPo _execOrder;
        public ExecOrderPo ExecOrder
        {
            get { return _execOrder; }
            set
            {
                _execOrder = value;

                this.OnPropertyChanged();
            }
        }

        private string _goodsCode = string.Empty;
        public string GoodsCode
        {
            get { return _goodsCode; }
            set
            {
                _goodsCode = value;

                this.OnPropertyChanged();
            }
        }

        private string _shortname = string.Empty;
        public string Shortname
        {
            get { return _shortname; }
            set
            {
                _shortname = value;

                this.OnPropertyChanged();
            }
        }

        private string _winTitle = string.Empty;
        public string WinTitle
        {
            get { return _winTitle; }
            set
            {
                _winTitle = value;

                this.OnPropertyChanged();
            }
        }

        private string _execId = string.Empty;
        public string ExecId
        {
            get { return _execId; }
            set
            {
                _execId = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _execVisibility = Visibility.Collapsed;

        public Visibility ExecVisibility
        {
            get
            {
                return _execVisibility;
            }

            set
            {
                _execVisibility = value;

                this.OnPropertyChanged();
                
            }
        }

        private Visibility _failVisibility = Visibility.Collapsed;

        public Visibility FailVisibility
        {
            get
            {
                return _failVisibility;
            }

            set
            {
                _failVisibility = value;

                this.OnPropertyChanged();

            }
        }

        private string _failTxt = string.Empty;
        public string FailTxt
        {
            get { return _failTxt; }
            set
            {
                _failTxt = value;

                this.OnPropertyChanged();
            }
        }

        private string _reason = string.Empty;
        public string Reason
        {
            get { return _reason; }
            set
            {
                _reason = value;

                this.OnPropertyChanged();
            }
        }

        #endregion


        public ICommand OkCommand { get { return new RelayCommand(OnConfirmClick); } }

        private void OnConfirmClick()
        {
            _myWindow.Close();
        }

        public void Init(FrameworkElement own, ExecutionReport er)
        {
            _myWindow = own as Window;

            //1:部分成交, 2:已成交
            er.ExecType = CommonMapUtil.FindEnum(er.ExecType, "venuestatus");
            if (er.ExecType.Equals("1") || er.ExecType.Equals("2"))
            {
                ExecVisibility = Visibility.Visible;
                FailVisibility = Visibility.Collapsed;

                GoodsCode = er.SecurityID;
                Shortname = er.Symbol;

                WinTitle = EXEC_TITLE;

                StackPanel sp = _myWindow.FindName("spExeOrder") as StackPanel;
                ExecOrderPo execOrderPo = new ExecOrderPo();

                VenuesExecOrderView orderView = new VenuesExecOrderView();
                orderView.DataContext = execOrderPo;
                sp.Children.Add(orderView);

                ExecId = er.OrderID;

                execOrderPo.Price = string.IsNullOrEmpty(er.LastPx)?er.Price:er.LastPx;
                execOrderPo.Side = CommonMapUtil.FindEnum(er.Side, "side");
                if (er.Side.Equals("1"))
                {
                    execOrderPo.SideBrush = (Brush)_myWindow.FindResource("ColorBrush.Orange5");

                }
                else if (er.Side.Equals("2"))
                {
                    execOrderPo.SideBrush = (Brush)_myWindow.FindResource("ColorBrush.Blue8");
                }
                execOrderPo.Amount = er.OrderQty;
                execOrderPo.Counterparty = string.IsNullOrEmpty(er.CounterPartyName)? DEFAULT_TXT:er.CounterPartyName;
                execOrderPo.CounterpartyExchanger = string.IsNullOrEmpty(er.CounterPartySubName) ? DEFAULT_TXT : er.CounterPartySubName;
                
            }
            else
            {
                _myWindow.Height = 320;


                ExecVisibility = Visibility.Collapsed;
                FailVisibility = Visibility.Visible;

                WinTitle = ORDER_TITLE;

                _execOrder = new ExecOrderPo();
                _execOrder.OrderId = er.OrderID;
                _execOrder.Settle = CommonMapUtil.FindEnum(er.SettlType, "settletype");
                if (string.IsNullOrEmpty(_execOrder.Settle))
                    _execOrder.Settle = er.SettlType;

                _execOrder.Side = CommonMapUtil.FindEnum(er.Side, "side");
                if (er.Side.Equals("1"))
                {
                    _execOrder.SideBrush = (Brush)_myWindow.FindResource("ColorBrush.Orange5");

                }
                else if (er.Side.Equals("2"))
                {
                    _execOrder.SideBrush = (Brush)_myWindow.FindResource("ColorBrush.Blue8");
                }
                _execOrder.Amount = er.OrderQty;
                _execOrder.Price = string.IsNullOrEmpty(er.LastPx) ? er.Price : er.LastPx;

                _execOrder.FirmType = er.FirmFlag.Equals("1")?"firm":"nonFirm";

                //4:撤销
                if (er.ExecType.Equals("4"))
                {
                    FailTxt = CANCEL;
                }
                else
                    FailTxt = EXECEPTION;


                Reason = er.RejectText;
            }

        }
    }

    public class ExecOrderPo : ViewModelBase
    {
        private string _side;
        public string Side{ get{ return _side;}set{_side = value;this.OnPropertyChanged();}}

        private Brush _sideBrush;
        public Brush SideBrush
        {
            get { return _sideBrush; }
            set
            {
                _sideBrush = value;
                this.OnPropertyChanged();

            }
        }

        private string _price;
        public string Price { get { return _price; } set { _price = value; this.OnPropertyChanged(); } }

        private string _amount;
        public string Amount { get { return _amount; } set { _amount = value; this.OnPropertyChanged(); } }

        private string _counterparty = string.Empty;
        public string Counterparty
        {
            get { return _counterparty; }
            set
            {
                _counterparty = value;

                this.OnPropertyChanged();
            }
        }

        private string _counterpartyExchanger = string.Empty;
        public string CounterpartyExchanger
        {
            get { return _counterpartyExchanger; }
            set
            {
                _counterpartyExchanger = value;

                this.OnPropertyChanged();
            }
        }

        private string _orderId;
        public string OrderId { get { return _orderId; } set { _orderId = value; this.OnPropertyChanged(); } }

        private string _firmType;
        public string FirmType { get { return _firmType; } set { _firmType = value; this.OnPropertyChanged(); } }

        private string _settle;
        public string Settle { get { return _settle; } set { _settle = value; this.OnPropertyChanged(); } }
    }
}
