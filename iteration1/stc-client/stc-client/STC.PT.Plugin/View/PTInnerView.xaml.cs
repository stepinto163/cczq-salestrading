using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.PT.Plugin.Handler;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.Utils;
using STC.PT.Plugin.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace STC.PT.Plugin.View
{
    /// <summary>
    /// PTInnerView.xaml 的交互逻辑
    /// </summary>
    public partial class PTInnerView : UserControl
    {
        private PTInnerViewModel _viewModel = new PTInnerViewModel();

        public Dictionary<string, IList> FilterDic = new Dictionary<string, IList>();

        private Popup QuotePopup;
        public PTInnerView()
        {
            InitializeComponent();
            this.Loaded += PTInnerView_Loaded;
            DataGridRowHelper.SetShowRowIndexProperty(Post_dataGrid, true);
            _viewModel.load();
            _viewModel.Post_dataGrid = this.Post_dataGrid;
            QuotePopup = new Popup();
            QuotePopup.MouseLeave += QuotePopup_MouseLeave;
            (DataContext as PTInnerViewModel).PageControl = this.PageControl;
        }
        public bool init = false;
        private void PTInnerView_Loaded(object sender, RoutedEventArgs e)
        {
           if (!init)
            {
                //this.PTFilterPanel.Init("3");
                //init = true;
                return;
            }

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            _viewModel.ptInnerView = this;
            SetInstitution();
            SetContract();
            InitOrgType();
            //SetComboBoxItemsSource(this.approvalstatusCbb, null, "approvalstatus");
            SetComboBoxItemsSource(this.VenuesMscb, null, "StrategyStauts");
            this.DataContext = _viewModel;
        }
        private void SetComboBoxItemsSource(MultiSelectComboBox cb, Dictionary<string, QuoteTag> tagsDic, string key)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (key == "StrategyStauts")
            {
                dic["自谈"] = "自谈";
                dic["国际"] = "国际";
                dic["国利"] = "国利";
                dic["平安"] = "平安";
                dic["信唐"] = "信唐";
                dic["中诚"] = "中诚";
                dic["上田"] = "上田";
            }
            else if (key == "approvalstatus")
            {
                dic["00001"] = "新建";
                dic["00010"] = "审批中";
                dic["00011"] = "审批拒绝";
                dic["00100"] = "审批通过";
                dic["00101"] = "交易执行中";
                dic["00110"] = "部分撤销";
                dic["00111"] = "已撤销";
                dic["01000"] = "交易终止";
                dic["01001"] = "成交确认";
            }
            else if(key == Consts.ROLETYPE)
            {
                dic["0001"] = "销交";
                dic["0010"] = "做市";
            }
            else if (key == Consts.SETTLTYPE)
            {
                dic["0010"] = "T+1";
                dic["0001"] = "T+0";
                dic["0011"] = "明+0";
                dic["0101"] = "远期";
            }
            else if (key == Consts.SIDE)
            {
                dic["0001"] = "Bid";
                dic["0010"] = "Ofr";
            }
            else if (key == Consts.FIRM)
            {
                dic["0001"] = "Firm";
                dic["0010"] = "NonFirm";
            }
            else if (key == Consts.RATING_CURRENT)
            {
                dic["00001"] = "AAA";
                dic["00010"] = "A-1";
                dic["00011"] = "AA+";
                dic["00100"] = "AA";
                dic["00101"] = "AA-";
                dic["00110"] = "A+";
                dic["xx"] = "其他";
            }
            else if (key == Consts.CDC_IMPLITED_RATING)
            {
                dic["00001"] = "AAA+";
                dic["00010"] = "AAA";
                dic["00011"] = "AAA-";
                dic["00100"] = "AA+";
                dic["00101"] = "AA";
                dic["00110"] = "AA(2)";
                dic["01000"] = "AA-";
                dic["xx"] = "其他";
            }
            else
            {
                if (tagsDic != null && tagsDic.ContainsKey(key))
                {
                    if (key == Consts.ISSUE_YEAR)
                    {
                        tagsDic[key].childTags = tagsDic[key].childTags.OrderByDescending(item => item.childTagName).ToList();
                    }
                    else if (key == Consts.RATING_CURRENT
                        || key == Consts.CDC_IMPLITED_RATING
                        || key == Consts.ISSUER_RATING_CURRENT_CBR
                        || key == Consts.CSI_IMPLIED_RATING
                        || key == Consts.RANKING_TYPE
                        || key == Consts.PERPETUAL_TYPE
                        || key == Consts.CAL_INTER_TYPE)
                    {

                    }
                    else
                    {
                        tagsDic[key].childTags = tagsDic[key].childTags.OrderBy(item => item.pinyin).ToList();
                    }

                    foreach (ChildTag ct in tagsDic[key].childTags)
                    {
                        if (ct.childTagName == "无")
                        {
                            continue;
                        }
                        dic[ct.childTagValue] = ct;
                    }
                }
            }
            cb.ItemsSource = dic;
        }
        private void SetInstitution()
        {
            Task.Run(() => {

                /*List<StcTbookUsers> crmInsts = Common.Utils.CommonUtils.GetInvestors();
                crmInsts = crmInsts.Where(item => !string.IsNullOrEmpty(item.Username)&& item.Username.Length > 0).OrderBy(item => item.Username.Substring(0, 1)).ToList();

                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (StcTbookUsers inst in crmInsts)
                {
                    Node node = new Node(inst.Name, inst.Username, inst.Username, inst.Userid);
                    list.Add(node);
                }
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.investorsCbb.ItemsSource = list;
                    }));
                }*/
            });
        }
        private void SetContract()
        {
            Task.Run(() => {
                /*List<StcTbookUsers> crmContacts = Common.Utils.CommonUtils.GetTraders();
                crmContacts = crmContacts.Where(item => !string.IsNullOrEmpty(item.Username) && item.Username.Length > 0).OrderBy(item => item.Username.Substring(0, 1)).ToList();
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (StcTbookUsers contact in crmContacts)
                {
                    Node node = new Node(contact.Name, contact.Username, contact.Username, contact.Userid);
                    list.Add(node);
                }
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.tradersCbb.ItemsSource = list;
                    }));
                }*/
            });
        }
        private void lbSettlType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init)
            {
                if (sender != null && sender is ListBox)
                {
                    ListBox listBox = sender as ListBox;
                    if (e.AddedItems.Count > 0)
                    {
                        foreach (object obj in e.AddedItems)
                        {
                            ListBoxItem item = obj as ListBoxItem;
                            if (item != null)
                            {
                                if (Constants.All.Equals(item.Content))
                                {
                                    foreach (ListBoxItem lbi in listBox.Items)
                                    {
                                        if (Constants.T1.Equals(item.Content)
                                             || Constants.T2.Equals(item.Content)
                                              || Constants.T3.Equals(item.Content)
                                               || Constants.T4.Equals(item.Content)
                                             || Constants.Forward.Equals(item.Content))
                                        {
                                            ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                            if (tmp.IsSelected)
                                                tmp.IsSelected = false;
                                        }
                                    }
                                }
                                else if (Constants.T1.Equals(item.Content)
                                            || Constants.T2.Equals(item.Content)
                                             || Constants.T3.Equals(item.Content)
                                              || Constants.T4.Equals(item.Content)
                                            || Constants.Forward.Equals(item.Content))
                                {
                                    ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                    if (tmp.IsSelected)
                                        tmp.IsSelected = false;
                                }
                            }
                        }
                    }
                    if (e.RemovedItems.Count > 0)
                    {
                        foreach (var item in e.RemovedItems)
                        {
                            if (listBox.SelectedItems.Count == 0)
                                listBox.SelectedItem = item;
                        }
                    }
                    FilterDic["SettlType"] = listBox.SelectedItems;
                }
                _viewModel.FilterDic = FilterDic;
                _viewModel.RefreshQueryData();
            }
        }

        private void Post_dataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Post_dataGrid.SelectedItems.Count > 1)
            {
                _viewModel.IsSingleRow = false;
            }
            else 
            {
                _viewModel.IsSingleRow = true;
            }
        }

        private void Post_dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Orders_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //在载入行的时候在行表头添加编号
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void copyDeal_Click(object sender, RoutedEventArgs e)
        {
            this.Post_dataGrid.copyDeal();

        }

        private void copyClientDeal_Click(object sender, RoutedEventArgs e)
        {
            this.Post_dataGrid.ClientDeal();
        }

        private void copyId_Click(object sender, RoutedEventArgs e)
        {
            this.Post_dataGrid.copyId();
        }


        private void dgMenu_Best_Opened(object sender, RoutedEventArgs e)
        {

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lbAssetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init)
            {
                if (sender != null && sender is ListBox)
                {
                    ListBox listBox = sender as ListBox;
                    if (e.AddedItems.Count > 0)
                    {
                        foreach (object obj in e.AddedItems)
                        {
                            ListBoxItem item = obj as ListBoxItem;
                            if (item != null)
                            {
                                if (Constants.All.Equals(item.Content))
                                {
                                    foreach (ListBoxItem lbi in listBox.Items)
                                    {
                                        if (Constants.BOND.Equals(item.Content)
                                             || Constants.IRS.Equals(item.Content))
                                        {
                                            ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                            if (tmp.IsSelected)
                                                tmp.IsSelected = false;
                                        }
                                    }
                                }
                                else if (Constants.BOND.Equals(item.Content)
                                            || Constants.IRS.Equals(item.Content))
                                {
                                    ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                    if (tmp.IsSelected)
                                        tmp.IsSelected = false;
                                }
                            }
                        }
                    }
                    if (e.RemovedItems.Count > 0)
                    {
                        foreach (var item in e.RemovedItems)
                        {
                            if (listBox.SelectedItems.Count == 0)
                                listBox.SelectedItem = item;
                        }
                    }
                    FilterDic["AssetType"] = listBox.SelectedItems;
                    /*if (listBox.SelectedItems.Count == 1 && this.PTFilterPanel != null)
                    {
                        ListBoxItem item = (ListBoxItem)listBox.SelectedItems[0];
                        if (item.Content.ToString().Equals(Constants.IRS))
                        {
                            this.PTFilterPanel.VisibleControl(false);
                            this.PTFilterPanel.ShowAll();
                        }
                        else
                        {
                            this.PTFilterPanel.VisibleControl(true);
                            if (item.Content.ToString().Equals(Constants.BOND))
                                this.PTFilterPanel.ShowAll();
                        }
                    }*/
                }
                _viewModel.FilterDic = FilterDic;
                _viewModel.RefreshQueryData();
            }
        }

        private void approvalstatusCbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (init)
            {
                MultiSelectComboBox ccb = sender as MultiSelectComboBox;
                string key = ccb.Tag?.ToString();

                if (!this.FilterDic.ContainsKey(key))
                {
                    this.FilterDic[key] = new List<string>();
                }

                this.FilterDic[key].Clear();

                if (ccb != null)
                {
                    if (!string.IsNullOrEmpty(ccb.SelectedValues))
                    {
                        string[] orgArr = ccb.SelectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);

                        if (orgArr != null && orgArr.Length > 0)
                        {
                            foreach (string org in orgArr)
                            {
                                this.FilterDic[key].Add(org);
                            }
                        }
                        else
                        {
                            this.FilterDic[key].Add(string.Empty);
                        }
                    }
                }
                _viewModel.FilterDic = FilterDic;
                _viewModel.RefreshQueryData();
            }
        }

        private void investorstCbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (init)
            {
                MultiAutoCompleteComboBox ccb = sender as MultiAutoCompleteComboBox;
                string key = ccb.Tag?.ToString();

                if (!this.FilterDic.ContainsKey(key))
                {
                    this.FilterDic[key] = new List<string>();
                }
                this.FilterDic[key].Clear();

                if (ccb != null)
                {
                    if (!string.IsNullOrEmpty(ccb.SelectedValues))
                    {
                        string[] orgArr = ccb.SelectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);

                        if (orgArr != null && orgArr.Length > 0)
                        {
                            foreach (string org in orgArr)
                            {
                                this.FilterDic[key].Add(org);
                            }
                        }
                        else
                        {
                            this.FilterDic[key].Add(string.Empty);
                        }
                    }
                }
                _viewModel.FilterDic = FilterDic;
                /*_viewModel.clear();
                _viewModel.QueryData();*/
            }
        }

        private void tradersCbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (init)
            {
                MultiAutoCompleteComboBox ccb = sender as MultiAutoCompleteComboBox;
                string key = ccb.Tag?.ToString();

                if (!this.FilterDic.ContainsKey(key))
                {
                    this.FilterDic[key] = new List<string>();
                }
                this.FilterDic[key].Clear();

                if (ccb != null)
                {
                    if (!string.IsNullOrEmpty(ccb.SelectedValues))
                    {
                        string[] orgArr = ccb.SelectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);

                        if (orgArr != null && orgArr.Length > 0)
                        {
                            foreach (string org in orgArr)
                            {
                                this.FilterDic[key].Add(org);
                            }
                        }
                        else
                        {
                            this.FilterDic[key].Add(string.Empty);
                        }
                    }
                }
                _viewModel.FilterDic = FilterDic;
                /*_viewModel.clear();
                _viewModel.QueryData();*/
            }
        }

        private void updateMutils_Click(object sender, RoutedEventArgs e)
        {
            if (Post_dataGrid.SelectedItems.Count > 1)
            {
                bool checkStatus = true;
                List<PtExecordersData> SelectedItemsList = Post_dataGrid.SelectedItems.Cast<PtExecordersData>().ToList();
                var result = SelectedItemsList.GroupBy(item => new { item.Blocknum, item.Createtime }).Where(group => group.Count() == 2).SelectMany(group => group).ToList();
                if(result.Count() > 0) 
                {
                    checkStatus = false;
                }
                if (!checkStatus)
                {
                    _viewModel.showMessage("选择了背对背的两笔交易，请重新选择");
                    return;
                }
                var noEditList = SelectedItemsList.Where(item => "1".Equals(item.ShowEditBtn)).ToList();
                if (noEditList != null && noEditList.Count() > 0) 
                {
                    _viewModel.showMessage("选择了不可编辑的交易，请重新选择");
                    return;
                }
                BatchManualEntry batchManual = new BatchManualEntry();
                batchManual.setUpdateList(Post_dataGrid.SelectedItems);
                batchManual.isSales = _viewModel.isSales;
                batchManual.ShowDialog();
            }
        }

        private void SearchExecId_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.RefreshQueryData();
            }
        }
        private void QuotePopup_MouseLeave(object sender, MouseEventArgs e)
        {
            closePopup();
        }
        private void approvalDataGridCell_MouseLeave(object sender, MouseEventArgs e)
        {
            closePopup();
        }
        private void closePopup() 
        {
            this.QuotePopup.IsOpen = false;
            ApprovalHisPopupView quotePopupView = this.QuotePopup.Child as ApprovalHisPopupView;
            if (quotePopupView != null && quotePopupView.DataContext is ApprovalHisPopupView)
            {
                ApprovalHisPopupViewModel viewModel = quotePopupView.DataContext as ApprovalHisPopupViewModel;
                viewModel.Mv.Clear();
            }
        }

        private void approvalDataGridCell_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridCell currentDgCell = e.Source as DataGridCell;
            string columnHeader = currentDgCell.Column.Header.ToString();
            if (columnHeader == "审批状态" || columnHeader == "审批单号")
            {
                PtExecordersData currentItem = currentDgCell.DataContext as PtExecordersData;
                if (string.IsNullOrEmpty(currentItem.Approvalorderid))
                {
                    this.QuotePopup.IsOpen = false;
                    return;
                }
                this.QuotePopup.PlacementTarget = currentDgCell;
                this.QuotePopup.Placement = PlacementMode.Bottom;
                this.QuotePopup.HorizontalOffset = 0;
                string execID = currentItem.ExecID;
                if (this.QuotePopup.IsOpen)
                {
                    ApprovalHisPopupView quotePopupView = this.QuotePopup.Child as ApprovalHisPopupView;
                    if (quotePopupView != null && quotePopupView.DataContext is ApprovalHisPopupView)
                    {
                        ApprovalHisPopupViewModel viewModel = quotePopupView.DataContext as ApprovalHisPopupViewModel;
                        if (!viewModel.Approvalorderid.Equals(execID))
                        {

                        }
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                ApprovalHisPopupView quotePopup = new ApprovalHisPopupView();
                                quotePopup.initData(execID);
                                this.QuotePopup.Child = quotePopup;
                                this.QuotePopup.IsOpen = true;
                            }));
                        }
                    }
                }
                else 
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ApprovalHisPopupView quotePopup = new ApprovalHisPopupView();
                        quotePopup.initData(execID);
                        this.QuotePopup.Child = quotePopup;
                        this.QuotePopup.IsOpen = true;
                    }));
                }
            }
            else 
            {
                this.QuotePopup.IsOpen = false;
            }
        }

        private void VenuesMscb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (init)
            {
                string[] selectedValues = this.VenuesMscb.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                List<string> selectedList = new List<string>(selectedValues);
                FilterDic["Venues"] = selectedList;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _viewModel.FilterDic = FilterDic;
                    _viewModel.refashFilterVenue("Venues");
                    if (this._viewModel.Mv != null && this._viewModel.Mv.View != null)
                        _viewModel.Mv.View?.Refresh();
                }));
            }
        }

        private void singleToggleBtn_Best_Click(object sender, RoutedEventArgs e)
        {
            if (this.singleToggleBt0n.Style == FindResource("CollapsedButton_24"))
            {
                //this.PTFilterPanel.Visibility = Visibility.Collapsed;
                this.singleToggleBt0n.Style = (Style)FindResource("ExpandButton_24");
            }
            else if (this.singleToggleBt0n.Style == FindResource("ExpandButton_24"))
            {
                //this.PTFilterPanel.Visibility = Visibility.Visible;
                this.singleToggleBt0n.Style = (Style)FindResource("CollapsedButton_24");
            }
        }

        private void investorsCbb_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                List<StcTbookUsers> crmInsts = Common.Utils.CommonUtils.GetInvestors();
                var findList = crmInsts.Where(item => (!string.IsNullOrEmpty(item.Name) && item.Name.Length > 0 && item.Name.ToLower().Contains(args.Pattern.ToLower()))
                || (!string.IsNullOrEmpty(item.Username) && item.Username.Length > 0 && item.Username.ToLower().Contains(args.Pattern.ToLower())));

                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (StcTbookUsers inst in findList)
                {
                    Node node = new Node(inst.Name, inst.Username, inst.Username, inst.Userid);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void investorsCbb_LetterFilterChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                List<StcTbookUsers> investorsList = Common.Utils.CommonUtils.GetInvestors();
                var findList = investorsList.Where(item => !string.IsNullOrEmpty(item.Username) && item.Username.Length > 0 && item.Username.ToLower().StartsWith(args.Pattern.ToLower()));

                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (StcTbookUsers inst in findList)
                {
                    Node node = new Node(inst.Name, inst.Username, inst.Username, inst.Userid);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void tradersCbb_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                List<StcTbookUsers> crmContacts = crmContacts = Common.Utils.CommonUtils.GetTraders();
                var findList = crmContacts.Where(item => (!string.IsNullOrEmpty(item.Name) && item.Name.Length > 0 && item.Name.ToLower().Contains(args.Pattern.ToLower()))
                || (!string.IsNullOrEmpty(item.Username) && item.Username.Length > 0 && item.Username.ToLower().Contains(args.Pattern.ToLower())));

                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (StcTbookUsers inst in findList)
                {
                    Node node = new Node(inst.Name, inst.Username, inst.Username, inst.Userid);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void tradersCbb_LetterFilterChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                List<StcTbookUsers> crmContacts = Common.Utils.CommonUtils.GetTraders();
                var findList = crmContacts.Where(item => !string.IsNullOrEmpty(item.Username) && item.Username.Length > 0 && item.Username.ToLower().StartsWith(args.Pattern.ToLower()));

                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (StcTbookUsers inst in findList)
                {
                    Node node = new Node(inst.Name, inst.Username, inst.Username, inst.Userid);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void PageControl_PageChanged(object sender, PageChangedEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    (this.DataContext as PTInnerViewModel).LoadMaskVisible = Visibility.Visible;
                    (this.DataContext as PTInnerViewModel).PageIndex = e.Page - 1;
                    (this.DataContext as PTInnerViewModel).RefreshQueryData();
                }));
            }
        }
        private void InitOrgType()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                Dictionary<string, QuoteTag> tagsDic = BaseHandler.Instance.GetQuoteTagDic();

                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        // 交易所
                        SetFilterOptions(this.listmarketfilter, tagsDic);
                        ObservableCollection<OptionItem> list11 = new ObservableCollection<OptionItem>();
                        list11.Add(new OptionItem("公募", "0001"));
                        list11.Add(new OptionItem("小公募", "0010"));
                        list11.Add(new OptionItem("私募", "0011"));
                        //list11.Add(new OptionItem("可交换", "0100"));
                        this.issuetypefilter.ItemsSource = list11;
                        //ObservableCollection<OptionItem> tradedatelist = new ObservableCollection<OptionItem>();
                        //tradedatelist.Add(new OptionItem("今谈", "0001"));
                        //tradedatelist.Add(new OptionItem("今发", "0010"));
                        //tradedatelist.Add(new OptionItem("明发", "0011"));
                        //tradedatelist.Add(new OptionItem("远期", "0100"));
                        //this.tradedatefliter.ItemsSource = tradedatelist;
                        ObservableCollection<OptionItem> delflaglist = new ObservableCollection<OptionItem>();
                        delflaglist.Add(new OptionItem("有效", "0"));
                        delflaglist.Add(new OptionItem("已删除", "1"));
                        this.delflagfliter.ItemsSource = delflaglist;
                        ObservableCollection<OptionItem> matchstauslist = new ObservableCollection<OptionItem>();
                        matchstauslist.Add(new OptionItem("全部匹配", "7"));
                        matchstauslist.Add(new OptionItem("部分匹配", "6"));
                        matchstauslist.Add(new OptionItem("未匹配", "5"));
                        matchstauslist.Add(new OptionItem("超量匹配", "8"));
                        this.matchstausfliter.ItemsSource = matchstauslist;
                    }));
                }
            });
        }
        private void SetFilterOptions(FilterOptions fo, Dictionary<string, QuoteTag> tagsDic)
        {
            if (fo == null || tagsDic == null)
                return;
            ObservableCollection<OptionItem> exchangelist = new ObservableCollection<OptionItem>();

            string key = fo.TagKey;
            if (tagsDic != null && tagsDic.ContainsKey(key))
            {
                foreach (ChildTag ct in tagsDic[key].childTags)
                {
                    if (ct.childTagName == "无")
                    {
                        continue;
                    }

                    exchangelist.Add(new OptionItem(ct.childTagName, ct.childTagValue));
                }
            }

            fo.ItemsSource = exchangelist;
        }
        Dictionary<string, List<string>> focusTagsDic = new Dictionary<string, List<string>>();
        private void TagsListChanged(object sender, RoutedEventArgs e)
        {
            FilterOptions fo = sender as FilterOptions;
            focusTagsDic[fo.TagKey] = fo.TagsList;
            _viewModel.FocusTags = JsonConvert.SerializeObject(focusTagsDic);
        }

        private void r_GridSplitter_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Total_dataGrid.Visibility == Visibility.Collapsed)
            {
                e.Handled = true;
            }
        }
        double reference_part_height;
        private void referenceToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.referenceToggleBtn.Style == FindResource("CollapsedButton_24"))
            {
                reference_part_height = Total_dataGrid.ActualHeight;
                r_part.Height = new GridLength(0, GridUnitType.Star);
                this.Total_dataGrid.Visibility = Visibility.Collapsed;
                this.referenceToggleBtn.Style = (Style)FindResource("ExpandButton_24");
            }
            else if (this.referenceToggleBtn.Style == FindResource("ExpandButton_24"))
            {
                if (p_part.Height.Value != 0)
                {
                    double a = p_part.ActualHeight - reference_part_height / 2 < 0 ? p_part.ActualHeight : p_part.ActualHeight - reference_part_height / 2;
                    p_part.Height = new GridLength(a, GridUnitType.Star);
                }

                r_part.Height = new GridLength(reference_part_height, GridUnitType.Star);
                this.Total_dataGrid.Visibility = Visibility.Visible;
                this.referenceToggleBtn.Style = (Style)FindResource("CollapsedButton_24");
            }
        }

        private void traderscb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (init)
            {
                string[] selectedValues = this.traderscb.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                List<string> selectedList = new List<string>(selectedValues);
                FilterDic["traders"] = selectedList;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _viewModel.FilterDic = FilterDic;
                    _viewModel.refashFilterVenue("traders");
                    //if (this._viewModel.Mv != null && this._viewModel.Mv.View != null)
                    //    _viewModel.Mv.View?.Refresh();
                }));
            }
        }

        private void investorstcb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (init)
            {
                string[] selectedValues = this.investorstcb.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                List<string> selectedList = new List<string>(selectedValues);
                FilterDic["investors"] = selectedList;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _viewModel.FilterDic = FilterDic;
                    _viewModel.refashFilterVenue("investors");
                    if (this._viewModel.Mv != null && this._viewModel.Mv.View != null)
                        _viewModel.Mv.View?.Refresh();
                }));
            }
        }
        int MaxSpanDays = 30;
        private void createDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (createDateStartPicker.SelectedDate.HasValue && createDateEndPicker.SelectedDate.HasValue)
            //{
            //    var start = createDateStartPicker.SelectedDate.Value.Date;
            //    var end = createDateEndPicker.SelectedDate.Value.Date;
            //    var days = (end - start).Days;
            //    if (Math.Abs(days) > MaxSpanDays)
            //    {
            //        if (days > 0)
            //        {
            //            createDateEndPicker.SelectedDate = start.AddDays(MaxSpanDays);
            //        }
            //        else
            //        {
            //            createDateStartPicker.SelectedDate = end.AddDays(-MaxSpanDays);
            //        }
            //    }
            //    // 强制结束日期不早于开始日期（如果这是业务规则的一部分）
            //    if (createDateEndPicker.SelectedDate.HasValue && createDateStartPicker.SelectedDate.HasValue)
            //    {
            //        if (createDateEndPicker.SelectedDate.Value.Date < createDateStartPicker.SelectedDate.Value.Date)
            //        {
            //            createDateEndPicker.SelectedDate = createDateStartPicker.SelectedDate;
            //        }
            //    }
            //}
            if (init)
            {
                if (createDateStartPicker.SelectedDate.HasValue && createDateEndPicker.SelectedDate.HasValue)
                {
                    List<DateTime> selectedList = new List<DateTime>();
                    selectedList.Add(createDateStartPicker.SelectedDate.Value.Date);
                    FilterDic["createDateStart"] = selectedList;

                    List<DateTime> selectedList2 = new List<DateTime>();
                    selectedList2.Add(createDateEndPicker.SelectedDate.Value.Date);
                    FilterDic["createDateEnd"] = selectedList2;
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        _viewModel.FilterDic = FilterDic;
                        _viewModel.refashFilterVenue("createDateStart");
                        _viewModel.refashFilterVenue("createDateEnd");
                    }));
                }
            }
        }

        private void tradeDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (tradeDateStartPicker.SelectedDate.HasValue && tradeDateEndPicker.SelectedDate.HasValue)
            //{
            //    var start = tradeDateStartPicker.SelectedDate.Value.Date;
            //    var end = tradeDateEndPicker.SelectedDate.Value.Date;
            //    var days = (end - start).Days;
            //    if (Math.Abs(days) > MaxSpanDays)
            //    {
            //        if (days > 0)
            //        {
            //            tradeDateEndPicker.SelectedDate = start.AddDays(MaxSpanDays);
            //        }
            //        else
            //        {
            //            tradeDateStartPicker.SelectedDate = end.AddDays(-MaxSpanDays);
            //        }
            //    }
            //    // 强制结束日期不早于开始日期（如果这是业务规则的一部分）
            //    if (tradeDateEndPicker.SelectedDate.HasValue && tradeDateStartPicker.SelectedDate.HasValue)
            //    {
            //        if (tradeDateEndPicker.SelectedDate.Value.Date < tradeDateStartPicker.SelectedDate.Value.Date)
            //        {
            //            tradeDateEndPicker.SelectedDate = tradeDateStartPicker.SelectedDate;
            //        }
            //    }
            //}
        }

        public void selectItemByExecID(string execid) 
        {
            PTInnerViewModel vm = this.DataContext as PTInnerViewModel;
            this.delflagfliter.IsAll = true;
            vm.selecteItemByExecID(execid);
        }

        private void CopyMenu_SecurityID_Click(object sender, RoutedEventArgs e)
        {
            this.Post_dataGrid.CopyData_SecurityID();
        }

        private void CopyMenu_Symbol_Click(object sender, RoutedEventArgs e)
        {
            this.Post_dataGrid.CopyData_Symbol();
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.Info($"PT Querying with: SHOW ALL Click!");
            (this.DataContext as PTInnerViewModel).ResetFilterPanel();
        }

        /// <summary>
        /// 清空筛选项
        /// </summary>
        public void ClearFilter()
        {
            if (investorstcb != null)
            {
                investorstcb.ClearSelection();
            }
            if (traderscb != null)
            {
                traderscb.ClearSelection();
            }
            if (VenuesMscb != null)
            {
                VenuesMscb.ClearSelection();
            }
            if (bookmcb != null)
            {
                bookmcb.ClearSelection();
            }
           
            this.matchstausfliter.IsAll = true;
            this.listmarketfilter.IsAll = true;
            this.issuetypefilter.IsAll = true;
            
            this.delflagfliter.IsAll = true;
            ObservableCollection<OptionItem> list = this.delflagfliter.ItemsSource;
            foreach (OptionItem item in list)
            {
                if ("0".Equals(item.Key))
                {
                    item.IsSelected = true;
                }
            }
        }

       
        
    }
}
