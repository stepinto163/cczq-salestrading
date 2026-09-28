using com.gateway.log;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.MM;
using STC.Common.Model.MMshe;
using STC.Common.Utils;
using STC.Mmshe.Plugin.Utils;
using STC.Mmshe.Plugin.Utils.Data;
using STC.Mmshe.Plugin.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.MmshePlugin.Utils
{
    public class MMExcelExportUtils
    {
        private static MMExcelExportUtils mMExcelExportUtils = new MMExcelExportUtils();
        public static MMExcelExportUtils Instance { get { return mMExcelExportUtils; } }
        private Dictionary<string, string> mmExcelDic = new Dictionary<string, string>();
        private List<string> mmExcelExportTime = new List<string>();
        private MainViewModel ViewModel;
        ConcurrentDictionary<string, MmsheListBoxViewModel> mmListBoxVMdic;
        ConcurrentDictionary<string, TimeTask> taskDic = new ConcurrentDictionary<string, TimeTask>();
        public string firstTime = "";
        public void init(MainViewModel viewModel)
        {
            taskDic.Clear();
            ViewModel = viewModel;
            mmListBoxVMdic = viewModel.getMmsheListBoxViewModel();
            string[] times = LocalEnv.ConfInfo.MMExcelExportDataTime.Split(',');
            foreach (string obj in times)
            {
                if (IsTime(obj))
                {
                    if (string.IsNullOrEmpty(firstTime))
                    {
                        firstTime = obj;
                    }
                    mmExcelExportTime.Add(obj);
                }
            }
            foreach (string obj in mmExcelExportTime)
            {
                if (!taskDic.ContainsKey(obj))
                {
                    TimeTask timeTask = new TimeTask();
                    timeTask.setTaskAtFixedTime(obj);
                    taskDic[obj] = timeTask;
                }
            }
        }
        public MMExcelExportUtils()
        {
            TemplateDT.Rows.Clear();
            TemplateDT.Rows.Add("做市报价导入模板填写说明：");
            TemplateDT.Rows.Add("字段名称", "填写规则");
            TemplateDT.Rows.Add("代码、名称", "应至少填入代码或名称字段中的一个");
            TemplateDT.Rows.Add("净价", "债券净价、到期收益率仅需填入其中一个，支持互推，以净价为准；");
            TemplateDT.Rows.Add("到期", "对于ABS券，特殊债，净价字段必填；");
            TemplateDT.Rows.Add("行权", "对于含权债，若净价为空，则行权收益率或到期收益率必填其中一个。");
            TemplateDT.Rows.Add("发送对象", "必填，\n1.支持同时输入多个发送对象且多个发送对象用‘/’分割。例如：北金所 / 群组X(其中群组X为机构设置的群组名称)\n2.群组的输入规则为：集合名称 - 群组名称。例如：SET1 - 群组1");
            TemplateDT.Rows.Add("冰山券面 ", "非必填");
            TemplateDT.Rows.Add("本方交易员", "非必填，导入时系统自动补填当前导入操作的交易员姓名。");
            TemplateDT.Rows.Add("买入成交、卖出成交", "导入时留空，导出时系统导出报价的成交券面。");
            TemplateDT.Rows.Add("注：支持最多导入300条报价，如果同时输入银行间/北金所/群组X/群组Y，视为四条报价；暂不支持批量导入外币债。", "");
        }
        public static bool IsTime(string StrSource)
        {
            return Regex.IsMatch(StrSource, @"^((20|21|22|23|[0-1]?\d):[0-5]?\d)$");
        }
        
        
        public Task<List<string>> Sub_ExportExcel(string filePath,bool isReadExcel)
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
                        if (!isReadExcel)
                        {
                            File.Delete(filePath);
                        }
                    }


                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    clear();
                    Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                    if (!isReadExcel)
                    {
                        if (MMsheGroupHandler.Instance.CacheSortedGroups != null)
                        {
                            foreach (MMsheGroupInfo item in MMsheGroupHandler.Instance.CacheSortedGroups)
                            {
                                MmsheListBoxViewModel mMListBoxView = mmListBoxVMdic[item.id];
                                DataView BondView = mMListBoxView.BondView;
                                if (BondView != null)
                                {
                                    GenerateExcelDataTable(item.id, BondView);
                                }
                            }
                            DataTable dataDT = DT.Copy();
                            dataDic["做市报价"] = dataDT;
                            DataTable refDT = ReferDT.Copy();
                            dataDic["参考基准价"] = refDT;
                            dataDic["模板说明"] = TemplateDT;
                            Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                            if (result == null) errorlist.Add("无数据导出");
                        }
                        else
                        {
                            errorlist.Add("无数据导出");
                        }
                    }
                    else 
                    {
                        if (!FileIsExists(filePath)) 
                        {
                            LogHelper.Info("file Name is not Exists：" + filePath);
                            return errorlist;
                        }
                        helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                        Dictionary<int, string> dic = helper.ReturnSheetList();
                        DataTable dataDT = null;
                        if (dic.ContainsKey(0))
                        {
                            dataDT = helper.ExcelToDataTable(0);
                            
                        }
                        DataTable referDT = null;
                        if (dic.ContainsKey(1))
                        {
                            referDT = helper.ExcelToDataTable(1);
                           
                        }
                        if (dataDT != null && referDT !=  null)
                        {
                            helper.deleteFile();
                            referDT.Columns[MMsheConstants.SECUTIRY_MMExcelField].ColumnName = "SecurityID";
                            foreach(DataRow row in dataDT.Rows) 
                            {
                                string securityid = (row[MMsheConstants.SECUTIRY_MMExcelField] != null) ? row[MMsheConstants.SECUTIRY_MMExcelField].ToString()+".IB" : "";
                                TBondValuationPo bondValuationPo = getValuation(securityid);
                                string valuationYield = "";
                                string valuationDate = "";
                                if (bondValuationPo != null)
                                {
                                    valuationYield = string.IsNullOrEmpty(bondValuationPo.valuation_yield)?"":bondValuationPo.valuation_yield.Split('|')[0];
                                    valuationDate = bondValuationPo.valuation_date;
                                }                                
                                DataRow[] drs = referDT.Select($"SecurityID = '{securityid.Replace(".IB","")}'");
                                if (drs != null && drs.Length > 0) 
                                {
                                    DataRow dr = drs[0];
                                    dr[MMsheConstants.VALUATION_MMExcelField] = valuationYield;
                                    dr[MMsheConstants.VALUATIONDATE_MMExcelField] = valuationDate;
                                }
                                row[MMsheConstants.BIDYEILD_MMExcelField] = (!string.IsNullOrEmpty(valuationYield)) ?
                                    decimal.Round(
                                        decimal.Add(
                                            CommonUtils.ConvertToDecimal(valuationYield),
                                            decimal.Divide(CommonUtils.ConvertToDecimal(LocalEnv.ConfInfo.MMExcelSpread), 200)), 4, MidpointRounding.AwayFromZero).ToString("#0.0000") : "";
                                row[MMsheConstants.OFRYEILD_MMExcelField] = (!string.IsNullOrEmpty(valuationYield)) ?
                                    decimal.Round(
                                        decimal.Subtract(
                                            CommonUtils.ConvertToDecimal(valuationYield),
                                            decimal.Divide(CommonUtils.ConvertToDecimal(LocalEnv.ConfInfo.MMExcelSpread), 200)), 4, MidpointRounding.AwayFromZero).ToString("#0.0000") : "";
                            }
                            dataDic["做市报价"] = dataDT;
                            referDT.Columns["SecurityID"].ColumnName = MMsheConstants.SECUTIRY_MMExcelField;
                            dataDic["参考基准价"] = referDT;
                        }
                        dataDic["模板说明"] = TemplateDT;
                        Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                        if (result == null) errorlist.Add("无数据导出");
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
        public void clear()
        {
            try
            {
                lock (DT)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        for (int drcount = this.DT.Rows.Count - 1; drcount >= 0; drcount--)
                        {
                            DataRow dr = this.DT.Rows[drcount];
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        this.DT.AcceptChanges();
                    }));
                }
                lock (ReferDT)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        for (int drcount = this.ReferDT.Rows.Count - 1; drcount >= 0; drcount--)
                        {
                            DataRow dr = this.ReferDT.Rows[drcount];
                            dr.BeginEdit();
                            dr.Delete();
                            dr.EndEdit();
                        }
                        this.ReferDT.AcceptChanges();
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        DataTable dt;
        public DataTable DT {
            get
            {
                if (dt == null)
                {
                    dt = new DataTable();
                    dt.Columns.Add(MMsheConstants.SECUTIRY_MMExcelField);
                    dt.Columns.Add(MMsheConstants.SYMBOL_MMExcelField);
                    dt.Columns.Add(MMsheConstants.MAXFLOOR_MMExcelField);
                    dt.Columns.Add(MMsheConstants.SENDTO_MMExcelField);
                    dt.Columns.Add(MMsheConstants.SETTL_MMExcelField);
                    dt.Columns.Add(MMsheConstants.SETTLTYPE_MMExcelField);
                    dt.Columns.Add(MMsheConstants.CLEARINGMETHOD_MMExcelField);
                    dt.Columns.Add(MMsheConstants.BIDNETPRICE_MMExcelField);
                    dt.Columns.Add(MMsheConstants.BIDQTY_MMExcelField);
                    dt.Columns.Add(MMsheConstants.BIDYEILD_MMExcelField);
                    dt.Columns.Add(MMsheConstants.BIDXQ_MMExcelField);
                    dt.Columns.Add(MMsheConstants.BIDINTEREST_MMExcelField);
                    dt.Columns.Add(MMsheConstants.BIDLASTQTY_MMExcelField);
                    dt.Columns.Add(MMsheConstants.OFRNETPRICE_MMExcelField);
                    dt.Columns.Add(MMsheConstants.OFRQTY_MMExcelField);
                    dt.Columns.Add(MMsheConstants.OFRYEILD_MMExcelField);
                    dt.Columns.Add(MMsheConstants.OFRXQ_MMExcelField);
                    dt.Columns.Add(MMsheConstants.OFRINTEREST_MMExcelField);
                    dt.Columns.Add(MMsheConstants.OFRLASTQTY_MMExcelField);
                    dt.Columns.Add(MMsheConstants.PARYTID_MMExcelField);
                }

                return this.dt;
            }
        }

        DataTable referDT;
        public DataTable ReferDT
        {
            get
            {
                if (referDT == null)
                {
                    referDT = new DataTable();
                    referDT.Columns.Add(MMsheConstants.SECUTIRY_MMExcelField);
                    referDT.Columns.Add(MMsheConstants.MYMID_MMExcelField);
                    referDT.Columns.Add(MMsheConstants.CFETSMID_MMExcelField);
                    referDT.Columns.Add(MMsheConstants.VALUATION_MMExcelField);
                    referDT.Columns.Add(MMsheConstants.VALUATIONDATE_MMExcelField);
                    referDT.Columns.Add(MMsheConstants.SPREAD_MMExcelField);
                }

                return this.referDT;
            }
        }
        DataTable templateDT;
        public DataTable TemplateDT
        {
            get
            {
                if (templateDT == null)
                {
                    templateDT = new DataTable();
                    templateDT.Columns.Add("做市报价模板V1.3.6");
                    templateDT.Columns.Add("\t");
                }
                return this.templateDT;
            }
        }

        private DataTable GenerateExcelDataTable(string groupid, DataView BondView)
        {
           

            IOrderedEnumerable<STC.Common.Model.MMshe.MMshe> mmlist = null;
            if (MMsheGroupHandler.Instance.GetMMsByGroupID(groupid) != null)
                mmlist = MMsheGroupHandler.Instance.GetMMsByGroupID(groupid).OrderByDescending(x => x.createtime);

            if (string.IsNullOrEmpty(groupid) == false)
            {
                lock (string.Intern(groupid))
                {
                    if (mmlist != null)
                    {
                        foreach (STC.Common.Model.MMshe.MMshe mm in mmlist)
                        {
                            TBondValuationPo bondValuationPo = getValuation(mm.securityid);
                            string valuationYield = "";
                            string valuationDate = "";
                            if (bondValuationPo != null) 
                            {
                                valuationYield = string.IsNullOrEmpty(bondValuationPo.valuation_yield)?"":bondValuationPo.valuation_yield.Split('|')[0];
                                valuationDate = bondValuationPo.valuation_date;
                            }
                            DataRow dr = BondView.Table.Rows.Find(new object[] { mm.securityid });
                            MMsheOrderParamReq mmOrderParamReq = mm.Params;
                            DataRow newDR = DT.NewRow();
                            DataRow referDR = ReferDT.NewRow();
                            if (mmOrderParamReq != null && dr != null)
                            {
                                newDR[MMsheConstants.SECUTIRY_MMExcelField] = mm.securityid.Replace(".IB","");
                                newDR[MMsheConstants.SYMBOL_MMExcelField] = mm.symbol;
                                newDR[MMsheConstants.MAXFLOOR_MMExcelField] = !string.IsNullOrEmpty(mmOrderParamReq.maxFloor) ?
                                    decimal.Multiply(CommonUtils.ConvertToDecimal(mmOrderParamReq.maxFloor),1000).ToString():"";
                                newDR[MMsheConstants.SENDTO_MMExcelField] = "银行间";
                                newDR[MMsheConstants.SETTL_MMExcelField] = "2".Equals(mm.settltype)?"+1":"+0";
                                newDR[MMsheConstants.SETTLTYPE_MMExcelField] = "全额清算";
                                newDR[MMsheConstants.CLEARINGMETHOD_MMExcelField] = "券款对付";
                                newDR[MMsheConstants.BIDQTY_MMExcelField] = "";
                                newDR[MMsheConstants.BIDQTY_MMExcelField] = decimal.Round(decimal.Multiply(CommonUtils.ConvertToDecimal(mmOrderParamReq.bidBaseQty),1000), 2).ToString("#0.00");
                                newDR[MMsheConstants.BIDYEILD_MMExcelField] = (!string.IsNullOrEmpty(valuationYield)) ?
                                    decimal.Round(
                                        decimal.Add(
                                            CommonUtils.ConvertToDecimal(valuationYield),
                                            decimal.Divide(CommonUtils.ConvertToDecimal(LocalEnv.ConfInfo.MMExcelSpread), 200)), 4,MidpointRounding.AwayFromZero).ToString("#0.0000") : "";
                                newDR[MMsheConstants.BIDXQ_MMExcelField] = "";
                                newDR[MMsheConstants.BIDINTEREST_MMExcelField] = "";
                                newDR[MMsheConstants.BIDLASTQTY_MMExcelField] = "";
                                newDR[MMsheConstants.OFRNETPRICE_MMExcelField] = "";
                                newDR[MMsheConstants.OFRQTY_MMExcelField] = decimal.Round(decimal.Multiply(CommonUtils.ConvertToDecimal(mmOrderParamReq.ofrBaseQty),1000), 2).ToString("#0.00");
                                newDR[MMsheConstants.OFRYEILD_MMExcelField] = (!string.IsNullOrEmpty(valuationYield)) ?
                                    decimal.Round(
                                        decimal.Subtract(
                                            CommonUtils.ConvertToDecimal(valuationYield), 
                                            decimal.Divide(CommonUtils.ConvertToDecimal(LocalEnv.ConfInfo.MMExcelSpread), 200)), 4, MidpointRounding.AwayFromZero).ToString("#0.0000") :"";
                                newDR[MMsheConstants.OFRXQ_MMExcelField] = "";
                                newDR[MMsheConstants.OFRINTEREST_MMExcelField] = "";
                                newDR[MMsheConstants.OFRLASTQTY_MMExcelField] = "";
                                newDR[MMsheConstants.PARYTID_MMExcelField] = "";
                                if (dr[MmsheListBoxViewModel.MY_BID_FIELD] != null && dr[MmsheListBoxViewModel.MY_OFR_FIELD] != null 
                                    && CommonUtils.ConvertToDecimal(dr[MmsheListBoxViewModel.MY_BID_FIELD].ToString()).CompareTo(100000000) != 0
                                    && CommonUtils.ConvertToDecimal(dr[MmsheListBoxViewModel.MY_OFR_FIELD].ToString()).CompareTo(100000000) != 0)
                                {
                                    referDR[MMsheConstants.MYMID_MMExcelField] = decimal.Round(decimal.Divide(
                                        decimal.Add(CommonUtils.ConvertToDecimal(dr[MmsheListBoxViewModel.MY_BID_FIELD].ToString()),
                                        CommonUtils.ConvertToDecimal(dr[MmsheListBoxViewModel.MY_OFR_FIELD].ToString())), 2), 4, MidpointRounding.AwayFromZero).ToString("#0.0000");
                                }
                                else 
                                {
                                    referDR[MMsheConstants.MYMID_MMExcelField] = "";
                                }
                                if (dr[MmsheListBoxViewModel.BEST_CFETS_BID_FIELD] != null && dr[MmsheListBoxViewModel.BEST_CFETS_OFR_FIELD] != null
                                    && CommonUtils.ConvertToDecimal(dr[MmsheListBoxViewModel.BEST_CFETS_BID_FIELD].ToString()).CompareTo(100000000) != 0
                                    && CommonUtils.ConvertToDecimal(dr[MmsheListBoxViewModel.BEST_CFETS_OFR_FIELD].ToString()).CompareTo(100000000) != 0)
                                {
                                    referDR[MMsheConstants.CFETSMID_MMExcelField] = decimal.Round(decimal.Divide(
                                        decimal.Add(CommonUtils.ConvertToDecimal(dr[MmsheListBoxViewModel.BEST_CFETS_BID_FIELD].ToString()),
                                        CommonUtils.ConvertToDecimal(dr[MmsheListBoxViewModel.BEST_CFETS_OFR_FIELD].ToString())), 2), 4, MidpointRounding.AwayFromZero).ToString("#0.0000");
                                }
                                else 
                                {
                                    referDR[MMsheConstants.CFETSMID_MMExcelField] = "";
                                }
                                referDR[MMsheConstants.VALUATION_MMExcelField] = valuationYield;
                                referDR[MMsheConstants.VALUATIONDATE_MMExcelField] = valuationDate;
                                referDR[MMsheConstants.SPREAD_MMExcelField] = CommonUtils.ConvertToDecimal(LocalEnv.ConfInfo.MMExcelSpread) + "%";
                                referDR[MMsheConstants.SECUTIRY_MMExcelField] = mm.securityid.Replace(".IB", "");


                            }
                            DT.Rows.Add(newDR);
                            ReferDT.Rows.Add(referDR);
                        }
                    }
                }
            }
            return dt;
        }
        private TBondValuationPo getValuation(string securityID)
        {
            TBondValuationPo bondValuationPo = null;
            TBondValuationReq req = new TBondValuationReq();
            req.SecurityID = securityID;
            var reply = BaseHandler.Instance.SearchChianSecurity(req);
            if (reply != null && "0000".Equals(reply.Code) && reply.Datas.Count > 0)
            {
                bondValuationPo = reply.Datas[0];
            }
            return bondValuationPo;
        }
        public static bool FileIsExists(string filePath)
        {
            return FileHelper.FileIsExists(filePath);
        }
    }

    public class TimeTask 
    {
        private ConcurrentDictionary<string, Timer> timerDic = new ConcurrentDictionary<string, Timer>();
        private string currentTime = "";
        public void setTaskAtFixedTime(string obj)
        {
            currentTime = obj;
            string[] times = obj.Split(':');
            if (times.Length != 2)
            {
                return;
            }
            int hours = int.Parse(times[0].ToString());
            int min = int.Parse(times[1].ToString());
            DateTime now = DateTime.Now;
            DateTime oneOClock = DateTime.Today.AddHours(hours).AddMinutes(min);
            if (now > oneOClock)
            {
                oneOClock = oneOClock.AddDays(1.0);
            }
            int msUntilFour = (int)((oneOClock - now).TotalMilliseconds);
            if (!timerDic.ContainsKey(obj))
            {
                var t = new System.Threading.Timer(doAt1AM);
                t.Change(msUntilFour, Timeout.Infinite);
                timerDic[obj] = t;
            }
            else 
            {
                var t = timerDic[obj];
                t.Change(msUntilFour, Timeout.Infinite);
                timerDic[obj] = t;
            }
        }
        private static string mmDataName = "CFETS做市报价.xlsx";
        private string getFilePath() 
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @"data\" + @"\CFETS做市报价.xlsx";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.DataPath))
                filePath = LocalEnv.ConfInfo.DataPath + mmDataName;
            return filePath;
        }

        //要执行的任务
        private void doAt1AM(object state)
        {
            DateTime now = DateTime.Now;
            string time = now.ToString("HH:mm");
            //执行功能...
            string filePath = getFilePath();
            CreateDirectory(filePath);
            bool isReadExcel = false;
            if (time.Contains(MMExcelExportUtils.Instance.firstTime))
            {
                isReadExcel = false;
            }
            else 
            {
                isReadExcel = true;
            }
            MMExcelExportUtils.Instance.Sub_ExportExcel(filePath, isReadExcel);
            //再次设定
            setTaskAtFixedTime(currentTime);
        }

        private static bool ExistINIFile(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void CreateDirectory(string filePath)
        {
            if (!ExistINIFile(filePath))
            {
                FileHelper.CreateDirectory(filePath);               
            }
        }
    }
}
