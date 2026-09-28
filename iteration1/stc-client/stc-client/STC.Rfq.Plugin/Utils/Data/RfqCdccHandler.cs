using com.gateway.log;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Rfq.Plugin.Utils.Data
{
    public class RfqCdccHandler
    {
        /// <summary>
        /// RfqCdccHandler
        /// </summary>
        private static RfqCdccHandler server = new RfqCdccHandler();

        public static RfqCdccHandler Instance { get { return server; } }

        public ConcurrentDictionary<string, CCDCValuaModel> CcdcDic = new ConcurrentDictionary<string, CCDCValuaModel>();

        private object lockobj = new object();

        public ConcurrentDictionary<string, Action<CCDCValuaModel>> DictCCDCCallBack = new ConcurrentDictionary<string, Action<CCDCValuaModel>>();
        private event Action<CCDCValuaModel> OnCDCC;
        private List<string> DictKeyList = new List<string>();
        public RfqCdccHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);

        }

        public void Clear()
        {
            lock (lockobj)
            {
                //DictCCDCCallBack.Clear();

                //ThreadPool.QueueUserWorkItem(o =>
                //{
                //    try
                //    {
                //        foreach (string securityId in CcdcDic.Keys)
                //        {
                //            PriceHandler.Instance.UnregisterCTPrice(securityId, Instance_OnCTPrice);
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        LogHelper.Error("RegisterCTPrice", e);
                //    }

                //    this.CcdcDic.Clear();
                //});
            }
        }

        public void SubscribeCCDC(string securityId, string groupid, Action<CCDCValuaModel> callBack)
        {
            if (callBack != null)
            {
                string primaryKey = $"{securityId}.{groupid}.{callBack.Method.DeclaringType.FullName.ToString()}.{callBack.Method.Name.ToString()}";
                if (callBack != null)
                {
                    if (!DictKeyList.Contains($"{securityId}.{groupid}"))
                    {
                        DictKeyList.Add($"{securityId}.{groupid}");
                        if (DictCCDCCallBack.ContainsKey(securityId) == false || DictCCDCCallBack[securityId] == null)
                        {
                            Action<CCDCValuaModel> newAction = null;
                            DictCCDCCallBack.AddOrUpdate(securityId, newAction, (key, value) => newAction);
                            DictCCDCCallBack[securityId] += callBack;
                        }
                        else
                        {
                            int index = Array.IndexOf(DictCCDCCallBack[securityId].GetInvocationList(), (Action<CCDCValuaModel>)callBack);
                            if (index < 0)
                            {
                                DictCCDCCallBack[securityId] += callBack;
                            }
                        }

                    }

                    if (CcdcDic.ContainsKey(securityId))
                    {
                        DictCCDCCallBack[securityId]?.Invoke(CcdcDic[securityId]);
                    }
                    else
                    {
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
                }
            }
        }


        /// <summary>
        /// 获取中债估值
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                if (obj != null)
                {
                    // 过滤
                    string secid = obj.SecurityID;
                    if (!string.IsNullOrEmpty(obj.CCDCValuation))
                    {
                        CCDCValuaModel cm = new CCDCValuaModel();
                        cm.CCDCValuation = obj.CCDCValuation;
                        cm.CCDCValuaDate = obj.CCDCValuaDate;
                        cm.CCDCValuationShow = obj.SecurityID;
                        cm.OrderCount = obj.Volume;

                        CcdcDic.AddOrUpdate(secid, cm, (key, value) => cm);

                        DictCCDCCallBack[secid]?.Invoke(CcdcDic[secid]);

                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            //注册
                            PriceHandler.Instance.UnregisterCTPrice(secid, Instance_OnCTPrice);
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
             
        }
    }
}
