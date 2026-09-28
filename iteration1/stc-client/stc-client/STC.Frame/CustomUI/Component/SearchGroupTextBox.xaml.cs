using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Frame.ViewModel;
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

namespace STC.Frame.CustomUI.Component
{
    /// <summary>
    /// SearchGroupTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class SearchGroupTextBox : UserControl
    {
        public event Action<HomeGroupInfo> AfterSearch;
        private bool TextChangeFlag = true;
        private bool IsInit = true;
        private HomeGroupEquality _BondGridDataEq = new HomeGroupEquality();
        public object DataContentModel { get; set; }

        public event Action<HomeGroupInfo, object> AfterSearchBond;

        public SearchGroupTextBox()
        {
            InitializeComponent();
        }
        static System.Windows.Media.Brush defaultMouseOverBorderBrush = new SolidColorBrush(_borderColor); //new SolidColorBrush(Color.FromRgb(255, 63, 68));
        private static Color _greenColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#70136C5E");
        private static Brush _greenBrush = new SolidColorBrush(_greenColor);
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
            DependencyProperty.Register("MouseOverBorderBrush", typeof(System.Windows.Media.Brush), typeof(SearchGroupTextBox), new FrameworkPropertyMetadata(_greenBrush));

        public System.Windows.Media.Brush FocusBorderBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(FocusBorderBrushProperty); }
            set { SetValue(FocusBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusBorderBrushProperty =
            DependencyProperty.Register("FocusBorderBrush", typeof(System.Windows.Media.Brush), typeof(SearchGroupTextBox), new FrameworkPropertyMetadata(_borderBrush));



        public static readonly DependencyProperty HintGroupTextProperty = DependencyProperty.Register("HintGroupText", typeof(string),
             typeof(SearchGroupTextBox), new FrameworkPropertyMetadata("搜索组..."));

        public static readonly DependencyProperty SearchGroupTextProperty = DependencyProperty.Register("SearchGroupText", typeof(string),
            typeof(SearchGroupTextBox), new PropertyMetadata("", new PropertyChangedCallback(OnSearchGroupTextPropertyChange)));

        public bool IsPopUpShow
        {
            get { return (bool)GetValue(IsPopUpShowProperty); }
            set { SetValue(IsPopUpShowProperty, value); }
        }

        public static readonly DependencyProperty IsPopUpShowProperty = DependencyProperty.Register("IsPopUpShow", typeof(bool), typeof(SearchGroupTextBox), null);


        public SolidColorBrush TextForeground
        {
            get { return (SolidColorBrush)GetValue(TextForegroundProperty); }
            set { SetValue(TextForegroundProperty, value); }
        }
        public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register("TextForeground", typeof(SolidColorBrush), typeof(SearchGroupTextBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTextForegroudPropertyChange)));

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchGroupTextBox));

        static void OnTextForegroudPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            SearchGroupTextBox source = (SearchGroupTextBox)sender;
            source.SearchTermTextBox.Foreground = (SolidColorBrush)args.NewValue;
        }

        static void OnSearchGroupTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            SearchGroupTextBox source = (SearchGroupTextBox)sender;

            string newValue = args.NewValue.ToString();
            if (source.SearchGroupText != newValue)
            {
                source.SearchGroupText = newValue;
            }
            if (source.SearchTermTextBox.Text != newValue)
            {
                source.SearchTermTextBox.Text = newValue;
            }

            if (source.IsMagnifierMode)
            {
                if (!string.IsNullOrEmpty(source.SearchTermTextBox.Text))
                {
                    source.SearchGrid.Visibility = Visibility.Visible;
                    source.SearchImage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (!source.SearchTermTextBox.IsFocused)
                    {
                        source.SearchGrid.Visibility = Visibility.Collapsed;
                        source.SearchImage.Visibility = Visibility.Visible;
                    }
                }
            }

        }



        public string ListMarketFilter
        {
            get { return (string)GetValue(ListMarketFilterProperty); }
            set { SetValue(ListMarketFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListMarketFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListMarketFilterProperty =
            DependencyProperty.Register("ListMarketFilter", typeof(string), typeof(SearchGroupTextBox));



        public string SearchGroupText
        {
            get { return (string)GetValue(SearchGroupTextProperty); }
            set
            {
                SetValue(SearchGroupTextProperty, value);
                if (string.IsNullOrEmpty(value))
                {
                    dgBondCode.DataContext = null;
                }
                RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
            }
        }

        public string HintGroupText
        {
            get { return (string)GetValue(HintGroupTextProperty); }
            set { SetValue(HintGroupTextProperty, value); }
        }

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }



        public bool IsGroupFuzzy
        {
            get { return (bool)GetValue(IsGroupFuzzyProperty); }
            set { SetValue(IsGroupFuzzyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGroupFuzzy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGroupFuzzyProperty =
            DependencyProperty.Register("IsGroupFuzzy", typeof(bool), typeof(SearchGroupTextBox), new PropertyMetadata(false));



        public bool IsMagnifierMode
        {
            get { return (bool)GetValue(IsMagnifierModeProperty); }
            set { SetValue(IsMagnifierModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMagnifierMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMagnifierModeProperty =
            DependencyProperty.Register("IsMagnifierMode", typeof(bool), typeof(SearchGroupTextBox), new PropertyMetadata(false, new PropertyChangedCallback(OnIsMagnifierModeChange)));

        static void OnIsMagnifierModeChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            SearchGroupTextBox source = (SearchGroupTextBox)sender;

            bool newValue = (bool)args.NewValue;
            if (newValue == false)
            {
                source.SearchImage.Visibility = Visibility.Collapsed;
                source.SearchGrid.Visibility = Visibility.Visible;
            }
            else
            {
                source.SearchImage.Visibility = Visibility.Visible;
                source.SearchGrid.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 搜索框全选
        /// </summary>
        public void SelectAll()
        {
            this.SearchTermTextBox.SelectAll();
            this.SearchTermTextBox.Focus();
        }

        private bool isFromEnter = false;

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            isFromEnter = false;
            AppUtils.IsInnerInput = true;
            if (e.Key == Key.Enter)
            {
                isFromEnter = true;
                dgBondCode_MouseUp(null, null);
            }
        }

        private void tbSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgBondCode.SelectedIndex > 0)
                    dgBondCode.SelectedIndex--;
                if (dgBondCode.SelectedItem != null)
                    dgBondCode.ScrollIntoView(dgBondCode.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgBondCode.SelectedIndex < dgBondCode.Items.Count - 1)
                    dgBondCode.SelectedIndex++;
                if (dgBondCode.SelectedItem != null)
                    dgBondCode.ScrollIntoView(dgBondCode.SelectedItem);
            }
            else if (e.Key == Key.Escape)
            {
                if (this.SearchGroupText == string.Empty)
                {
                    this.SearchTermTextBox.Text = string.Empty;
                }
                else
                {
                    this.SearchGroupText = string.Empty;
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
            if (!popBondCode.IsOpen)
                AppUtils.IsInnerInput = false;

            if (IsMagnifierMode)
            {
                if (string.IsNullOrEmpty(this.SearchTermTextBox.Text))
                {
                    this.SearchImage.Visibility = Visibility.Visible;
                    this.SearchGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgBondCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AppUtils.IsInnerInput = false;
            HomeGroupInfo dv = dgBondCode.SelectedItem as HomeGroupInfo;
            DataContentModel = this.DataContext;
            dgBondCode.SelectedIndex = -1;

            if (dv != null)
            {
                this.SearchGroupText = dv.groupname;
            }
            else
            {
                if (IsGroupFuzzy)
                {
                    this.SearchGroupText = this.SearchTermTextBox.Text;
                }
            }

            popBondCode.IsOpen = false;
            IsPopUpShow = popBondCode.IsOpen;

            AfterSearch?.Invoke(dv);
            AfterSearchBond?.Invoke(dv, this.DataContext);
        }

        public void ClearSerchTxt()
        {
            SearchTermTextBox.Text = string.Empty;
        }

        public void SetPop(bool flag)
        {
            popBondCode.IsOpen = flag;
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInit)
                return;

            AppUtils.IsInnerInput = true;
            if (isFromEnter)
            {
                AppUtils.IsInnerInput = false;
            }

            if (!TextChangeFlag)
                return;

            if (!popBondCode.IsOpen)
            {
                popBondCode.IsOpen = true;
            }
            IsPopUpShow = popBondCode.IsOpen;


            if (!string.IsNullOrEmpty(this.SearchTermTextBox.Text))
            {
                List<HomeGroupInfo> result = new List<HomeGroupInfo>();
                string txt = this.SearchTermTextBox.Text.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<HomeGroupInfo> selectDatas = SecurityGroupManagerViewModel.GetHomeGroupInfo(str, 20);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = SecurityGroupManagerViewModel.GetHomeGroupInfo(SearchTermTextBox.Text.Trim(), 20);

                result = result.Distinct(_BondGridDataEq).ToList();

                dgBondCode.DataContext = result;

                if (!IsGroupFuzzy)
                {
                    dgBondCode.SelectedIndex = 0;
                }

                RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
            }
            else
            {
                popBondCode.IsOpen = false;
                IsPopUpShow = popBondCode.IsOpen;
                this.SearchGroupText = string.Empty;
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
            if (this.SearchGroupText == string.Empty)
            {
                this.SearchTermTextBox.Text = string.Empty;
            }
            else
            {
                this.SearchGroupText = string.Empty;
            }
            IsInit = false;

            IsPopUpShow = popBondCode.IsOpen;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
        }

        public void FocusText()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
             new Action(() => Keyboard.Focus(this.SearchTermTextBox)));
        }

        private void SearchImage_Click(object sender, RoutedEventArgs e)
        {
            if (IsMagnifierMode)
            {
                this.SearchGrid.Visibility = Visibility.Visible;
                this.SearchTermTextBox.Focus();
                this.SearchImage.Visibility = Visibility.Collapsed;
            }
        }
    }

    public class HomeGroupEquality : IEqualityComparer<HomeGroupInfo>
    {
        public bool Equals(HomeGroupInfo x, HomeGroupInfo y)
        {
            return x.groupname.Equals(y.groupname);
        }

        public int GetHashCode(HomeGroupInfo obj)
        {
            return obj.groupname.GetHashCode();
        }
    }
}
