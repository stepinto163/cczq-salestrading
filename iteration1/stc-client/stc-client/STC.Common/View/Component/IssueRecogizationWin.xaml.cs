using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace STC.Common.View.Component
{
    /// <summary>
    /// IssueRecogizationWin.xaml 的交互逻辑
    /// </summary>
    public partial class IssueRecogizationWin : WindowBase
    {
        public Action<CustomCollection<IssueRecogizeItem>> RefreshAction;
        public Action<List<Node>> RefreshSelectedAction;

        private object _LockObject = new object();
        private CollectionViewSource viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return viewSource; }
            set { viewSource = value; }
        }

        private CustomCollection<IssueRecogizeItem> intentionQuoteInfo = new CustomCollection<IssueRecogizeItem>();

        public CustomCollection<IssueRecogizeItem> IntentionQuoteInfo
        {
            get { return intentionQuoteInfo; }
            set { intentionQuoteInfo = value; }
        }

        private bool _IsAutoPrase = false;

        /// <summary>
        /// 自动解析
        /// </summary>
        public bool IsAutoPrase
        {
            get { return _IsAutoPrase; }
            set
            {
                _IsAutoPrase = value;
            }
        }

        //private ObservableCollection<Node> _dataSource = new ObservableCollection<Node>();

        public IssueRecogizationWin(ObservableCollection<Node> ds = null)
        {
            InitializeComponent();
            //_dataSource = ds;
            IntentionQuoteInfo.CollectionChanged += IntentionQuoteInfo_CollectionChanged;
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);
            this.dg.ItemsSource = ViewSource.View;
        }

        public IssueRecogizationWin(object obj)
        {
            InitializeComponent();
            //_dataSource = ds;
            IntentionQuoteInfo.CollectionChanged += IntentionQuoteInfo_CollectionChanged;
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);
            this.dg.ItemsSource = ViewSource.View;

            //自动识别
            if (obj != null)
            {
                if (obj.GetType() == typeof(bool))
                {
                    isFuzzyChk.IsChecked= true;
                    IsAutoPrase = true;
                }
            }

            this.contentTxt.Dispatcher.Invoke((Action)(() => {
                string tmp = getClipboardText();
                if (!string.IsNullOrEmpty(tmp))
                {
                    this.contentTxt.Text = tmp;
                    this.Loaded += IssueRecogizationWin_Loaded;
                }
            }));

        }

        //剪贴板被占用（每次只能一个程序读取），则会报OpenClipboard HRESULT:0x800401D0 (CLIPBRD_E_CANT_OPEN))错误
        private string getClipboardText()
        {
            for (int i = 0; i < 200; i++)
            {
                try
                {
                    return System.Windows.Clipboard.GetText();
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);//这句加不加都没关系
                }
            }
            return string.Empty;
        }


        int dataCount = 0;
        int selectedCount = 0;
        private void IntentionQuoteInfo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                dataCount++;
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                dataCount--;
            }
            //this.CountStr.Text = $"共{dataCount},选中{selectedCount}";
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is IssueRecogizeItem)
            {
                (e.Row.Item as IssueRecogizeItem).No = e.Row.GetIndex() + 1;
            }
        }

        private void DataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            refreshDataGridRowNumbers(sender);
        }

        // 刷新序号
        private static void refreshDataGridRowNumbers(object sender)
        {
            DataGrid grid = sender as DataGrid;
            if (grid == null)
                return;

            foreach (var item in grid.Items)
            {
                var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null && row.Item is IssueRecogizeItem)
                    (row.Item as IssueRecogizeItem).No = row.GetIndex() + 1;
            }
        }

        int strRowCount = 0;

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.contentTxt.Text = string.Empty;
            strRowCount = 0;
            RowStr.Text = $"共{strRowCount}条";
        }

        char[] splitCharArr_Fuzzy = new char[] { '\n', '\r', ' ', '　', ',', '，', '、', ' ' };
        char[] splitCharArr = new char[] { '\n', '\r'};
        private async void btnRecognize_Click(object sender, RoutedEventArgs e)
        {
            bool isFuzzy = isFuzzyChk.IsChecked.Value;
            char[] splitArr = splitCharArr;
            if (isFuzzy)
            {
                splitArr = splitCharArr_Fuzzy;
            }
            string text = this.contentTxt.Text.TrimEnd(splitArr);
            text = text.TrimEnd(splitArr);
            //text = text.ToLower().Replace("bid", string.Empty);
            //text = text.ToLower().Replace("ofr", string.Empty);
            string pattern = $"\\b{Regex.Escape("bid")}\\b"; // \b 表示单词边界，防止部分匹配问题
            text = Regex.Replace(text, pattern, "", RegexOptions.IgnoreCase);
            pattern = $"\\b{Regex.Escape("ofr")}\\b";
            text = Regex.Replace(text, pattern, "", RegexOptions.IgnoreCase);
            text = text.TrimStart(new char[] { '收' });
            text = text.TrimStart(new char[] { '出' });
            //string[] splitSpace = text.Split(CommonConstants.Space_CHAR);
            string[] lsstring = text.Split(splitArr, StringSplitOptions.RemoveEmptyEntries);
            //Dictionary<string, List<IssueRecogizeItem>> iriDic = new Dictionary<string, List<IssueRecogizeItem>>();
           
            List<ChildTag> queryList = new List<ChildTag>();
            if (lsstring.Length > 3000)
            {
                CommonUtils.Prompt2Second("提示", "最大识别个数3000条,请检查");
                return;
            }
            await Task.Run(() =>
            {
                List<string> sourcelist = new List<string>();
                if (isFuzzy)
                {
                    string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where ";
                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    foreach (string findStr in lsstring)
                    {
                        StringBuilder patternSB = new StringBuilder();
                        string searchStr = findStr.Trim();
                        char[] charArr = searchStr.ToCharArray();
                        foreach (char a in charArr)
                        {
                            if (patternSB.Length > 0)
                            {
                                patternSB.Append("%");
                            }
                            patternSB.Append(a);
                        }

                        if (sb.Length > 0)
                        {
                            sb.Append(" or ");
                        }
                        sb.Append($"pinyin like '%{patternSB}%' or firstletter like '%{searchStr}%' or childTagName like '%{patternSB}%'");

                        i++;
                        if (i % 200 == 0)
                        {
                            if (sb.Length > 0)
                            {
                                queryList.AddRange(Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql + sb.ToString()));
                                sb.Clear();
                            }
                        }
                    }
                    if (sb.Length > 0)
                    {
                        queryList.AddRange(Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql + sb.ToString()));
                        sb.Clear();
                    }
                }
                else
                {
                    string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where ";
                    StringBuilder sb = new StringBuilder();
                   
                    int i = 0;
                    foreach (string findStr in lsstring)
                    {
                        string searchStr = findStr.Trim();
                        if (sb.Length > 0)
                        {
                            sb.Append(" or ");
                        }
                        sb.Append($"pinyin = '{searchStr}' or firstletter = '{searchStr}' or childTagName = '{searchStr}'");
                        sourcelist.Add(searchStr);
                        i++;
                        if (i % 200 == 0)
                        {
                            if (sb.Length > 0)
                            {
                                List<ChildTag> dbList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql + sb.ToString());
                                queryList.AddRange(dbList);
                                sb.Clear();
                                foreach(ChildTag ct in dbList)
                                {
                                    sourcelist.Remove(ct.childTagName);
                                }
                            }
                        }
                    }
                    if (sb.Length > 0)
                    {
                        List<ChildTag> dbList = Common.Utils.SqlLite.TagsSqliteManager.Instance.GetAllIssuers(sql + sb.ToString());
                        queryList.AddRange(dbList);
                        sb.Clear();
                        foreach (ChildTag ct in dbList)
                        {
                            sourcelist.Remove(ct.childTagName);
                        }
                    }
                   
                }

                StringBuilder failSb = new StringBuilder();
                if (sourcelist.Count > 0)
                {
                    foreach (string failStr in sourcelist)
                    {
                        //if (sb.Length > 0)
                        //{
                        //    sb.Append(Consts.COMMA_CHAR);
                        //}
                        failSb.AppendLine(failStr);
                    }
                    //CustomMessageBox.Info(sb.ToString(), "发行人识别");
                    //LogHandler.Info($"发行人识别（精确）失败：{sb}");
                }

                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    contentTxt1.Text = failSb.ToString();

                    RowStr.Text = $"共{lsstring.Length}条";
                    foreach (ChildTag ct in queryList)
                    {
                        Node n = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                        IntentionQuoteInfo.Add(new IssueRecogizeItem { NodeInfo = n, IsChecked = IsAutoPrase ? false : true });
                    }

                    if (!IsAutoPrase)
                        SetOppositeCheckAll();
                }));

                

                //foreach (Node n in _dataSource)
                //{
                //    foreach (string findStr in lsstring)
                //    {
                //        if (isFuzzy)
                //        {
                //            StringBuilder patternSB = new StringBuilder();
                //            char[] charArr = findStr.ToCharArray();
                //            foreach (char a in charArr)
                //            {
                //                if (patternSB.Length > 0)
                //                {
                //                    patternSB.Append(".*");
                //                }
                //                patternSB.Append(a);
                //            }

                //            if (n.FirstLetter.Contains(findStr)
                //            || Regex.IsMatch(n.Title, patternSB.ToString())
                //            || Regex.IsMatch(n.PinYin, patternSB.ToString()))
                //            {
                //                if (!IntentionQuoteInfo.Any(_ => _.NodeInfo.Key == n.Key))
                //                {
                //                    if (!iriDic.ContainsKey(findStr))
                //                    {
                //                        iriDic[findStr] = new List<IssueRecogizeItem>();
                //                    }
                //                    iriDic[findStr].Add(new IssueRecogizeItem { NodeInfo = n });
                //                }
                //            }
                //        }
                //        else
                //        {
                //            if (n.FirstLetter.Contains(findStr)
                //                || n.Title.Contains(findStr)
                //                || n.PinYin.Contains(findStr))
                //            {
                //                if (!IntentionQuoteInfo.Any(_ => _.NodeInfo.Key == n.Key))
                //                {
                //                    if (!iriDic.ContainsKey(findStr))
                //                    {
                //                        iriDic[findStr] = new List<IssueRecogizeItem>();
                //                    }
                //                    iriDic[findStr].Add(new IssueRecogizeItem { NodeInfo = n });
                //                }
                //            }
                //        }
                //    }
                //}

                //this.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    foreach(string findStr in lsstring)
                //    {
                //        if (iriDic.ContainsKey(findStr))
                //        {
                //            foreach (IssueRecogizeItem newItem in iriDic[findStr])
                //            {
                //                IntentionQuoteInfo.Add(newItem);
                //                newItem.IsChecked = IsAutoPrase ? false : true;
                //            }
                //        }
                //    }
                //    if (!IsAutoPrase)
                //        SetOppositeCheckAll();
                //}));
            });
            contentTxt.Focus();
        }

        private void btnClear_right_Click(object sender, RoutedEventArgs e)
        {
            IntentionQuoteInfo.Clear();
            dataCount = 0;
            selectedCount = 0;
            //this.CountStr.Text = $"共{dataCount},选中{selectedCount}";
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            foreach(IssueRecogizeItem item in IntentionQuoteInfo)
            {
                if (item.IsChecked)
                {
                    item.NodeInfo.IsSelected = true;
                }
            }

            if (IsAutoPrase)
            {
                List<Node> lst = IntentionQuoteInfo.Where(o => o.IsChecked).Select(o=>o.NodeInfo).ToList();
                RefreshSelectedAction?.Invoke(lst);
            }
            else
            {
                RefreshAction?.Invoke(IntentionQuoteInfo);
            }
            this.Close();
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            bool flag = (sender as CheckBox).IsChecked.Value;
            int i = 0;
            foreach (IssueRecogizeItem item in this.intentionQuoteInfo)
            {
                item.IsChecked = flag;
                if (flag)
                {
                    i++;
                }
            }
            selectedCount = i;
            //this.CountStr.Text = $"共{dataCount},选中{selectedCount}";
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
             SetOppositeCheckAll();
        }

        internal void SetOppositeCheckAll()
        {
            bool flag = true;
            int i = 0;
            foreach (IssueRecogizeItem item in this.intentionQuoteInfo)
            {
                flag = flag && item.IsChecked;
                if (item.IsChecked)
                {
                    i++;
                }
            }
            CheckBox_All.IsChecked = this.intentionQuoteInfo.Count > 0 && flag;

            selectedCount = i;
            //this.CountStr.Text = $"共{dataCount},选中{selectedCount}";
        }

        private void WindowBase_Activated(object sender, EventArgs e)
        {
            contentTxt.Focus();
        }

        /// <summary>
        /// 初始化,自动识别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IssueRecogizationWin_Loaded(object sender, RoutedEventArgs e)
        {
            this.contentTxt.Dispatcher.Invoke((Action)(() =>
            {
                string tmp = this.contentTxt.Text ?? "";
                if (!string.IsNullOrEmpty(tmp) && IsAutoPrase)
                {
                    btnRecognize_Click(sender, e);
                }
            }));
        }
    }

    public class IssueRecogizeItem : ViewModelBase
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set 
            {
                _isChecked = value;
                this.OnPropertyChanged();
            }
        }

        // 序号
        private int? _no;

        public int? No
        {
            get
            {
                return _no;
            }
            set
            {
                _no = value;
                this.OnPropertyChanged();
            }
        }


        private Node _nodeInfo;

        public Node NodeInfo
        {
            get { return _nodeInfo; }
            set { _nodeInfo = value;
                this.OnPropertyChanged();
            }
        }


    }
}
