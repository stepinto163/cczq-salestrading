using STC.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BDB.Trade.Plugin.Cache
{
    public class MainConditionCache : IMainConditionCache
    {
        private MainConditionCache() { }
        private static IMainConditionCache server = new MainConditionCache();


        public static IMainConditionCache Instance { get { return server; } }

        private Dictionary<string, object> _tabConditionDic = new Dictionary<string, object>();

        public bool IsContain(string groupId)
        {
            return !string.IsNullOrEmpty(groupId) && _tabConditionDic.ContainsKey(groupId);
        }

        public void AddOrUpdate(string groupId, object condition)
        {
            if (!string.IsNullOrEmpty(groupId))
            {
                if (_tabConditionDic.ContainsKey(groupId))
                    _tabConditionDic[groupId] = condition;
                else
                    _tabConditionDic.Add(groupId, condition);
            }
        }

        public object GetConditionById(string groupId)
        {
            object result = null;

            if (!string.IsNullOrEmpty(groupId))
            {
                if (!_tabConditionDic.ContainsKey(groupId))
                {
                    // 初始化全局筛选条件配置
                    Dictionary<string, IList> dic = new Dictionary<string, IList>();
                    List<ListBoxItem> settlelistBoxItems = new List<ListBoxItem>();
                    settlelistBoxItems.Add(new ListBoxItem() { Content = "全部"});
                    dic[CommonConstants.SETTLE] = settlelistBoxItems;

                    List<ListBoxItem> listBoxItems = new List<ListBoxItem>();
                    listBoxItems.Add(new ListBoxItem() { Content = "全部" });
                    dic[CommonConstants.BROKER] = listBoxItems;

                    IList list = new List<int>();
                    dic[CommonConstants.EXECTYPE] = list;

                    _tabConditionDic[groupId] = dic;
                }

                result = _tabConditionDic[groupId];
            }

            return result;
        }

        public void RemoveById(string groupId)
        {
            if (!string.IsNullOrEmpty(groupId) && _tabConditionDic.ContainsKey(groupId))
                _tabConditionDic.Remove(groupId);
        }
    }
}
