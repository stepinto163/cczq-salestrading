using com.gateway.log;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
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
using BDB.Price.Plugin.Model;
using STC.Common.Utils;

namespace BDB.Trade.Plugin.Component
{
    /// <summary>
    /// CustomBrokerFilterView.xaml 的交互逻辑
    /// </summary>
    public partial class CustomBrokerFilterView : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(CustomBrokerFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }


        public static readonly DependencyProperty SelectIndexProperty = DependencyProperty.Register("SelectIndex", typeof(int),
            typeof(CustomBrokerFilterView), new PropertyMetadata());

        public int SelectIndex
        {
            get { return (int)GetValue(SelectIndexProperty); }
            set { SetValue(SelectIndexProperty, value); }
        }


        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomBrokerFilterView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public CustomBrokerFilterView()
        {
            InitializeComponent();

            this.Loaded += BrokerFilterView_Loaded;
        }

        private CardShowType cardShowType = CardShowType.InCardPage;

        public CardShowType CardShowType
        {
            get { return cardShowType; }
            set
            {
                cardShowType = value;
            }
        }

        public void SetIndexByContent(string content)
        {
            if (cbAll.Content.ToString() == content)
            {
                cbAll.IsChecked = true;

                TitleBorker_Click(cbAll, null);
            }
            else if (cbBest.Content.ToString() == content)
            {
                cbBest.IsChecked = true;

                TitleBorker_Click(cbBest, null);
            }
            else if (Borker1.Content.ToString() == content)
            {
                Borker1.IsChecked = true;

                SetBrokerShow(content);
                //TitleBorker_Click(Borker1, null);
            } 
            else if (Borker2.Content.ToString() == content)
            {
                Borker2.IsChecked = true;

                SetBrokerShow(content);
                //TitleBorker_Click(Borker2, null);
            }
            else if (Borker3.Content.ToString() == content)
            {
                Borker3.IsChecked = true;

                SetBrokerShow(content);
                //TitleBorker_Click(Borker3, null);
            }
            else if (Borker4.Content.ToString() == content)
            {
                Borker4.IsChecked = true;

                SetBrokerShow(content);
                //TitleBorker_Click(Borker4, null);
            }
            else if (Borker5.Content.ToString() == content)
            {
                Borker5.IsChecked = true;

                SetBrokerShow(content);
                //TitleBorker_Click(Borker5, null);
            }
            else if (Borker6.Content.ToString() == content)
            {
                Borker6.IsChecked = true;

                SetBrokerShow(content);
                //TitleBorker_Click(Borker6, null);
            }
            else if (cbAll.Content.ToString() == content)
            {
                cbAll.IsChecked = true;

                SetBrokerShow(content);
                //TitleBorker_Click(cbAll, null);
            }
        }

        public void SetIndex(int index)
        {
            if (index == 0)
            {
                cbAll.IsChecked = true;
                cbBest.IsChecked = false;
                Borker1.IsChecked = false;
                Borker2.IsChecked = false;
                Borker3.IsChecked = false;
                Borker4.IsChecked = false;
                Borker5.IsChecked = false;
                Borker6.IsChecked = false;
            }
        }

        private List<CheckBox> brokerCheckBoxList = new List<CheckBox>();
        public void SetBrokerShow(string brokerCN)
        {
            if (brokerCheckBoxList != null && brokerCheckBoxList.Count > 0)
            {
                foreach (CheckBox checkBox in brokerCheckBoxList)
                {
                    string content = checkBox.Content as string;
                    if (content.Equals(brokerCN))
                    {
                        checkBox.Visibility = Visibility.Visible;
                        checkBox.IsChecked = true;

                        TitleBorker_Click(checkBox, null);
                    }
                    else
                    {
                        checkBox.Visibility = Visibility.Collapsed;
                        checkBox.IsChecked = false;
                    }
                }
            }
        }

        private void BrokerFilterView_Loaded(object sender, RoutedEventArgs e)
        {
            //LogHelper.Info("----BrokerFilterView_Loaded");
            this.Loaded -= BrokerFilterView_Loaded;

            try
            {
                string[] brokerSortArr = LocalEnv.UserInfo.BrokerSort.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (brokerSortArr != null)
                {
                    CheckBox xtCheckBox = null;
                    int index = 2;
                    foreach (string bs in brokerSortArr)
                    {
                        LogHelper.Info(string.Format("index:{0}, broker:{1}", index, bs));
                        string cn = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");
                        if (CommonConstants.BrokerNumEnum.Contains(bs))
                        {
                            CheckBox broker = null;
                            if (index == 2)
                            {
                                broker = Borker1;
                            }
                            else if (index == 3)
                            {
                                broker = Borker2;
                            }
                            else if (index == 4)
                            {
                                broker = Borker3;
                            }
                            else if (index == 5)
                            {
                                broker = Borker4;
                            }
                            else if (index == 6)
                            {
                                broker = Borker5;
                            }
                            else if (index == 7)
                            {
                                broker = Borker6;
                            }

                            if (broker != null)
                            {
                                brokerCheckBoxList.Add(broker);

                                broker.Content = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");

                                broker.Visibility = Visibility.Collapsed;

                                if (bs == "5")
                                {
                                    xtCheckBox = broker;
                                }
                            }
                            index++;
                        }
                    }

                    if (xtCheckBox != null)
                    {
                        xtCheckBox.LayoutTransform = new ScaleTransform(1, 1.1);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }


        private void TitleBorker_Click(object sender, RoutedEventArgs e)
        {
            var brokerCheck = (CheckBox)sender;
            brokerCheck.IsChecked = true;

            List<ListBoxItem> selectedItems = new List<ListBoxItem>();

            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = brokerCheck.Content;


            if (brokerCheck.Content.ToString().Equals(CommonConstants.ALL))
            {
                Borker1.Visibility = Borker2.Visibility = Borker3.Visibility = Borker4.Visibility = Borker5.Visibility = Borker6.Visibility = Visibility.Collapsed;
            }


            selectedItems.Add(listBoxItem);

            int index = 0;
            foreach (var item in gdTitle.Children)
            {
                var broker = (CheckBox)item;
                if (brokerCheck.Content.ToString() != broker.Content.ToString())
                    broker.IsChecked = false;
                else
                {
                    SelectIndex = index;
                }

                index++;
            }

            if (e != null && cardShowType == CardShowType.InCardPage)
            {
                ChangeBroker(brokerCheck);
            }

            FilterDic.Clear();
            FilterDic[CommonConstants.BROKER] = selectedItems;


            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });

        }

        private void ChangeBroker(CheckBox currentSelect)
        {
            if (FilterDic != null && FilterDic.Count > 0 && FilterDic.ContainsKey(CommonConstants.BROKER))
            {
                IList list = FilterDic[CommonConstants.BROKER];
                if (list != null && list.Count > 0)
                {
                    ListBoxItem listBoxItem = list[0] as ListBoxItem;
                    string oldContent = listBoxItem.Content as string;
                    string newContent = currentSelect.Content as string;

                    if (oldContent != newContent)
                    {
                        BondChangeHandler.Instance.OnOneBrokerChanged(OperationType.CardToMain, newContent);
                    }
                }
            }
            else
            {
                string newContent = currentSelect.Content as string;
                BondChangeHandler.Instance.OnOneBrokerChanged(OperationType.CardToMain, newContent);
            }
        }
    }
}
