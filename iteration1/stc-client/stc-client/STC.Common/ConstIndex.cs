using STC.Common.Model.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common
{
    public class ConstIndex
    {
        /// <summary>
        /// 债券id 索引  位数长度-位数值-债券
        /// </summary>
        public static Dictionary<string, Dictionary<string, BondsInfoPo>> BondIndex { get; } = new  Dictionary<string, Dictionary<string, BondsInfoPo>>();


        /// <summary>
        /// condition - bondid- bond
        /// </summary>
        public static Dictionary<string, Dictionary<string, BondsInfoPo>> BondIndexThree { get; } = new Dictionary<string, Dictionary<string,BondsInfoPo>>();

        /// <summary>
        /// 清空债券索引
        /// </summary>
        public static void ClearBondIndex()
        {
            foreach (var item in BondIndex)
            { 
                item.Value.Clear();
            }
            BondIndexThree.Clear();
        }

        /// <summary>
        /// 根据bondid 移除
        /// </summary>
        /// <param name="bondId"></param>
        public static void RemoveBondById(string bondId)
        {
            foreach (var pair in BondIndex)
            {
                pair.Value.Remove(bondId);
            }

            foreach (var pair in BondIndexThree)
            {
                pair.Value.Remove(bondId);
            }
        }

    }
}
