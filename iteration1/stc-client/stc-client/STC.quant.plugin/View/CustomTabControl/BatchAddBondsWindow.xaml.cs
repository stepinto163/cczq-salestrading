using com.gateway.log;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.quant.plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Windows.Shapes;

namespace STC.quant.plugin.View.HomePage
{
    /// <summary>
    /// BatchAddBondsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BatchAddBondsWindow : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }
        public string SecurityID_ExcelField = "债券代码";
        private AlterParamViewModel _batchAddBondsWindowViewModel = AlterParamViewModel.Instance;
        private Dictionary<string, string> dic = new Dictionary<string, string>();
        private Dictionary<string, List<HomeGroupSecurityIDsInfo>> dicGroup = new Dictionary<string, List<HomeGroupSecurityIDsInfo>>();
        public IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroupList;

        //新增还是编辑，默认是编辑
        public bool IsAdd = false;
        public string bondCode;
        public BatchAddBondsWindow(bool IsAdd, string bondCode)
        {
            InitializeComponent();
            _batchAddBondsWindowViewModel.IsAdd = this.IsAdd = IsAdd;
            this.bondCode = bondCode = bondCode.Replace(",", "\r");
            if (!IsAdd)
            {
                this.importTxt.Text = this.bondCode;
            }
            this.DataContext = _batchAddBondsWindowViewModel;
            LoadGroups();
        }

        private async void LoadGroups()
        {
            //sortedGroupList = await HomePageDataHandler.AsyncRequestGroups(false);
            //if (sortedGroupList != null)
            //{
            //    foreach (KeyValuePair<string, HomeGroupInfo> pair in sortedGroupList)
            //    {
            //        HomeGroupInfo group = pair.Value;
            //        if (!dic.ContainsKey(group.id))
            //        {
            //            dic.Add(group.id, group.groupname);
            //            dicGroup.Add(group.id, group.securityids);
            //        }
            //    }
            //    comGroup.ItemsSource = dic;
            //}
        }

        private void ComGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox c = sender as ComboBox;
            if (c != null)
            {
                string s = c.SelectedValue.ToString();
                // sortedGroupList.Select
                var parent = dicGroup.First(o => o.Key.Equals(s));
                List<HomeGroupSecurityIDsInfo> list = parent.Value;
                //this.importTxt.Text = bondCode;
                if (list != null && list.Count > 0)
                {
                    StringBuilder str = new StringBuilder();
                    foreach (HomeGroupSecurityIDsInfo home in list)
                    {
                        str.Append(home.securityid);
                        str.Append("\r\n");
                    }
                    if (IsAdd)
                        this.importTxt.Text += str.ToString();
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(this.importTxt.Text))
                            this.importTxt.Text += "\r\n";
                        this.importTxt.Text += str.ToString();
                    }
                }
            }
        }


        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string importStr = this.importTxt.Text;
            _batchAddBondsWindowViewModel.RecognizeIntentionQuote(importStr);
            this.step1.Visibility = Visibility.Collapsed;
            this.step2.Visibility = Visibility.Visible;
            this.btnCancel.Visibility = Visibility.Collapsed;
            this.btnOK.Visibility = Visibility.Collapsed;
            this.btnBack.Visibility = Visibility.Visible;
            this.btnOK2.Visibility = Visibility.Visible;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK2_Click(object sender, RoutedEventArgs e)
        {

            //// todo 验证
            //this.Close();
            // e.Handled = true;
            this.step2.Visibility = Visibility.Collapsed;
            this.btnBack.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Collapsed;
            this.step3.Visibility = Visibility.Visible;
            this.btnOK3.Visibility = Visibility.Visible;
            this.btnCancel.Visibility = Visibility.Hidden;
            // 开始批量导入
            _batchAddBondsWindowViewModel.BatchImportBonds();

            this.Result = true;
            this.Close();
            e.Handled = true;

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.step1.Visibility = Visibility.Visible;
            this.step2.Visibility = Visibility.Collapsed;
            this.btnCancel.Visibility = Visibility.Visible;
            this.btnOK.Visibility = Visibility.Visible;
            this.btnBack.Visibility = Visibility.Collapsed;
            this.btnOK2.Visibility = Visibility.Collapsed;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            _batchAddBondsWindowViewModel.CalCheckedCount();
        }

        private void btnOK3_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;

            // todo 验证
            this.Close();
            e.Handled = true;
        }

        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (this._batchAddBondsWindowViewModel != null && this._batchAddBondsWindowViewModel.FinishOpt() == false)
            //{
            //    bool flag = CustomMessageBox.Question("确认要取消批量导入吗?", "取消批量导入",this);
            //    if (!flag)
            //    {
            //        e.Cancel = true;
            //        return;
            //    }
            //}
            this.Owner = null;
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (_batchAddBondsWindowViewModel != null)
            {
                _batchAddBondsWindowViewModel.CancelOpt();
            }
        }

        private void Checkrole_Click(object sender, RoutedEventArgs e)
        {
            //if (checkrole.IsChecked.Value)
            //{
            //    comGroup.IsEnabled = true;
            //}
            //else
            //    comGroup.IsEnabled = false;

        }

        #region 导入
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            ImportExcel();
        }

        private async void ImportExcel()
        {
            try
            {
                // 1.弹框
                //创建一个保存文件式的对话框
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();                              //设置这个对话框的起始保存路径
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.RestoreDirectory = true;
                //设置保存的文件的类型，注意过滤器的语法
                ofd.Filter = "Excel Documents|*.xls;*.xlsx";
                ofd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ofd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !ofd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }

                    List<string> errorList = await Sub_ImportExcel(ofd.FileName);

                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导入成功";
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
                    //CustomMessageBox.Info(promptTxt, "批量导入");
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
        }

        private Task<List<string>> Sub_ImportExcel(string filePath)
        {
            return Task<List<string>>.Run(async () =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                    if (isFileLock)
                    {
                        errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                        return errorlist;
                    }
                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    Dictionary<int, string> dic = helper.ReturnSheetList();
                    StringBuilder str = new StringBuilder();
                    foreach (KeyValuePair<int, string> pair in dic)
                    {
                        DataTable importDT = helper.ExcelToDataTable(pair.Key);
                        if (importDT != null && importDT.Rows.Count > 0)
                        {
                            foreach (DataRow dr in importDT.Rows)
                            {
                                string securityID = CommonUtils.ConvertToString(dr[SecurityID_ExcelField]);
                                str.Append(securityID);
                                str.Append("\r");
                            }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.importTxt.Text = bondCode;
                                if (!string.IsNullOrWhiteSpace(this.importTxt.Text))
                                    this.importTxt.Text += "\r\n";
                                this.importTxt.Text += str.ToString();
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return null;
            });
        }
        #endregion

        #region 导出
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportExcel();
        }
        private async void ExportExcel()
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"策略债券导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
                //设置保存的文件的类型，注意过滤器的语法
                sfd.Filter = "Excel Documents|*.xls;*.xlsx;";
                sfd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!sfd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !sfd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导出";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    string text = this.importTxt.Text;
                    //});
                    List<string> errorList = await Sub_ExportExcel(sfd.FileName, text);
                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导出成功";
                    }
                }
                else
                {
                    promptTxt = "取消导出";
                }
                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导出";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                    //CustomMessageBox.Info(promptTxt, "批量导入");
                }
            }
            catch (Exception ex)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导出";
                prompt.PromptContext = ex.Message;
                prompt.Show();
                LogHelper.Error(ex);
            }
        }

        private Task<List<string>> Sub_ExportExcel(string filePath, string text)
        {
            return Task.Run(() =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    if (File.Exists(filePath))
                    {
                        bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                        if (isFileLock)
                        {
                            errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                            return errorlist;
                        }
                        File.Delete(filePath);
                    }
                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    if (string.IsNullOrEmpty(text) == false)
                    {
                        if (text.Contains("\r\n") || text.Contains("\r"))
                        {
                            text = text.Replace("\r\n", ",").Replace("\r", ",");
                            string[] str = text.Split(',');
                            Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                            DataTable dt = new DataTable();
                            dt.Columns.Add(SecurityID_ExcelField);
                            foreach (string s in str)
                            {
                                if (!string.IsNullOrEmpty(s))
                                {
                                    DataRow newDR = dt.NewRow();
                                    newDR[SecurityID_ExcelField] = s;
                                    dt.Rows.Add(newDR);
                                }
                            }
                            dataDic["1"] = dt;
                            Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                            if (str.Length == 0 || (result == null) || dt.Rows.Count == 0)
                                errorlist.Add("无数据导出");
                        }
                    }
                    else
                    {
                        errorlist.Add("无数据导出");
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }


        #endregion

    }
}
