using Ats.Foundation.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public class BaseHelper
    {
        /// <summary>
        /// 根据topic检查是否是需要解析的消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="isMatchStartChar"></param>
        /// <param name="stcMessage"></param>
        /// <returns></returns>
        public static T ConvertStcMessage<T>(string[] topic, object stcMessage)
        {
            T t = default(T);
            if (stcMessage != null)
            {
                if (typeof(T).Equals(typeof(string)))
                {
                    object o = JsonConvert.SerializeObject(stcMessage);
                    t = (T)o;
                    return t;
                }


                NetMessage msg = stcMessage as NetMessage;
                if (!string.IsNullOrEmpty(msg.Topic) && topic != null && topic.Length > 0 && IsMatch(topic, msg.Topic)
                    && !string.IsNullOrEmpty(msg.Data))
                    t = MsgHelper.Deserialize<T>(msg.Data);//JsonConvert.DeserializeObject<T>(msg.Data);
            }

            return t;
        }

        public static bool IsMatch(string[] topic, string matchTopic)
        {
            bool result = false;
            foreach (string str in topic)
            {
                result = TopicUtils.matchTopic(str, matchTopic);
                if (result)
                    break;
            }

            return result;
        }
    }
}
