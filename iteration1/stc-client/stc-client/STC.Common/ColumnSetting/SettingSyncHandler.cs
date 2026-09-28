using STC.Common.Utils;
using STC.Common.View.Component;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ColumnSetting
{
    public class SettingSyncHandler
    {
        private SettingSyncHandler()
        {
        }

        private static SettingSyncHandler _instance = new SettingSyncHandler();

        public static SettingSyncHandler Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        //private readonly Object m_notifySettingSyncLock = new Object();
        //private Action<string> _notifySettingSyncAction;

        //public event Action<string> NotifySettingSyncAction
        //{
        //    add
        //    {
        //        lock (m_notifySettingSyncLock)
        //        {
        //            if (_notifySettingSyncAction == null || !_notifySettingSyncAction.GetInvocationList().Contains(value))
        //                _notifySettingSyncAction += value;
        //        }
        //    }
        //    remove
        //    {
        //        lock (m_notifySettingSyncLock)
        //        {
        //            _notifySettingSyncAction -= value;
        //        }
        //    }
        //}
        private ConcurrentDictionary<string, List<ISettingColumnsBase>> _needSettingSyncDgDic = new ConcurrentDictionary<string, List<ISettingColumnsBase>>();

        public void AddSyncDg(ISettingColumnsBase exdg)
        {
            if (!_needSettingSyncDgDic.ContainsKey(exdg.GetName()))
                _needSettingSyncDgDic[exdg.GetName()] = new List<ISettingColumnsBase>();
            if (!_needSettingSyncDgDic[exdg.GetName()].Contains(exdg))
                _needSettingSyncDgDic[exdg.GetName()].Add(exdg);
        }
        
        public void RemoveSyncDg(ISettingColumnsBase exdg)
        {
            if (_needSettingSyncDgDic.ContainsKey(exdg.GetName()))
            {
                _needSettingSyncDgDic[exdg.GetName()].Remove(exdg);
                if (_needSettingSyncDgDic[exdg.GetName()].Count == 0)
                {
                    List<ISettingColumnsBase> removeList;
                    _needSettingSyncDgDic.TryRemove(exdg.GetName(), out removeList);
                }
            }
        }

        public void RefreshColumns(string currentDgName, ISettingColumnsBase currentDg = null)
        {
            if (_needSettingSyncDgDic.ContainsKey(currentDgName))
            {
                List<ISettingColumnsBase> exDataGridList = _needSettingSyncDgDic[currentDgName];
                string columnsInfoStr = IniFileHelper.ReadValue($"{currentDgName}-{Utils.LocalEnv.UserInfo.UserName}", ColumnSettingView.IniKey, 2048);
                foreach (ISettingColumnsBase dg in exDataGridList)
                {
                    if (currentDg == null || (currentDg != null && dg != currentDg))
                    {
                        if (string.IsNullOrEmpty(columnsInfoStr))
                        {
                            columnsInfoStr = dg.GetDefaultColumnsInfoStr();
                        }
                        dg.InitColumnsInfo(columnsInfoStr);
                    }
                }
            }
        }

        public void LoadDefaultColumns(string currentDgName)
        {
            if (_needSettingSyncDgDic.ContainsKey(currentDgName))
            {
                List<ISettingColumnsBase> exDataGridList = _needSettingSyncDgDic[currentDgName];
                foreach (ISettingColumnsBase dg in exDataGridList)
                {
                    dg.InitColumnsInfo(dg.GetDefaultColumnsInfoStr());
                }
            }
        }
    }
}
