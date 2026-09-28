using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.ViewModel.Order
{
    public class PermisionHelper
    {
        private static PermisionHelper permisionHelper = new PermisionHelper();

        public static PermisionHelper Instance { get { return permisionHelper; } }


        private PermisionHelper()
        { }

        /// <summary>
        /// 渠道权限
        /// </summary>
        private readonly string MODULEID = "1002";
        /// <summary>
        /// 所有权限
        /// </summary>
        private readonly string CHECK_ALL_ACTIONID = "100203";
        /// <summary>
        /// 仅内部
        /// </summary>
        private readonly string CHECK_INNER_ACTIONID = "100201";
        /// <summary>
        /// 内部，X-Bond
        /// </summary>
        private readonly string CHECK_INNER_XBOND_ACTIONID = "100202";

        public readonly string XBOND_TYPE = ">> X-bond报价";
        public readonly string NORMAL_TYPE = ">> 普通报价";
        public readonly string XBOND = "XBOND";

        public readonly string ADD_ONE_NOT_LIMIT = "2,3";


        //执行权限
        private readonly string MODULEID_EXEC = "1004";
        private readonly string CROSS = "100401";//直通
        private readonly string OCO = "100402";//OCO
        private readonly string SNIPER = "100404";
        private readonly string All_EXEC = "100403";//所有权限


        #region 渠道权限和交易权限
        /// <summary>
        /// 渠道权限-仅内部
        /// </summary>
        public bool IsHaveInnerPermission = false;

        /// <summary>
        /// 渠道权限-内部，X-Bond
        /// </summary>
        public bool IsHaveInnerXbondPermission = false;

        /// <summary>
        /// 渠道权限-所有权限
        /// </summary>
        public bool IsHaveAllPermission = false;

        /// <summary>
        /// 是否有Xbond撮合权限
        /// </summary>
        public bool IsHaveVenueXbondPermission = false;


        // 是否有内部交易权限
        public bool IsHaveVenueInnerPermission = false;
        #endregion

        public bool isCross()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_EXEC, CROSS);
        }

        public bool isOCO()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_EXEC, OCO);
        }

        public bool isSniper()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_EXEC, SNIPER);
        }

        public bool isAllExec()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_EXEC, All_EXEC);
        }

        public void InitPermission(string goodsType)
        {
            // 是否有内部交易权限
            IsHaveVenueInnerPermission = GetIsHaveInner(goodsType);
            // 渠道权限 内部
            IsHaveInnerPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
            // 渠道权限 内部+ X-Bond
            IsHaveInnerXbondPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);
            // 渠道权限 所有权限
            IsHaveAllPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
            // 是否有XBoud
            IsHaveVenueXbondPermission = CheckVenuePermission(XBOND, goodsType);

        }

        private bool GetIsHaveInner(string goodsType)
        {
            List<Venue> LstHaveInner = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, "1");
            if (LstHaveInner != null)
            {
                var result = LstHaveInner.Where(delegate (Venue uv)
                {
                    if (uv.venuetypegw == "INNER")
                        return true;
                    else
                        return false;
                }).ToList();

                return result.Count > 0 ? true : false;
            }
            else
                return false;
        }

        public bool CheckVenuePermission(string venue, string goodsType)
        {
            bool result = false;

            List<Venue> venues = SystemParamHandler.Instance.GetListVenueByCondition(goodsType, string.Empty);

            if (venues != null && venues.Count > 0)
            {
                foreach (Venue uv in venues)
                {
                    if (uv.name.ToUpper() == (venue.ToUpper()))
                    {
                        result = true;
                        break;
                    }
                }
            }


            return result;
        }
    }
}
