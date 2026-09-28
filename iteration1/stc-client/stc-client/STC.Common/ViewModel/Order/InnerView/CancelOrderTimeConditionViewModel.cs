using Ats.Foundation.Message;
using Newtonsoft.Json;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.CRM;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Order.InnerPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Common.ViewModel.Order.InnerView
{
    public class CancelOrderTimeConditionViewModel : BaseViewModel
    {
        #region 构造

        #region  报价有效时长属性
        private int validSelectIndex = 0;

        public int ValidSelectIndex
        {
            get
            {
                return validSelectIndex;
            }

            set
            {
                validSelectIndex = value;
                if (validSelectIndex == 0)
                {
                    DefaultValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 1)
                {
                    HourValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 2)
                {
                    TimeValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 3)
                {
                    NoLimitValidTimeQuotationCheck = true;
                }

                this.OnPropertyChanged(nameof(ValidSelectIndex));
            }
        }

        private int defaultValidUntilTime = 30;
        public int DefaultValidUntilTime
        {
            get
            {
                return defaultValidUntilTime;
            }

            set
            {
                defaultValidUntilTime = value;
                this.OnPropertyChanged(nameof(DefaultValidUntilTime));
            }
        }



        private bool defaultValidTimeQuotationCheck = true;
        /// <summary>
        /// 默认
        /// </summary>
        public bool DefaultValidTimeQuotationCheck
        {
            get
            {
                return defaultValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(HourValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                    {
                        return;
                    }
                }

                defaultValidTimeQuotationCheck = value;
                if (defaultValidTimeQuotationCheck)
                {
                    HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }

                this.OnPropertyChanged(nameof(DefaultValidTimeQuotationCheck));
            }
        }

        private bool hourValidTimeQuotationCheck = false;
        /// <summary>
        /// 时长
        /// </summary>
        public bool HourValidTimeQuotationCheck
        {
            get
            {
                return hourValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(DefaultValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                    {
                        return;
                    }
                }
                hourValidTimeQuotationCheck = value;
                if (hourValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = TimeValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged(nameof(HourValidTimeQuotationCheck));
            }
        }

        /// <summary>
        /// 时长 - 分钟
        /// </summary>
        private string validMinute = string.Empty;
        public string ValidMinute
        {
            get { return validMinute; }
            set
            {
                if (value != null)
                {
                    validMinute = value;
                    this.OnPropertyChanged(nameof(ValidMinute));
                }
            }
        }

        /// <summary>
        ///  时长 - 秒钟
        /// </summary>
        private string validSecond = string.Empty;
        public string ValidSecond
        {
            get { return validSecond; }
            set
            {
                if (value != null)
                {
                    validSecond = value;
                    this.OnPropertyChanged(nameof(ValidSecond));
                }
            }
        }

        /// <summary>
        /// 时点 - 小时
        /// </summary>
        private string validTimeHour = string.Empty;
        public string ValidTimeHour
        {
            get { return validTimeHour; }
            set
            {
                if (value != null)
                {
                    validTimeHour = value;
                    this.OnPropertyChanged(nameof(ValidTimeHour));
                }
            }
        }
        /// <summary>
        /// 时点 - 分钟
        /// </summary>
        private string validTimeMinute = string.Empty;
        public string ValidTimeMinute
        {
            get { return validTimeMinute; }
            set
            {
                if (value != null)
                {
                    validTimeMinute = value;
                    this.OnPropertyChanged(nameof(ValidTimeMinute));
                }
            }
        }

        /// <summary>
        /// 时点 - 秒钟
        /// </summary>
        private string validTimeSecond = string.Empty;
        public string ValidTimeSecond
        {
            get { return validTimeSecond; }
            set
            {
                if (value != null)
                {
                    validTimeSecond = value;
                    this.OnPropertyChanged(nameof(ValidTimeSecond));
                }
            }
        }


        private bool timeValidTimeQuotationCheck = false;
        /// <summary>
        /// 时点
        /// </summary>
        public bool TimeValidTimeQuotationCheck
        {
            get
            {
                return timeValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(DefaultValidTimeQuotationCheck ^ HourValidTimeQuotationCheck))
                    {
                        return;
                    }
                }
                timeValidTimeQuotationCheck = value;
                if (timeValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged(nameof(TimeValidTimeQuotationCheck));
            }
        }

        private bool noLimitValidTimeQuotationCheck = false;
        /// <summary>
        /// 无
        /// </summary>
        public bool NoLimitValidTimeQuotationCheck
        {
            get
            {
                return noLimitValidTimeQuotationCheck;
            }

            set
            {
                noLimitValidTimeQuotationCheck = value;
                if (noLimitValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged(nameof(NoLimitValidTimeQuotationCheck));
            }
        }


        private int validSelectIndexRfq = 0;
        /// <summary>
        /// Rfq
        /// </summary>
        public int ValidSelectIndexRfq
        {
            get
            {
                return validSelectIndexRfq;
            }

            set
            {
                validSelectIndexRfq = value;
                this.OnPropertyChanged(nameof(ValidSelectIndexRfq));
            }
        }

        /// <summary>
        /// 时长 - 分钟
        /// </summary>
        private string validMinuteRfq = string.Empty;
        public string ValidMinuteRfq
        {
            get { return validMinuteRfq; }
            set
            {
                if (value != null)
                {
                    validMinuteRfq = value;
                    this.OnPropertyChanged(nameof(ValidMinuteRfq));
                }
            }
        }

        /// <summary>
        ///  时长 - 秒钟
        /// </summary>
        private string validSecondRfq = string.Empty;
        public string ValidSecondRfq
        {
            get { return validSecondRfq; }
            set
            {
                if (value != null)
                {
                    validSecondRfq = value;
                    this.OnPropertyChanged(nameof(ValidSecondRfq));
                }
            }
        }

        #endregion

        #endregion

        public CancelOrderTimeConditionViewModel()
        {
            InitViewModel();
        }

        #region Init

        /// <summary>
        /// 初始化界面，读取默认设置
        /// </summary>
        public void InitViewModel()
        {
            if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf2))
            {
                ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);

                //解析默认有效时长
                ParseValidTimeFormat(consistencyCheckPo.DefaultValidTime);
            }
        }

        #endregion

        #region 报价选择类型解析
        /// <summary>
        /// 报价选择类型解析  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
        /// </summary>
        /// <param name="info2"></param>
        public void ParseValidTimeFormat(string info2)
        {
            if (!string.IsNullOrEmpty(info2))
            {
                //报价选择过期类型  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}"
                if (info2.StartsWith("0|"))
                {
                    ValidSelectIndex = 0;
                }
                else if (info2.StartsWith("1|"))
                {
                    ValidSelectIndex = 1;
                    // ValidMinute,ValidSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidMinute = s.Split(':')[0];
                        ValidSecond = s.Split(':')[1];
                    }
                }
                else if (info2.StartsWith("2|"))
                {
                    ValidSelectIndex = 2;

                    // ValidTimeHour ValidTimeMinute ValidTimeSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidTimeHour = s.Split(':')[0];
                        ValidTimeMinute = s.Split(':')[1];
                        ValidTimeSecond = s.Split(':')[2];
                    }
                }
                else if (info2.StartsWith("3|"))
                {
                    ValidSelectIndex = 3;
                    // 无
                }
            }
        }

      
        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <returns></returns>
        public BaseReplyPo GetErrorText()
        {
            BaseReplyPo reply = new BaseReplyPo();
            reply.Code = "10000";
            reply.Msg = "";
            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();

            TPlaceOrderResult por = new TPlaceOrderResult();
            por.Code = "10003";
            por.DoubleWarn = false;
            decimal temD = 0;

            if (HourValidTimeQuotationCheck)
            {
                // 时长
                // ValidMinute  ValidSecond
                if (string.IsNullOrEmpty(ValidMinute) && string.IsNullOrEmpty(ValidSecond))
                {
                    reply.Code = "10001";
                    por.Reason = "有效时长需大于0秒";
                }

                int totalsecond = 0;
                if (!string.IsNullOrEmpty(ValidMinute))
                {
                    totalsecond = int.Parse(ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(ValidSecond))
                {
                    totalsecond += int.Parse(ValidSecond);
                }

                if (totalsecond == 0)
                {
                    reply.Code = "10001";
                    por.Reason = "有效时长需大于0秒";
                }
            }
            else if (TimeValidTimeQuotationCheck)
            {
                // 时点
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                if (string.IsNullOrEmpty(ValidTimeHour))
                {
                    reply.Code = "10001";
                    por.Reason = "必须输入时";
                }

                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(ValidTimeHour))
                {
                    hour = int.Parse(ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(ValidTimeMinute))
                {
                    min = int.Parse(ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(ValidTimeSecond))
                {
                    sec = int.Parse(ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                if (DateTimeEx.ServerTimeNow >= tmpdt)
                {
                    reply.Code = "10001";
                    por.Reason = "订单撤单时点不能在报单时间之前";
                }
            }

            result.Add(por);

            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);

            return reply;
        }

 
        /// <summary>
        /// 获取撤单时间设置
        /// </summary>
        /// <returns>默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"</returns>
        public string GetValidTimeType()
        {
            // Modi:cyc Date:2021-04-13 Func:增加用户选择的 需求有效时长 选项记忆功能
            string validTimeType = DefaultValidTimeQuotationCheck ? "0" : HourValidTimeQuotationCheck ? "1" : TimeValidTimeQuotationCheck ? "2" : NoLimitValidTimeQuotationCheck ? "3" : "0";
            // 默认
            string info1 = string.Empty;
            if (DefaultValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
            }

            // 时长
            if (HourValidTimeQuotationCheck)
            {
                // ValidMinute,ValidSecond
                string min = string.IsNullOrEmpty(ValidMinute) ? "" : ValidMinute;
                string sec = string.IsNullOrEmpty(ValidSecond) ? "" : ValidSecond;
                info1 = $"{validTimeType}|{min}:{sec}";
            }

            // 时点
            if (TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                string hour = string.IsNullOrEmpty(ValidTimeHour) ? "" : ValidTimeHour;
                string min = string.IsNullOrEmpty(ValidTimeMinute) ? "" : ValidTimeMinute;
                string sec = string.IsNullOrEmpty(ValidTimeSecond) ? "" : ValidTimeSecond;
                info1 = $"{validTimeType}|{hour}:{min}:{sec}";
            }

            // 无
            if (NoLimitValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
            }

            return info1;
        }

        /// <summary>
        /// 获取撤单时间
        /// </summary>
        /// <returns>yyyy-MM-dd HH:mm:ss</returns>
        public string GetValidUntilTime()
        {
            string ValidUntilTime = string.Empty;
            string Info1 = string.Empty;
            // 默认时间
            if (DefaultValidTimeQuotationCheck)
            {
                int hour = DateTimeEx.ServerTimeNow.Hour;
                int min = DateTimeEx.ServerTimeNow.Minute;
                if (hour > 0 && hour < 6)
                {
                    ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (hour >= 6 && hour < 12)
                {
                    ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时长
            if (HourValidTimeQuotationCheck)
            {
                // ValidMinute  ValidSecond
                int totalsecond = 0;
                if (!string.IsNullOrEmpty(ValidMinute))
                {
                    totalsecond = int.Parse(ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(ValidSecond))
                {
                    totalsecond += int.Parse(ValidSecond);
                }

                if (totalsecond == 0)
                {
                    ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(DefaultValidUntilTime).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时点
            if (TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(ValidTimeHour))
                {
                    hour = int.Parse(ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(ValidTimeMinute))
                {
                    min = int.Parse(ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(ValidTimeSecond))
                {
                    sec = int.Parse(ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                ValidUntilTime = tmpdt.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // 无
            if (NoLimitValidTimeQuotationCheck)
            {
                // 不填写
            }

            // 用户设置的到期时间,需要大于用户自定义的时间
            string selfUntilTime = ValidUntilTime;
            UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
            if (userSetting != null)
            {
                // 个人设置时间
                string validUntiTimeSetting = userSetting.validUntiTimeSetting;

                // 如果撤单时间和个人设置时间不为空
                if (!string.IsNullOrEmpty(selfUntilTime) && (!string.IsNullOrEmpty(validUntiTimeSetting)))
                {
                    validUntiTimeSetting = string.Format("{0} {1}", DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString(), validUntiTimeSetting);
                    // 默认，时长，时点
                    if (!NoLimitValidTimeQuotationCheck)
                    {
                        ValidUntilTime = GetMinDate(selfUntilTime, validUntiTimeSetting, DefaultValidTimeQuotationCheck);
                    }
                    else
                    {
                        //选择了无
                        if (DateTimeEx.ServerTimeNow.CompareTo(DateTime.Parse(validUntiTimeSetting)) <= 0)
                        {
                            ValidUntilTime = validUntiTimeSetting;
                        }
                        else
                        {
                            ValidUntilTime = string.Empty;
                        }
                    }
                }
                else
                {
                    // 默认，时长，时点
                    if (!NoLimitValidTimeQuotationCheck)
                    {
                        // 默认
                        if (DefaultValidTimeQuotationCheck)
                        {
                            ValidUntilTime = GetMinDate(selfUntilTime, "", true);
                        }
                    }
                    else
                    {
                        ValidUntilTime = string.Empty;
                    }
                }
            }

            return ValidUntilTime;
        }

        /// <summary>
        /// 取2个日期的最小值,当为默认时
        /// </summary>
        /// <param name="selfUntilTime">撤单时间</param>
        /// <param name="validUntiTimeSetting">个人设置</param>
        /// <returns></returns>
        private string GetMinDate(string selfUntilTime, string validUntiTimeSetting = "", bool isDefault = true)
        {
            List<DateTime> LstTime = new List<DateTime>();
            DateTime cudatetime = DateTimeEx.ServerTimeNow;
            LogHandler.Info($"GetMinDate:selfUntilTime:{selfUntilTime} validUntiTimeSetting:{validUntiTimeSetting} isDefault:{isDefault.ToString()}" + " ServerTime:" + cudatetime.ToString("yyyy-MM-dd HH:mm:ss"));
            // 如果是时点, 失效时间<当前时间，则直接失效
            if (!isDefault)
            {
                if (cudatetime.CompareTo(DateTime.Parse(selfUntilTime)) >= 0)
                {
                    return selfUntilTime;
                }
            }

            // DateTime cudatetime = DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 08:35:00");
            if (isDefault)
            {
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00"));
            }

            LstTime.Add(cudatetime);
            LstTime.Add(DateTime.Parse(selfUntilTime));
            if (!string.IsNullOrEmpty(validUntiTimeSetting))
            {
                LstTime.Add(DateTime.Parse(validUntiTimeSetting));
            }

            List<DateTime> NewLstTime = LstTime.Distinct().ToList();
            NewLstTime.Sort();
            int i = NewLstTime.IndexOf(cudatetime);
            NewLstTime.RemoveRange(0, i);

            if (NewLstTime.Count == 1)
            {
                return string.Empty;
            }
            else
            {
                return NewLstTime[1].ToString("yyyy-MM-dd HH:mm:ss");
            }
        }


        #region RFQ
        /// <summary>
        /// Rfq选择类型解析  时长 "1|{min}:{sec}"
        /// </summary>
        /// <param name="info2"></param>
        public void ParseRfqValidTimeFormat(string info2)
        {
            if (!string.IsNullOrEmpty(info2))
            {
                if (info2.StartsWith("1|"))
                {
                    ValidSelectIndexRfq = 1;
                    // ValidMinute,ValidSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidMinuteRfq = s.Split(':')[0];
                        ValidSecondRfq = s.Split(':')[1];
                    }
                }
            }
        }

        /// <summary>
        /// Rfq
        /// </summary>
        /// <returns></returns>
        public string GetRfqDefaultValidTimeFormat()
        {
            // Modi:cyc Date:2022-12-16 Func:增加用户选择的 Rfq 需求有效时长 选项记忆功能  "1|{min}:{sec}
            string validTimeType = "1";
            // 默认
            string info1 = string.Empty;

            // 时长
            {
                // ValidMinute,ValidSecond
                string min = string.IsNullOrEmpty(ValidMinuteRfq) ? "" : ValidMinuteRfq;
                string sec = string.IsNullOrEmpty(ValidSecondRfq) ? "" : ValidSecondRfq;
                info1 = $"{validTimeType}|{min}:{sec}";
            }

            return info1;
        }
        #endregion

        #endregion
    }
}