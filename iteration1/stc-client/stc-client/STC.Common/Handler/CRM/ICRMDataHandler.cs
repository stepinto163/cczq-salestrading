using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using System.Collections.Generic;
using System.Data;

namespace STC.Common.Handler.CRM
{
    public interface ICRMDataHandler
    {
        IForm Application { get; set; }

        List<BondsInfoPo> GetBondsInfoPo(string condition, int resultNum);
        List<BondsInfoPo> GetBondsInfoPoNew(string condition, int resultNum);
        BondsInfoPo GetBondsInfoPoOne(string condition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        BondsInfoPo GetBondsInfoByCode(string goodsCode);

        /// <summary>
        /// 根据条件获得债券信息,返回条件依据
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <param name="flag"> 0：模糊匹配，1：代码，2：简称</param>
        /// <returns></returns>
        List<BondsInfoPo> GetBondsInfoPoByCon(string condition, int resultNum, out int flag);

        List<DataGridDataPo> GetContactByContactName(string contactName, int resultNum);

        List<DataGridDataPo> GetContactByInstName(string instName, int resultNum);

        #region Bond

        #endregion

        #region Contact

        /// <summary>
        /// 根据机构id和交易员名称获取Contact
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="contactName"></param>
        /// <returns></returns>
        DataTable GetContactByInstIdAndContactName(string instId, string contactName);

        /// <summary>
        /// 根据brokerId 加台子 查询对应机构。  BrokerId:台子ID
        /// </summary>
        /// <param name="brokerId"></param>
        /// <returns></returns>
        HashSet<Model.CRM.InstTraderInfo> GetInstIdsByBrokerId(string brokerId);

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
        DataRow SearchInstByInstId(string instid);
        DataRow SearchInstByInstIdAndInstName(string instid, string instname);
        DataTable SearchQuoteInst(string condition);
        DataTable SearchQuoteByIdbkey(string condition);
        DataTable SearchBillInst(string condition);
        DataTable SearchBillInstByKey(string condition);
        #endregion
        /// <summary>
        /// 根据idbKey获取机构信息
        /// 20200320
        /// </summary>
        /// <param name="idbKey"></param>
        /// <returns></returns>
        DataGridDataPo GetInstByIdbKey(string idbKey);

        #region Broker
        /// <summary>
        /// 通过SQL语句获取Broker信息
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="resultNum">返回数量</param>
        /// <returns></returns>
        DataTable SearchBrokerBySql(string sql, int resultNum);

        DataTable SearchBroker(string matchName, string filterName);
        DataTable SearchBroker2(string matchName, int resultNum);
        DataTable SearchBroker3(string matchName, int resultNum);
        DataTable SearchQuoteBroker(string matchName);
        DataTable SearchQuoteBrokerByAccount(string account);
        DataTable SearchBillBroker(string matchName, int resultNum);
        DataTable SearchBillBrokerByKey(string matchName, int resultNum);
        /// <summary>
        /// 根据名称检索用户
        /// </summary>
        /// <param name="matchName"></param>
        /// <returns></returns>
        DataTable SearchBillBrokerByName(string matchName);
        #endregion
    }
}
