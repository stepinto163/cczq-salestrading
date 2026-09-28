using STC.Common.Model.Admin;
using STC.XBond.Plugin.Setting.Utils.DataProcesser;
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
    /// STCAccountSearchTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class STCAccountSearchTextBox : UserControl
    {
        public event Action<StcTbookUsers> AfterSearch;
        private bool TextChangeFlag = true;
        private bool IsInit = true;
        public STCAccountSearchTextBox()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string),
             typeof(STCAccountSearchTextBox), new FrameworkPropertyMetadata("搜索", new PropertyChangedCallback(OnHintTextPropertyChange)));

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string),
            typeof(STCAccountSearchTextBox), new PropertyMetadata("", new PropertyChangedCallback(OnSearchTextPropertyChange)));

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(STCAccountSearchTextBox));


        static void OnHintTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            STCAccountSearchTextBox source = (STCAccountSearchTextBox)sender;
        }

        static void OnSearchTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            STCAccountSearchTextBox source = (STCAccountSearchTextBox)sender;

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

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dgAccountCode_MouseUp(null, null);
            }
        }

        private void tbSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgAccountCode.SelectedIndex > 0)
                    dgAccountCode.SelectedIndex--;
                dgAccountCode.ScrollIntoView(dgAccountCode.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgAccountCode.SelectedIndex < dgAccountCode.Items.Count - 1)
                    dgAccountCode.SelectedIndex++;
                dgAccountCode.ScrollIntoView(dgAccountCode.SelectedItem);
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

        private void dgAccountCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StcTbookUsers dv = dgAccountCode.SelectedItem as StcTbookUsers;
            dgAccountCode.SelectedIndex = -1;


            if (dv != null && this.SearchText != dv.Username)
            {
                this.SearchText = dv.Username;
            }

            popAccountCode.IsOpen = false;

            AfterSearch?.Invoke(dv);
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!TextChangeFlag)
                return;

            if (IsInit)
                return;

            if (!popAccountCode.IsOpen)
            {
                bool ifLeft = SystemParameters.MenuDropAlignment;
                if (ifLeft)
                {
                    popAccountCode.Width = 350;
                    popAccountCode.HorizontalOffset = (popAccountCode.Width - SearchTermTextBox.Width);
                }

                popAccountCode.IsOpen = true;
            }


            if (!string.IsNullOrEmpty(this.SearchTermTextBox.Text))
            {
                List<StcTbookUsers> result = new List<StcTbookUsers>();
                string txt = this.SearchTermTextBox.Text.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<StcTbookUsers> selectDatas = STCUserProcesser.Processer.GetTbookUsersPo(str, 20);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = STCUserProcesser.Processer.GetTbookUsersPo(SearchTermTextBox.Text.Trim(), 20);

                result = result.Distinct(new UserGridDataEquality()).ToList();

                dgAccountCode.DataContext = result;

                dgAccountCode.SelectedIndex = 0;
            }
            else
            {
                popAccountCode.IsOpen = false;
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

        public class UserGridDataEquality : IEqualityComparer<StcTbookUsers>
        {
            public bool Equals(StcTbookUsers x, StcTbookUsers y)
            {
                return x.Username.Equals(y.Username);
            }

            public int GetHashCode(StcTbookUsers obj)
            {
                return obj.Username.GetHashCode();
            }
        }
    }
}
