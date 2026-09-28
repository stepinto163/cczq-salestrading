using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.IRS.Plugin.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace STC.IRS.Plugin.View.Component
{
    /// <summary>
    /// InnerBoardFilterPanel_New.xaml 的交互逻辑
    /// </summary>
    public partial class InnerBoardFilterPanel_New : UserControl
    {
        ///// <summary>
        ///// 票面筛选事件
        ///// </summary>
        //public static readonly RoutedEvent CouponActionEvent = EventManager.RegisterRoutedEvent("CouponAction", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InnerBoardFilterPanel_New));

        //public event RoutedEventHandler CouponAction
        //{
        //    add { AddHandler(CouponActionEvent, value); }
        //    remove { RemoveHandler(CouponActionEvent, value); }
        //}

        public bool IsPrice
        {
            get { return (bool)GetValue(IsPriceProperty); }
            set { SetValue(IsPriceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPrice.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPriceProperty =
            DependencyProperty.Register("IsPrice", typeof(bool), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(false));
        

        public string StartPrice
        {
            get { return (string)GetValue(StartPriceProperty); }
            set { SetValue(StartPriceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTermDY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartPriceProperty =
            DependencyProperty.Register("StartPrice", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, onStartPriceChanged));

        private static void onStartPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            source.SPrice.Text = e.NewValue == null ? string.Empty : e.NewValue.ToString();
        }


        public string EndPrice
        {
            get { return (string)GetValue(EndPriceProperty); }
            set { SetValue(EndPriceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTermDY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndPriceProperty =
            DependencyProperty.Register("EndPrice", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, onEndPriceChanged));

        private static void onEndPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            source.EPrice.Text = e.NewValue == null ? string.Empty : e.NewValue.ToString();
        }

        /// <summary>
        /// 保存事件
        /// </summary>
        public static readonly RoutedEvent SaveActionEvent = EventManager.RegisterRoutedEvent("SaveAction", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InnerBoardFilterPanel_New));
        
        public event RoutedEventHandler SaveAction
        {
            add { AddHandler(SaveActionEvent, value); }
            remove { RemoveHandler(SaveActionEvent, value); }
        }

        /// <summary>
        /// 编辑事件
        /// </summary>
        public static readonly RoutedEvent EditActionEvent = EventManager.RegisterRoutedEvent("EditAction", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InnerBoardFilterPanel_New));

        public event RoutedEventHandler EditAction
        {
            add { AddHandler(EditActionEvent, value); }
            remove { RemoveHandler(EditActionEvent, value); }
        }

        public static string ALL = "0000";
        public static string DEFAULT_FOCUSTAGS = string.Empty;//$"{{\"{Constants.ROLETYPE}\":[],\"{Constants.SIDE}\":[],\"{Constants.BONDTYPE}\":[\"0000\",\"0001\",\"0010\",\"0011\",\"0100\"],\"{Constants.ORGTYPE}\":[],\"{Constants.TERM}\":[\"0000\",\"0001\",\"0010\",\"0011\",\"0100\",\"0101\",\"0110\",\"0111\",\"1000\"],\"{Constants.VIP}\":[],\"{Constants.FIRM}\":[],\"{Constants.SETTLTYPE}\":[],\"{Constants.FINBONDTYPE}\":[\"0000\",\"0001\",\"0010\",\"0011\"],\"{Constants.DQTYPE}\":[\"0000\",\"0001\",\"0010\"],\"{Constants.LOCAL_GOV_BOND_TYPE}\":[],\}}";


        private Dictionary<string, List<string>> _focusTagsDic = new Dictionary<string, List<string>>();

        public string FocusTags
        {
            get { return (string)GetValue(FocusTagsProperty); }
            set
            {
                SetValue(FocusTagsProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for FocusTags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusTagsProperty =
            DependencyProperty.Register("FocusTags", typeof(string), typeof(InnerBoardFilterPanel_New), 
                new PropertyMetadata(string.Empty, OnFocusTagsPropertyChangedCallback));

        private bool isUpdate = false;

        public bool IsMe
        {
            get { return (bool)GetValue(IsMeProperty); }
            set { SetValue(IsMeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMe.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMeProperty =
            DependencyProperty.Register("IsMe", typeof(bool), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(false));

        // isMe复选框的Visibility属性
        public Visibility Visibility4IsMe
        {
            get { return (Visibility)GetValue(Visibility4IsMePropertyProperty); }
            set { SetValue(Visibility4IsMePropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visibility4IsMePropertyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Visibility4IsMePropertyProperty =
            DependencyProperty.Register("Visibility4IsMe", typeof(Visibility), typeof(InnerBoardFilterPanel_New),
                new PropertyMetadata(Visibility.Collapsed, onVisibility4IsMe));

        private static void onVisibility4IsMe(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            InnerBoardFilterPanel_New source = (InnerBoardFilterPanel_New)sender;
            source.isMeCB.Visibility = source.savebtn.Visibility = source.editbtn.Visibility = (Visibility)args.NewValue;
        }

        public bool HasPrice
        {
            get { return (bool)GetValue(HasPriceProperty); }
            set { SetValue(HasPriceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasPrice.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasPriceProperty =
            DependencyProperty.Register("HasPrice", typeof(bool), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(false, OnHasPriceChanged));

        private static void OnHasPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            InnerBoardFilterPanel_New source = (InnerBoardFilterPanel_New)d;
            source.hasPriceChk.IsChecked = (bool)e.NewValue;
        }

        public Visibility Visibility4All
        {
            get { return (Visibility)GetValue(Visibility4AllProperty); }
            set { SetValue(Visibility4AllProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visibility4All.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Visibility4AllProperty =
            DependencyProperty.Register("Visibility4All", typeof(Visibility), typeof(InnerBoardFilterPanel_New), 
                new PropertyMetadata(Visibility.Collapsed, onVisibility4AllChanged));

        private static void onVisibility4AllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            InnerBoardFilterPanel_New source = (InnerBoardFilterPanel_New)d;
            source.hasPriceChk.Visibility = (Visibility)e.NewValue;
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, OnSearchTextChanged));

        private static void OnSearchTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            InnerBoardFilterPanel_New source = (InnerBoardFilterPanel_New)sender;
            source.AdjustShowAll_ToggleButtonCheck();
        }


        public double FixedWidth
        {
            get { return (double)GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FixedWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FixedWidthProperty =
            DependencyProperty.Register("FixedWidth", typeof(double), typeof(InnerBoardFilterPanel_New), new PropertyMetadata((double)1125, OnFixedWidthChanged));

        private static void OnFixedWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = (InnerBoardFilterPanel_New)d;
            source.row1.Width = (double)e.NewValue;
            source.row2.Width = (double)e.NewValue;
            //source.row3.Width = (double)e.NewValue;
        }

        /// <summary>
        /// 是否显示保存编辑按钮
        /// </summary>
        public Visibility IsShowSaveEdit
        {
            get { return (Visibility)GetValue(IsShowSaveEditProperty); }
            set { SetValue(IsShowSaveEditProperty, value); }
        }

        public static readonly DependencyProperty IsShowSaveEditProperty =
            DependencyProperty.Register("IsShowSaveEdit", typeof(Visibility), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(Visibility.Visible, IsShowSaveEditChanged));

        public static void IsShowSaveEditChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            InnerBoardFilterPanel_New source = (InnerBoardFilterPanel_New)sender;
            source.savebtn.Visibility = source.editbtn.Visibility = (Visibility)args.NewValue;
        }




        public static void OnFocusTagsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            InnerBoardFilterPanel_New source = (InnerBoardFilterPanel_New)sender;

            if (source.isUpdate == true)
            {
                return;
            }

            try
            {
                string ft = JsonConvert.SerializeObject(source._focusTagsDic);
                if (args.NewValue == args.OldValue || source.FocusTags == ft) return;
                
                if (args.NewValue == null) return;

                source.isUpdate = true;
                if (!string.IsNullOrEmpty(source.FocusTags))
                {
                    Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(source.FocusTags);
                    if (dic != null && dic.Count > 0)
                    {
                        foreach (KeyValuePair<string, List<string>> pair in dic)
                        {
                            if (pair.Key == Constants.IRS_PRODUCT_TYPE)
                            {
                                (source.CtrlDic[Constants.IRS_PRODUCT_TYPE_BASIS] as FilterOptions).InitData(pair.Value);
                                (source.CtrlDic[Constants.IRS_PRODUCT_TYPE_OUTRIGHT] as FilterOptions).InitData(pair.Value);
                                (source.CtrlDic[Constants.IRS_PRODUCT_TYPE_SPREAD] as FilterOptions).InitData(pair.Value);
                            }
                            else if (pair.Key == Constants.VIP)
                            {
                                if (pair.Value != null && pair.Value.Count > 0 && pair.Value.Contains("0001"))
                                {
                                    source.vipCB.IsChecked = true;
                                }
                                else
                                {
                                    source.vipCB.IsChecked = false;
                                }
                            }
                            else if (pair.Key == Constants.DQTYPE)
                            {
                                if (pair.Value != null && pair.Value.Count > 0 && !pair.Value.Contains("0001"))
                                {
                                    source.isMMNoReplyCB.IsChecked = true;
                                }
                                else
                                {
                                    source.isMMNoReplyCB.IsChecked = false;
                                }
                            }
                            else
                            {
                                if (source.CtrlDic.ContainsKey(pair.Key))
                                {
                                    if (source.CtrlDic[pair.Key] is MultiSelectComboBox)
                                    {
                                        source.SetMultiComboBox((source.CtrlDic[pair.Key] as MultiSelectComboBox), pair.Value);
                                    }
                                    else if (source.CtrlDic[pair.Key] is FilterOptions)
                                    {
                                        (source.CtrlDic[pair.Key] as FilterOptions).InitData(pair.Value);
                                    }
                                    else if (source.CtrlDic[pair.Key] is HierarchicalComboBox)
                                    {
                                        source.SetHierachicalComboBox(source.CtrlDic[pair.Key] as HierarchicalComboBox, pair.Value);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ClearSelected(source);
                    }
                }
                else
                {
                    ClearSelected(source);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                ClearSelected(source);
            }
            finally
            {
                source.isUpdate = false;
            }
        }

        private void SetMultiComboBox(MultiSelectComboBox cb, List<string> tagList)
        {
            StringBuilder sb = new StringBuilder();
            if (tagList == null || tagList.Count == 0)
            {
                cb.SelectedValues = string.Empty;
            }
            else
            {
                foreach (string value in tagList)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(CommonConstants.COMMA_CHAR);
                    }
                    sb.Append(value);
                }
                cb.SelectedValues = sb.ToString();
            }
        }

        private void SetHierachicalComboBox(HierarchicalComboBox cb, List<string> tagList)
        {
            StringBuilder sb = new StringBuilder();
            if (tagList == null || tagList.Count == 0)
            {
                cb.SelectedValues = string.Empty;
            }
            else
            {
                foreach (string value in tagList)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(CommonConstants.COMMA_CHAR);
                    }
                    sb.Append(value);
                }
                cb.SelectedValues = sb.ToString();
            }
        }

        private static void ClearSelected(InnerBoardFilterPanel_New source)
        {
            if (source == null)
                return;

            source.ShowAll_ToggleButton.IsChecked = true;
        }

        private static void Sub_ClearSelected(ListBox listBox)
        {
            foreach (ListBoxItem item in listBox.Items)
            {
                if (item.Tag != null)
                    item.IsSelected = false;
            }
        }

        private void SetFocusTagDic(string key, IList listBoxItems)
        {
            if (listBoxItems != null && !string.IsNullOrEmpty(key))
            {
                if (!this._focusTagsDic.ContainsKey(key))
                {
                    this._focusTagsDic[key] = new List<string>();
                }
                //清空
                this._focusTagsDic[key].Clear();
                //添加
                foreach (System.Windows.Controls.ListBoxItem item in listBoxItems)
                {
                    this._focusTagsDic[key].Add(item.Tag.ToString());
                }

                if (!isUpdate)
                {
                    FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
                }
            }
        }

        Dictionary<string, UserControl> CtrlDic = new Dictionary<string, UserControl>();

        public InnerBoardFilterPanel_New()
        {
            InitializeComponent();

            //CtrlDic[Constants.IRS_TYPE] = this.type1;
            CtrlDic[Constants.IRS_PRODUCT_TYPE_BASIS] = this.product_basis;
            CtrlDic[Constants.IRS_PRODUCT_TYPE_OUTRIGHT] = this.product_outright;
            CtrlDic[Constants.IRS_PRODUCT_TYPE_SPREAD] = this.product_spread;
            //CtrlDic[Constants.IRS_PRODUCT_TYPE] = this.product;
            CtrlDic[Constants.IRS_TERM] = this.term;
            CtrlDic[Constants.SIDE] = this.side;
            CtrlDic[Constants.FIRM] = this.firmflag;
            CtrlDic[Constants.ORGTYPE] = this.OrgCbb;
            CtrlDic[Constants.ROLETYPE] = this.business;

            ObservableCollection<OptionItem> listSide = new ObservableCollection<OptionItem>();
            listSide.Add(new OptionItem("Bid", "0001"));
            listSide.Add(new OptionItem("Ofr", "0010"));
            this.side.ItemsSource = listSide;//side
            //GenDefaultFocusTags();
            InitOrgType();
            ShowAll_ToggleButton.Checked += ShowAll_ToggleButton_Checked;

            FocusTags = DEFAULT_FOCUSTAGS;
        }

        private void AdjustListBox(ListBox listBox, int allCount, RadioButton rd)
        {
            int isTrueCount = 0;// 如果是true的个数是0或者4时， all按钮被选中 
            foreach (ListBoxItem lbi in listBox.Items)
            {
                if (lbi.IsSelected == true)
                {
                    isTrueCount++;
                }
            }

            //if (rd != null)
            //{
            //    if (isTrueCount == allCount)
            //    {
            //        rd.IsChecked = true;
            //    }
            //    else
            //    {
            //        rd.IsChecked = false;
            //    }
            //}
            if (isTrueCount == 0)
            {
                rd.IsChecked = true;
            }
            else
            {
                rd.IsChecked = false;
            }
        }

        private volatile bool isNonmanualCheckShowAll = false;
        private volatile bool isCheckShowAll = false;
        private Dictionary<string, string> venuesDic = new Dictionary<string, string>();
        private void ShowAll_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }
            SearchText = string.Empty;

            isCheckShowAll = true;
            hasPriceChk.IsChecked = true;
            isMeCB.IsChecked = false;
            vipCB.IsChecked = false;
            isMMNoReplyCB.IsChecked = false;
            foreach (UserControl pair in this.CtrlDic.Values)
            {
                if (pair is MultiSelectComboBox)
                {
                    (pair as MultiSelectComboBox).ClearSelection();
                }
                else if (pair is FilterOptions)
                {
                    (pair as FilterOptions).IsAll = true;
                }
                else if (pair is HierarchicalComboBox)
                {
                    (pair as HierarchicalComboBox).ClearSelection();
                }
            }
            this.ClearPriceCtrl();
            //RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            isCheckShowAll = false;
        }

        private void AdjustShowAll_ToggleButtonCheck()
        {
            bool isChecked = true;
            foreach (KeyValuePair<string, UserControl> pair in this.CtrlDic)
            {
                if (this.CtrlDic.ContainsKey(pair.Key))
                {
                    if (this.CtrlDic[pair.Key] is MultiSelectComboBox)
                    {
                        isChecked = isChecked && (this.CtrlDic[pair.Key] as MultiSelectComboBox).IsAll;
                    }
                    else if (this.CtrlDic[pair.Key] is FilterOptions)
                    {
                        isChecked = isChecked && (this.CtrlDic[pair.Key] as FilterOptions).IsAll;
                    }
                    else if (this.CtrlDic[pair.Key] is HierarchicalComboBox)
                    {
                        isChecked = isChecked && (this.CtrlDic[pair.Key] as HierarchicalComboBox).IsAll;
                    }
                }
            }

            isNonmanualCheckShowAll = true;
            this.ShowAll_ToggleButton.IsChecked = isChecked && string.IsNullOrEmpty(this.SearchText) && IsMe == false && HasPrice == true && (this.isMMNoReplyCB == null || this.isMMNoReplyCB.IsChecked == false) && (this.vipCB == null || this.vipCB.IsChecked == false)
                && IsPrice == false;

            // && this._focusTagsDic.Count == 10 

            isNonmanualCheckShowAll = false;
        }

        private bool isAll(List<string> items, int allCount)
        {
            return items != null && items.Count == allCount;
        }

        private void Term_All_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            //this.TermCtrl.ClearValue();
        }

        private void isMeCB_Checked(object sender, RoutedEventArgs e)
        {
            IsMe = true;
            this.AdjustShowAll_ToggleButtonCheck();
        }

        private void isMeCB_Unchecked(object sender, RoutedEventArgs e)
        {
            IsMe = false;
            this.AdjustShowAll_ToggleButtonCheck();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs { RoutedEvent = SaveActionEvent });
        }

        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs { RoutedEvent = EditActionEvent });
        }

        private void isMMNoReplyCB_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.DQTYPE))
            {
                this._focusTagsDic[Constants.DQTYPE] = new List<string>();
            }

            this._focusTagsDic[Constants.DQTYPE].Clear();

            this._focusTagsDic[Constants.DQTYPE].Add("0010");

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void isMMNoReplyCB_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.DQTYPE))
            {
                this._focusTagsDic[Constants.DQTYPE] = new List<string>();
            }

            this._focusTagsDic[Constants.DQTYPE].Clear();

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }


        private void InitOrgType()
        {
            Dictionary<string, QuoteTag> tagsDic = BaseHandler.Instance.GetQuoteTagDic();

            // 业务类型
            SetComboBoxItemsSource(this.business, tagsDic, Constants.ROLETYPE);
            // 意向
            SetComboBoxItemsSource(this.firmflag, tagsDic, Constants.FIRM);
            // 类型
            //SetFilterOptions(this.type1, tagsDic);
            // 客户类型
            SetComboBoxItemsSource(this.OrgCbb, tagsDic, Constants.ORGTYPE);
            //产品
            //SetFilterOptions(this.product, tagsDic, Constants.IRS_PRODUCT_TYPE);
            SetFilterOptions(this.product_outright, tagsDic, Constants.IRS_PRODUCT_TYPE);
            SetFilterOptions(this.product_spread, tagsDic, Constants.IRS_PRODUCT_TYPE);
            SetFilterOptions(this.product_basis, tagsDic, Constants.IRS_PRODUCT_TYPE);

            //期限
            SetFilterOptions(this.term, tagsDic);
        }

        private void SetComboBoxItemsSource(MultiSelectComboBox cb, Dictionary<string, QuoteTag> tagsDic, string key)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            if (key == Constants.FIRM)
            {
                dic["0001"] = "Firm";
                dic["0010"] = "NonFirm";
            }
            else
            {
                if (tagsDic != null && tagsDic.ContainsKey(key))
                {
                    if (key == Constants.ISSUE_YEAR)
                    {
                        tagsDic[key].childTags = tagsDic[key].childTags.OrderByDescending(item => item.childTagName).ToList();
                    }
                    else if (key == Constants.RATING_CURRENT
                        || key == Constants.CDC_IMPLITED_RATING
                        || key == Constants.ISSUER_RATING_CURRENT_CBR
                        || key == Constants.CSI_IMPLIED_RATING
                        || key == Constants.RANKING_TYPE
                        || key == Constants.PERPETUAL_TYPE
                        || key == Constants.CAL_INTER_TYPE)
                    {

                    }
                    else
                    {
                        tagsDic[key].childTags = tagsDic[key].childTags.OrderBy(item => item.pinyin).ToList();
                    }

                    foreach (ChildTag ct in tagsDic[key].childTags)
                    {
                        if (ct.childTagName == "无")
                        {
                            continue;
                        }
                        dic[ct.childTagValue] = ct.childTagName;
                    }
                }
            }
            cb.ItemsSource = dic;
        }

        private void SetFilterOptions(FilterOptions fo, Dictionary<string, QuoteTag> tagsDic, string tagKey = null)
        {
            if (fo == null || tagsDic == null)
                return;
            ObservableCollection<OptionItem> exchangelist = new ObservableCollection<OptionItem>();


            if (tagKey == Constants.IRS_PRODUCT_TYPE)
            {
                //string[] typeArr = new string[] { Constants.IRS_PRODUCT_TYPE_OUTRIGHT, Constants.IRS_PRODUCT_TYPE_SPREAD, Constants.IRS_PRODUCT_TYPE_BASIS };

                //if (tagsDic != null && tagsDic.ContainsKey(tagKey))
                //{
                //    foreach (string subType in typeArr)
                //    {
                //        if (exchangelist.Count > 0)
                //        {
                //            exchangelist.Add(new OptionItem(string.Empty, string.Empty));
                //        }
                //        ChildTag ct = tagsDic[tagKey].childTags.Find(item => item.childTag == subType);
                //        foreach (ChildTag sub_ct in ct.childTags)
                //        {
                //            if (sub_ct.childTagName == "无")
                //            {
                //                continue;
                //            }

                //            exchangelist.Add(new OptionItem(sub_ct.childTagName, sub_ct.childTagValue));
                //        }
                //    }

                //}
                string key = fo.TagKey;
                if (tagsDic != null && tagsDic.ContainsKey(tagKey))
                {
                    foreach (ChildTag ct in tagsDic[tagKey].childTags)
                    {
                        if (ct.childTagName == "无")
                        {
                            continue;
                        }

                        if (ct.childTag == key)
                        {
                            foreach (ChildTag sub_ct in ct.childTags)
                            {
                                if (sub_ct.childTagName == "无")
                                {
                                    continue;
                                }

                                exchangelist.Add(new OptionItem(sub_ct.childTagName, sub_ct.childTagValue));
                            }
                        }
                    }
                }
            }
            else
            {
                string key = fo.TagKey;
                if (tagsDic != null && tagsDic.ContainsKey(key))
                {
                    foreach (ChildTag ct in tagsDic[key].childTags)
                    {
                        if (ct.childTagName == "无")
                        {
                            continue;
                        }

                        exchangelist.Add(new OptionItem(ct.childTagName, ct.childTagValue));
                    }
                }
            }
            

            fo.ItemsSource = exchangelist;
        }

        private static void GenDefaultFocusTags()
        {
            Dictionary<string, QuoteTag> tagsDic = BaseHandler.Instance.GetQuoteTagDic();
            if (string.IsNullOrEmpty(DEFAULT_FOCUSTAGS))
            {
                if (tagsDic != null && tagsDic.Count > 0)
                {
                    Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
                    foreach(QuoteTag qt in tagsDic.Values)
                    {
                        if (!dic.ContainsKey(qt.tagType))
                            dic[qt.tagType] = new List<string>();

                        //if (qt.tagType == Constants.BONDTYPE)
                        //{
                        //    dic[qt.tagType].Add("0000");
                        //    dic[qt.tagType].Add("1011");
                        //    dic[qt.tagType].Add("1100");
                        //    dic[qt.tagType].Add("1101");
                        //    dic[qt.tagType].Add("1110");
                        //}
                        //else if (qt.tagType == Constants.TERM)
                        //{
                        //    dic[qt.tagType].Add("0000");
                        //    dic[qt.tagType].Add("0001");
                        //    dic[qt.tagType].Add("0010");
                        //    dic[qt.tagType].Add("0011");
                        //    dic[qt.tagType].Add("0100");
                        //    dic[qt.tagType].Add("0101");
                        //    dic[qt.tagType].Add("0110");
                        //    dic[qt.tagType].Add("0111");
                        //    dic[qt.tagType].Add("1000");
                        //}
                        //else if (qt.tagType == Constants.FINBONDTYPE)
                        //{
                        //    dic[qt.tagType].Add("0000");
                        //    dic[qt.tagType].Add("0001");
                        //    dic[qt.tagType].Add("0010");
                        //    dic[qt.tagType].Add("0011");
                        //}
                        //else if (qt.tagType == Constants.DQTYPE)
                        //{
                        //    dic[qt.tagType].Add("0000");
                        //    dic[qt.tagType].Add("0001");
                        //    dic[qt.tagType].Add("0010");
                        //}

                        //if (qt.childTags != null && qt.childTags.Count > 0)
                        //{
                        //    foreach(ChildTag ct in qt.childTags)
                        //    {
                        //        if (!dic[qt.tagType].Contains(ct.childTagValue))
                        //        {
                        //            dic[qt.tagType].Add(ct.childTagValue);
                        //        }
                        //    }
                        //}
                    }
                    DEFAULT_FOCUSTAGS = JsonConvert.SerializeObject(dic);
                }
            }
        }

        private void Cbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            string key = ccb.Tag?.ToString();

            //if (isCheckShowAll == true)
            //    return;

            if (!this._focusTagsDic.ContainsKey(key))
            {
                this._focusTagsDic[key] = new List<string>();
            }

            this._focusTagsDic[key].Clear();

            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    string[] orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);


                    if (orgArr != null && orgArr.Length > 0)
                    {
                        foreach (string org in orgArr)
                        {
                            this._focusTagsDic[key].Add(org);
                        }
                    }
                    else
                    {
                        //Dictionary<string, QuoteTag> tagsDic = IntentionIRSHandler.Instance.QueryTags(true);

                        //if (tagsDic.ContainsKey(Constants.ORGTYPE))
                        {
                            //if (tagsDic[Constants.ORGTYPE] != null && tagsDic.ContainsKey(Constants.ORGTYPE) && tagsDic[Constants.ORGTYPE].childTags != null)
                            {
                                //foreach(ChildTag ct in tagsDic[Constants.ORGTYPE].childTags)
                                {
                                    this._focusTagsDic[key].Add(string.Empty);
                                }
                            }
                        }
                    }
                }
            }

            this.AdjustShowAll_ToggleButtonCheck();

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }
        }


        private void vipCB_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.VIP))
            {
                this._focusTagsDic[Constants.VIP] = new List<string>();
            }

            this._focusTagsDic[Constants.VIP].Clear();

            this._focusTagsDic[Constants.VIP].Add("0001");


            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void vipCB_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.VIP))
            {
                this._focusTagsDic[Constants.VIP] = new List<string>();
            }

            this._focusTagsDic[Constants.VIP].Clear();

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }


        private void TagsListChanged(object sender, RoutedEventArgs e)
        {
            FilterOptions fo = sender as FilterOptions;

            string tagKey = fo.TagKey;
            //if (tagKey == Constants.IRS_TYPE)
            //{
            //    if (fo.TagsList.Contains("0001"))// Outright
            //    {
            //        product_outright.Visibility = Visibility.Visible;
            //        product_spread.Visibility = Visibility.Collapsed;
            //        product_basis.Visibility = Visibility.Collapsed;
            //    }
            //    else if (fo.TagsList.Contains("0010"))//Spread
            //    {
            //        product_outright.Visibility = Visibility.Collapsed;
            //        product_spread.Visibility = Visibility.Visible;
            //        product_basis.Visibility = Visibility.Collapsed;
            //    }
            //    else if (fo.TagsList.Contains("0011"))//Basis
            //    {
            //        product_outright.Visibility = Visibility.Collapsed;
            //        product_spread.Visibility = Visibility.Collapsed;
            //        product_basis.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        product_outright.Visibility = Visibility.Collapsed;
            //        product_spread.Visibility = Visibility.Collapsed;
            //        product_basis.Visibility = Visibility.Collapsed;
            //    }

            //    product_outright.IsAll = true;
            //    product_spread.IsAll = true;
            //    product_basis.IsAll = true;
            //}
            //else
            if (tagKey == Constants.IRS_PRODUCT_TYPE_SPREAD
                || tagKey == Constants.IRS_PRODUCT_TYPE_OUTRIGHT
                || tagKey == Constants.IRS_PRODUCT_TYPE_BASIS)
            {
                tagKey = Constants.IRS_PRODUCT_TYPE;
                List<string> list = new List<string>();
                list.AddRange(this.product_outright.TagsList);
                list.AddRange(this.product_spread.TagsList);
                list.AddRange(this.product_basis.TagsList);
                _focusTagsDic[tagKey] = list;
            }
            else
            {
                _focusTagsDic[tagKey] = fo.TagsList;
            }
            

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            AdjustShowAll_ToggleButtonCheck();
        }

        public void ShowAll()
        {
            this.ShowAll_ToggleButton.IsChecked = true;
        }

        private void HierarchicalComboBox_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            HierarchicalComboBox ccb = sender as HierarchicalComboBox;
            string key = ccb.Tag?.ToString();

            //if (isCheckShowAll == true)
            //    return;

            if (!this._focusTagsDic.ContainsKey(key))
            {
                this._focusTagsDic[key] = new List<string>();
            }

            this._focusTagsDic[key].Clear();

            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    string[] orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);


                    if (orgArr != null && orgArr.Length > 0)
                    {
                        foreach (string org in orgArr)
                        {
                            this._focusTagsDic[key].Add(org);
                        }
                    }
                    else
                    {
                        //Dictionary<string, QuoteTag> tagsDic = IntentionIRSHandler.Instance.QueryTags(true);

                        //if (tagsDic.ContainsKey(Constants.ORGTYPE))
                        {
                            //if (tagsDic[Constants.ORGTYPE] != null && tagsDic.ContainsKey(Constants.ORGTYPE) && tagsDic[Constants.ORGTYPE].childTags != null)
                            {
                                //foreach(ChildTag ct in tagsDic[Constants.ORGTYPE].childTags)
                                {
                                    this._focusTagsDic[key].Add(string.Empty);
                                }
                            }
                        }
                    }
                }
            }

            this.AdjustShowAll_ToggleButtonCheck();

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }
        }

        private void PriceOKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.StartPrice = this.SPrice.Text;
            this.EndPrice = this.EPrice.Text;

            if (string.IsNullOrEmpty(this.StartPrice) && string.IsNullOrEmpty(this.EndPrice))
            {
                IsPrice = false;
            }
            else
            {
                IsPrice = true;
            }

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void ClearPriceCtrl()
        {
            //this.PriceChk.IsChecked = false;
            this.IsPrice = false;
            this.StartPrice = this.SPrice.Text = string.Empty;
            this.EndPrice = this.EPrice.Text = string.Empty;
            AdjustShowAll_ToggleButtonCheck();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = null;
            regex = new Regex("^[0-9yd.]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text.ToLower();

            if (text.Contains(".") && e.Text.Equals("."))
            {
                e.Handled = true;
                return;
            }

            decimal d = 0;
            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            string temp = text.ToUpper().Replace("d", ",").Replace("y", ",");
            string[] sectionArr = text.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            foreach (string textSec in sectionArr)
            {
                string[] arr = textSec.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void PriceChk_Checked(object sender, RoutedEventArgs e)
        {
            IsPrice = true;
            this.AdjustShowAll_ToggleButtonCheck();
            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void PriceChk_Unchecked(object sender, RoutedEventArgs e)
        {
            IsPrice = false;
            this.AdjustShowAll_ToggleButtonCheck();
            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }


        private bool isFromEnter = false;

        private void Price_KeyDown(object sender, KeyEventArgs e)
        {
            isFromEnter = false;
            AppUtils.IsInnerInput = true;

            if (e.Key == Key.Enter)
            {
                isFromEnter = true;
                //PriceOKBtn_Click(null, null);
                tb.Focus();
            }
        }

        private void hasPriceChk_Checked(object sender, RoutedEventArgs e)
        {
            HasPrice = true;
            this.AdjustShowAll_ToggleButtonCheck();
        }

        private void hasPriceChk_Unchecked(object sender, RoutedEventArgs e)
        {
            HasPrice = false;
            this.AdjustShowAll_ToggleButtonCheck();
        }

        private void Price_LostFocus(object sender, RoutedEventArgs e)
        {
            AppUtils.IsInnerInput = false;
            PriceOKBtn_Click(null, null);
        }

        private void Price_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppUtils.IsInnerInput = true;
            if (isFromEnter)
            {
                AppUtils.IsInnerInput = false;
            }
        }
    }
}
