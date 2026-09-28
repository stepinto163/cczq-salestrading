using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Handler
{
    public class BondChangeHandler
    {
        private static BondChangeHandler _bondChangeHandler = new BondChangeHandler();
        
        public static BondChangeHandler Instance { get { return _bondChangeHandler; } }

        private BondChangeHandler()
        { }

        public event Action<BondUpdateType, string, string, string, string, bool> OnSelectBondChange;
        public event Action<OperationType, string, object> OnSelectSettleChange;
        public event Action<OperationType, string, string, List<string>> OnOneSettleChange;
        public event Action<OperationType, string> OnSortChange;
        public event Action<OperationType, string> OnOneBrokerChange;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionFrom">1:改变结算方式， 2：改变datagrid选中行</param>
        /// <param name="security"></param>
        /// <param name="settle"></param>
        /// <param name="needRefresh"></param>
        public void  OnBondChange(BondUpdateType actionFrom, string groupId, string id, string security, string settle, bool needRefresh)
        {
            this.OnSelectBondChange?.Invoke(actionFrom, groupId, id, security, settle, needRefresh);
        }

        public void OnMainSettleChange(OperationType operationType, string groupId, object obj)
        {
            OnSelectSettleChange?.Invoke(operationType, groupId, obj);
        }

        public void OnOneBrokerChanged(OperationType operationType, string broker)
        {
            OnOneBrokerChange?.Invoke(operationType, broker);
        }

        public void OnOneSettleChanged(OperationType operationType, string groupId, string id, List<string> settle)
        {
            OnOneSettleChange?.Invoke(operationType, groupId, id, settle);
        }

        public void OnSortChanged(OperationType operationType, string groupId)
        {
            OnSortChange?.Invoke(operationType, groupId);
        }
    }

    public enum OperationType
    {
        None, Main, Card, Table, CardToMain, CardToTable, TableToCard, RemerberToCard, TableChangeFocusRow, TableChangeFocusRowSettle//改变行结算方式
    }

    public enum BondUpdateType
    {
        //1,2,3
        ChangeRowSettle, ChangeFocusRow, ChangeRowSettleSignal
    }
}
