using com.gateway.log;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.OHLC;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.MDHist
{
    public class Kline1MCache
    {
        private object lockObj = new object();
        private ConcurrentDictionary<string, bool> loadDic = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, SortedList<string, OhlcMsg>> sortDic = new ConcurrentDictionary<string, SortedList<string, OhlcMsg>>();

        private Kline1MCache() 
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info("Kline1MCache Clear, security:{0}");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                OnClear();
            }
        }

        private static Kline1MCache server = new Kline1MCache();


        public static Kline1MCache Instance { get { return server; } }


        public void AddOrUpdateStatus(string securityId)
        {
            loadDic.AddOrUpdate(securityId, true, (key, value) => true);

        }

        public bool GetLoadStatus(string securityId)
        {
            if (loadDic.ContainsKey(securityId))
            {
                return loadDic[securityId];
            }

            return false;
        }


        public void OnClear()
        {
            //loadDic.Clear();
            //清除当日之前数据
            if (sortDic != null)
            {
                string todayStartTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 00:00:00";
                foreach (SortedList<string, OhlcMsg> list in sortDic.Values)
                {
                    var removeList = list.Where(item => item.Key.CompareTo(todayStartTime) < 0);
                    foreach(KeyValuePair<string, OhlcMsg> removeItem in removeList)
                    {
                        list.Remove(removeItem.Key);
                    }
                }
            }
        }

        public void AddOrUpdate(OhlcMsg order)
        {
            lock (lockObj)
            {
                if (!sortDic.ContainsKey(order.securityid))
                {
                    sortDic[order.securityid] = new SortedList<string, OhlcMsg>();
                }
                if (string.IsNullOrEmpty(order.closetime)) return;
                SortedList<string, OhlcMsg> sortList = sortDic[order.securityid];
                // 分钟线， 格式化分钟
                string closetime = $"{order.closetime.Substring(0, 17)}00";
                sortList[closetime] = order;
            }
        }

        public SortedList<string, OhlcMsg> GetKLine_1M(string SecurityID)
        {
            if (sortDic.ContainsKey(SecurityID))
            {
                return sortDic[SecurityID];
            }
            return null;
        }
    }
}
