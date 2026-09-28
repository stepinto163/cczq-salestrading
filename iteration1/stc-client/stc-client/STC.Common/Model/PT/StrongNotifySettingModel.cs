using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 默认卡片2*4排布
    /// </summary>
    public class StrongNotifySettingModel
    {
        private int strongnotifyfollowstyle = 0;
        /// <summary>
        /// 强提醒窗口跟随模式 0 强提醒窗口跟随主窗口 1 固定强提醒窗口在主窗口屏幕   2 强提醒窗口跟随交易处理  3 固定强提醒窗口在交易处理所在屏幕
        /// </summary>
        public int StrongNotifyFollowStyle
        {
            get
            {
                return strongnotifyfollowstyle;
            }
            set
            {
                strongnotifyfollowstyle = value;
            }
        }

        private string strongnotifyshowscreen = string.Empty;

        /// <summary>
        /// 强提醒显示屏幕
        /// </summary>
        public string StrongNotifyShowScreen
        {
            get
            {
                return strongnotifyshowscreen;
            }
            set
            {
                strongnotifyshowscreen = value;
            }
        }
    }
}
