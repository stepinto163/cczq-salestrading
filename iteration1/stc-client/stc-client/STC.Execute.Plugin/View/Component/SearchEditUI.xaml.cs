using Ats.Foundation.Message;
using STC.Common.Model.Base;
using STC.Execute.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace STC.Execute.Plugin.View.Component
{
    /// <summary>
    /// SearchEditUI.xaml 的交互逻辑
    /// </summary>
    public partial class SearchEditUI : UserControl
    {
        private static readonly int MaxRow = 100;
        private List<BondsInfoPo> bondList;

        public event EventHandler<object> SearchChanged;
        public event EventHandler<object> AfterDeleted;

        //声明和注册路由事件\
        public static readonly RoutedEvent AfterDeletedReRoutedEvent =
            EventManager.RegisterRoutedEvent("AfterDeletedRe", RoutingStrategy.Bubble, typeof(EventHandler<object>), typeof(SearchEditUI));
        //CLR事件包装 点击删除按钮事件
        public event RoutedEventHandler AfterDeletedRe
        {
            add { this.AddHandler(AfterDeletedReRoutedEvent, value); }
            remove { this.RemoveHandler(AfterDeletedReRoutedEvent, value); }
        }


        /// <summary>
        /// 是否具有焦点属性
        /// </summary>
        public bool HasFocused
        {
            get { return (bool)GetValue(HasFocusedProperty); }
            set { SetValue(HasFocusedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasFocused.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasFocusedProperty =
            DependencyProperty.Register("HasFocused", typeof(bool), typeof(SearchEditUI), new PropertyMetadata((o,e)=>
            {
                var control = o as SearchEditUI;
                if (bool.Parse(e.NewValue.ToString()))
                {
                    control.navSearch.Focus();
                }
                else
                {
                    control.navSearch.Text = string.Empty;
                }
            }));



        private Button btDelete;

        public TextBox SearchBox { get { return navSearch; } }

        public bool IsModify { get; set; }
        public bool IsModified { get; private set; }


        /// <summary>
        /// 依赖属性——选择的债券
        /// </summary>
        public string SelectBondCode
        {
            get { return (string)GetValue(SelectBondCodeProperty); }
            set { SetValue(SelectBondCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectBondCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectBondCodeProperty =
            DependencyProperty.Register("SelectBondCode", typeof(string), typeof(SearchEditUI), new PropertyMetadata(""));


        public SearchEditUI()
        {
            InitializeComponent();
            navSearchPopup.Closed += NavSearchPopup_Closed;
            navSearch.ApplyTemplate();
            btDelete = navSearch.Template.FindName("btDelete", navSearch) as Button;
            this.Loaded += SearchEditUI_Loaded;
        }

        private void SearchEditUI_Loaded(object sender, RoutedEventArgs e)
        {
            //Initialize();

            
        }

        private bool _isChange = false;

        public void SetFocus()
        {
            IsChoosedBond = false;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(() => { navSearch.Focus(); navSearch.SelectAll();  _isChange = true; }));

        }

        private void NavSearchPopup_Closed(object sender, EventArgs e)
        {
            navSearchListBox.Items.Clear();
        }

        public void Initialize()
        {
            //if (bondList == null)
            //{
            //    bondList = new List<BondsInfoPo>(BondsHandler.Instance.InterestBondDict.Values);
            //    Console.WriteLine("SearchEdit Initialize: bondList={0}", bondList.Count);
            //}
            navSearch.Text = "";
            //navSearch.Focus();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(() => navSearch.Focus()));
            //Console.WriteLine("SearchEditUI navSearch focus={0}", navSearch.Focus());
            IsModify = false;
            IsModified = false;
        }

        private void navSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isChange)
                return;

            navSearchListBox.Items.Clear();
            Button btDelete = navSearch.Template.FindName("btDelete", navSearch) as Button;
            if (string.IsNullOrEmpty(navSearch.Text))
            {
                navSearchPopup.IsOpen = false;
                btDelete.Visibility = Visibility.Collapsed;
                return;
            }
            btDelete.Visibility = Visibility.Visible;
            //for (int i = 0; i < 4; i++)
            //{
            //    SeBondPo po = new SeBondPo();
            //    po.BondCode = navSearch.Text + "0" + i + "" + (i + 1) + ".IB";
            //    po.KeyWord = navSearch.Text;
            //    po.Prefix = (i + 1) + "" + i;
            //    po.Suffix = (i + 2) + ".IB";

            //    //string str = "<Span Foreground='#FFEBC8'>{0}</Span><Span Foreground='#F9C152'>{1}</Span><Span Foreground='#FFEBC8'>{2}</Span>";
            //    //po.BondCode = string.Format(str, po.Prefix, po.KeyWord, po.Suffix);
            //    po.ShortName = "19附息国债123";
            //    po.LeftTenor = "1.99Y+30Y";
            //    navSearchListBox.Items.Add(po);
            //}
            List<SeBondPo> list = FilterByKeyWord(navSearch.Text);
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count && i < MaxRow; i++)
                {
                    navSearchListBox.Items.Add(list[i]);
                }
                navSearchListBox.ScrollIntoView(list[0]);
                navSearchPopup.IsOpen = true;
            }
            if (IsModify)
            {
                IsModified = true;
            }
        }

        private List<SeBondPo> FilterByKeyWord(string keyWord)
        {
            List<SeBondPo> list = new List<SeBondPo>();
            List<BondsInfoPo> selectDatas = CommonUtils.GetBondsInfoPo(keyWord, 20);
            foreach (BondsInfoPo item in selectDatas)
            {
                try
                {
                    int index = item.Bond_ID.IndexOf(keyWord);
                    if (index >= 0)
                    {
                        SeBondPo po = new SeBondPo();
                        po.KeyWordIndex = index;
                        po.BondCode = item.Bond_ID;
                        po.Sn_Prefix = item.Short_Name;
                        po.LeftTenor = item.RemaingingTime;
                        if (string.IsNullOrEmpty(po.LeftTenor)) po.LeftTenor = "已到期";
                        po.Prefix = po.BondCode.Substring(0, index);
                        po.KeyWord = keyWord;
                        po.Suffix = po.BondCode.Substring(index + keyWord.Length);
                        list.Add(po);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.pinYin))
                        {
                            index = item.pinYin.IndexOf(keyWord);
                        }
                        if (index >= 0)
                        {
                        }
                        else
                        {
                            index = item.Short_Name.IndexOf(keyWord);
                        }
                        if (index >= 0)
                        {
                            SeBondPo po = new SeBondPo();
                            po.KeyWordIndex = 10000 + index;
                            po.BondCode = item.Bond_ID;
                            //po.ShortName = item.ShortName;
                            po.Prefix = item.Bond_ID;
                            po.LeftTenor = item.RemaingingTime;
                            if (string.IsNullOrEmpty(po.LeftTenor)) po.LeftTenor = "已到期";
                            po.Sn_Prefix = item.Short_Name.Substring(0, index);
                            po.Sn_KeyWord = item.Short_Name.Substring(index, keyWord.Length);
                            po.Sn_Suffix = item.Short_Name.Substring(index + keyWord.Length);
                            list.Add(po);
                        }
                    }
                }
                catch { }
                //if (list.Count >= 4)
                //{
                //    break;
                //}
            }
            if (list.Count > 0)
            {
                list.Sort(list[0]);
            }
            return list;
        }

        private void navSearchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void navSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Console.WriteLine("navSearch_PreviewKeyDown: {0}", e.Key);
            if (e.Key == Key.Down)
            {
                int index = 0;
                if (navSearchListBox.SelectedIndex >= 0)
                {
                    index = navSearchListBox.SelectedIndex + 1;
                    if (index >= navSearchListBox.Items.Count)
                    {
                        index = 0;
                    }
                }
                navSearchListBox.SelectedIndex = index;
            }
            else if (e.Key == Key.Up)
            {
                int index = navSearchListBox.Items.Count - 1;
                if (navSearchListBox.SelectedIndex >= 0)
                {
                    index = navSearchListBox.SelectedIndex - 1;
                    if (index < 0)
                    {
                        index = navSearchListBox.Items.Count - 1;
                    }
                }
                navSearchListBox.SelectedIndex = index;
            }
            else if (e.Key == Key.Escape)
            {
                navSearchPopup.IsOpen = false;
            }
            else if (e.Key == Key.Enter)
            {
                ChooseBond();

                navSearch.Text = string.Empty;
            }
            else if (e.Key == Key.Back)
            {
                this.IsModified = true;
            }
        }

        private void ChooseBond()
        {
            SeBondPo selectedBond = null;
            string result = "";
            if (navSearchListBox.Items.Count > 0)
            {
                int index = navSearchListBox.SelectedIndex;
                if (index < 0)
                {
                    index = 0;
                }
                SeBondPo po = navSearchListBox.Items[index] as SeBondPo;
                selectedBond = po;
                Console.WriteLine("SearchEditUI selected: index={0}, item={1}", index, MsgHelper.Serializer(po));
                result = po.BondCode;
                _isChange = false;
                navSearch.Text = result;
                navSearch.SelectionStart = result.Length;
                _isChange = true;
            }
            else
            {
                result = navSearch.Text;

                selectedBond = new SeBondPo();
                selectedBond.BondCode = navSearch.Text;
            }
            navSearchPopup.IsOpen = false;
            Console.WriteLine("SearchEditUI: enter={0}", result);
            SelectBondCode = result;
            SearchChanged?.Invoke(this, selectedBond);
        }

        public bool IsChoosedBond { get; set; }

        private void navSearchListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChooseBond();
        }

        //private bool isDeleteClick = false;
        /// <summary>
        /// 点击删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("btDelete_Click");
            this.IsModified = true;
            navSearch.Text = "";
            navSearch.Focus();
            //AfterDeleted?.Invoke(this, new object());
        }
        private void navSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            //MainWindowHandler.PassPreviewKey = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(DelayHandlerNavSearchLostFocus));
            //Console.WriteLine("navSearch_LostFocus");
            //Button btDelete = navSearch.Template.FindName("btDelete", navSearch) as Button;
            //if (btDelete.IsFocused)
            //{
            //    return;
            //}
            ////if (string.IsNullOrEmpty(navSearch.Text))
            ////{
            //    AfterDeleted?.Invoke(this, new object());
            ////}

        }

        private void DelayHandlerNavSearchLostFocus(object obj)
        {
            Thread.Sleep(100);
            this.Dispatcher.Invoke(() =>
            {
                if (!navSearch.IsFocused && !IsChoosedBond && !navSearchPopup.IsMouseOver && !btDelete.IsFocused)
                {
                    AfterDeleted?.Invoke(this, new object());
                    //触发路由事件 -点击删除安全
                    RoutedEventArgs routedEventArgs = new RoutedEventArgs(AfterDeletedReRoutedEvent,this);
                    this.RaiseEvent(routedEventArgs);
                }
            });
        }
        /// <summary>
        /// 外部调用强制丢失焦点
        /// </summary>
        /// <param name="obj"></param>
        public void NavSearchLostFocus()
        {
            //MainWindowHandler.PassPreviewKey = false;
            navSearchPopup.IsOpen = false;
            AfterDeleted?.Invoke(this, new object());

            RoutedEventArgs routedEventArgs = new RoutedEventArgs(AfterDeletedReRoutedEvent, this);
            this.RaiseEvent(routedEventArgs);
        }

        private void NavSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            //MainWindowHandler.PassPreviewKey = true;
        }

        private void navSearchListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            e.Handled = true;
        }
    }

    public class SeBondPo : IComparer<SeBondPo>
    {
        public string Prefix { get; set; }
        public string KeyWord { get; set; }
        public string BondCode { get; set; }
        public string Suffix { get; set; }
        public string ShortName { get; set; }
        public string Sn_Prefix { get; set; }
        public string Sn_KeyWord { get; set; }
        public string Sn_Suffix { get; set; }
        public string LeftTenor { get; set; }
        public int KeyWordIndex { get; set; }

        public int Compare(SeBondPo x, SeBondPo y)
        {
            int cmp1 = x.KeyWordIndex.CompareTo(y.KeyWordIndex);
            if (cmp1 == 0)
            {
                return x.BondCode.CompareTo(y.BondCode);
            }
            else
            {
                return cmp1;
            }
        }
    }
}
