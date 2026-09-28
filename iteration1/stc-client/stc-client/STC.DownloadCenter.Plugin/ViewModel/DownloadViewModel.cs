using com.gateway.log;
using Downloader;
using STC.Common.Model;
using STC.Common.Model.MDHist;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.DownloadCenter.Plugin.Utils;
using STC.DownloadCenter.Plugin.ViewModel.Data;
using STC.Frame.Utils.ExcelHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.DownloadCenter.Plugin.ViewModel
{
    public class DownloadViewModel : BaseViewModel
    {
        private object _dataLock = new object();
        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private CustomCollection<DownloadItem> _downloadItems = new CustomCollection<DownloadItem>();

        public CustomCollection<DownloadItem> DownloadItems
        {
            get { return _downloadItems; }
            set { _downloadItems = value; }
        }

        private DownloadItem _selectedItem;

        public DownloadItem SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; }
        }

        private ICommand _delCommand;

        public ICommand DelCommand
        {
            get { return _delCommand; }
            set { _delCommand = value; }
        }

        private ICommand _delSelectedOrdersCommand;
        public ICommand DelSelectedOrdersCommand
        {
            get { return _delSelectedOrdersCommand; }
            set { _delSelectedOrdersCommand = value; }
        }

        private ICommand _openDirectoryCommand;

        public ICommand OpenDirectoryCommand
        {
            get { return _openDirectoryCommand; }
            set { _openDirectoryCommand = value; }
        }

        private ICommand _downloadCommand;

        public ICommand DownloadCommand
        {
            get { return _downloadCommand; }
            set { _downloadCommand = value; }
        }

        private ICommand _downloadSelectedCommand;

        public ICommand DownloadSelectedOrdersCommand
        {
            get { return _downloadSelectedCommand; }
            set { _downloadSelectedCommand = value; }
        }

        private ICommand _downloadAllCommand;

        public ICommand DownloadAllCommand
        {
            get { return _downloadAllCommand; }
            set { _downloadAllCommand = value; }
        }

        private ICommand _importCommand;

        public ICommand ImportCommand
        {
            get { return _importCommand; }
            set { _importCommand = value; }
        }

        private ICommand _exportCommand;

        public ICommand ExportCommand
        {
            get { return _exportCommand; }
            set { _exportCommand = value; }
        }

        private bool _ExportEnabled = true;

        public bool ExportEnabled
        {
            get
            {
                return _ExportEnabled;
            }
            set
            {
                this._ExportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _ImportEnabled = true;

        public bool ImportEnabled
        {
            get
            {
                return _ImportEnabled;
            }
            set
            {
                this._ImportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private string _FilePath;

        public string FilePath
        {
            get {
                if(string.IsNullOrEmpty(_FilePath))
                {
                    _FilePath = $"{System.AppDomain.CurrentDomain.BaseDirectory}Download";
                }
                return _FilePath;
            }
            set { 
                _FilePath = value;
                this.OnPropertyChanged();
            }
        }

        private int _dataCount;

        public int DataCount
        {
            get { return _dataCount; }
            set { _dataCount = value;
                this.OnPropertyChanged();
            }
        }

        private int _checkedCount;

            public int CheckedCount
        {
            get { return _checkedCount; }
            set
            {
                _checkedCount = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 0: 显示 1：不显示
        /// </summary>
        private Visibility isHiddenMyOrdersOperationGrid = Visibility.Collapsed;

        public Visibility IsHiddenMyOrdersOperationGrid
        {
            get
            {
                return isHiddenMyOrdersOperationGrid;
            }

            set
            {
                if (isHiddenMyOrdersOperationGrid != value)
                {
                    isHiddenMyOrdersOperationGrid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SearchText;

        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                this.OnPropertyChanged();
                this.ViewSource.View.Refresh();
            }
        }

        ConcurrentDictionary<string, DownloadService> _dic = new ConcurrentDictionary<string, DownloadService>();
        ConcurrentDictionary<string, CancellationTokenSource> _DownloadinDic = new ConcurrentDictionary<string, CancellationTokenSource>();
        

        public DownloadViewModel()
        {
            OpenDirectoryCommand = new ComRelayCommand(OpenDirectory);
            DelCommand = new ComRelayCommand(DelOne);
            DelSelectedOrdersCommand = new ComRelayCommand(DelSelected);
            DownloadCommand = new ComRelayCommand(DownloadOne);
            DownloadSelectedOrdersCommand = new ComRelayCommand(DownloadSelected);
            DownloadAllCommand = new ComRelayCommand(DownloadAll);
            ImportCommand = new ComRelayCommand(Import);
            ExportCommand = new ComRelayCommand(Export);

            this.FilePath = IniFileHelper.ReadValue($"Download-{LocalEnv.UserInfo.UserName}", "DownloadPath");

            ViewSource.Source = DownloadItems;
            DownloadItems.CollectionChanged += DownloadItems_CollectionChanged;
            BindingOperations.EnableCollectionSynchronization(ViewSource.View, _dataLock);
            SetFilterDatas();

            LoadData();

            DownloadSqliteManager.Instance.OperationAct += Instance_OperationAct;
        }

        #region 导出
        private async void Export()
        {
            int totalSize = 0;
            int successCount = 0;
            int failCount = 0;

            this.ExportEnabled = false;
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            try
            {
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = DateTimeEx.ServerTime.ToString("yyyyMMddHHmmss") + "_下载中心";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    using (StreamWriter writer = CSVHelper.Instance.createStream(fileName))
                    {
                        // 输出表头
                        writer.WriteLine(GetHeaderLine(mainExportDic));


                        await Task.Run(() =>
                        {
                            foreach (DownloadItem item in DownloadItems)
                            {

                                    StringBuilder lineSB = new StringBuilder();
                                    try
                                    {
                                        foreach (KeyValuePair<string, string> pair in mainExportDic)
                                        {
                                            string field = pair.Key;

                                            object fieldValue = typeof(DownloadItem).GetProperty(field).GetValue(item);

                                            string value = Common.Utils.CommonUtils.ConvertToString(fieldValue);

                                            if (field.Equals("DataType", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "download_datatype");
                                            }
                                            else if (field.Equals("SettleType", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "settletype");
                                            }

                                            if (lineSB.Length > 0)
                                            {
                                                lineSB.Append(",");
                                            }

                                            lineSB.Append($"\"{value}\"");
                                        }
                                        writer.WriteLine(lineSB);
                                        successCount++;
                                    }
                                    catch (Exception e)
                                    {
                                        LogHelper.Error(e);
                                        failCount++;
                                    }
                                    totalSize++;
                              

                            }
                        });
                        // 释放资源
                        writer.Flush();
                        writer.Close();
                    }
                    Common.View.Component.CustomMessageBox.Info($"《{saveFileDialog.FileName}》已保存。总条数：{totalSize}   成功：{successCount}  失败：{failCount}");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                this.ExportEnabled = true;
            }
        }

        private string GetHeaderLine(List<KeyValuePair<string, string>> list)
        {
            StringBuilder headerSb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in list)
            {
                if (headerSb.Length > 0)
                {
                    headerSb.Append(",");
                }
                headerSb.Append(pair.Value);
            }

            return headerSb.ToString();
        }

        List<KeyValuePair<string, string>> mainExportDic = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("SecurityID","合约代码"),
            new KeyValuePair<string, string>("Symbol","简称"),
            new KeyValuePair<string, string>("DataType","数据类型"),
            new KeyValuePair<string, string>("SettleType","清算速度"),
            new KeyValuePair<string, string>("StartDate","开始时间"),
            new KeyValuePair<string, string>("EndDate","结束时间"),
            new KeyValuePair<string, string>("Frequency","K线频率"),
            new KeyValuePair<string, string>("Type","行情类型"),
        };
        #endregion

        #region 导入
        List<string> datatypeList = new List<string>(new string[] { "1", "2", "3", "4", "5", "6", "7", "8" });
        List<string> settletypeList = new List<string>(new string[] { "1", "2" });
        List<string> frequencyList = new List<string>(new string[] { "1M", "5M", "15M", "30M", "1H ", "1D" });
        List<string> typeList = new List<string>(new string[] { "CTNP", "TBNP", "CT" });
        
        public async void Import()
        {

            try
            {
                this.ImportEnabled = false;
                // 1.弹框
                //创建一个保存文件式的对话框
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                //设置这个对话框的起始保存路径
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.RestoreDirectory = true;
                //设置保存的文件的类型，注意过滤器的语法
                ofd.Filter = "(文本文件*.csv)|*.csv";
                ofd.DefaultExt = ".csv";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ofd.FileName.EndsWith(".csv", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持csv";
                        prompt.Show();
                        return;
                    }

                    // 3.导入操作
                    StringBuilder sb = new StringBuilder();

                    DataTable dt = CSVHelper.Instance.ReadCSV(ofd.FileName);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        List<DownloadItem> importList = new List<DownloadItem>();
                        foreach (DataRow dr in dt.Rows)
                        {
                            string securityID = dr["合约代码"].ConvertToString().Replace("\"",string.Empty);
                            Common.Model.Base.BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                            string symbol = dr["简称"].ConvertToString().Replace("\"", string.Empty);
                            string datatype_ori = dr["数据类型"].ConvertToString().Replace("\"", string.Empty);
                            string settleType_ori = dr["清算速度"].ConvertToString().Replace("\"", string.Empty).ToUpper();
                            string datatype = ComboSourceUtil.Instance.FindEnum(datatype_ori, "download_datatype_revert");
                            string settleType = ComboSourceUtil.Instance.FindEnum(settleType_ori, "settletype_revert");
                            string startDate = dr["开始时间"].ConvertToString().Replace("\"", string.Empty);
                            string endDate = dr["结束时间"].ConvertToString().Replace("\"", string.Empty);
                            string frequency = dr["K线频率"].ConvertToString().Replace("\"", string.Empty);
                            string type = dr["行情类型"].ConvertToString().Replace("\"", string.Empty);

                            bool isValidTime = false;
                            try
                            {
                                DateTime startDate_dt = DateTime.ParseExact(startDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                                DateTime endDate_dt = DateTime.ParseExact(endDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                                isValidTime = startDate_dt.ToString("yyyy-MM-dd HH:mm:ss") == startDate
                                    && endDate_dt.ToString("yyyy-MM-dd HH:mm:ss") == endDate
                                    && startDate_dt.CompareTo(endDate_dt) <= 0;
                            }
                            catch (Exception e)
                            {

                            }

                            //检查要素
                            if (string.IsNullOrEmpty(securityID)
                                || bondsInfoPo == null
                                || string.IsNullOrEmpty(datatype)
                                || !datatypeList.Contains(datatype)
                                || (datatype != "5" && datatype != "6" && datatype != "7" && datatype != "8" && (string.IsNullOrEmpty(settleType) || !settletypeList.Contains(settleType)))
                                || isValidTime == false
                                || (datatype == "7" && (string.IsNullOrEmpty(frequency) || !frequencyList.Contains(frequency.ToUpper())))
                                || (datatype == "8" && (string.IsNullOrEmpty(type) || !typeList.Contains(type.ToUpper()))))
                            {
                                continue;
                            }

                            DownloadItem item = new DownloadItem();
                            item.ID = System.Guid.NewGuid().ToString("N");
                            item.SecurityID = securityID;
                            item.Symbol = bondsInfoPo == null ? symbol : bondsInfoPo.Short_Name;
                            item.DataType = datatype;
                            item.SettleType = settleType;
                            item.StartDate = startDate;
                            item.EndDate = endDate;
                            item.CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            item.UserName = LocalEnv.UserInfo.UserName;
                            item.Frequency = frequency;
                            item.Type = type;
                            importList.Add(item);
                        }

                        if (!DownloadSqliteManager.Instance.Insert(importList))
                        {
                            promptTxt = "导入失败";
                        }
                        else
                        {
                            if (importList.Count != dt.Rows.Count)
                            {
                                promptTxt = $"部分导入成功, 共{dt.Rows.Count}条，成功{importList.Count}条";
                            }
                            else
                            {
                                promptTxt = "导入成功";
                            }
                        }
                    }
                    else
                    {
                        promptTxt = "无数据或文件有误或文件被占用";
                    }
                }
                else
                {
                    promptTxt = "取消导入";
                }

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导入";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导入";
                prompt.PromptContext = e.Message;
                prompt.Show();

                LogHelper.Error(e);
            }
            finally
            {
                this.ImportEnabled = true;
            }
        }
        #endregion

        private void SetFilterDatas()
        {
            this.ViewSource.View.Filter = x => {
                DownloadItem data = x as DownloadItem;

                if (string.IsNullOrEmpty(this.SearchText) == false
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.SearchText))
                    && (data.Symbol == null || !data.Symbol.Contains(this.SearchText)))
                    return false;

                return true;
            };
        }

        private void DownloadItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DataCount = DownloadItems.Count;
            isAdjustAllChkStatus_Ignore = true;
            RefreshCheckBox();
        }

        private void LoadData()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    DataTable dt = DownloadSqliteManager.Instance.GetData();
                    foreach(DataRow dr in dt.Rows)
                    {
                        DownloadItem item = new DownloadItem();
                        item.ID = dr["ID"].ConvertToString();
                        item.SecurityID = dr["BondID"].ConvertToString();
                        item.Symbol = dr["BondName"].ConvertToString();
                        item.DataType = dr["DataType"].ConvertToString();
                        item.DataSize = dr["DataSize"].ConvertToString();
                        item.LastStatus = dr["LastStatus"].ConvertToString();
                        item.FileName = dr["FileName"].ConvertToString();
                        item.StartDate = dr["StartDate"].ConvertToString();
                        item.EndDate = dr["EndDate"].ConvertToString();
                        item.CreateTime = dr["CreateTime"].ConvertToString();
                        item.UpdateTime = dr["UpdateTime"].ConvertToString();
                        item.SettleType = dr["SettlType"].ConvertToString();
                        item.Frequency = dr["Frequency"].ConvertToString();
                        item.Type = dr["Type"].ConvertToString();
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            DownloadItems.Add(item);
                        }), null);
                        
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        private void Instance_OperationAct(string arg1, List<DownloadItem> arg2)
        {
            if (arg1 == "1")
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach(DownloadItem item in arg2)
                    {
                        DownloadItems.Add(item);
                    }
                }), null);
            }
        }

        private void OpenDirectory()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "下载路径";
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.FilePath = fbd.SelectedPath;    
            }
            IniFileHelper.WriteValue($"Download-{LocalEnv.UserInfo.UserName}", "DownloadPath", this.FilePath);
        }

        #region 选择
        private bool _hasSelected_Ignore = false;
        // 是否选中全选多选框
        private bool _IsAllChecked_Ignore = false;
        public bool IsAllChecked_Ignore
        {
            get
            {
                return _IsAllChecked_Ignore;
            }

            set
            {
                if (_IsAllChecked_Ignore != value)
                {
                    _IsAllChecked_Ignore = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private volatile bool isAdjustAllChkStatus_Ignore = false;
        public void CheckBoxAllValueChange_Ignore(bool isAllChecked)
        {

            lock (_dataLock)
            {
                foreach (DownloadItem item in this.ViewSource.View)
                {

                    item.IsChecked = isAllChecked;
                }
            }

            if (this.ViewSource.View.IsEmpty == true)
            {
                IsAllChecked_Ignore = false;
                _hasSelected_Ignore = false;
            }
            else
            {
                IsAllChecked_Ignore = isAllChecked;
                _hasSelected_Ignore = isAllChecked;
            }

            if (_hasSelected_Ignore)
            {
                IsHiddenMyOrdersOperationGrid = Visibility.Visible;
            }
            else
            {
                IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
            }

            CheckedCount = this.DownloadItems.Count(item => item.IsChecked == true);
        }

        public void AdjustAllChkStatus_Ignore()
        {
            bool isCheckAll = true;
            _hasSelected_Ignore = false;
            lock (_dataLock)
            {
                foreach (DownloadItem item in this.ViewSource.View)
                {
                    if (item.IsChecked == true)
                    {
                        _hasSelected_Ignore = true;
                    }
                    if (item.IsChecked == false)
                    {
                        isCheckAll = false;
                    }
                    if (_hasSelected_Ignore == true && isCheckAll == false)
                    {
                        break;
                    }
                }
                IsAllChecked_Ignore = isCheckAll && this.ViewSource.View.IsEmpty == false;
                if (this.ViewSource.View.IsEmpty)
                {
                    _hasSelected_Ignore = false;
                }
            }

            if(_hasSelected_Ignore)
            {
                IsHiddenMyOrdersOperationGrid = Visibility.Visible;
            }
            else
            {
                IsHiddenMyOrdersOperationGrid = Visibility.Hidden;
            }

            CheckedCount = this.DownloadItems.Count(item => item.IsChecked == true);
        }

        public void CheckBoxSingleClick_Ignore()
        {
            isAdjustAllChkStatus_Ignore = true;
            RefreshCheckBox();
        }

        public void RefreshCheckBox()
        {
            isAdjustAllChkStatus_Ignore = false;
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.AdjustAllChkStatus_Ignore();

                    if (isAdjustAllChkStatus_Ignore == true)
                    {
                        RefreshCheckBox();
                    }
                }));
            });
        }
        #endregion

        private void DelOne()
        {
            bool result = CustomMessageBox.Question("注意,该操作会同时删除本地下载数据记录,是否继续删除？", "删除订单");
            if (!result)
            {
                return;
            }

            StringBuilder ids = new StringBuilder();
            if (this.SelectedItem != null)
            {
                ids.Append($"'{this.SelectedItem.ID}'");
            }

            if (!DownloadSqliteManager.Instance.DeleteBond(ids.ToString()))
            {
                CommonUtils.Prompt2Second("提示", "删除失败");
            }
            else
            {
                if (!string.IsNullOrEmpty(this.SelectedItem.FileName) && File.Exists(this.SelectedItem.FileName))
                {
                    File.Delete(this.SelectedItem.FileName);
                }

                if (_DownloadinDic.ContainsKey(SelectedItem.ID))
                {
                    _DownloadinDic[SelectedItem.ID].Cancel();
                }
                this.DownloadItems.Remove(this.SelectedItem);
            }
        }

        private void DelSelected()
        {
            bool result = CustomMessageBox.Question("注意,该操作会同时删除本地下载数据记录,是否继续删除？", "删除订单");
            if (!result)
            {
                return;
            }

            StringBuilder ids = new StringBuilder();
            List<DownloadItem> removelist = new List<DownloadItem>();
            lock (_dataLock)
            {
                foreach(DownloadItem item in DownloadItems)
                {
                    if (item.IsChecked)
                    {
                        if (ids.Length > 0)
                        {
                            ids.Append(CommonConstants.COMMA_CHAR);
                        }
                        ids.Append($"'{item.ID}'");
                        removelist.Add(item);
                    }
                }
            }

            if (!DownloadSqliteManager.Instance.DeleteBond(ids.ToString()))
            {
                CommonUtils.Prompt2Second("提示", "删除失败");
            }
            else
            {
                ThreadPoolHelper.InvokeThreadPool((o) => {
                    foreach (DownloadItem removeitem in removelist)
                    {
                        if (!string.IsNullOrEmpty(removeitem.FileName) && File.Exists(removeitem.FileName))
                        {
                            File.Delete(removeitem.FileName);
                        }
                    }
                });

                foreach (DownloadItem removeitem in removelist)
                {
                    if (_DownloadinDic.ContainsKey(removeitem.ID))
                    {
                        _DownloadinDic[removeitem.ID].Cancel();
                    }
                    lock (_dataLock)
                    {
                        this.DownloadItems.Remove(removeitem);
                    }
                }
            }
        }

        private void DownloadOne()
        {
            List<DownloadItem> list = new List<DownloadItem>();
            list.Add(this.SelectedItem);
            ProcessDownload(list);
        }

        private void DownloadSelected()
        {
            List<DownloadItem> list = new List<DownloadItem>();
            foreach(DownloadItem item in this.DownloadItems)
            {
                if (item.IsChecked)
                {
                    list.Add(item);
                }
            }
            ProcessDownload(list);
        }

        private void DownloadAll()
        {
            ProcessDownload(this.DownloadItems.ToList());
        }

        private bool IsWebResourceAvailable(string webResourceAddress)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(webResourceAddress));
                req.Method = "HEAD";
                req.Timeout = 1000;
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                return (res.StatusCode == HttpStatusCode.OK);
            }
            catch (WebException wex)
            {
                System.Diagnostics.Trace.Write(wex.Message); return false;
            }
        }

        private bool IsDownloadFolderAvailable()
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                CommonUtils.Prompt2Second("提示", "请先选择下载路径");
                return false;
            }
            else
            {
                if (!System.IO.Directory.Exists(this.FilePath))
                {
                    System.IO.Directory.CreateDirectory(this.FilePath);
                }
            }
            return true;
        }

        private void ProcessDownload(List<DownloadItem> list)
        {
            if (list == null || list.Count == 0)
                return;

            if (!IsDownloadFolderAvailable())
                return;


            foreach (DownloadItem item in list)
            {
                if (_dic.ContainsKey(item.ID))
                {
                    continue;
                }
                else
                {
                    item.IsDownloading = true;
                    _dic[item.ID] = GenerateDownloador();
                }

                if (!_DownloadinDic.ContainsKey(item.ID))
                {
                    _DownloadinDic[item.ID] = new CancellationTokenSource();
                }
                
                Task.Factory.StartNew((token) => {
                    string datatype_display = ComboSourceUtil.Instance.FindEnum(item.DataType, "downloadfile_datatype");
                    string settlType_display = ComboSourceUtil.Instance.FindEnum(item.SettleType, "settletype");
                    string display = settlType_display;
                    
                    if (item.DataType == "7")
                    {
                        display = item.Frequency;
                    }
                    else if (item.DataType == "8")
                    {
                        display = item.Type;
                    }

                    string filename = string.Empty;
                    if (string.IsNullOrEmpty(item.FileName))
                    {
                        filename = $"{item.SecurityID}_{display}_{datatype_display}_{item.StartDate?.Replace(" ", "").Replace("-", "").Replace(":","")}_{item.EndDate?.Replace(" ", "").Replace("-", "").Replace(":", "")}.csv.gz"; //220215.IB_T + 1_bbo.csv.gz
                    }
                    else
                    {
                        int fileIndex = item.FileName.LastIndexOf("\\");
                        filename = item.FileName.Substring(fileIndex + 1);
                    }

                    lock(_dataLock)
                    {
                        int fileIndex = 1;
                        while (DownloadItems.Any(obj => obj.ID != item.ID && obj.FileName == $"{this.FilePath}\\{filename}"))
                        {
                            filename = $"{item.SecurityID}_{display}_{datatype_display}_{item.StartDate?.Replace(" ", "").Replace("-", "").Replace(":", "")}_{item.EndDate?.Replace(" ", "").Replace("-", "").Replace(":", "")}_({fileIndex}).csv.gz";
                            fileIndex++;
                        }
                        item.FileName = $"{this.FilePath}\\{filename}";
                    }
                    //string filename = string.Empty;
                    //lock (_dataLock)
                    //{
                    //    if (string.IsNullOrEmpty(item.FileName))
                    //    {
                    //        filename = $"{item.SecurityID}_{settlType_display}_{datatype_display}_{item.StartDate}_{item.EndDate}.csv.gz"; //220215.IB_T + 1_bbo.csv.gz
                    //        item.FileName = $"{this.FilePath}\\{filename}";
                    //    }
                    //    else
                    //    {
                    //        int fileIndex = item.FileName.LastIndexOf("\\");
                    //        filename = item.FileName.Substring(fileIndex + 1);
                    //    }
                    //}

                    item.DataSize = string.Empty;
                    item.LastStatus = $"文件{filename}尝试连接中...";
                    item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    DownloadSqliteManager.Instance.Update(item);

                    MDHistDReq req = new MDHistDReq();
                    req.securityID = item.SecurityID;
                    if (item.DataType == "1")
                    {
                        req.type = "7";
                        req.settlType = item.SettleType;
                    }
                    else if (item.DataType == "2")
                    {
                        req.type = "1";
                        req.settlType = item.SettleType;
                    }
                    else if (item.DataType == "3")
                    {
                        req.type = "3";
                        req.settlType = item.SettleType;
                    }
                    else if (item.DataType == "4")
                    {
                        req.settlType = item.SettleType;
                    }
                    else if (item.DataType == "7")
                    {
                        req.text = item.Frequency.ToUpper();
                    }
                    else if (item.DataType == "8")
                    {
                        req.type = item.Type.ToLower();
                    }
                    req.start = item.StartDate;
                    req.end = item.EndDate;
                    MDHistDReply reply = null;

                    if (item.DataType == "1" || item.DataType == "2" || item.DataType == "3")
                    {
                        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD(req);
                    }
                    else if (item.DataType == "4")
                    {
                        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD3(req);
                    }
                    else if (item.DataType == "5")
                    {
                        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD2(req);
                    }
                    else if (item.DataType == "6")
                    {
                        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD4(req);
                    }
                    else if (item.DataType == "7")
                    {
                        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD5(req);
                    }
                    else if (item.DataType == "8")
                    {
                        req.securityID = req.securityID.Replace(".", "");
                        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD6(req);
                    }

                    int count = 0;
                    //while ((reply == null || reply.data == null) && count < 6)
                    //{
                    //    System.Threading.Thread.Sleep(5000);

                    //    item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    //    DownloadSqliteManager.Instance.Update(item);

                    //    if (item.DataType == "1" || item.DataType == "2" || item.DataType == "3")
                    //    {
                    //        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD(req);
                    //    }
                    //    else if (item.DataType == "4")
                    //    {
                    //        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD3(req);
                    //    }
                    //    else if (item.DataType == "5")
                    //    {
                    //        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD2(req);
                    //    }
                    //    else if (item.DataType == "6")
                    //    {
                    //        reply = Common.Handler.MDHist.MDHistHandler.Instance.MDHistD4(req);
                    //    }
                    //    count++;
                    //};


                    if (reply == null || reply.data == null)
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                            item.IsDownloading = false;
                        //}));

                        DownloadService downloader;
                        _dic.TryRemove(item.ID, out downloader);

                        CancellationTokenSource cts;
                        _DownloadinDic.TryRemove(item.ID, out cts);

                        string errorMsg = reply != null && !string.IsNullOrEmpty(reply.Msg) ? $" {reply.Msg}" : " 返回超时";
                        item.LastStatus = $"文件{filename}{errorMsg}";
                        item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        DownloadSqliteManager.Instance.Update(item);
                    }
                    else
                    {
                        count = 0;
                        do
                        {
                            if (_DownloadinDic[item.ID].IsCancellationRequested)
                            {
                                //Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                                    item.IsDownloading = false;
                                //}));

                                DownloadService downloader;
                                _dic.TryRemove(item.ID, out downloader);

                                CancellationTokenSource cts;
                                _DownloadinDic.TryRemove(item.ID, out cts);
                                return;
                            }

                            System.Threading.Thread.Sleep(3000);
                            item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            count++;
                        }
                        while (!IsWebResourceAvailable(reply.data));

                        //if (count == 30)
                        //{
                        //    item.LastStatus = $"文件{filename}生成超时";
                        //    item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        //    DownloadSqliteManager.Instance.Update(item);
                        //    return;
                        //}

                        item.LastStatus = $"文件{filename}正在下载...";
                        item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        DownloadSqliteManager.Instance.Update(item);

                        Download(reply.data, filename, item.ID);

                        Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                            item.IsDownloading = false;
                        }));
                    }
                }, _DownloadinDic[item.ID].Token);
            }
        }

        private DownloadService GenerateDownloador()
        {
            var downloadOpt = new DownloadConfiguration()
            {
                BufferBlockSize = 10240, // 通常，主机最大支持8000字节，默认值为8000。
                ChunkCount = 8, // 要下载的文件分片数量，默认值为1
                //MaximumBytesPerSecond = 1024 * 1024 * 20, // 下载速度限制为1MB/s，默认值为零或无限制
                MaxTryAgainOnFailover = int.MaxValue, // 失败的最大次数
                ParallelDownload = true, // 下载文件是否为并行的。默认值为false
                Timeout = 30000, // 每个 stream reader  的超时（毫秒），默认值是1000
                RequestConfiguration = // 定制请求头文件
                {
                    Accept = "*/*",
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    CookieContainer = new CookieContainer(), // Add your cookies
                    Headers = new WebHeaderCollection(), // Add your custom headers
                    KeepAlive = false,
                    ProtocolVersion = HttpVersion.Version11, // Default value is HTTP 1.1
                    UseDefaultCredentials = false,
                    UserAgent = $"DownloaderSample/{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}",
                }
            };

            DownloadService downloader = new DownloadService(downloadOpt);
            // Provide `FileName` and `TotalBytesToReceive` at the start of each downloads
            // 在每次下载开始时提供 "文件名 "和 "要接收的总字节数"。
            downloader.DownloadStarted += OnDownloadStarted;

            // Provide any information about chunker downloads, like progress percentage per chunk, speed, total received bytes and received bytes array to live streaming.
            // 提供有关分块下载的信息，如每个分块的进度百分比、速度、收到的总字节数和收到的字节数组，以实现实时流。
            downloader.ChunkDownloadProgressChanged += OnChunkDownloadProgressChanged;

            // Provide any information about download progress, like progress percentage of sum of chunks, total speed, average speed, total received bytes and received bytes array to live streaming.
            // 提供任何关于下载进度的信息，如进度百分比的块数总和、总速度、平均速度、总接收字节数和接收字节数组的实时流。
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;

            // Download completed event that can include occurred errors or cancelled or download completed successfully.
            // 下载完成的事件，可以包括发生错误或被取消或下载成功。
            downloader.DownloadFileCompleted += DownloadFileCompleted;

            return downloader;
        }

        private async void Download(string url, string filename, string id)
        {
            if (_dic.ContainsKey(id))
            {
                DownloadService downloader = _dic[id];
                string file = $"{this.FilePath}\\{filename}";
                await downloader.DownloadFileTaskAsync(url, file); // string url = @"https://www.baidu.com";

                await downloader.Clear();
                downloader.Dispose();

                _dic.TryRemove(id, out downloader);

                CancellationTokenSource token;
                _DownloadinDic.TryRemove(id, out token);
            }
        }

        private void OnDownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            if (sender != null && sender is DownloadService)
            {
                DownloadService ds = sender as DownloadService;

                foreach(KeyValuePair<string, DownloadService> pair in _dic)
                {
                    if (pair.Value == ds)
                    {
                        string filename = ds.Package.FileName.Substring(ds.Package.FileName.LastIndexOf("\\") + 1);
                        //string id = filename.Substring(0, filename.IndexOf("_temp_"));
                        //string newfilename = filename.Substring(filename.IndexOf("_temp_") + 6);
                        string id = pair.Key;
                        IEnumerable<DownloadItem> items = DownloadItems.Where(item => item.ID == id);
                        if (items != null && items.Count() > 0)
                        {
                            DownloadItem item = items.First();
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                item.LastStatus = $"文件{filename}:{e.ProgressPercentage.ToString("0")}%;{e.ReceivedBytesSize}/{e.TotalBytesToReceive}";//;[{e.BytesPerSecondSpeed}B/s]
                                item.DataSize = ds.Package.TotalFileSize.ToString();
                                item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }));
                            DownloadSqliteManager.Instance.Update(item);
                        }
                        return;
                    }
                }
            }
        }

        private void OnChunkDownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            
        }

        private void OnDownloadStarted(object sender, DownloadStartedEventArgs e)
        {
            if (sender is DownloadService)
            {
                DownloadService ds = sender as DownloadService;

                foreach (KeyValuePair<string, DownloadService> pair in _dic)
                {
                    if (pair.Value == ds)
                    {
                        string filename = e.FileName.Substring(e.FileName.LastIndexOf("\\") + 1);
                        //string id = filename.Substring(0, filename.IndexOf("_temp_"));
                        string id = pair.Key;
                        IEnumerable<DownloadItem> items = DownloadItems.Where(item => item.ID == id);
                        if (items != null && items.Count() > 0)
                        {
                            DownloadItem item = items.First();
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                item.DataSize = e.TotalBytesToReceive.ToString();
                                item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                            }));
                            DownloadSqliteManager.Instance.Update(item);
                        }
                    }
                }
            }
        }

        private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (sender is DownloadService)
            {
                DownloadService ds = sender as DownloadService;

                foreach (KeyValuePair<string, DownloadService> pair in _dic)
                {
                    if (pair.Value == ds)
                    {
                        if (e.UserState != null && e.UserState is DownloadPackage)
                        {
                            DownloadPackage userState = e.UserState as DownloadPackage;
                            string filename = userState.FileName.Substring(userState.FileName.LastIndexOf("\\") + 1);
                            //string id = filename.Substring(0, filename.IndexOf("_temp_"));
                            //string newfilename = filename.Substring(filename.IndexOf("_temp_") + 6);
                            string id = pair.Key;

                            //try
                            //{
                            //    if (System.IO.File.Exists(userState.FileName.Replace(filename, newfilename)))
                            //        System.IO.File.Delete(userState.FileName.Replace(filename, newfilename));
                            //}
                            //catch (Exception ex)
                            //{
                            //    LogHelper.Error("completed delete", ex);
                            //}

                            //try
                            //{
                            //    if (System.IO.File.Exists(userState.FileName))
                            //        System.IO.File.Move(userState.FileName, userState.FileName.Replace(filename, newfilename));
                            //}
                            //catch (Exception ex)
                            //{
                            //    LogHelper.Error("completed delete", ex);
                            //}
                            IEnumerable<DownloadItem> items = DownloadItems.Where(item => item.ID == id);
                            if (items != null && items.Count() > 0)
                            {
                                DownloadItem item = items.First();
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    item.LastStatus = $"文件{filename}:100%;{userState.TotalFileSize}/{userState.TotalFileSize}";
                                    item.DataSize = userState.TotalFileSize.ToString();
                                    item.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                }));
                                DownloadSqliteManager.Instance.Update(item);
                            }
                        }
                    }
                }
            }
        }
    }
}
