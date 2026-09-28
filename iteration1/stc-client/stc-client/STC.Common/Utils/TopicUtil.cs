using STC.Common.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class TopicUtil
    {
        private static Dictionary<string, string> _brokerDic = new Dictionary<string, string>();

        private static object lockobj = new object();
        private static void Init()
        {

                if (!_brokerDic.ContainsKey("1"))
                    _brokerDic.Add("1", "TPSC");//国利
                if (!_brokerDic.ContainsKey("2"))
                    _brokerDic.Add("2", "CNEX");//国际
                if (!_brokerDic.ContainsKey("3"))
                    _brokerDic.Add("3", "CCTB");//中诚
                if (!_brokerDic.ContainsKey("4"))
                    _brokerDic.Add("4", "PTCN");//平安
                if (!_brokerDic.ContainsKey("5"))
                    _brokerDic.Add("5", "CTTJ");//信唐
            
        }

        private static string GetBrokerCode(string key)
        {
            lock (lockobj)
            {
                if (_brokerDic.Count == 0)
                {
                    Init();
                }
            }


            if (_brokerDic.ContainsKey(key))
                return _brokerDic[key];
            else
                return string.Empty;
        }


        //BDB.md.sdn.200215IBTPSC=sdn
        public static List<string> GetMdRic(string security, string gw)
        {
            List<string> result = new List<string>();

            if (!string.IsNullOrEmpty(security))
            {
                string[] arr = gw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string str in arr)
                {
                    string broker = GetBrokerCode(str);
                    string ric = string.Format("{0}{1}.{2}={3}", ApiConsts.Topic_stc_md_prefix, ApiConsts.CT.ToLower(), security.Replace(".", ""), ApiConsts.CT.ToLower());

                    if (!result.Contains(ric))
                        result.Add(ric);
                }
            }
            return result;
        }


        public static List<string> GetHoRic(string security, string gw)
        {
            List<string> result = new List<string>();

            string[] arr = gw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in arr)
            {
                string broker = GetBrokerCode(str);
                string ric = string.Format("{0}{1}.{2}{3}={4}", ApiConsts.Topic_stc_md_prefix, ApiConsts.H0, security.Replace(".", ""), broker, ApiConsts.SDN.ToLower());

                result.Add(ric);
            }

            return result;
        }




        public static string getTopic(string topic, string gw, string pair, string tenor,
            string type, string broker)
        {
            string topicPreFix = topic + gw.ToLower() + ".";
            return topicPreFix + getRic(gw, pair, tenor, broker, type);
        }

        public static string getRic(string gw, string pair, string tenor, string broker,
            string type)
        {
            string ric = "";
            if (string.IsNullOrEmpty(pair))
                pair = string.Empty;

            if (!string.IsNullOrEmpty(broker))
                ric = string.Format("{0}{1}{2}{3}={4}", pair.Replace(".", ""), tenor==null?string.Empty:tenor, broker, gw.ToLower());
            else
                ric = string.Format("{0}{1}{2}={3}", pair.Replace(".", ""), tenor == null ? "" : tenor, type, gw.ToLower());

            return ric;
        }
    }
}
