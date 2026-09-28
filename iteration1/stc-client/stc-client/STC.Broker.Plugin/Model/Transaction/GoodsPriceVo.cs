using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace STC.Broker.Plugin.Model.Transaction
{
    /// <summary>
    /// 一个债券的报价，远期或者即期或者组合
    /// </summary>
    public class GoodsPriceVo
    {
        public bool isNew = false;
        public bool isFwd = false;
        /// <summary>
        /// 给远期实时数据标记源头方向
        /// </summary>
        public bool isBid = false;
        /// <summary>
        /// 债券代码
        /// </summary>
        public string goodscode;
        /// <summary>
        /// 次优卖价
        /// </summary>
        public decimal SuboptimalOfferPrice;
        /// <summary>
        /// 记住当前债券报价框中的最优卖价
        /// </summary>
        public decimal OptimalOfferPrice;
        /// <summary>
        /// 次优买价
        /// </summary>
        public decimal SuboptimalBidPrice;
        /// <summary>
        /// 记住当前债券报价框中的最优买价
        /// </summary>
        public decimal OptimalBidPrice;
        /// <summary>
        /// 某列的卖价
        /// </summary>
        public decimal OfferPrice;
        /// <summary>
        /// 某列的卖价链表
        /// </summary>
        public List<GoodsVolumeVo> Offer;
        /// <summary>
        /// 次优买报价列表
        /// </summary>
        public List<GoodsVolumeVo> SuboptimalOffer;
        /// <summary>
        /// 最优卖报价列表
        /// </summary>
        public List<GoodsVolumeVo> OptimalOffer;
        /// <summary>
        /// 次优买报价列表
        /// </summary>
        public List<GoodsVolumeVo> SuboptimalBid;
        /// <summary>
        /// 最优买报价列表
        /// </summary>
        public List<GoodsVolumeVo> OptimalBid;
        /// <summary>
        /// 结算日期
        /// </summary>
        public string timetomaturityreal;


        #region V_2.17

        /// <summary>
        /// 包含暗盘的最优买报价列表
        /// </summary>
        public List<GoodsVolumeVo> AllOptimalBid { get; set; } = new List<GoodsVolumeVo>();

        /// <summary>
        /// 包含暗盘最优卖报价列表
        /// </summary>
        public List<GoodsVolumeVo> AllOptimalOffer { get; set; } = new List<GoodsVolumeVo>();

        /// <summary>
        /// 明盘最优买报价列表
        /// </summary>
        public List<GoodsVolumeVo> OpenOptimalBid { get; set; } = new List<GoodsVolumeVo>();

        /// <summary>
        /// 明盘最优卖报价列表
        /// </summary>
        public List<GoodsVolumeVo> OpenOptimalOffer { get; set; } = new List<GoodsVolumeVo>();

        #endregion V_2.17


        #region 20211221  新增

        /// <summary>
        /// 即期结算为0的买方最优报价
        /// </summary>
        public List<GoodsVolumeVo> SpotZeroBid { get; set; }
        /// <summary>
        /// 即期结算为0的卖方最优报价
        /// </summary>
        public List<GoodsVolumeVo> SpotZeroOfr { get; set; }

        /// <summary>
        /// 即期结算不为0的买方最优报价
        /// </summary>
        public List<GoodsVolumeVo> SpotFirstBid { get; set; }
        /// <summary>
        /// 即期结算不为0的卖方最优报价
        /// </summary>
        public List<GoodsVolumeVo> SpotFirstOfr { get; set; }
        /// <summary>
        /// 即期结算不为0的买方次优报价
        /// </summary>
        public List<GoodsVolumeVo> SpotSecondBid { get; set; }
        /// <summary>
        /// 即期结算不为0的卖方次优报价
        /// </summary>
        public List<GoodsVolumeVo> SpotSecondOfr { get; set; }

        /// <summary>
        /// 即期结算不为0的买方次优报价
        /// </summary>
        public decimal SpotFirstBidPrice { get; set; }
        /// <summary>
        /// 即期结算不为0的买方最优报价
        /// </summary>
        public decimal SpotSecondBidPrice { get; set; }

        /// <summary>
        /// 即期结算不0的买方最优报价
        /// </summary>
        public decimal SpotZeroBidPrice { get; set; }

        /// <summary>
        /// 即期结算不为0的卖方次优报价
        /// </summary>
        public decimal SpotFirstOfrPrice { get; set; }
        /// <summary>
        /// 即期结算不为0的卖方最优报价
        /// </summary>
        public decimal SpotSecondOfrPrice { get; set; }

        /// <summary>
        /// 即期结算不0的卖方最优报价
        /// </summary>
        public decimal SpotZeroOfrPrice { get; set; }

        #endregion


        #region  全部报价列表
        /// <summary>
        /// 即期买报价列表
        /// </summary>
        public List<GoodsVolumeVo> AtSightBidPriceList;
        /// <summary>
        /// 即期卖报价列表
        /// </summary>
        public List<GoodsVolumeVo> AtSightOfferPriceList;
        #endregion


    }
}
