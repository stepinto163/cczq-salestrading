using BDB.Common.Api;
using BDB.Common.Handler.Base;
using BDB.Common.Handler.MarketExec;
using BDB.Common.Handler.OHLC;
using BDB.Common.Model.Base;
using BDB.Common.Model.Fix.Message;
using BDB.Common.Model.TBook;
using BDB.Common.Utils;
using BDB.Common.ViewModel;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade;
using com.gateway.log;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BDB.Price.Plugin.Chart.ViewModel
{
    public class ExecChartViewModel : ViewModelBase
    {
        private object _lockObj = new object();

        private string _searchText = string.Empty;

        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                lock (_lockObj)
                {
                    string oldSecurity = _searchText;

                    //if (oldSecurity == value)
                    //{
                    //    return;
                    //}

                    _searchText = value;
                    this.OnPropertyChanged();

                    // 清缓存数据
                    //MarketOrderModel.MarketExecutions.Clear();

                    // 高开低收
                    //if (!string.IsNullOrEmpty(oldSecurity))
                    //{
                    //    OhlcHandler.Instance.UnSubscribeOhlc(oldSecurity, onOHLC);
                    //    PriceHandler.Instance.UnregisterCTPrice(this.SearchText, Instance_OnMDPrice);
                    //}

                    if (!string.IsNullOrEmpty(value))
                    {
                        LoadCacheData();

                    }
                }
            }
        }

        public ExecChartViewModel()
        {
            InitUI();
        }

        ~ExecChartViewModel()
        {
            Dispose();
        }

        new public void Dispose()
        {
            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;

            base.Dispose();
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("MarketOrderViewModel Clear, security:{0}", this.SearchText));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                lock (this._lockObj)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        //MarketOrderModel.MarketExecutions.Clear();

                        //Thread.Sleep(2000);

                        LoadCacheData();
                    }));
                }
            }
        }


        public void LoadCacheData()
        {
            //MarketOrderModel.MarketExecutions.Clear();

            bool isLoad = MarketExecCache.Instance.GetLoadStatus(SearchText);

            if (isLoad)
            {
                List<ExecutionReport> execList = MarketExecCache.Instance.GetMarketExecByCondition(SearchText, string.Empty);
                LogHelper.Warn("LoadCacheData SecurityId:" + SearchText + "," + isLoad + "," + (execList != null ? execList.Count : 0));
                //Instance_OnMarketExeList(execList);
                if (execList != null && execList.Count > 0)
                {
                    foreach (ExecutionReport order in execList)
                    {
                        this.Instance_OnMarketExecutions(order);

                    }
                }
                else
                {
                    RequestMarketExecutions();
                }
            }
            else
            {
                RequestMarketExecutions();
            }
        }

        public void Init(FrameworkElement ownUI)
        {
            BaseHandler.Instance.Init(Instance_OnClear);

            //if (this.ownUI == null)
            //{
            //    this.ownUI = ownUI;
            //}

            try
            {
                //if (_showModel == TabShowModel.Table)
                //    BondChangeHandler.Instance.OnSelectBondChange += Instance_OnSelectBondChange;

                //MarketOrderModel.Init();

                #region 收MarketExec消息0
                ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                #endregion

                //LoadCacheData();

                //#region 订阅OHLC
                //bool isFuture = CommonUtils.IsFutureBond(SearchText);
                //if (isFuture)
                //{
                //    PriceHandler.Instance.RegisterCTPrice(this.SearchText, Instance_OnMDPrice);
                //}
                //else
                //{
                //    OhlcHandler.Instance.SubscribeOhlc(this.SearchText, onOHLC);
                //}
                //#endregion

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }




        #region 各个card自己加载自己的数据

        private bool isSend = false;
        public void RequestMarketExecutions()
        {
            bool isFuture = CommonUtils.IsFutureBond(SearchText);


            if (isSend || string.IsNullOrEmpty(SearchText))
            {
                return;
            }

            isSend = true;

            LogHelper.Warn("RequestMarketExecutions Load SecurityId:" + SearchText);

            Task.Run(() =>
            {
                try
                {
                    string yyyyMMdd = "yyyy-MM-dd";
                    string commonFormat = "yyyy-MM-dd HH:mm:ss";
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString(yyyyMMdd);

                    DateTime openDate = DateTime.ParseExact(DateTime.Now.ToString(yyyyMMdd) + " 09:30:00", commonFormat, CultureInfo.InvariantCulture);

                    //if (isFuture && DateTime.Now.CompareTo(openDate) < 0)
                    //{
                    //    today = DateTimeEx.ServerTime.AddDays(-1).ToString(yyyyMMdd);

                    //    request.pageSize = 5;
                    //}
                    //else
                    {
                        if (isFuture)
                        {
                            request.pageSize = MarketExecCache.Instance.Limit;
                        }
                        else
                        {
                            request.pageSize = 2000;
                        }
                    }

                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    request.venuetypegw = isFuture ? ApiConsts.SDN : ApiConsts.CDH;
                    //request.venuetypegw = "CDH";
                    request.securityid = SearchText;

                    List<string> permissionBroker = PermisionHandler.Instance.GetBondPermission();

                    foreach (string broker in permissionBroker)
                    {
                        request.venues += string.Format("{0},", broker);
                    }

                    if (!string.IsNullOrEmpty(request.venues))
                    {
                        request.venues += "CTP,";
                        request.venues = request.venues.Substring(0, request.venues.Length - 1);
                    }
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    request.orderStatus = "1,2";
                    int index = 0;


                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取成交
                        LogHelper.Warn(string.Format("MarketOrderViewModel QueryExecOrderForEMS request:{0}", JsonConvert.SerializeObject(request)));
                        var reply = MarketExecHandler.Instance.QueryExecOrderForEMS(request);
                        LogHelper.Warn(string.Format("MarketOrderViewModel QueryExecOrderForEMS replay:{0}", reply == null ? string.Empty : reply.totalSize.ToString()));
                        if (reply == null || reply.totalSize == 0 || (reply != null && reply.datas != null))
                        {
                            flag = false;

                            MarketExecCache.Instance.AddOrUpdateStatus(SearchText);

                            PublishLastOrder(reply);

                            break;
                        }


                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestExecutions", e);
                }
                finally
                {
                    //MarketExecHandler.Instance.SetLoadDataStatus(true);

                    isSend = false;
                }
            });

        }

        #endregion

        public void PublishLastOrder(TBookOrder bookOrder)
        {
            if (bookOrder != null && bookOrder.datas != null)
            {
                LogHelper.Warn("PublishLastOrder Load SecurityId:" + SearchText + "," + CommonHandler.Instance.IsLoaded());
                if (CommonHandler.Instance.IsLoaded() && bookOrder.datas.Count > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        LoadCacheData();
                    }));
                }

                HashSet<string> sendExecIdHs = new HashSet<string>();
                foreach (ExecutionReport er in bookOrder.datas)
                {

                    if (sendExecIdHs.Count < 5)
                    {
                        ExecutionsHandler.Instance.Instance_OnExecOrder(er);
                    }
                    else
                    {
                        break;
                    }
                    if (er.OrdStatus == "2")
                    {
                        sendExecIdHs.Add(er.ExecID);
                    }

                }
            }
        }



    }
}
