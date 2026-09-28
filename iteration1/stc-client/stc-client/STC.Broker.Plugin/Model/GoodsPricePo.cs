using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    /// <summary>
    /// 一个债券的报价，包括两个方向的远期和即期，以及组合的报价列表
    /// </summary>
    public class GoodsPricePo
    {

        /// <summary>
        /// 债券是否净价
        /// </summary>
        public bool isCleanPrice;
        /// <summary>
        /// 债券代码
        /// </summary>
        public string goodscode;
        /// <summary>
        /// 买价列表
        /// </summary>
        public List<GoodsVolumePo> bid;
        /// <summary>
        /// 卖价列表
        /// </summary>
        public List<GoodsVolumePo> ofr;
        /// <summary>
        /// 剩余期限
        /// </summary>
        public string timetomaturityreal;
        /// <summary>
        /// 远期买价列表
        /// </summary>
        public List<GoodsVolumePo> bidFwd;
        /// <summary>
        /// 远期卖价列表
        /// </summary>
        public List<GoodsVolumePo> ofrFwd;
        /// <summary>
        /// 即期买报价列表
        /// </summary>
        public List<GoodsVolumePo> bidSpot;
        /// <summary>
        /// 即期卖报价列表
        /// </summary>
        public List<GoodsVolumePo> ofrSpot;

        /// <summary>
        /// 按照结算区分报价，20220209新增
        /// </summary>
        public Dictionary<string, GoodsPricePo> spotHashMap { get; set; }



        /// <summary>
        /// 即期结算为0的买方最优报价
        /// </summary>
        public List<GoodsVolumePo> SpotZeroBid
        {
            get
            {
                List<GoodsVolumePo> goodsVolumes = new List<GoodsVolumePo>();
                if (spotHashMap != null && spotHashMap.Count > 0)
                {
                    GoodsPricePo pricePo;
                    if (spotHashMap.TryGetValue("+0", out pricePo))
                    {
                        if (pricePo != null)
                        {
                            goodsVolumes = pricePo.bid;
                        }
                    }
                    goodsVolumes.Sort();
                }
                return goodsVolumes;
            }
        }
        /// <summary>
        /// 即期结算为0的卖方最优报价
        /// </summary>
        public List<GoodsVolumePo> SpotZeroOfr
        {
            get
            {
                List<GoodsVolumePo> goodsVolumes = new List<GoodsVolumePo>();
                if (spotHashMap != null && spotHashMap.Count > 0)
                {
                    GoodsPricePo pricePo;
                    if (spotHashMap.TryGetValue("+0", out pricePo))
                    {
                        if (pricePo != null)
                        {
                            goodsVolumes = pricePo.ofr;
                        }
                    }
                    goodsVolumes.Sort();
                }
                return goodsVolumes;
            }
        }

        /// <summary>
        /// 即期结算不为0的买方最优报价
        /// </summary>
        public List<GoodsVolumePo> SpotFirstBid
        {
            get
            {
                List<GoodsVolumePo> goodsVolumes = new List<GoodsVolumePo>();
                if (bidSpot != null && bidSpot.Count > 0)
                {
                    goodsVolumes = bidSpot.Where(_ => _.SettlementDisplay != "+0").ToList();
                }
                return goodsVolumes;
            }
        }
        /// <summary>
        /// 即期结算不为0的卖方最优报价
        /// </summary>
        public List<GoodsVolumePo> SpotFirstOfr
        {
            get
            {
                List<GoodsVolumePo> goodsVolumes = new List<GoodsVolumePo>();
                if (ofrSpot != null && ofrSpot.Count > 0)
                {
                    goodsVolumes = ofrSpot.Where(_ => _.SettlementDisplay != "+0").ToList();
                }
                return goodsVolumes;
            }
        }
        /// <summary>
        /// 即期结算全部报价
        /// </summary>
        public List<GoodsVolumePo> SpotAllBid
        {

            get
            {
                List<GoodsVolumePo> goodsVolumes = new List<GoodsVolumePo>();
                if (spotHashMap != null && spotHashMap.Count > 0)
                {

                    GoodsPricePo pricePo2;
                    if (spotHashMap.TryGetValue("+1,明天+0", out pricePo2))
                    {
                        if (pricePo2 != null)
                        {
                            if (pricePo2.bid != null && pricePo2.bid.Count > 0)
                            {
                                pricePo2.bid.Sort();
                                foreach (GoodsVolumePo item in pricePo2.bid)
                                {
                                    if (!goodsVolumes.Any(_ => _.id == item.id))
                                    {
                                        goodsVolumes.Add(item);
                                    }
                                }
                            }
                        }
                    }

                    GoodsPricePo pricePo;
                    if (spotHashMap.TryGetValue("+0", out pricePo))
                    {
                        if (pricePo != null)
                        {
                            if (pricePo.bid != null && pricePo.bid.Count > 0)
                            {
                                pricePo.bid.Sort();
                                foreach (GoodsVolumePo item in pricePo.bid)
                                {
                                    if (!goodsVolumes.Any(_ => _.id == item.id))
                                    {
                                        goodsVolumes.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }

                if (isCleanPrice)
                {
                    return goodsVolumes.OrderByDescending(_ => _.price).ToList();
                }
                else
                {
                    return goodsVolumes.OrderBy(_ => _.price).ToList();
                }

            }
        }
        /// <summary>
        /// 即期结算全部卖方报价
        /// </summary>
        public List<GoodsVolumePo> SpotAllOfr
        {
            get
            {
                List<GoodsVolumePo> goodsVolumes = new List<GoodsVolumePo>();
                if (spotHashMap != null && spotHashMap.Count > 0)
                {
                    GoodsPricePo pricePo2;
                    if (spotHashMap.TryGetValue("+1,明天+0", out pricePo2))
                    {
                        if (pricePo2 != null)
                        {
                            if (pricePo2.ofr != null && pricePo2.ofr.Count > 0)
                            {
                                pricePo2.ofr.Sort();
                                foreach (GoodsVolumePo item in pricePo2.ofr)
                                {
                                    if (!goodsVolumes.Any(_ => _.id == item.id))
                                    {
                                        goodsVolumes.Add(item);
                                    }
                                }
                            }
                        }
                    }

                    GoodsPricePo pricePo;
                    if (spotHashMap.TryGetValue("+0", out pricePo))
                    {
                        if (pricePo != null)
                        {
                            pricePo.ofr.Sort();
                            if (pricePo.ofr != null && pricePo.ofr.Count > 0)
                            {
                                foreach (GoodsVolumePo item in pricePo.ofr)
                                {
                                    if (!goodsVolumes.Any(_ => _.id == item.id))
                                    {
                                        goodsVolumes.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
                if (isCleanPrice)
                {
                    return goodsVolumes.OrderBy(_ => _.price).ToList();
                }
                else
                {
                    return goodsVolumes.OrderByDescending(_ => _.price).ToList();
                }

            }
        }
    }
}
