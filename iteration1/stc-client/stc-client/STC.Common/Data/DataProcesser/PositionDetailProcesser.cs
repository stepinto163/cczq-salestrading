using com.gateway.log;
using STC.Common.Model.Position;
using STC.Common.Utils;
using System;
using System.Data;
using System.Linq;
using System.Threading;

namespace STC.Common.Data.DataProcesser
{
    public class PositionDetailProcesser
    {

        /// <summary>
        /// MyOrderProcesser实例
        /// </summary>
        private static PositionDetailProcesser proc = new PositionDetailProcesser();

        public static PositionDetailProcesser Instance
        {
            get
            {
                return proc;
            }
        }

        /// <summary>
        /// 我的订单资源锁
        /// </summary>
        private object lockObj_MyOrders = new object();
        public object LockObj_MyOrders
        {
            get
            {
                return lockObj_MyOrders;
            }
        }

        private DataTable _MyOrdersDT;
        public DataTable MyOrdersDT
        {
            get
            {
                lock (lockObj_MyOrders)
                {
                    if (_MyOrdersDT == null)
                    {
                        _MyOrdersDT = new DataTable();
                        _MyOrdersDT.Columns.Add("bookid", typeof(string));
                        _MyOrdersDT.Columns.Add("bookname", typeof(string));
                        _MyOrdersDT.Columns.Add("securityid", typeof(string));
                        _MyOrdersDT.Columns.Add("symbol", typeof(string));
                        _MyOrdersDT.Columns.Add("symboltype", typeof(string));
                        _MyOrdersDT.Columns.Add("marketindicator", typeof(string));
                        _MyOrdersDT.Columns.Add("netprice", typeof(decimal));
                        _MyOrdersDT.Columns.Add("netpricesource", typeof(string));

                        _MyOrdersDT.Columns.Add("longposition", typeof(decimal));
                        _MyOrdersDT.Columns.Add("longpriceTotal", typeof(decimal));
                        _MyOrdersDT.Columns.Add("longavgprice", typeof(decimal));
                        _MyOrdersDT.Columns.Add("shortposition", typeof(decimal));
                        _MyOrdersDT.Columns.Add("shortpricetotal", typeof(decimal));
                        _MyOrdersDT.Columns.Add("shortavgprice", typeof(decimal));

                        _MyOrdersDT.Columns.Add("netposition", typeof(decimal));
                        _MyOrdersDT.Columns.Add("totalprofitandloss", typeof(decimal));

                        _MyOrdersDT.Columns.Add("closingprofitandloss", typeof(decimal));
                        _MyOrdersDT.Columns.Add("positionprofitandloss", typeof(decimal));
                        _MyOrdersDT.Columns.Add("createtime", typeof(string));
                        _MyOrdersDT.Columns.Add("updatetime", typeof(string));
                    }
                    return this._MyOrdersDT;
                }
            }
        }

        /// <summary>
        /// 处理我的订单消息的线程队列
        /// </summary>
        private AsyncThreadQueue<PositionDetailModel> _ProcessMyOrdersThreadQueue;

        /// <summary>
        /// 构造函数
        /// </summary>
        private PositionDetailProcesser()
        {
            // BaseHandler.Instance.Init(Instance_OnClear);

            if (_ProcessMyOrdersThreadQueue == null)
            {
                _ProcessMyOrdersThreadQueue = new AsyncThreadQueue<PositionDetailModel>(UpdateMarketExecutions);
            }
        }

        private object lockObj = new object();
        /// <summary>
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMarketExecutions(PositionDetailModel obj)
        {
            lock (lockObj)
            {
                //"姓名='张' and 编号=1"
                //DataRow findDr = this.MyOrdersDT.Rows.Find(obj.bookid);
                DataRow[] findDr = MyOrdersDT.Select($"bookid='{obj.bookid}' and securityid='{obj.securityid}'");
                if (findDr == null)
                {
                    DataRow myOrder = MyOrdersDT.NewRow();
                    myOrder["bookid"] = obj.bookid;
                    myOrder["bookname"] = obj.bookname;
                    myOrder["securityid"] = obj.securityid;
                    myOrder["symbol"] = obj.symbol;
                    myOrder["symboltype"] = obj.symboltype;
                    myOrder["marketindicator"] = obj.marketindicator;
                    myOrder["netprice"] = CommonUtils.ConvertToDecimal(obj.netprice.ToString());
                    myOrder["netpricesource"] = obj.netpricesource;

                    myOrder["longposition"] = CommonUtils.ConvertToDecimal(obj.longposition.ToString());
                    myOrder["longpriceTotal"] = CommonUtils.ConvertToDecimal(obj.longpriceTotal.ToString());
                    myOrder["longavgprice"] = CommonUtils.ConvertToDecimal(obj.longavgprice.ToString());

                    myOrder["shortposition"] = CommonUtils.ConvertToDecimal(obj.shortposition.ToString());
                    myOrder["shortpricetotal"] = CommonUtils.ConvertToDecimal(obj.shortpricetotal.ToString());
                    myOrder["shortavgprice"] = CommonUtils.ConvertToDecimal(obj.shortavgprice.ToString());


                    myOrder["netposition"] = CommonUtils.ConvertToDecimal(obj.netposition.ToString());
                    myOrder["totalprofitandloss"] = CommonUtils.ConvertToDecimal(obj.totalprofitandloss.ToString());
                    myOrder["closingprofitandloss"] = CommonUtils.ConvertToDecimal(obj.closingprofitandloss.ToString());
                    myOrder["positionprofitandloss"] = CommonUtils.ConvertToDecimal(obj.positionprofitandloss.ToString());
                    myOrder["createtime"] = obj.createtime;
                    myOrder["updatetime"] = obj.updatetime;

                    this.MyOrdersDT.Rows.Add(myOrder);
                }
                else
                {

                }
            }
        }


        #region Dispose
        /// <summary>
        /// 析构函数
        /// </summary>
        ~PositionDetailProcesser()
        {
            Dispose();
        }

        public void Dispose()
        {
            //if (_updateMyOrdersThread != null)
            //{
            //    _updateMyOrdersThread.Abort();
            //    _updateMyOrdersThread = null;
            //}

            //this.Clear();

            //lock (lockObj_dummyDT)
            //{
            //    if (_DummyDT != null)
            //    {
            //        _DummyDT.Clear();
            //        _DummyDT.Dispose();
            //        _DummyDT = null;
            //    }
            //}

            //lock (this.lockObj_MyOrders)
            //{
            //    if (_MyOrdersDT != null)
            //    {
            //        _MyOrdersDT.Clear();
            //        _MyOrdersDT.Dispose();
            //        _MyOrdersDT = null;
            //    }
            //}
        }
        #endregion
    }
}
