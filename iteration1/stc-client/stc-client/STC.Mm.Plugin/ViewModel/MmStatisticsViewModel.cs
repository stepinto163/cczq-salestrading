using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Frame.Utils;
using System.Data;
using System.Windows.Input;
using STC.Mm.Plugin.Utils;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using Newtonsoft.Json;
using STC.Common.Model.MM;
using STC.Common.Handler.Admin;
using STC.Mm.Plugin.View.Component;
using STC.Mm.Plugin.View;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Data;
using STC.Common.Model.Ans;
using STC.Common.Handler.Ans;
using com.gateway.log;
using STC.Mm.Plugin.Command;
using STC.Frame.CustomUI.Component;
using STC.Common.Utils;

namespace STC.Mm.Plugin.ViewModel
{
    public class MmStatisticsViewModel : ViewModelBase
    {
        #region 做市概要表格字段
        public const string DATE_FIELD = "date"; // 日期 MM-dd
        public const string BNC_SECURITY_COUNT_FIELD = "bncSecurityCount"; // 利率债有效报价
        public const string BNC_SECURITY_TYPE_COUNT_FIELD = "bncSecurityTypeCount"; //利率债品种分布
        public const string BNC_TIME_TO_MATURITY_COUNT_FIELD = "bncTimetomaturityCount"; // 利率债剩余期限
        public const string BNC_ND_COUNT_FIELD = "bncNDCount"; // 利率债国债数量
        public const string BNC_NEW_ND_COUNT_FIELD = "bncNewNDCount"; // 利率债新发国债期限
        public const string BCO_SECURITY_COUNT_FIELD = "bcoSecurityCount"; // 信用债有效报价
        public const string BCO_SECURITY_TYPE_COUNT_FIELD = "bcoSecurityTypeCount"; // 信用债品种分布
        public const string BCO_ISSUER_COUNT_FIELD = "bcoIssuerCount"; // 信用债发行人覆盖
        #endregion

        #region 做市详情表格字段
        public const string SECURITY_ID_FIELD = "securityId"; // 债券代码
        public const string SYMBOL_FIELD = "symbol"; // 债券简称
        public const string SYMBOL_TYPE_FIELD = "symbolType"; //债券种类
        public const string USER_NAME_FIELD = "username"; // 报价人
        public const string SECURITY_SUPER_TYPE_FIELD = "securitySuperType"; // 债券类型
        public const string TIME_TO_MATURITY_FIELD = "timetomaturity"; // 剩余期限
        public const string TIME_TO_MATURITY_GROUP_FIELD = "timetomaturityGroup"; // 剩余期限分组
        public const string QUOTE_TIME_LENGTH_FIELD = "quoteTimeLength"; //  报价时长
        public const string EXECUTION_COUNT_FIELD = "executionCount"; // 成交笔数
        public const string EXECUTION_QTY_FIELD = "executionQty"; // 成交金额
        public const string AVG_BP_FIELD = "avgBP"; // 平均点差
        public const string QUOTE_BEGIN_TIME_FIELD = "quoteBeginTime"; // 报价开始时间
        public const string QUOTE_END_TIME_FIELD = "quoteEndTime"; // 报价结束时间
        #endregion

        private MmStatisticsView  mmStatisticsView;
        public MmStatisticsView MMStatisticsView
        {
            get { return mmStatisticsView; }
            set
            {
                mmStatisticsView = value;
                OnPropertyChanged();
            }
        }

        private ArrayList bondTypeList = new ArrayList() { "全部", "利率债", "信用债" };
        public ArrayList BondTypeList
        {
            get { return bondTypeList; }
            set
            {
                bondTypeList = value;
                OnPropertyChanged();
            }
        }

        private bool isInited = false;
        public bool IsInited
        {
            get { return isInited; }
            set
            {
                isInited = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get { return new RelayCommand(DoSearchCommand); } }
        public ICommand OutLineExportCommand { get; set; }
        public ICommand DetailExportCommand { get; set; }

        private string searchText = string.Empty;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                if (string.IsNullOrEmpty(value))
                {
                    this.DetailReq.securityId = string.Empty;
                    UpdateDetailRecords();
                }
                OnPropertyChanged();
            }
        }

        private IList bondTypeSelectedItems;
        public IList BondTypeSelectedItems
        {
            get { return bondTypeSelectedItems; }
            set
            {
                if (bondTypeSelectedItems != value)
                {
                    bondTypeSelectedItems = value;
                    UpdateBondType(value);
                    OnPropertyChanged();
                }
            }
        }

        private const int PageSize = CommonConstants.pageCount;

        private PageControl outLinePageControl;
        public PageControl OutLinePageControl
        {
            get { return outLinePageControl; }
            set
            {
                outLinePageControl = value;
                OnPropertyChanged();
            }
        }

        private int outlineTotalCount;
        public int OutlineTotalCount
        {
            get
            {
                return outlineTotalCount;
            }

            set
            {
                outlineTotalCount = value;
                this.OnPropertyChanged();
            }
        }

        private int outlineIndex = 0;
        public int OutlineIndex
        {
            get
            {
                return outlineIndex;
            }

            set
            {
                outlineIndex = value;
            }
        }

        private string outlinePageVisible;

        public string OutlinePageVisible
        {
            get
            {
                return outlinePageVisible;
            }

            set
            {
                outlinePageVisible = value;
                this.OnPropertyChanged();
            }
        }

        private PageControl detailPageControl;
        public PageControl DetailPageControl
        {
            get { return detailPageControl; }
            set
            {
                detailPageControl = value;
                OnPropertyChanged();
            }
        }

        private int detailTotalCount;
        public int DetailTotalCount
        {
            get
            {
                return detailTotalCount;
            }

            set
            {
                detailTotalCount = value;
                this.OnPropertyChanged();
            }
        }

        private int detailIndex = 0;
        public int DetailIndex
        {
            get
            {
                return detailIndex;
            }

            set
            {
                detailIndex = value;
            }
        }

        private string detailPageVisible;

        public string DetailPageVisible
        {
            get
            {
                return detailPageVisible;
            }

            set
            {
                detailPageVisible = value;
                this.OnPropertyChanged();
            }
        }

        private QueryANSOutlineReq outLineReq = new QueryANSOutlineReq();
        public QueryANSOutlineReq OutLineReq
        {
            get { return outLineReq; }
            set
            {
                outLineReq = value;
                OnPropertyChanged();
            }
        }

        private QueryANSDetailReq detailReq = new QueryANSDetailReq();
        public QueryANSDetailReq DetailReq
        {
            get { return detailReq; }
            set
            {
                detailReq = value;
                OnPropertyChanged();
            }
        }

        private object lockObj_OutLine = new object();
        public object LockObj_OutLine
        {
            get
            {
                return lockObj_OutLine;
            }
        }

        private DataView _OutLineDV;
        public DataView OutLineDV
        {
            get
            {
                lock (LockObj_OutLine)
                {
                    if (_OutLineDV == null)
                    {
                        _OutLineDV = new DataTable().DefaultView;
                        _OutLineDV.Table.Columns.Add(DATE_FIELD, typeof(string));
                        _OutLineDV.Table.Columns.Add(BNC_SECURITY_COUNT_FIELD, typeof(int));
                        _OutLineDV.Table.Columns.Add(BNC_SECURITY_TYPE_COUNT_FIELD, typeof(int));
                        _OutLineDV.Table.Columns.Add(BNC_TIME_TO_MATURITY_COUNT_FIELD, typeof(int));
                        _OutLineDV.Table.Columns.Add(BNC_ND_COUNT_FIELD, typeof(int));
                        _OutLineDV.Table.Columns.Add(BNC_NEW_ND_COUNT_FIELD, typeof(int));
                        _OutLineDV.Table.Columns.Add(BCO_SECURITY_COUNT_FIELD, typeof(int));
                        _OutLineDV.Table.Columns.Add(BCO_SECURITY_TYPE_COUNT_FIELD, typeof(int));
                        _OutLineDV.Table.Columns.Add(BCO_ISSUER_COUNT_FIELD, typeof(int));
                        _OutLineDV.Sort = $"{DATE_FIELD} desc";
                        //_OutLineDV.Table.PrimaryKey = new DataColumn[] { _OutLineDV.Table.Columns[BNC_SECURITY_COUNT_FIELD] };
                    }

                    return this._OutLineDV;
                }
            }
        }

        private object lockObj_Detail = new object();
        public object LockObj_Detail { get { return lockObj_Detail; } }

        private DataView _DetailDV;
        public DataView DetailDV
        {
            get
            {
                lock(LockObj_Detail)
                {
                    if (_DetailDV == null)
                    {
                        _DetailDV = new DataTable().DefaultView;
                        _DetailDV.Table.Columns.Add(DATE_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(SYMBOL_TYPE_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(USER_NAME_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(SECURITY_SUPER_TYPE_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(TIME_TO_MATURITY_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(TIME_TO_MATURITY_GROUP_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(QUOTE_TIME_LENGTH_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(EXECUTION_COUNT_FIELD, typeof(int));
                        _DetailDV.Table.Columns.Add(EXECUTION_QTY_FIELD, typeof(decimal));
                        _DetailDV.Table.Columns.Add(AVG_BP_FIELD, typeof(decimal));
                        _DetailDV.Table.Columns.Add(QUOTE_BEGIN_TIME_FIELD, typeof(string));
                        _DetailDV.Table.Columns.Add(QUOTE_END_TIME_FIELD, typeof(string));
                        _DetailDV.Sort = $"{DATE_FIELD} desc";
                        //_DetailDV.Table.PrimaryKey = new DataColumn[] { _DetailDV.Table.Columns[SECURITY_ID_FIELD] };
                    }

                    return this._DetailDV;
                }
            }
        }

        private readonly string today = string.Empty;
        private readonly string week = string.Empty;
        private readonly string month = string.Empty;
        private readonly string quarter = string.Empty;

        public MmStatisticsViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(OutLineDV, LockObj_OutLine);
            BindingOperations.EnableCollectionSynchronization(DetailDV, LockObj_Detail);

            today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            week = DateTimeEx.ServerTime.AddDays(-7).ToString("yyyy-MM-dd");
            month = DateTimeEx.ServerTime.AddDays(-30).ToString("yyyy-MM-dd");
            quarter = DateTimeEx.ServerTime.AddDays(-90).ToString("yyyy-MM-dd");

            this.OutLineExportCommand = new RelayCommand(DoOutLineExportCommand);
            this.DetailExportCommand = new RelayCommand(DoDetailExportCommand);
        }

        public static string ConverSymbolTypeText(string text)
        {
            string ret = string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                switch (text.ToLower())
                {
                    case "bnc":
                        ret = "利率债";
                        break;
                    case "bco":
                        ret = "信用债";
                        break;
                }
            }

            return ret;
        }

        public void RequestOutLineTodayData()
        {
            this.OutLineReq.beginDate = today;
            this.OutLineReq.endDate = today;

            UpdateOutLineRecords();
        }

        public void UpdateOutLineRecords()
        {
            try
            {

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    mmStatisticsView.ShowOutlineMask(true);

                    OutLineReq.pageSize = PageSize;
                    OutLineReq.pageNum = OutlineIndex;

                    QueryANSOutlineReplyPo reply = null;
                    reply = AnsHandler.Instance.QueryANSOutline(this.OutLineReq);

                    if (reply != null && reply.Datas != null)
                    {
                        OutlineTotalCount = reply.count;
                        OutLinePageControl.PageSize = PageSize;
                        OutLinePageControl.RecordCount = reply.count;
                        if (OutlineTotalCount > PageSize)
                        {
                            OutlinePageVisible = "0";
                        }
                        else
                        {
                            OutlinePageVisible = string.Empty;
                        }

                        this.OutLineDV.Table.Rows.Clear();

                        DataRow row;

                        foreach (ANSOutline item in reply.Datas)
                        {
                            if (item != null)
                            {
                                row = this.OutLineDV.Table.NewRow();
                                row[DATE_FIELD] = item.date;
                                row[BNC_SECURITY_COUNT_FIELD] = item.bncSecurityCount;
                                row[BNC_SECURITY_TYPE_COUNT_FIELD] = item.bncSecurityTypeCount;
                                row[BNC_TIME_TO_MATURITY_COUNT_FIELD] = item.bncTimetomaturityCount;
                                row[BNC_ND_COUNT_FIELD] = item.bncNDCount;
                                row[BNC_NEW_ND_COUNT_FIELD] = item.bncNewNDCount;
                                row[BCO_SECURITY_COUNT_FIELD] = item.bcoSecurityCount;
                                row[BCO_SECURITY_TYPE_COUNT_FIELD] = item.bcoSecurityTypeCount;
                                row[BCO_ISSUER_COUNT_FIELD] = item.bcoIssuerCount;
                                this.OutLineDV.Table.Rows.Add(row);

                            }
                        }
                        //this.OnPropertyChanged(nameof(OutLineDV));
                    }

                    mmStatisticsView.ShowOutlineMask(false);

                }));

                
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateOutLineRecords", e);
            }
        }

        public void RequestDetailTodayData()
        {
            this.DetailReq.beginDate = today;
            this.DetailReq.endDate = today;

            UpdateDetailRecords();
        }

        public void UpdateDetailRecords()
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    mmStatisticsView.ShowDetailMask(true);

                    DetailReq.pageSize = PageSize;
                    DetailReq.pageNum = DetailIndex;

                    QueryANSDetailReplyPo reply = AnsHandler.Instance.QueryANSDetail(this.DetailReq);

                    if (reply != null && reply.Datas != null)
                    {
                        DetailTotalCount = reply.count;
                        DetailPageControl.PageSize = PageSize;
                        DetailPageControl.RecordCount = reply.count;
                        if (DetailTotalCount > PageSize) 
                        {
                            DetailPageVisible = "0";
                        }
                        else
                        {
                            DetailPageVisible = string.Empty;
                        }

                        this.DetailDV.Table.Rows.Clear();

                        foreach (ANS item in reply.Datas)
                        {
                            DataRow row;

                            if (item != null)
                            {
                                row = this.DetailDV.Table.NewRow();
                                row[DATE_FIELD] = item.date;
                                row[SECURITY_ID_FIELD] = item.securityId;
                                row[SYMBOL_FIELD] = item.symbol;
                                row[SYMBOL_TYPE_FIELD] = item.symbolType;
                                row[USER_NAME_FIELD] = item.username;
                                row[SECURITY_SUPER_TYPE_FIELD] = MMCommonUtils.GetBondCategoryName(item.securityTypeId);
                                row[TIME_TO_MATURITY_FIELD] = item.timetomaturity;
                                row[TIME_TO_MATURITY_GROUP_FIELD] = item.timetomaturityGroup;
                                row[QUOTE_TIME_LENGTH_FIELD] = convertQuoteTimeLength(item.quoteTimeLength);
                                row[EXECUTION_COUNT_FIELD] = item.executionCount;
                                row[EXECUTION_QTY_FIELD] = item.executionQty;
                                row[AVG_BP_FIELD] = item.avgBP;
                                row[QUOTE_BEGIN_TIME_FIELD] = item.quoteBeginTime;
                                row[QUOTE_END_TIME_FIELD] = item.quoteEndTime;
                                this.DetailDV.Table.Rows.Add(row);

                            }
                        }

                    }

                    mmStatisticsView.ShowDetailMask(false);

                }));
                
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateDetailRecords", e);
            }
        }

        private string convertQuoteTimeLength(long quoteTimeLength) 
        {
            string result = "";
            long minutes = quoteTimeLength % 60;
            long hours = quoteTimeLength / 60;
            result = string.Format("{0}h{1}min",hours,minutes);
            return result;
        }
        public void ProcessOutLineFilterCondition(Dictionary<string,IList> filterdic)
        {
            if (filterdic.ContainsKey("TransactDate"))
            {
                string begin;
                string end;
                GetCustomDate(out begin,out end, filterdic);
                if (!string.IsNullOrEmpty(begin) && !string.IsNullOrEmpty(end))
                {
                    if (this.OutLineReq.beginDate != begin || this.OutLineReq.endDate != end)
                    {
                        this.OutLineReq.beginDate = begin;
                        this.OutLineReq.endDate = end;
                        UpdateOutLineRecords();
                    }                      
                }
            } 
            
        }

        public void ProcessDetailFilterCondition(Dictionary<string, IList> filterdic)
        {
            if (filterdic.ContainsKey("TransactDate"))
            {
                string begin;
                string end;
                GetCustomDate(out begin, out end, filterdic);
                if (!string.IsNullOrEmpty(begin) && !string.IsNullOrEmpty(end))
                {
                    if (this.DetailReq.beginDate != begin || this.DetailReq.endDate!= end)
                    {
                        this.DetailReq.beginDate = begin;
                        this.DetailReq.endDate = end;
                        ResetPageControl("detail");
                        //UpdateDetailRecords();
                    }
                }
            }
        }

        public void UpdateBondType(IList list)
        {
            string bondType = string.Empty;

            if (list == null) return;

            if (list.Count > 0)
            {
                if (list.Count == 1)
                {
                    switch ((list[0] as ListBoxItem).Content.ToString())
                    {
                        case "全部":
                            break;

                        case "利率债":
                            bondType = "bnc";
                            break;

                        case "信用债":
                            bondType = "bco";
                            break;
                    }
                }
            }

            if (DetailReq.securityType != bondType)
            { 
                DetailReq.securityType = bondType;
                ResetPageControl("detail");
                //UpdateDetailRecords(); 
            }

            if (IsInited)
            {
                //MMStatisticsView.bondType_lbx.SelectedItem = list as IList;
                //if (string.IsNullOrEmpty(bondType))
                //{
                //    (MMStatisticsView.bondType_lbx.Items[0] as ListBoxItem).IsSelected = true;
                //    (MMStatisticsView.bondType_lbx.Items[1] as ListBoxItem).IsSelected = false;
                //    (MMStatisticsView.bondType_lbx.Items[2] as ListBoxItem).IsSelected = false;
                //}
                //else if (bondType == "bnc")
                //{
                //    //MMStatisticsView.bondType_lbx.Items[0] as ListBoxItem).IsSelected = false;
                //    (MMStatisticsView.bondType_lbx.Items[1] as ListBoxItem).IsSelected = true;
                //    //(MMStatisticsView.bondType_lbx.Items[2] as ListBoxItem).IsSelected = false;
                //}
                //else if (bondType == "bco")
                //{
                //    (MMStatisticsView.bondType_lbx.Items[0] as ListBoxItem).IsSelected = false;
                //    (MMStatisticsView.bondType_lbx.Items[1] as ListBoxItem).IsSelected = false;
                //    (MMStatisticsView.bondType_lbx.Items[2] as ListBoxItem).IsSelected = true;
                //}
            }
        }

        private void ResetPageControl(string mode)
        {
            if (!IsInited) return;

            switch(mode.ToLower())
            {
                case "outline":
                    OutLinePageControl.Page = 1;
                    OutlineIndex = 0;
                    UpdateOutLineRecords();
                    break;

                case "detail":
                    DetailPageControl.Page = 1;
                    DetailIndex = 0;
                    UpdateDetailRecords();
                    break;
            }
        }

        private string GetMaxTimeSpanText(Dictionary<string, IList> filterdic)
        {
            string maxSpan = string.Empty;

            if (filterdic.ContainsKey("TimeSpan"))
            {
                IList timespanList = filterdic["TimeSpan"];
                if (timespanList.Count > 0)
                {
                    //挑选最大范围的
                    foreach (ListBoxItem item in timespanList)
                    {
                        if (item.Content.ToString() == "最近一季")
                        {
                            maxSpan = item.Content.ToString();
                            break;
                        }

                        if (item.Content.ToString() == "最近一月")
                        {
                            maxSpan = item.Content.ToString();
                        }

                        if (item.Content.ToString() == "最近一周")
                        {
                            if (maxSpan != "最近一月")
                            {
                                maxSpan = item.Content.ToString();
                            }
                        }

                        if (item.Content.ToString() == "今日")
                        {
                            if (maxSpan != "最近一月" && maxSpan != "最近一周")
                            {
                                maxSpan = item.Content.ToString();
                            }
                        }
                    }
                }
            }

            return maxSpan;
        }

        private void GetCustomDate(out string dealBegin,out string dealEnd, Dictionary<string, IList> filterdic)
        {
            dealBegin = string.Empty;
            dealEnd = string.Empty;

            IList dateList = filterdic["TransactDate"];
            if (dateList.Count > 0)
            {
                DateTime begin = Convert.ToDateTime(dateList[0].ToString());
                DateTime end = Convert.ToDateTime(dateList[1].ToString());
                bool status = TimeUtils.Instance.TimeLengthMoreThanMonth(begin, end);
                if (status)
                {
                    //showMessage("界面不支持半年以上订单筛选，请修改日期区间");
                    return;
                }

                dealBegin = begin.ToString("yyyy-MM-dd");
                dealEnd = end.ToString("yyyy-MM-dd");
            }
        }

        private void DoSearchCommand()
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                this.DetailReq.securityId = SearchText;
                ResetPageControl("detail");
                //UpdateDetailRecords();
            }
        }

        private async void ExportToExcel(string mode)
        {
            //创建一个保存文件式的对话框
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
            sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            sfd.RestoreDirectory = true;
            string filename = mode == "outline" ? "做市概要导出" : "做市详情导出";
            sfd.FileName = $"{filename}_{DateTime.Now.ToString("yyyy_MM_dd")}";
            //设置保存的文件的类型，注意过滤器的语法
            sfd.Filter = "Excel Documents|*.xls;*.xlsx";
            sfd.DefaultExt = ".xlsx";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                List<string> errorList = await Sub_ExportExcel(sfd.FileName,mode);

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
                    CustomMessageBox.Warning(errors.ToString(), "批量导出");
                }
                else
                {
                    CustomMessageBox.Info($"成功！", "批量导出");
                }
            }
            else
            {
                CustomMessageBox.Info("取消导出", "批量导出");
            }
        }

        private  void DoOutLineExportCommand()
        {
            ExportToExcel("outline");
        }

        private Task<List<string>> Sub_ExportExcel(string filePath,string mode)
        {
            return Task.Run(() =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);

                    Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                    string errorMsg = string.Empty;
                    DataTable dt = GenerateExcelDataTable(mode,out errorMsg);
                    if (dt!= null && string.IsNullOrEmpty(errorMsg))
                    {
                        dataDic[mode.ToLower() == "outline"?"做市概要":"做市详情"] = dt;
                        helper.DataTableToExcel(dataDic, true);
                    }
                    else
                    {
                        errorlist.Add(errorMsg);
                    }
                    //dataDic[item.groupname] = dt;


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

        private DataTable GenerateExcelDataTable(string mode, out string errorMsg)
        {
            errorMsg = string.Empty;
            DataTable dt = new DataTable();

            if (mode.ToLower() == "outline")
            {
                dt.Columns.Add(MMConstants.AnsOutline_DateField);
                dt.Columns.Add(MMConstants.AnsOutline_BncSecurityCountField);
                dt.Columns.Add(MMConstants.AnsOutline_BncSecurityTypeCountField);
                dt.Columns.Add(MMConstants.AnsOutline_BncTimetomaturityCountField);
                dt.Columns.Add(MMConstants.AnsOutline_BncNDCountField);
                dt.Columns.Add(MMConstants.AnsOutline_BncNewNDCountField);
                dt.Columns.Add(MMConstants.AnsOutline_BcoSecurityCountField);
                dt.Columns.Add(MMConstants.AnsOutline_BcoSecurityTypeCountField);
                dt.Columns.Add(MMConstants.AnsOutline_BcoIssuerCountField);

                QueryANSOutlineReq req = new QueryANSOutlineReq();
                req.beginDate = OutLineReq.beginDate;
                req.endDate = OutLineReq.endDate;
                req.pageSize = 0;
                req.pageNum = 0;
                QueryANSOutlineReplyPo reply = AnsHandler.Instance.QueryANSOutline(req);

                if (req != null && reply.Datas != null)
                {
                    foreach (ANSOutline item in reply.Datas)
                    {
                        DataRow newDR = dt.NewRow();
                        if (item != null)
                        {
                            newDR[MMConstants.AnsOutline_DateField] = item.date;
                            newDR[MMConstants.AnsOutline_BncSecurityCountField] = item.bncSecurityCount;
                            newDR[MMConstants.AnsOutline_BncSecurityTypeCountField] = item.bncSecurityTypeCount;
                            newDR[MMConstants.AnsOutline_BncTimetomaturityCountField] = item.bncTimetomaturityCount;
                            newDR[MMConstants.AnsOutline_BncNDCountField] = item.bncNDCount;
                            newDR[MMConstants.AnsOutline_BncNewNDCountField] = item.bncNewNDCount;
                            newDR[MMConstants.AnsOutline_BcoSecurityCountField] = item.bcoSecurityCount;
                            newDR[MMConstants.AnsOutline_BcoSecurityTypeCountField] = item.bcoSecurityTypeCount;
                            newDR[MMConstants.AnsOutline_BcoIssuerCountField] = item.bcoIssuerCount;
                        }

                        dt.Rows.Add(newDR);
                    }
                }
                else
                {
                    dt = null;
                    errorMsg = string.IsNullOrEmpty(reply.Msg)?"服务器无响应":reply.Msg;
                }
            }
            else if (mode.ToLower() == "detail")
            {
                dt.Columns.Add(MMConstants.AnsDetail_DateField);
                dt.Columns.Add(MMConstants.AnsDetail_SecurityIdField);
                dt.Columns.Add(MMConstants.AnsDetail_SymbolField);
                dt.Columns.Add(MMConstants.AnsDetail_TimetomaturityField);
                dt.Columns.Add(MMConstants.AnsDetail_SymbolTypeField);
                dt.Columns.Add(MMConstants.AnsDetail_TimetomaturityGroupField);
                dt.Columns.Add(MMConstants.AnsDetail_UsernameField);
                dt.Columns.Add(MMConstants.AnsDetail_QuoteBeginTimeField);
                dt.Columns.Add(MMConstants.AnsDetail_QuoteEndTimeField);
                //dt.Columns.Add(MMConstants.AnsDetail_SecuritySuperTypeField);
                dt.Columns.Add(MMConstants.AnsDetail_QuoteTimeLengthField);
                dt.Columns.Add(MMConstants.AnsDetail_AvgBPField);
                dt.Columns.Add(MMConstants.AnsDetail_ExecutionCountField);
                dt.Columns.Add(MMConstants.AnsDetail_ExecutionQtyField);


                QueryANSDetailReq req = new QueryANSDetailReq();
                req.beginDate = DetailReq.beginDate;
                req.endDate = DetailReq.endDate;
                req.securityId = DetailReq.securityId;
                req.securityType = DetailReq.securityType;
                req.pageSize = 0;
                req.pageNum = 0;

                QueryANSDetailReplyPo reply = AnsHandler.Instance.QueryANSDetail(req);

                if (reply != null && reply.Datas != null)
                {
                    foreach (ANS item in reply.Datas)
                    {
                        DataRow newDR = dt.NewRow();
                        if (item != null)
                        {
                            newDR[MMConstants.AnsDetail_DateField] = item.date;
                            newDR[MMConstants.AnsDetail_SecurityIdField] = item.securityId;
                            newDR[MMConstants.AnsDetail_SymbolField] = item.symbol;
                            newDR[MMConstants.AnsDetail_SymbolTypeField] = MMCommonUtils.GetBondCategoryName(item.securityTypeId); 
                            newDR[MMConstants.AnsDetail_UsernameField] = item.username;
                            //newDR[MMConstants.AnsDetail_SecuritySuperTypeField] = item.securitySuperType;
                            newDR[MMConstants.AnsDetail_TimetomaturityField] = item.timetomaturity;
                            newDR[MMConstants.AnsDetail_TimetomaturityGroupField] = item.timetomaturityGroup;
                            newDR[MMConstants.AnsDetail_QuoteTimeLengthField] = convertQuoteTimeLength(item.quoteTimeLength);
                            newDR[MMConstants.AnsDetail_ExecutionCountField] = item.executionCount;
                            newDR[MMConstants.AnsDetail_ExecutionQtyField] = item.executionQty;
                            newDR[MMConstants.AnsDetail_AvgBPField] = item.avgBP;
                            newDR[MMConstants.AnsDetail_QuoteBeginTimeField] = item.quoteBeginTime;
                            newDR[MMConstants.AnsDetail_QuoteEndTimeField] = item.quoteEndTime;
                        }

                        dt.Rows.Add(newDR);
                    }
                }
                else
                {
                    dt = null;
                    errorMsg = string.IsNullOrEmpty(reply.Msg) ? "服务器无响应" : reply.Msg;
                }
            }

            return dt;
        }

        private void DoDetailExportCommand()
        {
            ExportToExcel("detail");
        }
    }
}
