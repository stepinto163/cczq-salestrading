using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
{
    public class PermissionHandler
    {
        private static PermissionHandler _permissionHandler = new PermissionHandler();
        public static PermissionHandler Instance { get { return _permissionHandler; } }

        private Dictionary<string, string> _secutityDic = new Dictionary<string, string>();
        private object _lockObj = new object();
        private List<string> IsInner = new List<string>() { "IDB", "QUANT" };
        private PermissionHandler()
        {

        }
        
        public bool CheckVenuePermission(string venue, string security)
        {
            //暂时不检查期货渠道权限 需要产品确定后面期货交易所的渠道权限管理怎么做 - zhongliang，songzhi
            if (Consts.CFFEX.Equals(venue)
                || Consts.SHFE.Equals(venue)
                || Consts.CZCE.Equals(venue)
                || Consts.DCE.Equals(venue)
                || Consts.INE.Equals(venue)
                || Consts.CTP.Equals(venue))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(venue) && IsInner.Contains(venue.ToUpper()))
                return false;

            bool result = false;
            lock (_lockObj)
            {
                string goodsType = string.Empty;

                if (_secutityDic.ContainsKey(security))
                {
                    goodsType = _secutityDic[security];
                    if (string.IsNullOrEmpty(goodsType))
                    {
                        goodsType = CommonUtils.SetGoodsShortname(security);

                        _secutityDic.Remove(security);
                        _secutityDic.Add(security, goodsType);
                    }
                }
                else
                {
                    goodsType = CommonUtils.SetGoodsShortname(security);

                    _secutityDic.Add(security, goodsType);
                }

                List<Venue> venues = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, string.Empty);


                if (venues != null)
                {
                    foreach (Venue ve in venues)
                    {
                        if (ve.venue.ToUpper() == venue.ToUpper())
                        {
                            result = true;

                            break;
                        }
                    }
                }
            }
            return result;
        }

    }
}
