using com.gateway.connector.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public class TopicUtils
    {
        public const string WILDCARD = "*";
        public const string ALL_WILDCARD = "**";
        public static bool isWildcard(string topic)
        {
            if (topic.IndexOf(WILDCARD) == -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        //判断地址是否符合
        public static bool matchTopic(System.String szWildDest, System.String szDest)
        {
            if (szWildDest.Equals(szDest))
            {
                return true;
            }

            if (!isWildcard(szWildDest))
            {
                return false;
            }


            bool bMatched = false;





            if (szWildDest.Equals(ALL_WILDCARD))
            {
                return true;
            }


            StringTokenizer wildPart = new StringTokenizer(szWildDest, '.');
            StringTokenizer destPart = new StringTokenizer(szDest, '.');

            int tokenCountDiff = destPart.countTokens() - wildPart.countTokens();


            if ((tokenCountDiff == 0) || (tokenCountDiff == -1) || szWildDest.IndexOf(ALL_WILDCARD) >= 0)
            {
                while (wildPart.hasMoreElements() && destPart.hasMoreElements())
                {
                    System.String szWildDescPart = (System.String)wildPart.nextElement();
                    System.String szDescPart = (System.String)destPart.nextElement();

                    if (szWildDescPart.Equals(WILDCARD))
                    //通配符*
                    {
                        bMatched = true;
                        continue;
                    }
                    else if (szWildDescPart.Equals(ALL_WILDCARD))
                    //全配符**
                    {
                        bMatched = true;
                        break;
                    }
                    else if (szWildDescPart.Equals(szDescPart))
                    //此部分一致
                    {
                        bMatched = true;
                        continue;
                    }
                    else
                    {
                        bMatched = false;
                        break;
                    }
                }
            }

            wildPart = null;
            destPart = null;

            return bMatched;
        }
    }
}
