using com.gateway.log;
using STC.Common.DataProcesser;
using STC.Common.Handler.Cal;
using STC.Common.Handler.MarketExec;
using STC.Common.Model.Cal;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 我的订单中明细单浮框CrossQtyPopupView的数据内容
    /// </summary>
    public class CrossQtyPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// 执行类型
        /// </summary>
        private string _sendExecType = "OCO";
        public string SendExecType
        {
            get
            {
                return _sendExecType;
            }
            set
            {
                _sendExecType = value;
                this.OnPropertyChanged();
            }
        }

        private string _MarketIndicator;
        public string MarketIndicator
        {
            get
            {
                return _MarketIndicator;
            }
            set
            {
                _MarketIndicator = value;
                this.OnPropertyChanged();
            }
        }
        //量(万)/量(手)
        private string _HeaderAmount;
        public string HeaderAmount
        {
            get
            {
                return _HeaderAmount;
            }
            set
            {
                _HeaderAmount = value;
                this.OnPropertyChanged();
            }
        }

        private string _SecurtityID;

        public string SecurtityID
        {
            get
            {
                return _SecurtityID;
            }

            set
            {
                _SecurtityID = value;
                this.OnPropertyChanged();
            }
        }

        private string _Symbol;
        public string Symbol
        {
            get
            {
                return _Symbol;
            }

            set
            {
                _Symbol = value;
                this.OnPropertyChanged();
            }
        }


        private string _STCCode;
        public string STCCode
        {
            get
            {
                return _STCCode;
            }

            set
            {
                _STCCode = value;
                this.OnPropertyChanged();
            }
        }

        private long _ExeQty;
        public long ExeQty
        {
            get
            {
                return _ExeQty;
            }

            set
            {
                _ExeQty = value;
                this.OnPropertyChanged();
            }
        }

        
        private decimal _TotalQty;
        public decimal TotalQty
        {
            get
            {
                return _TotalQty;
            }

            set
            {
                _TotalQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _SettlType;
        public string SettleType
        {
            get
            {
                return _SettlType;
            }
            set
            {
                _SettlType = value;
                this.OnPropertyChanged();
            }
        }

        private string _Side;

        public string Side
        {
            get
            {
                return _Side;
            }
            set
            {
                _Side = value;
                this.OnPropertyChanged();
            }
        }

        private string _Price;

        public string Price
        {
            get
            {
                return _Price;
            }
            set
            {
                _Price = value;
                this.OnPropertyChanged();
            }
        }
        

        //#region 表格字段
        //public const string STATUS_FIELD = "Status";
        //public const string ORDERID_FIELD = "OrderID";
        //public const string VENUES_FIELD = "Venues";
        //public const string PRICE_FIELD = "Price";
        //public const string NETPRICE_FIELD = "NetPrice";
        //public const string QTY_FIELD = "Qty";
        //public const string EDIT_NETPRICE_FIELD = "Edit_NetPrice";
        //public const string DISPLAYBTN_FIELD = "DisplayBtn";
        //public const string CLORDID_FIELD = "ClOrdID";
        //#endregion

        private DataView _CrossDv;
        public DataView CrossDv
        {
            get
            {
                lock(CrossOrdersProcesser.Processer.DetailDvLockObj)
                {
                    if (this._CrossDv == null)
                    {
                        _CrossDv = new DataView(CrossOrdersProcesser.Processer.CrossDt);
                        _CrossDv.Sort = $"{CrossOrdersProcesser.BLOCK_NUM} desc,{CrossOrdersProcesser.TRANSACTTIME_FIELD} desc";
                        this.OnPropertyChanged();
                    }
                }
                return _CrossDv;
            }
        }

        public CrossQtyPopupViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(CrossDv, CrossOrdersProcesser.Processer.DetailDvLockObj);
        }

        ~CrossQtyPopupViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            // 释放我的订单数据
            lock (CrossOrdersProcesser.Processer.DetailDvLockObj)
            {
                if (_CrossDv != null)
                {
                    _CrossDv.Dispose();
                    _CrossDv = null;
                }
            }

            base.Dispose();
        }

        /// <summary>
        /// 构建表格数据
        /// </summary>
        public void GenCrossDv()
        {
            this.CrossDv.RowFilter = $"{CrossOrdersProcesser.STRATEGYORDERID_FIELD} = '{this.STCCode}' and {CrossOrdersProcesser.SIDE} = '{this.Side}'";           
        }

        public void getEmsOrder()
        {
            this.CrossDv.Table.Rows.Clear();
            RequestEmsOrders(this.SecurtityID,this.STCCode,this.Side);
        }


        public decimal CalBond(string calType, string settleType, string val)
        {
            //ThreadPool.QueueUserWorkItem(o =>
            //{
                try
                {
                    CalReq requst = new CalReq();
                    requst.SecurityID = this.SecurtityID;
                    requst.CalType = calType;//"4";
                    requst.SettlType = settleType;// "1";
                    requst.Value = val;// customPrice;

                    CalResult result = CalHandler.Instance.CalBond(requst);
                    if (requst != null && result.Data != null)
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        if (calType.Equals("4"))
                        {
                            //NetPrice = result.Data.CleanPrice.ToString("N4");
                            return result.Data.CleanPrice;
                        }
                        else if (calType.Equals("2"))
                        {
                            //CustomPrice = (result.Data.Yield * 100).ToString("N4");
                            return result.Data.Yield;
                        }

                        //isSendReq = false;
                        //}));
                    }
                    //else
                        //isSendReq = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    //isSendReq = false;
                }
            // });
            return decimal.Zero;
        }

        private void RequestEmsOrders(string securityId, string strageorderid,string side)
        {

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    request.securityid = securityId;
                    request.side = side;
                    if (!string.IsNullOrEmpty(strageorderid))
                        request.strategyorderid = strageorderid;
                    //request.userName = LocalEnv.UserInfo.UserName;
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    request.orderStatus = "N,0,1,S,6";
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = MarketExecHandler.Instance.QueryOrderForEMS(request);

                        if (reply == null || reply.datas == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            foreach (Common.Model.Fix.Message.ExecutionReport obj in reply.datas)
                            {
                                obj.Vs = obj.Vs == null || obj.Vs == "Y" || obj.Vs == "" ? "Y" : "N";

                                Instance_OnEmsOrder(obj);
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
                    LogHelper.Error("RequestEmsOrders", e);
                }
            });
        }
        private void Instance_OnEmsOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            lock (CrossOrdersProcesser.Processer.DetailDvLockObj)
            {
                string primaryKey = $"{obj.VenueTypeGW}#{obj.OrderID}";
                DataRow findDr = this.CrossDv.Table.Rows.Find(primaryKey);
                if (!(obj.OrdStatus == "N" || obj.OrdStatus == "0" || obj.OrdStatus == "1")
                    && !((obj.OrdStatus == "4" || obj.OrdStatus == "8" || obj.OrdStatus == "C") && obj.TimeInForce == "0" )
                    && findDr == null)
                {
                    return;
                }

                if (findDr != null)
                {
                    if (!(obj.OrdStatus == "N" || obj.OrdStatus == "0" || obj.OrdStatus == "1")
                        && !((obj.OrdStatus == "4" || obj.OrdStatus == "8" || obj.OrdStatus == "C") && obj.TimeInForce == "0" ))
                    {
                        string ordStatus = CommonUtils.ConvertToString(findDr[CrossOrdersProcesser.STATUS_FIELD]);
                        string key = CommonUtils.ConvertToString(findDr[CrossOrdersProcesser.KEY_FIELD]);
                        string ordID = CommonUtils.ConvertToString(findDr[CrossOrdersProcesser.ORDERID_FIELD]);

                        this.CrossDv.Table.Rows.Remove(findDr);

                        this.CrossDv.Table.AcceptChanges();
                        return;
                    }
                }

                string oldKey = string.Empty;
                string oldOrdID = string.Empty;
                string oldOrdStatus = string.Empty;
                if (findDr == null)
                {
                    // 添加记录
                    findDr = this.CrossDv.Table.NewRow();
                    findDr[CrossOrdersProcesser.ORDERID_FIELD] = obj.OrderID.EndsWith(CommonConstants.OrderEnd) ? obj.OrderID.Replace(CommonConstants.OrderEnd, "") : obj.OrderID; //obj.OrderID;
                    findDr[CrossOrdersProcesser.STRATEGYORDERID_FIELD] = obj.StrategyOrderId;
                    findDr[CrossOrdersProcesser.KEY_FIELD] = primaryKey;

                    this.CrossDv.Table.Rows.Add(findDr);
                }
                oldKey = CommonUtils.ConvertToString(findDr[CrossOrdersProcesser.KEY_FIELD]);
                oldOrdID = CommonUtils.ConvertToString(findDr[CrossOrdersProcesser.ORDERID_FIELD]);
                oldOrdStatus = CommonUtils.ConvertToString(findDr[CrossOrdersProcesser.STATUS_FIELD]);

                findDr[CrossOrdersProcesser.VENUES_FIELD] = obj.Venues;
                findDr[CrossOrdersProcesser.BLOCK_NUM] = $"{obj.BlockNum}:{obj.OrdStatus}:{obj.ExecType}:{obj.TimeInForce}";
                //findDr[TIMEINFORCE_FIELD] = obj.TimeInForce;

                findDr[CrossOrdersProcesser.STATUS_FIELD] = ComboSourceUtil.Instance.FindEnum(obj.OrdStatus, "ordstatus");
                if ((obj.OrdStatus == "0" || obj.OrdStatus == "1") && obj.ExecType == "6")
                {
                    findDr[CrossOrdersProcesser.STATUS_FIELD] = "撤销中";
                }

                findDr[CrossOrdersProcesser.DISPLAY_PRICE_FIELD] = CommonUtils.FormatPrice(obj.Price);// 用于显示的收益率
                findDr[CrossOrdersProcesser.PRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);// 收益率
                findDr[CrossOrdersProcesser.NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice);  // 净价
                findDr[CrossOrdersProcesser.EDIT_NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.NetPrice); // 净价（编辑）
                findDr[CrossOrdersProcesser.QTY_FIELD] = CommonUtils.ConvertToLong(obj.OrderQty); // 数量
                findDr[CrossOrdersProcesser.CLORDID_FIELD] = obj.ClOrdID;
                findDr[CrossOrdersProcesser.SIDE] = obj.Side;
                findDr[CrossOrdersProcesser.MARKETINDICATOR_FIELD] = obj.MarketIndicator;
                findDr[CrossOrdersProcesser.BOOKID_FIELD] = obj.BookID;
                if (Consts.MARKETINDICATOR_FUTURE.Equals(obj.MarketIndicator))
                {
                    //国债期货 成交价(%) 不显示
                    findDr[CrossOrdersProcesser.DISPLAY_PRICE_FIELD] = DBNull.Value;
                    //国债期货 净价显示成交价
                    findDr[CrossOrdersProcesser.NETPRICE_FIELD] = CommonUtils.ConvertToDecimal(obj.Price);
                }

                if (obj.Venues == "ODM" && (obj.OrdStatus == "0" || obj.OrdStatus == "1"))
                {
                    findDr[CrossOrdersProcesser.DISPLAYBTN_FIELD] = Visibility.Visible;
                }
                else
                {
                    findDr[CrossOrdersProcesser.DISPLAYBTN_FIELD] = Visibility.Collapsed;
                }
                this.CrossDv.Table.AcceptChanges();
            }
        }
    }
}
