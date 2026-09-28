using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{

    /// <summary>
    /// pt执行订单明细
    /// </summary>
    /// <remarks>
    /// 2023.08.25: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
    /// </remarks>
    [Serializable]
    public class TbookPtExecorders
    {
        #region 字段

        private string _execid = "";//执行订单id
        private string _strategyorderid;//策略订单编号
        private string _orderid = "";//订单id
        private string _venueexecid = "";//交易执行渠道生成的执行订单id
        private string _venueid = "";//交易执行渠道生成的订单id
        private string _venuetypegw;//交易平台
        private string _venues = "";//交易执行渠道
        private string _securityid;//产品唯一编号
        private string _symbol;//产品名称
        private string _symboltype;//产品类型
        private string _marketindicator;//市场
        private string _partysubid;//交易执行渠道帐号
        private string _username;//stc用户
        private decimal? _price;//价格
        private decimal? _qty;//报量
        private string _exectype;//执行状态
        private string _orderstatus;//交易状态
        private string _ordersn;//订单顺序号
        private string _terminal;//终端
        private string _yield;//收益率
        private decimal? _netprice;//净价
        private string _remark;//备注
        private string _rejecter;//拒绝者
        private string _rejectercode;//拒绝代码
        private string _rejecterreason;//拒绝原因
        private string _ip;//服务器ip
        private string _createtime;//创建时间
        private string _updatetime;//更新时间
        private string _inf1;//扩展字段1
        private string _inf2;//扩展字段2
        private string _inf3;//扩展字段3
        private string _inf4;//扩展字段4
        private string _inf5;//扩展字段5
        private string _inf6;//扩展字段6
        private string _counterpartyid;//对手机构id
        private string _counterpartyname = "";//对手机构名
        private string _counterpartysubid;//对手交易员id
        private string _counterpartysubname;//对手交易员名字
        private string _counterpartycode;//对手方机构21位码
        private string _partyid;//本方机构id
        private string _userid;//用户id
        private string _firmflag;//firm标记
        private string _wholeflag;//整量
        private string _name;//名字
        private string _quantid;//量化编号
        private string _partyname;//本方机构名称
        private string _partysubname;//本方交易员名字
        private string _partycode;//机构21位码
        private string _settltype;//清算类型
        private string _side;//方向
        private string _timeinforce;//有效时间
        private string _ordtype;//订单类型
        private string _clordid;//客户订单编号
        private decimal? _dirtyprice;//全价
        private string _transacttime;//业务发生时间
        private string _blocknum;//交易包号
        private string _tradedate;//交易日
        private string _settldate;//交割日
        private string _counterpartyordid;//对手方订单id
        private string _synchflag;//同步标记
        private string _biztype;//book部门
        private string _biztypeid;//book部门ID
        private string _showtrader;//客盘保密标记
        private string _roleid;//角色id
        private string _rolename;//角色名字
        private string _salesagentflag;//销售标记
        private string _origordprice;//订单原始报价
        private string _counterpartysalesagentflag;//对手代客标记
        private string _institutionid;//机构id
        private string _institutionname;//机构名
        private string _traderid;//交易员id
        private string _tradername;//交易员名字
        private string _counterpartyfullname;//对手机构全称
        private string _strategyordervenues;//策略订单渠道
        private string _counterpartyvenues;//对手渠道
        private string _brokerexecid;//内码
        private string _exectypereason;//执行原因
        private string _trademethod;//交易方法
        private string _delflag;//删除标记
        private string _sendingtime;//接收时间
        private string _b0;//bid t+0收益率
        private string _b1;//bid t+1收益率
        private string _b2;//bid远期收益率
        private string _m5b1;//全市场bidt+1收益率
        private string _a0;//ofrt+0收益率
        private string _a1;//ofrt+1收益率
        private string _a2;//ofr远期收益率
        private string _m5a1;//全市场ofrt+1收益率
        private string _vs;//是否为异常数据(Y-正常,N-异常)
        private string _bookid;//组合id
        private string _qmnumber;//qm 编号
        private string _partysubid2;//sor通道放交易员qm帐号
        private string _intentionid;//意向交易编号
        private string _offsetflag;//开平标志 0是开仓，1是平仓/平昨，2是平今
        private string _hedgeflag;//投机套保 0-投机  1-套利  2-套保
        private string _pricetype;//价格类型
        private string _exerciseflag;//执行标志
        private string _ssdetect;//ss检查
        private string _accountid;//账户id
        private string _accountname;//账户名称
        private string _tag1;//标签
        private string _counteraccountid;//对手方账号ID
        private string _counteraccountname;//对手方账号名称
        private string _matchstatus = "5";//交易匹配状态:5-未匹配,"6"-"部分匹配","7"-"全部匹配","8"-"超量匹配"
        private string _relatestatus = "0";//关联状态:0-未关联,1-已关联
        private string _sendstatus;//下发送状态:0-发送失败,1-发送成功
        private string _quotemethod;//报价方式:0-待定/1-发请求/2-收请求/3-发对话/4-收对话
        private string _entrytype;//录入方式 0-自动录入 1-手工录入
        private string _seatnumber;//席位号
        private string _approvalstatus;//审批状态//1-新建 2-审批中 3-审批拒绝 4-审批通过 5-交易执行中 6-部分撤销 7-已撤销 8-交易终止 9-成交确认
        private int _maturityDays;
        private string _issuerCode;
        private string __warranterCode;
        private string _issuerName;
        private string __warranterName;
        private decimal? _yieldtoexecution;//行权收益率
        private decimal? _fullPrice;//全价
        private string _realcounterpartyname;//终端交易对手
        private string _trdSubType;//发单方向
        private string _trdType;//发单方式
        private decimal? _settlAmount;//结算金额？
        private string _bridgePaymentFixedRate;//DF费率
        private decimal? _yield2;//费后收益率
        private decimal? _cleanPrice2;//费后净价
        private decimal? _fullPrice2;//费后全价
        private string _trdMatchId;//约定号

        #endregion

        #region 属性

        /// <summary>
        /// 执行订单id
        /// </summary>
        public string Execid
        {
            get { return _execid; }
            set { _execid = value; }
        }

        /// <summary>
        /// 策略订单编号
        /// </summary>
        public string Strategyorderid
        {
            get { return _strategyorderid; }
            set { _strategyorderid = value; }
        }

        /// <summary>
        /// 订单id
        /// </summary>
        public string Orderid
        {
            get { return _orderid; }
            set { _orderid = value; }
        }

        /// <summary>
        /// 交易执行渠道生成的执行订单id
        /// </summary>
        public string Venueexecid
        {
            get { return _venueexecid; }
            set { _venueexecid = value; }
        }

        /// <summary>
        /// 交易执行渠道生成的订单id
        /// </summary>
        public string Venueid
        {
            get { return _venueid; }
            set { _venueid = value; }
        }

        /// <summary>
        /// 交易平台
        /// </summary>
        public string Venuetypegw
        {
            get { return _venuetypegw; }
            set { _venuetypegw = value; }
        }

        /// <summary>
        /// 交易执行渠道
        /// </summary>
        public string Venues
        {
            get { return _venues; }
            set { _venues = value; }
        }

        /// <summary>
        /// 产品唯一编号
        /// </summary>
        public string Securityid
        {
            get { return _securityid; }
            set { _securityid = value; }
        }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        /// <summary>
        /// 产品类型
        /// </summary>
        public string Symboltype
        {
            get { return _symboltype; }
            set { _symboltype = value; }
        }

        /// <summary>
        /// 市场
        /// </summary>
        public string Marketindicator
        {
            get { return _marketindicator; }
            set { _marketindicator = value; }
        }

        /// <summary>
        /// 交易执行渠道帐号
        /// </summary>
        public string Partysubid
        {
            get { return _partysubid; }
            set { _partysubid = value; }
        }

        /// <summary>
        /// stc用户
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal? Price
        {
            get { return _price; }
            set { _price = value; }
        }

        /// <summary>
        /// 报量
        /// </summary>
        public decimal? Qty
        {
            get { return _qty; }
            set { _qty = value; }
        }

        /// <summary>
        /// 执行状态
        /// </summary>
        public string Exectype
        {
            get { return _exectype; }
            set { _exectype = value; }
        }

        /// <summary>
        /// 交易状态
        /// </summary>
        public string Orderstatus
        {
            get { return _orderstatus; }
            set { _orderstatus = value; }
        }

        /// <summary>
        /// 订单顺序号
        /// </summary>
        public string Ordersn
        {
            get { return _ordersn; }
            set { _ordersn = value; }
        }

        /// <summary>
        /// 终端
        /// </summary>
        public string Terminal
        {
            get { return _terminal; }
            set { _terminal = value; }
        }

        /// <summary>
        /// 收益率
        /// </summary>
        public string Yield
        {
            get { return _yield; }
            set { _yield = value; }
        }

        /// <summary>
        /// 净价
        /// </summary>
        public decimal? Netprice
        {
            get { return _netprice; }
            set { _netprice = value; }
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get { return _remark; }
            set { _remark = value; }
        }

        /// <summary>
        /// 拒绝者
        /// </summary>
        public string Rejecter
        {
            get { return _rejecter; }
            set { _rejecter = value; }
        }

        /// <summary>
        /// 拒绝代码
        /// </summary>
        public string Rejectercode
        {
            get { return _rejectercode; }
            set { _rejectercode = value; }
        }

        /// <summary>
        /// 拒绝原因
        /// </summary>
        public string Rejecterreason
        {
            get { return _rejecterreason; }
            set { _rejecterreason = value; }
        }

        /// <summary>
        /// 服务器ip
        /// </summary>
        public string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string Createtime
        {
            get { return _createtime; }
            set { _createtime = value; }
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string Updatetime
        {
            get { return _updatetime; }
            set { _updatetime = value; }
        }

        /// <summary>
        /// 扩展字段1
        /// </summary>
        public string Inf1
        {
            get { return _inf1; }
            set { _inf1 = value; }
        }

        /// <summary>
        /// 扩展字段2
        /// </summary>
        public string Inf2
        {
            get { return _inf2; }
            set { _inf2 = value; }
        }

        /// <summary>
        /// 扩展字段3
        /// </summary>
        public string Inf3
        {
            get { return _inf3; }
            set { _inf3 = value; }
        }

        /// <summary>
        /// 扩展字段4
        /// </summary>
        public string Inf4
        {
            get { return _inf4; }
            set { _inf4 = value; }
        }

        /// <summary>
        /// 扩展字段5
        /// </summary>
        public string Inf5
        {
            get { return _inf5; }
            set { _inf5 = value; }
        }

        /// <summary>
        /// 扩展字段6
        /// </summary>
        public string Inf6
        {
            get { return _inf6; }
            set { _inf6 = value; }
        }

        /// <summary>
        /// 对手机构id
        /// </summary>
        public string Counterpartyid
        {
            get { return _counterpartyid; }
            set { _counterpartyid = value; }
        }

        /// <summary>
        /// 对手机构名
        /// </summary>
        public string Counterpartyname
        {
            get { return _counterpartyname; }
            set { _counterpartyname = value; }
        }

        /// <summary>
        /// 对手交易员id
        /// </summary>
        public string Counterpartysubid
        {
            get { return _counterpartysubid; }
            set { _counterpartysubid = value; }
        }

        /// <summary>
        /// 对手交易员名字
        /// </summary>
        public string Counterpartysubname
        {
            get { return _counterpartysubname; }
            set { _counterpartysubname = value; }
        }

        /// <summary>
        /// 对手方机构21位码
        /// </summary>
        public string Counterpartycode
        {
            get { return _counterpartycode; }
            set { _counterpartycode = value; }
        }

        /// <summary>
        /// 本方机构id
        /// </summary>
        public string Partyid
        {
            get { return _partyid; }
            set { _partyid = value; }
        }

        /// <summary>
        /// 用户id
        /// </summary>
        public string Userid
        {
            get { return _userid; }
            set { _userid = value; }
        }

        /// <summary>
        /// firm标记
        /// </summary>
        public string Firmflag
        {
            get { return _firmflag; }
            set { _firmflag = value; }
        }

        /// <summary>
        /// 整量
        /// </summary>
        public string Wholeflag
        {
            get { return _wholeflag; }
            set { _wholeflag = value; }
        }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 量化编号
        /// </summary>
        public string Quantid
        {
            get { return _quantid; }
            set { _quantid = value; }
        }

        /// <summary>
        /// 本方机构名称
        /// </summary>
        public string Partyname
        {
            get { return _partyname; }
            set { _partyname = value; }
        }

        /// <summary>
        /// 本方交易员名字
        /// </summary>
        public string Partysubname
        {
            get { return _partysubname; }
            set { _partysubname = value; }
        }

        /// <summary>
        /// 机构21位码
        /// </summary>
        public string Partycode
        {
            get { return _partycode; }
            set { _partycode = value; }
        }

        /// <summary>
        /// 清算类型
        /// </summary>
        public string Settltype
        {
            get { return _settltype; }
            set { _settltype = value; }
        }

        /// <summary>
        /// 方向
        /// </summary>
        public string Side
        {
            get { return _side; }
            set { _side = value; }
        }

        /// <summary>
        /// 有效时间
        /// </summary>
        public string Timeinforce
        {
            get { return _timeinforce; }
            set { _timeinforce = value; }
        }

        /// <summary>
        /// 订单类型
        /// </summary>
        public string Ordtype
        {
            get { return _ordtype; }
            set { _ordtype = value; }
        }

        /// <summary>
        /// 客户订单编号
        /// </summary>
        public string Clordid
        {
            get { return _clordid; }
            set { _clordid = value; }
        }

        /// <summary>
        /// 全价
        /// </summary>
        public decimal? Dirtyprice
        {
            get { return _dirtyprice; }
            set { _dirtyprice = value; }
        }

        /// <summary>
        /// 业务发生时间
        /// </summary>
        public string Transacttime
        {
            get { return _transacttime; }
            set { _transacttime = value; }
        }

        /// <summary>
        /// 交易包号
        /// </summary>
        public string Blocknum
        {
            get { return _blocknum; }
            set { _blocknum = value; }
        }

        /// <summary>
        /// 交易日
        /// </summary>
        public string Tradedate
        {
            get { return _tradedate; }
            set { _tradedate = value; }
        }

        /// <summary>
        /// 交割日
        /// </summary>
        public string Settldate
        {
            get { return _settldate; }
            set { _settldate = value; }
        }

        /// <summary>
        /// 对手方订单id
        /// </summary>
        public string Counterpartyordid
        {
            get { return _counterpartyordid; }
            set { _counterpartyordid = value; }
        }

        /// <summary>
        /// 同步标记
        /// </summary>
        public string Synchflag
        {
            get { return _synchflag; }
            set { _synchflag = value; }
        }

        /// <summary>
        /// book部门
        /// </summary>
        public string Biztype
        {
            get { return _biztype; }
            set { _biztype = value; }
        }

        /// <summary>
        /// book部门ID
        /// </summary>
        public string Biztypeid
        {
            get { return _biztypeid; }
            set { _biztypeid = value; }
        }

        /// <summary>
        /// 客盘保密标记
        /// </summary>
        public string Showtrader
        {
            get { return _showtrader; }
            set { _showtrader = value; }
        }

        /// <summary>
        /// 角色id
        /// </summary>
        public string Roleid
        {
            get { return _roleid; }
            set { _roleid = value; }
        }

        /// <summary>
        /// 角色名字
        /// </summary>
        public string Rolename
        {
            get { return _rolename; }
            set { _rolename = value; }
        }

        /// <summary>
        /// 销售标记
        /// </summary>
        public string Salesagentflag
        {
            get { return _salesagentflag; }
            set { _salesagentflag = value; }
        }

        /// <summary>
        /// 订单原始报价
        /// </summary>
        public string Origordprice
        {
            get { return _origordprice; }
            set { _origordprice = value; }
        }

        /// <summary>
        /// 对手代客标记
        /// </summary>
        public string Counterpartysalesagentflag
        {
            get { return _counterpartysalesagentflag; }
            set { _counterpartysalesagentflag = value; }
        }

        /// <summary>
        /// 机构id
        /// </summary>
        public string Institutionid
        {
            get { return _institutionid; }
            set { _institutionid = value; }
        }

        /// <summary>
        /// 机构名
        /// </summary>
        public string Institutionname
        {
            get { return _institutionname; }
            set { _institutionname = value; }
        }

        /// <summary>
        /// 交易员id
        /// </summary>
        public string Traderid
        {
            get { return _traderid; }
            set { _traderid = value; }
        }

        /// <summary>
        /// 交易员名字
        /// </summary>
        public string Tradername
        {
            get { return _tradername; }
            set { _tradername = value; }
        }

        /// <summary>
        /// 对手机构全称
        /// </summary>
        public string Counterpartyfullname
        {
            get { return _counterpartyfullname; }
            set { _counterpartyfullname = value; }
        }

        /// <summary>
        /// 策略订单渠道
        /// </summary>
        public string Strategyordervenues
        {
            get { return _strategyordervenues; }
            set { _strategyordervenues = value; }
        }

        /// <summary>
        /// 对手渠道
        /// </summary>
        public string Counterpartyvenues
        {
            get { return _counterpartyvenues; }
            set { _counterpartyvenues = value; }
        }

        /// <summary>
        /// 内码
        /// </summary>
        public string Brokerexecid
        {
            get { return _brokerexecid; }
            set { _brokerexecid = value; }
        }

        /// <summary>
        /// 执行原因
        /// </summary>
        public string Exectypereason
        {
            get { return _exectypereason; }
            set { _exectypereason = value; }
        }

        /// <summary>
        /// 交易方法
        /// </summary>
        public string Trademethod
        {
            get { return _trademethod; }
            set { _trademethod = value; }
        }

        /// <summary>
        /// 删除标记
        /// </summary>
        public string Delflag
        {
            get { return _delflag; }
            set { _delflag = value; }
        }

        /// <summary>
        /// 接收时间
        /// </summary>
        public string Sendingtime
        {
            get { return _sendingtime; }
            set { _sendingtime = value; }
        }

        /// <summary>
        /// bid t+0收益率
        /// </summary>
        public string B0
        {
            get { return _b0; }
            set { _b0 = value; }
        }

        /// <summary>
        /// bid t+1收益率
        /// </summary>
        public string B1
        {
            get { return _b1; }
            set { _b1 = value; }
        }

        /// <summary>
        /// bid远期收益率
        /// </summary>
        public string B2
        {
            get { return _b2; }
            set { _b2 = value; }
        }

        /// <summary>
        /// 全市场bidt+1收益率
        /// </summary>
        public string M5b1
        {
            get { return _m5b1; }
            set { _m5b1 = value; }
        }

        /// <summary>
        /// ofrt+0收益率
        /// </summary>
        public string A0
        {
            get { return _a0; }
            set { _a0 = value; }
        }

        /// <summary>
        /// ofrt+1收益率
        /// </summary>
        public string A1
        {
            get { return _a1; }
            set { _a1 = value; }
        }

        /// <summary>
        /// ofr远期收益率
        /// </summary>
        public string A2
        {
            get { return _a2; }
            set { _a2 = value; }
        }

        /// <summary>
        /// 全市场ofrt+1收益率
        /// </summary>
        public string M5a1
        {
            get { return _m5a1; }
            set { _m5a1 = value; }
        }

        /// <summary>
        /// 是否为异常数据(Y-正常,N-异常)
        /// </summary>
        public string Vs
        {
            get { return _vs; }
            set { _vs = value; }
        }

        /// <summary>
        /// 组合id
        /// </summary>
        public string Bookid
        {
            get { return _bookid; }
            set { _bookid = value; }
        }

        /// <summary>
        /// qm 编号
        /// </summary>
        public string Qmnumber
        {
            get { return _qmnumber; }
            set { _qmnumber = value; }
        }

        /// <summary>
        /// sor通道放交易员qm帐号
        /// </summary>
        public string Partysubid2
        {
            get { return _partysubid2; }
            set { _partysubid2 = value; }
        }

        /// <summary>
        /// 意向交易编号
        /// </summary>
        public string Intentionid
        {
            get { return _intentionid; }
            set { _intentionid = value; }
        }

        /// <summary>
        /// 开平标志 0是开仓，1是平仓/平昨，2是平今
        /// </summary>
        public string Offsetflag
        {
            get { return _offsetflag; }
            set { _offsetflag = value; }
        }

        /// <summary>
        /// 投机套保 0-投机  1-套利  2-套保
        /// </summary>
        public string Hedgeflag
        {
            get { return _hedgeflag; }
            set { _hedgeflag = value; }
        }

        /// <summary>
        /// 价格类型
        /// </summary>
        public string Pricetype
        {
            get { return _pricetype; }
            set { _pricetype = value; }
        }

        /// <summary>
        /// 执行标志
        /// </summary>
        public string Exerciseflag
        {
            get { return _exerciseflag; }
            set { _exerciseflag = value; }
        }

        /// <summary>
        /// ss检查
        /// </summary>
        public string Ssdetect
        {
            get { return _ssdetect; }
            set { _ssdetect = value; }
        }

        /// <summary>
        /// 账户id
        /// </summary>
        public string Accountid
        {
            get { return _accountid; }
            set { _accountid = value; }
        }

        /// <summary>
        /// 账户名称
        /// </summary>
        public string Accountname
        {
            get { return _accountname; }
            set { _accountname = value; }
        }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag1
        {
            get { return _tag1; }
            set { _tag1 = value; }
        }

        /// <summary>
        /// 对手方账号ID
        /// </summary>
        public string Counteraccountid
        {
            get { return _counteraccountid; }
            set { _counteraccountid = value; }
        }

        /// <summary>
        /// 对手方账号名称
        /// </summary>
        public string Counteraccountname
        {
            get { return _counteraccountname; }
            set { _counteraccountname = value; }
        }

        /// <summary>
        /// 交易匹配状态:0-待匹配,1-已自动匹配,2-已匹配
        /// </summary>
        public string Matchstatus
        {
            get { return _matchstatus; }
            set { _matchstatus = value; }
        }

        /// <summary>
        /// 关联状态:0-未关联,1-已关联
        /// </summary>
        public string Relatestatus
        {
            get { return _relatestatus; }
            set { _relatestatus = value; }
        }

        /// <summary>
        /// 下发送状态:0-发送失败,1-发送成功
        /// </summary>
        public string Sendstatus
        {
            get { return _sendstatus; }
            set { _sendstatus = value; }
        }

        /// <summary>
        /// 报价方式:0-待定/1-发请求/2-收请求/3-发对话/4-收对话
        /// </summary>
        public string Quotemethod
        {
            get { return _quotemethod; }
            set { _quotemethod = value; }
        }

        /// <summary>
        /// 录入方式 0-自动录入 1-手工录入
        /// </summary>
        public string Entrytype
        {
            get { return _entrytype; }
            set { _entrytype = value; }
        }

        public string Seatnumber
        {
            get { return _seatnumber; }
            set { _seatnumber = value; }
        }

        public string Approvalstatus
        {
            get { return _approvalstatus; }
            set { _approvalstatus = value; }
        }

        public int MaturityDays
        {
            get { return _maturityDays; }
            set { _maturityDays = value; }
        }

        public string IssuerCode
        {
            get { return _issuerCode; }
            set { _issuerCode = value; }
        }
        public string IssuerName
        {
            get { return _issuerName; }
            set { _issuerName = value; }
        }
        public string WarranterCode
        {
            get { return __warranterCode; }
            set { __warranterCode = value; }
        }
        public string WarranterName
        {
            get { return __warranterName; }
            set { __warranterName = value; }
        }
        public string dayOffStr;

        private string _Approvalorderid;
        public string Approvalorderid
        {
            get { return _Approvalorderid; }
            set { _Approvalorderid = value; }
        }
        private string _Approvaltime;
        public string Approvaltime
        {
            get { return _Approvaltime; }
            set { _Approvaltime = value; }
        }
        private string _Approvalterminal;
        public string Approvalterminal
        {
            get { return _Approvalterminal; }
            set { _Approvalterminal = value; }
        }
        private string _BusinessType;
        public string BusinessType
        {
            get { return _BusinessType; }
            set { _BusinessType = value; }
        }

        private string _bookname;
        /// <summary>
        /// 组合名称
        /// </summary>
        public string BookName
        {
            get { return _bookname; }
            set { _bookname = value; }
        }
        public string TrdMatchId
        {
            get
            {
                return _trdMatchId;
            }
            set
            {
                _trdMatchId = value;
            }
        }
        public decimal? CleanPrice2
        {
            get
            {
                return _cleanPrice2;
            }
            set
            {
                _cleanPrice2 = value;
            }
        }
        public decimal? FullPrice2
        {
            get
            {
                return _fullPrice2;
            }
            set
            {
                _fullPrice2 = value;
            }
        }
        public decimal? Yield2
        {
            get
            {
                return _yield2;
            }

            set
            {
                _yield2 = value;
            }
        }
        public string BridgePaymentFixedRate
        {
            get
            {
                return _bridgePaymentFixedRate;
            }

            set
            {
                _bridgePaymentFixedRate = value;
            }
        }

        private string _couponrate;
        public string Couponrate
        {
            get
            {
                return _couponrate;
            }

            set
            {
                _couponrate = value;
            }
        }
        public decimal? SettlAmount
        {
            get
            {
                return _settlAmount;
            }

            set
            {
                _settlAmount = value;
            }
        }
        public string TrdType
        {
            get
            {
                return _trdType;
            }

            set
            {
                _trdType = value;
            }
        }
        public string TrdSubType
        {
            get
            {
                return _trdSubType;
            }

            set
            {
                _trdSubType = value;
            }
        }
        public string Realcounterpartyname
        {
            get
            {
                return _realcounterpartyname;
            }

            set
            {
                _realcounterpartyname = value;
            }
        }
        public decimal? FullPrice
        {
            get
            {
                return _fullPrice;
            }

            set
            {
                _fullPrice = value;
            }
        }
        public decimal? Yieldtoexecution
        {
            get
            {
                return _yieldtoexecution;
            }

            set
            {
                _yieldtoexecution = value;
            }
        }
        /// <summary>
        /// 操作人
        /// </summary>
        public string _CloseBy;
        public string CloseBy
        {
            get
            {
                return _CloseBy;
            }

            set
            {
                _CloseBy = value;
            }
        }
        /// <summary>
        /// 操作类型
        /// </summary>
        public string _Operationtype;
        public string Operationtype
        {
            get
            {
                return _Operationtype;
            }

            set
            {
                _Operationtype = value;
            }
        }
        /// <summary>
        /// 费后行权收益率
        /// </summary>
        public decimal? _Yieldtoexecution2;
        public decimal? Yieldtoexecution2
        {
            get
            {
                return _Yieldtoexecution2;
            }
            set
            {
                _Yieldtoexecution2 = value;
            }
        }
        public string _id;
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public string _allocText;
        public string AllocText
        {
            get
            {
                return _allocText;
            }
            set
            {
                _allocText = value;
            }
        }
        private string _riskflag;//风控检查标记0不通过1通过
        public string Riskflag
        {
            get
            {
                return _riskflag;
            }
            set
            {
                _riskflag = value;
            }
        }
        private string _riskremark;//风控检查备注
        public string Riskremark
        {
            get
            {
                return _riskremark;
            }
            set
            {
                _riskremark = value;
            }
        }
        private string _partymanagername;//投资经理
        public string Partymanagername
        {
            get
            {
                return _partymanagername;
            }
            set
            {
                _partymanagername = value;
            }
        }
        private string _partymanagerid;//投资经理ID
        public string Partymanagerid
        {
            get
            {
                return _partymanagerid;
            }
            set
            {
                _partymanagerid = value;
            }
        }
        private string _Sseinstitutioncode;//上交所机构代码
        public string Sseinstitutioncode
        {
            get
            {
                return _Sseinstitutioncode;
            }
            set
            {
                _Sseinstitutioncode = value;
            }
        }
        private string _Szedealercode;//深交所-交易商代码
        public string Szedealercode
        {
            get
            {
                return _Szedealercode;
            }
            set
            {
                _Szedealercode = value;
            }
        }
        private string _Szetradingentitycode;//深交所-交易主体代码
        public string Szetradingentitycode
        {
            get
            {
                return _Szetradingentitycode;
            }
            set
            {
                _Szetradingentitycode = value;
            }
        }
        private string _Szetradercode;//深交所-交易员代码
        public string Szetradercode
        {
            get
            {
                return _Szetradercode;
            }
            set
            {
                _Szetradercode = value;
            }
        }
        private long? _Matchqty;
        /// <summary>
        /// 已匹配量
        /// </summary>
        public long? Matchqty
        {
            get { return _Matchqty; }
            set { _Matchqty = value; }
        }
        private string _Toqq;//QQ号
        public string Toqq
        {
            get
            {
                return _Toqq;
            }
            set
            {
                _Toqq = value;
            }
        }
        #endregion

    }
    public class ParserInf
    {
        private long _sign;
        private long _sign1;
        private long _sign2;
        private long _sign3;
        public long Sign
        {
            get { return _sign; }
            set { _sign = value; }
        }
        public long Sign1
        {
            get { return _sign1; }
            set { _sign1 = value; }
        }
        public long Sign2
        {
            get { return _sign2; }
            set { _sign2 = value; }
        }
        public long Sign3
        {
            get { return _sign3; }
            set { _sign3 = value; }
        }
        
    }
}
