using Ats.Foundation.Message;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.PT;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.Utils;
using STC.PT.Plugin.View.ExecManage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace STC.PT.Plugin.ViewModel.ExecManage
{
    public class AddIntentionQuoteViewModel : ViewModelBase
    {
        private Dictionary<string, object> _InvestorstItemsSource = new Dictionary<string, object>();
        public Dictionary<string, object> InvestorstItemsSource
        {
            get { return _InvestorstItemsSource; }
            set
            {
                if (_InvestorstItemsSource != value)
                {
                    _InvestorstItemsSource = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _SelectedInvestorstItem = null;
        public string SelectedInvestorstItem
        {
            get { return _SelectedInvestorstItem; }
            set
            {
                _SelectedInvestorstItem = value;
                if (!string.IsNullOrEmpty(_SelectedInvestorstItem))
                {
                    ConfirmIsEnabled = true;
                }
                else 
                {
                    ConfirmIsEnabled = false;
                }
                this.OnPropertyChanged();
            }
        }
        private bool _ConfirmIsEnabled = false;
        public bool ConfirmIsEnabled
        {
            get { return _ConfirmIsEnabled; }
            set 
            {
                _ConfirmIsEnabled = value; 
                this.OnPropertyChanged(); 
            }
        }
        /// <summary>
        /// 量
        /// </summary>
        private string _Qty;
        public string Qty
        {
            get { return _Qty; }
            set
            {
                if (value != null)
                {
                    _Qty = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public PtExecordersData _ExecordersData;
        public PtExecordersData ExecordersData
        {
            get { return _ExecordersData; }
            set
            {
                _ExecordersData = value;
                this.OnPropertyChanged();
                if (_ExecordersData != null) 
                {
                    setDefault();
                }
            }
        }

        public event Action RequestClose;
        public ICommand ConfirmCommand { get { return new RelayCommand(ConfirmClick); } }

        private bool IsInvestor = false;
        public AddIntentionQuoteViewModel()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                PTExecOrderProcesser.Instance.BookAllDict = AdminHandler.Instance.GetAllBookDic();
                _InvestorstItemsSource.Clear();
                Dictionary<string, object> dic = new Dictionary<string, object>();
                if (Common.Utils.CommonUtils.IsInvestor())
                {
                    dic.Add(LocalEnv.UserInfo.UserName, LocalEnv.UserInfo.FullName);
                    IsInvestor = true;
                }
                else
                {
                    List<StcTbookUsers> investorsList = PTExecOrderProcesser.Instance.InvestorsList;
                    List<string> managerList = AdminHandler.Instance.GetManagerList(LocalEnv.UserInfo.UserId);
                    if (managerList != null)
                    {
                        List<StcTbookUsers> list = investorsList.Where(d => managerList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加

                        foreach (StcTbookUsers res in list)
                        {
                            dic.Add(res.Username, res.Name);
                        }
                    }
                }
                this.InvestorstItemsSource = dic;
            }));
        }
        private void setDefault() 
        {
            if (!string.IsNullOrEmpty(ExecordersData.LastQty)) 
            { 
                this.Qty = ExecordersData.LastQty;
            }
        }
        private bool isCheck() 
        {
            bool ischeck = false;

            if (string.IsNullOrEmpty(SelectedInvestorstItem))
            {
                CustomMessageBox.Info("缺少投资经理!", "产生个需");
                return ischeck;
            }
            if (string.IsNullOrWhiteSpace(Qty))
            {
                CustomMessageBox.Info("缺少量!", "产生个需");
                return ischeck;
            }
            else 
            {
                decimal qty = Common.Utils.CommonUtils.ConvertToDecimal(Qty);
                if (qty > 9999999 || qty < 100)
                {
                    CustomMessageBox.Info("输入量取值需要在[100,9999999]范围内!", "产生个需");
                    return ischeck;
                }
                else 
                {
                    bool isMultipleOf100 = qty % 100m == 0m;
                    if (!isMultipleOf100) 
                    {
                        CustomMessageBox.Info("输入量取值需要在[100,9999999]范围内!", "产生个需");
                        return ischeck;
                    }
                }
                if (!string.IsNullOrEmpty(ExecordersData.LastQty) && 
                    (qty > 
                    (Common.Utils.CommonUtils.ConvertToDecimal(ExecordersData.LastQty) - 
                    (!string.IsNullOrEmpty(ExecordersData.MatchQty)?
                    Common.Utils.CommonUtils.ConvertToDecimal(ExecordersData.MatchQty):0)))) 
                {
                    CustomMessageBox.Info("输入量不能大于剩余可匹配量!", "产生个需");
                    return ischeck;
                }
            }
            ischeck = true;
            return ischeck;
        }
        private void ConfirmClick()
        {
            if (!isCheck()) 
            {
                return;
            }
            STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = getIntentionQuoteInfo(ExecordersData);

            ThreadPool.QueueUserWorkItem(o =>
            {
                Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo intentionResult = IntentionOldHandler.Instance.InsertIntentionQuote_Common(request);

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (intentionResult != null)
                    {
                        if (intentionResult.Code == "10000")
                        {
                            string riskCheckResult = string.Empty;
                            string riskCheckDetail = string.Empty;
                            if (intentionResult != null && intentionResult.Datas != null && intentionResult.Datas.Count > 0)
                            {
                                foreach (Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in intentionResult.Datas)
                                {
                                    if (JsonSplit.IsJson(obj.info5))
                                    {
                                        NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(obj.info5);

                                        riskCheckResult = info5tag.RISK_CHECK_RESULT;
                                        riskCheckDetail = info5tag.RISK_CHECK_DETAIL;
                                    }
                                }
                            }

                            BaseReplyPo reply = new BaseReplyPo();
                            reply.Code = intentionResult.Code;
                            reply.Msg = riskCheckDetail;
                            NewOrderSingle nos = new NewOrderSingle();
                            nos.SecurityID = ExecordersData.SecurityID;
                            nos.OrderQty = Qty;
                            nos.Side = ExecordersData.Side;
                            SendPriceWarnView warnView = new SendPriceWarnView(reply, nos, request);

                            warnView.ShowDialog();
                        }
                        else if (intentionResult.Code == "0000")
                        {
                            OpenMatchIntention();
                            CloseWindow();
                        }
                        else
                        {
                            string errMsg = intentionResult == null ? $"创建个需失败" : "创建个需失败：" + (!intentionResult.IsSuccess() ? intentionResult.Msg : string.Empty);
                            if (!string.IsNullOrEmpty(errMsg))
                            {
                                showMessage(errMsg, false);
                            }
                            CloseWindow();
                        }
                    }
                    else
                    {
                        string errMsg = intentionResult == null ? $"创建个需失败" : "创建个需失败：" + (!intentionResult.IsSuccess() ? intentionResult.Msg : string.Empty);
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            showMessage(errMsg, false);
                        }
                        CloseWindow();
                    }
                }));
            });
        }
        private void OpenMatchIntention()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                MatchIntentionView view = new MatchIntentionView(ExecordersData, IsInvestor);
                WindowManager.Instance.OpenWindow("MatchIntention_" + ExecordersData.ExecID, view);
            }));
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message, bool autoClose)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.HideCancelButton();
            me.setWarnMsg(message, autoClose);
            me.ShowDialog();
        }
        public void CloseWindow()
        {
            RequestClose?.Invoke();
        }
        private STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq getIntentionQuoteInfo(PtExecordersData req)
        {
            STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = new STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq();
            request.userName = LocalEnv.UserInfo.UserName;
            string quotetype = "INTENTION";
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                quotetype = "BUYER_INTENTION";
            }
            request.quoteType = quotetype;
            request.quoteList = new List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>();
            STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo record = new STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo();
            record.counterPartyID = req.CounterPartyId;
            record.counterPartyName = req.CounterPartyName;
            record.counterPartySubID = req.CounterPartySubId;
            record.counterPartySubName = req.CounterPartySubName;
            record.traderId = req.TraderID;
            record.traderName = req.TraderName;
            record.side = req.Side;
            record.securityId = req.SecurityID;
            record.symbol = req.Symbol;
            record.price = Common.Utils.CommonUtils.ConvertToDecimalOrNull(req.LastPx);
            record.priceType = "3";
            record.cleanPrice = Common.Utils.CommonUtils.ConvertToDecimalOrNull(req.NetPrice);
            record.qty = Common.Utils.CommonUtils.ConvertToDecimalOrNull(Qty);

            record.settlType = string.Format("{0}{1}", STC.Common.Utils.CommonUtils.ConvertToDateTime(req.TradeDate).ToString("MM.dd"), req.SettlType);
            record.userName = req.Username;
            record.closeBy = LocalEnv.UserInfo.UserName;
            record.closeByName = LocalEnv.UserInfo.FullName;
            record.status = "0";
            record.channel = "CLIENT";
            record.clordId = DataTimeSequence.Instance.GetId();


            if (LocalEnv.ConfInfo.IsPAYL)
            {
                if (!string.IsNullOrEmpty(SelectedInvestorstItem))
                {
                    if (Common.Utils.CommonUtils.IsInvestor())
                    {
                        record.partyManager = LocalEnv.UserInfo.UserName;
                        record.partyManagerName = LocalEnv.UserInfo.FullName;
                        List<StcTbookUsers> traders = Common.Utils.CommonUtils.GetTraders();
                        List<string> traderList = AdminHandler.Instance.GetTraderList(LocalEnv.UserInfo.UserId);
                        if (traderList != null)
                        {
                            List<StcTbookUsers> list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                            if (list != null && list.Count > 0) 
                            {
                                StcTbookUsers stcTbookUsers = list[0];
                                record.partyTrader = stcTbookUsers.Username;
                                record.partyTraderName = stcTbookUsers.Name;
                            }
                        }

                    }
                    else
                    {
                        List<StcTbookUsers> list = PTExecOrderProcesser.Instance.InvestorsList.Where(o => SelectedInvestorstItem.Equals(o.Username)).ToList();
                        if (list.Count > 0)
                        {
                            StcTbookUsers stcTbook = list[0];
                            record.partyManager = stcTbook.Username;
                            record.partyManagerName = stcTbook.Name;
                        }

                        record.partyTrader = LocalEnv.UserInfo.UserName;
                        record.partyTraderName = LocalEnv.UserInfo.FullName;
                    }
                }
                if (Common.Utils.CommonUtils.IsInvestor())
                {
                    request.actionFlag = "0";
                }
                else
                {
                    request.actionFlag = "2";
                }
                request.riskManualCk = "1";
            }

            request.quoteList.Add(record);
            return request;
        }
    }
}
