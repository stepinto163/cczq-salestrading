using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Order;
using STC.Common.View.Order.InnerView;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STC.Common.ViewModel.Order.InnerView
{
    public class CrossViewModel : BaseViewModel
    {

        private FrameworkElement parentUI;
        private FrameworkElement myWindow;

        #region 界面属性


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
            /*get { return sniperVisibility; }*/
            get { return Visibility.Collapsed; }
            set
            {
                sniperVisibility = value;

                this.OnPropertyChanged();
            }
        }


        private string bgImageSource;
        public string BgImageSource
        {
            get
            {
                return bgImageSource;
            }

            set
            {
                bgImageSource = value;

                this.OnPropertyChanged();

            }
        }


        private Brush bgImageBrush;
        public Brush BgImageBrush
        {
            get
            {
                return bgImageBrush;
            }

            set
            {
                bgImageBrush = value;

                this.OnPropertyChanged();

            }
        }



        private Brush itemHeadForeground;
        public Brush ItemHeadForeground
        {
            get { return itemHeadForeground; }
            set { itemHeadForeground = value; this.OnPropertyChanged(); }
        }

        private Brush bgBrush;
        public Brush BgBrush
        {
            get { return bgBrush; }
            set { bgBrush = value; this.OnPropertyChanged(); }
        }


        private Brush itemBgBrush;
        public Brush ItemBgBrush
        {
            get { return itemBgBrush; }
            set { itemBgBrush = value; this.OnPropertyChanged(); }
        }

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


        private SendPriceItem sendPriceItem = null;

        public SendPriceItem SendPriceItem
        {
            get { return sendPriceItem; }
            set
            {
                if (value != null)
                {
                    sendPriceItem = value;
                    this.OnPropertyChanged(nameof(SendPriceItem));
                }
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

                SendPriceViewModel sendPriceViewModel = getSendPriceViewModel();
                if (sendPriceViewModel != null)
                    ChangeBg(sendPriceViewModel.IsBid);

                if (selectTabIndex == 0)
                {
                    VenueSelectionMode = SelectionMode.Single;
                }
                else
                {
                    VenueSelectionMode = SelectionMode.Extended;
                }

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

        #endregion

        private void ChangeBg(bool isBid)
        {
            ImageBrush brush = new ImageBrush();
            string imageSource = string.Empty;
            if (selectTabIndex == 0)
            {
                imageSource = isBid ? "/STC.Common;component/Images/stc_cross_bid.png" : "/STC.Common;component/Images/stc_cross_ofr.png";
            }
            else if (selectTabIndex == 1)
            {
                if (PermisionHelper.Instance.isAllExec())
                {
                    imageSource = isBid ? "/STC.Common;component/Images/stc_oco_bid.png" : "/STC.Common;component/Images/stc_oco_ofr.png";
                }
                else if (PermisionHelper.Instance.isOCO())
                {
                    imageSource = isBid ? "/STC.Common;component/Images/stc_oco_bid_first.png" : "/STC.Common;component/Images/stc_oco_ofr_first.png";
                }
            }
            else if (selectTabIndex == 2)
            {
                if (PermisionHelper.Instance.isAllExec())
                {
                    imageSource = isBid ? "/STC.Common;component/Images/stc_sniper_bid.png" : "/STC.Common;component/Images/stc_sniper_ofr.png";
                }
                else if (PermisionHelper.Instance.isSniper())
                {
                    imageSource = isBid ? "/STC.Common;component/Images/stc_oco_bid_first.png" : "/STC.Common;component/Images/stc_oco_ofr_first.png";
                }
            }

            brush.ImageSource = new BitmapImage(new Uri(@imageSource, UriKind.Relative));

            BgImageBrush = brush;

            BgImageSource = imageSource;

        }


        // 渠道项(lbDirect ListBoxItem)的尺寸，需与 GreenBlueFilterListBoxItemStyle 保持一致
        private const double VenueItemWidth = 55 + 1;   // Width=55 + Margin.Left=1
        private const double VenueItemHeight = 24;      // Height=24
        // 渠道选择区可用宽度的兜底值，需与 CrossView.xaml 中 lbDirect 固定 Width(394) 一致：每行 7 个(7*56=392)+约 2px 间隙
        private const double DefaultVenueAreaWidth = 394;

        public void changeHeight()
        {
            if (parentUI != null)
            {
                int baseHeight;
                if (selectTabIndex == 0)
                {
                    baseHeight = 72;
                }
                else if (selectTabIndex == 1)
                {
                    baseHeight = 136;
                }
                else if (selectTabIndex == 2)
                {
                    baseHeight = 136;
                }
                else if (selectTabIndex == 3)
                {
                    baseHeight = 168;
                }
                else
                {
                    baseHeight = 72;
                }

                // 渠道项折行后，根据实际行数在基准高度上叠加多出的高度，并同步加高承载行/窗口
                int extraHeight = (GetVenueRowCount() - 1) * (int)VenueItemHeight;
                if (extraHeight < 0)
                    extraHeight = 0;

                int crossViewHeight = baseHeight + extraHeight;
                parentUI.Height = crossViewHeight;

                SendPriceViewModel sendPriceViewModel = getSendPriceViewModel();
                if (sendPriceViewModel != null)
                    sendPriceViewModel.SetChannelAreaHeight(crossViewHeight);
            }
        }

        /// <summary>
        /// 计算渠道选择区(lbDirect)中可见项按当前可用宽度折行后的行数。
        /// </summary>
        private int GetVenueRowCount()
        {
            if (ChannelVisibility != Visibility.Visible || DirectionItems == null)
                return 1;

            int visibleCount = DirectionItems.Count(x => x.ItemVisibility == Visibility.Visible);
            if (visibleCount <= 0)
                return 1;

            double areaWidth = DefaultVenueAreaWidth;
            ListBox listBox = parentUI != null ? parentUI.FindName("lbDirect") as ListBox : null;
            if (listBox != null && listBox.ActualWidth > VenueItemWidth)
                areaWidth = listBox.ActualWidth;

            int itemsPerRow = (int)(areaWidth / VenueItemWidth);
            if (itemsPerRow < 1)
                itemsPerRow = 1;

            return (int)Math.Ceiling((double)visibleCount / itemsPerRow);
        }

        public void changeUI(bool isBid)
        {
            if (isBid)
            {
                ItemBgBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange-10");
                BgBrush = (Brush)Application.Current.MainWindow.FindResource("30BidBtnGradientBrush");
                ItemHeadForeground = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");
            }
            else
            {
                ItemBgBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue-14");
                BgBrush = ItemBgBrush = (Brush)Application.Current.MainWindow.FindResource("30OfrBtnGradientBrush");
                ItemHeadForeground = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue-13");
            }

            ChangeBg(isBid);
        }

        public void RefreshVenue(string securityId)
        {
            string goodsType = CommonUtils.SetGoodsShortname(securityId);
            DirectionItems?.Clear();
            DirectionItems = this.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenue(goodsType, string.Empty));
            SetItemParam(DirectionItems, goodsType, "3");
        }

        public void initUI(string security, string goodsType, FrameworkElement parentUI, FrameworkElement myWindow, bool isBid)
        {
            this.parentUI = parentUI;
            this.myWindow = myWindow;

            DirectionItems = this.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenue(goodsType, string.Empty));

            SetItemParam(DirectionItems, goodsType, "3");

            changeUI(isBid);

            OCOView ocoView = parentUI.FindName("ocoView") as OCOView;
            (ocoView.DataContext as OCOViewModel).InitUI();

        }

        public bool isUpdate = false;
        /// <summary>
        /// 设置渠道相关的界面显示状态
        /// </summary>
        /// <param name="priceMatchParam"></param>
        public void SetChannel(PriceMatchParam priceMatchParam)
        {
            
            OCOViewModel oCOViewModel = GetOCOViewModel();
            SendPriceViewModel sendPriceViewModel = getSendPriceViewModel();
            if (!string.IsNullOrEmpty(priceMatchParam.CrossVenues))
            {
                if (priceMatchParam.CrossVenues.Equals("1"))
                {
                    oCOViewModel.CheckAll = true;
                }
                else if (priceMatchParam.CrossVenues.Equals("2"))
                {
                    oCOViewModel.CheckInner = true;
                    if (!priceMatchParam.IsFirm)
                        oCOViewModel.CheckXBondVisibility = Visibility.Collapsed;
                }
                else if (priceMatchParam.CrossVenues.Equals("3"))
                {
                    oCOViewModel.CheckXBond = true;
                }
            }

            if (!string.IsNullOrEmpty(priceMatchParam.VenuesSelectF) && priceMatchParam.VenuesSelectF.Equals("1"))
            {
                oCOViewModel.CheckNotOut = true;
            }

            //if (string.IsNullOrEmpty(priceMatchParam.VenuesSelectF) && string.IsNullOrEmpty(priceMatchParam.SelfSelectVenues))
            //{
            //    oCOViewModel.CheckNotOut = true;
            //}
            isUpdate = true;
            if (!string.IsNullOrEmpty(priceMatchParam.SendOrderType))
            {
                if (priceMatchParam.SendOrderType.Equals("直通"))
                {
                    SelectTabIndex = 0;
                }
                else if (priceMatchParam.SendOrderType.Equals("OCO"))
                {
                    if (PermisionHelper.Instance.isSniper())
                    {
                        SelectTabIndex = 2;
                    }
                    else if (PermisionHelper.Instance.isCross())
                    {
                        SelectTabIndex = 0;
                    }
                    else
                    {
                        SelectTabIndex = 1;
                    }
                }
                else
                {
                    SelectTabIndex = 1;
                }
            }


            if (!string.IsNullOrEmpty(priceMatchParam.SelfSelectVenues))
            {
                //ListBox listBox = myWindow.FindName("lbDirect") as ListBox;
                string[] arr = priceMatchParam.SelfSelectVenues.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                //foreach (ListBoxItemData itemData in listBox.SelectedItems)
                bool isContain = false;
                foreach (ListBoxItemData itemData in DirectionItems)
                {
                    isContain = false;

                    itemData.ItemEnable = true;

                    string tem = string.Format("{0}-{1}", itemData.Venuetypegw, itemData.Code);

                    foreach (string str in arr)
                    {
                        if (str.ToUpper().Equals(tem.ToUpper()))
                        {
                            itemData.Checked = true;
                            isContain = true;
                        }
                    }

                    if (!isContain)
                    {
                        itemData.Checked = false;
                    }

                    if (itemData.Name.ToUpper().Equals(PermisionHelper.Instance.XBOND))
                    {
                        itemData.ItemEnable = sendPriceViewModel.IsShowXBondItem();
                        if (!itemData.ItemEnable)
                            itemData.Checked = false;
                    }

                }
            }
        }


        /// <summary>
        /// operationType/venuesselectf: 1: 不发送外部 2:默认 3:指定
        /// </summary>
        /// <param name="itemDatas"></param>
        /// <param name="goodsType"></param>
        /// <param name="operationType"></param>
        private void SetItemParam(List<ListBoxItemData> itemDatas, string goodsType, string operationType)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                List<UserVenue> userItemDatas = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, operationType);

                foreach (ListBoxItemData item in itemDatas)
                {
                    item.ItemEnable = true;

                    foreach (UserVenue uv in userItemDatas)
                    {
                        if (!string.IsNullOrEmpty(uv.selfselectvenues) && uv.selfselectvenues.Contains(item.Code))
                        {
                            item.Checked = true;

                            break;
                        }

                        if (!string.IsNullOrEmpty(uv.venuesselectf) && uv.venuesselectf.Equals("2"))
                        {
                            SendPriceItem.RandomNum = uv.venuesrdmnum;
                        }
                    }
                }


                if (itemDatas.Count == 1)
                {
                    SelectVenueIndex = 0;
                    //itemDatas[0].Checked = true;
                }

            }
        }

        private SendPriceViewModel getSendPriceViewModel()
        {
            if (myWindow != null)
                return myWindow.DataContext as SendPriceViewModel;

            return null;
        }


        public void onCheckChange(Dictionary<string, bool> dic)
        {
            bool isCheckInner = dic["inner"];
            SendPriceViewModel sendPriceViewModel = getSendPriceViewModel();
            OCOViewModel ocoViewModel = GetOCOViewModel();
            if (ocoViewModel != null && sendPriceViewModel != null)
            {
                if (isCheckInner)
                {
                    bool isShowXBond = (PermisionHelper.Instance.IsHaveAllPermission || PermisionHelper.Instance.IsHaveInnerXbondPermission) && PermisionHelper.Instance.IsHaveVenueXbondPermission;

                    if (isShowXBond && !sendPriceViewModel.FirmChecked && !sendPriceViewModel.WholeFlagChecked
                        && ((sendPriceViewModel.SettleBoxIndex == 1
                            && !sendPriceViewModel.GetSendPriceModelAdapter().IsFwdSettle(sendPriceViewModel.FwdSelectDate, sendPriceViewModel.FwdComboBoxSelectIndex))
                                || (sendPriceViewModel.SettleBoxIndex == 0 || sendPriceViewModel.SettleBoxIndex == 1)))
                        ocoViewModel.CheckXBondVisibility = PermisionHelper.Instance.IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed;

                    if (!(ocoViewModel.CheckXBondVisibility == Visibility.Visible && isShowXBond))
                    {
                        sendPriceViewModel.AllowanceChannelVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        sendPriceViewModel.AllowanceChannelVisibility = Visibility.Visible;
                    }

                    if (DirectionItems != null)
                    {
                        foreach (ListBoxItemData itemData in DirectionItems)
                        {
                            itemData.ItemVisibility = Visibility.Visible;
                            if (selectTabIndex == 1 && "MANUAL".Equals(itemData.Code))
                            {
                                itemData.ItemVisibility = Visibility.Collapsed;
                            }
                            itemData.ItemEnable = false;
                            itemData.Checked = false;

                        }
                    }

                }
                else
                {
                    sendPriceViewModel.AllowanceChannelVisibility = Visibility.Visible;

                    bool isCheckXBond = dic["xbond"];

                    if (DirectionItems != null)
                    {
                        foreach (ListBoxItemData itemData in DirectionItems)
                        {
                            itemData.ItemVisibility = isCheckXBond ? Visibility.Collapsed : Visibility.Visible;

                            if (selectTabIndex == 1 && "MANUAL".Equals(itemData.Code))
                            {
                                itemData.ItemVisibility = Visibility.Collapsed;
                            }
                            if (PermisionHelper.Instance.IsHaveInnerXbondPermission || PermisionHelper.Instance.IsHaveInnerPermission)
                            {
                                itemData.ItemEnable = false;

                                if (PermisionHelper.Instance.IsHaveInnerXbondPermission)
                                {
                                    if (itemData.Name.ToUpper().Equals(PermisionHelper.Instance.XBOND))
                                    {
                                        itemData.ItemEnable = sendPriceViewModel.IsShowXBondItem();
                                        itemData.ItemVisibility = Visibility.Visible;
                                        if (!itemData.ItemEnable)
                                            itemData.Checked = false;
                                        else
                                        {
                                            if (sendPriceViewModel.FirmChecked || sendPriceViewModel.WholeFlagChecked)
                                            {
                                                itemData.ItemEnable = false;
                                            }
                                            else
                                            {
                                                if (this.IsSpot())
                                                {
                                                    itemData.ItemEnable = true;
                                                    itemData.Checked = isCheckXBond;
                                                }
                                                else
                                                {
                                                    itemData.ItemEnable = false;
                                                    itemData.Checked = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (itemData.Name.ToUpper().Equals(PermisionHelper.Instance.XBOND))
                                {
                                    itemData.ItemEnable = sendPriceViewModel.IsShowXBondItem();
                                    itemData.ItemVisibility = Visibility.Visible;
                                    if (!itemData.ItemEnable)
                                        itemData.Checked = false;
                                    else
                                    {
                                        if (sendPriceViewModel.FirmChecked || sendPriceViewModel.WholeFlagChecked)
                                        {
                                            itemData.ItemEnable = false;
                                        }
                                        else
                                        {
                                            if (this.IsSpot())
                                            {
                                                itemData.ItemEnable = true;
                                                itemData.Checked = isCheckXBond;
                                            }
                                            else
                                            {
                                                itemData.ItemEnable = false;
                                                itemData.Checked = false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (sendPriceViewModel.FirmChecked || sendPriceViewModel.WholeFlagChecked)
                                    {
                                        itemData.ItemEnable = false;
                                        itemData.Checked = false;
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


                SetPriceAndOrderHeight();
            }
        }



    private void SetPriceAndOrderHeight()
    {
        // 可见渠道项变化后，待布局完成再根据折行行数重新计算高度
        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
            changeHeight();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }


        public OCOViewModel GetOCOViewModel()
        {
            if (parentUI != null)
            {
                return (parentUI.FindName("ocoView") as OCOView).DataContext as OCOViewModel;
            }

            return null;
        }


        #region 获取界面设置渠道

        public string GetSingleSelectItem()
        {
            ListBox listBox = parentUI.FindName("lbDirect") as ListBox;

            string result = string.Empty;

            ListBoxItemData listBoxItemData = listBox.SelectedItem as ListBoxItemData;

            if (listBoxItemData != null)
            {
                result = listBoxItemData.Code + "-" + listBoxItemData.Venuetypegw;
            }

            return result;
        }


        public string GetSelectItem()
        {
            ListBox listBox = parentUI.FindName("lbDirect") as ListBox;

            string result = string.Empty;

            foreach (ListBoxItemData itemData in listBox.SelectedItems)
            {
                if (itemData != null)
                {
                    result += string.Format("{0}-{1},", itemData.Venuetypegw, itemData.Code);
                }
            }

            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
        }

        //xbond报价时，指定渠道固定获取XBOND
        public string GetXBondVenue(string xbond)
        {
            ListBox listBox = parentUI.FindName("lbDirect") as ListBox;
            string result = string.Empty;

            foreach (ListBoxItemData itemData in listBox.SelectedItems)
            {
                if (itemData != null && itemData.Name.ToUpper().Equals(xbond.ToUpper()))
                {
                    result += string.Format("{0}-{1},", itemData.Venuetypegw, itemData.Code);
                }
            }

            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
        }

        #endregion


        /// <summary>
        /// 设置功能权限
        /// </summary>
        public void SetPermission()
        {
            bool isAllow = PermisionHelper.Instance.IsHaveAllPermission;
            OCOViewModel ocoViewModel = GetOCOViewModel();
            SendPriceViewModel sendPriceViewModel = getSendPriceViewModel();
            if (isAllow)
            {
                ocoViewModel.CheckInnerVisibility = Visibility.Visible;
                ocoViewModel.SetCheckAllVisibility(Visibility.Visible);
                ocoViewModel.SetCheckXBondVisibility(Visibility.Visible);
                ocoViewModel.CheckInnerEnable = ocoViewModel.CheckXBondEnable = ocoViewModel.CheckNotoutEnable = true;


                if (!sendPriceViewModel.WholeFlagChecked && !sendPriceViewModel.FirmChecked)
                {
                    if (!ocoViewModel.CheckInner)
                    {
                        ocoViewModel.CheckAll = true;
                        ocoViewModel.CheckInner = false;
                    }

                    ocoViewModel.CheckXBond = false;
                }
                else
                {
                    ocoViewModel.CheckAllVisibility = Visibility.Collapsed;
                    ocoViewModel.CheckInnerVisibility = Visibility.Visible;
                    ocoViewModel.CheckInnerEnable = true;
                    ocoViewModel.CheckAll = false;
                    ocoViewModel.CheckInner = true;
                    ocoViewModel.CheckXBond = false;
                }
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
                SniperVisibility = CrossVisibility = OCOVisibility = Visibility.Visible;

                if (SelectTabIndex <= 0)
                    SelectTabIndex = 0;
            }

            if (PermisionHelper.Instance.isCross())
            {
                CrossVisibility = Visibility.Visible;
                ocoVisibility = Visibility.Collapsed;
                SniperVisibility = Visibility.Collapsed;
                SelectTabIndex = 0;
            }

            if (PermisionHelper.Instance.isOCO())
            {
                OCOVisibility = Visibility.Visible;
                CrossVisibility = Visibility.Collapsed;
                SniperVisibility = Visibility.Collapsed;
                SelectTabIndex = 1;
            }
            
            if (PermisionHelper.Instance.isSniper())
            {
                OCOVisibility = Visibility.Collapsed;
                CrossVisibility = Visibility.Collapsed;
                SniperVisibility = Visibility.Visible;

                SelectTabIndex = 2;
            }
            
            //如果都没有
            if (!isAllExec && !PermisionHelper.Instance.isCross() && !PermisionHelper.Instance.isOCO() && !PermisionHelper.Instance.isSniper())
            {
                SniperVisibility = CrossVisibility = OCOVisibility = Visibility.Collapsed;

                ChannelVisibility = Visibility.Collapsed;

                SelectTabIndex = 0;
            }

            if (!PermisionHelper.Instance.IsHaveAllPermission && !PermisionHelper.Instance.IsHaveInnerPermission && !PermisionHelper.Instance.IsHaveInnerXbondPermission)
            {
                ChannelVisibility = Visibility.Collapsed;
            }


            changeHeight();
        }

        #region 渠道设置显示

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
                        itemData.ItemVisibility = uv.isvisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    else if (PermisionHelper.Instance.IsHaveInnerPermission)
                        itemData.ItemVisibility = Visibility.Collapsed;
                    else if (PermisionHelper.Instance.IsHaveInnerXbondPermission)
                    {
                        itemData.ItemVisibility = Visibility.Collapsed;
                        if (uv.name.Equals(PermisionHelper.Instance.XBOND))
                            itemData.ItemVisibility = Visibility.Visible;
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

        public List<ListBoxItemData> UserVenueToListBoxItemData(List<UserVenue> userVenues, string level)
        {
            List<ListBoxItemData> result = null;

            if (userVenues != null && userVenues.Count > 0)
            {
                result = new List<ListBoxItemData>();
                foreach (UserVenue uv in userVenues)
                {
                    string[] arr = uv.selfselectvenues.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in arr)
                    {
                        ListBoxItemData itemData = new ListBoxItemData();
                        itemData.Code = str;
                        Venue venue = SystemParamHandler.Instance.GetVenueByCondition(uv.type, str);
                        if (venue != null)
                        {
                            itemData.Name = venue.name;
                            itemData.Seq = venue.sn;
                            itemData.Level = venue.level;
                            //itemData.SendPriceItem = sendPriceItem;
                            itemData.Venuetypegw = venue.venuetypegw;

                            if (string.IsNullOrEmpty(level) || (!string.IsNullOrEmpty(level) && itemData.Level.ToString().Equals(level)))
                                result.Add(itemData);
                        }
                    }
                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        #endregion

        public bool IsSpot()
        {
            SendPriceViewModel sendPriceViewModel = getSendPriceViewModel();
            if (sendPriceViewModel != null)
            {
                bool isSpot = sendPriceViewModel.GetSendPriceModelAdapter().IsSpotSettle(sendPriceViewModel.FwdSelectDate,
                    sendPriceViewModel.FwdComboBoxSelectIndex);

                if (!isSpot)
                {
                    if (sendPriceViewModel.SettleBoxIndex == 0 
                        || ((sendPriceViewModel.SettleBoxIndex2 == 1 || sendPriceViewModel.SettleBoxIndex2 == 0) && sendPriceViewModel.SettleBoxIndex == -1))
                        isSpot = true;
                        
                }

                return isSpot;
            }

            return false;
        }

        public bool IsNoNFIRMOrWholeFlag()
        {
            SendPriceViewModel sendPriceViewModel = getSendPriceViewModel();
            if (sendPriceViewModel != null)
            {
                return sendPriceViewModel.WholeFlagChecked || sendPriceViewModel.FirmSelectIndex == 1;
            }

            return false;
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

        public void SetVenueItemEnable(bool isEnable, string venue, bool isChecked = false)
        {
            //if (!PermisionHelper.Instance.IsHaveInnerXbondPermission)
            {
                if (DirectionItems != null)
                {
                    foreach (ListBoxItemData itemData in DirectionItems)
                    {
                        if (itemData.Name.ToUpper().Equals(venue.ToUpper()) || itemData.Venuetypegw.ToUpper().Equals(venue.ToUpper()))
                        {
                            itemData.ItemEnable = isEnable;
                            if (isChecked)
                                itemData.Checked = true;
                            else
                                itemData.Checked = false;

                            if (!isEnable)
                                itemData.Checked = false;

                            if (!isEnable && itemData.Name.ToUpper().Equals(PermisionHelper.Instance.XBOND))
                            {
                                GetOCOViewModel().CheckXBondEnable = false;
                                GetOCOViewModel().CheckXBond = false;
                            }
                        }
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
                        if (selectTabIndex == 1 && "MANUAL".Equals(itemData.Code)) 
                        {
                            itemData.ItemVisibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }


        public bool CheckVenuePermission(string venue)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(venue) && DirectionItems != null && DirectionItems.Count > 0)
            {
                foreach (ListBoxItemData listBoxItemData in DirectionItems)
                {
                    if (listBoxItemData.Name.ToUpper() == (venue.ToUpper()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
                result = true;

            return result;
        }
    }
}
