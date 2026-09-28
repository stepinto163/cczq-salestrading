using STC.Common.Model.Base;
using STC.XBond.Plugin.Utils;
using System;
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

namespace STC.XBond.Plugin.Setting.Component
{
    /// <summary>
    /// XBondSearchTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class XBondSearchTextBox : UserControl
    {
        public event Action<BondsInfoPo> AfterSearch;
        //public event Action<object, BondsInfoPo> AfterSearchData;
        //public object Data;
        private bool TextChangeFlag = true;
        private bool IsInit = true;
        private BondGridDataEquality _BondGridDataEq = new BondGridDataEquality();

        public XBondSearchTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string),
             typeof(XBondSearchTextBox), new FrameworkPropertyMetadata("搜索", new PropertyChangedCallback(OnHintTextPropertyChange)));

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string),
            typeof(XBondSearchTextBox), new PropertyMetadata("", new PropertyChangedCallback(OnSearchTextPropertyChange)));

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XBondSearchTextBox));


        static void OnHintTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            XBondSearchTextBox source = (XBondSearchTextBox)sender;
            source.HintTextBox.Text = args.NewValue.ToString();
        }

        static void OnSearchTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            XBondSearchTextBox source = (XBondSearchTextBox)sender;

            var newValue = args.NewValue.ToString();
            if (source.SearchText != newValue)
            {
                source.SearchText = newValue;
            }
            if (source.SearchTermTextBox.Text != newValue)
            {
                source.SearchTermTextBox.Text = newValue;
            }
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set
            {
                SetValue(SearchTextProperty, value);
                RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
            }
        }

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        /// <summary>
        /// 搜索框全选
        /// </summary>
        public void SelectAll()
        {
            this.SearchTermTextBox.SelectAll();
            this.SearchTermTextBox.Focus();
        }

        //public void SetButtonVisible(Visibility status)
        //{
        //    object obj = SearchTermTextBox.Template.FindName("btnDelete", SearchTermTextBox);
        //    if (obj != null)
        //    {
        //        Button button = obj as Button;
        //        if (button != null)
        //            button.Visibility = status;
        //    }
        //}

        //public void SetSearchTxt(string txt)
        //{
        //    IsInit = true;

        //    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
        //        new Action(() => SearchTermTextBox.Focus()));

        //    SearchTermTextBox.Text = txt;
        //    SearchTermTextBox.SelectAll();
        //    //tbSearch.Focus();

        //    IsInit = false;
        //}

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dgBondCode_MouseUp(null, null);
            }
        }

        private void tbSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgBondCode.SelectedIndex > 0)
                    dgBondCode.SelectedIndex--;
                dgBondCode.ScrollIntoView(dgBondCode.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgBondCode.SelectedIndex < dgBondCode.Items.Count - 1)
                    dgBondCode.SelectedIndex++;
                dgBondCode.ScrollIntoView(dgBondCode.SelectedItem);
            }
            else if (e.Key == Key.Escape)
            {
                if (this.SearchText == string.Empty)
                {
                    this.SearchTermTextBox.Text = string.Empty;
                }
                else
                {
                    this.SearchText = string.Empty;
                }
            }
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            IsInit = false;
        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void dgBondCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BondsInfoPo dv = dgBondCode.SelectedItem as BondsInfoPo;
            dgBondCode.SelectedIndex = -1;
            //if (dv != null)


            if (dv != null && this.SearchText != dv.Bond_ID)
            {
                //this.SearchTermTextBox.Text = dv.Bond_ID;
                this.SearchText = dv.Bond_ID;
            }

            popBondCode.IsOpen = false;

            AfterSearch?.Invoke(dv);
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (this.SearchText != this.SearchTermTextBox.Text)
            //    this.SearchText = this.SearchTermTextBox.Text;

            if (!TextChangeFlag)
                return;

            if (IsInit)
                return;

            if (!popBondCode.IsOpen)
            {
                popBondCode.IsOpen = true;
            }
                

            if (!string.IsNullOrEmpty(this.SearchTermTextBox.Text))
            {
                List<BondsInfoPo> result = new List<BondsInfoPo>();
                string txt = this.SearchTermTextBox.Text.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<BondsInfoPo> selectDatas = CommonUtils.GetODMBondsInfoPo(str, 20);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = CommonUtils.GetODMBondsInfoPo(SearchTermTextBox.Text.Trim(), 20);

                result = result.Distinct(_BondGridDataEq).ToList();
                //result.Sort(_BondGridDataSort);

                dgBondCode.DataContext = result;

                dgBondCode.SelectedIndex = 0;
            }
            else
            {
                popBondCode.IsOpen = false;
                this.SearchText = string.Empty;
            }
        }

        private void tbTraderAInfoChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = true;
        }

        private void tbTraderAInfoChange_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = false;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            IsInit = true;
            if (this.SearchText == string.Empty)
            {
                this.SearchTermTextBox.Text = string.Empty;
            }
            else
            {
                this.SearchText = string.Empty;
            }
            SearchTermTextBox.Focus();
            IsInit = false;
        }
    }

    public class BondGridDataEquality : IEqualityComparer<BondsInfoPo>
    {
        public bool Equals(BondsInfoPo x, BondsInfoPo y)
        {
            return x.Bond_ID.Equals(y.Bond_ID);
        }

        public int GetHashCode(BondsInfoPo obj)
        {
            return obj.Bond_ID.GetHashCode();
        }
    }
}
