using com.gateway.log;
using STC.Common.Handler.Ptr;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Execute;
using STC.Common.Utils;
using STC.Execute.Plugin.Handler.Process;
using STC.Execute.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonUtils = STC.Execute.Plugin.Utils.CommonUtils;

namespace STC.Execute.Plugin.ViewModel.Execute
{
    public class AlterHistoryViewModel : BaseViewModel
    {
        public const string Text = "Text";
        public const string UpdateTime = "UpdateTime";
        private object _lockObj = new object();
        public AlterHistoryViewModel()
        {
            ExecOrderProcesser.Instance.OnDetailOrder += Instance_OnDetailOrder;
        }
        public string SecondaryTradeID;
        private void Instance_OnDetailOrder(DataRow obj)
        {
            string id = obj[ExecOrderProcesser.SecondaryTradeID] + "";
            if (string.IsNullOrEmpty(id))
                return;
            if (id.Equals(this.SecondaryTradeID))
            {
                string STID = obj[ExecOrderProcesser.SecondaryTradeID] + "";
                if (STID.Equals(SecondaryTradeID))
                    OnOrder2(obj);
            }
        }

        /// <summary>
        /// 修改历史订单数据源
        /// </summary>
        private DataView _HistiryOrdersDT;
        public DataView HistiryOrdersDT
        {
            get
            {
                lock (_lockObj)
                {
                    if (_HistiryOrdersDT == null)
                    {
                        _HistiryOrdersDT = new DataTable().DefaultView;
                        _HistiryOrdersDT.Table.Columns.Add(Text, typeof(decimal));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.UpdateTime, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.Sender, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.ChangeMsg, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.AllocText, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.SecurityID, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.Side, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.AvgPx, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.AllocQty, typeof(decimal));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.SettlType, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.TradeType, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.CounterPartyName, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.CounterPartySubName, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.SecondaryTradeID, typeof(string));
                        _HistiryOrdersDT.Table.Columns.Add(ExecOrderProcesser.ChangeFlag, typeof(string));
                       // _HistiryOrdersDT.Table.PrimaryKey = new DataColumn[] { _HistiryOrdersDT.Table.Columns[ExecOrderProcesser.SecondaryTradeID], _HistiryOrdersDT.Table.Columns[ExecOrderProcesser.UpdateTime] };
                        //_HistiryOrdersDT.Table.DefaultView.Sort = $"{ExecOrderProcesser.UpdateTime_Sort} desc";
                        this.OnPropertyChanged();
                    }
                    return this._HistiryOrdersDT;
                }
            }
        }

        public void OnOrder2(DataRow dataRowView)
        {
            try
            {
                if (dataRowView != null)
                {//点确认 下面修改记录不展示
                    string TradeType = dataRowView[ExecOrderProcesser.TradeType] + "";
                    string IsShow = dataRowView[ExecOrderProcesser.IsShow] + "";
                    //if ("1".Equals(TradeType) && "1".Equals(IsShow))
                    //    return;
                    lock (_lockObj)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            DataRow dataRow = _HistiryOrdersDT.Table.NewRow();
                            dataRow[Text] = dataRowView[ExecOrderProcesser.Text] + "";
                            dataRow[UpdateTime] = dataRowView[ExecOrderProcesser.UpdateTime] + "";
                            dataRow[ExecOrderProcesser.ChangeMsg] = dataRowView[ExecOrderProcesser.ChangeMsg] + "";
                            dataRow[ExecOrderProcesser.AllocText] = dataRowView[ExecOrderProcesser.AllocText] + "";
                            dataRow[ExecOrderProcesser.SecurityID] = dataRowView[ExecOrderProcesser.SecurityID] + "";
                            dataRow[ExecOrderProcesser.Side] = dataRowView[ExecOrderProcesser.Side] + "";
                            dataRow[ExecOrderProcesser.AvgPx] = dataRowView[ExecOrderProcesser.AvgPx] + "";
                            dataRow[ExecOrderProcesser.AllocQty] = dataRowView[ExecOrderProcesser.AllocQty] + "";
                            dataRow[ExecOrderProcesser.SettlType] = dataRowView[ExecOrderProcesser.SettlType] + "";
                            dataRow[ExecOrderProcesser.TradeType] = dataRowView[ExecOrderProcesser.TradeType] + "";
                            dataRow[ExecOrderProcesser.CounterPartyName] = dataRowView[ExecOrderProcesser.CounterPartyName] + "";
                            dataRow[ExecOrderProcesser.CounterPartySubName] = dataRowView[ExecOrderProcesser.CounterPartySubName] + "";
                            dataRow[ExecOrderProcesser.SecondaryTradeID] = dataRowView[ExecOrderProcesser.SecondaryTradeID] + "";
                            dataRow[ExecOrderProcesser.Sender] = "经纪商";
                            //this._HistiryOrdersDT.Table.Rows.Add(dataRow);
                            this._HistiryOrdersDT.Table.Rows.InsertAt(dataRow, 0);
                            this._HistiryOrdersDT.Table.AcceptChanges();
                        }));
                    }
                }
                else
                    LogHelper.Error("dataRowView is null");
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public void OnOrder(DataRow dataRowView)
        {
            try
            {
                lock (_lockObj)
                {
                    if (dataRowView != null)
                    {
                        string ordStatus = dataRowView[ExecOrderProcesser.TradeStat] + "";
                        if ("已删除".Equals(ordStatus))
                        {
                            this._HistiryOrdersDT.Table.Rows.Clear();
                            this._HistiryOrdersDT.Table.AcceptChanges();
                            return;
                        }
                        ThreadPool.QueueUserWorkItem(o =>
                        {

                            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            SecondaryTradeID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.SecondaryTradeID].ToString());
                            HistoryExecuteOrderCondition sel = new HistoryExecuteOrderCondition();
                            sel.secondaryTradeId = SecondaryTradeID;
                            sel.pageNum = 1;
                            sel.pageSize = 100;
                            ExeOrderSearchResp resp = PtrHandler.Instance.SearchHistoryExeOrder(sel);
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                this._HistiryOrdersDT.Table.Rows.Clear();
                                this._HistiryOrdersDT.Table.AcceptChanges();

                                if (resp != null && "0000".Equals(resp.Code))
                                {
                                    if (resp.Datas != null && resp.Datas.Count > 0)
                                    {
                                        foreach (ExecuteOrder po in resp.Datas)
                                        {
                                            //点确认 下面修改记录不展示
                                            //if ("1".Equals(po.TradeType) && "1".Equals(po.TradeStat))
                                            //    continue;
                                            DataRow Item = _HistiryOrdersDT.Table.NewRow();//this.HistiryOrdersDT.Table.Rows.Find(new object[] { po.SecondaryTradeID });                                      
                                            DataRow dataRow = _HistiryOrdersDT.Table.NewRow();
                                            dataRow[Text] = po.Text;
                                            dataRow[UpdateTime] = ExecOrderProcesser.Instance.parseUpdateDate(po.UpdateTime);
                                            StringBuilder sb = ExecOrderProcesser.Instance.setChange(po.UpdateFields);
                                            if (sb != null && sb.Length > 0)
                                            {
                                                sb.Append("已修改");
                                            }
                                            dataRow[ExecOrderProcesser.ChangeMsg] = sb;//po.UpdateFields;
                                            dataRow[ExecOrderProcesser.AllocText] = ExecOrderProcesser.Instance.SetAllocText(po);
                                            dataRow[ExecOrderProcesser.SecurityID] = po.SecurityID;
                                            dataRow[ExecOrderProcesser.Side] = po.Side;
                                            if (po.AvgPx.HasValue)
                                                dataRow[ExecOrderProcesser.AvgPx] = CommonUtils.FormatNum(po.AvgPx.ToString());
                                            else
                                                dataRow[ExecOrderProcesser.AvgPx] = CommonUtils.FormatNum(po.StipulationValue.ToString());
                                            dataRow[ExecOrderProcesser.AllocQty] = CommonUtils.FormatNum0(po.AllocQty.ToString());
                                            dataRow[ExecOrderProcesser.SettlType] = po.SettlType;
                                            dataRow[ExecOrderProcesser.TradeType] = po.TradeType;
                                            dataRow[ExecOrderProcesser.CounterPartyName] = po.CounterPartyName;
                                            dataRow[ExecOrderProcesser.CounterPartySubName] = po.CounterPartySubName;
                                            dataRow[ExecOrderProcesser.SecondaryTradeID] = po.SecondaryTradeID;
                                            //dataRow[ExecOrderProcesser.UpdateFields] = setChange(po.UpdateFields) ;
                                            dataRow[ExecOrderProcesser.Sender] = "经纪商";
                                            this._HistiryOrdersDT.Table.Rows.Add(dataRow);
                                            this._HistiryOrdersDT.Table.AcceptChanges();
                                        }
                                    }
                                }
                            }));

                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }



        //if (!checkVal(dataRow[SecurityID] + "", executerOrder.SecurityID + ""))
        //{
        //    sb.Append("债券代码 ");
        //}
        //if (!checkVal(dataRow[AllocQty] + "", CommonUtils.FormatNum(executerOrder.AllocQty + "")))
        //{
        //    sb.Append("券面总额 ");
        //}
        //if (!checkVal(dataRow[AvgPx] + "", CommonUtils.FormatNum(executerOrder.AvgPx + "")))
        //{
        //    sb.Append("成交价 ");
        //}
        //if (!checkVal(dataRow[CounterPartyName] + "", executerOrder.CounterPartyName))
        //{
        //    sb.Append("对手方 ");
        //}
        //if (!checkVal(dataRow[CounterPartySubName] + "", executerOrder.CounterPartySubName))
        //{
        //    sb.Append("对手方交易员 ");
        //}
        //if (!checkVal(dataRow[TrdSubType] + "", executerOrder.TrdSubType + ""))
        //{
        //    sb.Append("方向说明 ");
        //}
        //if (!checkVal(dataRow[TrdType] + "", executerOrder.TrdType + ""))
        //{
        //    sb.Append("报价方式 ");
        //}
        //if (!checkVal(dataRow[TradeType] + "", executerOrder.TradeType + ""))
        //{
        //    sb.Append("交易类型 ");
        //}
        //string clean = CommonUtils.FormatNum(executerOrder.CleanPrice + "");
        //if (!checkVal(dataRow[CleanPrice] + "", clean))
        //{
        //    sb.Append("净价 ");
        //}
        //string dirty = CommonUtils.FormatNum(executerOrder.DirtyPrice + "");
        //if (!checkVal(dataRow[DirtyPrice] + "", dirty))
        //{
        //    sb.Append("全价 ");
        //}
        //if (!checkVal(dataRow[AllocText] + "", string.Format("{0} {1}", executerOrder.IndividualAllocID, executerOrder.AllocText)))
        //{
        //    sb.Append("经纪商备注 ");
        //}
        //if (!checkVal(dataRow[Yield] + "", executerOrder.Yield + ""))
        //{
        //    sb.Append("到期收益率 ");
        //}
        //if (!checkVal(dataRow[StipulationValue] + "", executerOrder.StipulationValue + ""))
        //{
        //    sb.Append("行权收益率 ");
        //}
        //string tradeDate = !string.IsNullOrEmpty(executerOrder.TradeDate) && executerOrder.TradeDate.Length > 7 ? (executerOrder.TradeDate.Insert(4, "/").Insert(7, "/")) : executerOrder.TradeDate;
        //if (!checkVal(dataRow[TradeDate] + "", tradeDate))
        //{
        //    sb.Append("交易日期 ");
        //}
        //if (!checkVal(dataRow[SettlType] + "", executerOrder.SettlType + ""))
        //{
        //    sb.Append("清算速度 ");
        //}

        //if (sb.Length > 0)
        //{
        //    sb.Append("已改变");

        //    dataRow[ChangeFlag] = "1";
        //    dataRow[ChangeMsg] = sb.ToString();
        //}
        // }

        public void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("ExecuteOrderViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Clear();
            }
           
        }
        public void Clear()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (this._lockObj)
                    {
                        if (_HistiryOrdersDT != null)
                        {
                            _HistiryOrdersDT.Table.Rows.Clear();
                        }
                    }
                }));
        }
    }
}
