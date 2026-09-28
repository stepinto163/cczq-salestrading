using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.gateway.log;
using STC.Common.Handler.Price;
using STC.Common.Model.Fix.Message;

namespace STC.dps.plugin.Handler
{
    public class PriceHandler : IPriceHandler
    {
        private PriceHandler()
        {
            this.Init();
        }
        private static IPriceHandler priceHandler = new PriceHandler();


        public static IPriceHandler Instance { get { return priceHandler; } }

        /// <summary>
        /// 更新价格的线程
        /// </summary>
        private Thread _updatePriceThread;

        /// <summary>
        /// 深度行情是否要更新情况
        /// </summary>
        private ConcurrentDictionary<string, bool> _UpdateCTPricesDic = new ConcurrentDictionary<string, bool>();
        /// <summary>
        /// 深度行情回调函数
        /// </summary>
        private ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>> _UpdateCTCallbackDic = new ConcurrentDictionary<string, Action<MarketDataSnapshotFullRefresh>>();

        private void Init()
        {
            // 定时检查 ctprice是否需要更新。
            if (_updatePriceThread == null)
            {
                _updatePriceThread = new Thread(UpdatePrice);
                _updatePriceThread.IsBackground = true;
                _updatePriceThread.Start();
            }
        }

        /// <summary>
        /// 更新行情
        /// </summary>
        /// <param name="obj"></param>
        private void UpdatePrice()
        {
            while (true)
            {
                // 更新深度行情
                foreach (var ctPair in _UpdateCTPricesDic)
                {
                    string primaryKey = ctPair.Key;
                    if (ctPair.Value == true)
                    {
                        lock (string.Intern(primaryKey))
                        {
                            MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                            if (_UpdateCTCallbackDic.ContainsKey(primaryKey) && ctPrice != null)
                            {
                                _UpdateCTCallbackDic[primaryKey]?.Invoke(ctPrice);
                                _UpdateCTPricesDic.AddOrUpdate(primaryKey, false, (key, value) => false);

                            }
                        }
                    }
                    //lock (string.Intern(primaryKey))
                    //{
                    //    _UpdateCTPricesDic.AddOrUpdate(primaryKey, false, (key, value) => false);
                    //}
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// 订阅指定债券CT行情
        /// </summary>
        /// <param name="securityID"></param>
        private void subscribe(string securityID)
        {
            _UpdateCTPricesDic.AddOrUpdate(securityID, true, (key, value) => true);
            MDHandler.Instance.SubscribeCT(securityID, Instance_OnCTPrice);
        }


        private void Instance_OnCTPrice(MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh)
        {

        }

        public void Subscribe(string securityId, string settle)
        {
            lock (string.Intern(securityId))
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        this.subscribe(securityId);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("RegisterCTPrice", e);
                    }
                });
            }
        }
    }
}
