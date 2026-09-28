using com.gateway.log;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Common.View.Component
{
    /// <summary>
    /// MultiSelectComboBox.xaml 的交互逻辑
    /// </summary>
    /// <summary>
    /// MultiSelectComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class MultiSelectComboBox : UserControl
    {
        private Timer _interval;

        //private const string ALL = "All";
        private const string ALL = "全部";

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(MultiSelectComboBox), new PropertyMetadata(false, OnIsEdiableChanged));

        private static void OnIsEdiableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = d as MultiSelectComboBox;

            if (control != null && e.NewValue != null)
            {
                if ((bool)e.NewValue)
                {
                    if (control._interval == null)
                    {
                        control._interval = new Timer(200);
                        control._interval.AutoReset = true;
                        control._interval.Elapsed += new ElapsedEventHandler(control._interval_Elapsed);
                    }
                }
                else
                {
                    if (control._interval != null)
                    {
                        control._interval.Close();
                        control._interval = null;
                    }
                }
            }
        }

        /// <summary>
        /// 是否允许使用 IsEnabled
        /// </summary>
        public bool IsCanUse
        {
            get { return (bool)GetValue(IsCanUseProperty); }
            set { SetValue(IsCanUseProperty, value); }
        }


        public static readonly DependencyProperty IsCanUseProperty = DependencyProperty.Register("IsCanUse", typeof(bool), typeof(MultiSelectComboBox), new PropertyMetadata(true, OnIsCanUse_Changed));


        private static void OnIsCanUse_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox treeCmb = d as MultiSelectComboBox;
            if (treeCmb != null)
            {
                treeCmb.MultiSelectCombo.IsEnabled = (bool)e.NewValue;
                treeCmb.count_text.IsEnabled = (bool)e.NewValue;
            }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiSelectComboBox), new PropertyMetadata("All", OnDefaultChanged));

        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private ObservableCollection<Node> _nodeList;

        private TextBox _SearchTextBox;
        private TextBox SearchTextBox
        {
            get
            {
                if (_SearchTextBox == null)
                {
                    if (MultiSelectCombo.Template != null)
                    {
                        Popup p = MultiSelectCombo.Template.FindName("PART_Popup", MultiSelectCombo) as Popup;
                        if (p != null)
                        {
                            _SearchTextBox = p.FindName("SearchTextBox") as TextBox;
                        }
                    }
                    
                }
                return _SearchTextBox;
            }
        }

        public MultiSelectComboBox()
        {
            InitializeComponent();
            _nodeList = new ObservableCollection<Node>();
        }

        void _interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            // pause the timer
            this._interval.Stop();

            // show the drop down when user starts typing then stops
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                this.ViewSource.View?.Refresh();
            },
            System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
             DependencyProperty.Register("ItemsSource", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
        new PropertyChangedCallback(MultiSelectComboBox.OnItemsSourceChanged)));

        //public static readonly DependencyProperty SelectedItemsProperty =
        // DependencyProperty.Register("SelectedItems", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
        //new PropertyChangedCallback(MultiSelectComboBox.OnSelectedItemsChanged)));

        public static readonly DependencyProperty SelectedTextsProperty =
           DependencyProperty.Register("SelectedTexts", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata("All"));

        public static readonly DependencyProperty SelectedValuesProperty =
          DependencyProperty.Register("SelectedValues", typeof(string), typeof(MultiSelectComboBox), 
              new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedValuesChanged)));

        //public static readonly DependencyProperty DefaultTextProperty =
        //DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(string.Empty,
        //new PropertyChangedCallback(MultiSelectComboBox.OnDefaultChanged)));

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiSelectComboBox));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        public Dictionary<string, object> ItemsSource
        {
            get { return (Dictionary<string, object>)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        ///// <summary>
        ///// 不用
        ///// </summary>
        //public Dictionary<string, object> SelectedItems
        //{
        //    get { return (Dictionary<string, object>)GetValue(SelectedItemsProperty); }
        //    set
        //    {
        //        SetValue(SelectedItemsProperty, value);
        //    }
        //}

        public bool IsAll
        {
            get
            {
                int selectedItemsCount = _nodeList.Count(item => item.IsSelected == true);
                return selectedItemsCount == 0;// || selectedItemsCount == _nodeList.Count;
            }
        }

        /// <summary>
        /// 选择的文本列表 ,分割
        /// </summary>
        public string SelectedTexts
        {
            get { return (string)GetValue(SelectedTextsProperty); }
            set { SetValue(SelectedTextsProperty, value); }
        }

        /// <summary>
        /// 选择的值列表 ,分割
        /// </summary>
        public string SelectedValues
        {
            get { return (string)GetValue(SelectedValuesProperty); }
            set { SetValue(SelectedValuesProperty, value);
                RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            }
        }

        //public string DefaultText
        //{
        //    get { return (string)GetValue(DefaultTextProperty); }
        //    set { SetValue(DefaultTextProperty, value); }
        //}



        public int SelectedCount
        {
            get { return (int)GetValue(SelectedCountProperty); }
            set { SetValue(SelectedCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCountProperty =
            DependencyProperty.Register("SelectedCount", typeof(int), typeof(MultiSelectComboBox), new PropertyMetadata(0, OnSelectedCountChanged));

        private static void OnSelectedCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            MultiSelectComboBox control = (MultiSelectComboBox)d;
            if (control != null)
            {
                int count = e.NewValue.ConvertToInt();
                if (count > 0)
                {
                    control.count_text.Visibility = Visibility.Visible;
                }
                else
                {
                    control.count_text.Visibility = Visibility.Collapsed;
                }
            }
        }



        #endregion

        #region Events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            if (control != null)
                control.DisplayInControl();
        }

        private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == null)
            {
                return;
            }
            MultiSelectComboBox control = (MultiSelectComboBox)d;

            if (control.SelectedValues != control.GetSelectedStr())
            {
                string selectedValues = e.NewValue.ToString();
                string[] valuesArr = selectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (Node node in control._nodeList)
                {
                    node.IsSelected = false;
                }
                if (valuesArr != null && valuesArr.Length > 0 && control._nodeList != null && control._nodeList.Count > 0)
                {
                    foreach (Node node in control._nodeList)
                    {
                        node.IsSelected = valuesArr.Any(item => item == node.Key);
                    }

                    //int _selectedCount = 0;
                    //foreach (Node s in control._nodeList)
                    //{
                    //    if (s.IsSelected && s.Title != ALL)
                    //        _selectedCount++;
                    //}
                    //if (_selectedCount != 0 && _selectedCount == control._nodeList.Count)
                    //{
                    //    //control._nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = true;
                    //    CheckBox chk = control.MultiSelectCombo.Template.FindName("allChk", control.MultiSelectCombo) as CheckBox;
                    //    if (chk != null)
                    //    {
                    //        chk.IsChecked = true;
                    //    }
                    //}
                    //else if (_selectedCount != 0)
                    //{
                    //    CheckBox chk = control.MultiSelectCombo.Template.FindName("allChk", control.MultiSelectCombo) as CheckBox;
                    //    if (chk != null)
                    //    {
                    //        chk.IsChecked = null;
                    //    }
                    //}
                    //else
                    //{
                    //    //control._nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = false;
                    //    CheckBox chk = control.MultiSelectCombo.Template.FindName("allChk", control.MultiSelectCombo) as CheckBox;
                    //    if (chk != null)
                    //    {
                    //        chk.IsChecked = false;
                    //    }
                    //}
                }

                control.SetText();
            }
        }

        private static void OnDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                MultiSelectComboBox control = (MultiSelectComboBox)d;
                control.SelectedTexts = e.NewValue.ToString();
            }
            
            //if (control.IsAll)
            //{
            //    control.SetText();
            //}
            //control.DisplayInControl();
        }

        public void ClearSelection()
        {
            foreach(Node node in _nodeList)
            {
                node.IsSelected = false;
            }
            SetText();
            if (this.SearchTextBox != null)
            {
                this.SearchTextBox.Text = string.Empty;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedBox = (CheckBox)sender;

            if (clickedBox.Content.ToString() == ALL)
            {
                if (clickedBox.IsChecked.Value)
                {
                    if (this.ViewSource.View != null)
                    {
                        foreach (Node node in this.ViewSource.View)
                        {
                            node.IsSelected = true;
                        }
                    }
                }
                else
                {
                    if (this.ViewSource.View != null)
                    {
                        foreach (Node node in this.ViewSource.View)
                        {
                            node.IsSelected = false;
                        }
                    }
                }
            }
            else
            {
                //int _selectedCount = 0;
                //foreach (Node s in _nodeList)
                //{
                //    if (s.IsSelected && s.Title != ALL)
                //        _selectedCount++;
                //}
                //if (_selectedCount != 0 && _selectedCount == _nodeList.Count)
                //{
                //    //_nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = true;
                //    CheckBox chk = this.MultiSelectCombo.Template.FindName("allChk", MultiSelectCombo) as CheckBox;
                //    if (chk != null)
                //    {
                //        chk.IsChecked = true;
                //    }
                //}
                //else if (_selectedCount != 0)
                //{
                //    CheckBox chk = this.MultiSelectCombo.Template.FindName("allChk", MultiSelectCombo) as CheckBox;
                //    if (chk != null)
                //    {
                //        chk.IsChecked = null;
                //    }
                //}
                //else
                //{
                //    //_nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = false;
                //    CheckBox chk = this.MultiSelectCombo.Template.FindName("allChk", MultiSelectCombo) as CheckBox;
                //    if (chk != null)
                //    {
                //        chk.IsChecked = false;
                //    }
                //}
            }

            //SetSelectedItems();
            this.SetText();
        }
        #endregion

        #region Methods
        //private void SelectNodes()
        //{
        //    int _selectedCount = 0;
        //    foreach (KeyValuePair<string, object> keyValue in SelectedItems)
        //    {
        //        if (keyValue.Key != ALL)
        //            _selectedCount++;
        //        Node node = _nodeList.FirstOrDefault(i => i.Title == keyValue.Key);
        //        if (node != null)
        //            node.IsSelected = true;
        //    }
        //    if (_nodeList.Count > 0)
        //    {
        //        if (_selectedCount == _nodeList.Count - 1)
        //            _nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = true;
        //        else
        //            _nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = false;
        //    }
        //}

        //private void SetSelectedItems()
        //{
        //    if (SelectedItems == null)
        //        SelectedItems = new Dictionary<string, object>();

        //    SelectedItems.Clear();
        //    foreach (Node node in _nodeList)
        //    {
        //        if (node.IsSelected && node.Title != ALL)
        //        {
        //            if (this.ItemsSource.Count > 0)
        //                SelectedItems.Add(node.Title, node.Key);//this.ItemsSource[node.Title]);
        //        }
        //    }
        //}

        private void DisplayInControl()
        {
            try
            {
                if (_nodeList == null)
                    return;

                _nodeList.Clear();
                //Node allnode = new Node(ALL, ALL);
                ////allnode.IsSelected = true;
                //_nodeList.Add(allnode);
                if (this.ItemsSource != null)
                {
                    foreach (KeyValuePair<string, object> keyValue in this.ItemsSource)
                    {
                        if (keyValue.Value is ChildTag)
                        {
                            ChildTag ct = keyValue.Value as ChildTag;
                            Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                            //node.IsSelected = true;
                            _nodeList.Add(node);
                        }
                        else
                        {
                            Node node = new Node(keyValue.Value + "", keyValue.Key);
                            //node.IsSelected = true;
                            _nodeList.Add(node);
                        }
                    }
                }

                this.ViewSource.Source = _nodeList;
                this.ViewSource.IsLiveFilteringRequested = true;
                this.ViewSource.IsLiveSortingRequested = true;
                this.ViewSource.View.Filter = x => {
                    Node data = x as Node;
                    if (this.SearchTextBox != null && !string.IsNullOrEmpty(this.SearchTextBox.Text))
                    {
                        return data.Title.Contains(this.SearchTextBox.Text) || (!string.IsNullOrEmpty(data.PinYin) && data.PinYin.Contains(this.SearchTextBox.Text));
                    }
                    return true;
                };
                MultiSelectCombo.ItemsSource = this.ViewSource.View;
                this.SetText();
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private string GetSelectedStr()
        {
            StringBuilder venueText = new StringBuilder();
            foreach (Node s in this._nodeList)
            {
                bool flag = this.IsAll;
                if (s.IsSelected == true && s.Title != ALL)
                {
                    if (!flag)
                    {
                        venueText.Append(s.Key);
                        venueText.Append(',');
                    }
                }
            }
            return venueText.ToString().TrimEnd(new char[] { ',' }); ;
        }

        private void SetText()
        {
            //if (this.SelectedItems != null)
            //{
            StringBuilder displayText = new StringBuilder();
            StringBuilder venueText = new StringBuilder();

            int count = 0;
            bool flag = this.IsAll;
            foreach (Node s in this._nodeList)
            {
                if (s.IsSelected == true && s.Title != ALL)
                {
                    count++;

                    displayText.Append(s.Title);
                    displayText.Append(',');

                    if (!flag)
                    {
                        venueText.Append(s.Key);
                        venueText.Append(',');
                    }
                }
            }

            this.SelectedCount = count;
            this.SelectedValues = venueText.ToString().TrimEnd(new char[] { ',' });
            if (displayText.Length > 0)
            {
                this.SelectedTexts = string.Format("{0}", displayText.ToString().TrimEnd(new char[] { ',' }));
            }
            else
            {
                this.SelectedTexts = this.DefaultText;
            }


            //}
            //// set DefaultText if nothing else selected
            //if (string.IsNullOrEmpty(this.SelectedTexts))
            //{
            //    this.SelectedTexts = this.DefaultText;
            //    var txText = FindChild<TextBlock>(this, "txText");
            //    if (txText != null)
            //        txText.Text = this.SelectedTexts;
            //}

            //if (!string.IsNullOrEmpty(control.SelectedTexts))
            //{
            //    var txText = FindChild<TextBlock>(control, "txText");
            //    if (txText != null)
            //        txText.Text = control.SelectedTexts;
            //}

            int _selectedCount = 0;
            foreach (Node s in _nodeList)
            {
                if (s.IsSelected && s.Title != ALL)
                    _selectedCount++;
            }
            if (this.MultiSelectCombo.Template != null)
            {
                CheckBox chk = this.MultiSelectCombo.Template.FindName("allChk", MultiSelectCombo) as CheckBox;
                if (_selectedCount != 0 && _selectedCount == _nodeList.Count)
                {
                    //_nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = true;
                    if (chk != null)
                    {
                        chk.IsChecked = true;
                    }
                }
                else if (_selectedCount != 0)
                {
                    if (chk != null)
                    {
                        chk.IsChecked = null;
                    }
                }
                else
                {
                    //_nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = false;
                    if (chk != null)
                    {
                        chk.IsChecked = false;
                    }
                }
            }
        }

        #endregion


        public static T FindChild<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T && (child as T).Name.Equals(name))
                    return (T)child;
                else
                {
                    T childOfChild = FindChild<T>(child, name);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ClearSelection();
        }

        private void count_text_DropDownOpened(object sender, EventArgs e)
        {
            this.count_text.ItemsSource = _nodeList.Where(item => item.IsSelected == true);
        }

        private void right_part_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (count_text.Visibility == Visibility.Collapsed)
            {
                this.count_text.IsDropDownOpen = false;
            }
        }

        private void MultiSelectCombo_TextChanged(object sender, TextChangedEventArgs e)
        {
            //this.MultiSelectCombo.IsDropDownOpen = true;
            ResetTimer();
        }
        
        protected void ResetTimer()
        {
            this._interval.Stop();
            this._interval.Start();
        }

        private void cleanBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.SearchTextBox != null)
            {
                this.SearchTextBox.Text = string.Empty;
            }
        }

        private void MultiSelectCombo_DropDownOpened(object sender, EventArgs e)
        {
            // 回到 UI 线程操作界面
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MultiSelectCombo.Template != null && IsEditable)
                {
                    Popup p = MultiSelectCombo.Template.FindName("PART_Popup", MultiSelectCombo) as Popup;
                    if (p != null)
                    {
                        TextBox _tmpTextBox = p.FindName("SearchTextBox") as TextBox;
                        if (_tmpTextBox != null)
                        {
                            _tmpTextBox.Focus();
                            _tmpTextBox.SelectAll();
                        }
                    }
                }
            }));
        }
    }



    public class Node : INotifyPropertyChanged
    {

        private string _title;
        private string _key;
        private string _pinyin;
        private string _firstLetter;
        private bool _isSelected;

        #region ctor
        public Node(string title,string key, string pinyin = null, string firstletter = null)
        {
            Title = title;
            Key = key;
            PinYin = pinyin;
            FirstLetter = firstletter;
        }
        #endregion

        #region Properties
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                NotifyPropertyChanged("Key");
            }
        }
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        public string PinYin
        {
            get
            {
                return _pinyin;
            }
            set
            {
                if (_pinyin != value)
                {
                    _pinyin = value;
                    NotifyPropertyChanged("PinYin");
                }
            }
        }

        public string FirstLetter
        {
            get
            {
                return _firstLetter;
            }
            set
            {
                if (_firstLetter != value)
                {
                    _firstLetter = value;
                    NotifyPropertyChanged("FirstLetter");
                }
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
