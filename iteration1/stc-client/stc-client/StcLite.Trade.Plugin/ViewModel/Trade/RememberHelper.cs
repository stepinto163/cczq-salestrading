using BDB.Trade.Plugin.Cache;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class RememberHelper
    {
        private static RememberHelper proc = new RememberHelper();

        public static RememberHelper Instance
        {
            get
            {
                return proc;
            }
        }

        private RememberHelper()
        { }

        private readonly string Group = "Group";

        #region 自定义结算

        private readonly string Settle_Section = "Settle";
        

        private string GetSectionKey(string section)
        {
            return string.Format("{0}-{1}", section, LocalEnv.UserInfo.UserName);
        }

        private string GetSectionGroupKey(string group, string groupId)
        {
            return string.Format("{0}-{1}", group, groupId);
        }


        public string WriteCustomSettle(string groupId)
        {
            string sectionKey = GetSectionKey(Settle_Section);
            string sectionGroupKey = GetSectionGroupKey(Group, groupId);

            StringBuilder sb = new StringBuilder();
            ConcurrentDictionary<string, string> settleDic = SettleCache.Instance.GetSettleByGroupId(groupId);
            if (settleDic != null)
            {
                foreach (KeyValuePair<string, string> kv in settleDic)
                {
                    sb.Append(string.Format("{0}-{1},", kv.Key, kv.Value));
                }
            }

            string result = sb.ToString();
            result = result.Length > 0 ? result.Substring(0, result.Length - 1) : string.Empty;
            IniFileHelper.WriteValue(sectionKey, sectionGroupKey, result);


            return result;
        }

        public void ReadCustomSettle(string groupId)
        {
            string sectionKey = GetSectionKey(Settle_Section);
            string sectionGroupKey = GetSectionGroupKey(Group, groupId);

            string result = IniFileHelper.ReadValue(sectionKey, sectionGroupKey);

            List<string> sortList = result.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string str in sortList)
            {
                string[] arr = str.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                int d = 0;
                if (arr != null && arr.Length > 1)
                {
                    SettleCache.Instance.AddOrUpdate(groupId, arr[0], arr[1]);
                }
            }
        }
        #endregion

        #region 自定义排序

        private readonly string Sort_Section = "Sort";


        public string WriteCustomSort(string groupId)
        {
            string sectionKey = GetSectionKey(Sort_Section);
            string sectionGroupKey = GetSectionGroupKey(Group, groupId);

            StringBuilder sb = new StringBuilder();

            ConcurrentDictionary<string, int> sortDic = SortCache.Instance.GetSettleByGroupId(groupId);
            if (sortDic != null)
            {
                foreach (KeyValuePair<string, int> kv in sortDic)
                {
                    sb.Append(string.Format("{0}-{1},", kv.Key, kv.Value));
                }
            }
            string result = sb.ToString();
            result = result.Length > 0 ? result.Substring(0, result.Length - 1) : string.Empty;
            IniFileHelper.WriteValue(sectionKey, sectionGroupKey, result);


            return result;
        }

        public void ReadCustomSort(string groupId)
        {
            string sectionKey = GetSectionKey(Sort_Section);
            string sectionGroupKey = GetSectionGroupKey(Group, groupId);

            string result = IniFileHelper.ReadValue(sectionKey, sectionGroupKey);

            List<string> sortList = result.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string str in sortList)
            {
                string[] arr = str.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                int d = 0;
                if (arr != null && arr.Length > 1 && int.TryParse(arr[1], out d))
                {
                    SortCache.Instance.AddOrUpdate(groupId, arr[0], d);
                }
            }
        }

        #endregion
    }
}
