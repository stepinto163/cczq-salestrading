using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class PtExecOrderQueryReq
    {
        //债券编号
        public string securityid;
        //t+0:1 t+1:2 明+0:3 远期：F
        public string settltype;
        //订单编号
        public string strategyorderid;
        //报价编号
        public string counterpartyordid;
        //对手机构id
        public string counterpartyid;
        //对手交易员id
        public string counterpartysubid;
        //交易日期开始时间
        public string tradedateBegin;
        //交易日期结束时间
        public string tradedateEnd;
        //结算日期开始时间
        public string settldateBegin;
        //结算日期结束时间
        public string settldateEnd;
        //债券类型
        public string symboltype;
        //交易场所
        public string venues;
        //策略名称
        public string quantid;
        //交易方向
        public string side;
        //删除标记 0:未删除， 1:已删除
        public string delflag;
        //用户名
        public string userName;
        public string userId;
        //下发状态
        public bool send;
        //历史交易
        public bool histOrder;


        public string bondType;//bond/irs

        public string approvalstatus;
        //机构id
        public string institutionid;
        //客户id
        public string traderid;



        public string dealTimeBegin;
        public string dealTimeEnd;

        public string createTimeBegin;
        public string createTimeEnd;

        public int pageNum = 0;
        public int pageSize = 5000;
        public string execid;
        public string venueexecid;
        public string ownOrder;//Y-我；N-所有
                               
        public string partysubid;//本方交易员id
        public string partymanagerid;//投资经理ID
        public string sendstatus;//1已下发，0未下发，2发失败
        public List<string> execIdList;

        public PtExecOrderQueryReq Clone() 
        {
            PtExecOrderQueryReq req = new PtExecOrderQueryReq();
            req.securityid = this.securityid;
            req.settltype = this.settltype;
            req.strategyorderid = this.strategyorderid;
            req.counterpartyordid = this.counterpartyordid;
            req.counterpartyid = this.counterpartyid;
            req.counterpartysubid = this.counterpartysubid;
            req.tradedateBegin = this.tradedateBegin;
            req.tradedateEnd = this.tradedateEnd;
            req.settldateBegin = this.settldateBegin;
            req.settldateEnd = this.settldateEnd;
            req.symboltype = this.symboltype;
            req.venues = this.venues;
            req.quantid = this.quantid;
            req.side = this.side;
            req.delflag = this.delflag;
            req.userName = this.userName;
            req.userId = this.userId;
            req.send = this.send;
            req.histOrder = this.histOrder;
            req.bondType = this.bondType;
            req.approvalstatus = this.approvalstatus;
            req.institutionid = this.institutionid;
            req.traderid = this.traderid;
            req.dealTimeBegin = this.dealTimeBegin;
            req.dealTimeEnd = this.dealTimeEnd;
            req.createTimeBegin = this.createTimeBegin;
            req.createTimeEnd = this.createTimeEnd;
            req.pageNum = this.pageNum;
            req.pageSize = this.pageSize;
            req.execid = this.execid;
            req.venueexecid = this.venueexecid;
            req.ownOrder = this.ownOrder;
            req.partysubid = this.partysubid;
            req.partymanagerid = this.partymanagerid;
            req.sendstatus = this.sendstatus;
            req.execIdList = this.execIdList;
            return req;
        }
    }
    public class PTExecOrderUnSendReq
    {
        public int pageNum = 0;
        public int pageSize = 1000;
    }
}