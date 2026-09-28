using System;

namespace STC.Common.Model.PP
{
    /// <summary>
    /// 债券持仓
    /// </summary>
    public class StcBondPositionModel
    {
        /// <summary>
        /// 组合代码
        /// </summary>
        public string portfolioNo{get;set;}
        
        /// <summary>
        /// 组合名称
        /// </summary>
        public string portfolioName{get;set;}
        
        /// <summary>
        /// 交易账户代码
        /// </summary>
        public string tradeAccount{get;set;}
        
        /// <summary>
        /// 交易账户名称
        /// </summary>
        public string tradeName{get;set;}

        /// <summary>
        /// 投资经理
        /// </summary>
        public string fundAccount{get;set;}

        /// <summary>
        /// 剩余期限
        /// </summary>
        public string remaingingTime{get;set;}

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityId{get;set;}

        /// <summary>
        /// 债券名称
        /// </summary>
        public string symbolName{get;set;}

        /// <summary>
        /// 发行人
        /// </summary>
        public string issuer{get;set;}

        /// <summary>
        /// 可用持仓量(万)
        /// </summary>
        public decimal availablePosition { get;set;}
        
        /// <summary>
        /// 资金询价占用量
        /// </summary>
        public decimal usedQty{get;set;}

        /// <summary>
        /// STC在途卖量(万)
        /// </summary>
        public decimal stcInSalesVolume{get;set;}

        /// <summary>
        /// stc可用持仓量(万)
        /// </summary>
        public decimal stcAvailablePosition { get;set;}
        
        /// <summary>
        /// STC在途买量(万)
        /// </summary>
        public decimal stcInBuyVolume{get;set;}

        /// <summary>
        /// 主体额度(万)
        /// </summary>
        public decimal quotalimit{get;set;}

        /// <summary>
        /// 单券余额(万)
        /// </summary>
        public decimal outstandingAmount{ get;set;}
        
        /// <summary>
        /// 可投比例(%)
        /// </summary>
        public string investmentratio{get;set;}

        /// <summary>
        /// 内部主体评级
        /// </summary>
        public string rate{get;set;}

        /// <summary>
        /// 内部债券评级
        /// </summary>
        public string bonddrating{get;set;}

        /// <summary>
        /// 成本净价（元）
        /// </summary>
        public decimal netprice{get;set;}

        /// <summary>
        /// 中债估值（元）
        /// </summary>
        public string valuationYield {get;set;}

        /// <summary>
        /// 中债估值（%）
        /// </summary>
        public string valuationYield2 {get;set;}

        /// <summary>
        /// 中证估值（%）
        /// </summary>
        public string csiBondValuation {get;set;}

        /// <summary>
        /// 交易所收盘价（元）
        /// </summary>
        public string exClose{get;set;}

        /// <summary>
        /// 交易所收盘价（%）
        /// </summary>
        public string exClose2 {get;set;}

        //optim总持仓量(万)
        public decimal totalQty { get; set; }

        //stc可用额度(万)
        public decimal stcAvailableLimit { get; set; }

        //单券剩余额度
        public decimal singleBondAmount{ get; set; }

        //内部主体评级到期日
        public string rateenddate { get; set; }

        //恒生基金编号
        public string hsProductId{ get; set; }

        //内部债券评级到期日
        public string ratingExpirydate { get; set; }

        //募集类型
        public string issueType { get; set; }

        //担保人
        public string guarantor{ get; set; }

        //担保人内部评级
        public string guarantorRating{ get; set; }

    }
}
