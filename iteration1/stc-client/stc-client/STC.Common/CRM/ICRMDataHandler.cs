using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Frame.Plugin.WindowsPlugin;

using System.Collections.Generic;
using System.Data;

namespace STC.Common.CRM
{
    public interface ICRMDataHandler
    {
        IForm Application { get; set; }

        List<BondsInfoPo> GetBondsInfoPo(string condition, int resultNum);

        List<DataGridDataPo> GetContactByContactName(string contactName, int resultNum);
        
        List<DataGridDataPo> GetContactByInstName(string instName, int resultNum);
        /**
        #region Bond

        //DataTable SearchBondBySql(string sql, int resultNum);

        DataTable SearchBond2(string condition);
        DataTable SearchBond3(string condition, int resultNum);
        #endregion

        #region Contact

        DataTable SearchContactBySql(string sql, int resultNum);

        DataTable SearchContact(string condition, int num);
        DataTable SearchContactByinstid(string condition, int resultNum);
        DataTable SearchContactByIn(string condition, int resultNum);
        DataTable SearchQuoteContact(string condition);
        DataTable SearchQuoteContactByinstid(string condition);
        #endregion

        #region Inst

        DataTable SearchInstBySql(string sql, int resultNum);

        DataTable SearchInst(string condition);
        DataTable SearchInst2(string condition, int resultNum);
        DataTable SearchInst3(string condition);
        DataTable SearchInst3(string condition, int resultNum);
        DataTable SearchInst4(string condition, int resultNum);
        DataTable SearchQuoteInst(string condition);
        DataTable SearchQuoteByIdbkey(string condition);
        DataTable SearchBillInst(string condition);
        DataTable SearchBillInstByKey(string condition);
        #endregion

        #region Broker
        
        DataTable SearchBrokerBySql(string sql, int resultNum);

        DataTable SearchBroker(string matchName, string filterName);
        DataTable SearchBroker2(string matchName, int resultNum);
        DataTable SearchBroker3(string matchName, int resultNum);
        DataTable SearchQuoteBroker(string matchName);
        DataTable SearchQuoteBrokerByAccount(string account);
        DataTable SearchBillBroker(string matchName, int resultNum);
        DataTable SearchBillBrokerByKey(string matchName, int resultNum);
        #endregion

    **/
    }
}
