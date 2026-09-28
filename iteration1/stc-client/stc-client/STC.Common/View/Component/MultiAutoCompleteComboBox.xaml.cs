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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static STC.Common.View.Component.AutoCompleteComboBox;

namespace STC.Common.View.Component
{
    /// <summary>
    /// MultiAutoCompleteComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class MultiAutoCompleteComboBox : UserControl
    {


        public bool HasRecognizeBtn
        {
            get { return (bool)GetValue(HasRecognizeBtnProperty); }
            set { SetValue(HasRecognizeBtnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasRecognizeBtn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasRecognizeBtnProperty =
            DependencyProperty.Register("HasRecognizeBtn", typeof(bool), typeof(MultiAutoCompleteComboBox), new PropertyMetadata(false, OnHasRecognizeChanged));

        private static void OnHasRecognizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool flag = (bool)e.NewValue;
            if (d != null && d is MultiAutoCompleteComboBox)
            {
                MultiAutoCompleteComboBox control = d as MultiAutoCompleteComboBox;
                if (flag == true)
                {
                    control.recogniceBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    control.recogniceBtn.Visibility = Visibility.Collapsed;
                }
            }
        }

        //private ObservableCollection<Node> _nodeList = new ObservableCollection<Node>();

        private ObservableCollection<Node> _selectedList = new ObservableCollection<Node>();

        public ObservableCollection<Node> SelectedList
        {
            get { return _selectedList; }
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(MultiAutoCompleteComboBox), new PropertyMetadata(null, OnSelectedChanged));

        private static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiAutoCompleteComboBox control = d as MultiAutoCompleteComboBox;
            if (e.NewValue != null)
            {
                if (e.NewValue is Node)
                {
                    Node node = e.NewValue as Node;
                    node.IsSelected = true;
                    if (!control.ItemsSource.Any(item => item.Key == node.Key))
                    {
                        control.ItemsSource.Add(node);
                    }
                    else
                    {
                        control.ItemsSource.FirstOrDefault(item => item.Key == node.Key).IsSelected = true;
                    }

                    if (!control.SelectedList.Any(item => item.Key == node.Key))
                    {
                        control.SelectedList.Add(control.ItemsSource.FirstOrDefault(item => item.Key == node.Key));
                        control.SetText();
                    }
                }
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<Node>), typeof(MultiAutoCompleteComboBox), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        public ObservableCollection<Node> ItemsSource
        {
            get { return (ObservableCollection<Node>)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiAutoCompleteComboBox control = (MultiAutoCompleteComboBox)d;
            if (control != null)
            {
                control.DisplayInControl();
                SetSelectedValues(control, control.SelectedValues);
            }
        }

        internal object _dataLock = new object();

        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private void DisplayInControl()
        {
            try
            {
                //if (_nodeList == null)
                //    _nodeList = new ObservableCollection<Node>();

                //_nodeList.Clear();

                ////Node allnode = new Node(ALL, ALL);
                //////allnode.IsSelected = true;
                ////_nodeList.Add(allnode);
                //if (this.ItemsSource != null)
                //{
                //    var sortedDic = this.ItemsSource.OrderBy(item => (item.Value as ChildTag).pinyin.Substring(0, 1)).ThenBy(item => (item.Value as ChildTag).childTagName);

                //    foreach (KeyValuePair<string, object> keyValue in sortedDic)
                //    {
                //        if (keyValue.Value is ChildTag)
                //        {
                //            ChildTag ct = keyValue.Value as ChildTag;
                //            Node node = new Node(ct.childTagName, ct.childTag, ct.pinyin);
                //            //node.IsSelected = true;
                //            _nodeList.Add(node);
                //        }
                //        else
                //        {
                //            Node node = new Node(keyValue.Value + "", keyValue.Key);
                //            //node.IsSelected = true;
                //            _nodeList.Add(node);
                //        }

                //    }
                //}

                //filterDg.ItemsSource = _nodeList;
                ////filterDg.ItemsSource = _nodeList.OrderBy(item => item.Key);
                this.ViewSource.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Ascending));
                this.ViewSource.IsLiveFilteringRequested = true;
                this.ViewSource.IsLiveSortingRequested = true;
                this.ViewSource.Source = this.ItemsSource;
                BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _dataLock);
                filterDg.ItemsSource = this.ViewSource.View;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiAutoCompleteComboBox));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Watermark.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(MultiAutoCompleteComboBox), new PropertyMetadata("搜索..."));


        public string EditiableText
        {
            get { return (string)GetValue(EditiableTextProperty); }
            set { SetValue(EditiableTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditiableText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditiableTextProperty =
            DependencyProperty.Register("EditiableText", typeof(string), typeof(MultiAutoCompleteComboBox), new PropertyMetadata(string.Empty));

        public string DisplayValuePath
        {
            get { return (string)GetValue(DisplayValuePathProperty); }
            set { SetValue(DisplayValuePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayValuePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayValuePathProperty =
            DependencyProperty.Register("DisplayValuePath", typeof(string), typeof(MultiAutoCompleteComboBox), new PropertyMetadata(null));

        private ObservableCollection<DataGridColumn> _autoCompColumns = new ObservableCollection<DataGridColumn>();
        public ObservableCollection<DataGridColumn> AutoCompColumns
        {
            get
            {
                return _autoCompColumns;
            }
        }

        public int SelectedCount
        {
            get { return (int)GetValue(SelectedCountProperty); }
            set { SetValue(SelectedCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCountProperty =
            DependencyProperty.Register("SelectedCount", typeof(int), typeof(MultiAutoCompleteComboBox), new PropertyMetadata(0, OnSelectedCountChanged));

        private static void OnSelectedCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            MultiAutoCompleteComboBox control = (MultiAutoCompleteComboBox)d;
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

        /// <summary>
        /// 选择的值列表 ,分割
        /// </summary>
        public string SelectedValues
        {
            get { return (string)GetValue(SelectedValuesProperty); }
            set
            {
                SetValue(SelectedValuesProperty, value);
                RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            }
        }

        public static readonly DependencyProperty SelectedValuesProperty =
         DependencyProperty.Register("SelectedValues", typeof(string), typeof(MultiAutoCompleteComboBox),
             new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedValuesChanged)));


        private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }
            MultiAutoCompleteComboBox control = (MultiAutoCompleteComboBox)d;
            SetSelectedValues(control, e.NewValue.ToString());
        }

        private static void SetSelectedValues(MultiAutoCompleteComboBox control, string selectedValues)
        {
            StringBuilder valuesb = new StringBuilder();

            if (!control.isEq(selectedValues))
            {
                foreach (Node node in control.SelectedList)
                {
                    node.IsSelected = false;
                }
                control.SelectedList.Clear();

                string[] valuesArr = selectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (valuesArr != null && valuesArr.Length > 0 && control.ItemsSource != null && control.ItemsSource.Count > 0)
                {
                    foreach (Node node in control.ItemsSource)
                    {
                        node.IsSelected = valuesArr.Any(item => item == node.Key);
                        if (node.IsSelected)
                        {
                            control.SelectedList.Add(node);
                        }
                    }
                }
                control.SetText();
            }
        }

        private bool isEq(string selectedValues)
        {
            string[] valuesArr = selectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            if (valuesArr.Length == this.SelectedList.Count)
            {
                return this.SelectedList.All(item => valuesArr.Contains(item.Key));
            }
            else
            {
                return false;
            }
        }

        public MultiAutoCompleteComboBox()
        {
            InitializeComponent();
            foreach(DataGridColumn column in this.AutoCompColumns)
            {
                this.CCB_edit.AutoCompColumns.Add(column);
            }
        }


        public event AutoCompleteHandler PatternChanged;

        private void CCB_edit_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (PatternChanged != null)
            {
                PatternChanged(sender, args);
            }
            //if (!string.IsNullOrEmpty(args.Pattern))
            //{
            //    IEnumerable<Node> result = (this.filterDg.ItemsSource as ObservableCollection<Node>).Where(item => item.Title != null && item.Title.Contains(args.Pattern)
            //                              || (item.PinYin != null && item.PinYin.Contains(args.Pattern)));

            //    args.DataSource = result;
            //}
            //else
            //{
            //    args.DataSource = null;
            //}
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = true;
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ClearSelection();
        }

        private void SetText()
        {
            this.SelectedCount = this.SelectedList.Count;

            StringBuilder venueText = new StringBuilder();
            foreach (Node s in this.SelectedList)
            {
                venueText.Append(s.Key);
                venueText.Append(',');
            }

            this.SelectedValues = venueText.ToString().TrimEnd(new char[] { ',' });
        }


        public void ClearSelection()
        {
            if (SelectedList.Count > 0)
            {
                foreach(Node node in this.SelectedList)
                {
                    node.IsSelected = false;
                }

                this.SelectedList.Clear();
                this.SetText();
            }
            this.CCB_edit.Text = string.Empty;
        }

        /// <summary>
        /// Event for when pattern has been changed
        /// </summary>
        public event AutoCompleteHandler LetterFilterChanged;

        private void FilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                try
                {
                    ListBoxItem lbi = e.AddedItems[0] as ListBoxItem;
                    string selectedLetter = lbi.Content.ToString().ToLower();
                    
                    AutoCompleteArgs args = new AutoCompleteArgs(selectedLetter);
                    if (LetterFilterChanged != null)
                    {
                        LetterFilterChanged(sender, args);
                    }

                    if (args != null)
                    {
                        if (args.DataSource != null && args.DataSource is ObservableCollection<Node>)
                        {
                            ObservableCollection<Node> ds = args.DataSource as ObservableCollection<Node>;

                            foreach(Node node in ds)
                            {
                                if (!this.ItemsSource.Any(item => item.Key == node.Key))
                                {
                                    this.ItemsSource.Add(node);
                                }
                            }

                            IList<Node> list = this.ItemsSource.Where(item => item.PinYin.ToLower().StartsWith(selectedLetter.ToLower())).ToList();
                            if (list != null && list.Count() > 0)
                            {
                                Node findNode = list.First();
                                this.filterDg.ScrollIntoView(findNode);
                                this.filterDg.SelectedItem = findNode;
                            }

                            ds.Clear();
                        }
                    }
                    
                } 
                catch(Exception ex)
                {
                    LogHandler.Error(ex);
                }
            }
        }

        private void count_text_DropDownOpened(object sender, EventArgs e)
        {
            this.count_text.ItemsSource = _selectedList;
        }

        private void right_part_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (count_text.Visibility == Visibility.Collapsed)
            {
                this.count_text.IsDropDownOpen = false;
            }
        }

        private void CCB_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CCB_edit.Text))
            {
                CCB_edit.ExpandOptions();
            }
        }

        private void deleteOneBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Node removeNode = btn.DataContext as Node;
            if (removeNode != null)
            {
                removeNode.IsSelected = false;
                this.SelectedList.Remove(removeNode);
                this.SetText();
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                Node addNode = chk.DataContext as Node;
                if (addNode != null)
                {
                    if (!this.SelectedList.Any(item => item.Key == addNode.Key))
                    {
                        addNode.IsSelected = true;
                        this.SelectedList.Add(addNode);
                        this.SetText();
                    }
                }
            }
            else
            {
                Node removeNode = chk.DataContext as Node;
                if (removeNode != null)
                {
                    removeNode.IsSelected = false;
                    this.SelectedList.Remove(removeNode);
                    this.SetText();
                }
            }
        }

        private void recogniceBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = false;

            if (!WindowManager.Instance.IsExist("IssueRecogizationWin"))
            {
                //IssueRecogizationWin issueRecogizationWin = new IssueRecogizationWin(this.ItemsSource);
                IssueRecogizationWin issueRecogizationWin = new IssueRecogizationWin();
                issueRecogizationWin.RefreshAction = RefreshAction;
                WindowManager.Instance.OpenOnlyOneWindow("IssueRecogizationWin", issueRecogizationWin, Window.GetWindow(this));
            }
        }

        private void RefreshAction(CustomCollection<IssueRecogizeItem> items)
        {
            foreach(IssueRecogizeItem item in items)
            {
                if (item.IsChecked && item.NodeInfo != null)
                {
                    item.NodeInfo.IsSelected = true;
                    if (!this.ItemsSource.Any(n => n.Key == item.NodeInfo.Key))
                    {
                        this.ItemsSource.Add(item.NodeInfo);
                    }
                    else
                    {
                        this.ItemsSource.FirstOrDefault(n => n.Key == item.NodeInfo.Key).IsSelected = true;
                    }

                    if (!this.SelectedList.Any(_ => _.Key == item.NodeInfo.Key))
                    {
                        this.SelectedList.Add(this.ItemsSource.FirstOrDefault(n => n.Key == item.NodeInfo.Key));
                    }
                }
            }
            this.SetText();
        }
    }
}
