using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Handler.EMS;
using STC.Common.Model.Base;
using STC.Common.Model.EMS;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.DataProcesser
{
    /// <summary>
    /// 我的成交数据处理类
    /// </summary>
    public class MyExecutionProcesser
    {
        /// <summary>
        /// MyExecutionProcesser实例
        /// </summary>
        private static MyExecutionProcesser proc = new MyExecutionProcesser();

        public static MyExecutionProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        /// <summary>
        /// 最新成交事件通知
        /// </summary>
        private Action<object> _OnLastExecutions;
        public event Action<object> OnLastExecutions
        {
            add
            {
                _OnLastExecutions += value;
                value.Invoke(this.MyExecutionsDT.Rows.Count);
            }
            remove
            {
                _OnLastExecutions -= value;
            }
        }

        private volatile bool isInit = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        private MyExecutionProcesser()
        {
            ExecutionsHandler.Instance.OnClear += Instance_OnClear;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~MyExecutionProcesser()
        {
            Dispose();
        }

        /// <summary>
        /// 资源锁
        /// </summary>
        private object lockObj_MyExecutions = new object();
        public object LockObj_MyExecutions
        {
            get
            {
                return lockObj_MyExecutions;
            }
        }

        #region '我的成交'表格字段
        public const string KEY_FIELD = "Key";
        public const string SIDE_FIELD = "Side";
        public const string SECURITYID_FIELD = "SecurityID";
        public const string SYMBOL_FIELD = "Symbol";
        public const string SECURITY_TYPE = "SecurityType";
        public const string DISPLAY_LASTPX_FIELD = "LastPx_Display";
        public const string LASTPX_FIELD = "LastPx";
        public const string NETPRICE_FIELD = "NetPrice";
        public const string DIRTYPRICE_FIELD = "DirtyPrice"; 
        public const string SETTLTYPE_FIELD = "SettlType";
        public const string SETTLDATE_FIELD = "SettlDate";
        public const string CUMQTY_FIELD = "CumQty";
        public const string LASTQTY_FIELD = "LastQty";
        public const string DELIVERYAMOUNT_FIELD = "DeliveryAmount";
        public const string COUNTERPARTYNAME_FIELD = "CounterPartyName";
        public const string COUNTERPARTYSUBNAME_FIELD = "CounterPartySubName";
        public const string EXECID_FIELD = "ExecID";
        public const string VENUES_FIELD = "Venues";
        public const string PARTYNAME_FIELD = "PartyName";
        public const string PARTYSUBNAME_FIELD = "PartySubName";
        public const string EXECUTIONTIME_FIELD = "ExecutionTime";
        public const string STRATEGY_ORDER_ID_FIELD = "StrategyOrderID";
        public const string REFORDERID_FIELD = "RefOrderID";
        public const string SYNC_STATUS_FIELD = "SyncStatus";
        public const string CLORDID_FIELD = "ClOrdID";
        public const string PRICE_FIELD = "Price"; // 撮合参考价，目前存储收益
        public const string ORDERQTY_FIELD = "OrderQty";// 订单量
        public const string ORDERID_FIELD = "OrderID"; // 订单号
        public const string TRADEDATE_FIELD = "TradeDate";
        public const string USERNAME_FIELD = "UserName";
        public const string INSTITUTIONID_FIELD = "InstitutionID";
        public const string INSTITUTIONNAME_FIELD = "InstitutionName";

        public const string TRADEID_FIELD = "TraderID";
        public const string TRADENAME_FIELD = "TraderName";
        public const string INFO6_FIELD = "Info6";//报价的结算方式

        public const string QUANTID_FIELD = "QuantID";
        public const string INTENTIONID_FIELD = "IntentionId"; // 意向单单号
        public const string MARKETINDICATOR_FIELD = "MarketIndicator"; // 合约类型 4=债券，5=国债期货
        public const string OPENCLOSE_FIELD = "OffsetFlag"; //开平仓标志 0=开仓，1=平仓
        public const string SPEC_HEDGE_FIELD = "HedgeFlag"; //投机套保套利 1=投机，2=套利，3=套保
        public const string CAP_ACCOUNT_FIELD = "CapAccount";//资金账号
        public const string BOOKID_FIELD = "BookID";
        public const string BOOKNAME_FIELD = "BookName";
        public const string MINCHANGE_FIELD = "MinChange";
        public const string ACCOUNTID_FIELD = "AccountId";
        public const string ACCOUNTNAME_FIELD = "AccountName";
        public const string TAG1_FIELD = "tag1";//标签
        public const string YieldToExecution_FIELD = "YieldToExecution";
        public const string YieldToExecutionBAK_FIELD = "YieldToExecutionBAK";
        #endregion

        /// <summary>
        /// 我的成交数据源
        /// </summary>
        private DataTable _MyExecutionsDT;
        public DataTable MyExecutionsDT
        {
            get
            {
                lock (LockObj_MyExecutions)
                {
                    if (_MyExecutionsDT == null)
                    {
                        _MyExecutionsDT = new DataTable();
                        _MyExecutionsDT.Columns.Add(KEY_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(SIDE_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(SECURITYID_FIELD, typeof(string)); 
                        _MyExecutionsDT.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(SECURITY_TYPE, typeof(string));
                        _MyExecutionsDT.Columns.Add(DISPLAY_LASTPX_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(LASTPX_FIELD, typeof(decimal));
                        _MyExecutionsDT.Columns.Add(NETPRICE_FIELD, typeof(decimal));
                        _MyExecutionsDT.Columns.Add(DIRTYPRICE_FIELD, typeof(decimal));
                        _MyExecutionsDT.Columns.Add(SETTLTYPE_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(SETTLDATE_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(CUMQTY_FIELD, typeof(long));
                        _MyExecutionsDT.Columns.Add(LASTQTY_FIELD, typeof(decimal));
                        _MyExecutionsDT.Columns.Add(DELIVERYAMOUNT_FIELD, typeof(decimal));
                        _MyExecutionsDT.Columns.Add(COUNTERPARTYNAME_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(COUNTERPARTYSUBNAME_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(EXECID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(VENUES_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(PARTYNAME_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(PARTYSUBNAME_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(EXECUTIONTIME_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(STRATEGY_ORDER_ID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(REFORDERID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(SYNC_STATUS_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(CLORDID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(PRICE_FIELD, typeof(decimal));
                        _MyExecutionsDT.Columns.Add(ORDERQTY_FIELD, typeof(int)); 
                        _MyExecutionsDT.Columns.Add(ORDERID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(TRADEDATE_FIELD, typeof(string)); 
                        _MyExecutionsDT.Columns.Add(USERNAME_FIELD, typeof(string)); 
                        _MyExecutionsDT.Columns.Add(TRADEID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(TRADENAME_FIELD, typeof(string));

                        _MyExecutionsDT.Columns.Add(INFO6_FIELD, typeof(string));

                        _MyExecutionsDT.Columns.Add(INSTITUTIONID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(INSTITUTIONNAME_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(INTENTIONID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(QUANTID_FIELD, typeof(string));

                        _MyExecutionsDT.Columns.Add("ID", typeof(int));

                        _MyExecutionsDT.Columns.Add(MARKETINDICATOR_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(OPENCLOSE_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(SPEC_HEDGE_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(CAP_ACCOUNT_FIELD, typeof(string));
                        //组合ID 组合名称
                        _MyExecutionsDT.Columns.Add(BOOKID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(BOOKNAME_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(MINCHANGE_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(ACCOUNTID_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(ACCOUNTNAME_FIELD, typeof(string));
                        // 标签
                        _MyExecutionsDT.Columns.Add(TAG1_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(YieldToExecution_FIELD, typeof(string));
                        _MyExecutionsDT.Columns.Add(YieldToExecutionBAK_FIELD, typeof(string));

                        _MyExecutionsDT.PrimaryKey = new DataColumn[] { _MyExecutionsDT.Columns[KEY_FIELD] };
                    }
                    return this._MyExecutionsDT;
                }
            }
        }
        
        private ThreadQueue<Common.Model.Fix.Message.ExecutionReport> _ProcessMyExecutionThreadQueue;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (isInit == false)
            {
                if (_ProcessMyExecutionThreadQueue == null)
                {
                    _ProcessMyExecutionThreadQueue = new ThreadQueue<Common.Model.Fix.Message.ExecutionReport>(UpdateMyExecutions);
                }
                //#region 创建测试数据
                //this.GenerateTestData();
                //return;
                //#endregion


                #region 收EMSSvr消息
                //---------------收EMSSvr消息--------------
                //我的成交
                ExecutionsHandler.Instance.OnOwnExecutions += Instance_OnOwnExecutions;
                //---------------------------------------
                #endregion


                isInit = true;
            }
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            ExecutionsHandler.Instance.OnOwnExecutions -= Instance_OnOwnExecutions;

            lock (this.LockObj_MyExecutions)
            {
                if (_MyExecutionsDT != null)
                {
                    _MyExecutionsDT.Rows.Clear();
                    _OnLastExecutions?.Invoke("0");
                }
            }

            if (_ProcessMyExecutionThreadQueue != null)
            {
                _ProcessMyExecutionThreadQueue.Clear();
            }

            isInit = false;
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            this.Clear();

            lock (this.LockObj_MyExecutions)
            {
                if (_MyExecutionsDT != null)
                {
                    _MyExecutionsDT.Clear();
                    _MyExecutionsDT.Dispose();
                    _MyExecutionsDT = null;
                }
            }

            if (_ProcessMyExecutionThreadQueue != null)
            {
                _ProcessMyExecutionThreadQueue.Clear();
                _ProcessMyExecutionThreadQueue.Dispose();
                _ProcessMyExecutionThreadQueue = null;
            }

            isInit = false;
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void ResetClear()
        {
            LogHandler.Info("MyExecutionProcesser ResetClear.");
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Clear();
                this.Init();
            }));
        }

        /// <summary>
        /// 清零数据回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("MyExecutionProcesser Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init();
                }));
            }
        }

        /// <summary>
        /// 我的成交数据回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOwnExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            //UpdateMyExecutions(obj);
            if (this._ProcessMyExecutionThreadQueue != null)
            {
                if (obj.UserName == LocalEnv.UserInfo.UserName)
                {
                    this._ProcessMyExecutionThreadQueue.Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// 更新我的成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMyExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            try
            {
                lock (this.LockObj_MyExecutions)
                {
                    DataRow myExecution = this.MyExecutionsDT.Rows.Find(obj.OrderID);
                    if (myExecution != null)
                    {
                        if (CommonUtils.ConvertToString(myExecution[EXECUTIONTIME_FIELD]).CompareTo(obj.TransactTime) > 0)
                        {
                            // 如果时间早， 就不更新了。
                            return;
                        }
                    }
                    else
                    {
                        myExecution = this.MyExecutionsDT.NewRow();
                        myExecution[KEY_FIELD] = obj.OrderID; // 成交编号
                        if (this.MyExecutionsDT.Rows.Count == 0)
                        {
                            this.MyExecutionsDT.Rows.InsertAt(myExecution, this.MyExecutionsDT.Rows.Count);
                        }
                        else
                        {
                            string firsttime = this.MyExecutionsDT.Rows[0][EXECUTIONTIME_FIELD].ToString();
                            if (CommonUtils.ConvertToString(firsttime).CompareTo(obj.TransactTime) > 0)
                            {
                                this.MyExecutionsDT.Rows.InsertAt(myExecution, this.MyExecutionsDT.Rows.Count);
                            }
                            else
                            {
                                this.MyExecutionsDT.Rows.InsertAt(myExecution, 0);
                            }
                        }
                    }

                    myExecution[SIDE_FIELD] = obj.Side; // 方向
                    myExecution[SECURITYID_FIELD] = obj.SecurityID; // 代码
                    myExecution[SYMBOL_FIELD] = obj.Symbol; // 简称，可能要从基本数据里取

                    myExecution[SECURITY_TYPE] = obj.SecurityType;

                    myExecution[DISPLAY_LASTPX_FIELD] = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                    myExecution[LASTPX_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);// 成交价；
                    myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价
                    if(Consts.MARKETINDICATOR_FUTURE.Equals(obj.MarketIndicator))
                    {
                        //国债期货 成交价(%) 不显示
                        myExecution[LASTPX_FIELD] = DBNull.Value;
                        //国债期货净价=成交价
                        myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);
                    }
                    decimal dirtyPrice = CommonUtils.ConvertToDecimal(obj.DirtyPrice);
                    myExecution[DIRTYPRICE_FIELD] = dirtyPrice;// 全价
                    myExecution[SETTLTYPE_FIELD] = obj.SettlType; // 清算速度
                    //myExecution[SETTLDATE_FIELD] = obj.SettlDate; // 交割日
                    myExecution[CUMQTY_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);//总成交量
                    decimal lastQty = CommonUtils.ConvertToDecimal(obj.LastQty);
                    myExecution[LASTQTY_FIELD] = lastQty; // 成交量
                    myExecution[DELIVERYAMOUNT_FIELD] = dirtyPrice * lastQty;//结算金额,结算金额=全价 *成交量
                    myExecution[COUNTERPARTYNAME_FIELD] = obj.CounterPartyName;//对手方
                    myExecution[COUNTERPARTYSUBNAME_FIELD] = obj.CounterPartySubName;// 对手交易员
                    myExecution[EXECID_FIELD] = obj.ExecID;//成交编号
                    myExecution[VENUES_FIELD] = obj.Venues;// 交易场所
                    myExecution[PARTYNAME_FIELD] = obj.PartyName;//本方交易机构
                    myExecution[PARTYSUBNAME_FIELD] = obj.PartySubName;//本方交易员
                    myExecution[EXECUTIONTIME_FIELD] = obj.TransactTime;// 成交时间
                    myExecution[STRATEGY_ORDER_ID_FIELD] = obj.StrategyOrderId;//我的订单号 
                    myExecution[REFORDERID_FIELD] = obj.CounterPartyOrdID;//对手订单号
                    myExecution[SYNC_STATUS_FIELD] =  obj.SynchFlag; // 同步状态
                    myExecution[CLORDID_FIELD] = obj.ClOrdID; //clOrdID
                    myExecution[ORDERID_FIELD] = obj.OrderID; //OrderID

                    myExecution[INSTITUTIONID_FIELD] = obj.InstitutionID;
                    myExecution[INSTITUTIONNAME_FIELD] = obj.InstitutionName;
                    myExecution[TRADEDATE_FIELD] = obj.TradeDate; //TradeDate
                    myExecution[USERNAME_FIELD] = obj.UserName;

                    myExecution[INFO6_FIELD] = obj.Info6;

                    myExecution[TRADEID_FIELD] = obj.TraderID;
                    myExecution[TRADENAME_FIELD] = obj.TraderName;

                    myExecution[QUANTID_FIELD] = obj.QuantID;

                    myExecution[ORDERQTY_FIELD] = CommonUtils.ConvertToInt(obj.OrderQty); //  订单量
                    myExecution[PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Price); //  撮合参考价，目前存储收益
                    myExecution[INTENTIONID_FIELD] = obj.Info4;
                    if (string.IsNullOrEmpty(obj.Info4) == false)
                    {
                        STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo intentionQuoteInfo = IntentionOldHandler.Instance.GetIntentionByID(obj.Info4);
                        if (intentionQuoteInfo != null)
                        {
                            myExecution[INSTITUTIONID_FIELD] = intentionQuoteInfo.institutionId;
                            myExecution[INSTITUTIONNAME_FIELD] = intentionQuoteInfo.institutionName;
                            myExecution[TRADEID_FIELD] = intentionQuoteInfo.traderId;
                            myExecution[TRADENAME_FIELD] = intentionQuoteInfo.traderName;
                        }
                    }

                    string newSettleType = string.Empty;
                    //string settleDate = CommonUtils.CalSettlDate(DateTimeEx.ServerTime.ToString("yyyyMMdd"), obj.SettlType, out newSettleType);
                    myExecution[SETTLDATE_FIELD] = obj.SettlDate;
                    //myExecution[VENUES_FIELD] = "IDB";// 交易场所
                    //myExecution[SYNC_STATUS_FIELD] = "S"; // 同步状态

                    myExecution[MARKETINDICATOR_FIELD] = obj.MarketIndicator;
                    //开平仓标志
                    myExecution[OPENCLOSE_FIELD] = CommonUtils.GetOffsetFlagDisplay(obj.OffsetFlag);
                    //投机套保套利 1=投机，2=套利，3=套保
                    switch (obj.HedgeFlag)
                    {
                        case Consts.SPECULATE_CODE:
                            myExecution[SPEC_HEDGE_FIELD] = Consts.SPECULATE;
                            break;
                        case Consts.ARBITRAGE_CODE:
                            myExecution[SPEC_HEDGE_FIELD] = Consts.ARBITRAGE;
                            break;
                        case Consts.HEDGE_CODE:
                            myExecution[SPEC_HEDGE_FIELD] = Consts.HEDGE;
                            break;
                    }
                    //资金账号暂时没有
                    //myExecution[CAP_ACCOUNT_FIELD] = obj.HedgeFlag;
                    myExecution[BOOKID_FIELD] = obj.BookID;
                    myExecution[BOOKNAME_FIELD] = (obj.BookID != null && MyOrderProcesser.Processer.BookDict.ContainsKey(obj.BookID)) ? MyOrderProcesser.Processer.BookDict[obj.BookID] : obj.BookID;
                    //默认4位小数,最小价格变动单位
                    myExecution[MINCHANGE_FIELD] = CommonUtils.GetMinChangeBySecurityID(obj.SecurityID);
                    //账户组合
                    myExecution[ACCOUNTID_FIELD] = string.IsNullOrEmpty(obj.AccountID) ? null : obj.AccountID;
                    myExecution[ACCOUNTNAME_FIELD] = string.IsNullOrEmpty(obj.AccountName) ? null : obj.AccountName;
                    // 标签
                    myExecution[TAG1_FIELD] = obj.Tag1;
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(obj.SecurityID);
                    if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                    {
                        myExecution[YieldToExecution_FIELD] = CommonUtils.FormatPrice(obj.YieldToExecution);
                    }
                    myExecution[YieldToExecutionBAK_FIELD] = CommonUtils.FormatPrice(obj.YieldToExecution);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateMyExecutions Method", e);
            }
            finally
            {
                _OnLastExecutions?.Invoke(this.MyExecutionsDT.Rows.Count);
            }
        }

        /// <summary>
        /// 更新客户信息
        /// </summary>
        public void UpdateCustomInfo(string intentionID, string institutionId, string institutionName,
            string traderID, string traderName)
        {
            lock (this.LockObj_MyExecutions)
            {
                foreach(DataRow dr in this.MyExecutionsDT.Rows)
                {
                    string intentionIDs = dr[INTENTIONID_FIELD].ConvertToString();
                    if (!string.IsNullOrEmpty(intentionIDs) && intentionIDs.Contains(intentionID))
                    {
                        string[] intentionsArr = intentionIDs.Split(CommonConstants.COMMA_CHAR);
                        int index = 0;
                        foreach (string item in intentionsArr)
                        {
                            if (item == intentionID)
                            {
                                break;
                            }
                            index++;
                        }

                        dr[INSTITUTIONID_FIELD] = GenNewText(dr[INSTITUTIONID_FIELD].ConvertToString(), institutionId, index);
                        dr[INSTITUTIONNAME_FIELD] = GenNewText(dr[INSTITUTIONNAME_FIELD].ConvertToString(), institutionName, index);
                        dr[TRADEID_FIELD] = GenNewText(dr[TRADEID_FIELD].ConvertToString(), traderID, index);
                        dr[TRADENAME_FIELD] = GenNewText(dr[TRADENAME_FIELD].ConvertToString(), traderName, index);

                        // 更新状态
                        ThreadPoolHelper.InvokeThreadPool((o) => {
                            if (!string.IsNullOrEmpty(traderName) && !string.IsNullOrEmpty(institutionName))
                            {
                                string exeid = dr[ORDERID_FIELD].ConvertToString();
                                string price = dr[PRICE_FIELD].ConvertToString();
                                string yield = dr[LASTPX_FIELD].ConvertToString();
                                string qty = dr[LASTQTY_FIELD].ConvertToString();
                                string settlType = dr[SETTLTYPE_FIELD].ConvertToString();

                                UpdateOrderReq req = new UpdateOrderReq();
                                req.TraderID = traderID;
                                req.TraderName = traderName;
                                req.InstitutionID = institutionId;
                                req.InstitutionName = institutionName;
                                req.OrderId = exeid;
                                req.IntentionId = intentionID;

                                
                                req.Price = price;
                                req.Info1 = yield;
                                req.Info2 = qty;
                                req.Info5 = "MANUAL";
                                req.TradeMethod = "60";
                                req.Info6 = settlType;


                                PResult result = EMSHandler.Instance.updateOrder(req);

                                if (result == null)
                                {
                                    CommonUtils.Prompt2Second("警告", $"意向id:[{intentionID}]修改失败");
                                }
                                else
                                {
                                    if (!result.IsSuccess())
                                    {
                                        CommonUtils.Prompt2Second("警告", result.Msg);
                                    }
                                }
                            }
                        });
                    }
                }
            }
        }

        private string GenNewText(string oldText, string newPartText, int index)
        {
            string[] arr = oldText.Split(CommonConstants.COMMA_CHAR);
            arr[index] = newPartText;
            StringBuilder tempSb = new StringBuilder();
            foreach (string item in arr)
            {
                if (tempSb.Length > 0)
                {
                    tempSb.Append(CommonConstants.COMMA_CHAR);
                }
                tempSb.Append($"{item}");
            }
            return tempSb.ToString();
        }

        /// <summary>
        /// 生成测试数据
        /// </summary>
        private void GenerateTestData()
        {
            // -------- 创建测试数据 -------------

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                // 我的成交
                Common.Model.Fix.Message.ExecutionReport myExe = new Common.Model.Fix.Message.ExecutionReport();
                myExe.AvgPx = "0";// 平均价
                myExe.ClOrdID = "681916606795743232"; // 客户端编号
                myExe.CumQty = "0"; // 总成交量
                myExe.ExecID = "SOR681916606930063360"; //成交编号
                myExe.Info1 = "2020-02-25 17:33:08"; // 
                myExe.LastPx = "0";// 成交价
                myExe.LastQty = "0"; // 成交量
                myExe.LeavesQty = "1000"; //剩余量  
                myExe.MarketIndicator = "4";//市场：债券笃定填写4
                myExe.OrdRejReason = "";//错误代码
                myExe.OrdStatus = "0";//
                                      //                订单状态
                                      //0 = 有效（正常）
                                      //1 = 部分成交
                                      //2 = 成交
                                      //4 = 撤销
                                      //5 = 拒绝
                                      //8 = 无效（下单被拒绝）
                                      //I = 冻结
                                      //21 = 过期
                myExe.OrdType = "2";//订单类型
                                    //2 = 限价订单

                myExe.OrderID = "emse681916606938353664";//订单编号
                myExe.OrderQty = "1000";//订单量
                myExe.PartyID = "";//本机构ID
                myExe.PartyName = "";//本机构名
                myExe.PartySubID = "songqihu@sxzq.com";//本交易员ID
                myExe.PartySubName = "";//对手方机构名
                myExe.Price = "4.2000";//戳合参考价价，目前存储收益率
                myExe.SecurityID = "180210.IB";//债券代码
                myExe.SettlType = "2";//清算速度
                myExe.Side = "1";//方向
                //1 = Bid
                //2 = Ofr
                myExe.StandardHeader.MsgType = "8";
                myExe.StrategyOrderId = "s681916606778966016";
                myExe.Symbol = "18国开10";
                myExe.TimeInForce = "3";
                myExe.VenueTypeGW = "SOR";
                myExe.Venues = "TPSC";

                //净价不一定有
                myExe.NetPrice = "0";

                //UpdateMyExecutions(myExe);

            });

            //-------------------------------------
        }
    }
}
