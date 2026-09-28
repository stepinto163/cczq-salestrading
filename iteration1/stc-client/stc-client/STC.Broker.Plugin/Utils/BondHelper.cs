using STC.Common.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public static class BondHelper
    {
        /// <summary>
        /// 指定债券代码的债券是否是地方债
        /// </summary>
        /// <param name="bondCode"></param>
        /// <returns></returns>
        public static bool IsLGBBond(string bondCode)
        {
            var result = false;
            BondsInfoPo bond =Utils.CommonUtils.GetBondsInfoByCode(bondCode);
            //CRMDataHandler.Instance.GetBondsInfoByCode(bondCode);
            if (bond != null)
            {
                result = Constant.LocalBondFlags.Contains(bond.Bond_Subtype.ToUpper());//BondSubtype
            }
            return result;
        }
    }
}
