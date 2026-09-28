using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel.HomePage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace STC.XBond.Plugin.View.HomePage
{
    /// <summary>
    /// BatchImportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BatchImportWindow : WindowBase
    {
        private BatchImportWindowViewModel _batchImportWindowViewModel = new BatchImportWindowViewModel();

        public BatchImportWindow()
        {
            InitializeComponent();

            BaseHandler.Instance.SubscribeLoadData_Issuer(OnLoadIsserData);
            BaseHandler.Instance.SubscribeLoadData_Guarantor(OnLoadGuarantorData);

            InitOrgType();

            InitFilterUI();

            this.DataContext = _batchImportWindowViewModel;
        }

        private void InitFilterUI()
        {
            ObservableCollection<OptionItem> listProduct = new ObservableCollection<OptionItem>();
            listProduct.Add(new OptionItem("债券", "0001"));
            listProduct.Add(new OptionItem("国债期货", "0010"));
            listProduct.Add(new OptionItem("IRS", "0011"));
            this.product.ItemsSource = listProduct;//product


            ObservableCollection<OptionItem> listSecurityType = new ObservableCollection<OptionItem>();
            listSecurityType.Add(new OptionItem("R", "R"));
            listSecurityType.Add(new OptionItem("C", "C"));
            this.securityType.ItemsSource = listSecurityType;

            ObservableCollection<OptionItem> ratelist = new ObservableCollection<OptionItem>();
            ratelist.Add(new OptionItem("AAA+", "00001"));
            ratelist.Add(new OptionItem("AAA", "00010"));
            ratelist.Add(new OptionItem("AA+", "00011"));
            ratelist.Add(new OptionItem("AA", "00100"));
            ratelist.Add(new OptionItem("AA-", "00101"));
            ratelist.Add(new OptionItem("A+", "00110"));
            ratelist.Add(new OptionItem("其他", "xx"));//其他评级
            this.rankingType.ItemsSource = ratelist;//RANKING_TYPE
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
                        if (this.filterGrid != null && filterGrid.Children.Count > 0)
                        {
                            foreach(UIElement uiElement in filterGrid.Children)
                            {
                                
                                if (uiElement is FilterOptions)
                                {
                                    SetFilterOptions(uiElement as FilterOptions, tagsDic);
                                }
                                else if (uiElement is MultiSelectComboBox)
                                {
                                    SetComboBoxItemsSource(uiElement as MultiSelectComboBox, tagsDic);
                                }
                            }
                        }

                        // 行业
                        if (tagsDic != null && tagsDic.ContainsKey(Constants.INDUSTRY_TYPE))
                        {
                            this.industry_type.InitData(tagsDic[Constants.INDUSTRY_TYPE].childTags);
                        }
                    }));
                }
            });
        }

        private void SetComboBoxItemsSource(MultiSelectComboBox cb, Dictionary<string, QuoteTag> tagsDic)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            string key = string.Empty;
            if (cb.Tag != null)
                key = cb.Tag.ToString();

            if (key == Constants.ROLETYPE)
            {
                dic["0001"] = "销交";
                dic["0010"] = "做市";
            }
            else if (key == Constants.SETTLTYPE)
            {
                dic["0010"] = "T+1";
                dic["0001"] = "T+0";
                dic["0011"] = "明+0";
                dic["0101"] = "远期";
            }
            else if (key == Constants.SIDE)
            {
                dic["0001"] = "Bid";
                dic["0010"] = "Ofr";
            }
            else if (key == Constants.FIRM)
            {
                dic["0001"] = "Firm";
                dic["0010"] = "NonFirm";
            }
            else if (key == Constants.RATING_CURRENT)
            {
                dic["00001"] = "AAA";
                dic["00010"] = "A-1";
                dic["00011"] = "AA+";
                dic["00100"] = "AA";
                dic["00101"] = "AA-";
                dic["00110"] = "A+";
                dic["xx"] = "其他";
            }
            else if (key == Constants.CDC_IMPLITED_RATING)
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
                    if (key == Constants.ISSUE_YEAR)
                    {
                        tagsDic[key].childTags = tagsDic[key].childTags.OrderByDescending(item => item.childTagName).ToList();
                    }
                    else if (key == Constants.RATING_CURRENT
                        || key == Constants.CDC_IMPLITED_RATING
                        || key == Constants.ISSUER_RATING_CURRENT_CBR
                        || key == Constants.CSI_IMPLIED_RATING
                        || key == Constants.RANKING_TYPE
                        || key == Constants.PERPETUAL_TYPE
                        || key == Constants.CAL_INTER_TYPE)
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

        private void SetFilterOptions(FilterOptions fo, Dictionary<string, QuoteTag> tagsDic)
        {
            if (fo.Name == "product" || fo.Name == "securityType" || fo.Name == "rankingType")
            {
                return;
            }

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

        private void OnLoadIsserData(bool flag)
        {
            SetIssuer();

        }

        private void OnLoadGuarantorData(bool flag)
        {
            SetGuarantor();
        }

        private void SetIssuer()
        {
            Task.Run(() => {
                string sql = @"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }

                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.issuer.ItemsSource = list;
                    }));
                }
            });
        }

        private void SetGuarantor()
        {
            Task.Run(() => {
                string sql = @"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllGuarantors(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }

                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.guarantor.ItemsSource = list;
                    }));
                }
            });
        }

        private void groupCbb_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            List<HomeGroupInfo> homeGroupInfoList = new List<HomeGroupInfo>();
            IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> groupList = HomePageDataHandler.RequestGroups(false);
            foreach(KeyValuePair<string, HomeGroupInfo> pair in groupList)
            {
                homeGroupInfoList.Add(pair.Value);
            }
            args.DataSource = homeGroupInfoList;
        }

        private void gridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void DataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            _batchImportWindowViewModel.ClickChkBox();
        }

        private void TagsListChanged(object sender, RoutedEventArgs e)
        {
            FilterOptions fo = sender as FilterOptions;

            GenDes(fo.TagKey, fo.TagsList);
        }

        private void GenDes(string TagKey, List<string> filterOptionsList)
        {
            StringBuilder text = new StringBuilder();
            if (filterOptionsList != null && filterOptionsList.Count > 0)
            {
                if (TagKey == Utils.Constants.RANKING_TYPE)
                {
                    text.Append("主体:");
                }
                if (TagKey == Utils.Constants.RATING_CURRENT)
                {
                    text.Append("债项:");
                }
                else if (TagKey == Utils.Constants.ISSUER_RATING_CURRENT_CBR)
                {
                    text.Append("中债资信:");
                }
                else if (TagKey == Utils.Constants.CDC_IMPLITED_RATING)
                {
                    text.Append("中债隐含:");
                }
                else if (TagKey == Utils.Constants.CSI_IMPLIED_RATING)
                {
                    text.Append("中证隐含:");
                }
            }

            foreach (string filterValue in filterOptionsList)
            {
                string temp = string.Empty;
                if (filterValue != "xx")
                {
                    temp = CommonUtils.GetTagNameByCode(filterValue, TagKey);
                }
                else
                {
                    temp = "其他";
                }
                text.Append(CommonUtils.ConvertDes(TagKey, temp));
                text.Append("/");
            }
            if (text.Length > 0)
            {
                text.Remove(text.Length - 1, 1);
            }

            //if (TagKey == Consts.TERM && text.Length == 0)
            //{
            //    if (!string.IsNullOrEmpty(StartTermDY) || !string.IsNullOrEmpty(EndTermDY))
            //    {
            //        text.Append($"剩余期限:{StartTermDY}-{EndTermDY}");
            //    }
            //}
            _batchImportWindowViewModel.SetDesSource(TagKey, text.ToString());
        }


        private void Cbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            string key = ccb.Tag?.ToString();

            //if (isCheckShowAll == true)
            //    return;

            List<string> selectedList = new List<string>();

            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    string[] orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);


                    if (orgArr != null && orgArr.Length > 0)
                    {
                        foreach (string org in orgArr)
                        {
                            selectedList.Add(org);
                        }
                    }
                    else
                    {
                        //Dictionary<string, QuoteTag> tagsDic = IntentionIRSHandler.Instance.QueryTags(true);

                        //if (tagsDic.ContainsKey(Constants.ORGTYPE))
                        {
                            //if (tagsDic[Constants.ORGTYPE] != null && tagsDic.ContainsKey(Constants.ORGTYPE) && tagsDic[Constants.ORGTYPE].childTags != null)
                            {
                                //foreach(ChildTag ct in tagsDic[Constants.ORGTYPE].childTags)
                                {
                                    selectedList.Add(string.Empty);
                                }
                            }
                        }
                    }
                }
            }
            GenDes(key, selectedList);
        }

        private void TermCtrl_OKAction(object sender, RoutedEventArgs e)
        {

        }

        private void MultiAutoCbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiAutoCompleteComboBox ccb = sender as MultiAutoCompleteComboBox;
            string key = ccb.Tag?.ToString();

            //if (isCheckShowAll == true)
            //    return;

            List<string> selectedList = new List<string>();

            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    string[] orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);


                    if (orgArr != null && orgArr.Length > 0)
                    {
                        foreach (string org in orgArr)
                        {
                            selectedList.Add(org);
                        }
                    }
                    else
                    {
                        selectedList.Add(string.Empty);
                    }
                }
            }
            GenDes(key, selectedList);
        }

        private void HierarchicalComboBox_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            HierarchicalComboBox ccb = sender as HierarchicalComboBox;
            string key = ccb.Tag?.ToString();

            //if (isCheckShowAll == true)
            //    return;

            List<string> selectedList = new List<string>();

            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    string[] orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);


                    if (orgArr != null && orgArr.Length > 0)
                    {
                        foreach (string org in orgArr)
                        {
                            selectedList.Add(org);
                        }
                    }
                    else
                    {
                        //Dictionary<string, QuoteTag> tagsDic = IntentionIRSHandler.Instance.QueryTags(true);

                        //if (tagsDic.ContainsKey(Constants.ORGTYPE))
                        {
                            //if (tagsDic[Constants.ORGTYPE] != null && tagsDic.ContainsKey(Constants.ORGTYPE) && tagsDic[Constants.ORGTYPE].childTags != null)
                            {
                                //foreach(ChildTag ct in tagsDic[Constants.ORGTYPE].childTags)
                                {
                                    selectedList.Add(string.Empty);
                                }
                            }
                        }
                    }
                }
            }
            GenDes(key, selectedList);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.contentTxt.Text = string.Empty;
        }

        private void btnRecognize_Click(object sender, RoutedEventArgs e)
        {
            _batchImportWindowViewModel.RecognizeIntentionQuote(this.contentTxt.Text);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void issuer_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where childTagName like '%{args.Pattern}%' or pinyin LIKE '%{args.Pattern}%' limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void issuer_LetterFilterChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where pinyin like '{args.Pattern.ToUpper()}%' or pinyin LIKE '{args.Pattern.ToLower()}%' limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void guarantor_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                //IEnumerable<Node> result = (this.filterDg.ItemsSource as ObservableCollection<Node>).Where(item => item.Title != null && item.Title.Contains(args.Pattern)
                //                          || (item.PinYin != null && item.PinYin.Contains(args.Pattern)));

                string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor where childTagName like '%{args.Pattern}%' or pinyin LIKE '%{args.Pattern}%' limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllGuarantors(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }

        private void guarantor_LetterFilterChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                //IEnumerable<Node> result = (this.filterDg.ItemsSource as ObservableCollection<Node>).Where(item => item.Title != null && item.Title.Contains(args.Pattern)
                //                          || (item.PinYin != null && item.PinYin.Contains(args.Pattern)));

                string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor where pinyin like '{args.Pattern.ToUpper()}%' or pinyin LIKE '{args.Pattern.ToLower()}%' limit 200";
                List<ChildTag> queryList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllGuarantors(sql);
                ObservableCollection<Node> list = new ObservableCollection<Node>();
                foreach (ChildTag ct in queryList)
                {
                    Node node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                    list.Add(node);
                }
                args.DataSource = list;
            }
            else
            {
                args.DataSource = null;
            }
        }
    }
}
