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

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// MultiSelectComboBox.xaml 的交互逻辑
    /// </summary>
    /// <summary>
    /// MultiSelectComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class MultiSelectComboBox : UserControl
    {

        private ObservableCollection<Node> _nodeList;
        public MultiSelectComboBox()
        {
            InitializeComponent();
            _nodeList = new ObservableCollection<Node>();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
             DependencyProperty.Register("ItemsSource", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
        new PropertyChangedCallback(MultiSelectComboBox.OnItemsSourceChanged)));

        public static readonly DependencyProperty SelectedItemsProperty =
         DependencyProperty.Register("SelectedItems", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
        new PropertyChangedCallback(MultiSelectComboBox.OnSelectedItemsChanged)));

        public static readonly DependencyProperty SelectedTextsProperty =
           DependencyProperty.Register("SelectedTexts", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty SelectedValuesProperty =
          DependencyProperty.Register("SelectedValues", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
        DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(string.Empty,
        new PropertyChangedCallback(MultiSelectComboBox.OnDefaultChanged)));


        public Dictionary<string, object> ItemsSource
        {
            get { return (Dictionary<string, object>)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>
        /// 不用
        /// </summary>
        public Dictionary<string, object> SelectedItems
        {
            get { return (Dictionary<string, object>)GetValue(SelectedItemsProperty); }
            set
            {
                SetValue(SelectedItemsProperty, value);
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
            set { SetValue(SelectedValuesProperty, value); }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }
        #endregion

        #region Events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.SetText();
        }

        private static void OnDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            //control.DisplayInControl();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedBox = (CheckBox)sender;

            if (clickedBox.Content.ToString() == "All")
            {
                if (clickedBox.IsChecked.Value)
                {
                    foreach (Node node in _nodeList)
                    {
                        node.IsSelected = true;
                    }
                }
                else
                {
                    foreach (Node node in _nodeList)
                    {
                        node.IsSelected = false;
                    }
                }
            }
            else
            {
                int _selectedCount = 0;
                foreach (Node s in _nodeList)
                {
                    if (s.IsSelected && s.Title != "All")
                        _selectedCount++;
                }
                if (_selectedCount == _nodeList.Count - 1)
                    _nodeList.FirstOrDefault(i => i.Title == "All").IsSelected = true;
                else
                    _nodeList.FirstOrDefault(i => i.Title == "All").IsSelected = false;
            }
            
            //SetSelectedItems();
            SetText();
        }
        #endregion

        #region Methods
        private void SelectNodes()
        {
            int _selectedCount = 0;
            foreach (KeyValuePair<string, object> keyValue in SelectedItems)
            {
                if (keyValue.Key != "All")
                    _selectedCount++;
                Node node = _nodeList.FirstOrDefault(i => i.Title == keyValue.Key);
                if (node != null)
                    node.IsSelected = true;
            }
            if (_nodeList.Count > 0)
            {
                if (_selectedCount == _nodeList.Count - 1)
                    _nodeList.FirstOrDefault(i => i.Title == "All").IsSelected = true;
                else
                    _nodeList.FirstOrDefault(i => i.Title == "All").IsSelected = false;
            }
        }

        private void SetSelectedItems()
        {
            if (SelectedItems == null)
                SelectedItems = new Dictionary<string, object>();

            SelectedItems.Clear();
            foreach (Node node in _nodeList)
            {
                if (node.IsSelected && node.Title != "All")
                {
                    if (this.ItemsSource.Count > 0)
                        SelectedItems.Add(node.Title, node.Key);//this.ItemsSource[node.Title]);
                }
            }
        }

        private void DisplayInControl()
        {
            _nodeList.Clear();
            if (this.ItemsSource.Count > 0)
            {
                Node allnode = new Node("All", "All");
                allnode.IsSelected = true;
                _nodeList.Add(allnode);
            }
            foreach (KeyValuePair<string, object> keyValue in this.ItemsSource)
            {
                Node node = new Node(keyValue.Value + "",keyValue.Key);
                node.IsSelected = true;
                _nodeList.Add(node);
            }

            MultiSelectCombo.ItemsSource = _nodeList;
            SetText();
        }

        private void SetText()
        {
            //if (this.SelectedItems != null)
            //{
                StringBuilder displayText = new StringBuilder();
                StringBuilder venueText = new StringBuilder();
                foreach (Node s in _nodeList)
                {
                    if (s.IsSelected == true && s.Title == "All")
                    {
                        //displayText = new StringBuilder();
                        //displayText.Append("All");
                        //break;
                    }
                    else if (s.IsSelected == true && s.Title != "All")
                    {
                        displayText.Append(s.Title);
                        displayText.Append(',');
                        venueText.Append(s.Key);
                        venueText.Append(',');
                    }
                }

                this.SelectedTexts = displayText.ToString().TrimEnd(new char[] { ',' });
                this.SelectedValues = venueText.ToString().TrimEnd(new char[] { ',' });
            //}
            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(this.SelectedTexts))
            {
                this.SelectedTexts = this.DefaultText;
                var txText = FindChild<TextBlock>(this, "txText");
                if (txText != null)
                    txText.Text = this.SelectedTexts;
            }

            if (!string.IsNullOrEmpty(this.SelectedTexts))
            {
                var txText = FindChild<TextBlock>(this, "txText");
                if (txText != null)
                    txText.Text = this.SelectedTexts;
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

    }



    public class Node : INotifyPropertyChanged
    {

        private string _title;
        private string _key;
        private bool _isSelected;
        #region ctor
        public Node(string title,string key)
        {
            Title = title;
            Key = key;
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
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
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
