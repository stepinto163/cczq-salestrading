using Newtonsoft.Json;
using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.PT.Plugin.ViewModel
{
    public class BatchUpdateDetailsViewModel : ViewModelBase
    {
        public event EventHandler RequestCloseAfterSubmit;
        private ObservableCollection<PTUpdatePo> _Mv ;
        public ObservableCollection<PTUpdatePo> Mv
        {
            get
            {
                if (_Mv == null)
                {
                    _Mv = new ObservableCollection<PTUpdatePo>();

                    this.OnPropertyChanged();
                }

                return _Mv;
            }

        }
        private int _SelectCount;
        public int SelectCount
        {
            get { return _SelectCount; }
            set
            {
                _SelectCount = value;
                this.OnPropertyChanged();
            }
        }
        private string _MsgDetails;
        public string MsgDetails
        {
            get { return _MsgDetails; }
            set 
            {
                _MsgDetails = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsUpdating = false;
        public bool IsUpdating
        {
            get { return _IsUpdating; }
            set
            {
                _IsUpdating = value;
                this.OnPropertyChanged();
            }
        }
        private Dictionary<string, string> updateDic = new Dictionary<string, string>();
        public ICommand ConfirmCommand { get { return new Utils.RelayCommand(ConfirmClick); } }

        private string currentDate = "";
        public BatchUpdateDetailsViewModel() 
        {
            currentDate = DateTimeEx.ServerTime.ToString("yyyyMMdd");
        }
        private void ConfirmClick()
        {
            if (updateDic.Count > 0)
            {
                List<PtExecOrderSaveReq> reqList = new List<PtExecOrderSaveReq>();
                foreach (PTUpdatePo drv in Mv)
                {
                    PtExecOrderSaveReq req = new PtExecOrderSaveReq();
                    req.execid = drv.ExecID;
                    bool isrefresh = false;

                    bool isrefreshNetPrice = false;
                    if (updateDic.ContainsKey(PTExecOrderProcesser.Securityid))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.Securityid, out para))
                        {
                            req.securityid = para;
                        }
                    }
                    //true交易所（席位号+约定号）false银行间（报价方式+备注）
                    bool isBank = STC.PT.Plugin.Utils.CommonUtils.IsBank(string.IsNullOrEmpty(req.securityid)?drv.SecurityID:req.securityid);
                    if (updateDic.ContainsKey(PTExecOrderProcesser.Side))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.Side, out para))
                        {
                            req.side = para;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.LastQty))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.LastQty, out para))
                        {
                            req.qty = para;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.TradeDate))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.TradeDate, out para))
                        {
                            req.tradedate = para;
                            isrefresh = true;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.SettlType))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.SettlType, out para))
                        {
                            req.settltype = para;
                            isrefresh = true;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.CounterPartyName))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.CounterPartyName, out para))
                        {
                            req.counterpartyname = para;
                        }
                        if (updateDic.TryGetValue(PTExecOrderProcesser.CounterPartyid, out para))
                        {
                            req.counterpartyid = para;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.CounterPartySubName))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.CounterPartySubName, out para))
                        {
                            req.counterpartysubname = para;
                        }
                        if (updateDic.TryGetValue(PTExecOrderProcesser.CounterPartySubid, out para))
                        {
                            req.counterpartysubid = para;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.InstitutionName))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.InstitutionName, out para))
                        {
                            req.institutionname = para;
                        }
                        if (updateDic.TryGetValue(PTExecOrderProcesser.InstitutionID, out para))
                        {
                            req.institutionid = para;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.TraderName))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.TraderName, out para))
                        {
                            req.tradername = para;
                        }
                        if (updateDic.TryGetValue(PTExecOrderProcesser.TraderID, out para))
                        {
                            req.traderid = para;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.Seatnumber))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.Seatnumber, out para))
                        {
                            if (isBank)
                            {
                                req.seatnumber = para;
                            }
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.Quotemethod))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.Quotemethod, out para))
                        {
                            if (!isBank)
                            {
                                req.quotemethod = para;
                            }
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.BusinessType))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.BusinessType, out para))
                        {
                            req.businessType = para;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.Remark))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.Remark, out para))
                        {
                            req.remark = para;
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.Bookid))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.Bookid, out para))
                        {
                            req.bookid = para;
                            string newBookname = "";
                            if (PTExecOrderProcesser.Instance.BookDict.TryGetValue(para, out newBookname))
                            {
                                req.bookname = newBookname;
                            }
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.LastPx))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.LastPx, out para))
                        {
                            string oldPrice = string.IsNullOrEmpty(req.price)?"":req.price;
                            if (!string.IsNullOrEmpty(para) && !oldPrice.Equals(para))
                            {
                                req.price = para;
                                req.yield = para;
                                isrefresh = true;
                                req.netprice = CommonUtils.CalBond(string.IsNullOrEmpty(req.securityid)?drv.SecurityID:req.securityid, "4", GetSetttle(string.IsNullOrEmpty(req.settltype) ? drv.SettlType : req.settltype, string.IsNullOrEmpty(req.tradedate) ? drv.TradeDate : req.tradedate), NumChange(para));
                            }
                        }
                    }
                    if (updateDic.ContainsKey(PTExecOrderProcesser.NetPrice))
                    {
                        string para = "";
                        if (updateDic.TryGetValue(PTExecOrderProcesser.NetPrice, out para))
                        {
                            string oldPrice = string.IsNullOrEmpty(req.netprice) ? "" : req.netprice;
                            if (!string.IsNullOrEmpty(para) && !oldPrice.Equals(para))
                            {
                                req.netprice = para;
                                isrefresh = true;
                                isrefreshNetPrice = true;
                            }
                        }
                    }
                    if (isrefresh) 
                    {
                        if (isrefreshNetPrice)
                        {
                            req.price = CommonUtils.CalBond(string.IsNullOrEmpty(req.securityid) ? drv.SecurityID : req.securityid, "2", GetSetttle(string.IsNullOrEmpty(req.settltype) ? drv.SettlType : req.settltype, string.IsNullOrEmpty(req.tradedate) ? drv.TradeDate : req.tradedate), req.netprice);
                            req.yield = req.price;
                        }
                        else 
                        {
                            req.netprice = CommonUtils.CalBond(string.IsNullOrEmpty(req.securityid) ? drv.SecurityID : req.securityid, "4", GetSetttle(string.IsNullOrEmpty(req.settltype) ? drv.SettlType : req.settltype, string.IsNullOrEmpty(req.tradedate) ? drv.TradeDate : req.tradedate), NumChange(req.price));
                        }
                    }
                    reqList.Add(req);
                }
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                    {
                        _IsUpdating = true;
                        PtExecUpdateReplyPo result = PTHandler.Instance.ExecOrderBatchUpdate(reqList);
                        if (result != null && "0000".Equals(result.Code))
                        {
                            Dictionary<string,string> Datas = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Datas);
                            if (Datas != null && Datas.Count > 0) 
                            {
                                StringBuilder builder = new StringBuilder();
                                foreach (string value in Datas.Values) 
                                {
                                    if (!string.IsNullOrEmpty(value)) 
                                    {
                                        builder.Append(value).Append("\n");
                                    }
                                }
                                if (builder.Length > 0) 
                                {
                                    showMessage(string.Format("{0}", builder.ToString()));
                                }
                            }
                            RequestCloseAfterSubmit?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            showMessage(string.Format("批量修改失败，原因：{0}", result.Msg));
                        }
                        _IsUpdating = false;
                    }));
                });
            }
            else 
            {
                showMessage("未选择交易记录");
            }
        }
        private string GetSetttle(string settlTypeStr,string tradeDate)
        {
            //20240418
            string tdate = tradeDate.Substring(4,2)+"."+ tradeDate.Substring(6,2);
            bool isCurrent = tradeDate.Equals(currentDate);
            string settle = string.Empty;
            if ("+1".Equals(settlTypeStr)) 
            {
                //选择+1
                settle = "2";
                if (!isCurrent)
                {
                    settle = tdate + "+1";
                }
            }
            else if ("+0".Equals(settlTypeStr))
            {
                settle = "1";
                if (!isCurrent)
                {
                    settle = tdate + "+0";
                }
            }
            if (string.IsNullOrEmpty(settle))
                settle = "2";//默认+1
            return settle;
        }
        private ObservableCollection<PtExecordersData> DataDT = new ObservableCollection<PtExecordersData>();
        internal void setData(IList selectedItemsList, PTUpdatePo po)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                Mv.Clear();
                convertMsgDetails(selectedItemsList.Count, po);
                if (!string.IsNullOrEmpty(MsgDetails))
                {
                    int i = 0;
                    foreach (PtExecordersData drv in selectedItemsList)
                    {
                        DataDT.Add(drv);
                        //}
                        i++;
                        PTUpdatePo pt = new PTUpdatePo();
                        pt.ExecID = drv.ExecID + "";
                        pt.TradeDate = drv.TradeDate + "";
                        pt.SecurityID = drv.SecurityID + "";
                        pt.Side = drv.Side + "";
                        pt.LastPx = drv.LastPx + "";
                        pt.NetPrice = drv.NetPrice + "";
                        pt.LastQty = drv.LastQty + "";
                        pt.SettlType = drv.SettlType + "";
                        pt.InstitutionName = drv.InstitutionName + "";
                        pt.TraderName = drv.TraderName + "";
                        pt.BusinessType = drv.BusinessType + "";
                        pt.Bookid = drv.Bookid + "";
                        pt.CounterPartyName = drv.CounterPartyName;
                        pt.CounterPartySubName = drv.CounterPartySubName;
                        //true交易所（席位号+约定号）false银行间（报价方式+备注）
                        bool isBank = STC.PT.Plugin.Utils.CommonUtils.IsBank(pt.SecurityID);
                        pt.Details = new ObservableCollection<PTUpdateDetailPo>();
                        foreach (KeyValuePair<string, string> pair in updateDic)
                        {
                            if (pair.Key.Equals(PTExecOrderProcesser.Securityid) && !pair.Value.Equals(pt.SecurityID))
                            {
                                pt.Details.Add(convertDetail(pt.SecurityID, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.Side) && !pair.Value.Equals(pt.Side))
                            {
                                pt.Details.Add(convertDetail(pt.Side.Equals("1") ? "Bid" : "Ofr", pair.Value.Equals("1") ? "Bid" : "Ofr"));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.LastPx) && !pair.Value.Equals(pt.LastPx))
                            {
                                pt.Details.Add(convertDetail(pt.LastPx, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.NetPrice) && !pair.Value.Equals(pt.NetPrice))
                            {
                                pt.Details.Add(convertDetail(pt.NetPrice, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.LastQty) && !pair.Value.Equals(pt.LastQty))
                            {
                                pt.Details.Add(convertDetail(pt.LastQty, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.TradeDate) && !pair.Value.Equals(pt.TradeDate))
                            {
                                pt.Details.Add(convertDetail(pt.TradeDate, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.SettlType) && !pair.Value.Equals(pt.SettlType))
                            {
                                pt.Details.Add(convertDetail(pt.SettlType, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.CounterPartyName) && !pair.Value.Equals(pt.CounterPartyName))
                            {
                                pt.Details.Add(convertDetail(pt.CounterPartyName, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.CounterPartySubName) && !pair.Value.Equals(pt.CounterPartySubName))
                            {
                                pt.Details.Add(convertDetail(pt.CounterPartySubName, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.InstitutionName) && !pair.Value.Equals(pt.InstitutionName))
                            {
                                pt.Details.Add(convertDetail(pt.InstitutionName, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.TraderName) && !pair.Value.Equals(pt.TraderName))
                            {
                                pt.Details.Add(convertDetail(pt.TraderName, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.BusinessType) && !pair.Value.Equals(pt.BusinessType))
                            {
                                pt.Details.Add(convertDetail(ComboSourceUtil.Instance.FindEnum(pt.BusinessType, "businesstype"), ComboSourceUtil.Instance.FindEnum(pair.Value, "businesstype")));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.Seatnumber) && !pair.Value.Equals(pt.Seatnumber))
                            {
                                if (isBank)
                                {
                                    pt.Details.Add(convertDetail(pt.Seatnumber, pair.Value));
                                }
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.Quotemethod) && !pair.Value.Equals(pt.Quotemethod))
                            {
                                if (!isBank)
                                {
                                    pt.Details.Add(convertDetail(ComboSourceUtil.Instance.FindEnum(pt.Quotemethod, "quotemethod_pt"), ComboSourceUtil.Instance.FindEnum(pair.Value, "quotemethod_pt")));
                                }
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.Remark) && !pair.Value.Equals(pt.Remark))
                            {
                                pt.Details.Add(convertDetail(pt.Remark, pair.Value));
                            }
                            else if (pair.Key.Equals(PTExecOrderProcesser.Bookid) && !pair.Value.Equals(pt.Bookid))
                            {
                                string oldBookid = "";
                                if (!string.IsNullOrEmpty(pt.Bookid) && PTExecOrderProcesser.Instance.BookDict.TryGetValue(pt.Bookid, out oldBookid))
                                    ;
                                string newBookid = "";
                                if (PTExecOrderProcesser.Instance.BookDict.TryGetValue(pair.Value, out newBookid))
                                    ;
                                pt.Details.Add(convertDetail(oldBookid, newBookid));
                            }
                        }
                        Mv.Add(pt);
                        this.OnPropertyChanged("Mv");
                    }
                }
            }));
        }
        private PTUpdateDetailPo convertDetail(string oldvalue,string newvalue) 
        {
            PTUpdateDetailPo detailPo = new PTUpdateDetailPo();
            detailPo.OldValue = oldvalue;
            detailPo.NewValue = newvalue;
            return detailPo;
        }
        private void convertMsgDetails(int count,PTUpdatePo po) 
        {
            StringBuilder builder = new StringBuilder();
            int updateFieldCount = 0;
            if (!string.IsNullOrEmpty(po.SecurityID)) 
            {
                builder.Append($"产品代码、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.Securityid,po.SecurityID);
            }
            if (!string.IsNullOrEmpty(po.Side))
            {
                builder.Append($"交易方向、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.Side, po.Side);
            }
            if (!string.IsNullOrEmpty(po.LastPx))
            {
                builder.Append($"成交价格、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.LastPx, po.LastPx);
            }
            if (!string.IsNullOrEmpty(po.NetPrice))
            {
                builder.Append($"净价、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.NetPrice, po.NetPrice);
            }
            if (!string.IsNullOrEmpty(po.LastQty))
            {
                builder.Append($"成交量、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.LastQty, po.LastQty);
            }
            if (!string.IsNullOrEmpty(po.TradeDate))
            {
                builder.Append($"交易日期、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.TradeDate, po.TradeDate);
            }
            if (!string.IsNullOrEmpty(po.SettlType))
            {
                builder.Append($"清算速度、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.SettlType, po.SettlType);
            }
            if (!string.IsNullOrEmpty(po.CounterPartyName))
            {
                builder.Append($"对方机构、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.CounterPartyName, po.CounterPartyName);
                updateDic.Add(PTExecOrderProcesser.CounterPartyid, po.CounterPartyid);
            }
            if (!string.IsNullOrEmpty(po.CounterPartySubName))
            {
                builder.Append($"对方交易员、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.CounterPartySubName, po.CounterPartySubName);
                updateDic.Add(PTExecOrderProcesser.CounterPartySubid, po.CounterPartySubid);
            }
            if (!string.IsNullOrEmpty(po.InstitutionName))
            {
                builder.Append($"客户机构、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.InstitutionName, po.InstitutionName);
                updateDic.Add(PTExecOrderProcesser.InstitutionID, po.ClientInstID);
            }
            if (!string.IsNullOrEmpty(po.TraderName))
            {
                builder.Append($"客户、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.TraderName, po.TraderName);
                updateDic.Add(PTExecOrderProcesser.TraderID, po.ClientID);
            }
            if (!string.IsNullOrEmpty(po.BusinessType))
            {
                builder.Append($"业务类别、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.BusinessType, po.BusinessType);
            }
            if (!string.IsNullOrEmpty(po.Seatnumber))
            {
                builder.Append($"席位号、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.Seatnumber, po.Seatnumber);
            }
            if (!string.IsNullOrEmpty(po.Quotemethod))
            {
                builder.Append($"报价方式、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.Quotemethod, po.Quotemethod);
            }
            if (!string.IsNullOrEmpty(po.Remark))
            {
                builder.Append($"备注、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.Remark, po.Remark);
            }
            if (!string.IsNullOrEmpty(po.Bookid))
            {
                builder.Append($"组合、");
                updateFieldCount++;
                updateDic.Add(PTExecOrderProcesser.Bookid, po.Bookid);
            }
            if (updateFieldCount > 0) 
            {
                builder = builder.Remove(builder.Length -1, 1);
            }
            MsgDetails = builder.ToString();
            SelectCount = count;
        }


        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
        private string NumChange(string price)
        {
            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                d = d / 100;
                price = d.ToString();
            }
            return price;
        }
    }
}
