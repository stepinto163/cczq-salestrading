using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    /// <summary>
    ///  持仓明细
    /// </summary>
    public class PositionDetailModel
    {
        /// <summary>
        /// 组合ID
        /// </summary>
        public string bookid { get; set; }

        /// <summary>
        /// 组合名称
        /// </summary>
        public string bookname { get; set; }

        /// <summary>
        /// key   BookId + "#" + Securityid
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        public string securityid { get; set; }
        /// <summary>
        /// 简称
        /// </summary>
        public string symbol { get; set; }


        /// <summary>
        /// 债券类型
        /// </summary>
        public string symboltype { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string marketindicator { get; set; }


        /// <summary>
        /// 均价(净)
        /// </summary>
        public decimal netavgprice { get; set; }

        /// <summary>
        /// 均价(净)
        /// </summary>
        public decimal netavgyield { get; set; }

        /// <summary>
        /// 净价来源 净价来源 1-中债  2-成交
        /// </summary>
        public string netpricesource { get; set; }

        /// <summary>
        /// 多头持仓
        /// </summary>
        public decimal longposition { get; set; }


        /// <summary>
        /// 多头总价
        /// </summary>
        public decimal longpriceTotal { get; set; }

        /// <summary>
        /// 多头均价
        /// </summary>
        public decimal longavgprice { get; set; }

        /// <summary>
        /// 空头持仓
        /// </summary>
        public decimal shortposition { get; set; }

        /// <summary>
        /// 空头总价
        /// </summary>
        public decimal shortpricetotal { get; set; }

        /// <summary>
        /// 空头均价
        /// </summary>
        public decimal shortavgprice { get; set; }

        /// <summary>
        /// 持仓
        /// </summary>
        public decimal netposition { get; set; }

        /// <summary>
        /// 总盈亏（结转计算）
        /// </summary>
        public decimal totalprofitandloss { get; set; }

        /// <summary>
        /// 平仓盈亏（结转计算）
        /// </summary>
        public decimal closingprofitandloss { get; set; }

        /// <summary>
        /// 持仓盈亏（结转计算）
        /// </summary>
        public decimal positionprofitandloss { get; set; }


        public string createtime { get; set; }


        public string updatetime { get; set; }

        /// <summary>
        /// 今日持仓（多）
        /// </summary>
        public decimal todaylongposition { get; set; }
        /// <summary>
        /// 今日持仓（空）
        /// </summary>
        public decimal todayshortposition { get; set; }
        /// <summary>
        /// 昨结价
        /// </summary>
        public decimal ytdsettlementprice { get; set; }

        /// <summary>
        /// 净价 
        /// </summary>
        public decimal netprice { get; set; }

        /// <summary>
        /// 收益率
        /// </summary>
        public decimal yield { get; set; }

        /// <summary>
        /// 持仓市值
        /// </summary>
        public decimal positionmarketvalue { get; set; }



        #region 新增字段
        /// <summary>
        /// dv01 
        /// </summary>
        public decimal dv01 { get; set; }
        /// <summary>
        /// 今日净持仓 
        /// </summary>
        public decimal  dailyNetPosition { get; set; }

        /// <summary>
        /// 日初头寸,日终写入 initialPosition = initialPosition + dailyNetPosition
        /// </summary>
        public decimal initialPosition { get; set; }

        /// <summary>
        /// 起息日 = 交割日
        /// </summary>
        public string settlDate { get; set; }

        /// <summary>
        /// 清算速度
        /// </summary>
        public string settlType { get; set; }

        /// <summary>
        /// 可用持仓
        /// </summary>
        public decimal availablePosition { get; set; }

        /// <summary>
        /// 累计可用持仓
        /// </summary>
        public decimal cumAvaliablePosition { get; set; }
        
        /// <summary>
        /// 冻结额度
        /// </summary>
        public decimal frozenAmount { get; set; }

        /// <summary>
        /// 删除标记  "1" 删除
        /// </summary>
        public string delflag { get; set; }

        /// <summary>
        /// 持仓日期
        /// </summary>
        public string traderdate { get; set; }
        #endregion
    }
}
