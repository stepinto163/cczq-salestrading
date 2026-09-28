using com.gateway.log;
using Newtonsoft.Json;
using STC.Broker.Plugin.Model;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STC.Common.Utils.HomePageDataHandler;
namespace STC.Broker.Plugin.ViewModel.UI
{
    public class QuoteConfigHandler
    {
        
        private Action<FavoriteTypeEnum, string, string, CardItem> _OnBondsChanged;
        private Action<object> _BaseDataUpdated;

        

        private readonly Object m_OnFavoriteEventLock = new Object();
        /// <summary>
        /// 债券修改变化事件
        /// </summary>
        public event Action<FavoriteTypeEnum, string, string, CardItem> OnBondsChanged
        {
            add
            {
                lock (m_OnFavoriteEventLock)
                {
                    _OnBondsChanged += value;
                }
            }
            remove
            {
                lock (m_OnFavoriteEventLock)
                {
                    _OnBondsChanged -= value;
                }
            }
        }


        /// <summary>
        /// 基础数据变化事件
        /// </summary>
        public event Action<object> BaseDataUpdated
        {
            add
            {
                lock (m_OnFavoriteEventLock)
                {
                    _BaseDataUpdated += value;
                }
            }
            remove
            {
                lock (m_OnFavoriteEventLock)
                {
                    _BaseDataUpdated -= value;
                }
            }
        }



        ~QuoteConfigHandler()
        {
            
        }

        private static QuoteConfigHandler quoteConfigHandler;
        public static QuoteConfigHandler Instance
        {
            get
            {
                if (quoteConfigHandler == null)
                {
                    quoteConfigHandler = new QuoteConfigHandler();
                }
                return quoteConfigHandler;
            }
        }


        public void Notify(FavoriteTypeEnum favoriteTypeEnum, string groupid, string securtityID, CardItem cardItem)
        {
            _OnBondsChanged?.Invoke(favoriteTypeEnum, groupid, securtityID, cardItem);
        }


        public void TriggerBaseDataUpdated(object t)
        {
            _BaseDataUpdated?.Invoke(t);
        }
    }
}
