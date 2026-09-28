using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.SystemParam
{
    /// <summary>
    /// 系统参数
    /// </summary>
    public class SystemParamHandler : ISystemParamHandler
    {
        private UserSetting cacheUserSetting;
        private SystemParamHandler() { }
        private static ISystemParamHandler instance = new SystemParamHandler();
        private object _lock = new object();
        private bool _isFirlt = true;

        public static ISystemParamHandler Instance { get { return instance; } }

        /// <summary>
        /// 订阅Tbook系统参数
        /// </summary>
        public void Init()
        {
            TBookHandler.Instance.SubscribeSystemParam(Instance_OnSystemParam);
        }

        private void SetDefaultVenueSort()
        {
            if (cacheUserSetting != null && !string.IsNullOrEmpty(cacheUserSetting.inf2))
            {
                ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(cacheUserSetting.inf2);
                consistencyCheckPo.BrokerSort = "TPSC:CTTJ:CNEX:CCTB:PTCN:UEDA";

                cacheUserSetting.inf2 = JsonConvert.SerializeObject(consistencyCheckPo);
            }
        }

        /// <summary>
        /// Tbook系统参数回调方法
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnSystemParam(UserSetting us)
        {
            lock (_lock)
            {
                if (_isFirlt)
                {
                    
                    cacheUserSetting = us;
                    //设置手工单默认下单组合
                    LocalEnv.ManualOrderDefaultBookId = View.Order.UserSettingAdapter.Instance.GetConsistencyCheckPo().BookId;
                    //待删除
                    SetDefaultVenueSort();
                       
                    _isFirlt = false;
                }
                else
                {
                    if (us.systemVenue != null)
                        cacheUserSetting.systemVenue = us.systemVenue;
                }
            }
        }

        /// <summary>
        /// 获取用户设置信息
        /// </summary>
        /// <returns></returns>
        public UserSetting GetUserSetting()
        {
            return cacheUserSetting;
        }

        public void SetUserSetting(UserSetting userSetting)
        {
            this.cacheUserSetting = userSetting;
        }

        /// <summary>
        /// 获取用户渠道信息
        /// </summary>
        /// <returns></returns>
        public List<UserVenue> GetUserVenue()
        {
            if (cacheUserSetting != null)
                return cacheUserSetting.userVenue;

            return null;
        }

        /// <summary>
        /// 更新新的用户设置
        /// </summary>
        /// <param name="newSetting"></param>
        public void CopyParam(UserSetting newSetting)
        {
            lock (_lock)
            {
                cacheUserSetting.userVenue = newSetting.userVenue;
                cacheUserSetting.userQty = newSetting.userQty;
                cacheUserSetting.userNofirmIOCTimeOut = newSetting.userNofirmIOCTimeOut;
                cacheUserSetting.validUntiTimeSetting = newSetting.validUntiTimeSetting;
                cacheUserSetting.inf1 = newSetting.inf1;
                cacheUserSetting.inf2 = newSetting.inf2;
                cacheUserSetting.inf3 = newSetting.inf3;
                cacheUserSetting.priceCondition = newSetting.priceCondition;
                //设置手工单默认下单组合
                LocalEnv.ManualOrderDefaultBookId = View.Order.UserSettingAdapter.Instance.GetConsistencyCheckPo().BookId;
            }
        }

        /// <summary>
        /// 更新用户设置信息
        /// </summary>
        /// <returns></returns>
        public UserSetting CopyUserSetting()
        {
            UserSetting result = null;
            try
            {
                lock (_lock)
                {
                    string str = MsgHelper.Serializer<UserSetting>(cacheUserSetting);

                    result = MsgHelper.Deserialize<UserSetting>(str);
                }
            }
            catch(Exception e)
            {
                LogHelper.Info(cacheUserSetting);
                LogHelper.Error(e);
            }
            
            return result;
        }
        
        /// <summary>
        /// 得到firm转nofirm时间
        /// </summary>
        /// <returns></returns>
        public string GetFirmtonontime()
        {
            string result = string.Empty;

            if (cacheUserSetting != null && cacheUserSetting.userVenue != null && cacheUserSetting.userVenue.Count > 0)
            {
                result = cacheUserSetting.userVenue[0].firmtonontime;
            }

            return result;
        }


        /// <summary>
        /// 获取包含INNER的所有渠道
        /// </summary>
        /// <param name="goodsType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<Venue> GetAllVenueByCondition(string goodsType, string level)
        {
            List<Venue> result = null;
            lock (_lock)
            {
                if (cacheUserSetting != null && cacheUserSetting.systemVenue != null)
                {
                    if (!string.IsNullOrEmpty(goodsType))
                    {
                        result = cacheUserSetting.systemVenue.Where(delegate (Venue uv)
                        {
                            if (!string.IsNullOrEmpty(uv.type) && uv.type.Equals(goodsType) && uv.level > 0)
                                return true;
                            else
                                return false;
                        }).ToList();
                    }

                    if (result != null && result.Count > 0 && !string.IsNullOrEmpty(level))
                    {
                        result = result.Where(delegate (Venue uv)
                        {
                            if (uv.level.ToString().Equals(level) && uv.level > 0)
                                return true;
                            else
                                return false;
                        }).ToList();
                    }
                }
            }

            return result;
        }


        public List<string> GetAllVenue()
        {
            List<string> result = new List<string>();
            lock (_lock)
            {
                if (cacheUserSetting != null && cacheUserSetting.systemVenue != null)
                {
                    List<Venue> venueList = cacheUserSetting.systemVenue.Where(delegate (Venue uv)
                    {
                        if (!string.IsNullOrEmpty(uv.type) && uv.level > 0 && uv.venuetypegw != "INNER")
                            return true;
                        else
                            return false;
                    }).ToList();

                    foreach (Venue venue in venueList)
                    {
                        if (!result.Contains(venue.venue))
                        {
                            result.Add(venue.venue);
                        }
                    }

                }
            }

            return result;
        }

        /// <summary>
        /// 根据goodsType和level筛选渠道列表
        /// </summary>
        /// <param name="goodsType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<Venue> GetListVenueByCondition(string goodsType, string level)
        {
            List<Venue> result = null;
            lock (_lock)
            {
                if (cacheUserSetting != null && cacheUserSetting.systemVenue != null)
                {
                    if (!string.IsNullOrEmpty(goodsType))
                    {
                        result = cacheUserSetting.systemVenue.Where(delegate (Venue uv)
                        {
                            if (!string.IsNullOrEmpty(uv.type) && uv.type.Equals(goodsType) && uv.level > 0 && uv.venuetypegw != "INNER")
                                return true;
                            else
                                return false;
                        }).ToList();
                    }

                    if (result != null && result.Count > 0 && !string.IsNullOrEmpty(level))
                    {
                        result = result.Where(delegate (Venue uv)
                        {
                            if (uv.level.ToString().Equals(level) && uv.level > 0 && uv.venuetypegw!="INNER")
                                return true;
                            else
                                return false;
                        }).ToList();
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// 根据goodsType和level筛选渠道列表
        /// </summary>
        /// <param name="goodsType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<Venue> GetListVenue(string goodsType, string level)
        {
            List<Venue> result = null;
            lock (_lock)
            {
                if (cacheUserSetting != null && cacheUserSetting.systemVenue != null)
                {
                    if (!string.IsNullOrEmpty(goodsType))
                    {
                        result = cacheUserSetting.systemVenue.Where(delegate (Venue uv)
                        {
                            if (!string.IsNullOrEmpty(uv.type) && uv.type.Equals(goodsType) && uv.level > 0)// && uv.venuetypegw != "INNER")
                                return true;
                            else
                                return false;
                        }).ToList();
                    }

                    if (result != null && result.Count > 0 && !string.IsNullOrEmpty(level))
                    {
                        result = result.Where(delegate (Venue uv)
                        {
                            if (uv.level.ToString().Equals(level) && uv.level > 0)// && uv.venuetypegw != "INNER")
                                return true;
                            else
                                return false;
                        }).ToList();
                    }
                }
            }

            return result;
        }



        /// <summary>
        /// 根据goodsType和level筛选渠道列表
        /// </summary>
        /// <param name="goodsType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<Venue> GetListVenueByFilter(string goodsType, string level)
        {
            List<Venue> result = null;
            lock (_lock)
            {
                if (cacheUserSetting != null && cacheUserSetting.systemVenue != null)
                {
                    if (!string.IsNullOrEmpty(goodsType))
                    {
                        result = cacheUserSetting.systemVenue.Where(delegate (Venue uv)
                        {
                            if (!string.IsNullOrEmpty(uv.type)
                                && (uv.type.Equals(goodsType) || ("CTP".Equals(uv.venue) && ("T".Equals(goodsType) || "TF".Equals(goodsType) || "TS".Equals(goodsType)) || "BGB".Equals(goodsType)))
                                && uv.level > 0)
                                return true;
                            else
                                return false;
                        }).ToList();
                    }

                    if (result != null && result.Count > 0 && !string.IsNullOrEmpty(level))
                    {
                        result = result.Where(delegate (Venue uv)
                        {
                            if (uv.level.ToString().Equals(level) && uv.level > 0)
                                return true;
                            else
                                return false;
                        }).ToList();
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// 根据goodsType和venue筛选渠道列表
        /// </summary>
        /// <param name="goodsType"></param>
        /// <param name="venue"></param>
        /// <returns></returns>
        public Venue GetVenueByCondition(string goodsType, string venue)
        {
            Venue result = null;
            lock (_lock)
            {
                if (cacheUserSetting != null && cacheUserSetting.systemVenue != null)
                {
                    List<Venue> venues = cacheUserSetting.systemVenue.Where(delegate (Venue uv)
                    {
                        if (uv.type.Equals(goodsType) && uv.venue.Equals(venue) && uv.level > 0)
                            return true;
                        else
                            return false;
                    }).ToList();

                    if (venues.Count > 0)
                        result = venues[0];
                }
            }
            return result;
        }
        
        /// <summary>
        /// 根据goodType和operationType筛选用户渠道列表
        /// </summary>
        /// <param name="goodType"></param>
        /// <param name="oprationType"></param>
        /// <returns></returns>
        public List<UserVenue> GetUserVenueByCondition(string goodType, string oprationType)
        {
            List<UserVenue> result = null;
            lock (_lock)
            {
                if (cacheUserSetting != null && cacheUserSetting.userVenue != null)
                {
                    if (!string.IsNullOrEmpty(goodType))
                    {
                        result = cacheUserSetting.userVenue.Where(delegate (UserVenue uv)
                        {
                            if (!string.IsNullOrEmpty(uv.type) && uv.type.Equals(goodType))
                                return true;
                            else
                                return false;
                        }).ToList();
                    }

                    if (result != null && !string.IsNullOrEmpty(oprationType))
                    {
                        result = result.Where(delegate (UserVenue uv)
                        {
                            if (!string.IsNullOrEmpty(uv.venuesselectf) && uv.venuesselectf.Equals(oprationType))
                                return true;
                            else
                                return false;
                        }).ToList();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 更新TBook系统参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult UpdatesystemParas(UserSetting request)
        {
            return TBookHandler.Instance.UpdatesystemParas(request);
        }

        public List<string> GetCustomVenueSort()
        {
            List<string> result = new List<string>();

            //还需前置总开关
            if (cacheUserSetting != null)
            {
                if (!string.IsNullOrEmpty(cacheUserSetting.inf2))
                {
                    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(cacheUserSetting.inf2);
                    if (!string.IsNullOrEmpty(consistencyCheckPo.BrokerSort))
                    {
                        result = consistencyCheckPo.BrokerSort.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }
            }
            return result;
        }

        private List<string> LstVenueSort = new List<string>();
        private bool IsQueryVenueSort = false;
        public string GetVenueSort(string venue)
        {
            if (!IsQueryVenueSort)
            {
                LstVenueSort = GetCustomVenueSort();
                IsQueryVenueSort = true;
            }

            int sq = LstVenueSort.IndexOf(venue);
            if (sq == -1)
            {
                switch (venue.ToUpper())
                {
                    case "IDB":
                    case "QUANT":
                    case "MANUAL":
                        sq = 95;
                        break;
                    case "QDMESP":
                        sq = 96;
                        break;
                    case "ODM":
                        sq = 97;
                        break;
                    default:
                        sq = 99;
                        break;
                }
            }

            return sq.ToString();
        }

        public static bool IsShowTenor()
        {
            return true;//return "true".Equals(GlobSystemParameter.formatTenorCheck);
        }

        public static bool IsShowShortName()
        {
            return true;
            // return "true".Equals(GlobSystemParameter.formatShortNameCheck);
        }

        public static bool AppSettingIsTrue(string name)
        //=> GRSettingsHandler.ClientAppSettings.TryGetValue(name, out var valueStr)
        //&& bool.TryParse(valueStr, out var appSetting)
        //&& appSetting;
        {
            return false;
        }

        /// <summary>
        /// 判断当前登录用户对userid是否具有修改权限
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        static public bool IsMyModifys(string userid)
        {
            bool flag1 = false;
            bool flag2 = false;
            //自己
            flag1 = LocalEnv.UserInfo.IsMySelfByName(userid);
            //if (SystemParamHandler.GlobSystemParameter.UpdateOrders != null)
            //{
            //    List<AccountInfo> accountInfos = (from a in SystemParamHandler.GlobSystemParameter.UpdateOrders where a.Name == LocalEnv.UserInfo.FullName select a).ToList();

            //    if (accountInfos != null && accountInfos.Count > 0 && accountInfos[0].Users != null && accountInfos[0].Users.Count > 0)
            //    {
            //        //修改权限
            //        flag2 = accountInfos[0].Users.Any(a => a.Name == userid);
            //    }
            //}
            return flag1 || flag2;
        }


    }
}
