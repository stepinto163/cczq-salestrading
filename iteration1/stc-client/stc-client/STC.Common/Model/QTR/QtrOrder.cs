using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.QTR
{
    /*
     * 订单反馈
     */
    public class QtrOrder
    {
        public string qmID;
        //"经纪商代码，取值：
        //1 = TPSC 国利
        //2 = CNEX 国际
        //3 = CCTB 中诚
        //4 = PTCN 平安
        //5 = CTTJ 信唐"
        public string senderCompID;
        //发送方经纪人标识
        public string senderSubID;
        //接收方所在机构标识（SC码）
        public string deliverToCompID;
        //接收方交易员标识（QB账户名）
        public string deliverToSubID;
        //市场：4=现券买卖
        public int marketIndicator;
        //报文发送时间，格式：yyyy-MM-dd HH:mm:ss.SSS
        public string sendingTime;
        //订单类型：2=现价订单，Limit
        public string ordType;
        //客户端参考编号
        public string clOrdID;
        //订单编号
        public string orderID;
        //订单执行操作编号
        public string execID;
        //"0=new，新建订单
        //4=canceled，订单被撤销
        //5=replaced，订单被覆盖更新
        //8=rejected，订单被拒绝
        //C=expired，订单过期
        //F=trade，订单被成交或部分成交
        //M=locked，订单被锁"
        public string execType;
        //"当ordStatus=4时，必传：
        //3=order cancelled upon request，表示是由api客户发起的订单取消操作
        //4=unsolicited order cancellation，表示由broker发起的订单取消操作"
        public int execTypeReason;
        //"订单状态：
        //0 = 有效 New
        //1 = 部分成交Partially filled
        //2 = 全部成交 Filled
        //4 = 撤销 Canceled
        //8 = 拒绝 Reject
        //C = 过期 Expired"
        public string ordStatus;
        //当OrdStatus=8时，描述订单拒绝理由
        public int ordRejReason;
        //当OrdRejReason=99时，表示错误原因的文字说明
        public string rejectText;
        //债券代码
        public string securityID;
        //交易方向，指本方的买卖方向: 1=买入 2=卖出
        public int side;
        //原始订单价格
        public decimal price;
        //原始订单报价方式，传输MATURITY
        public string yieldType;
        //原始订单收益率
        public decimal yield;

        //当ExecType=F时必传，表示本次成交价格，收益率，单位%
        public decimal lastPx;
        //当ExecType=F时必传，表示本次成交量，单位万元
        public decimal lastQty;
        //"当OrdStatus=0时，表示原始订单数量；
        //当ExecType=F时必传，表示订单的原始数量，单位万元"
        public decimal orderQty;
        //当ExecType=F时必传，表示本订单目前为止的总成交量，单位 万元
        public decimal cumQty;
        //当ExecType=F时必传，表示本订单剩余可成交量，单位 万元
        public decimal leavesQty;
        //当ExecType=F时必传，表示成交日期
        public string tradeDate;
        //当ExecType=F时必传，表示清算日期
        public string settlDate;
        //"当ExecType=F时必传，表示清算速度：
        //1=T+0
        //2=T+1
        //3=明天+0
        //MM.DD+数字=远期"
        public string settlType;
        //当ExecType=F时必传，表示核单号的值
        public string text;

        public string stcText;

        public string venues;
        public string dataSourcestring;
        public string symbol;
        public string transactTime;

        public string seq;

        public string comment;
    }
}
