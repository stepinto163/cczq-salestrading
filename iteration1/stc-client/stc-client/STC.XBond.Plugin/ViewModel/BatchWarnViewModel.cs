using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.ViewModel;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel
{
    public class BatchWarnViewModel : ViewModelBase
    {
        private readonly string RISK_TITLE = "风控检查";
        private readonly string RISK_REMIND = "[风险警示]";
        private readonly string RISK_STOP = "[风险禁止]";
        private readonly string RISK_REMIND_VAL = "风控警戒值：";
        private readonly string RISK_STOP_VAL = "风控禁止值：";

        private readonly string BID = "Bid";
        private readonly string OFR = "Ofr";

        public SolidColorBrush Gray20;
        public SolidColorBrush Gray18;
        public SolidColorBrush Gray13;

        public SolidColorBrush Red7;
        public SolidColorBrush Red14;
        public SolidColorBrush Red15;

        /// <summary>
        /// 标题
        /// </summary>
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 背景颜色
        /// </summary>
        private Brush showTitleBG;
        public Brush ShowTitleBG 
        { 
            get { return showTitleBG; }
            set 
            { 
                showTitleBG = value;
                this.OnPropertyChanged(); 
            } 
        }

        private Brush showBG;
        public Brush ShowBG { get { return showBG; } set { showBG = value; this.OnPropertyChanged(); } }

        private Brush showBdBG;
        public Brush ShowBdBG { get { return showBdBG; } set { showBdBG = value; this.OnPropertyChanged(); } }

        private Visibility okVisibility = Visibility.Collapsed;
        public Visibility OkVisibility { get { return okVisibility; } set { okVisibility = value; this.OnPropertyChanged(); } }

        private Visibility buttonVisibility = Visibility.Collapsed;
        public Visibility ButtonVisibility { get { return buttonVisibility; } set { buttonVisibility = value; this.OnPropertyChanged(); } }

        public ICommand ConfirmSingleWarnCommand { get { return new RelayCommand<Button>(ConfirmSingleWarn); } }

        public ICommand CancelSingleWarnCommnad { get { return new RelayCommand<Button>(CancelSingleWarn); } }

        public ICommand OkCommand { get { return new RelayCommand<Window>(ConfirmClick); } }

        public ICommand CancelCommand { get { return new RelayCommand<Window>(CancelClick); } }

        private ObservableCollection<RiskDetailPo> _WarnList = new ObservableCollection<RiskDetailPo>();

        public ObservableCollection<RiskDetailPo> WarnList
        {
            get
            {
                return _WarnList;
            }
        }

        private Dictionary<string, List<TPlaceOrderResult>> _replyDic = null;

        private Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> _orderDic = null;

        private Dictionary<string, List<int>> _warnRecordIndexDic = new Dictionary<string, List<int>>();

        private List<string> _sendOrderList = new List<string>();

        ~BatchWarnViewModel()
        {
            if (_warnRecordIndexDic != null)
            {
                _warnRecordIndexDic.Clear();
                _warnRecordIndexDic = null;
            }
            if (_sendOrderList != null)
            {
                _sendOrderList.Clear();
                _sendOrderList = null;
            }
            Gray20 = null;
            Gray18 = null;
            Gray13 = null;
            Red7 = null;
            Red14 = null;
            Red15 = null;
        }

        public void Init(Dictionary<string, List<TPlaceOrderResult>> replyDic, Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> orderDic, bool isDoubleWarn = false)
        {
            _sendOrderList.Clear();
            _warnRecordIndexDic.Clear();
            WarnList.Clear();
            if (replyDic == null || orderDic == null)
            {
                LogHelper.Info("参数错误");
                return;
            }

            _replyDic = replyDic;
            _orderDic = orderDic;

            int index = 0;
            List<TPlaceOrderResult> orderRisks = null;
            foreach (KeyValuePair<string, List<TPlaceOrderResult>> pair in replyDic)
            {
                string orderID = pair.Key;
                orderRisks = pair.Value;
                Common.Model.Fix.Message.NewOrderSingle order = orderDic[orderID];

                string priceDetail = order == null ? string.Empty : string.Format("{0} {1} {2}万", order.SecurityID + "", (order.Side + "").Equals("1") ? BID : OFR, order.OrderQty + "");

                if (orderRisks != null)
                {
                    bool isProhibit = false;
                    bool isConfirm = false;
                    foreach (TPlaceOrderResult por in orderRisks)
                    {
                        if (por.Code.Equals("10002"))
                        {
                            isProhibit = true;
                            break;
                        }
                    }

                    foreach (TPlaceOrderResult por in orderRisks)
                    {
                        RiskDetailPo riskDetailPo = new RiskDetailPo();
                        riskDetailPo.placeOrderResult = por;
                        riskDetailPo.orderID = orderID;
                        Title = RISK_TITLE;

                        riskDetailPo.RiskVisibility = Visibility.Visible;
                        riskDetailPo.ParamVisibility = Visibility.Collapsed;
                        riskDetailPo.RiskPromptVisibility = Visibility.Collapsed;

                        if (por.Code.Equals("10001"))
                        {
                            riskDetailPo.RiskName = RISK_REMIND;
                            riskDetailPo.RiskType = RISK_REMIND_VAL;
                            if (isProhibit == false)
                            {
                                if (string.IsNullOrEmpty(por.State) == false && por.State != "2")
                                {
                                    riskDetailPo.StateVisibility = Visibility.Visible;
                                    riskDetailPo.State = por.State;
                                }
                                else
                                {
                                    riskDetailPo.ConfirmWarnVisiblity = Visibility.Visible;
                                    isConfirm = true;
                                }
                            }
                            
                        }
                        else if (por.Code.Equals("10002"))
                        {
                            riskDetailPo.RiskName = RISK_STOP;
                            riskDetailPo.RiskType = RISK_STOP_VAL;
                        }

                        riskDetailPo.Detail = priceDetail;
                        riskDetailPo.RiskReason = por.Reason;

                        if (isDoubleWarn
                            || (string.IsNullOrEmpty(por.State) == false && por.State == "2"))
                        {
                            riskDetailPo.TwoVisibility = Visibility.Visible;
                            riskDetailPo.ShowBorderBG = Red7;
                            riskDetailPo.ShowBG = Red15;
                        }
                        else
                        {
                            riskDetailPo.TwoVisibility = Visibility.Collapsed;
                            riskDetailPo.ShowBorderBG = Gray20;
                            riskDetailPo.ShowBG = Gray13;
                        }

                        WarnList.Add(riskDetailPo);

                        if (_warnRecordIndexDic.ContainsKey(orderID) == false)
                        {
                            _warnRecordIndexDic[orderID] = new List<int>();
                        }
                        _warnRecordIndexDic[orderID].Add(index);
                        index++;
                    }

                    if (isConfirm == true)
                    {
                        _sendOrderList.Add(orderID);
                    }    
                }
            
                if (_sendOrderList.Count > 0)
                {
                    OkVisibility = Visibility.Visible;
                }
                else
                {
                    ButtonVisibility = Visibility.Visible;
                }

                ShowTitleBG = Gray18;
                ShowBdBG = Gray20;
                ShowBG = Gray13;
            }
        }

        private void ConfirmSingleWarn(Button control)
        {
            if (control != null && control.DataContext is RiskDetailPo)
            {
                RiskDetailPo riskDetailPo = control.DataContext as RiskDetailPo;

                Sub_ConfirmSingleWarn(riskDetailPo);
            }

            foreach (RiskDetailPo rdp in _WarnList)
            {
                if (rdp.ConfirmWarnVisiblity == Visibility.Visible)
                {
                    return;
                }
            }

            var ownerWindow = Window.GetWindow(control);
            if (ownerWindow != null && ownerWindow is BatchWarnView)
            {
                BatchWarnView batchWarnView = ownerWindow as BatchWarnView;
                batchWarnView.Close();
            }
        }

        private void Sub_ConfirmSingleWarn(RiskDetailPo riskDetailPo)
        {
            if (riskDetailPo != null && riskDetailPo.placeOrderResult != null)
            {
                // 判断本单是否所有报警都处理了
                int state = CheckWarnRecord(riskDetailPo);

                if (riskDetailPo.placeOrderResult.DoubleWarn)
                {
                    if (state == 1)
                    {
                        // 变成2次确认
                        riskDetailPo.TwoVisibility = Visibility.Visible;
                        riskDetailPo.ShowBorderBG = Red7;
                        riskDetailPo.ShowBG = Red15;

                        riskDetailPo.placeOrderResult.State = "2";
                    }
                    else if (state == 2)
                    {
                        // 变成已发出
                        riskDetailPo.ConfirmWarnVisiblity = Visibility.Collapsed;
                        riskDetailPo.StateVisibility = Visibility.Visible;
                        riskDetailPo.State = riskDetailPo.placeOrderResult.State = "待其余警示确认后，发出";

                        TrySendOrder(riskDetailPo);

                    }
                }
                else
                {
                    if (state == 1)
                    {
                        // 变成已发出
                        riskDetailPo.ConfirmWarnVisiblity = Visibility.Collapsed;
                        riskDetailPo.StateVisibility = Visibility.Visible;
                        riskDetailPo.State = riskDetailPo.placeOrderResult.State = "待其余警示确认后，发出";

                        TrySendOrder(riskDetailPo);
                    }
                }
            }
        }

        /// <summary>
        /// 取消就把相关同条orderid都撤销
        /// </summary>
        /// <param name="control"></param>
        private void CancelSingleWarn(Button control)
        {
            if (control != null && control.DataContext is RiskDetailPo)
            {
                RiskDetailPo riskDetailPo = control.DataContext as RiskDetailPo;

                Sub_CancelSingleWarn(riskDetailPo);
            }

            foreach (RiskDetailPo rdp in _WarnList)
            {
                if (rdp.ConfirmWarnVisiblity == Visibility.Visible)
                {
                    return;
                }
            }

            var ownerWindow = Window.GetWindow(control);
            if (ownerWindow != null && ownerWindow is BatchWarnView)
            {
                BatchWarnView batchWarnView = ownerWindow as BatchWarnView;
                batchWarnView.Close();
            }
        }

        private void Sub_CancelSingleWarn(RiskDetailPo riskDetailPo)
        {
            if (riskDetailPo != null && riskDetailPo.placeOrderResult != null)
            {
                if (_warnRecordIndexDic.ContainsKey(riskDetailPo.orderID))
                {
                    foreach (int index in _warnRecordIndexDic[riskDetailPo.orderID])
                    {
                        if (string.IsNullOrEmpty(_WarnList[index].State) == false
                            && "待其余警示确认后，发出" != _WarnList[index].State)
                        {
                            continue;
                        }
                        _WarnList[index].ConfirmWarnVisiblity = Visibility.Collapsed;
                        _WarnList[index].StateVisibility = Visibility.Visible;
                        _WarnList[index].State = _WarnList[index].placeOrderResult.State = "关闭";
                    }

                    _sendOrderList.Remove(riskDetailPo.orderID);
                }
            }
        }

        /// <summary>
        /// 尝试下单， 下单前需要检查所有警告都已确认， 
        /// 下完单后把数据从_sendOrderList中移除
        /// </summary>
        /// <param name="orderID"></param>
        private void TrySendOrder(RiskDetailPo riskDetailPo)
        {
            string orderID = riskDetailPo.orderID;
            // 检查
            if (_warnRecordIndexDic.ContainsKey(orderID))
            {
                foreach(int index in _warnRecordIndexDic[orderID])
                {
                    if(WarnList[index].ConfirmWarnVisiblity == Visibility.Visible)
                    {
                        return;
                    }
                }
            }

            if (_sendOrderList.Contains(orderID))
            {
                NewOrderSingle order = _orderDic[orderID];
                List<TPlaceOrderResult> placeOrderResult = _replyDic[orderID];
                if (placeOrderResult != null && placeOrderResult.Count > 0)
                    order.StrategyOrderId = placeOrderResult[0].RequestID;

                LogHelper.Info(string.Format("Try to Send the risk order, StrategyOrderId:{0}", order.StrategyOrderId));
                order.RiskManualCk = "2";//默认空值（检查）, 1：只检查风控禁止，2：风控不检查

                // 下单
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        PResult result = OrdersHandler.Instance.PlaceOrder(order);
                        _sendOrderList.Remove(orderID);
                       
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (result.IsSuccess())
                            {
                                foreach (int index in _warnRecordIndexDic[orderID])
                                {
                                    _WarnList[index].State = _WarnList[index].placeOrderResult.State = "已发出";
                                }
                            }
                            else
                            {
                                foreach (int index in _warnRecordIndexDic[orderID])
                                {
                                    _WarnList[index].State = _WarnList[index].placeOrderResult.State = "发送失败";
                                }
                            }
                        }));
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }
        }

        /// <summary>
        /// 检查报警记录的当前状态
        /// 0:已确认
        /// 1:一次确认
        /// 2:二次确认
        /// </summary>
        private int CheckWarnRecord(RiskDetailPo riskDetailPo)
        {
            int result = -1;

            if (riskDetailPo.ConfirmWarnVisiblity == Visibility.Visible)
            {
                if (riskDetailPo.TwoVisibility == Visibility.Visible)
                {
                    result = 2;
                }
                else
                {
                    result = 1;
                }
            }
            else
            {
                result = 0;
            }

            return result;
        }


        private void ConfirmClick(Window win)
        {
            foreach (RiskDetailPo po in WarnList)
            {
                Sub_ConfirmSingleWarn(po);
            }

            //Dictionary<string, List<TPlaceOrderResult>> doubleWarnDic = new Dictionary<string, List<TPlaceOrderResult>>();
            //foreach (RiskDetailPo po in WarnList)
            //{
            //    int result = CheckWarnRecord(po);
            //    if (result == 2)
            //    {
            //        if (doubleWarnDic.ContainsKey(po.orderID) == false)
            //        {
            //            doubleWarnDic[po.orderID] = new List<TPlaceOrderResult>();
            //        }
            //        doubleWarnDic[po.orderID].Add(po.placeOrderResult);
            //    }
            //}

            //if (doubleWarnDic.Count > 0)
            //{
            //    BatchWarnView batchWarnView = new BatchWarnView(doubleWarnDic, _orderDic, true);
            //    batchWarnView.Owner = Application.Current.MainWindow;
            //    batchWarnView.Show();
            //}

            foreach (RiskDetailPo po in WarnList)
            {
                int result = CheckWarnRecord(po);
                if (result == 2)
                {
                    return;
                }
            }

            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.tbTitle.Text = "风控警告全部发出";
            prompt.tbRefer.Text = "处理完成";
            prompt.Owner = win.Owner;

            win.Close();

            prompt.Show();
           
        }

        private void CancelClick(Window win)
        {
            foreach (RiskDetailPo po in WarnList)
            {
                Sub_CancelSingleWarn(po);
            }

            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.tbTitle.Text = "风控警告全部关闭";
            prompt.tbRefer.Text = "处理完成";
            prompt.Owner = win.Owner;

            win.Close();

            prompt.Show();
        }
    }
}
