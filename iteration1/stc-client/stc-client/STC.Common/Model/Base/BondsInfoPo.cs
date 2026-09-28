using STC.Common.Handler.Base;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Base
{
    /// <summary>
    /// 债券信息
    /// </summary>
    public class BondsInfoPo
    {
        public string Bond_ID { get; set; }
        public string Bond_Key { get; set; }
        public string bondlistinfo_modify_date { get; set; }
        public string bond_modify_date { get; set; }
        public string delflag { get; set; }
        public string Maturity_Date { get; set; }
        public string RemaingingTime { get; set; }
        public string Short_Name { get; set; }
        public string listed_Market { get; set; }
        public string Bond_Subtype { get; set; }
        //public string IssuerRatingCurrent { get; set; }
        public string pinYin { get; set; }
        public string pinyin_full { get; set; }
        public string SECURITY_TYPE { get; set; }
        public decimal? Option_Date { get; set; }

        public string Option_Type { get; set; }
        
        public string Security_Type_Id { get; set; }
        public string Security_Dec { get; set; }
        public string Rating_Current { get; set; }
        public string Full_Name_CN { get; set; }
        public string Full_Name_EN { get; set; }
        public string flag { get; set; }

        public string bondmaster_modify_date { get; set; }
        public string Ent_Cor { get; set; }
        public string Issuer_Rating_Current_NPY { get; set; }

        public string Issuer_Rating_Current { get; set; }

        public string MarketIndicator { get; set; }

        public string SECURITY_DEC { get; set; }

        public string ISSUE_DATE { get; set; }

        public string Issuer_Code { get; set; }

        public string issuer { get; set; }

        public string searchKey { get; set; }

        public string Security_Term { get; set; }

        /// <summary>
        /// 发行人
        /// </summary>
        public string CN_Short_Name { get; set; }

        /// <summary>
        /// 0-意向；1-净价；2-全价；3-收益率；4-利差；8-经纪商未指定 如果没有输入则为 3-收益率
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// 票面
        /// </summary>
        public string Coupon_Rate_Current { get; set; }

        private CCDCValuaModel ccdcvalua;
        /// <summary>
        /// 中债估值 信息
        /// </summary>
        public CCDCValuaModel CCDCValua 
        {
            get
            {
                return ccdcvalua;
            }
            set
            {
                ccdcvalua = value;
            } 
        }

        public string Min_change { get; set; }

        /// <summary>
        /// 债券评级
        /// </summary>
        public string Rating_Current_NPY { get; set; }

        //todo...
        /// <summary>
        /// 基础利率代码
        /// </summary>
        public string FRNIndexID { get; set; }

        /// <summary>
        /// Asset_Status
        /// </summary>
        public string Asset_Status { get; set; }

        /// <summary>
        /// 债券发行量
        /// </summary>
        public decimal? Issue_Amount { get; set; }

        /// <summary>
        /// 债券分类
        /// </summary>
        public string Fund_Objectives_Category { get; set; }

        /// <summary>
        /// 分类标签
        /// </summary>
        public string Category_flag { get; set; }


        /// <summary>
        /// 债券类型 BCO,BNC,ABS,NCD  转换关系：1-信用债 BCO 2-利率债 BNC 11-ABS 12-NCD 
        /// </summary>
        public string symbolType { get; set; }

        /// <summary>
        /// tag
        /// </summary>
        public string bondSigns { get; set; }

        /// <summary>
        /// cdcAuth
        /// </summary>
        public string cdcAuth { get; set; }

        public List<string> SignList { get; set; }

        public List<Dictionary<string, string>> SignDicList { get; set; }

        public void ParserSign()
        {
            if (!string.IsNullOrEmpty(this.bondSigns))
            {
                SignList = new List<string>();
                SignDicList = new List<Dictionary<string, string>>();

                string[] arr = this.bondSigns.Split(Consts.COMMA_CHAR);

                int i = 0;
                foreach(string sign in arr)
                {
                    SignList.Add(sign);
                    if (i == 0)
                    {
                        SignDicList.Add(ParseSign("sign", sign.ConvertToLong()));
                    }
                    else if (i == 1)
                    {
                        SignDicList.Add(ParseSign("sign1", sign.ConvertToLong()));
                    }
                    else if (i == 2)
                    {
                        SignDicList.Add(ParseSign("sign2", sign.ConvertToLong()));
                    }
                    else if (i == 3)
                    {
                        SignDicList.Add(ParseSign("sign3", sign.ConvertToLong()));
                    }

                    i++;
                }
            }
        }

        private Dictionary<string, string> ParseSign(string signField, long sign)
        {
            Dictionary<string, QuoteTag> quoteTagDic = BaseHandler.Instance.GetQuoteTagDic();
            string a = Utils.CommonUtils.ToBinary(sign);
            if (!string.IsNullOrEmpty(a))
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (KeyValuePair<string, QuoteTag> pair in quoteTagDic)
                {
                    if (pair.Value.signField == signField)
                    {
                        dic[pair.Value.tagType] = a.Substring(pair.Value.tagTypeStartIndex, pair.Value.tagTypeLength);
                    }
                }
                return dic;
            }
            return null;
        }
    }
    public class CCDCValuaModel
    {
        /// <summary>
        /// 债券的ID
        /// </summary>
        public string SecurityID { get; set; }

        /// <summary>
        ///  CCDCValuaType 1表示行权债中债，为2表示到期中债
        /// </summary>
        public string CCDCValuaType { get; set; }

        /// <summary>
        /// 中债估值日期
        /// </summary>
        public string CCDCValuaDate { get; set; }

        /// <summary>
        /// 中债估值
        /// </summary>
        public string CCDCValuation { get; set; }

        ///若为含权债，采用中债推荐估值，当中债推荐估值为"到期收益率"时无特殊展示，当中债推荐估值为"行权收益率"时展示"行权"字样。（CDCValuation，待产品确认 ）
        ///若上个工作日的中债估值空缺，则以(!)标识，并前溯最新中债估值进行展示，鼠标hover在(!)上时，展示具体估值日期。若前溯也无法找到中债估值，则不展示中债估值收益率。
        /// <summary>
        /// 用于显示的中债估值
        /// </summary>
        public string CCDCValuationShow { get; set; }

        /// <summary>
        /// 中债估值日期警告标记 !
        /// </summary>
        public string ShowLastWorkDay { get; set; }

        /// <summary>
        /// 上一个工作日
        /// </summary>
        public string LastWorkDay { get; set; }

        private string _orderCount = string.Empty;

        public string OrderCount
        {
            get
            {
                return _orderCount;
            }

            set
            {
                _orderCount = value;
            }
        }

    }
}
