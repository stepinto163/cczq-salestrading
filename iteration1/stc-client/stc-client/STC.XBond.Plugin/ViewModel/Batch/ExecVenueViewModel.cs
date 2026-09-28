using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Order.InnerView;
using STC.Common.ViewModel.Order;
using STC.Common.ViewModel.Order.InnerView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.XBond.Plugin.ViewModel.Batch
{
    public class ExecVenueViewModel : ViewModelBase
    {
        private FrameworkElement parentUI;
        private FrameworkElement myWindow;


        #region 界面属性

        private List<ListBoxItemData> directionItems;
        public List<ListBoxItemData> DirectionItems
        {
            get
            {
                return directionItems;
            }

            set
            {
                directionItems = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility channelVisibility = Visibility.Visible;
        public Visibility ChannelVisibility
        {
            get { return channelVisibility; }
            set
            {
                channelVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private int selectVenueIndex = -1;
        public int SelectVenueIndex
        {
            get { return selectVenueIndex; }
            set
            {
                selectVenueIndex = value;

                this.OnPropertyChanged();
            }
        }


        private Visibility crossVisibility;
        public Visibility CrossVisibility
        {
            get { return crossVisibility; }
            set
            {
                crossVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility ocoVisibility;
        public Visibility OCOVisibility
        {
            get { return ocoVisibility; }
            set
            {
                ocoVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility sniperVisibility;
        public Visibility SniperVisibility
        {
            get { return sniperVisibility; }
            set
            {
                sniperVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private SelectionMode venueSelectionMode = SelectionMode.Extended;
        public SelectionMode VenueSelectionMode
        {
            get { return venueSelectionMode; }
            set
            {
                venueSelectionMode = value;

                this.OnPropertyChanged();
            }
        }

        private int selectTabIndex = 0;
        public int SelectTabIndex
        {
            get { return selectTabIndex; }
            set
            {
                selectTabIndex = value;
                this.OnPropertyChanged();

            }
        }

        #endregion

        public void initUI(string goodsType, FrameworkElement parentUI, FrameworkElement myWindow)
        {
            this.parentUI = parentUI;
            this.myWindow = myWindow;

            PermisionHelper.Instance.InitPermission(goodsType);

            DirectionItems = this.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, string.Empty));

            if (DirectionItems.Count == 1)
            {
                SelectVenueIndex = 0;
            }


            OCOView ocoView = parentUI.FindName("ocoView") as OCOView;
            (ocoView.DataContext as OCOViewModel).InitUI();

            SniperViewModel sniperView = parentUI.FindName("sniperView") as SniperViewModel;
            

            SetPermission();
        }


        public OCOViewModel GetOCOViewModel()
        {
            if (parentUI != null)
            {
                return (parentUI.FindName("ocoView") as OCOView).DataContext as OCOViewModel;
            }

            return null;
        }

        public SniperViewModel GetSniperViewModel()
        {
            if (parentUI != null)
            {
                return (parentUI.FindName("sniperView") as SniperView).DataContext as SniperViewModel;
            }

            return null;
        }

        public void onCheckChange(Dictionary<string, bool> dic)
        {
            bool isCheckInner = dic["inner"];
            OCOViewModel ocoViewModel = GetOCOViewModel();
            if (ocoViewModel != null)
            {
                if (isCheckInner)
                {
                    bool isShowXBond = (PermisionHelper.Instance.IsHaveAllPermission || PermisionHelper.Instance.IsHaveInnerXbondPermission) && PermisionHelper.Instance.IsHaveVenueXbondPermission;

                    if (isShowXBond)
                        ocoViewModel.CheckXBondVisibility = PermisionHelper.Instance.IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed;

                    if (DirectionItems != null)
                    {
                        foreach (ListBoxItemData itemData in DirectionItems)
                        {
                            itemData.ItemVisibility = Visibility.Visible;
                            itemData.ItemEnable = false;
                            itemData.Checked = false;

                        }
                    }

                }
                else
                {
                    bool isCheckXBond = dic["xbond"];

                    if (DirectionItems != null)
                    {
                        foreach (ListBoxItemData itemData in DirectionItems)
                        {
                            itemData.ItemVisibility = isCheckXBond ? Visibility.Collapsed : Visibility.Visible;

                            if (PermisionHelper.Instance.IsHaveInnerXbondPermission || PermisionHelper.Instance.IsHaveInnerPermission)
                            {
                                itemData.ItemEnable = false;

                                if (PermisionHelper.Instance.IsHaveInnerXbondPermission)
                                {
                                    if (itemData.Name.ToUpper().Equals(PermisionHelper.Instance.XBOND))
                                    {
                                        itemData.ItemEnable = true;
                                        itemData.ItemVisibility = Visibility.Visible;
                                        if (!itemData.ItemEnable)
                                            itemData.Checked = false;
                                        else
                                        {
                                            itemData.ItemEnable = true;
                                            itemData.Checked = isCheckXBond;

                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (itemData.Name.ToUpper().Equals(PermisionHelper.Instance.XBOND))
                                {
                                    itemData.ItemEnable = true;
                                    itemData.ItemVisibility = Visibility.Visible;
                                    if (!itemData.ItemEnable)
                                        itemData.Checked = false;
                                    else
                                    {
                                        itemData.ItemEnable = true;
                                        itemData.Checked = isCheckXBond;

                                    }
                                }
                                else
                                {
                                    itemData.ItemEnable = true;
                                    itemData.Checked = false;

                                }
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 设置功能权限
        /// </summary>
        public void SetPermission()
        {
            bool isAllow = PermisionHelper.Instance.IsHaveAllPermission;
            OCOViewModel ocoViewModel = GetOCOViewModel();
            if (isAllow)
            {
                if (PermisionHelper.Instance.IsHaveVenueInnerPermission)
                {
                    ocoViewModel.CheckInnerVisibility = Visibility.Visible;
                }
                else
                {
                    ocoViewModel.CheckInnerVisibility = Visibility.Collapsed;
                }


                ocoViewModel.CheckAllVisibility = Visibility.Visible;
                ocoViewModel.CheckInnerVisibility = Visibility.Visible;
                ocoViewModel.CheckInnerEnable = true;
                ocoViewModel.CheckAll = true;
                ocoViewModel.CheckInner = false;
                ocoViewModel.CheckXBond = false;

                ocoViewModel.SetCheckXBondVisibility(PermisionHelper.Instance.IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed);

                ocoViewModel.CheckXBondEnable = true;

            }
            else
            {
                ocoViewModel.SetCheckAllVisibility(Visibility.Collapsed);

                isAllow = PermisionHelper.Instance.IsHaveInnerPermission;
                if (isAllow)
                {
                    ocoViewModel.CheckInnerVisibility = Visibility.Visible;

                    ocoViewModel.CheckInner = true;
                    ocoViewModel.CheckInnerEnable = true;
                }
                else
                {
                    isAllow = PermisionHelper.Instance.IsHaveInnerXbondPermission;

                    ocoViewModel.SetCheckXBondVisibility(isAllow && PermisionHelper.Instance.IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed);

                    ocoViewModel.CheckInnerVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;

                    if (isAllow)
                    {
                        ocoViewModel.CheckXBond = true;
                        ocoViewModel.CheckXBondEnable = true;
                        ocoViewModel.CheckInner = false;
                        ocoViewModel.SetCheckXBondEnable(false);
                        ocoViewModel.CheckInnerEnable = true;
                        ocoViewModel.CheckNotoutEnable = true;
                    }
                }
            }

            

            setAlgoPermision();
        }


        private void setAlgoPermision()
        {
            bool isAllExec = PermisionHelper.Instance.isAllExec();
            if (isAllExec)
            {
                CrossVisibility = OCOVisibility = SniperVisibility = Visibility.Visible;

                SelectTabIndex = 0;
            }
            else if (PermisionHelper.Instance.isCross())
            {
                CrossVisibility = Visibility.Visible;
                OCOVisibility = Visibility.Collapsed;
                SniperVisibility = Visibility.Collapsed;
                SelectTabIndex = 0;
            }
            else if (PermisionHelper.Instance.isOCO())
            {
                CrossVisibility = Visibility.Collapsed;
                OCOVisibility = Visibility.Visible;
                SniperVisibility = Visibility.Collapsed;
                SelectTabIndex = 1;
            }
            else if (PermisionHelper.Instance.isSniper())
            {
                SetVenueItemEnable(false);
                CrossVisibility = Visibility.Collapsed;
                OCOVisibility = Visibility.Collapsed;
                SniperVisibility = Visibility.Visible;
                SelectTabIndex = 2;
            }
            else
            {
                CrossVisibility = OCOVisibility = SniperVisibility = Visibility.Collapsed;
            }
        }


        public List<ListBoxItemData> VenueToListBoxItemData(List<Venue> venues)
        {
            List<ListBoxItemData> result = null;

            if (venues != null && venues.Count > 0)
            {
                result = new List<ListBoxItemData>();
                foreach (Venue uv in venues)
                {
                    ListBoxItemData itemData = new ListBoxItemData();
                    itemData.Code = uv.venue;
                    itemData.Name = uv.name;
                    if (PermisionHelper.Instance.IsHaveAllPermission)
                    {
                        itemData.ItemVisibility = uv.isvisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                        itemData.ItemEnable = true;
                    }
                    else if (PermisionHelper.Instance.IsHaveInnerPermission)
                        itemData.ItemVisibility = Visibility.Collapsed;
                    else if (PermisionHelper.Instance.IsHaveInnerXbondPermission)
                    {
                        itemData.ItemVisibility = Visibility.Collapsed;
                        if (uv.name.Equals(PermisionHelper.Instance.XBOND))
                        {
                            itemData.ItemVisibility = Visibility.Visible;
                            itemData.ItemEnable = true;
                        }
                    }
                    if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                        itemData.Seq = int.Parse(SystemParamHandler.Instance.GetVenueSort(uv.venue));
                    else
                        itemData.Seq = uv.sn;

                    itemData.Level = uv.level;
                    itemData.Venuetypegw = uv.venuetypegw;

                    if (itemData.ItemVisibility == Visibility.Visible)
                        result.Add(itemData);

                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        public string GetSelectItem()
        {
            string result = string.Empty;
            if (parentUI != null)
            {
                ListBox listBox = parentUI.FindName("lbDirect") as ListBox;

                foreach (ListBoxItemData itemData in listBox.SelectedItems)
                {
                    if (itemData != null)
                    {
                        result += string.Format("{0}-{1},", itemData.Venuetypegw, itemData.Code);
                    }
                }
            }
            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
        }

        public void SetVenueItemEnable(bool isEnable)
        {
            //if (!PermisionHelper.Instance.IsHaveInnerXbondPermission)
            {
                if (DirectionItems != null)
                {
                    foreach (ListBoxItemData itemData in DirectionItems)
                    {
                        itemData.ItemEnable = isEnable;
                        if (!isEnable)
                            itemData.Checked = false;
                    }
                }
            }
        }

        public void SetVenueItemVisibility(Visibility visibility)
        {
            //if (!PermisionHelper.Instance.IsHaveInnerXbondPermission)
            {
                if (DirectionItems != null)
                {
                    foreach (ListBoxItemData itemData in DirectionItems)
                    {
                        itemData.ItemVisibility = visibility;
                    }
                }
            }
        }


        public void SetVenueItemEnable(bool isEnable, string venue)
        {
            //if (!PermisionHelper.Instance.IsHaveInnerXbondPermission)
            {
                if (DirectionItems != null)
                {
                    foreach (ListBoxItemData itemData in DirectionItems)
                    {
                        if (itemData.Name.ToUpper().Equals(venue.ToUpper()))
                        {
                            itemData.ItemEnable = isEnable;
                            if (!isEnable)
                                itemData.Checked = false;
                        }
                    }
                }
            }
        }
    }
}
