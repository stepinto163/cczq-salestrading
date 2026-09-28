using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.InnerBoard.Plugin.Utils;
using STC.WorkBench.Plugin.InnerOrders;
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

namespace STC.InnerBoard.Plugin.View.Component
{
    /// <summary>
    /// InnerBoardFilterPanel_New.xaml 的交互逻辑
    /// </summary>
    public partial class InnerBoardFilterPanel_New : UserControl
    {


        public bool IsAppTerm
        {
            get { return (bool)GetValue(IsAppTermProperty); }
            set { SetValue(IsAppTermProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAppTerm.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAppTermProperty =
            DependencyProperty.Register("IsAppTerm", typeof(bool), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(false, OnIsAppTermChanged));

        private static void OnIsAppTermChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            if ((bool)e.NewValue)
            {
                source.isCheckShowAll = true;
                source.Term_All_ToggleButton.IsChecked = false;

                foreach (ListBoxItem item in source.TermListBox.Items)
                {
                    item.IsSelected = false;
                }

                source.StartTermDY = source.TermCtrl.StartTermDY;
                source.EndTermDY = source.TermCtrl.EndTermDY;

                source.AdjustShowAll_ToggleButtonCheck();
                source.isCheckShowAll = false;
            }
        }

        public string StartTermDY
        {
            get { return (string)GetValue(StartTermDYProperty); }
            set { SetValue(StartTermDYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTermDY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTermDYProperty =
            DependencyProperty.Register("StartTermDY", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, onStartTermDYChanged));

        private static void onStartTermDYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            source.TermCtrl.StartTermDY = e.NewValue == null ? string.Empty : e.NewValue.ToString();
        }


        public string EndTermDY
        {
            get { return (string)GetValue(EndTermDYProperty); }
            set { SetValue(EndTermDYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTermDY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndTermDYProperty =
            DependencyProperty.Register("EndTermDY", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, onEndTermDYChanged));

        private static void onEndTermDYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            source.TermCtrl.EndTermDY = e.NewValue == null ? string.Empty : e.NewValue.ToString();
        }

        ///// <summary>
        ///// 票面筛选事件
        ///// </summary>
        //public static readonly RoutedEvent CouponActionEvent = EventManager.RegisterRoutedEvent("CouponAction", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InnerBoardFilterPanel_New));

        //public event RoutedEventHandler CouponAction
        //{
        //    add { AddHandler(CouponActionEvent, value); }
        //    remove { RemoveHandler(CouponActionEvent, value); }
        //}

        public bool IsCoupon
        {
            get { return (bool)GetValue(IsCouponProperty); }
            set { SetValue(IsCouponProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCoupon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCouponProperty =
            DependencyProperty.Register("IsCoupon", typeof(bool), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(false));

        public string StartCoupon
        {
            get { return (string)GetValue(StartCouponProperty); }
            set { SetValue(StartCouponProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTermDY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartCouponProperty =
            DependencyProperty.Register("StartCoupon", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, onStartCouponChanged));

        private static void onStartCouponChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.SCoupon.Text != newValue)
            {
                source.SCoupon.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }


        public string EndCoupon
        {
            get { return (string)GetValue(EndCouponProperty); }
            set { SetValue(EndCouponProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTermDY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndCouponProperty =
            DependencyProperty.Register("EndCoupon", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, onEndCouponChanged));

        private static void onEndCouponChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.ECoupon.Text != newValue)
            {
                source.ECoupon.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }


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
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.SPrice.Text != newValue)
            {
                source.SPrice.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
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
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.EPrice.Text != newValue)
            {
                source.EPrice.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }

        public string StartChinaBondValue
        {
            get { return (string)GetValue(StartChinaBondValueProperty); }
            set { SetValue(StartChinaBondValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartChinaBondValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartChinaBondValueProperty =
            DependencyProperty.Register("StartChinaBondValue", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, OnChinaBondValueChanged));

        private static void OnChinaBondValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.SChinaBondValue.Text != newValue)
            {
                source.SChinaBondValue.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }

        public string  EndChinaBondValue
        {
            get { return (string )GetValue(EndChinaBondValueProperty); }
            set { SetValue(EndChinaBondValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndChinaBondValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndChinaBondValueProperty =
            DependencyProperty.Register("EndChinaBondValue", typeof(string ), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, OnEndChinaBondValueChanged));

        private static void OnEndChinaBondValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.EChinaBondValue.Text != newValue)
            {
                source.EChinaBondValue.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }

        public string StartBidBp
        {
            get { return (string)GetValue(StartBidBpProperty); }
            set { SetValue(StartBidBpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartBidBp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartBidBpProperty =
            DependencyProperty.Register("StartBidBp", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, OnBidBpChanged));

        private static void OnBidBpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.SBidBP.Text != newValue)
            {
                source.SBidBP.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }

        public string EndBidBp
        {
            get { return (string)GetValue(EndBidBpProperty); }
            set { SetValue(EndBidBpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndBidBp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndBidBpProperty =
            DependencyProperty.Register("EndBidBp", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, OnEndBpChanged));

        private static void OnEndBpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.EBidBP.Text != newValue)
            {
                source.EBidBP.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }
        public string StartOfrBp
        {
            get { return (string)GetValue(StartOfrBpProperty); }
            set { SetValue(StartOfrBpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartOfrBp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartOfrBpProperty =
            DependencyProperty.Register("StartOfrBp", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, OnStartOfrBpChanged));

        private static void OnStartOfrBpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.SOfrBP.Text != newValue)
            {
                source.SOfrBP.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }

        public string EndOfrBp
        {
            get { return (string)GetValue(EndOfrBpProperty); }
            set { SetValue(EndOfrBpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndOfrBp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndOfrBpProperty =
            DependencyProperty.Register("EndOfrBp", typeof(string), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(string.Empty, OnEndOfrBpChanged));

        private static void OnEndOfrBpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InnerBoardFilterPanel_New source = d as InnerBoardFilterPanel_New;
            string newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            if (source.EOfrBP.Text != newValue)
            {
                source.EOfrBP.Text = newValue;
                source.AdjustShowAll_ToggleButtonCheck();
            }
        }



        //// 过滤属性
        //public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
        //    typeof(InnerBoardFilterPanel_New), new PropertyMetadata(null));

        //public Dictionary<string, IList> FilterDic
        //{
        //    get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
        //    set { SetValue(FilterDicProperty, value); }
        //}

        ///// <summary>
        ///// 过滤条件变化事件
        ///// </summary>
        //public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InnerBoardFilterPanel_New));

        //public event RoutedEventHandler FilterDicChanged
        //{
        //    add { AddHandler(FilterDicChangedEvent, value); }
        //    remove { RemoveHandler(FilterDicChangedEvent, value); }
        //}

        /// <summary>
        /// ShowAll事件
        /// </summary>
        public static readonly RoutedEvent ShowAllActionEvent = EventManager.RegisterRoutedEvent("ShowAllAction", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InnerBoardFilterPanel_New));

        public event RoutedEventHandler ShowAllAction
        {
            add { AddHandler(ShowAllActionEvent, value); }
            remove { RemoveHandler(ShowAllActionEvent, value); }
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
                Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);

                if (dic != null && !dic.ContainsKey(Constants.BOND_SUB_TYPE))
                {
                    dic[Constants.BOND_SUB_TYPE] = new List<string>();
                }

                if (dic != null && dic.ContainsKey(Constants.BONDTYPE) && dic[Constants.BONDTYPE].Count > 0)
                {
                    dic[Constants.BOND_SUB_TYPE].RemoveAll(item => item == "1011"
                        || item == "1100"
                        || item == "1101"
                        || item == "1110"
                        || item == "1111");

                    foreach (string bondtype_item in dic[Constants.BONDTYPE])
                    {
                        if (!dic[Constants.BOND_SUB_TYPE].Contains(bondtype_item))
                        {
                            dic[Constants.BOND_SUB_TYPE].Add(bondtype_item);
                        }
                    }
                }

                if (dic != null && !dic.ContainsKey(Constants.BOND_SUB_TYPE2))
                {
                    dic[Constants.BOND_SUB_TYPE2] = new List<string>();
                }

                if (dic != null)
                {
                    dic[Constants.BOND_SUB_TYPE2].Clear();
                    if (dic != null && dic.ContainsKey(Constants.BOND_SUB_TYPE) && dic[Constants.BOND_SUB_TYPE].Count > 0)
                    {
                        foreach (string bondtype_item in dic[Constants.BOND_SUB_TYPE])
                        {
                            if (bondtype_item.StartsWith("x"))
                            {
                                dic[Constants.BOND_SUB_TYPE2].Add(bondtype_item.Substring(1));
                            }
                        }
                    }

                    value = JsonConvert.SerializeObject(dic);
                }

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
            source.isMeCB.Visibility = (Visibility)args.NewValue;
        }

        public Visibility Visibility4NoReply
        {
            get { return (Visibility)GetValue(Visibility4NoReplyProperty); }
            set { SetValue(Visibility4NoReplyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Visibility4NoReplyProperty =
            DependencyProperty.Register("Visibility4NoReply", typeof(Visibility), typeof(InnerBoardFilterPanel_New),
            new PropertyMetadata(Visibility.Visible, onVisibility4NoReply));

        private static void onVisibility4NoReply(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            InnerBoardFilterPanel_New source = (InnerBoardFilterPanel_New)sender;
            source.isMMNoReplyCB.Visibility = (Visibility)args.NewValue;
        }

        public bool IsFuzzy
        {
            get { return (bool)GetValue(IsFuzzyProperty); }
            set { SetValue(IsFuzzyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFuzzy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFuzzyProperty =
            DependencyProperty.Register("IsFuzzy", typeof(bool), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(false));

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
            DependencyProperty.Register("IsShowSaveEdit", typeof(Visibility), typeof(InnerBoardFilterPanel_New), new PropertyMetadata(Visibility.Collapsed, IsShowSaveEditChanged));

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
                    if (source.ZuoShi != null)
                        source.ZuoShi.IsChecked = false;
                    if (source.WeiYue != null)
                        source.WeiYue.IsChecked = false;
                    if (source.KeChuang != null)
                        source.KeChuang.IsChecked = false;
                    if (source.ShuangChuang != null)
                        source.ShuangChuang.IsChecked = false;
                    if (source.IfTier2Perp != null)
                        source.IfTier2Perp.IsChecked = false;
                    Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(source.FocusTags);
                    if (dic != null && dic.Count > 0)
                    {
                        List<string> bond_sub_type_list = new List<string>();// 分布在两个tag内, BOND_SUB_TYPE和BOND_SUB_TYPE2
                        foreach (KeyValuePair<string, List<string>> pair in dic)
                        {
                            if (pair.Key == Constants.TERM)
                            {
                                if (!source.IsAppTerm)
                                {
                                    if (pair.Value.Count > 0 && !pair.Value.Contains(ALL))
                                    {
                                        foreach (ListBoxItem item in source.TermListBox.Items)
                                        {
                                            string tag = item.Tag?.ToString();
                                            if (tag != null)
                                                item.IsSelected = pair.Value.Contains(tag);
                                            else
                                                item.IsSelected = false;
                                        }
                                    }
                                    else
                                    {
                                        source.Term_All_ToggleButton.IsChecked = true;
                                    }
                                }
                                else
                                {
                                    foreach (ListBoxItem item in source.TermListBox.Items)
                                    {
                                        item.IsSelected = false;
                                    }
                                }
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
                            else if (pair.Key == Constants.IF_MARKET_MAKING)
                            {
                                if (pair.Value != null && pair.Value.Count > 0 && pair.Value.Contains("10"))
                                {
                                    source.ZuoShi.IsChecked = true;
                                }
                                else
                                {
                                    source.ZuoShi.IsChecked = false;
                                }
                            }
                            else if (pair.Key == Constants.IF_DEFAULT)
                            {
                                if (pair.Value != null && pair.Value.Count > 0 && pair.Value.Contains("10"))
                                {
                                    source.WeiYue.IsChecked = true;
                                }
                                else
                                {
                                    source.WeiYue.IsChecked = false;
                                }
                            }
                            else if (pair.Key == Constants.INNOVATION_TYPE)
                            {
                                if (pair.Value != null && pair.Value.Count > 0 && pair.Value.Contains("0001"))
                                {
                                    source.KeChuang.IsChecked = true;
                                }
                                else
                                {
                                    source.KeChuang.IsChecked = false;
                                }
                                if (pair.Value != null && pair.Value.Count > 0 && pair.Value.Contains("0010"))
                                {
                                    source.ShuangChuang.IsChecked = true;
                                }
                                else
                                {
                                    source.ShuangChuang.IsChecked = false;
                                }
                            
                            }
                            else if (pair.Key == Constants.IF_TIER2_PERP)
                            {
                                if (pair.Value != null && pair.Value.Count > 0 && pair.Value.Contains("10"))
                                {
                                    source.IfTier2Perp.IsChecked = true;
                                }
                                else
                                {
                                    source.IfTier2Perp.IsChecked = false;
                                }
                            }
                            else if (pair.Key == Constants.FINBONDTYPE)
                            {
                                if (pair.Value.Count > 0 && !pair.Value.Contains(ALL))
                                {
                                    foreach (ListBoxItem item in source.FinBondTypeListBox.Items)
                                    {
                                        string tag = item.Tag?.ToString();
                                        if (tag != null)
                                            item.IsSelected = pair.Value.Contains(tag);
                                        else
                                            item.IsSelected = false;
                                    }
                                }
                                else
                                {
                                    source.FinBondType_All_ToggleButton.IsChecked = true;
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
                            else if (source.CtrlDic.ContainsKey(pair.Key))
                            {
                                if (source.CtrlDic[pair.Key] is MultiSelectComboBox)
                                {
                                    source.SetMultiComboBox(source.CtrlDic[pair.Key] as MultiSelectComboBox, pair.Value);
                                }
                                else if (source.CtrlDic[pair.Key] is FilterOptions)
                                {
                                    if (pair.Key == Constants.BOND_SUB_TYPE)
                                    {
                                        bond_sub_type_list.AddRange(pair.Value);
                                    }
                                    else if(pair.Key == Constants.BOND_SUB_TYPE2)
                                    {
                                        foreach(string subValue in pair.Value)
                                        {
                                            bond_sub_type_list.Add($"x{subValue}");
                                        }
                                    }
                                    else
                                    {
                                        (source.CtrlDic[pair.Key] as FilterOptions).InitData(pair.Value);
                                    }
                                }
                                else if (source.CtrlDic[pair.Key] is HierarchicalComboBox)
                                {
                                    source.SetHierachicalComboBox(source.CtrlDic[pair.Key] as HierarchicalComboBox, pair.Value);
                                }
                                else if (source.CtrlDic[pair.Key] is MultiAutoCompleteComboBox)
                                {
                                    string sql = string.Empty;
                                    if(pair.Value != null && pair.Value.Count > 0)
                                    {
                                        if (pair.Key == "ISSUER")
                                        {
                                            StringBuilder whereSql = new StringBuilder();
                                            foreach (string key in pair.Value)
                                            {
                                                if (whereSql.Length > 0)
                                                {
                                                    whereSql.Append(',');
                                                }
                                                whereSql.Append($"'{key}'");
                                            }

                                            sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where childTagValue in ({whereSql})";
                                        }
                                        else if (pair.Key == "GUARANTOR")
                                        {
                                            StringBuilder whereSql = new StringBuilder();
                                            foreach (string key in pair.Value)
                                            {
                                                if (whereSql.Length > 0)
                                                {
                                                    whereSql.Append(',');
                                                }
                                                whereSql.Append($"'{key}'");
                                            }

                                            sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor where childTagValue in ({whereSql})";
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(sql))
                                    {
                                        
                                        List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllGuarantors(sql);
                                        MultiAutoCompleteComboBox cb = source.CtrlDic[pair.Key] as MultiAutoCompleteComboBox;
                                        foreach (ChildTag ct in queryList)
                                        {
                                            Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                                            if (!cb.ItemsSource.Any(item => item.Key == node.Key))
                                            {
                                                cb.ItemsSource.Add(node);
                                            }
                                        }
                                    }

                                    source.SetMultiAutoCompleteComboBox(source.CtrlDic[pair.Key] as MultiAutoCompleteComboBox, pair.Value);
                                }
                            }
                            if (pair.Key == Constants.BONDTYPE
                                || (pair.Key == Constants.BOND_SUB_TYPE && !dic.ContainsKey(Constants.BONDTYPE)))
                            {
                                if (pair.Value.Count > 0
                                    && (pair.Value.Contains("1011")
                                    || pair.Value.Contains("1100")
                                    || pair.Value.Contains("1101")
                                    || pair.Value.Contains("1110")
                                    || pair.Value.Contains("1111")))
                                {
                                    foreach (ListBoxItem item in source.BondTypeListBox.Items)
                                    {
                                        string tag = item.Tag?.ToString();
                                        if (tag != null)
                                            item.IsSelected = pair.Value.Contains(tag);
                                        else
                                            item.IsSelected = false;
                                    }
                                }
                                else
                                {
                                    if (source.BondType_All_ToggleButton != null)
                                    {
                                        source.BondType_All_ToggleButton.IsChecked = true;
                                    }
                                }
                            }
                        }
                        if (source.CtrlDic.ContainsKey(Constants.BOND_SUB_TYPE))
                        {
                            (source.CtrlDic[Constants.BOND_SUB_TYPE] as FilterOptions).InitData(bond_sub_type_list);
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

        private void SetMultiAutoCompleteComboBox(MultiAutoCompleteComboBox cb, List<string> tagList)
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

                //if (this._focusTagsDic[key].Count == 0)
                //{
                //    this._focusTagsDic[key].Add(ALL);
                //    if (key == Constants.BONDTYPE)
                //    {
                //        this._focusTagsDic[key].Add("1011");
                //        this._focusTagsDic[key].Add("1100");
                //        this._focusTagsDic[key].Add("1101");
                //        this._focusTagsDic[key].Add("1110");
                //    }
                //    else if (key == Constants.TERM)
                //    {
                //        this._focusTagsDic[key].Add("0001");
                //        this._focusTagsDic[key].Add("0010");
                //        this._focusTagsDic[key].Add("0011");
                //        this._focusTagsDic[key].Add("0100");
                //        this._focusTagsDic[key].Add("0101");
                //        this._focusTagsDic[key].Add("0110");
                //        this._focusTagsDic[key].Add("0111");
                //        this._focusTagsDic[key].Add("1000");
                //    }
                //    //else if (key == Constants.FIRM)
                //    //{
                //    //    this._focusTagsDic[key].Add("0001");
                //    //    this._focusTagsDic[key].Add("0010");
                //    //    this._focusTagsDic[key].Add("0011");
                //    //}
                //    //else if (key == Constants.SETTLTYPE)
                //    //{
                //    //    this._focusTagsDic[key].Add("0001");
                //    //    this._focusTagsDic[key].Add("0010");
                //    //    this._focusTagsDic[key].Add("0011");
                //    //    this._focusTagsDic[key].Add("0100");
                //    //    this._focusTagsDic[key].Add("0101");
                //    //}
                //    //else if (key == Constants.SIDE)
                //    //{
                //    //    this._focusTagsDic[key].Add("0001");
                //    //    this._focusTagsDic[key].Add("0010");
                //    //}
                //    //else if (key == Constants.VIP)
                //    //{
                //    //    this._focusTagsDic[key].Add("0001");
                //    //    this._focusTagsDic[key].Add("0010");
                //    //}
                //    //else if (key == Constants.ORGTYPE)
                //    //{
                //    //    this._focusTagsDic[key].Add("0001");
                //    //    this._focusTagsDic[key].Add("0010");
                //    //    this._focusTagsDic[key].Add("0011");
                //    //    this._focusTagsDic[key].Add("0100");
                //    //    this._focusTagsDic[key].Add("0101");
                //    //    this._focusTagsDic[key].Add("0110");
                //    //    this._focusTagsDic[key].Add("0111");
                //    //    this._focusTagsDic[key].Add("1000");
                //    //    this._focusTagsDic[key].Add("1001");
                //    //    this._focusTagsDic[key].Add("1010");
                //    //}
                //    //else if (key == Constants.ROLETYPE)
                //    //{
                //    //    this._focusTagsDic[key].Add("0001");
                //    //    this._focusTagsDic[key].Add("0010");
                //    //}
                //    else if (key == Constants.FINBONDTYPE)
                //    {
                //        this._focusTagsDic[key].Add("0001");
                //        this._focusTagsDic[key].Add("0010");
                //        this._focusTagsDic[key].Add("0011");
                //    }
                //}

                if (!isUpdate || key == Constants.BONDTYPE)
                {
                    FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
                }
            }
        }

        Dictionary<string, UserControl> CtrlDic = new Dictionary<string, UserControl>();

        string[] onlyRateArr = new string[] { Constants.BONDTYPE, Constants.FINBONDTYPE, Constants.LOCAL_GOV_BOND_TYPE };
        string[] onlyCreditArr = new string[] { Constants.BOND_SUB_TYPE, Constants.ENTERPRISE_TYPE, Constants.FIN_INST_BOND, Constants.NCD_TYPE,
            Constants.RANKING_TYPE, Constants.RATING_CURRENT, Constants.ISSUER_RATING_CURRENT_CBR, Constants.CDC_IMPLITED_RATING, Constants.CSI_IMPLIED_RATING,
            Constants.IF_MUNICIPAL, Constants.IF_PLATFORM, Constants.ADMIN_LEVEL, Constants.IF_GUARANTEE, Constants.ISSUE_TYPE};

        public InnerBoardFilterPanel_New()
        {
            InitializeComponent();

            if (Common.Utils.CommonUtils.isOnlyRate)
            {
                this.SetDisplayMode("1");
            }
            else if (Common.Utils.CommonUtils.isOnlyCredit)
            {
                this.SetDisplayMode("2");
            }

            CtrlDic[Constants.SIDE] = this.side;
            //CtrlDic[Constants.FIRM] = this.firmflag;
            CtrlDic[Constants.SETTLTYPE] = this.settltype;
            //CtrlDic[Constants.ORGTYPE] = this.OrgCbb;
            //CtrlDic[Constants.ROLETYPE] = this.business;
            CtrlDic[Constants.LOCAL_GOV_BOND_TYPE] = this.filter2;
            CtrlDic[Constants.CAL_INTER_TYPE] = this.ratetype;
            CtrlDic[Constants.PERPETUAL_TYPE] = this.OrgCbb10;
            CtrlDic[Constants.BOND_SUB_TYPE] = this.filter1;
            CtrlDic[Constants.ISSUE_TYPE] = this.filter11;
            CtrlDic[Constants.RANKING_TYPE] = this.rate;
            CtrlDic[Constants.IF_MUNICIPAL] = this.filter7;
            CtrlDic[Constants.IF_GUARANTEE] = this.filter10;
            CtrlDic[Constants.CUM_TYPE] = this.filter12;
            CtrlDic[Constants.IF_REPAY] = this.filter13;
            CtrlDic[Constants.ENTERPRISE_TYPE] = this.filter3;
            CtrlDic[Constants.IF_PLATFORM] = this.filter8;
            CtrlDic[Constants.HOLIDAY_TYPE] = this.OrgCbb11;
            CtrlDic[Constants.LISTED_MARKET] = this.exchange;
            CtrlDic[Constants.NCD_TYPE] = this.banktype;
            CtrlDic[Constants.ISSUE_YEAR] = this.OrgCbb7;
            CtrlDic[Constants.REGION_TYPE] = this.OrgCbb6;
            CtrlDic[Constants.INDUSTRY_TYPE] = this.OrgCbb5;
            CtrlDic[Constants.RATING_CURRENT] = this.OrgCbb3;
            CtrlDic[Constants.CDC_IMPLITED_RATING] = this.OrgCbb1;
            CtrlDic[Constants.ISSUER_RATING_CURRENT_CBR] = this.creditRate;
            CtrlDic[Constants.CSI_IMPLIED_RATING] = this.OrgCbb2;
            CtrlDic[Constants.FIN_INST_BOND] = this.filter5;
            CtrlDic[Constants.ADMIN_LEVEL] = this.filter9;
            CtrlDic[Constants.ISSUER] = this.OrgCbb8;
            CtrlDic[Constants.GUARANTOR] = this.OrgCbb9;
            CtrlDic[Constants.IF_PLEDGED] = this.ifPleded;
            CtrlDic[Constants.IF_TAX_FREE] = this.filter15;

            ObservableCollection<OptionItem> listSide = new ObservableCollection<OptionItem>();
            listSide.Add(new OptionItem("Bid", "0001"));
            listSide.Add(new OptionItem("Ofr", "0010"));
            listSide.Add(new OptionItem("Bid&Ofr", "0011"));
            this.side.ItemsSource = listSide;//side
            // 信用债， 债券类型
            ObservableCollection <OptionItem> list = new ObservableCollection<OptionItem>();
            list.Add(new OptionItem("超短融", "0001"));
            list.Add(new OptionItem("短融", "0010"));
            list.Add(new OptionItem("中票", "0011"));
            list.Add(new OptionItem("企业债", "0100"));
            list.Add(new OptionItem("公司债", "0101"));
            list.Add(new OptionItem("次级", "0110"));//次级债
            list.Add(new OptionItem("PPN", "0111"));
            list.Add(new OptionItem("其他", "1000"));//其他信用类型
            list.Add(new OptionItem("ABS", "1010"));//暂无
            list.Add(new OptionItem("NCD", "1001"));
            list.Add(new OptionItem("可转债", "x0001"));
            list.Add(new OptionItem("可交换债", "x0010"));
            this.filter1.ItemsSource = list;//BOND_SUB_TYPE

            // 信用债 发行商
            ObservableCollection<OptionItem> list3 = new ObservableCollection<OptionItem>();
            list3.Add(new OptionItem("央企", "0001"));
            list3.Add(new OptionItem("国企", "0010"));
            list3.Add(new OptionItem("民企", "0011"));
            list3.Add(new OptionItem("其他", "0100"));//其他主体
            this.filter3.ItemsSource = list3;//ENTERPRISE_TYPE

            //// 信用债 是否上市
            //ObservableCollection<OptionItem> list4 = new ObservableCollection<OptionItem>();
            //list4.Add(new OptionItem("上市", "1"));
            //this.filter4.ItemsSource = list4;

            // 主体
            ObservableCollection<OptionItem> ratelist = new ObservableCollection<OptionItem>();
            ratelist.Add(new OptionItem("AAA+", "00001"));
            ratelist.Add(new OptionItem("AAA", "00010"));
            ratelist.Add(new OptionItem("AA+", "00011"));
            ratelist.Add(new OptionItem("AA", "00100"));
            ratelist.Add(new OptionItem("AA-", "00101"));
            ratelist.Add(new OptionItem("A+", "00110"));
            ratelist.Add(new OptionItem("其他", "xx"));//其他评级
            this.rate.ItemsSource = ratelist;//RANKING_TYPE

            //城投
            ObservableCollection<OptionItem> list7 = new ObservableCollection<OptionItem>();
            list7.Add(new OptionItem("城投", "01"));//城投债
            list7.Add(new OptionItem("非城投", "10"));//非城投债
            this.filter7.ItemsSource = list7;//IF_MUNICIPAL
            //平台
            ObservableCollection<OptionItem> list8 = new ObservableCollection<OptionItem>();
            list8.Add(new OptionItem("平台", "0001"));//平台债
            list8.Add(new OptionItem("非平台", "0010"));//非平台债
            this.filter8.ItemsSource = list8;//IF_PLATFORM

            //行政级别
            ObservableCollection<OptionItem> list9 = new ObservableCollection<OptionItem>();
            list9.Add(new OptionItem("省", "001"));
            list9.Add(new OptionItem("市", "010"));
            list9.Add(new OptionItem("区县", "011"));
            list9.Add(new OptionItem("其他", "100"));
            this.filter9.ItemsSource = list9;
            
            //公墓
            ObservableCollection<OptionItem> list11 = new ObservableCollection<OptionItem>();
            list11.Add(new OptionItem("公募", "0001"));
            list11.Add(new OptionItem("小公募", "0010"));
            list11.Add(new OptionItem("私募", "0011"));
            //list11.Add(new OptionItem("可交换", "0100"));
            this.filter11.ItemsSource = list11;
            //含权
            ObservableCollection<OptionItem> list12 = new ObservableCollection<OptionItem>();
            list12.Add(new OptionItem("含权", "01"));
            list12.Add(new OptionItem("非含权", "10"));//不含权
            list12.Add(new OptionItem("待行权", "11"));
            this.filter12.ItemsSource = list12;


            //到期还本
            ObservableCollection<OptionItem> list13 = new ObservableCollection<OptionItem>();
            list13.Add(new OptionItem("到期还本", "01"));
            list13.Add(new OptionItem("提前还本", "10"));//不含权
            this.filter13.ItemsSource = list13;

            GenDefaultFocusTags();
            InitOrgType();

            BaseHandler.Instance.SubscribeLoadData_Issuer(OnLoadIsserData);
            BaseHandler.Instance.SubscribeLoadData_Guarantor(OnLoadGuarantorData);

            ShowAll_ToggleButton.Checked += ShowAll_ToggleButton_Checked;
            BondType_All_ToggleButton.Checked += BondType_All_ToggleButton_Click;
            Term_All_ToggleButton.Checked += Term_All_ToggleButton_Click;
            Term_All_ToggleButton.Unchecked += Term_All_ToggleButton_Unchecked;
            FinBondType_All_ToggleButton.Checked += FinBondType_All_ToggleButton_Checked;

            FocusTags = DEFAULT_FOCUSTAGS;
        }

        private void TermListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            IsAppTerm = false;

            if (this.TermListBox != null)
            {
                AdjustListBox(this.TermListBox, 8, this.Term_All_ToggleButton);

                SetFocusTagDic(Constants.TERM, this.TermListBox.SelectedItems);
            }

            this.ClearTermCtrl();

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void BondTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.BondTypeListBox != null)
            {
                AdjustListBox(this.BondTypeListBox, 4, this.BondType_All_ToggleButton);

                SetFocusTagDic(Constants.BONDTYPE, this.BondTypeListBox.SelectedItems);
            }

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
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
            this.BondType_All_ToggleButton.IsChecked = true;
            this.Term_All_ToggleButton.IsChecked = true;
            this.FinBondType_All_ToggleButton.IsChecked = true;
            isMeCB.IsChecked = false;
            isFuzzyCB.IsChecked = false;
            vipCB.IsChecked = false;
            isMMNoReplyCB.IsChecked = false;
            ZuoShi.IsChecked = false;
            WeiYue.IsChecked = false;
            KeChuang.IsChecked = false;
            ShuangChuang.IsChecked = false;
            IfTier2Perp.IsChecked = false;

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
                else if (pair is MultiAutoCompleteComboBox)
                {
                    (pair as MultiAutoCompleteComboBox).ClearSelection();
                }
            }
            this.ClearTermCtrl();
            this.ClearCouponCtrl();
            this.ClearPriceCtrl();
            this.ClearChinaBondValueCtrl();
            this.ClearBidBpCtrl();
            this.ClearOfrBpCtrl();
            //RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            isCheckShowAll = false;

            RaiseEvent(new RoutedEventArgs { RoutedEvent = ShowAllActionEvent });
        }

        private void AdjustShowAll_ToggleButtonCheck()
        {
            bool isChecked = true;
            foreach (KeyValuePair<string, List<string>> pair in this._focusTagsDic)
            {
                if (pair.Key == Constants.BONDTYPE)
                {
                    isChecked = isChecked && this.BondType_All_ToggleButton.IsChecked.Value;
                }
                else if (pair.Key == Constants.TERM)
                {
                    isChecked = isChecked && this.Term_All_ToggleButton.IsChecked.Value;
                }
                
                else if (pair.Key == Constants.FINBONDTYPE)
                {
                    isChecked = isChecked && this.FinBondType_All_ToggleButton.IsChecked.Value;
                }

                else if (this.CtrlDic.ContainsKey(pair.Key))
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
                    else if (this.CtrlDic[pair.Key] is MultiAutoCompleteComboBox)
                    {
                        isChecked = isChecked && (this.CtrlDic[pair.Key] as MultiAutoCompleteComboBox).SelectedCount == 0;
                    }
                }
            }

            isNonmanualCheckShowAll = true;
            this.ShowAll_ToggleButton.IsChecked = isChecked && string.IsNullOrEmpty(this.SearchText) && IsMe == false && IsFuzzy == false 
                && (this.isMMNoReplyCB == null || this.isMMNoReplyCB.IsChecked == false) 
                && (this.vipCB == null || this.vipCB.IsChecked == false)
                && (this.ZuoShi == null || this.ZuoShi.IsChecked == false)
                && (this.WeiYue == null || this.WeiYue.IsChecked == false)
                && (this.KeChuang == null || this.KeChuang.IsChecked == false)
                && (this.ShuangChuang == null || this.ShuangChuang.IsChecked == false)
                && (this.IfTier2Perp == null || this.IfTier2Perp.IsChecked == false)
                && string.IsNullOrEmpty(this.StartCoupon) && string.IsNullOrEmpty(this.EndCoupon)
                && string.IsNullOrEmpty(this.StartPrice) && string.IsNullOrEmpty(this.EndPrice)
                && string.IsNullOrEmpty(this.StartChinaBondValue) && string.IsNullOrEmpty(this.EndChinaBondValue)
                && string.IsNullOrEmpty(this.StartBidBp) && string.IsNullOrEmpty(this.EndBidBp)
                && string.IsNullOrEmpty(this.StartOfrBp) && string.IsNullOrEmpty(this.EndOfrBp);

            // && this._focusTagsDic.Count == 10 

            isNonmanualCheckShowAll = false;
        }

        private bool isAll(List<string> items, int allCount)
        {
            return items != null && items.Count == allCount;
        }

        private void BondType_All_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            foreach (ListBoxItem item in this.BondTypeListBox.Items)
            {
                item.IsSelected = false;
            }

            SetFocusTagDic(Constants.BONDTYPE, this.BondTypeListBox.SelectedItems);

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void Term_All_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            IsAppTerm = false;
            foreach (ListBoxItem item in this.TermListBox.Items)
            {
                item.IsSelected = false;
            }

            SetFocusTagDic(Constants.TERM, this.TermListBox.SelectedItems);

            this.ClearTermCtrl();

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
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

        private void FinBondTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.FinBondTypeListBox != null)
            {
                AdjustListBox(this.FinBondTypeListBox, 3, this.FinBondType_All_ToggleButton);

                SetFocusTagDic(Constants.FINBONDTYPE, this.FinBondTypeListBox.SelectedItems);
            }

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void FinBondType_All_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            foreach (ListBoxItem item in this.FinBondTypeListBox.Items)
            {
                item.IsSelected = false;
            }

            SetFocusTagDic(Constants.FINBONDTYPE, this.FinBondTypeListBox.SelectedItems);

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
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

        private void OnLoadIsserData(bool flag)
        {
            SetIssuer();
            
        }

        private void OnLoadGuarantorData(bool flag)
        {
            SetGuarantor();
        }

        private void InitOrgType()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                Dictionary<string, QuoteTag> tagsDic = BaseHandler.Instance.GetQuoteTagDic();

                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        // 交易所
                        SetFilterOptions(this.exchange, tagsDic);

                        // 可质押
                        SetFilterOptions(this.ifPleded, tagsDic);

                        //一般债、 、 非城投债（10）
                        SetFilterOptions(this.filter2, tagsDic);

                        //金融债类型
                        SetFilterOptions(this.filter5, tagsDic);

                        ////还本
                        //SetFilterOptions(this.filter13, tagsDic);

                        // 是否免税
                        SetFilterOptions(this.filter15, tagsDic);

                        // 中债资信
                        //SetFilterOptions(this.creditRate, tagsDic);
                        SetComboBoxItemsSource(this.creditRate, tagsDic, Constants.ISSUER_RATING_CURRENT_CBR);

                        //有担保
                        SetFilterOptions(this.filter10, tagsDic);

                        // 业务类型
                        //SetComboBoxItemsSource(this.business, tagsDic, Constants.ROLETYPE);

                        // 清算速度
                        SetComboBoxItemsSource(this.settltype, tagsDic, Constants.SETTLTYPE);

                        //// 方向
                        ////SetComboBoxItemsSource(this.side, tagsDic, Constants.SIDE);
                        //SetFilterOptions(this.side, tagsDic);

                        // 意向
                        //SetComboBoxItemsSource(this.firmflag, tagsDic, Constants.FIRM);

                        // 客户类型
                        //SetComboBoxItemsSource(this.OrgCbb, tagsDic, Constants.ORGTYPE);

                        // 利息类型
                        SetComboBoxItemsSource(this.ratetype, tagsDic, Constants.CAL_INTER_TYPE);

                        // 永续类型
                        SetComboBoxItemsSource(this.OrgCbb10, tagsDic, Constants.PERPETUAL_TYPE);

                        // 节假/工作日类型
                        SetComboBoxItemsSource(this.OrgCbb11, tagsDic, Constants.HOLIDAY_TYPE);

                        // 主体类型
                        SetComboBoxItemsSource(this.banktype, tagsDic, Constants.NCD_TYPE);

                        // 地区
                        SetComboBoxItemsSource(this.OrgCbb6, tagsDic, Constants.REGION_TYPE);

                        // 年份
                        SetComboBoxItemsSource(this.OrgCbb7, tagsDic, Constants.ISSUE_YEAR);

                        //中债隐含
                        SetComboBoxItemsSource(this.OrgCbb1, tagsDic, Constants.CDC_IMPLITED_RATING);

                        //中证隐含
                        SetComboBoxItemsSource(this.OrgCbb2, tagsDic, Constants.CSI_IMPLIED_RATING);

                        // 债项
                        SetComboBoxItemsSource(this.OrgCbb3, tagsDic, Constants.RATING_CURRENT);

                        // 行业
                        if (tagsDic != null && tagsDic.ContainsKey(Constants.INDUSTRY_TYPE))
                        {
                            this.OrgCbb5.InitData(tagsDic[Constants.INDUSTRY_TYPE].childTags);
                        }
                    }));
                }
            });
        }

        private void SetIssuer()
        {
            Task.Run(() => {
                string sql = @"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }

                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                            this.OrgCbb8.ItemsSource = list;
                    }));
                }
            });
        }

        private void SetGuarantor()
        {
            Task.Run(() => {
                string sql = @"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllGuarantors(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }

                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.OrgCbb9.ItemsSource = list;
                    }));
                }
            });
        }

        private void SetComboBoxItemsSource(MultiSelectComboBox cb, Dictionary<string, QuoteTag> tagsDic, string key)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            if (key == Constants.ROLETYPE)
            {
                dic["0001"] = "销交";
                dic["0010"] = "做市";
            }
            else if (key == Constants.SETTLTYPE)
            {
                dic["0010"] = "T+1";
                dic["0001"] = "T+0";
                dic["0011"] = "明+0";
                dic["0101"] = "远期";
            }
            else if (key == Constants.SIDE)
            {
                dic["0001"] = "Bid";
                dic["0010"] = "Ofr";
            }
            else if (key == Constants.FIRM)
            {
                dic["0001"] = "Firm";
                dic["0010"] = "NonFirm";
            }
            else if (key == Constants.RATING_CURRENT)
            {
                dic["00001"] = "AAA";
                dic["00010"] = "A-1";
                dic["00011"] = "AA+";
                dic["00100"] = "AA";
                dic["00101"] = "AA-";
                dic["00110"] = "A+";
                dic["xx"] = "其他";
            }
            else if (key == Constants.CDC_IMPLITED_RATING)
            {
                dic["00001"] = "AAA+";
                dic["00010"] = "AAA";
                dic["00011"] = "AAA-";
                dic["00100"] = "AA+";
                dic["00101"] = "AA";
                dic["00110"] = "AA(2)";
                dic["01000"] = "AA-";
                dic["xx"] = "其他";
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
                        dic[ct.childTagValue] = ct;
                    }
                }
            }
            cb.ItemsSource = dic;
        }

        private void SetFilterOptions(FilterOptions fo, Dictionary<string, QuoteTag> tagsDic)
        {
            if (fo == null || tagsDic == null)
                return;
            ObservableCollection<OptionItem> exchangelist = new ObservableCollection<OptionItem>();

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
                        //Dictionary<string, QuoteTag> tagsDic = IntentionOldHandler.Instance.QueryTags(true);

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

        private void MultiAutoCbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiAutoCompleteComboBox ccb = sender as MultiAutoCompleteComboBox;
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
                        this._focusTagsDic[key].Add(string.Empty);
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

            _focusTagsDic[fo.TagKey] = fo.TagsList;

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            AdjustShowAll_ToggleButtonCheck();
        }

        /// <summary>
        /// 0:全
        /// 1:R
        /// 2:C
        /// </summary>
        /// <param name="type"></param>
        public void SetDisplayMode(string type)
        {
            if (type == "1")
            {
                this.row1.Visibility = Visibility.Visible;

                this.row2.Visibility = Visibility.Collapsed;
                this.row3_2.Visibility = Visibility.Collapsed;
                this.row6.Visibility = Visibility.Collapsed;
            }
            else if (type == "2")
            {
                this.row1.Visibility = Visibility.Collapsed;

                this.row2.Visibility = Visibility.Visible;
                this.row3_2.Visibility = Visibility.Visible;
                this.row6.Visibility = Visibility.Visible;
            }
            else
            {
                this.row1.Visibility = Visibility.Visible;

                this.row2.Visibility = Visibility.Visible;
                this.row3_2.Visibility = Visibility.Visible;
                this.row6.Visibility = Visibility.Visible;
            }
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
                        //Dictionary<string, QuoteTag> tagsDic = IntentionOldHandler.Instance.QueryTags(true);

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

        private void TermCtrl_OKAction(object sender, RoutedEventArgs e)
        {
            IsAppTerm = true;

            this.isCheckShowAll = true;
            

            if (string.IsNullOrEmpty(this.TermCtrl.StartTermDY) && string.IsNullOrEmpty(this.TermCtrl.EndTermDY))
            {
                IsAppTerm = false;
                this.Term_All_ToggleButton.IsChecked = true;
            }
            else
            {
                IsAppTerm = true;

                this.Term_All_ToggleButton.IsChecked = false;

                foreach (ListBoxItem item in this.TermListBox.Items)
                {
                    item.IsSelected = false;
                }

                this.StartTermDY = this.TermCtrl.StartTermDY;
                this.EndTermDY = this.TermCtrl.EndTermDY;
            }

            SetFocusTagDic(Constants.TERM, this.TermListBox.SelectedItems);

            this.AdjustShowAll_ToggleButtonCheck();
            this.isCheckShowAll = false;
        }

        private void ClearTermCtrl()
        {
            IsAppTerm = false;
            this.TermCtrl.ClearValue();

            this.StartTermDY = string.Empty;
            this.EndTermDY = string.Empty;
        }

        private void CouponOKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.StartCoupon = this.SCoupon.Text;
            this.EndCoupon = this.ECoupon.Text;

            if (string.IsNullOrEmpty(this.StartCoupon) && string.IsNullOrEmpty(this.EndCoupon))
            {
                IsCoupon = false;
            }
            else
            {
                IsCoupon = true;
            }

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void ClearCouponCtrl()
        {
            this.IsCoupon = false;
            this.StartCoupon = this.SCoupon.Text = string.Empty;
            this.EndCoupon = this.ECoupon.Text = string.Empty;
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

        private void CouponChk_Checked(object sender, RoutedEventArgs e)
        {
            IsCoupon = true;
            this.AdjustShowAll_ToggleButtonCheck();
            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void CouponChk_Unchecked(object sender, RoutedEventArgs e)
        {
            IsCoupon = false;
            this.AdjustShowAll_ToggleButtonCheck();
            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void Coupon_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb.Focus();
                //CouponOKBtn_Click(null, null);
            }
        }

        private void SCoupon_LostFocus(object sender, RoutedEventArgs e)
        {
            CouponOKBtn_Click(null, null);
        }

        private void ECoupon_LostFocus(object sender, RoutedEventArgs e)
        {
            CouponOKBtn_Click(null, null);
        }

        private void ClearPriceCtrl()
        {
            this.StartPrice = this.SPrice.Text = string.Empty;
            this.EndPrice = this.EPrice.Text = string.Empty;
            AdjustShowAll_ToggleButtonCheck();
        }

        private void PriceOKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.StartPrice = this.SPrice.Text;
            this.EndPrice = this.EPrice.Text;

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void Price_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb.Focus();
                //PriceOKBtn_Click(null, null);
            }
        }

        private void Price_LostFocus(object sender, RoutedEventArgs e)
        {
            PriceOKBtn_Click(null, null);
        }

        private void ClearChinaBondValueCtrl()
        {
            this.StartChinaBondValue = this.SChinaBondValue.Text = string.Empty;
            this.EndChinaBondValue = this.EChinaBondValue.Text = string.Empty;
            AdjustShowAll_ToggleButtonCheck();
        }

        private void ChinaBondValueOKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.StartChinaBondValue = this.SChinaBondValue.Text;
            this.EndChinaBondValue = this.EChinaBondValue.Text;

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void ChinaBondValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb.Focus();
                //ChinaBondValueOKBtn_Click(null, null);
            }
        }

        private void ChinaBondValue_LostFocus(object sender, RoutedEventArgs e)
        {
            ChinaBondValueOKBtn_Click(null, null);
        }

        private void ClearBidBpCtrl()
        {
            this.StartBidBp = this.SBidBP.Text = string.Empty;
            this.EndBidBp = this.EBidBP.Text = string.Empty;
            AdjustShowAll_ToggleButtonCheck();
        }

        private void BidBpOKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.StartBidBp = this.SBidBP.Text;
            this.EndBidBp = this.EBidBP.Text;

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void Bp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb.Focus();
                //ChinaBondValueOKBtn_Click(null, null);
            }
        }

        private void Bp_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = null;
            regex = new Regex("^[0-9.-]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 10000;
            int min = -10000;

            string text = tb.Text.ToLower();

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 2)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;
        }

        private void BP_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            int max = 10000;
            int min = -10000;

            // 功能键有效性检查
            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (text.Length > 0)
                {
                    if (!decimal.TryParse(text, out d))
                    {
                        return;
                    }
                }

                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void BidBp_LostFocus(object sender, RoutedEventArgs e)
        {
            BidBpOKBtn_Click(null, null);
        }

        private void ClearOfrBpCtrl()
        {
            this.StartOfrBp = this.SOfrBP.Text = string.Empty;
            this.EndOfrBp = this.EOfrBP.Text = string.Empty;
            AdjustShowAll_ToggleButtonCheck();
        }

        private void OfrBpOKBtn_Click(object sender, RoutedEventArgs e)
        {
            this.StartOfrBp = this.SOfrBP.Text;
            this.EndOfrBp = this.EOfrBP.Text;

            this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = CouponActionEvent });
        }

        private void OfrBp_LostFocus(object sender, RoutedEventArgs e)
        {
            OfrBpOKBtn_Click(null, null);
        }

        private void isFuzzyCB_Checked(object sender, RoutedEventArgs e)
        {
            IsFuzzy = true;
            this.AdjustShowAll_ToggleButtonCheck();
        }

        private void isFuzzyCB_Unchecked(object sender, RoutedEventArgs e)
        {
            IsFuzzy = false;
            this.AdjustShowAll_ToggleButtonCheck();
        }

        /// <summary>
        /// 发行人输入筛选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void GuarantorCbb_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                //IEnumerable<Node> result = (this.filterDg.ItemsSource as ObservableCollection<Node>).Where(item => item.Title != null && item.Title.Contains(args.Pattern)
                //                          || (item.PinYin != null && item.PinYin.Contains(args.Pattern)));

                string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor where childTagName like '%{args.Pattern}%' or pinyin LIKE '%{args.Pattern}%' limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllGuarantors(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }
               args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void GuarantorCbb_LetterFilterChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                //IEnumerable<Node> result = (this.filterDg.ItemsSource as ObservableCollection<Node>).Where(item => item.Title != null && item.Title.Contains(args.Pattern)
                //                          || (item.PinYin != null && item.PinYin.Contains(args.Pattern)));

                string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor where pinyin like '{args.Pattern.ToUpper()}%' or pinyin LIKE '{args.Pattern.ToLower()}%' limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllGuarantors(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void IssuerCbb_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where childTagName like '%{args.Pattern}%' or pinyin LIKE '%{args.Pattern}%' limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void IssuerCbb_LetterFilterChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where pinyin like '{args.Pattern.ToUpper()}%' or pinyin LIKE '{args.Pattern.ToLower()}%' limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void IfTier2Perp_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.IF_TIER2_PERP))
            {
                this._focusTagsDic[Constants.IF_TIER2_PERP] = new List<string>();
            }

            this._focusTagsDic[Constants.IF_TIER2_PERP].Clear();

            this._focusTagsDic[Constants.IF_TIER2_PERP].Add("10");


            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void IfTier2Perp_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.IF_TIER2_PERP))
            {
                this._focusTagsDic[Constants.IF_TIER2_PERP] = new List<string>();
            }

            this._focusTagsDic[Constants.IF_TIER2_PERP].Clear();

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void KeChuang_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.INNOVATION_TYPE))
            {
                this._focusTagsDic[Constants.INNOVATION_TYPE] = new List<string>();
            }

            if (!this._focusTagsDic[Constants.INNOVATION_TYPE].Contains("0001"))
            {
                this._focusTagsDic[Constants.INNOVATION_TYPE].Add("0001");
            }

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void KeChuang_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.INNOVATION_TYPE))
            {
                this._focusTagsDic[Constants.INNOVATION_TYPE] = new List<string>();
            }

            if (this._focusTagsDic[Constants.INNOVATION_TYPE].Contains("0001"))
            {
                this._focusTagsDic[Constants.INNOVATION_TYPE].Remove("0001");
            }

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void ShuangChuang_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.INNOVATION_TYPE))
            {
                this._focusTagsDic[Constants.INNOVATION_TYPE] = new List<string>();
            }

            if (!this._focusTagsDic[Constants.INNOVATION_TYPE].Contains("0010"))
            {
                this._focusTagsDic[Constants.INNOVATION_TYPE].Add("0010");
            }

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void ShuangChuang_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.INNOVATION_TYPE))
            {
                this._focusTagsDic[Constants.INNOVATION_TYPE] = new List<string>();
            }

            if (this._focusTagsDic[Constants.INNOVATION_TYPE].Contains("0010"))
            {
                this._focusTagsDic[Constants.INNOVATION_TYPE].Remove("0010");
            }

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void WeiYue_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.IF_DEFAULT))
            {
                this._focusTagsDic[Constants.IF_DEFAULT] = new List<string>();
            }

            this._focusTagsDic[Constants.IF_DEFAULT].Clear();

            this._focusTagsDic[Constants.IF_DEFAULT].Add("10");


            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void WeiYue_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.IF_DEFAULT))
            {
                this._focusTagsDic[Constants.IF_DEFAULT] = new List<string>();
            }

            this._focusTagsDic[Constants.IF_DEFAULT].Clear();

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void ZuoShi_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.IF_MARKET_MAKING))
            {
                this._focusTagsDic[Constants.IF_MARKET_MAKING] = new List<string>();
            }

            this._focusTagsDic[Constants.IF_MARKET_MAKING].Clear();

            this._focusTagsDic[Constants.IF_MARKET_MAKING].Add("10");


            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }

        private void ZuoShi_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            if (!this._focusTagsDic.ContainsKey(Constants.IF_MARKET_MAKING))
            {
                this._focusTagsDic[Constants.IF_MARKET_MAKING] = new List<string>();
            }

            this._focusTagsDic[Constants.IF_MARKET_MAKING].Clear();

            if (!isUpdate)
            {
                FocusTags = JsonConvert.SerializeObject(_focusTagsDic);
            }

            this.AdjustShowAll_ToggleButtonCheck();
            isCheckShowAll = false;
        }
    }
}
