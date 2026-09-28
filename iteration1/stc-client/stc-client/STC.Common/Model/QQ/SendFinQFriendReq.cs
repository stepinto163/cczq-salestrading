using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.QQ
{
    public class SendFinQFriendReq
    {
        public string content;
        public DataMessage data;
        public string fromQB;
        public bool openChat;
        public string fromQBAccountID;
        public int dataType;//0或null：点击单家最优；1：点击综合最优；2：点击成交价
        public string UpdateTime;
    }

    public class SendFinQFriendReq_Best
    {
        public string content;
        public List<DataMessage> data;
        public string fromQB;
        public bool openChat;
        public string fromQBAccountID;
        public int dataType;//0或null：点击单家最优；1：点击综合最优；2：点击成交价
        public string UpdateTime;
    }

    public class DataMessage
    {
        public string id;

        public string code;
        public string name;
        public string side;
        public string price;
        public string[,] vol_delivery;
        public string join;

        public string comp_id;//1 = TPSC 国利; 2 = CNEX 国际; 3 = CCTB 中诚; 4 = PTCN 平安; 5 = CTTJ 信唐; 6 = UEDA 上田
        public string broker_qq; //这一组数据要发送给的经纪人QQ号
        public string bond_type;//债券类型 BNC BCO NCD
        public string price_type;//价格类型：1=净价；2=全价；3=收益率；4=利差
        public string yield_type;
        public string ext;//调用方定义字段，可填入调用方自定义ID或 JSON string 等自定义内容
        public string settl_type;//交割，即当前成交对应的交割。"0"=空值 "1"=T+0 "2"=T+1 "4"=FWD
        public string[,] opp_vol_delivery;
        public string opp_price;
        public string opp_price_type;
        public string opp_yield_type;
        /// <summary>示例: [["3000","2","08.26+1"]]</summary>
        public List<List<string>> vol_delivery_qb;
        public string val_yield; //中债估值
    }

    public class GetFinQTokenReq
    {
        /// <summary>
        /// 用户 QQ 号
        /// </summary>
        public string fromQQ;

        /// <summary>
        /// 用户 qbID
        /// </summary>
        public string fromQB;

        /// <summary>
        /// 用户 qb account ID可选 用户的 qb account ID 示例值:50364cc3e45e40b4949d8be04ee2abcb
        /// </summary>
        public string fromQBAccountID;

        /// <summary>
        /// 接口调用方 qb-others 或 qb 或 bdb
        /// </summary>
        public string fromService;
    }


    public class FQBindTokenResponse
    {
        /// <summary>
        /// 操作ID 同 /bind_token 回复中 actionID, 可用于串联整个异步流程
        /// </summary>
        public string actionID { get; set; }

        /// <summary>
        /// 错误代码  401错误
        /// </summary>
        public int errCode { get; set; }

        /// <summary>
        /// 错误信息，仅供参考
        /// </summary>
        public string errMsg { get; set; }

        /// <summary>
        /// 原始错误信息，仅供参考
        /// </summary>
        public string error { get; set; }
        /// <summary>
        /// QQ 账号
        /// </summary>
        public string fromQQ { get; set; }
        /// <summary>
        /// QB 用户名
        /// </summary>
        public string fromQB { get; set; }

        public userInfo userInfo { get; set; }

        public token token { get; set; }
    }

    public class userInfo
    {
        /// <summary>
        /// 企点 QQ
        /// </summary>
        public string qq { get; set; }

        /// <summary>
        /// 企点机构 ID
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 企点 OpenID
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// FinQ 用户 ID
        /// </summary>
        public string userid { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 所属机构名称
        /// </summary>
        public string corpname { get; set; }

        /// <summary>
        /// QB Account ID
        /// </summary>
        public string qbAcc { get; set; }

        /// <summary>
        /// token v2   orgid
        /// </summary>
        public string orgid { get; set; }


        /// <summary>
        /// token v2   qb
        /// </summary>
        public string qb { get; set; }
    }

    public class token
    {
        /// <summary>
        /// 创建时间戳
        /// </summary>
        public long createTs { get; set; }

        /// <summary>
        /// 有效时长
        /// </summary>
        public long expiresIn { get; set; }


        /// <summary>
        /// 部分发消息用 token，由服务端传递的发消息用 token 部分，通过请求接口返回
        /// </summary>
        public string serverToken { get; set; }


        /// <summary>
        /// 部分发消息用 token,由客户端传递的发消息用 token 部分, 通过回调接口返回
        /// </summary>
        public string clientToken { get; set; }
        
    }

    public class SendTokenResponse
    { 
        public string Code;
        public string Msg;
        public string token;
        /// <summary>
        /// 有效时长
        /// </summary>
        public string ExpiresTime;

        public long createTs;
        public long expiresIn;
    }

}
