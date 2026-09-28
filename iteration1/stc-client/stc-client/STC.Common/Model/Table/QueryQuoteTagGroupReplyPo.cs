using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryQuoteTagGroupReplyPo : BaseReplyPo
    {
        public List<QuoteTagGroup> Datas;
    }

    public class QuoteTagGroup
    {
        public string id;//
        public string groupname;//分数名字
        public int ordersn;//顺序
        public string username;//stc用户
        public string terminal;//终端
        public string ip;//服务器ip
        public string closeby;//操作人
        public string remark;//备注
        public string createtime;//创建时间
        public string updatetime;//更新时间
        public string inf1;//扩展字段1，分类方式，TAG(标签）/BOND(债券)
        public string inf2;//扩展字段2，债券ID
        public string inf3;//扩展字段3，关注债券方向，0:All,1:Buy,2:Sell; // 如果是DYNAMIC_PARAM， 回显term和coupon
        public string inf4;//扩展字段4
        public string inf5;//扩展字段5
        public string focusTags;//关注的标签
        public string ifSubscribe;//是否订阅 0:不订阅，1:订阅
        public string marketindicator;//资产类别
        ////动态期限字符串 5Y30D,10Y60D
        //public string termRangeStr;
        ////动态期限的天数 3650,7300
        //public string termDays;
        //// 添加动态票面订阅 xxx,xxx
        //public string couponRangeStr;
    }
}
