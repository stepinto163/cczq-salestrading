using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.Common
{
    public class Constants
    {

        public static readonly string TOPIC_SIMULATE_ORDER = "stclite.s.o";


        public const string TBookSvr = "TBookSvr";
        public const string Topic_stc_tbook_prefix = "stc.tbook.s.";
        public const string Topic_stc_tbook_placeOrder = "stc.tbook.o.n";
        public const string Topic_stc_tbook_replaceOrder = "stc.tbook.o.r";
        public const string Topic_stc_tbook_massCancelOrder = "stc.tbook.o.m";

        public const string Topic_stc_tbook_cancelOrder = "stc.tbook.o.c";
        public const string Topic_stc_tbook_orderQuery = "orderQuery";

        public const string BaseSvr = "BaseSvr";
        public const string Topic_stc_base_queryAccount = "queryAccount";
        public const string Topic_stc_base_mqAccount = "stc.base.mq.account";

        public const string EMSSvr = "EMSSvr";
        public static string Topic_stc_ems_prefix = "stc.ems.v.";
        public const string Topic_stc_ems_orderQuery = "orderQuery";

        //预埋单激活或撤销
        public const string Topic_pre_activation = "stc.event.publish";

        public static int delayTime = 5;

        public const string CODE_7000 = "7000";
        public const string MSG_7000 = "发送失败，请登录QM";
        public const string CODE_7001 = "7001";
        public const string MSG_7001 = "发送失败，未找到对应Broker";

        public const string VenueTypeGW = "SOR";

        public const string MMSvr = "MMSvr";
        public const string Topic_stc_mm_cancelQuant = "stc.mm.bond.strategyApi";

    }
}
