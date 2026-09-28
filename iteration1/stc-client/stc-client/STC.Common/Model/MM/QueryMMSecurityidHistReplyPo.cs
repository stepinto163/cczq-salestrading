using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    public class QueryMMSecurityidHistReplyPo : BaseReplyPo
    {
        public List<MMSecurityidHist> Datas;
    }

    public class MMSecurityidHist
    {
        public String id;//
        public String venuetypegw;//交易平台
        public String venues;//交易执行渠道
        public String groupid;//债券分组id
        public String securityid;//产品唯一编号
        public String username;//stc用户
        public String operation;//操作
        public String detail;//详细信息
        public String createtime;//新建时间
        public String terminal;//终端
        public String ip;//服务器ip
        public String closeby;//操作人
        public String inf1;//
        public String inf2;//
        public String inf3;//
        public String inf4;//
        public String inf5;//
        public string actiontime;//操作时间
        public string beforeBidPrice;
        public string beforeOfrPrice;
        public string beforeSettleType;
        public string displayBidQty;
        public string displayOfrQty;
    }
}
