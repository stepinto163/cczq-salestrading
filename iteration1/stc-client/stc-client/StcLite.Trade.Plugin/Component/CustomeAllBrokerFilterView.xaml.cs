using com.gateway.log;
using STC.Common.Utils;
using System;
using System.Collections;
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

namespace BDB.Trade.Plugin.Component
{
    /// <summary>
    /// CustomeAllBrokerFilterView.xaml 的交互逻辑
    /// </summary>
    public partial class CustomeAllBrokerFilterView : UserControl
    {
        private int originWith = 70;
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(CustomeAllBrokerFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }


        public static readonly DependencyProperty SelectIndexProperty = DependencyProperty.Register("SelectIndex", typeof(int),
            typeof(CustomeAllBrokerFilterView), new PropertyMetadata());

        public int SelectIndex
        {
            get { return (int)GetValue(SelectIndexProperty); }
            set { SetValue(SelectIndexProperty, value); }
        }


        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomeAllBrokerFilterView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        public CustomeAllBrokerFilterView()
        {
            InitializeComponent();

            this.Loaded += BrokerFilterView_Loaded;
        }

        private void BrokerFilterView_Loaded(object sender, RoutedEventArgs e)
        {
            //LogHelper.Info("CustomeAllBrokerFilterView load");
            this.Loaded -= BrokerFilterView_Loaded;

            try
            {
                string[] brokerSortArr = LocalEnv.UserInfo.BrokerSort.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (brokerSortArr != null)
                {
                    ComboBoxItem xtCheckBox = null;
                    int index = 1;
                    foreach (string bs in brokerSortArr)
                    {
                        LogHelper.Info(string.Format("index:{0}, broker:{1}", index, bs));
                        string cn = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");
                        if (CommonConstants.BrokerNumEnum.Contains(bs))
                        {
                            ComboBoxItem broker = null;
                            if (index == 1)
                            {
                                broker = Borker1;
                            }
                            else if (index == 2)
                            {
                                broker = Borker2;
                            }
                            else if (index == 3)
                            {
                                broker = Borker3;
                            }
                            else if (index == 4)
                            {
                                broker = Borker4;
                            }
                            else if (index == 5)
                            {
                                broker = Borker5;
                            }
                            else if (index == 6)
                            {
                                broker = Borker6;
                            }

                            if (broker != null)
                            {
                                broker.Content = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");

                                broker.Visibility = Visibility.Visible;

                                //if (bs == "5")
                                //{
                                //    xtCheckBox = broker;
                                //}
                            }
                            index++;
                        }
                    }

                    //if (xtCheckBox != null)
                    //{
                    //    xtCheckBox.LayoutTransform = new ScaleTransform(1, 1.1);
                    //}

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private bool _isChange = true;
        public void SetSelectedIndex(int index)
        {
            _isChange = false;

            if (emptyTB != null)
            {
                if (gdTitle.SelectedIndex == -1)
                    emptyTB.Visibility = Visibility.Visible;
                else
                    emptyTB.Visibility = Visibility.Collapsed;
            }
            
              this.gdTitle.SelectedIndex = index;
            //if (index < 0)
            //{
            //    Borker1.IsChecked = Borker2.IsChecked = Borker3.IsChecked = Borker4.IsChecked = Borker5.IsChecked = Borker6.IsChecked = cbAll.IsChecked = false;
            //}
            
            //switch (index)
            //{
            //    case 0:
            //        cbAll.IsChecked = true;
            //        break;
            //    case 1:
            //        Borker1.IsChecked = true;
            //        break;
            //    case 2:
            //        Borker2.IsChecked = true;
            //        break;
            //    case 3:
            //        Borker3.IsChecked = true;
            //        break;
            //    case 4:
            //        Borker4.IsChecked = true;
            //        break;
            //    case 5:
            //        Borker5.IsChecked = true;
            //        break;
            //    case 6:
            //        Borker6.IsChecked = true;
            //        break;
            //}

            if (index < 0)
            {
                FilterDic.Clear();

            }

            _isChange = true;
        }

        private void TitleBorker_Click(object sender, RoutedEventArgs e)
        {
            //if (!_isChange)
            //    return;

            //var brokerCheck = (CheckBox)sender;
            //brokerCheck.IsChecked = true;

            //List<ListBoxItem> selectedItems = new List<ListBoxItem>();

            //ListBoxItem listBoxItem = new ListBoxItem();
            //listBoxItem.Content = brokerCheck.Content;

            //selectedItems.Add(listBoxItem);

            //int index = 0;
            //foreach (var item in gdTitle.Children)
            //{
            //    var broker = (CheckBox)item;
            //    if (brokerCheck.Content.ToString() != broker.Content.ToString())
            //        broker.IsChecked = false;
            //    else
            //    {
            //        SelectIndex = index;
            //    }

            //    index++;
            //}


            //FilterDic.Clear();
            //FilterDic[CommonConstants.BROKER] = selectedItems;


            //RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        //public void ChangeSize(int totalLen)
        //{
        //    int newWidth = (totalLen - 52) / 7;

        //    if (totalLen > 550)
        //        newWidth = originWith;

        //    foreach (var item in gdTitle.Children)
        //    {
        //        var broker = (CheckBox)item;
        //        broker.MinWidth = newWidth;
        //    }

        //    if (newWidth > 52)
        //    {
        //        cbAll.Width = 52;
        //        cbAll.Padding = new Thickness(8, 4, 0, 0);
        //    }
        //    else if (newWidth < 40)
        //    {
        //        cbAll.Width = 40;
        //        cbAll.Padding = new Thickness(0, 4, 0, 0);
        //    }
        //    else
        //    {
        //        cbAll.Width = newWidth;
        //        cbAll.Padding = new Thickness(2, 4, 0, 0);
        //    }    
        //}

        private void gdTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (emptyTB != null)
            {
                if (gdTitle.SelectedIndex == -1)
                    emptyTB.Visibility = Visibility.Visible;
                else
                    emptyTB.Visibility = Visibility.Collapsed;
            }

            if (!_isChange)
                return;

            string content = string.Empty;
            if(e.AddedItems != null && e.AddedItems.Count > 0)
            {
                content = (e.AddedItems[0] as ComboBoxItem).Content.ToString();
            }

            List<ListBoxItem> selectedItems = new List<ListBoxItem>();

            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = content;

            selectedItems.Add(listBoxItem);

            //int index = 0;
            //foreach (var item in gdTitle.Children)
            //{
            //    var broker = (CheckBox)item;
            //    if (brokerCheck.Content.ToString() != broker.Content.ToString())
            //        broker.IsChecked = false;
            //    else
            //    {
            //        SelectIndex = index;
            //    }

            //    index++;
            //}


            FilterDic.Clear();
            FilterDic[CommonConstants.BROKER] = selectedItems;


            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
