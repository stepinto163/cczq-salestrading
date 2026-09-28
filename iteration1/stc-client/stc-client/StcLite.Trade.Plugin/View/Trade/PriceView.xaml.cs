using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Component;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.View.Order;
using BDB.Trade.Plugin.ViewModel.Adapter;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade;
using STC.Common.Data;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Frame.CustomUI.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// PriceView.xaml 的交互逻辑
    /// </summary>
    public partial class PriceView : UserControl
    {
        private PriceViewModel _priceViewModel = null;
        private bool _isInit = false;
        
        public PriceView()
        {
            InitializeComponent();

            this.Loaded += PriceView_Loaded;

            this.Unloaded += PriceView_Unloaded;

            brokerFilter.SetSelectModel(SelectionMode.Single);
        }

        private void PriceView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= PriceView_Unloaded;
            if (_priceViewModel != null)
            {
                _priceViewModel.Dispose();
            }
        }

        ~PriceView()
        {
            if (_priceViewModel != null)
            {
                _priceViewModel.Dispose();
            }
        }

        public List<string> GetCurrentSettle()
        {
            return settleFilter.GetSelectContent();
        }
        
        public void OnSelectSettleChange(OperationType operationType, Dictionary<string, IList> obj)
        {
            if (_priceViewModel != null && _priceViewModel.CurrentCardItem != null)
                _priceViewModel.Instance_OnSelectSettleChange(operationType, _priceViewModel.CurrentCardItem.GroupId, obj);
        }

        public void SubscribeAddStatus(Action<string> action)
        {
            _priceViewModel.OnAddBondEnd += action;
        }

        public void UnSubscribeAddStatus(Action<string> action)
        {
            _priceViewModel.OnAddBondEnd -= action;
        }

        public void Init(TabShowModel tabShowModel, string security, List<string> firstSettleList)
        {
            _isInit = true;
            settleFilter.SetButtonProperty(23, 20);
            brokerFilter.SetButtonProperty(44, 18);

            _priceViewModel = new PriceViewModel();
            _priceViewModel.SetTabShowModel(tabShowModel);

            if (tabShowModel == TabShowModel.Card)
            {
                btnDelete.Visibility = Visibility.Visible;
            }
            else
                btnDelete.Visibility = Visibility.Collapsed;

            _priceViewModel.Security = security;

            CardItem cardItem = new CardItem();
            cardItem.Security = security;

            _priceViewModel.Init(this, new CardItem());

            this.DataContext = _priceViewModel;

            _isInit = false;

            if (tabShowModel == TabShowModel.Card)// && firstSettleList != null)
            {
                _priceViewModel.ChangeOweSettle(firstSettleList);
            }

            
        }

        public void Init(TabShowModel tabShowModel, string security, List<string> firstSettleList, CardItem cardItem)
        {
            _isInit = true;
            settleFilter.SetButtonProperty(23, 20);
            brokerFilter.SetButtonProperty(44, 18);

            _priceViewModel = new PriceViewModel();
            _priceViewModel.SetTabShowModel(tabShowModel);

            if (tabShowModel == TabShowModel.Card)
            {
                btnDelete.Visibility = Visibility.Visible;
            }
            else
                btnDelete.Visibility = Visibility.Collapsed;

            _priceViewModel.Security = security;

            _priceViewModel.Init(this, cardItem);

            this.DataContext = _priceViewModel;
            
            if (tabShowModel == TabShowModel.Card)// && firstSettleList != null)
            {
                _priceViewModel.ChangeOweSettle(firstSettleList);
            }

            _isInit = false;
        }



        private void PriceView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PriceView_Loaded;

            SetContextMenu();

            settleFilter.SetButtonVisibility(CommonConstants.NOW, Visibility.Collapsed);

            if (_priceViewModel != null && _priceViewModel.CurrentCardItem != null)
            {
                string cacheSettle = SettleCache.Instance.GetSettleById(_priceViewModel.CurrentCardItem.GroupId, _priceViewModel.CurrentCardItem.Id);
                if (!string.IsNullOrEmpty(cacheSettle))
                    settleFilter.SetItemSelect(cacheSettle);
            }
        }

        private void SettleFilterView_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (!_isInit && _priceViewModel != null)
            {
                Dictionary<string, IList> dic = PriceCalHelper.Instance.CopyDictionary(this.settleFilter.FilterDic);
                _priceViewModel.ProcessFilterCondition(dic);

                if (_priceViewModel.GetShowModel() == TabShowModel.Card 
                    && (_priceViewModel.CurrentOperationType != OperationType.Card && _priceViewModel.CurrentOperationType != OperationType.Table && _priceViewModel.CurrentOperationType != OperationType.RemerberToCard))
                {
                    BondChangeHandler.Instance.OnMainSettleChange(OperationType.CardToMain, _priceViewModel.CurrentCardItem.GroupId, null);

                }


                if ((_priceViewModel.CurrentOperationType != OperationType.Card && _priceViewModel.CurrentOperationType != OperationType.Table 
                    && _priceViewModel.CurrentOperationType != OperationType.TableChangeFocusRow && _priceViewModel.CurrentOperationType != OperationType.RemerberToCard)
                    && _priceViewModel.CurrentCardItem != null && !string.IsNullOrEmpty(_priceViewModel.CurrentCardItem.GroupId) && !string.IsNullOrEmpty(_priceViewModel.CurrentCardItem.Id))
                {
                    BondChangeHandler.Instance.OnOneSettleChanged(OperationType.CardToTable, _priceViewModel.CurrentCardItem.GroupId, _priceViewModel.CurrentCardItem.Id, GetSelectSettle(dic));

                }

                _priceViewModel.CurrentOperationType = OperationType.None;
            }
            
        }

        private List<string> GetSelectSettle(Dictionary<string, IList> filterDic)
        {
            List<string> result = new List<string>();
            if (filterDic.ContainsKey(CommonConstants.SETTLE))
            {
                IList settlTypeList = filterDic[CommonConstants.SETTLE];
                if (settlTypeList.Count > 0)
                {
                    foreach (ListBoxItem settlType in settlTypeList)
                    {
                        result.Add(settlType.Content.ToString());
                    }
                }
            }

            return result;
        }

        private void BrokerFilterView_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (_priceViewModel != null)
            {
                _priceViewModel.ProcessFilterCondition(this.brokerFilter.FilterDic);
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void DgBid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_priceViewModel != null)
                _priceViewModel.DataGridMouseDoubleClick();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ClickCount == 2)
            //{
            //    seSearch.SearchBox.Text = _priceViewModel.Security;


            //    seSearch.Visibility = Visibility.Visible;
            //    bdSecurity.Visibility = Visibility.Collapsed;

            //    seSearch.SetFocus();
            //}
        }

        private void SeSearch_SearchChanged(object sender, object e)
        {
            SeBondPo seBondPo = e as SeBondPo;

            if (_priceViewModel != null && seBondPo != null && !seBondPo.BondCode.Equals(_priceViewModel.Security))
            {
                HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                request.securityid = seBondPo.BondCode;
                request.symbol = seBondPo.ShortName;
                request.action = "1"; // 添加
                request.remark = $"{request.securityid}  {request.symbol}  {seBondPo.LeftTenor}";
                request.terminal = string.Format("STCLlient_v{0}", LocalEnv.ServerInfo.Version);
                request.remainingTime = seBondPo.LeftTenor;
                if (_priceViewModel != null && _priceViewModel.CurrentCardItem != null)
                    request.groupid = _priceViewModel.CurrentCardItem.GroupId;

                HomePageDataHandler.Instance.AddBond(request, null);
                //this._priceViewModel.Instance_OnSelectBondChange(BondUpdateType.ChangeFocusRow, seBondPo.BondCode, string.Empty, true);
            }
            else
            {
                _priceViewModel.AddBondVisibility = Visibility.Visible;
                _priceViewModel.BondGridVisibility = Visibility.Collapsed;
                _priceViewModel.SearchVisibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CardPriceView cardPriceView = CommonUtils.GetParentObject<CardPriceView>(btnDelete);

            if (cardPriceView != null)
            {
                CardItem cardItem = cardPriceView.DataContext as CardItem;

                string id = cardItem.Id;//cardItem.HomeGroupSecurityIDsInfo.id;
                string security = cardItem.Security;
                string text = string.Format("是否确定删除该债券({0})？", security);
                string title = "删除债券";
                if (CustomMessageBox.Question(text, title) == true)
                {
                    HomePageDataHandler.Instance.DelBond(Model.DisplayMode.Card, cardItem.GroupId, id, security);
                }
            }
        }

        private void DgBid_LostFocus(object sender, RoutedEventArgs e)
        {
            dgBid.UnselectAllCells();
        }

        // <summary>
        /// 只读行情，禁止点价
        /// </summary>
        public void RemoveDbDoubleClickEvent()
        {
            dgBid.MouseDoubleClick -= DgBid_MouseDoubleClick;
            btnDelete.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 选择 Settle
        /// </summary>
        /// <param name="content"></param>
        public void SetSettleSelect(string content)
        {
            settleFilter.SetItemSelect(content);
        }

        /// <summary>
        /// 选择 Broker
        /// </summary>
        /// <param name="content"></param>
        public void SetBrokerSelect(string content)
        {
            brokerFilter.SetItemSelect(content);
        }

        private void OrderMenuItme_Click(object sender, RoutedEventArgs e)
        {
            PriceMatchParam priceMatchParam = new PriceMatchParam();

            if (_isBid)
            {
                priceMatchParam.Side = "BUY";
                priceMatchParam.Price = _priceViewModel.BestBid;
                priceMatchParam.UpdataPrice = false;
                priceMatchParam.CustomFirmAmount = _priceViewModel.BestBidAmount;
            }
            else
            {
                priceMatchParam.Side = "SELL";
                priceMatchParam.Price = _priceViewModel.BestOfr;
                priceMatchParam.UpdataPrice = false;
                priceMatchParam.CustomFirmAmount = _priceViewModel.BestOfrAmount;
            }

            priceMatchParam.IsFirm = true;
            priceMatchParam.Symbol = _priceViewModel.Security;
            priceMatchParam.NonfirmTime = string.Empty;
            priceMatchParam.OperationType = "1";
            //设置默认结算方式
            List<string> settleList = settleFilter.GetSelectContent();
            priceMatchParam.SettlType = (settleList != null && settleList.Count > 0) ? settleList[0] : CommonConstants.ALL_PARTY;
            //priceMatchParam.NetPrice = GetNetPrice(drv);
            SendPriceViewLite view = new SendPriceViewLite(priceMatchParam);

            view.Show();
        }

        private void ShowSendPriceView(string side)
        {

        }

        ContextMenu contextMenu;

        private void SetContextMenu()
        {
            bool result = PermisionHandler.Instance.GetIdcTraderPermission();
            if (contextMenu == null && result)
            {
                contextMenu = (ContextMenu)this.FindResource("OrderContextMenu");

            }
        }

        private bool _isBid = false;

        private void BdBid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();

            _isBid = true;
            if (contextMenu != null)
            {
                contextMenu.IsOpen = true;
            }
        }

        private void BdOfr_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();

            _isBid = false;
            if (contextMenu != null)
            {
                contextMenu.IsOpen = true;
            }
        }
    }
}
