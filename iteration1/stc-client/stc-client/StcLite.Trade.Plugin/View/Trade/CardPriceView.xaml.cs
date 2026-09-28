using com.gateway.log;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.ViewModel.Trade;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using STC.Common.Utils;

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// CardPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class CardPriceView : UserControl
    {
        public CardPriceView()
        {
            InitializeComponent();

            this.Loaded += CardPriceView_Loaded;

            this.Unloaded += CardPriceView_Unloaded;
        }

        private void CardPriceView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= CardPriceView_Unloaded;
            LogHelper.Info("CardPriceView_Unloaded");
            MarketOrderViewModel viewModel = dgMarket.DataContext as MarketOrderViewModel;
            if (viewModel != null)
                viewModel.Dispose();
        }

        private List<string> _cacheMainSettleList = null;
        public void SetMainSettleList(List<string> settleList)
        {
            _cacheMainSettleList = settleList;
        }

        private void CardPriceView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= CardPriceView_Loaded;

            CardItem cardItem = this.DataContext as CardItem;
            string security = cardItem.Security;

            dgPrice.Init(ViewModel.Data.TabShowModel.Card, security, _cacheMainSettleList, cardItem);

            if (string.IsNullOrEmpty(security))
            {
                dgPrice.SubscribeAddStatus(OnAddEnd);
                dgMarket.Visibility = Visibility.Collapsed;
            }
            else
                dgMarket.Init(ViewModel.Data.TabShowModel.Card, security);

            PriceViewModel priceViewModel = dgPrice.DataContext as PriceViewModel;
            if (MainConditionCache.Instance.IsContain(cardItem.GroupId))
            {
                Dictionary<string, IList> dic = MainConditionCache.Instance.GetConditionById(cardItem.GroupId) as Dictionary<string, IList>;
                
                if (dic != null && priceViewModel != null)
                {
                    
                    if (dic.ContainsKey(CommonConstants.SETTLE))
                    {
                        priceViewModel.Instance_OnSelectSettleChange(OperationType.Card, cardItem.GroupId, dic);
                    }
                    else
                    {
                        priceViewModel.Instance_OnSelectSettleChange(OperationType.RemerberToCard, cardItem.GroupId, dic);
                    }
                }
            }
            else
            {
                if (priceViewModel != null)
                {
                    priceViewModel.Instance_OnSelectSettleChange(OperationType.RemerberToCard, cardItem.GroupId, null);
                }
            }
        }

        private void OnAddEnd(string securityId)
        {
            dgMarket.Visibility = Visibility.Visible;
            dgMarket.Init(ViewModel.Data.TabShowModel.Card, securityId);
            dgPrice.UnSubscribeAddStatus(OnAddEnd);
        }


        public List<string> GetCurrentSettle()
        {
            List<string> result = dgPrice.GetCurrentSettle();

            return result;
        }


    }
}
