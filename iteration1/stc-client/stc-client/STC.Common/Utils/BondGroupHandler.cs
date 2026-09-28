using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class BondGroupHandler
    {
        private static BondGroupHandler _instance = new BondGroupHandler();

        public static BondGroupHandler Instance
        {
            get { return _instance; }
        }

        private BondGroupHandler()
        {
            LoadGroupBonds2();
            DesignHandler.Instance.OnNotifyUpdateGroup += Instance_OnNotifyUpdateGroup;
        }

        private void Instance_OnNotifyUpdateGroup(object obj)
        {
            LoadGroupBonds2();
        }

        public List<string> GetSecuritys(List<string> groupids)
        {
            List<string> bondsList = new List<string>();
            if (groupids != null && groupids.Count > 0)
            {
                foreach(string groupid in groupids)
                {
                    if (_groupBondsDic.ContainsKey(groupid))
                    {
                        foreach(HomeGroupSecurityIDsInfo securityIdsInfo in _groupBondsDic[groupid].securityids)
                        {
                            if (!bondsList.Contains(securityIdsInfo.securityid))
                            {
                                bondsList.Add(securityIdsInfo.securityid);
                            }
                        }
                    }
                    
                }
            }
            return bondsList;
        }

        #region 券池

        ConcurrentDictionary<string, HomeGroupInfo> _groupBondsDic = new ConcurrentDictionary<string, HomeGroupInfo>();

        public void LoadGroupBonds2()
        {
            HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
            request.type = CommonConstants.INNER_Type;
            request.username = LocalEnv.UserInfo.UserName;
            HomeGroupSecurityidsReplyPo replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery2(request);

            if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
            {
                // 收集收藏债券
                _groupBondsDic.Clear();
                foreach (HomeGroupInfo hgi in replyPo.Datas.Values)
                {
                    _groupBondsDic[hgi.id] = hgi;
                }
            }
        }
        #endregion
    }
}
