using Newtonsoft.Json;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Order.InnerView;
using STC.Common.ViewModel.Adapter;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Order;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Common.ViewModel.Order.InnerView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.View.Order
{
    public class UserSettingAdapter
    {
        private static UserSettingAdapter userSettingParse = new UserSettingAdapter();

        public static UserSettingAdapter Instance { get { return userSettingParse; } }

        private UserSettingAdapter()
        { }


        public void ParseAmount(SendRequireViewModel sendPriceViewModel)
        {
            sendPriceViewModel.SendPriceItem.Amount = "3";
            // 缺省报量
            if (SystemParamHandler.Instance.GetUserSetting() != null)
            {
                UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
                if (!string.IsNullOrEmpty(Us.userQty))
                {
                    sendPriceViewModel.SendPriceItem.Amount = !sendPriceViewModel.IsIRS ? (decimal.Parse(Us.userQty) / 1000).ToString() : (decimal.Parse(Us.userQty) / 100).ToString();
                }
                if (!string.IsNullOrEmpty(Us.inf2))
                {
                    if (JsonSplit.IsJson(Us.inf2))
                    {
                        ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
                        sendPriceViewModel.SendPriceConsistencyCheck = string.IsNullOrEmpty(ck.SendPriceConsistencyCheck) ? false : ck.SendPriceConsistencyCheck == "1" ? true : false;
                        sendPriceViewModel.PointPriceConsistencyCheck = string.IsNullOrEmpty(ck.PointPriceConsistencyCheck) ? false : ck.PointPriceConsistencyCheck == "1" ? true : false;
                    }
                }

            }
        }


        public void ParseAmount(SendPriceViewModel sendPriceViewModel)
        {
            sendPriceViewModel.SendPriceItem.Amount = "";
            // 缺省报量
            if (SystemParamHandler.Instance.GetUserSetting() != null)
            {
                UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
                if (!string.IsNullOrEmpty(Us.userQty))
                {
                    sendPriceViewModel.SendPriceItem.Amount = !sendPriceViewModel.IsIRS ? (decimal.Parse(Us.userQty) / 1000).ToString() : (decimal.Parse(Us.userQty) / 100).ToString();
                }
                if (!string.IsNullOrEmpty(Us.inf2))
                {
                    if (JsonSplit.IsJson(Us.inf2))
                    {
                        ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
                        sendPriceViewModel.SendPriceConsistencyCheck = string.IsNullOrEmpty(ck.SendPriceConsistencyCheck) ? false : ck.SendPriceConsistencyCheck == "1" ? true : false;
                        sendPriceViewModel.PointPriceConsistencyCheck = string.IsNullOrEmpty(ck.PointPriceConsistencyCheck) ? false : ck.PointPriceConsistencyCheck == "1" ? true : false;
                    }
                }

            }
        }


        /// <summary>
        /// 报价选择类型解析  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
        /// </summary>
        /// <param name="info2"></param>
        public void ParseValidTimeFormat(SendPriceViewModel sendPriceViewModel)
        {
            string info2 = string.Empty;
            if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf2))
            {
                ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                if (consistencyCheckPo != null)
                {
                    info2 = consistencyCheckPo.DefaultValidTime;
                }
            }

            if (!string.IsNullOrEmpty(info2))
            {
                DetailConditionViewModel detailConditionViewModel = sendPriceViewModel.GetDetailConditionViewModel();
                //报价选择过期类型  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}"
                if (info2.StartsWith("0|"))
                {
                    detailConditionViewModel.ValidSelectIndex = 0;
                }
                else if (info2.StartsWith("1|"))
                {
                    detailConditionViewModel.ValidSelectIndex = 1;
                    // ValidMinute,ValidSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        detailConditionViewModel.ValidMinute = s.Split(':')[0];
                        detailConditionViewModel.ValidSecond = s.Split(':')[1];
                    }
                }
                else if (info2.StartsWith("2|"))
                {
                    detailConditionViewModel.ValidSelectIndex = 2;

                    // ValidTimeHour ValidTimeMinute ValidTimeSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        detailConditionViewModel.ValidTimeHour = s.Split(':')[0];
                        detailConditionViewModel.ValidTimeMinute = s.Split(':')[1];
                        detailConditionViewModel.ValidTimeSecond = s.Split(':')[2];
                    }
                }
                else if (info2.StartsWith("3|"))
                {
                    detailConditionViewModel.ValidSelectIndex = 3;
                    // 无
                }
            }
        }


        /// <summary>
        /// 报价选择类型解析  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
        /// </summary>
        /// <param name="info2"></param>
        public void ParseValidTimeFormat(SendRequireViewModel sendPriceViewModel)
        {
            string info2 = string.Empty;
            if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf2))
            {
                ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                if (consistencyCheckPo != null)
                {
                    info2 = consistencyCheckPo.DefaultValidTime;
                }
            }

            if (!string.IsNullOrEmpty(info2))
            {

                TagConditionViewModel detailConditionViewModel = sendPriceViewModel.GetDetailConditionViewModel();

                //报价选择过期类型  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}"
                if (info2.StartsWith("0|"))
                {
                    detailConditionViewModel.ValidSelectIndex = 0;
                }
                else if (info2.StartsWith("1|"))
                {
                    detailConditionViewModel.ValidSelectIndex = 1;
                    // ValidMinute,ValidSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        detailConditionViewModel.ValidMinute = s.Split(':')[0];
                        detailConditionViewModel.ValidSecond = s.Split(':')[1];
                    }
                }
                else if (info2.StartsWith("2|"))
                {
                    detailConditionViewModel.ValidSelectIndex = 2;

                    // ValidTimeHour ValidTimeMinute ValidTimeSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        detailConditionViewModel.ValidTimeHour = s.Split(':')[0];
                        detailConditionViewModel.ValidTimeMinute = s.Split(':')[1];
                        detailConditionViewModel.ValidTimeSecond = s.Split(':')[2];
                    }
                }
                else if (info2.StartsWith("3|"))
                {
                    detailConditionViewModel.ValidSelectIndex = 3;
                    // 无
                }
            }
        }

        /// <summary>
        /// 获取条件撤单等设置
        /// </summary>
        /// <returns></returns>
        public ConsistencyCheckPo GetConsistencyCheckPo()
        {
            if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf2))
            {
                string s = SystemParamHandler.Instance.GetUserSetting().inf2;
                ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(s);
                if (string.IsNullOrEmpty(consistencyCheckPo.SubscribeReverse) && string.IsNullOrEmpty(consistencyCheckPo.SubscribeSyntropy))
                {
                    consistencyCheckPo.SubscribeReverse = "1";
                }

                return consistencyCheckPo;
            }
            else
            {
                ConsistencyCheckPo consistencyCheckPo = new ConsistencyCheckPo();
                consistencyCheckPo.SubscribeReverse = "1";
                return consistencyCheckPo;
            }
        }

        public void SetPriceCondition(ConditionCancelView conditionCancelView)
        {
            UserSetting us = SystemParamHandler.Instance.GetUserSetting();
            if (conditionCancelView != null && us != null)
            {
                conditionCancelView.SetPriceCondition(us.priceCondition);
            }
        }

        public void SetPriceCondition(ConditionCancelView conditionCancelView, string priceCondition)
        {
            if (conditionCancelView != null && !string.IsNullOrEmpty(priceCondition))
            {
                conditionCancelView.SetPriceCondition(priceCondition);
            }
        }

        public void SetPriceCondition(DetailConditionViewModel detailConditionViewModel, string priceCondition)
        {
            if (detailConditionViewModel != null && !string.IsNullOrEmpty(priceCondition))
            {
                detailConditionViewModel.SetPriceCondition(priceCondition);
            }
        }

        /// <summary>
        /// 解析订阅类型或 bool解析
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool ParseCheckedType(string info)
        {
            if (string.IsNullOrEmpty(info))
            {
                return false;
            }
            else
            {
                return info == "0" ? false : true;
            }
        }
    }
}
