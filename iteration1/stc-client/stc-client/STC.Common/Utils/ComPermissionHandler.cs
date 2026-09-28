using com.gateway.log;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class ComPermissionHandler
    {
        private static ComPermissionHandler _permissionHandler = new ComPermissionHandler();
        public static ComPermissionHandler Instance { get { return _permissionHandler; } }

        private Dictionary<string, string> _secutityDic = new Dictionary<string, string>();
        private object _lockObj = new object();
        private List<string> IsInner = new List<string>() { "IDB", "QUANT" };
        private Dictionary<string, bool> DictPermission = new Dictionary<string, bool>();
        private ComPermissionHandler()
        {
            DictPermission.Add(Consts.CFFEX, true);
            DictPermission.Add(Consts.SHFE, true);
            DictPermission.Add(Consts.CZCE, true);
            DictPermission.Add(Consts.DCE, true);
            DictPermission.Add(Consts.INE, true);
            DictPermission.Add(Consts.CTP, true);

            DictPermission.Add("IDB", false);
            DictPermission.Add("QUANT", false);
        }

        private Dictionary<string, Dictionary<string, Venue>> DictVenue = new Dictionary<string, Dictionary<string, Venue>>();
        

        public bool CheckVenuePermission(string venue, string security)
        {
            if (string.IsNullOrEmpty(venue))
                return false;

            //暂时不检查期货渠道权限 需要产品确定后面期货交易所的渠道权限管理怎么做 - zhongliang，songzhi
            if (DictPermission.ContainsKey(venue.ToUpper()))
                return DictPermission[venue];

            //if (!string.IsNullOrEmpty(venue) && IsInner.Contains(venue.ToUpper()))
            //    return false;

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


                if (!DictVenue.ContainsKey(goodsType))
                {
                    List<Venue> tmpvenues = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, string.Empty) ?? new List<Venue>();
                   
                    if (tmpvenues.Count > 0)
                    {
                        Dictionary<string, Venue> tempDict = tmpvenues.GroupBy(item => item.venue.ToUpper()).ToDictionary(group => group.Key, group => group.First());
                        DictVenue.Add(goodsType, tempDict);
                    }
                }
                
                if (DictVenue.ContainsKey(goodsType))
                {
                    result = DictVenue[goodsType].ContainsKey(venue.ToUpper());
                }
            }

            return result;
        }

    }
}
