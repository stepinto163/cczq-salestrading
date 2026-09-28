using com.gateway.log;
using BDB.Trade.Plugin.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Handler
{
    public class CommonHandler
    {
        private static CommonHandler commonHandler;
        public static CommonHandler Instance
        {
            get
            {
                if (commonHandler == null)
                {
                    commonHandler = new CommonHandler();
                }
                return commonHandler;
            }
        }

        private CommonHandler()
        {
        }

        public event Action<string> OnCardLoadEnd;

        private ConcurrentDictionary<string, bool> loadDic = new ConcurrentDictionary<string, bool>();
        //private int loadNum = 0;
        private ConcurrentDictionary<string, int> loadNumDic = new ConcurrentDictionary<string, int>();

        public void AddLoadNum(string key)
        {
            int count = LayoutSetttingCache.Instance.layoutSettingPo.Col * LayoutSetttingCache.Instance.layoutSettingPo.Row * 2;//报价和市场成交
            if (!loadNumDic.ContainsKey(key))
            {
                loadNumDic[key] = 0;
            }
            if (loadNumDic[key] < count)
            {
                loadNumDic[key]++;
                //Interlocked.Increment(ref loadNum);

                if (loadNumDic[key] == count)
                {
                    LogHelper.Warn("AddLoadNum loadNum:" + loadNumDic[key] + ", count:" + count);
                    OnCardLoadEnd?.Invoke(key);
                }
            }
        }

        /// <summary>
        /// 确认是否已加载好
        /// </summary>
        /// <returns></returns>
        public bool IsLoaded(string key)
        {
            int count = LayoutSetttingCache.Instance.layoutSettingPo.Col * LayoutSetttingCache.Instance.layoutSettingPo.Row * 2;//报价和市场成交
            //LogHelper.Warn("IsLoaded loadNum:" + loadNum + ", count:" + count);
             if (!loadNumDic.ContainsKey(key))
            {
                loadNumDic[key] = 0;
            }
            return loadNumDic[key] >= count;
        }

        public void ResetNum()
        {
            foreach(string key in loadNumDic.Keys)
            {
                loadNumDic[key] = 0;
            }
            //loadNum = 0;
        }

        public void AddOrUpdateStatus(string securityId, string key)
        {
            loadDic.AddOrUpdate(securityId, true, (key1, value) => true);

            int count = LayoutSetttingCache.Instance.layoutSettingPo.Col * LayoutSetttingCache.Instance.layoutSettingPo.Row;

            int total = 0;
            foreach (KeyValuePair<string, bool> kv in loadDic)
            {
                if (kv.Value)
                {
                    total++;
                }
            }

            if (total == count)
            {
                OnCardLoadEnd?.Invoke(key);
            }
        }

        public void ClearCache()
        {
            loadDic.Clear();
        }

    }
}
