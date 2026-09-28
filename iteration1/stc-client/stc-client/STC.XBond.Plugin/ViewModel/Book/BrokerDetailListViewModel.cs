using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Api;
using STC.Common.Handler.BookManagement;
using STC.Common.Model.BookManagement;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// BrokerDetailListView的数据内容
    /// </summary>
    public class BrokerDetailListViewModel : ViewModelBase
    {
        /// <summary>
        /// 需要需要添加数据的数据源 
        /// </summary>
        public DataTable TargetDatas = null;

        public BookEditViewModel bookEditViewModel;

        public string Broker;

        public string SecurityID;

        public string Side;

        /// <summary>
        /// 选择数量
        /// </summary>
        private long _SelectedQty = 0;

        public long SelectedQty
        {
            get
            {
                return _SelectedQty;
            }
            set
            {
                if (_SelectedQty != value)
                {
                    _SelectedQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 成交总量
        /// </summary>
        private long _TotalQty = 0;

        public long TotalQty
        {
            get
            {
                return _TotalQty;
            }
            set
            {
                if (_TotalQty != value)
                {
                    this._TotalQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public const string ISCHECKED_FIELD = "IsChecked";
        public const string SOURCETRADER_FIELD = "SourceTrader";
        public const string EXECID_FIELD = "ExecID";
        public const string MATCHCODE_FIELD = "MatchCode";
        public const string LASTPRICE_FIELD = "LastPrice";
        public const string LEAVESQTY_FILED = "LeavesQty";
        public const string LASTQTY_FIELD = "LastQty";
        public const string MATCHQTY_FIELD = "MatchQty";
        public const string SETTLTYPE_FIELD = "SettlType";
        public const string COUNTERPARTY_FIELD = "CounterPart";
        public const string COUNTERPARTYID_FIELD = "CounterPartyID";
        public const string COUNTERPARTYSUBNAME_FIELD = "CounterPartSubName";
        public const string COUNTERPARTYSUBID_FIELD = "CounterPartSubID";
        public const string TRADEDATE_FIELD = "TradeDate";
        public const string SETTLDATE_FIELD = "settlDate";
        public const string CREATEDATE_FIELD = "createDate";
        public const string DCSEXECID_FIELD = "dcsExecID";
        public const string JSONINFO_FILED = "jsonInfo";


        public DataTable _datas;

        public DataTable Datas
        {
            get
            {
                if (_datas == null)
                {
                    _datas = new DataTable();
                    _datas.Columns.Add(ISCHECKED_FIELD, typeof(bool));
                    _datas.Columns.Add(SOURCETRADER_FIELD, typeof(string));
                    _datas.Columns.Add(EXECID_FIELD, typeof(string));
                    _datas.Columns.Add(MATCHCODE_FIELD, typeof(string));
                    _datas.Columns.Add(LASTPRICE_FIELD, typeof(string));
                    _datas.Columns.Add(LASTQTY_FIELD, typeof(long));
                    _datas.Columns.Add(LEAVESQTY_FILED, typeof(long));
                    _datas.Columns.Add(MATCHQTY_FIELD, typeof(string));
                    _datas.Columns.Add(SETTLTYPE_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTERPARTYID_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTERPARTY_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTERPARTYSUBID_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTERPARTYSUBNAME_FIELD, typeof(string));
                    _datas.Columns.Add(TRADEDATE_FIELD, typeof(string));
                    _datas.Columns.Add(SETTLDATE_FIELD, typeof(string));
                    _datas.Columns.Add(CREATEDATE_FIELD, typeof(string));
                    _datas.Columns.Add(DCSEXECID_FIELD, typeof(string));
                    _datas.Columns.Add(JSONINFO_FILED, typeof(string));
                    this.OnPropertyChanged();
                }
                return _datas;
            }
        }


        public BrokerDetailListViewModel()
        {
            //this.GenerateTestDatas();
            //LoadBrokers();
            
        }

        /// <summary>
        /// 加载经纪商数据
        /// </summary>
        public async void LoadBrokers()
        {
            await Task.Run(() =>
            {
                try
                {
                    SorExecAckQueryParas request = new SorExecAckQueryParas();
                    int index = 0;
                    request.pageSize = 1000;
                    request.venues = this.Broker;
                    request.securityid = this.SecurityID;


                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取broker订单 
                        BlockQueryReplyPo reply = BookMgtHandler.Instance.SorExecAckQuery(request);// ClientApi.Instance.SorExecAckQuery(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            if (reply != null && reply.datas != null && reply.datas.Count > 0)
                            {
                                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    foreach (ExecutionReport obj in reply.datas)
                                    {
                                        DataRow dr = Datas.NewRow();
                                        dr[ISCHECKED_FIELD] = false;
                                        dr[SOURCETRADER_FIELD] = obj.Venues;
                                        dr[EXECID_FIELD] = obj.OrderID;
                                        dr[MATCHCODE_FIELD] = obj.BrokerExecID;
                                        dr[LASTPRICE_FIELD] = obj.LastPx;
                                        dr[LASTQTY_FIELD] = CommonUtils.ConvertToDecimal(obj.LastQty);
                                        dr[LEAVESQTY_FILED] = CommonUtils.ConvertToLong(obj.LeavesQty);
                                        dr[MATCHQTY_FIELD] = CommonUtils.ConvertToLong(obj.LeavesQty).ToString();
                                        dr[SETTLTYPE_FIELD] = obj.SettlType;
                                        dr[COUNTERPARTYID_FIELD] = obj.CounterPartyID;
                                        dr[COUNTERPARTY_FIELD] = obj.CounterPartyName;
                                        dr[COUNTERPARTYSUBID_FIELD] = obj.CounterPartySubID;
                                        dr[COUNTERPARTYSUBNAME_FIELD] = obj.CounterPartySubName;
                                        dr[TRADEDATE_FIELD] = obj.TradeDate;
                                        dr[SETTLDATE_FIELD] = obj.SettlDate;
                                        dr[CREATEDATE_FIELD] = obj.TransactTime;
                                        dr[DCSEXECID_FIELD] = obj.DcsexecID;
                                        dr[JSONINFO_FILED] = MsgHelper.Serializer(obj);
                                        Datas.Rows.Add(dr);
                                        this.TotalQty++;
                                    }
                                }));
                            }
                            if (reply.datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("LoadBrokers", e);
                }
            });
        }

        //private void GenerateTestDatas()
        //{
        //    this.TotalQty = 4000;   

        //    for(int i = 0; i< 10; i++)
        //    {
        //        DataRow dr = Datas.NewRow();
        //        dr[ISCHECKED_FIELD] = false;
        //        dr[SOURCETRADER_FIELD] = "CTTJ";
        //        dr[EXECID_FIELD] = $"{i}";
        //        dr[MATCHCODE_FIELD] = $"xxxx{i}";
        //        dr[LASTPRICE_FIELD] = "2.34";
        //        dr[LASTQTY_FIELD] = 1000;
        //        dr[MATCHQTY_FIELD] = 1000;
        //        dr[SETTLTYPE_FIELD] = "2";
        //        dr[COUNTERPARTY_FIELD] = "xxxxxx";
        //        Datas.Rows.Add(dr);
        //    }
        //}
		
        /// <summary>
        /// 确认按钮逻辑
        /// </summary>
        /// <returns></returns>
		public bool Finish()
        {
            if (TargetDatas == null)
            {
                Common.View.Component.CustomMessageBox.Info("程序异常！", "添加代客IDB订单");
                return false;
            }
            //  判断勾选数量
            if (TargetDatas != null)
            {
                int count = 0;
                foreach (DataRow dr in this.Datas.Rows)
                {
                    if ((bool)dr[ISCHECKED_FIELD])
                    {
                        count++;
                    }
                }

                if ((TargetDatas.Rows.Count + count) > 10)
                {
                    // 添加数据超出10条限制，请检查
                    Common.View.Component.CustomMessageBox.Info("添加数据超出10条限制，请检查", "添加经纪商成交单");
                    return false;
                }
            }

            // 添加数据
            foreach (DataRow addDr in this.Datas.Rows)
            {

                if ((bool)addDr[ISCHECKED_FIELD])
                {
                    DataRow[] findDrs = this.TargetDatas.Select($"{BookEditItemViewModel.DCSEXEID_FIELD} = '{addDr[DCSEXECID_FIELD]}'");
                    //findDrs = this.TargetDatas.Select($"{BookEditItemViewModel.BROKEREXECID_FIELD} = '409'");
                    
                    for (int i = 0; i < addDr.ItemArray.Length; i++)
                    {
                        LogHelper.Info(string.Format("{0}", addDr[i]));
                    }

                    if (findDrs != null && findDrs.Length > 0)
                    {
                        DataRow find_broker = findDrs[0];
                        find_broker[BookEditItemViewModel.SOURCETRADER_FIELD] = addDr[SOURCETRADER_FIELD];
                        find_broker[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD] = addDr[COUNTERPARTYID_FIELD];
                        find_broker[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = addDr[COUNTERPARTY_FIELD];
                        find_broker[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD] = addDr[COUNTERPARTYSUBID_FIELD];
                        find_broker[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = addDr[COUNTERPARTYSUBNAME_FIELD];
                        find_broker[BookEditItemViewModel.YIELD_FIELD] = addDr[LASTPRICE_FIELD];
                        long qty = CommonUtils.ConvertToLong(find_broker[BookEditItemViewModel.QTY_FIELD].ToString());
                        long originalQty = CommonUtils.ConvertToLong(find_broker[BookEditItemViewModel.EDIT_QTY_FIELD].ToString());
                        long matchQty = CommonUtils.ConvertToLong(addDr[MATCHQTY_FIELD].ToString());
                        find_broker[BookEditItemViewModel.QTY_FIELD] = (originalQty + matchQty).ToString();
                        find_broker[BookEditItemViewModel.SETTLETYPE_FIELD] = addDr[SETTLTYPE_FIELD];
                        find_broker[BookEditItemViewModel.TRADEDATE_FIELD] = addDr[TRADEDATE_FIELD];
                        find_broker[BookEditItemViewModel.SETTLDATE_FIELD] = addDr[SETTLDATE_FIELD];
                        find_broker[BookEditItemViewModel.DCSEXEID_FIELD] = addDr[DCSEXECID_FIELD];
                        find_broker[BookEditItemViewModel.JSONINFO_FIELD] = addDr[JSONINFO_FILED];
                        find_broker[BookEditItemViewModel.BROKEREXECID_FIELD] = addDr[MATCHCODE_FIELD];
                        find_broker[BookEditItemViewModel.CREATEDATE_FIELD] = addDr[CREATEDATE_FIELD];
                        string createdate = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(find_broker[BookEditItemViewModel.CREATEDATE_FIELD])).ToString("yyyy-MM-dd");
                        string currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                        if (createdate.Equals(currentdate))
                        {
                            find_broker[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "0";
                        }
                        else
                        {
                            find_broker[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "1";
                        }
                        if (bookEditViewModel != null)
                        {
                            bookEditViewModel.CounterPartQty -= qty;
                            bookEditViewModel.CounterPartQty += (originalQty + matchQty);
                        }
                    }
                    else
                    {
                        DataRow dr_broker = this.TargetDatas.NewRow();
                        //dr_broker[BookEditItemViewModel.EXECID_FIELD] = addDr[EXECID_FIELD];
                        dr_broker[BookEditItemViewModel.SOURCETRADER_FIELD] = addDr[SOURCETRADER_FIELD];
                        dr_broker[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD] = addDr[COUNTERPARTYID_FIELD];
                        dr_broker[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = addDr[COUNTERPARTY_FIELD];
                        dr_broker[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD] = addDr[COUNTERPARTYSUBID_FIELD];
                        dr_broker[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = addDr[COUNTERPARTYSUBNAME_FIELD];
                        dr_broker[BookEditItemViewModel.YIELD_FIELD] = addDr[LASTPRICE_FIELD];
                        dr_broker[BookEditItemViewModel.QTY_FIELD] = addDr[MATCHQTY_FIELD];
                        dr_broker[BookEditItemViewModel.SETTLETYPE_FIELD] = addDr[SETTLTYPE_FIELD];
                        dr_broker[BookEditItemViewModel.TRADEDATE_FIELD] = addDr[TRADEDATE_FIELD];
                        dr_broker[BookEditItemViewModel.SETTLDATE_FIELD] = addDr[SETTLDATE_FIELD];
                        dr_broker[BookEditItemViewModel.DCSEXEID_FIELD] = addDr[DCSEXECID_FIELD];
                        dr_broker[BookEditItemViewModel.JSONINFO_FIELD] = addDr[JSONINFO_FILED];
                        dr_broker[BookEditItemViewModel.BROKEREXECID_FIELD] = addDr[MATCHCODE_FIELD];
                        dr_broker[BookEditItemViewModel.CREATEDATE_FIELD] = addDr[CREATEDATE_FIELD];
                        string createdate = CommonUtils.ConvertToDateTime(CommonUtils.ConvertToString(dr_broker[BookEditItemViewModel.CREATEDATE_FIELD])).ToString("yyyy-MM-dd");
                        string currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                        if (createdate.Equals(currentdate))
                        {
                            dr_broker[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "0";
                        }
                        else
                        {
                            dr_broker[BookEditItemViewModel.CREATEDATEVIS_FIELD] = "1";
                        }
                        if (bookEditViewModel != null)
                        {
                            long qty = CommonUtils.ConvertToLong(dr_broker[BookEditItemViewModel.QTY_FIELD] == null ? "0" : dr_broker[BookEditItemViewModel.QTY_FIELD].ToString());
                            bookEditViewModel.CounterPartQty += qty;
                        }
                        this.TargetDatas.Rows.Add(dr_broker);
                    }
                }
            }
            this.TargetDatas.AcceptChanges();
            return true;
        }
    }
}
