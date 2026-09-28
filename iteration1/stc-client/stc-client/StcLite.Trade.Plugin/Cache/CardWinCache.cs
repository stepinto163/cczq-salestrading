using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.Cache
{
    public class CardWinCache
    {
        private CardWinCache() { }
        private static CardWinCache server = new CardWinCache();


        public static CardWinCache Instance { get { return server; } }

        private Dictionary<string, object> cardDic = new Dictionary<string, object>();


        public void AddOrUpdate(string security, object obj)
        {
            if (cardDic.ContainsKey(security))
            {
                cardDic[security] = obj;
            }
            else
            {
                cardDic.Add(security, obj);
            }
        }


        public object GetBySecurity(string security)
        {
            if (cardDic.ContainsKey(security))
            {
                return cardDic[security];
            }

            return null;
        }

        public void DeleteBySecurity(string security)
        {
            if (cardDic.ContainsKey(security))
            {
                cardDic.Remove(security);
            }
        }
    }
}
