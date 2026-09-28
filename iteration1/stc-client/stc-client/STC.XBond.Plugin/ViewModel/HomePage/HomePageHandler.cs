using STC.XBond.Plugin.View.HomePage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.XBond.Plugin.ViewModel.HomePage
{
    public class HomePageHandler
    {
        private static HomePageHandler _homePageHandler = new HomePageHandler();
        public static HomePageHandler Instance { get { return _homePageHandler; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        private HomePageHandler()
        {
        }


        public void Instance_OnTabChange(object obj)
        {
            if (obj != null)
            {
                TabItem tabItem = obj as TabItem;
                if (tabItem != null)
                {
                        string securityId = tabItem.Header as string;

                    FavoriteBondsListBox tradeBookView = GetHomePageView();
                        tabItem.Content = tradeBookView;
                }
            }
        }

        private FavoriteBondsListBox favoriteBondsLB = null;// new FavoriteBondsListBox();


        public FavoriteBondsListBox GetHomePageView()
        {
            favoriteBondsLB.ClearView();
            
            return favoriteBondsLB;
        }
    }
}
