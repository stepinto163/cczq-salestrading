using STC.Common.Model.Base;
using STC.PT.Plugin.Utils;
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

namespace STC.PT.Plugin.View.Component
{
    /// <summary>
    /// SearchTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class SearchTextBox : UserControl
    {
        public event Action<SimpleBondsInfoPo> AfterSearch;
        //public event Action<object, BondsInfoPo> AfterSearchData;
        //public object Data;
        private bool TextChangeFlag = true;
        private bool IsInit = true;
        private BondGridDataEquality _BondGridDataEq = new BondGridDataEquality();

        public SearchTextBox()
        {
            InitializeComponent();
        }

        ////"#0F0F0F"
        //static System.Windows.Media.Brush defaultMouseOverBackGround =  new SolidColorBrush(Color.FromRgb(15, 15, 15));
        // "#FF3F44"
        static System.Windows.Media.Brush defaultMouseOverBorderBrush = new SolidColorBrush(_borderColor); //new SolidColorBrush(Color.FromRgb(255, 63, 68));
        private static Color _greenColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#70136C5E");
        private static Brush _greenBrush = new SolidColorBrush(_greenColor);
        // #E63034
        static System.Windows.Media.Brush defaultFocusBorderBrush = new SolidColorBrush(_borderColor); //new SolidColorBrush(Color.FromRgb(230, 48, 52));
        private static Color _borderColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#238F7E");
        private static Brush _borderBrush = new SolidColorBrush(_borderColor);

        public System.Windows.Media.Brush MouseOverBorderBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(MouseOverBorderBrushProperty); }
            set { SetValue(MouseOverBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseOverBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverBorderBrushProperty =
            DependencyProperty.Register("MouseOverBorderBrush", typeof(System.Windows.Media.Brush), typeof(SearchTextBox), new FrameworkPropertyMetadata(_greenBrush));

        public System.Windows.Media.Brush FocusBorderBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(FocusBorderBrushProperty); }
            set { SetValue(FocusBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusBorderBrushProperty =
            DependencyProperty.Register("FocusBorderBrush", typeof(System.Windows.Media.Brush), typeof(SearchTextBox), new FrameworkPropertyMetadata(_borderBrush));



        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string),
             typeof(SearchTextBox), new FrameworkPropertyMetadata("搜索", new PropertyChangedCallback(OnHintTextPropertyChange)));

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string),
            typeof(SearchTextBox), new PropertyMetadata("", new PropertyChangedCallback(OnSearchTextPropertyChange)));

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchTextBox));


        static void OnHintTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            SearchTextBox source = (SearchTextBox)sender;
            source.HintTextBox.Text = args.NewValue.ToString();
        }

        static void OnSearchTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            SearchTextBox source = (SearchTextBox)sender;

            string newValue = args.NewValue.ToString();
            if (source.SearchText != newValue)
            {
                source.SearchText = newValue;
            }
            if (source.SearchTermTextBox.Text != newValue)
            {
                source.SearchTermTextBox.Text = newValue;
            }
        }



        public string ListMarketFilter
        {
            get { return (string)GetValue(ListMarketFilterProperty); }
            set { SetValue(ListMarketFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListMarketFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListMarketFilterProperty =
            DependencyProperty.Register("ListMarketFilter", typeof(string), typeof(SearchTextBox));



        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set
            {
                SetValue(SearchTextProperty, value);
                if (string.IsNullOrEmpty(value))
                {
                    dgBondCode.DataContext = null;
                }
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
            this.SearchTermTextBox.SelectAll();
            IsInit = false;
        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void dgBondCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SimpleBondsInfoPo dv = dgBondCode.SelectedItem as SimpleBondsInfoPo;
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

        public void ClearSerchTxt()
        {
            SearchTermTextBox.Text = string.Empty;
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
                

            //if (!string.IsNullOrEmpty(this.SearchTermTextBox.Text))
            //{
            //    List<BondsInfoPo> result = new List<BondsInfoPo>();
            //    string txt = this.SearchTermTextBox.Text.Replace("'", "");
            //    if (txt.Contains(" "))
            //    {
            //        string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            //        foreach (string str in arr)
            //        {
            //            List<BondsInfoPo> selectDatas = CommonUtils.GetBondsInfoPo(str, 20, ListMarketFilter);
            //            result.AddRange(selectDatas);
            //        }

            //    }
            //    else
            //        result = CommonUtils.GetBondsInfoPo(SearchTermTextBox.Text.Trim(), 20, ListMarketFilter);

            //    result = result.Distinct(_BondGridDataEq).ToList();
            //    //result.Sort(_BondGridDataSort);

            //    dgBondCode.DataContext = result;

            //    dgBondCode.SelectedIndex = 0;

            //    RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
            //}
            //else
            //{
            //    popBondCode.IsOpen = false;
            //    this.SearchText = string.Empty;
            //}
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
            //SearchTermTextBox.Focus();
            IsInit = false;
        }

        public void FocusText()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
new Action(() => Keyboard.Focus(this.SearchTermTextBox)));
        }
    }

    public class BondGridDataEquality : IEqualityComparer<SimpleBondsInfoPo>
    {
        public bool Equals(SimpleBondsInfoPo x, SimpleBondsInfoPo y)
        {
            return x.Bond_ID.Equals(y.Bond_ID);
        }

        public int GetHashCode(SimpleBondsInfoPo obj)
        {
            return obj.Bond_ID.GetHashCode();
        }
    }
}
