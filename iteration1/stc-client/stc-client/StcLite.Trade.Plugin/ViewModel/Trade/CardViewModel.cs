using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Trade;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class CardViewModel : BaseViewModel
    {
        private FrameworkElement _ownUI = null;
        private object _lock = new object();

        private string _currentGroupId = string.Empty;

        private ObservableCollection<CardItem> _cardItems = new ObservableCollection<CardItem>();
        public ObservableCollection<CardItem> CardItems { get { return _cardItems; } set { _cardItems = value; this.OnPropertyChanged(); } }

        private ICollectionView _collectionView = null;

        public ICollectionView CollectionView
        {
            get
            {
                if (_collectionView == null)
                    _collectionView = CollectionViewSource.GetDefaultView(CardItems);

                return _collectionView;
            }
        }


        #region 拖拽

        private string _dragTxt = string.Empty;

        public string DragTxt { get { return _dragTxt; } set { _dragTxt = value; this.OnPropertyChanged(); } }

        private CardItem _selectedListViewItem;

        public CardItem SelectedListViewItem
        {
            get { return _selectedListViewItem; }
            set
            {
                _selectedListViewItem = value;

                if (_selectedListViewItem.HomeGroupSecurityIDsInfo != null)
                    DragTxt = _selectedListViewItem.HomeGroupSecurityIDsInfo.remark;

            }
        }

        #endregion

        ~CardViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            BondChangeHandler.Instance.OnSelectSettleChange -= Instance_OnSelectSettleChange;
            HomePageDataHandler.Instance.OnFavoritesChanged -= Instance_OnFavoritesChanged;
            BondChangeHandler.Instance.OnSortChange -= Instance_OnSortChange;

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                CardItems.Clear();
            }));

            base.Dispose();
        }

        public void Init(string groupId, FrameworkElement ownUI)
        {
            _currentGroupId = groupId;
            _ownUI = ownUI;
            CardItems.Clear();

            CardItems.OrderBy(item => item.CustomSort);

            BondChangeHandler.Instance.OnSelectSettleChange += Instance_OnSelectSettleChange;
            HomePageDataHandler.Instance.OnFavoritesChanged += Instance_OnFavoritesChanged;

            BondChangeHandler.Instance.OnSortChange += Instance_OnSortChange;

            HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupId);
            if (homeGroupInfo != null && homeGroupInfo.securityids != null)
            {
                foreach (HomeGroupSecurityIDsInfo securityIDsInfo in homeGroupInfo.securityids)
                {
                    bool result = false;
                    foreach (CardItem item in CardItems)
                    {
                        if (item.Id == securityIDsInfo.id && (item.HomeGroupSecurityIDsInfo != null && item.HomeGroupSecurityIDsInfo.groupId == groupId))
                        {
                            result = true;
                            break;
                        }
                    }

                    if (!result)
                    {
                        CardItem cardItem = new CardItem();
                        cardItem.Security = securityIDsInfo.securityid;
                        cardItem.Id = securityIDsInfo.id;
                        cardItem.GroupId = securityIDsInfo.groupId;
                        cardItem.HomeGroupSecurityIDsInfo = securityIDsInfo;
                        
                        int sort = SortCache.Instance.GetSettleById(securityIDsInfo.groupId, securityIDsInfo.id);
                        if (sort >= 0)
                            cardItem.CustomSort = sort;
                        else
                            cardItem.CustomSort = CardItems.Count;

                        CalBaseData(cardItem);

                        CardItems.Add(cardItem);
                    }
                }

                CardItem newCardItem = new CardItem();
                newCardItem.GroupId = homeGroupInfo.id;
                newCardItem.CustomSort = CardItems.Count + 1;

                CardItems.Add(newCardItem);
            }

            CollectionView.SortDescriptions.Add(new SortDescription("CustomSort", ListSortDirection.Ascending));
        }

        

        private void CalBaseData(CardItem cardItem)
        {
            if (cardItem != null && !string.IsNullOrEmpty(cardItem.Security))
            {
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(cardItem.Security);
                if (bondsInfoPo != null)
                {
                    cardItem.RemaingingTimeConvert = CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);//剩余时间转换成时间段
                    //cardItem.BondType = CommonUtils.GetBondCategoryName(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);// 债券类型
                    cardItem.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                }
            }
        }

        private void Instance_OnSelectSettleChange(OperationType operationType, string groupId, object obj)
        {
            if (operationType == OperationType.CardToMain)
            {
                List<string> allSettle = new List<string>();
                ListView listView = _ownUI.FindName("FavoriteBondslistBox") as ListView;
                foreach (var item in listView.Items)
                {
                    ListViewItem viewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromItem(item);
                    if (viewItem != null)
                    {
                        ContentPresenter contentPresenter = CommonUtils.FindVisualChild<ContentPresenter>(viewItem);

                        if (contentPresenter != null)
                        {
                            DataTemplate myDataTemplate = contentPresenter.ContentTemplate;

                            CardPriceView cardPriceView = myDataTemplate.FindName("cpvCard", contentPresenter) as CardPriceView;

                            List<string> cardSettleList = cardPriceView.GetCurrentSettle();

                            foreach (string settle in cardSettleList)
                            {
                                if (!allSettle.Contains(settle))
                                    allSettle.Add(settle);
                            }

                            CardItem cardItem = cardPriceView.DataContext as CardItem;
                            if (cardItem != null && !string.IsNullOrEmpty(cardItem.Id) && !string.IsNullOrEmpty(cardItem.GroupId) && cardSettleList.Count > 0)
                            {
                                SettleCache.Instance.AddOrUpdate(cardItem.GroupId, cardItem.Id, cardSettleList[0]);
                            }

                        }
                    }
                }

                if (allSettle.Count > 0)
                {
                    //通知Main setle改变选中项
                    BondChangeHandler.Instance.OnMainSettleChange(OperationType.Main, this._currentGroupId, allSettle);

                    RememberHelper.Instance.WriteCustomSettle(_currentGroupId);
                }
            }
        }

        private void Instance_OnFavoritesChanged(FavoriteTypeEnum typeEnum, string groupID, string securityID, string bondId)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (_lock)
                {
                    if (_currentGroupId == groupID)
                    {
                        if (typeEnum == FavoriteTypeEnum.AddBond)
                        {
                            HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupID);
                            if (homeGroupInfo != null && homeGroupInfo.securityids != null)
                            {
                                foreach (HomeGroupSecurityIDsInfo info in homeGroupInfo.securityids)
                                {
                                    if (CardItems.Any(o => o.Id != null && o.Id.Equals(info.id)) == false)
                                    {
                                        CardItem newCardItem = new CardItem();
                                        newCardItem.Security = info.securityid;
                                        newCardItem.HomeGroupSecurityIDsInfo = info;//HomePageDataHandler.Instance.GetHomeGroupSecurityIDsInfo(groupID, bondId);
                                        newCardItem.Id = newCardItem.HomeGroupSecurityIDsInfo.id;
                                        newCardItem.GroupId = info.groupId;
                                        newCardItem.CustomSort = CardItems.Count;

                                        CalBaseData(newCardItem);

                                        CardItems.Add(newCardItem);
                                    }
                                }

                                int index = 0;
                                CardItem nullItem = null;
                                foreach (CardItem cardItem in CardItems)
                                {
                                    if (!string.IsNullOrEmpty(cardItem.Id))
                                        cardItem.CustomSort = index;
                                    else
                                        nullItem = cardItem;

                                    index++;
                                }

                                nullItem.CustomSort = index;
                            }

                        }
                        else if (typeEnum == FavoriteTypeEnum.DelBond)
                        {

                            CardItem removeItem = null;
                            foreach (CardItem cardItem in CardItems)
                            {
                                if (cardItem.GroupId == groupID && cardItem.Id == bondId)
                                {
                                    removeItem = cardItem;
                                    break;
                                }
                            }
                            if (removeItem != null)
                                CardItems.Remove(removeItem);
                        }
                        else if (typeEnum == FavoriteTypeEnum.AddBondFromCard)
                        {
                            HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupID);
                            if (homeGroupInfo != null)
                            {
                                CardItem newCardItem = new CardItem();
                                newCardItem.GroupId = homeGroupInfo.id;
                                newCardItem.CustomSort = CardItems.Count + 2;
                                //newCardItem.Security = securityID;

                                CalBaseData(newCardItem);

                                CardItems.Add(newCardItem);
                            }
                        }
                    }
                }
            }));
        }

        public void Drop(int prevRowIndex, int index, ListView dgData)
        {
            lock (_lock)
            {
                //ListCollectionView listCollectionView = CollectionView as ListCollectionView;
                //CardItem prevCard = listCollectionView.GetItemAt(prevRowIndex) as CardItem;
                //CardItem indexCard = listCollectionView.GetItemAt(index) as CardItem;

                //if (prevRowIndex > index)
                //    prevCard.CustomSort = index - 1;
                //else
                //    prevCard.CustomSort = index ;

                //CollectionView.SortDescriptions.Clear();
                //CollectionView.SortDescriptions.Add(new SortDescription("CustomSort", ListSortDirection.Ascending));

                //int flag = -1;
                //CardItem nullCard = null;
                //for (int i = 0; i < listCollectionView.Count; i++)
                //{
                //    CardItem item = listCollectionView.GetItemAt(i) as CardItem;
                //    if (!string.IsNullOrEmpty(item.Id))
                //    {
                //        flag++;
                //        item.CustomSort = flag;
                //        SortCache.Instance.AddOrUpdate(item.GroupId, item.Id, flag);
                //    }
                //    else
                //        nullCard = item;
                //}
                //nullCard.CustomSort = CardItems.Count;


                //CardItem nullInnerCard = null;
                //foreach (CardItem item in CardItems)
                //{
                //    if (!string.IsNullOrEmpty(item.Id))
                //    {
                //        int sort = SortCache.Instance.GetSettleById(item.GroupId, item.Id);
                //        item.CustomSort = sort;

                //        nullInnerCard = item;
                //    }
                //}
                //nullInnerCard.CustomSort = CardItems.Count;

                //dgData.SelectedIndex = index;

                //string sortStr = WriteCustomSort();

                //BondChangeHandler.Instance.OnSortChanged(OperationType.CardToTable, _currentGroupId);








                CardItem newItem = new CardItem();

                ListCollectionView listCollectionView = CollectionView as ListCollectionView;

                CardItem old = listCollectionView.GetItemAt(prevRowIndex) as CardItem;

                //CommonUtils.CopyCardItem(old, newItem);

                string removeKey = old.Id;

                CardItem removeItem = null;
                foreach (CardItem cardItem in CardItems)
                {
                    if (!string.IsNullOrEmpty(cardItem.Id) && cardItem.Id == removeKey)
                    {
                        removeItem = cardItem;
                    }
                }

                CardItems.Remove(removeItem);

                if (prevRowIndex > index)
                    newItem.CustomSort = index;
                else
                    newItem.CustomSort = index + 1;

                CardItems.Add(newItem);

                listCollectionView = CollectionView as ListCollectionView;

                int flag = -1;
                CardItem nullCard = null;
                for (int i = 0; i < listCollectionView.Count; i++)
                {
                    CardItem item = listCollectionView.GetItemAt(i) as CardItem;
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        flag++;
                        item.CustomSort = flag;
                        
                        SortCache.Instance.AddOrUpdate(item.GroupId, item.Id, flag);
                    }
                    else
                        nullCard = item;
                }
                nullCard.CustomSort = CardItems.Count;

                CardItem nullInnerCard = null;
                foreach (CardItem item in CardItems)
                {
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        int sort = SortCache.Instance.GetSettleById(item.GroupId, item.Id);
                        item.CustomSort = sort;

                        nullInnerCard = item;
                    }
                }
                nullInnerCard.CustomSort = CardItems.Count;

                dgData.SelectedIndex = index;

                string sortStr = RememberHelper.Instance.WriteCustomSort(_currentGroupId);//WriteCustomSort();

                BondChangeHandler.Instance.OnSortChanged(OperationType.CardToTable, _currentGroupId);
            }

        }

        private void Instance_OnSortChange(OperationType operationType, string groupdId)
        {
            if (groupdId == _currentGroupId && operationType == OperationType.TableToCard)
            {

                foreach (CardItem item in CardItems)
                {
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        int sort = SortCache.Instance.GetSettleById(item.GroupId, item.Id);
                        item.CustomSort = sort;
                    }
                }

                //ListCollectionView listCollectionView = CollectionView as ListCollectionView;

                //for (int i = 0; i < listCollectionView.Count; i++)
                //{
                //    CardItem item = listCollectionView.GetItemAt(i) as CardItem;

                //    if (!string.IsNullOrEmpty(item.Id))
                //    {
                //        int sort = SortCache.Instance.GetSettleById(item.GroupId, item.Id);
                //        item.CustomSort = sort;
                //    }
                //}

                CollectionView.SortDescriptions.Clear();
                CollectionView.SortDescriptions.Add(new SortDescription("CustomSort", ListSortDirection.Ascending));
            }
        }

        #region 本地缓存

        //private readonly string Sort_Section = "Sort";
        //private readonly string Sort_Group = "Group";

        //private string GetSectionKey()
        //{
        //    return string.Format("{0}-{1}", Sort_Section, LocalEnv.UserInfo.UserName);
        //}

        //private string GetSectionGroupKey()
        //{
        //    return string.Format("{0}-{1}", Sort_Group, _currentGroupId);
        //}

        //private string WriteCustomSort()
        //{
        //    string sectionKey = GetSectionKey();
        //    string sectionGroupKey = GetSectionGroupKey();

        //    StringBuilder sb = new StringBuilder();

        //    foreach (CardItem item in CardItems)
        //    {
        //        if (!string.IsNullOrEmpty(item.Id))
        //        {
        //            sb.Append(string.Format("{0}-{1},", item.Id, item.CustomSort));
        //        }
        //    }
            

        //    string result = sb.ToString();
        //    result = result.Length > 0 ? result.Substring(0, result.Length - 1) : string.Empty;
        //    IniFileHelper.WriteValue(sectionKey, sectionGroupKey, result);


        //    return result;
        //}

        //public void ReadCustomSort()
        //{
        //    string sectionKey = GetSectionKey();
        //    string sectionGroupKey = GetSectionGroupKey();

        //    string result = IniFileHelper.ReadValue(sectionKey, sectionGroupKey);

        //    List<string> sortList = result.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        //    foreach (string str in sortList)
        //    {
        //        string[] arr = str.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        //        int d = 0;
        //        if (arr != null && arr.Length > 1 && int.TryParse(arr[1], out d))
        //        {
        //            SortCache.Instance.AddOrUpdate(_currentGroupId, arr[0], d);
        //        }
        //    }
        //}

        private Dictionary<string, int> ParseSortStr(string sortStr)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            List<string> sortList = sortStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string str in sortList)
            {
                string[] arr = str.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                int d = 0;
                if (arr != null && arr.Length > 1 && int.TryParse(arr[1], out d))
                {
                    dic.Add(_currentGroupId + arr[0], d);
                }
            }

            return dic;
        }

        #endregion

        private Dictionary<string, IList> _conditionDic = new Dictionary<string, IList>();

        public void FilterData(Dictionary<string, IList> conditionDic)
        {
            _conditionDic = conditionDic;

            CollectionView.Filter = predicate();
        }

        private bool IsFilterBroker(Dictionary<string, IList> newDic)
        {
            bool result = false;



            return result;
        }

        private Predicate<object> predicate()
        {
            return new Predicate<object>(NeedShow);
        }

        private bool NeedShow(object item)
        {
            CardItem cardItem = item as CardItem;
            bool result = false;

            if (_conditionDic.ContainsKey(CommonConstants.SECURITYID))
            {
                IList securityList = _conditionDic[CommonConstants.SECURITYID];
                if (securityList.Count > 0)
                {
                    string securityId = securityList[0] as string;
                    if (cardItem.Security != securityId)
                        return result;
                }
            }
            else
            {
                result = true;
            }

            if (_conditionDic.ContainsKey(CommonConstants.REMAINING_TIME))
            {
                IList remaingTimeList = _conditionDic[CommonConstants.REMAINING_TIME];
                foreach (ListBoxItem lbi in remaingTimeList)
                {
                    if (lbi.Content.ToString() == CommonConstants.ALL)
                    {
                        result = true;

                        break;
                    }

                    if ((lbi.Content.ToString() == "0~1Y" && (cardItem.RemaingingTimeConvert > 0 && cardItem.RemaingingTimeConvert < 365))
                        || (lbi.Content.ToString() == "1~3Y" && (cardItem.RemaingingTimeConvert >= 365 && cardItem.RemaingingTimeConvert < (365 * 3)))
                        || (lbi.Content.ToString() == "3~5Y" && (cardItem.RemaingingTimeConvert >= (365 * 3) && cardItem.RemaingingTimeConvert < (365 * 5)))
                        || (lbi.Content.ToString() == "5~7Y" && (cardItem.RemaingingTimeConvert >= (365 * 5) && cardItem.RemaingingTimeConvert < (365 * 7)))
                        || (lbi.Content.ToString() == "7~10Y" && (cardItem.RemaingingTimeConvert >= (365 * 7) && cardItem.RemaingingTimeConvert < (365 * 10)))
                        || (lbi.Content.ToString() == ">10Y" && cardItem.RemaingingTimeConvert >= (365 * 10)))
                    {
                            result = true;
                            break;
                    }
                    else
                        result = false;
                }
            }
            else
            {
                result = true;
            }

            if (result)
            {
                if (_conditionDic.ContainsKey(CommonConstants.BOND_TYPE))
                {
                    IList bondList = _conditionDic[CommonConstants.BOND_TYPE];
                    foreach (ListBoxItem lbi in bondList)
                    {
                        if (lbi.Content.ToString() == CommonConstants.ALL)
                        {
                            result = true;

                            break;
                        }

                        if ((lbi.Content.ToString() == "国债" && cardItem.BondCategory == "A")
                            || (lbi.Content.ToString() == "央票" && cardItem.BondCategory == "B")
                            || (lbi.Content.ToString() == "金融债" && cardItem.BondCategory == "C")
                            || (lbi.Content.ToString() == "地方债" && cardItem.BondCategory == "D"))
                        {
                            result = true;

                            break;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }
            


            return result;
        }

    }


    public class CardItem : BaseViewModel
    {
        private string _id;
        public string Id { get { return _id; } set { _id = value; this.OnPropertyChanged(); } }

        private string _groupId;
        public string GroupId { get { return _groupId; } set { _groupId = value; this.OnPropertyChanged(); } }

        private string _security;
        public string Security { get { return _security; } set { _security = value; this.OnPropertyChanged(); } }

        private decimal _customSort;
        public decimal CustomSort { get { return _customSort; } set { _customSort = value; } }

        private string _bondType;
        public string BondType { get { return _bondType; } set { _bondType = value; this.OnPropertyChanged(); } }

        private string _bondCategory;
        public string BondCategory { get { return _bondCategory; } set { _bondCategory = value; this.OnPropertyChanged(); } }

        private int _remaingingTimeConvert;
        public int RemaingingTimeConvert { get { return _remaingingTimeConvert; } set { _remaingingTimeConvert = value; this.OnPropertyChanged(); } }

        private HomeGroupSecurityIDsInfo _homeGroupSecurityIDsInfo;
        public HomeGroupSecurityIDsInfo HomeGroupSecurityIDsInfo { get { return _homeGroupSecurityIDsInfo; } set { _homeGroupSecurityIDsInfo = value; this.OnPropertyChanged(); } }
    }
}
