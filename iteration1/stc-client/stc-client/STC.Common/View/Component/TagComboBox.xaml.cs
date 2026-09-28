using STC.Common.Model.Quant;
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
using System.Windows.Threading;

namespace STC.Common.View.Component
{
    /// <summary>
    /// TagComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class TagComboBox : UserControl
    {
        private Dictionary<string, string> dic = new Dictionary<string, string>();
        private CollectionViewSource dic_cvs = new CollectionViewSource();

        public static readonly DependencyProperty SelectedValuesProperty = DependencyProperty.Register("SelectedValues", typeof(Dictionary<string, string>),
            typeof(TagComboBox));

        public Dictionary<string, string> SelectedValues
        {
            get { return (Dictionary<string, string>)GetValue(SelectedValuesProperty); }
            set
            {
                if (SelectedValues == null || SelectedValues.Count != value.Count)
                {
                    SetValue(SelectedValuesProperty, value);
                    ic.ItemsSource = value;
                }
            }
        }

        public static readonly DependencyProperty MaxItemCountProperty = DependencyProperty.Register("MaxItemCount", typeof(int),
            typeof(TagComboBox), new PropertyMetadata(10, new PropertyChangedCallback(OnMaxItemCountPropertyChange)));

        public int MaxItemCount
        {
            get { return (int)GetValue(MaxItemCountProperty); }
            set
            {
                SetValue(MaxItemCountProperty, value);
            }
        }

        private static void OnMaxItemCountPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {

        }
        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent SelectedChangedEvent = EventManager.RegisterRoutedEvent("SelectedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TagComboBox));

        public event RoutedEventHandler SelectedChanged
        {
            add { AddHandler(SelectedChangedEvent, value); }
            remove { RemoveHandler(SelectedChangedEvent, value); }
        }
        // 引发 SelectedChanged 事件  
        private void RaiseSelectedChangedEvent()
        {
            RaiseEvent(new RoutedEventArgs(SelectedChangedEvent));
        }
        public TagComboBox()
        {
            InitializeComponent();

            ReflashCVS();

            datagrid.DataContext = dic_cvs.View;

            SelectedValues = new Dictionary<string, string>();

            ic.ItemsSource = SelectedValues;


            this.Loaded += TagComboBox_Loaded;

        }

        public void setData(Dictionary<string, string> defaultDic) 
        {
            if (defaultDic != null) 
            {
                foreach (KeyValuePair<string, string> pair in defaultDic) 
                {
                    dic.Add(pair.Key, pair.Value);
                }
            }
            ReflashCVS();
        }

        private void TagComboBox_Loaded(object sender, RoutedEventArgs e)
        {

        }
        public void SetSelectValues(Dictionary<string, string> dic)
        {
            foreach (KeyValuePair<string, string> t in dic)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    AddNewItemToIC(t);
                }));
            }
        }

        private void ReflashCVS()
        {
            dic_cvs.Source = dic;
            dic_cvs.IsLiveFilteringRequested = true;
            dic_cvs.Filter += Dic_cvs_Filter;
            dic_cvs.View.Refresh();
        }

        private void Dic_cvs_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = true;
            string text = SearchTextBox.Text;
            if (e.Item != null && !string.IsNullOrEmpty(text))
            {
                KeyValuePair<string, string> item = (KeyValuePair<string, string>)e.Item;
                if (item.Value.IndexOf(text) < 0)
                {
                    e.Accepted = false;
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ShowPop();
        }

        private void datagrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (datagrid.SelectedItem != null)
            {
                KeyValuePair<string, string> key = (KeyValuePair<string, string>)datagrid.SelectedItem;
                AddNewItemToIC(key);
                RaiseSelectedChangedEvent();
            }
        }

        private void AddNewItemToIC(KeyValuePair<string, string> item)
        {
            if (SelectedValues == null)
            {
                SelectedValues = new Dictionary<string, string>();
            }
            //Fill_TextBlock.Visibility = Visibility.Collapsed;
            if (SelectedValues.Count == this.MaxItemCount)
            {
                Prompt2Seconds prom = new Prompt2Seconds();
                prom.PromptTitle = "标签提醒";
                prom.PromptContext = string.Format("选择的标签不能超过{0}个！", this.MaxItemCount);
                prom.ShowDialog();

                Fill_TextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                if (!SelectedValues.ContainsKey(item.Key) && SelectedValues.Count < this.MaxItemCount)
                {
                    SelectedValuesAdd(item);
                    if (SelectedValues.Count >= 3)
                    {
                        Fill_TextBlock.Visibility = Visibility.Visible;
                    }
                }
            }

            SearchTextBox.Text = string.Empty;
            ReflashCVS();

            ReflashItemControl();

            if (pop.IsOpen)
            {
                pop.IsOpen = false;
            }

        }

        private void SelectedValuesAdd(KeyValuePair<string, string> item)
        {
            Dictionary<string, string> temp = new Dictionary<string, string>(SelectedValues);
            SelectedValues.Clear();
            SelectedValues = new Dictionary<string, string>(temp);
            SelectedValues.Add(item.Key, item.Value);
        }

        private void close_btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            KeyValuePair<string, string> item = (KeyValuePair<string, string>)btn.DataContext;

            closeTag(item.Key);
            RaiseSelectedChangedEvent(); 
        }

        public void closeTag(string key) 
        {
            if (SelectedValues.ContainsKey(key))
            {
                SelectedValues[key] = null;
                SelectedValues.Remove(key);

                ReflashItemControl();
            }
        }

        public void clearTag() 
        {
            if (SelectedValues == null) 
            {
                return;
            }
            lock (SelectedValues)
            {
                if (SelectedValues.Count > 0)
                {

                    var pairs = SelectedValues.ToList();
                    foreach (KeyValuePair<string, string> pair in pairs)
                    {
                        SelectedValues[pair.Key] = null;
                        SelectedValues.Remove(pair.Key);
                        ReflashItemControl();
                    }
                }
            }
        }
        private void ReflashItemControl()
        {
            ic.ItemsSource = null;
            ic.ItemsSource = SelectedValues;

            if (SelectedValues.Count < this.MaxItemCount)
            {
                Fill_TextBlock.Visibility = Visibility.Collapsed;

                SearchTextBox.Width = stackpanel.ActualWidth - ic.ActualWidth;
                SearchTextBox.Focus();
                Keyboard.Focus(SearchTextBox);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => { Keyboard.Focus(SearchTextBox); }));

            }
        }

        private void StackPanel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            ReflashCVS();

            if (!string.IsNullOrEmpty(textbox.Text))
            {
                ShowPop();
            }
            else
            {
                if (pop.IsOpen)
                {
                    pop.IsOpen = false;
                }
            }

        }

        private void ShowPop()
        {
            if (!pop.IsOpen)
            {
                pop.IsOpen = true;
                datagrid.SelectedIndex = 0;
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // mainborder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#238F7E"));
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            mainborder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B3C3E"));
        }

        private void usercontrol_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("===== TagComboBox usercontrol_Initialized ======  ActualWidth = {0}  down_button.ActualWidth = {1}", this.ActualWidth, down_button.ActualWidth);

            stackpanel.Width = this.ActualWidth - down_button.ActualWidth;
        }

        private void usercontrol_Initialized(object sender, EventArgs e)
        {
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessSearchEnterKey();
                //SearchBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void ProcessSearchEnterKey()
        {
            string text = SearchTextBox.Text;
            int length = System.Text.Encoding.Default.GetByteCount(text);
            if (length > 16)
            {
                Prompt2Seconds prom = new Prompt2Seconds();
                prom.PromptTitle = "标签提醒";
                prom.PromptContext = string.Format("标签不能超过16字节！", this.MaxItemCount);
                prom.ShowDialog();
                return;
            }

            if (dic_cvs.View.IsEmpty == false)
            {
                int count = 0;
                KeyValuePair<string, string> lastSearced = new KeyValuePair<string, string>();
                foreach (KeyValuePair<string, string> item in dic_cvs.View)
                {
                    if (item.Value == text)
                    {
                        count++;
                        lastSearced = new KeyValuePair<string, string>(item.Key, item.Value);
                        break;
                    }
                }

                if (count > 0)
                {
                    AddNewItemToIC(lastSearced);
                }
                else
                {
                    //搜索出来有内容，但没有一模一样的，需要重新新增

                    AddNewItemToIC(new KeyValuePair<string, string>(AddNewItemToDic(text), text));
                }

            }
            else
            {
                //就没找到，需要重新新增
                AddNewItemToIC(new KeyValuePair<string, string>(AddNewItemToDic(text), text));
            }
        }

        private string AddNewItemToDic(string newText)
        {
            string retID = string.Empty;

            retID = DateTime.Now.ToString("yyMMddHHmmssfffff");


            //TODO:此处应该调用接口，新增新的标签，如果已经存在的话，也应该有相应的提示！

            Dictionary<string, string> temp = new Dictionary<string, string>(dic);
            dic.Clear();
            dic.Add(retID, newText);
            foreach (var item in temp)
            {
                dic.Add(item.Key, item.Value);
            }

            ReflashCVS();

            return retID;
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        public string getSearchTextBoxText() 
        { 
            return SearchTextBox.Text;
        }
    }
}
