using STC.Broker.Plugin.Model.Transaction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.View.Transaction.UnderLineExec
{
    public abstract class BaseUnderlineExec
    {
        protected void setBrokerVal(OrderDetailPo changeOrder, string lineData, bool isA)
        {
            BondGridDataPo bestBond = new BondGridDataPo();//1 CRMService.Instance.GetBestBond(changeOrder.goodsCode, 20);
            DataTable temDt = new DataTable();//2 CRMService.Instance.GetBrokerByGoodstype(lineData, null, 1, bestBond == null ? string.Empty : bestBond.Goodstype);//CRMDataHandler.Instance.SearchBroker3(lineData, 1);
            if (temDt.Rows.Count > 0)
            {
                if (isA)
                {
                    changeOrder.PartySubID = temDt.Rows[0]["Idb_Key"] + "";
                    changeOrder.PartySubName = temDt.Rows[0]["AccountName"] + "";
                }
                else
                {
                    changeOrder.CounterPartySubID = temDt.Rows[0]["Idb_Key"] + "";
                    changeOrder.CounterPartySubName = temDt.Rows[0]["AccountName"] + "";
                }
            }
            else
            {
                if (isA)
                    changeOrder.PartySubID = changeOrder.PartySubName = string.Empty;
                else
                    changeOrder.CounterPartySubID = changeOrder.CounterPartySubName = string.Empty;
            }
        }

        /// <summary>
        /// 赋值Inst
        /// </summary>
        /// <param name="changeOrder"></param>
        /// <param name="lineData"></param>
        /// <param name="isA"></param>
        protected void setInstVal(OrderDetailPo changeOrder, string lineData, bool isA)
        {
            DataTable temDt = new DataTable();//3 CRMService.Instance.GetInstByName(lineData, 1);//CRMDataHandler.Instance.SearchInst4(lineData, 1);
            if (temDt.Rows.Count > 0)
            {
                if (isA)
                {
                    changeOrder.InstitutionID = temDt.Rows[0]["idbkey"] + "";
                    changeOrder.InstitutionName = GetInstName(temDt.Rows[0], changeOrder.SecurityID);
                }
                else
                {
                    changeOrder.CounterPartyInstitutionId = temDt.Rows[0]["idbkey"] + "";
                    changeOrder.CounterPartyInstitutionName = GetInstName(temDt.Rows[0], changeOrder.SecurityID);
                }
            }
            else
            {
                if (isA)
                    changeOrder.InstitutionID = changeOrder.InstitutionName = string.Empty;
                else
                    changeOrder.CounterPartyInstitutionId = changeOrder.CounterPartyInstitutionName = string.Empty;
            }
        }

        protected BondGridDataPo GetBestBond(string goodscode, int resultNum)
        {
            BondGridDataPo bestBond = null;
            List<BondGridDataPo> dtBond = new List<BondGridDataPo>();//4 CRMService.Instance.GetBondGridData(goodscode, 20);
            if (dtBond.Count > 0)
            {
                int bestLength = 100;
                foreach (BondGridDataPo bg in dtBond)
                {
                    if (bg.BondID.Length < bestLength)
                    {
                        bestLength = bg.BondID.Length;
                        bestBond = bg;
                    }
                }
            }
            return bestBond;
        }

        protected string GetInstName(DataRow dr, string goodcode)
        {
            string result = string.Empty;

            if (dr == null)
                return result;

            BondGridDataPo bg = GetBestBond(goodcode, 20);
            string goodstype = bg.Goodstype;
            if (dr == null)
                return result;

            if (!string.IsNullOrEmpty(goodstype))
            {
                //债券类型: 1信用,2利率,11 ABS,12 NCD
                if (goodstype == "2")
                    result = dr["bncshortnamelist"] + "";
                else if (goodstype == "1" || goodstype == "11")
                    result = dr["bcoshortnamelist"] + "";
                else if (goodstype == "12")
                    result = dr["ncdshortnamelist"] + "";
            }
            else
            {
                if (IsContain("2"))
                {
                    result = dr["bncshortnamelist"] + "";
                    if (string.IsNullOrEmpty(result))
                    {
                        if (IsContain("1") || IsContain("11"))
                            result = dr["bcoshortnamelist"] + "";
                    }

                    if (string.IsNullOrEmpty(result) && IsContain("12"))
                        result = dr["ncdshortnamelist"] + "";

                }
                else if (IsContain("1") || IsContain("11"))
                {
                    result = dr["bcoshortnamelist"] + "";
                    if (string.IsNullOrEmpty(result) && IsContain("12"))
                        result = dr["ncdshortnamelist"] + "";
                }
                else if (IsContain("12"))
                    result = dr["ncdshortnamelist"] + "";

            }


            if (string.IsNullOrEmpty(result))
                result = dr["shortnamecn"] + "";

            if (result.Contains(","))
            {
                string[] arr = result.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                    result = arr[0];
            }

            return result;
        }
        /// <summary>
        /// 是否包含债券权限
        /// </summary>
        /// <param name="goodstype"></param>
        /// <returns></returns>
        protected bool IsContain(string goodstype)
        {
            //List<string> termidsList = LocalEnv.UserInfo.Termids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            //if (termidsList != null && termidsList.Count > 0)
            //    return termidsList.Contains(goodstype);

            //return false;
            return true;
        }

        protected void setOrderNull(OrderDetailPo changeOrder)
        {
            changeOrder.CounterPartySubID = changeOrder.CounterPartySubName = string.Empty;
            changeOrder.CounterPartyInstitutionId = changeOrder.CounterPartyInstitutionName = string.Empty;
            changeOrder.CounterPartyTraderId = changeOrder.CounterPartyTraderName = string.Empty;
            changeOrder.PartySubID = changeOrder.PartySubName = string.Empty;
            changeOrder.TraderID = changeOrder.TraderName = string.Empty;
            changeOrder.InstitutionID = changeOrder.InstitutionName = string.Empty;
        }

        /// <summary>
        /// 赋值Trader
        /// </summary>
        /// <param name="changeOrder"></param>
        /// <param name="lineData"></param>
        /// <param name="isA"></param>
        protected void setTraderVal(OrderDetailPo changeOrder, string lineData, bool isA)
        {
            DataTable temDt = new DataTable();//6 CRMService.Instance.GetContactByName(lineData, 10);//CRMDataHandler.Instance.SearchContact(lineData, 10);
            if (temDt.Rows.Count > 0)
            {
                foreach (DataRow dr in temDt.Rows)
                {
                    string instid = dr["instid"] + "";

                    if (isA)
                    {
                        if (!string.IsNullOrEmpty(changeOrder.InstitutionID) && instid.Equals(changeOrder.InstitutionID))
                        {
                            changeOrder.TraderName = (dr["namecn"] + "").Trim();
                            changeOrder.TraderID = dr["idbkey"] + "";
                            break;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(changeOrder.CounterPartyInstitutionId) && instid.Equals(changeOrder.CounterPartyInstitutionId))
                        {
                            changeOrder.CounterPartyTraderName = (dr["namecn"] + "").Trim();
                            changeOrder.CounterPartyTraderId = dr["idbkey"] + "";
                            break;
                        }
                    }
                }
            }
            else
            {
                if (isA)
                    changeOrder.TraderName = changeOrder.TraderID = string.Empty;
                else
                    changeOrder.CounterPartyTraderName = changeOrder.CounterPartyTraderId = string.Empty;
            }
        }

        public abstract void Analysis(int index, OrderDetailPo changeOrder, string[] lineData);
    }
}
