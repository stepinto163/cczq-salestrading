using com.gateway.log;
using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.PT.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.PT.Plugin.ViewModel
{
    public class BatchManualSendViewModel : ViewModelBase
    {
        public event EventHandler RequestCloseAfterSubmit;
        private ObservableCollection<PtExecordersData> _Mv = new ObservableCollection<PtExecordersData>();
        public ObservableCollection<PtExecordersData> Mv
        {

            get
            {
                if (_Mv == null)
                {
                    _Mv = new ObservableCollection<PtExecordersData>();
                }

                return _Mv;
            }

            set
            {
                _Mv = value;
                this.OnPropertyChanged();
            }

        }
        private bool _IsAllChecked;
        public bool IsAllChecked
        {
            get { return _IsAllChecked; }
            set
            {




                _IsAllChecked = value;
                this.OnPropertyChanged();
                checkAll();
            }
        }
        private int dataCount = 0;
        private int selectCount = 0;
        public ICommand ConfirmCommand { get { return new Utils.RelayCommand(ConfirmClick); } }

        private string currentData = "";
        private const string YYYYMMDD = "yyyyMMdd";

        public BatchManualSendViewModel() 
        {
        }
        public void loadData()
        {
            Mv.Clear();
            PTExecOrderUnSendReq req = new PTExecOrderUnSendReq();
            currentData = DateTime.Now.Date.ToString(YYYYMMDD);
            ThreadPool.QueueUserWorkItem(o =>
            {
                ExecOrderQuery(req);
            });
        }
        public void ExecOrderQuery(PTExecOrderUnSendReq req)
        {
            LogHelper.Info("ExecOrderQuery...............................");
            IsAllChecked = false;
            bool flag = true;
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                while (flag)
                {
                    PtExecordersReplyPo reply = PTHandler.Instance.ExecOrderUnSendQuery(req);
                    if (reply == null || reply.Datas == null)
                    {
                        flag = false;
                        break;
                    }
                    if (reply != null && reply.Datas != null)
                    {
                        dataCount = dataCount + reply.Datas.Count();
                        foreach (TbookPtExecorders eo in reply.Datas)
                        {
                            OnOrder(eo);
                        }
                        if (reply.Datas.Count < req.pageSize)
                        {
                            flag = false;
                            if (selectCount == dataCount)
                            {
                                IsAllChecked = true;
                            }
                        }
                    }
                    LogHelper.Info("ExecOrderQuery2...............................");
                    req.pageNum += 1;
                }
                OrderByTradeDate();
            }));
        }
        private void OrderByTradeDate() 
        {
            var sortItems = new ObservableCollection<PtExecordersData>(Mv.OrderBy(item => item.TradeDated));
            Mv.Clear();
            foreach (var item in sortItems) 
            {
                Mv.Add(item);
            }
        }
        public void OnOrder(TbookPtExecorders order)
        {
            try
            {
                PtExecordersData dataRow = this.Mv.FirstOrDefault(o => o.ExecID == order.Execid);
                bool isold = false;
                if (dataRow != null)
                {
                    if ("1".Equals(order.Delflag))
                    {   //删除
                        Mv.Remove(dataRow);
                        return;
                    }
                    isold = true;
                }
                else
                {
                    dataRow = new PtExecordersData();
                    dataRow.ExecID = order.Execid;
                }

                dataRow.ClOrdID = order.Clordid;
                dataRow.Matchstatus = order.Matchstatus;
                dataRow.Relatestatus = order.Relatestatus;
                dataRow.Sendstatus = order.Sendstatus;
                dataRow.SecurityID = order.Securityid;
                dataRow.Side = order.Side;
                dataRow.LastPx = string.IsNullOrEmpty(order.Yield) ? "0" : order.Yield;
                dataRow.LastPx_Display = Common.Utils.CommonUtils.FormatPrice(order.Yield + "");// 用于显示的成交价
                dataRow.NetPrice = Common.Utils.CommonUtils.ConvertToString(order.Netprice);
                dataRow.LastQty = Common.Utils.CommonUtils.ConvertToString(order.Qty);
                dataRow.TradeDate = order.Tradedate;
                if (currentData.Equals(order.Tradedate))
                {
                    dataRow.IsChecked = true;
                    dataRow.IsCurrentDate = "1";
                    selectCount++;
                }
                else 
                {
                    dataRow.IsChecked = false;
                    dataRow.IsCurrentDate = "0";
                }
                dataRow.ExecutionTime = order.Transacttime;
                dataRow.SettlType = string.IsNullOrEmpty(order.Settltype) ? "" : order.Settltype;
                dataRow.StrategyOrderId = order.Strategyorderid;
                dataRow.Venueexecid = order.Venueexecid;
                dataRow.Venueid = order.Venueid;
                dataRow.Symbol = order.Symbol;
                dataRow.Symboltype = order.Symboltype;
                dataRow.Tag1 = order.Tag1;
                dataRow.InstitutionName = order.Institutionname;
                dataRow.InstitutionID = order.Institutionid;
                dataRow.TraderName = order.Tradername;
                dataRow.TraderID = order.Traderid;
                dataRow.Venuetypegw = order.Venuetypegw;
                dataRow.Venues = order.Venues;
                dataRow.Quotemethod = order.Quotemethod;
                dataRow.CounterPartyId = order.Counterpartyid;                
                dataRow.CounterPartyName = order.Counterpartyname;
                dataRow.CounterPartySubId = order.Counterpartysubid;
                dataRow.CounterPartySubName = order.Counterpartysubname;
                dataRow.PartySubName = order.Partysubname;
                dataRow.AccountId = order.Accountid;
                dataRow.AccountName = order.Accountname;
                dataRow.Remark = order.Remark;
                dataRow.UpdateTime = order.Updatetime;
                dataRow.Seatnumber = order.Seatnumber;
                dataRow.Approvalstatus = order.Approvalstatus;
                dataRow.IsMe = order.Userid == LocalEnv.UserInfo.UserId;
                dataRow.MaturityDays = order.MaturityDays;
                dataRow.IssuerCode = order.IssuerCode;
                dataRow.WarranterCode = order.WarranterCode;
                dataRow.DayOffStr = order.dayOffStr;
                dataRow.Blocknum = order.Blocknum;
                dataRow.BusinessType = order.BusinessType;
                if (!isold)
                {
                    Mv.Insert(0, dataRow);
                }
                
            }
            catch (Exception e)
            {
                LogHelper.Error("OnOrder", e);
            }

        }
        public bool isSales = false;
        private bool checkSendField(PtExecordersData drv)
        {
            if (isSales)
            {
                if (string.IsNullOrEmpty(drv.InstitutionID))
                {
                    CustomMessageBox.Info(string.Format("{0}缺少客户机构!",drv.Venueexecid), "下发");
                    return false;
                }
                if (string.IsNullOrEmpty(drv.TraderID))
                {
                    CustomMessageBox.Info(string.Format("{0}缺少客户!", drv.Venueexecid), "下发");
                    return false;
                }
            }
            if (string.IsNullOrEmpty(drv.BusinessType))
            {
                CustomMessageBox.Info(string.Format("{0}缺少业务类别!", drv.Venueexecid), "下发");
                return false;
            }
            return true;
        }
        private void ConfirmClick()
        {
            PtExecOrderSaveReq req = new PtExecOrderSaveReq();
            foreach (PtExecordersData drv in Mv)
            {
                if (drv.IsChecked) 
                {
                    req.execIdList.Add(drv.ExecID);
                }
            }
            req.isSend = true;
            if (req.execIdList.Count > 0)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        PtExecSendReplyPo reply = PTHandler.Instance.ExecOrderSend(req);
                        if (reply != null && "0000".Equals(reply.Code))
                        {
                            string msg = "";
                            List<string> unSendOrder = new List<string>();
                            if (reply.Datas.ContainsKey("unSendOrder") && reply.Datas.TryGetValue("unSendOrder", out unSendOrder) && unSendOrder.Count() > 0)
                            {
                                string unsendmsg = string.Join("\n", unSendOrder);
                                msg = string.Format("下发未成功:\n{0}", unsendmsg);
                            }
                            List<string> sendOrder = new List<string>();
                            if (reply.Datas.ContainsKey("sendOrder") && reply.Datas.TryGetValue("sendOrder", out sendOrder) && sendOrder.Count() > 0)
                            {
                                string sendmsg = string.Join("\n", sendOrder);
                                msg = string.Format("已成功:\n{0}", sendmsg);
                            }
                            if (!string.IsNullOrEmpty(msg))
                            {
                                showMessage(msg);
                            }
                            RequestCloseAfterSubmit?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            showMessage("批量下发失败:" + reply.Msg);
                        }
                    }));
                });
            }
            else
            {
                showMessage("未选中交易");
            }
        }


        public void checkAll()
        {
            foreach (PtExecordersData drv in Mv)
            {
                if (drv.TradeDate.Equals(currentData))
                {
                    drv.IsChecked = IsAllChecked;
                }
            }
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.HideCancelButton();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
    }
}
