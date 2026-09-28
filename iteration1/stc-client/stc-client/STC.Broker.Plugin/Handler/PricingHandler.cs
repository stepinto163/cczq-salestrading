using com.gateway.log;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Handler
{
    public class PricingHandler
    {
        private static PricingHandler instance = new PricingHandler();
        public static PricingHandler Instance
        {
            get { return instance; }
        }

        
        private volatile bool isInit = false;

        private const string BondKey = "2_Bond";

        private object lockobj = new object();

        private ConcurrentQueue<MarketDataSnapshotFullRefresh> GrMDQueue = new ConcurrentQueue<MarketDataSnapshotFullRefresh>();
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private Thread RefreshMDThread = null;
        private object _queueLock = new object();
        public bool StopRefreshER = false;

        public PricingHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);

            if (RefreshMDThread == null)
            {
                RefreshMDThread = new Thread(RefreshGrMD);
                RefreshMDThread.IsBackground = true;
                RefreshMDThread.Start();
            }
        }


        private void RefreshGrMD()
        {
            while (true)
            {
                try
                {
                    if (StopRefreshER)
                    {
                        break;
                    }

                    MarketDataSnapshotFullRefresh collect = null;

                    lock (_queueLock)
                    {
                        if (GrMDQueue.TryDequeue(out collect))
                        {
                            ProcGrMD(collect);                          

                            while (GrMDQueue.Count > 0)
                            {
                                collect = null;
                                GrMDQueue.TryDequeue(out collect);
                                ProcGrMD(collect);
                            }
                        }
                    }

                    if (GrMDQueue.Count == 0)
                        _autoResetEvent.WaitOne();
                }
                catch (ThreadAbortException ex)
                {
                    LogHelper.Debug("GrMDQueue Normal Abort:", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("GrMDQueue exception:", ex);
                }
            }
        }

        private void ProcGrMD(MarketDataSnapshotFullRefresh obj)
        {
            if (obj != null)
            {
                if (obj.NoMDEntries == null)
                {
                    return;
                }
                string securityId = obj.SecurityID;

                DictCTPrice.AddOrUpdate(securityId, obj, (key, value) => obj);

                if (DictCTPriceInfoCallBack.ContainsKey(securityId))
                {
                    DictCTPriceInfoCallBack[securityId]?.Invoke(obj);
                }
            }
        }

        public void Init()
        {
            isInit = true;
        }

        #region 接收信号
        /// <summary>
        /// 清零信号
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("PricingHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                isInit = false;
                DictCTPrice.Clear();
            }
        }

        #endregion


        #region 订阅债券行情信息
        public Dictionary<string, Action<MarketDataSnapshotFullRefresh>> DictCTPriceInfoCallBack = new Dictionary<string, Action<MarketDataSnapshotFullRefresh>>();

        private ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> dictCTPrice = new ConcurrentDictionary<string, MarketDataSnapshotFullRefresh>();
        public ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> DictCTPrice
        {
            get
            {
                return dictCTPrice;
            }
        }

        public void SubscribeCTPriceInfo(string securityId, Action<MarketDataSnapshotFullRefresh> callBack)
        {
            if (callBack != null)
            {
                string callbackkey = $"{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";

                lock (lockobj)
                {
                    if (!DictCTPriceInfoCallBack.ContainsKey(securityId))
                    {
                        //不包含
                        DictCTPriceInfoCallBack.Add(securityId, callBack);
                        //订阅
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                PriceHandler.Instance.RegisterCTPrice(securityId, Instance_OnCTPrice);
                            }
                            catch (Exception e)
                            {
                                LogHelper.Error("RegisterCTPrice", e);
                            }
                        });

                    }
                    else
                    {
                        Action<MarketDataSnapshotFullRefresh> data;
                        if (DictCTPriceInfoCallBack.TryGetValue(securityId, out data))
                        {
                            data += callBack;
                            DictCTPriceInfoCallBack[securityId] = data;
                        }
                    }

                    if (DictCTPrice.ContainsKey(securityId))
                    {
                        DictCTPriceInfoCallBack[securityId]?.Invoke(DictCTPrice[securityId]);
                    }
                }
            }
        }

        private void Instance_OnCTPrice(MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                lock (_queueLock)
                {
                    GrMDQueue.Enqueue(obj);
                    _autoResetEvent.Set();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }
        #endregion
    }
}
