using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{

    /// <summary>
    /// pt订单表
    /// </summary>
    /// <remarks>
    /// 2023.08.25: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
    /// </remarks>
    [Serializable]
    public class TbookPtOrders
    {
        #region 字段

        private string _orderid = "";//ems订单编号
        private string _strategyorderid = "";//策略订单编号
        private string _strategyordervenues;//策略订单渠道
        private string _clordid;//客户订单编号
        private string _venueid;//交易执行渠道生成的订单id
        private string _quoteid;//报价编号
        private string _mdreqid;//报价请求编号
        private string _venuetypegw = "";//交易平台
        private string _venues;//交易执行渠道
        private string _securityid;//产品唯一编号
        private string _symbol;//产品名称
        private string _symboltype;//产品类型
        private string _marketindicator;//市场
        private string _partysubid;//本交易员ID
        private string _username;//stc用户
        private string _rule;//交易规则
        private string _orderstatus;//订单状态
        private string _side;//买卖方向
        private decimal? _price;//下单价格
        private decimal? _orderqty;//下单量
        private string _ordtype;//订单类型
        private string _settltype;//清算类型
        private string _currency;//交易货币
        private string _tradedate;//交易日期
        private string _validuntitime;//订单有效期
        private string _timeinforce;//订单执行方式
        private string _terminal;//终端
        private string _rejecter;//拒绝者
        private string _rejectercode;//拒绝代码
        private string _rejecterreason;//拒绝原因
        private decimal? _openqty;//已经开始执行的交易量
        private decimal? _cancelledqty;//已经撤销的交易量
        private decimal? _executionqty;//已经成交的交易量
        private decimal? _matchqty;//匹配交易量
        private string _submitclientqty;//提交时的交易量
        private string _ip;//服务器ip
        private string _execflag;//是否有执行订单标记
        private string _remark;//备注
        private string _createtime;//创建时间
        private string _updatetime;//更新时间
        private string _inf1;//扩展字段1
        private string _inf2;//扩展字段2
        private string _inf3;//扩展字段3
        private string _inf4;//扩展字段4
        private string _inf5;//扩展字段5
        private string _inf6;//扩展字段6
        private decimal? _avgprice;//平均价
        private string _quotetype;//报价类型
        private string _quotetranstype;//操作类别
        private string _quotestatusdesc;//报价状态说明
        private decimal? _yield2;//收益率2
        private decimal? _price2;//下单价格2
        private decimal? _orderqty2;//下单量2
        private string _clearingmethod;//清算类型
        private string _deliverytype;//结算方式
        private string _partysubname;//本交易员Name
        private string _partycode;//机构21位码
        private string _counterpartyid;//对手方机构ID
        private string _counterpartyname;//对手方机构Name
        private string _counterpartysubid;//对手方交易员ID
        private string _counterpartysubname;//对手机构名
        private string _counterpartycode;//对手方机构21位码
        private decimal? _yield;//收益率
        private decimal? _netprice;//净价
        private decimal? _netprice2;//净价2
        private string _partyname;//本方机构名称
        private string _partyid;//本方机构id
        private string _userid;//用户id
        private string _firmflag;//firm标记
        private string _wholeflag;//整量
        private string _name;//名字
        private string _quantid;//量化编号
        private int? _blocknum;//交易包号
        private int? _sn;//顺序号
        private string _ocoflag;//oco标记
        private string _institutionid;//客盘机构id
        private string _institutionname;//客盘机构名字
        private string _traderid;//客盘交易员id
        private string _tradername;//客盘交易员名字

        private string _biztype;//book部门
        private string _biztypeid;//book部门ID
        private string _showtrader;//客盘保密标记
        private string _roleid;//角色id
        private string _rolename;//角色名字
        private string _salesagentflag;//销售标记
        private string _origordprice;//订单原始报价
        private string _counterpartysalesagentflag;//对手代客标记
        private string _exectype;//执行类型
        private string _ioctimeout;//ioc 超时
        private string _handlinst;//暗盘标记 1暗盘，2明盘
        private string _exectypereason;//交易方法
        private string _trademethod;//已经撤销的交易量
        private decimal? _cancelledqty2;//已经撤销的交易量2
        private decimal? _executionqty2;//已经成交的交易量2
        private decimal? _avgprice2;//平均价2
        private string _orderstatus2;//订单状态2
        private string _exectype2;//执行类型2
        private string _bookid;//组合id
        private string _maxfloor;//冰山量
        private string _qmnumber;//qm 编号
        private string _partysubid2;//sor通道放交易员qm帐号
        private string _intentionid;//意向交易编号
        private string _offsetflag;//开平标志 0是开仓，1是平仓/平昨，2是平今
        private string _hedgeflag;//投机套保 0-投机  1-套利  2-套保
        private string _accountid;//账户id
        private string _accountname;//账户名称
        private string _externalorderid;//外部报价编号

        #endregion

        #region 属性

        /// <summary>
        /// ems订单编号
        /// </summary>
        public string Orderid
        {
            get { return _orderid; }
            set { _orderid = value; }
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
        /// 策略订单渠道
        /// </summary>
        public string Strategyordervenues
        {
            get { return _strategyordervenues; }
            set { _strategyordervenues = value; }
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
        /// 交易执行渠道生成的订单id
        /// </summary>
        public string Venueid
        {
            get { return _venueid; }
            set { _venueid = value; }
        }

        /// <summary>
        /// 报价编号
        /// </summary>
        public string Quoteid
        {
            get { return _quoteid; }
            set { _quoteid = value; }
        }

        /// <summary>
        /// 报价请求编号
        /// </summary>
        public string Mdreqid
        {
            get { return _mdreqid; }
            set { _mdreqid = value; }
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
        /// 本交易员ID
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
        /// 交易规则
        /// </summary>
        public string Rule
        {
            get { return _rule; }
            set { _rule = value; }
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string Orderstatus
        {
            get { return _orderstatus; }
            set { _orderstatus = value; }
        }

        /// <summary>
        /// 买卖方向
        /// </summary>
        public string Side
        {
            get { return _side; }
            set { _side = value; }
        }

        /// <summary>
        /// 下单价格
        /// </summary>
        public decimal? Price
        {
            get { return _price; }
            set { _price = value; }
        }

        /// <summary>
        /// 下单量
        /// </summary>
        public decimal? Orderqty
        {
            get { return _orderqty; }
            set { _orderqty = value; }
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
        /// 清算类型
        /// </summary>
        public string Settltype
        {
            get { return _settltype; }
            set { _settltype = value; }
        }

        /// <summary>
        /// 交易货币
        /// </summary>
        public string Currency
        {
            get { return _currency; }
            set { _currency = value; }
        }

        /// <summary>
        /// 交易日期
        /// </summary>
        public string Tradedate
        {
            get { return _tradedate; }
            set { _tradedate = value; }
        }

        /// <summary>
        /// 订单有效期
        /// </summary>
        public string Validuntitime
        {
            get { return _validuntitime; }
            set { _validuntitime = value; }
        }

        /// <summary>
        /// 订单执行方式
        /// </summary>
        public string Timeinforce
        {
            get { return _timeinforce; }
            set { _timeinforce = value; }
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
        /// 已经开始执行的交易量
        /// </summary>
        public decimal? Openqty
        {
            get { return _openqty; }
            set { _openqty = value; }
        }

        /// <summary>
        /// 已经撤销的交易量
        /// </summary>
        public decimal? Cancelledqty
        {
            get { return _cancelledqty; }
            set { _cancelledqty = value; }
        }

        /// <summary>
        /// 已经成交的交易量
        /// </summary>
        public decimal? Executionqty
        {
            get { return _executionqty; }
            set { _executionqty = value; }
        }

        /// <summary>
        /// 匹配交易量
        /// </summary>
        public decimal? Matchqty
        {
            get { return _matchqty; }
            set { _matchqty = value; }
        }

        /// <summary>
        /// 提交时的交易量
        /// </summary>
        public string Submitclientqty
        {
            get { return _submitclientqty; }
            set { _submitclientqty = value; }
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
        /// 是否有执行订单标记
        /// </summary>
        public string Execflag
        {
            get { return _execflag; }
            set { _execflag = value; }
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
        /// 平均价
        /// </summary>
        public decimal? Avgprice
        {
            get { return _avgprice; }
            set { _avgprice = value; }
        }

        /// <summary>
        /// 报价类型
        /// </summary>
        public string Quotetype
        {
            get { return _quotetype; }
            set { _quotetype = value; }
        }

        /// <summary>
        /// 操作类别
        /// </summary>
        public string Quotetranstype
        {
            get { return _quotetranstype; }
            set { _quotetranstype = value; }
        }

        /// <summary>
        /// 报价状态说明
        /// </summary>
        public string Quotestatusdesc
        {
            get { return _quotestatusdesc; }
            set { _quotestatusdesc = value; }
        }

        /// <summary>
        /// 收益率2
        /// </summary>
        public decimal? Yield2
        {
            get { return _yield2; }
            set { _yield2 = value; }
        }

        /// <summary>
        /// 下单价格2
        /// </summary>
        public decimal? Price2
        {
            get { return _price2; }
            set { _price2 = value; }
        }

        /// <summary>
        /// 下单量2
        /// </summary>
        public decimal? Orderqty2
        {
            get { return _orderqty2; }
            set { _orderqty2 = value; }
        }

        /// <summary>
        /// 清算类型
        /// </summary>
        public string Clearingmethod
        {
            get { return _clearingmethod; }
            set { _clearingmethod = value; }
        }

        /// <summary>
        /// 结算方式
        /// </summary>
        public string Deliverytype
        {
            get { return _deliverytype; }
            set { _deliverytype = value; }
        }

        /// <summary>
        /// 本交易员Name
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
        /// 对手方机构ID
        /// </summary>
        public string Counterpartyid
        {
            get { return _counterpartyid; }
            set { _counterpartyid = value; }
        }

        /// <summary>
        /// 对手方机构Name
        /// </summary>
        public string Counterpartyname
        {
            get { return _counterpartyname; }
            set { _counterpartyname = value; }
        }

        /// <summary>
        /// 对手方交易员ID
        /// </summary>
        public string Counterpartysubid
        {
            get { return _counterpartysubid; }
            set { _counterpartysubid = value; }
        }

        /// <summary>
        /// 对手机构名
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
        /// 收益率
        /// </summary>
        public decimal? Yield
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
        /// 净价2
        /// </summary>
        public decimal? Netprice2
        {
            get { return _netprice2; }
            set { _netprice2 = value; }
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
        /// 交易包号
        /// </summary>
        public int? Blocknum
        {
            get { return _blocknum; }
            set { _blocknum = value; }
        }

        /// <summary>
        /// 顺序号
        /// </summary>
        public int? Sn
        {
            get { return _sn; }
            set { _sn = value; }
        }

        /// <summary>
        /// oco标记
        /// </summary>
        public string Ocoflag
        {
            get { return _ocoflag; }
            set { _ocoflag = value; }
        }

        /// <summary>
        /// 客盘机构id
        /// </summary>
        public string Institutionid
        {
            get { return _institutionid; }
            set { _institutionid = value; }
        }

        /// <summary>
        /// 客盘机构名字
        /// </summary>
        public string Institutionname
        {
            get { return _institutionname; }
            set { _institutionname = value; }
        }

        /// <summary>
        /// 客盘交易员id
        /// </summary>
        public string Traderid
        {
            get { return _traderid; }
            set { _traderid = value; }
        }

        /// <summary>
        /// 客盘交易员名字

        /// </summary>
        public string Tradername
        {
            get { return _tradername; }
            set { _tradername = value; }
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
        /// 执行类型
        /// </summary>
        public string Exectype
        {
            get { return _exectype; }
            set { _exectype = value; }
        }

        /// <summary>
        /// ioc 超时
        /// </summary>
        public string Ioctimeout
        {
            get { return _ioctimeout; }
            set { _ioctimeout = value; }
        }

        /// <summary>
        /// 暗盘标记 1暗盘，2明盘
        /// </summary>
        public string Handlinst
        {
            get { return _handlinst; }
            set { _handlinst = value; }
        }

        /// <summary>
        /// 交易方法
        /// </summary>
        public string Exectypereason
        {
            get { return _exectypereason; }
            set { _exectypereason = value; }
        }

        /// <summary>
        /// 已经撤销的交易量
        /// </summary>
        public string Trademethod
        {
            get { return _trademethod; }
            set { _trademethod = value; }
        }

        /// <summary>
        /// 已经撤销的交易量2
        /// </summary>
        public decimal? Cancelledqty2
        {
            get { return _cancelledqty2; }
            set { _cancelledqty2 = value; }
        }

        /// <summary>
        /// 已经成交的交易量2
        /// </summary>
        public decimal? Executionqty2
        {
            get { return _executionqty2; }
            set { _executionqty2 = value; }
        }

        /// <summary>
        /// 平均价2
        /// </summary>
        public decimal? Avgprice2
        {
            get { return _avgprice2; }
            set { _avgprice2 = value; }
        }

        /// <summary>
        /// 订单状态2
        /// </summary>
        public string Orderstatus2
        {
            get { return _orderstatus2; }
            set { _orderstatus2 = value; }
        }

        /// <summary>
        /// 执行类型2
        /// </summary>
        public string Exectype2
        {
            get { return _exectype2; }
            set { _exectype2 = value; }
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
        /// 冰山量
        /// </summary>
        public string Maxfloor
        {
            get { return _maxfloor; }
            set { _maxfloor = value; }
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
        /// 外部报价编号
        /// </summary>
        public string Externalorderid
        {
            get { return _externalorderid; }
            set { _externalorderid = value; }
        }


        #endregion

    }
}
