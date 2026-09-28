using com.gateway.log;
using STC.Common.Handler.PT;
using STC.Common.Model;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
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

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// CostSettingWin.xaml 的交互逻辑
    /// </summary>
    public partial class CostSettingWin : WindowBase
    {

        /// <summary>
        /// 静态对象
        /// </summary>
        private static CostSettingWin editBillWindow = null;
        /// <summary>
        /// 费代付机构集合缓存
        /// </summary>
        private List<FeeSettingItem> paymentOrg = new List<FeeSettingItem>();

        private string _instId;

        private string _instName;

        DateTime _dateTime;
        public CostSettingWin(string instId, string instName)
        {
            InitializeComponent();
            this.Title = "费用设置 -" + instName;
            _instId = instId;
            _instName = instName;
            QueryCostSetting(instId);
            SetControlStaus();
        }

        public static CostSettingWin CreateInstrance(string instId, string instName, out bool isNewWin)
        {
            isNewWin = false;
            if (editBillWindow == null)
            {
                editBillWindow = new CostSettingWin(instId, instName);
                isNewWin = true;
            }
            else
            {
                editBillWindow.ReshEditBillWindow(instId, instName);
                isNewWin = false;
            }
            //数据初始化后事件才能响应
            //editBillWindow.textChangeFlag = true;
            return editBillWindow;
        }

        private void ReshEditBillWindow(string instId, string instName)
        {
            this.Title = "费用设置 -" + instName;
            _instId = instId;
            _instName = instName;
            QueryCostSetting(instId);
            SetControlStaus();
        }
        /// <summary>
        /// 插叙费用设置
        /// </summary>
        private void QueryCostSetting(string instId)
        {

            FeeSettingRequest request = new FeeSettingRequest();
            request.instId = instId;
            request.action = "0";
            FeeSettingReply paymentInstReplyPo = GRHandler.Instance.OperateInstProxyFeeSetting(request);
            if (paymentInstReplyPo != null && "0000".Equals(paymentInstReplyPo.Code) && paymentInstReplyPo.Datas != null && paymentInstReplyPo.Datas.Any())
            {
                DateTime dateTime = DateTime.Now;
                _dateTime = dateTime;
                foreach (FeeSettingItem item in paymentInstReplyPo.Datas)
                {
                    //item.KeyId = CreateOrderID();
                    item.OrderDateTime = GetOrderDateTime(item.termStart, dateTime);
                    item.OrderEndDateTime = GetOrderDateTime(item.termEnd, dateTime);
                }
                paymentOrg = paymentInstReplyPo.Datas.ToList();
                int rowData = paymentOrg.Count;
                if (rowData < 5)
                {
                    //添加数据行
                    for (int i = 0; i < 5 - rowData; i++)
                    {
                        FeeSettingItem feeSettingInfo = new FeeSettingItem() { IsEmptyData = true, Id = CreateOrderID() };
                        paymentOrg.Add(feeSettingInfo);
                    }
                }
                boxGroupList.DataContext = paymentOrg;
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    FeeSettingItem feeSettingInfo = new FeeSettingItem() { IsEmptyData = true, Id = CreateOrderID() };
                    paymentOrg.Add(feeSettingInfo);
                }
                boxGroupList.DataContext = paymentOrg;
            }
        }



        private DateTime GetOrderDateTime(string term, DateTime dateTime)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                string strTerm = term.ToUpper();
                if (strTerm.EndsWith("D"))
                {
                    strTerm = strTerm.Replace("D", "").Replace("Y", "");
                    dateTime = dateTime.AddDays(Convert.ToInt32(strTerm));

                }
                else if (strTerm.EndsWith("Y"))
                {
                    strTerm = strTerm.Replace("D", "").Replace("Y", "");
                    dateTime = dateTime.AddYears(Convert.ToInt32(strTerm));
                }
                else
                {
                    strTerm = strTerm.Replace("D", "").Replace("Y", "");
                    dateTime = dateTime.AddYears(Convert.ToInt32(strTerm));
                }
            }

            return dateTime;

        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// 提交前校验
        /// </summary>
        /// <param name="fees"></param>
        /// <returns></returns>
        private int CheckEments(List<FeeSettingItem> fees)
        {
            int checkedk = 0;
            List<FeeSettingItem> feeSettings = fees.ToList();
            DateTime? termStartStr = null;
            DateTime? termEndStr = null;
            foreach (FeeSettingItem item in feeSettings)
            {
                if (!string.IsNullOrWhiteSpace(item.termStart))
                {
                    string newtermStart = item.termStart.ToUpper();
                    if (newtermStart.IndexOf("D") != -1 && newtermStart.IndexOf("Y") != -1)
                    {
                        return 3;
                    }
                    else
                    {
                        if (newtermStart.IndexOf("D") != -1)
                        {
                            string strTemp = newtermStart.Substring(0, newtermStart.Length - 1);
                            if (!IsNumeric(strTemp))
                            {
                                return 3;
                            }
                        }
                        else
                        {
                            if (newtermStart.IndexOf("Y") != -1)
                            {
                                string strTemp = newtermStart.Substring(0, newtermStart.Length - 1);
                                if (!IsNumeric(strTemp))
                                {
                                    return 3;
                                }
                            }
                        }
                    }
                    newtermStart = newtermStart.Replace("D", "").Replace("Y", "");
                    if (!IsNumeric(newtermStart))
                    {
                        return 3;
                    }
                    else
                    {
                        item.OrderDateTime = GetOrderDateTime(item.termStart, _dateTime);
                    }
                }

                if (!string.IsNullOrWhiteSpace(item.termEnd))
                {

                    string newtermStart = item.termEnd.ToUpper();
                    if (newtermStart.IndexOf("D") != -1 && newtermStart.IndexOf("Y") != -1)
                    {
                        return 3;
                    }
                    else
                    {
                        if (newtermStart.IndexOf("D") != -1)
                        {
                            string strTemp = newtermStart.Substring(0, newtermStart.Length - 1);
                            if (!IsNumeric(strTemp))
                            {
                                return 3;
                            }
                        }
                        else
                        {
                            if (newtermStart.IndexOf("Y") != -1)
                            {
                                string strTemp = newtermStart.Substring(0, newtermStart.Length - 1);
                                if (!IsNumeric(strTemp))
                                {
                                    return 3;
                                }
                            }
                        }
                    }
                    newtermStart = newtermStart.Replace("D", "").Replace("Y", "");
                    if (!IsNumeric(newtermStart))
                    {
                        return 3;
                    }
                    else
                    {
                        item.OrderEndDateTime = GetOrderDateTime(item.termEnd, _dateTime);
                    }
                }

                if (!string.IsNullOrWhiteSpace(item.fee))
                {
                    if (!IsNumeric(item.fee))
                    {
                        return 4;
                    }
                }
            }
            feeSettings = feeSettings.OrderBy(_ => _.OrderDateTime).ToList();
            int i = 0;
            //空开始期限个数
            int startEmptyCount = 0;
            //空结束期限个数
            //空结束期限个数
            int endEmptyCount = 0;
            //空开始期值
            DateTime? startEmptyStr = null;
            //空结束期限值
            DateTime? endEmptyStr = null;
            //空开始期值
            DateTime? maxDateTime = null;
            //空结束期限值
            DateTime? minDateTime = null;

            foreach (FeeSettingItem item in feeSettings)
            {
                if (!string.IsNullOrWhiteSpace(item.termStart))
                {
                    string newtermStart = item.termStart.ToUpper();
                    if (newtermStart.IndexOf("D") != -1 && newtermStart.IndexOf("Y") != -1)
                    {
                        return 3;
                    }
                    else
                    {
                        if (newtermStart.IndexOf("D") != -1)
                        {
                            string strTemp = newtermStart.Substring(0, newtermStart.Length - 1);
                            if (!IsNumeric(strTemp))
                            {
                                return 3;
                            }
                        }
                        else
                        {
                            if (newtermStart.IndexOf("Y") != -1)
                            {
                                string strTemp = newtermStart.Substring(0, newtermStart.Length - 1);
                                if (!IsNumeric(strTemp))
                                {
                                    return 3;
                                }
                            }
                        }
                    }
                    newtermStart = newtermStart.Replace("D", "").Replace("Y", "");
                    if (!IsNumeric(newtermStart))
                    {
                        return 3;
                    }
                    else
                    {
                        item.OrderDateTime = GetOrderDateTime(item.termStart, _dateTime);
                        if (maxDateTime == null)
                        {
                            maxDateTime = item.OrderDateTime;
                        }
                        else
                        {
                            if (maxDateTime < item.OrderDateTime)
                            {
                                maxDateTime = item.OrderDateTime;
                            }
                        }
                        if (minDateTime == null)
                        {
                            minDateTime = item.OrderDateTime;
                        }
                        else
                        {
                            if (minDateTime > item.OrderDateTime)
                            {
                                minDateTime = item.OrderDateTime;
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(item.termEnd))
                {

                    string newtermStart = item.termEnd.ToUpper();
                    if (newtermStart.IndexOf("D") != -1 && newtermStart.IndexOf("Y") != -1)
                    {
                        return 3;
                    }
                    else
                    {
                        if (newtermStart.IndexOf("D") != -1)
                        {
                            string strTemp = newtermStart.Substring(0, newtermStart.Length - 1);
                            if (!IsNumeric(strTemp))
                            {
                                return 3;
                            }
                        }
                        else
                        {
                            if (newtermStart.IndexOf("Y") != -1)
                            {
                                string strTemp = newtermStart.Substring(0, newtermStart.Length - 1);
                                if (!IsNumeric(strTemp))
                                {
                                    return 3;
                                }
                            }
                        }
                    }
                    newtermStart = newtermStart.Replace("D", "").Replace("Y", "");
                    if (!IsNumeric(newtermStart))
                    {
                        return 3;
                    }
                    else
                    {
                        item.OrderEndDateTime = GetOrderDateTime(item.termEnd, _dateTime);
                        if (maxDateTime == null)
                        {
                            maxDateTime = item.OrderEndDateTime;
                        }
                        else
                        {
                            if (maxDateTime < item.OrderEndDateTime)
                            {
                                maxDateTime = item.OrderEndDateTime;
                            }
                        }
                        if (minDateTime == null)
                        {
                            minDateTime = item.OrderEndDateTime;
                        }
                        else
                        {
                            if (minDateTime > item.OrderEndDateTime)
                            {
                                minDateTime = item.OrderEndDateTime;
                            }
                        }

                    }
                }

                if (!string.IsNullOrWhiteSpace(item.fee))
                {
                    if (!IsNumeric(item.fee))
                    {
                        return 4;
                    }
                }

                if (string.IsNullOrWhiteSpace(item.termStart) &&
                    !string.IsNullOrWhiteSpace(item.termEnd))
                {
                    startEmptyCount++;
                    if (startEmptyCount > 1)
                    {
                        return 1;
                    }
                    endEmptyStr = item.OrderEndDateTime;
                }

                if (!string.IsNullOrWhiteSpace(item.termStart) &&
                    string.IsNullOrWhiteSpace(item.termEnd))
                {
                    endEmptyCount++;
                    if (endEmptyCount > 1)
                    {
                        return 1;
                    }
                    startEmptyStr = item.OrderDateTime;
                }

                if (!string.IsNullOrWhiteSpace(item.termStart) &&
                    !string.IsNullOrWhiteSpace(item.termEnd))
                {
                    i++;

                    if (termStartStr == null)
                    {
                        termStartStr = item.OrderDateTime;
                    }
                    if (termEndStr == null)
                    {
                        termEndStr = item.OrderEndDateTime;
                    }
                    if (i == 1)
                    {
                        termStartStr = item.OrderDateTime;
                        termEndStr = item.OrderEndDateTime;
                        if (termStartStr > termEndStr)
                        {
                            return 5;
                        }
                    }
                    else
                    {
                        if (termStartStr != null && termEndStr != null)
                        {
                            if (item.OrderDateTime > item.OrderEndDateTime)
                            {
                                return 5;
                            }
                            if (termStartStr <= termEndStr
                                && termEndStr <= item.OrderDateTime
                                && item.OrderDateTime <= item.OrderEndDateTime)
                            {
                                if (termStartStr == item.OrderDateTime
                                    && termEndStr == item.OrderEndDateTime)
                                {
                                    return 2;
                                }
                                else
                                {
                                    termStartStr = item.OrderDateTime;
                                    termEndStr = item.OrderEndDateTime;
                                }
                            }
                            else
                            {
                                return 1;
                            }
                        }
                    }
                }
            }
            if (startEmptyCount > 1 || endEmptyCount > 1)
            {
                return 1;
            }
            else
            {
                if (startEmptyCount == 1 && endEmptyStr > minDateTime)
                {

                    return 1;
                }
                if (endEmptyCount == 1 && startEmptyStr < maxDateTime)
                {
                    return 1;
                }
            }

            return checkedk;
        }


        private bool IsNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9]\d*$");
            return reg1.IsMatch(str);
        }
        //quren
        private void btnOkClose_Click(object sender, RoutedEventArgs e)
        {

            List<FeeSettingItem> feeSettingInfos = boxGroupList.DataContext as List<FeeSettingItem>;
            List<FeeSettingItem> fees = new List<FeeSettingItem>();
            if (feeSettingInfos != null && feeSettingInfos.Count > 0)
            {
                //校验
                int result = CheckEments(feeSettingInfos);
                if (result == 1)
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "期限设置有重合部分";
                    warn.ShowDialog();
                    return;
                }
                else if (result == 2)
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "期限设置有重复";
                    warn.ShowDialog();
                    return;
                }
                else if (result == 3)
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "期限格式错误(期限格式为整数，或整数以y或d结尾，不区分大小写)";
                    warn.ShowDialog();
                    return;
                }
                else if (result == 4)
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "费用只能为整数";
                    warn.ShowDialog();
                    return;
                }
                else if (result == 5)
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "期限左区间要小于等于右区间";
                    warn.ShowDialog();
                    return;
                }
                FeeSettingRequest feeSettingRequestPo = new FeeSettingRequest();
                feeSettingRequestPo.action = "2";
                feeSettingRequestPo.instId = _instId;
                feeSettingRequestPo.instName = _instName;
                foreach (FeeSettingItem item in feeSettingInfos)
                {
                    FeeSettingItem settingInfo = new FeeSettingItem();
                    if (string.IsNullOrWhiteSpace(item.termStart)
                        && string.IsNullOrWhiteSpace(item.termEnd))
                    {
                        continue;
                    }
                    settingInfo.termStart = item.termStart;
                    settingInfo.termEnd = item.termEnd;
                    settingInfo.fee = item.fee;
                    settingInfo.createby = LocalEnv.UserInfo.UserName;
                    fees.Add(settingInfo);
                }
                feeSettingRequestPo.feeList = fees;
                BaseReplyPo reply = GRHandler.Instance.OperateInstProxyFeeSetting(feeSettingRequestPo);
                if (reply == null || reply.Code != "0000")
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    Window win = Window.GetWindow(this);
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    warn.tbMsg.Text = "设置费用失败！";
                    warn.ShowDialog();
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                SysWarnGUI warn = new SysWarnGUI();
                Window win = Window.GetWindow(this);
                if (win != null)
                    warn.Owner = win;
                warn.tbTitle.Text = "提示";
                warn.tbMsg.Text = "请至少保留一个设置！";
                warn.ShowDialog();
            }
        }

        private void btn_CostSetting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCloseWin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //关闭时清空静态对象
            editBillWindow = null;
        }



        private string CreateOrderID()
        {
            string orderId = string.Empty;
            try
            {
                orderId = Guid.NewGuid().ToString("N");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return orderId;
        }


        /// <summary>
        /// 设置控件状态
        /// </summary>
        private void SetControlStaus()
        {
            List<FeeSettingItem> feeSettingInfos = boxGroupList.DataContext as List<FeeSettingItem>;
            if (feeSettingInfos != null && feeSettingInfos.Count > 0)
            {

                if (feeSettingInfos.Count >= 10)
                {
                    btn_Add.IsEnabled = false;
                }
                else
                {
                    btn_Add.IsEnabled = true;
                }
                if (feeSettingInfos.Count <= 1)
                {
                    feeSettingInfos[0].IsDel = false;
                }
                else
                {
                    foreach (var item in feeSettingInfos)
                    {
                        item.IsDel = true;
                    }
                }
            }
        }

        private void btn_delete1_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }

            FeeSettingItem feeSetting = button.DataContext as FeeSettingItem;
            if (feeSetting == null)
            {
                return;
            }
            if (paymentOrg.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(feeSetting.Id))
                {
                    paymentOrg.Remove(paymentOrg.Where(_ => _.Id == feeSetting.Id).FirstOrDefault());
                }
            }
            boxGroupList.DataContext = paymentOrg.ToList();
            SetControlStaus();
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {


        }

        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {

            FeeSettingItem feeSettingInfo = new FeeSettingItem() { IsEmptyData = true, Id = CreateOrderID() };
            paymentOrg.Add(feeSettingInfo);
            boxGroupList.DataContext = paymentOrg.ToList();
            SetControlStaus();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
