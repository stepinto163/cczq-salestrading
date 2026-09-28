using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryQuoteTagGroupRequest
    {
        public string id;
        public List<string> bondTypes;//债券类型
        public List<string> terms;//债券期限
        public List<string> firms;//是否意向单
        public List<string> settlTypes;//清算速度
        public List<string> sides;//方向
        public List<string> ifVip;//是否VIP
        public string focusType;//关注类型TAG:标签关注 BOND:债券关注 TERM:期限 COUPON:票面
        public string securityId;//关注的债券代码
        public string securitySide;//关注的债券方向
        public string groupname;//分组名字
        public int ordersn;//顺序
        public string username;//stc用户
        public string terminal;//终端
        public string ip;//服务器ip
        public string closeby;//操作人
        public string remark;//备注
        public string createtime;//创建时间
        public string updatetime;//更新时间
        public string ifSubscribe;//是否订阅 0:不订阅，1:订阅
        public string action;//操作 0:查询，1:新增，2:修改，3:删除
        public List<string> roleTypes;//角色类型
        public List<string> orgTypes;//客户机构标签
        public List<string> finBondTypes;//金融债标签
        public List<string> dqTypes; //做市未回

        public string focusTags;//标签组
        //动态期限字符串 5Y30D,10Y60D
        public string termRangeStr;
        //动态期限的天数 3650,7300
        public string termDays;
        // 添加动态票面订阅 xxx,xxx
        public string couponRangeStr;
        // 报价动态订阅
        public string priceRangeStr;
        // 估值动态订阅
        public string valuationRangeStr;

        public string marketindicator;// 资产品种

        public string focusGroupId;// 订阅券池id

        public string bidSubValRangeStr;//bid-中债

        public string valSubOfrRangeStr;//中债-ofr
    }
}
