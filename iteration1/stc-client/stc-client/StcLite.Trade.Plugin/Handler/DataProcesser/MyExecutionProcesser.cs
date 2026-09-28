using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BDB.Trade.Plugin.Utils;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;

namespace BDB.Trade.Plugin.Handler.DataProcesser
{
    public class MyExecutionProcesser
    {
        private static MyExecutionProcesser proc = new MyExecutionProcesser();

        public static MyExecutionProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        private volatile bool isInit = false;

        private MyExecutionProcesser()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }
        ~MyExecutionProcesser()
        {
            Dispose();
        }

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
        public const string INSTITUTIONNAME_FIELD = "InstitutionName";

        public const string TRADENAME_FIELD = "TraderName";
        public const string INFO6_FIELD = "Info6";//报价的结算方式

        #endregion

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
                        _MyExecutionsDT.Columns.Add(LASTQTY_FIELD, typeof(long));
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
                        _MyExecutionsDT.Columns.Add(TRADENAME_FIELD, typeof(string));

                        _MyExecutionsDT.Columns.Add(INFO6_FIELD, typeof(string));

                        _MyExecutionsDT.Columns.Add(INSTITUTIONNAME_FIELD, typeof(string));

                        _MyExecutionsDT.PrimaryKey = new DataColumn[] { _MyExecutionsDT.Columns[KEY_FIELD] };
                    }
                    return this._MyExecutionsDT;
                }
            }
        }
        
        private AsyncThreadQueue<ExecutionReport> _ProcessMyExecutionThreadQueue;

        public void Init()
        {
            if (isInit == false)
            {
                if (_ProcessMyExecutionThreadQueue == null)
                {
                    _ProcessMyExecutionThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMyExecutions);
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

        public void Clear()
        {
            ExecutionsHandler.Instance.OnOwnExecutions -= Instance_OnOwnExecutions;

            lock (this.LockObj_MyExecutions)
            {
                if (_MyExecutionsDT != null)
                {
                    _MyExecutionsDT.Rows.Clear();
                }
            }

            if (_ProcessMyExecutionThreadQueue != null)
            {
                _ProcessMyExecutionThreadQueue.Clear();
            }

            isInit = false;
        }

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

        public void ResetClear()
        {
            LogHandler.Info("MyExecutionProcesser ResetClear.");
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Clear();
                this.Init();
            }));
        }

        private void Instance_OnClear(ClearPo obj)
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


        private void Instance_OnOwnExecutions(ExecutionReport obj)
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
        private void UpdateMyExecutions(ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
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
                            this.MyExecutionsDT.Rows.Add(myExecution);
                        }

                        myExecution[SIDE_FIELD] = obj.Side; // 方向
                        myExecution[SECURITYID_FIELD] = obj.SecurityID; // 代码
                        myExecution[SYMBOL_FIELD] = obj.Symbol; // 简称，可能要从基本数据里取

                        //myExecution[SECURITY_TYPE] = obj.SecurityType;

                        myExecution[DISPLAY_LASTPX_FIELD] = CommonUtils.FormatPrice(obj.Yield);// 用于显示的成交价
                        myExecution[LASTPX_FIELD] = CommonUtils.ConvertToDecimal(obj.Yield);// 成交价；
                        myExecution[NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价
                        decimal dirtyPrice = CommonUtils.ConvertToDecimal(obj.DirtyPrice);
                        myExecution[DIRTYPRICE_FIELD] = dirtyPrice;// 全价
                        myExecution[SETTLTYPE_FIELD] = obj.SettlType; // 清算速度
                        myExecution[SETTLDATE_FIELD] = obj.SettlDate; // 交割日
                        myExecution[CUMQTY_FIELD] = CommonUtils.ConvertToLong(obj.CumQty);//总成交量
                        long lastQty = CommonUtils.ConvertToLong(obj.LastQty);
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
                        myExecution[SYNC_STATUS_FIELD] = obj.SynchFlag; // 同步状态
                        myExecution[CLORDID_FIELD] = obj.ClOrdID; //clOrdID
                        myExecution[ORDERID_FIELD] = obj.OrderID; //OrderID

                        myExecution[INSTITUTIONNAME_FIELD] = obj.InstitutionName;
                        myExecution[TRADEDATE_FIELD] = obj.TradeDate; //TradeDate
                        myExecution[USERNAME_FIELD] = obj.UserName;

                        myExecution[INFO6_FIELD] = obj.Info6;

                        myExecution[TRADENAME_FIELD] = obj.TraderName;

                        myExecution[ORDERQTY_FIELD] = CommonUtils.ConvertToInt(obj.OrderQty); //  订单量
                        myExecution[PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Price); //  撮合参考价，目前存储收益

                        //myExecution[VENUES_FIELD] = "IDB";// 交易场所
                        //myExecution[SYNC_STATUS_FIELD] = "S"; // 同步状态

                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("UpdateMyExecutions Method", e);
                }
            }));
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
                ExecutionReport myExe = new ExecutionReport();
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
