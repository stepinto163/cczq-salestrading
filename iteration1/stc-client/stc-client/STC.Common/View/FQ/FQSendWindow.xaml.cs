using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Data.DataModel;
using STC.Common.Handler.Admin;
using STC.Common.Handler.FinQ;
using STC.Common.Model.Admin;
using STC.Common.Model.CRM;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace STC.Common.View.FQ
{
    /// <summary>
    /// FQSendWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FQSendWindow : WindowBase
    {
        public static string CopyStr = string.Empty;
        public static string CopyStr_YC = string.Empty;
        private bool _IsCopy = false;

        private string original_text = string.Empty;
        private List<CopyElement> elementList = new List<CopyElement>();

        public FQSendWindow(bool isCopy = false)
        {
            _IsCopy = isCopy;

            InitializeComponent();

            if (!isCopy && LocalEnv.ConfInfo.PushQQGroup_PushView == "1")
            {
                qq_part.Visibility = Visibility.Collapsed;
                yc_qq_part.Visibility = Visibility.Visible;
                option_part.Visibility = Visibility.Collapsed;
                yc_option_part.Visibility = Visibility.Visible;

                settingBtn.Visibility = Visibility.Collapsed;
            }

            LoadParam();

            if (isCopy)
            {
                this.Title = "复制";
                TraderTB.Visibility = Visibility.Collapsed;
                TraderCbb.Visibility = Visibility.Collapsed;
                SendBtn.Visibility = Visibility.Collapsed;
                this.CopyBtn.Visibility = Visibility.Visible;
            }

            if (LocalEnv.ConfInfo.IsPAYL)
            {
                option_part.Visibility = Visibility.Collapsed;
            }

            if (!isCopy && LocalEnv.ConfInfo.PushQQGroup_PushView == "1")
            {
                LoadCrm_YC();
            }
            else
            {
                LoadCrm();
            }
        }

        private void LoadCrm_YC()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                SystemParameterReq req_sale = new SystemParameterReq();
                req_sale.ParamKey = "YC_PUSH_QQ";
                req_sale.Username = "*";
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req_sale);
                if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && JsonSplit.IsJson(reply.Datas.tbookSystemParameter.param_value))
                {
                    List<YCQQ> qqList = JsonConvert.DeserializeObject<List<YCQQ>>(reply.Datas.tbookSystemParameter.param_value);

                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (qqList != null && qqList.Count > 0)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            foreach (YCQQ ycqq in qqList)
                            {
                                dic[ycqq.qq] = ycqq.name;
                            }
                            this.TraderCbb_YC.ItemsSource = dic;
                        }
                    }));
                }
            });
        }

        private void LoadCrm()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            List<CrmContact> contactList = CommonUtils.GetContactInfoPo(null, null);
            foreach(CrmContact cc in contactList)
            {
                dic[cc.idbkey] = $"{cc.namecn} {cc.instname}";
            }
            TraderCbb.ItemsSource = dic;
        }

        private string bp1 = "0.25";
        private string bp2 = "0.5";
        private string bp3 = "1";
        private string context = "今日好价，欢迎咨询";
        private string tabStr = "    ";

        private void LoadParam()
        {
            if (CommonUtils.CopyDic.ContainsKey("BP"))
            {
                foreach(CopyColumnItem cci in CommonUtils.CopyDic["BP"])
                {
                    if (cci.Name == "BP1")
                    {
                        bp1 = cci.Value;
                    }
                    else if (cci.Name == "BP2")
                    {
                        bp2 = cci.Value;
                    }
                    else if (cci.Name == "BP3")
                    {
                        bp3 = cci.Value;
                    }
                }

            }
            if (CommonUtils.CopyDic.ContainsKey("SUGGEST"))
            {
                foreach(CopyColumnItem cci in CommonUtils.CopyDic["SUGGEST"])
                {
                    if (cci.Name == "TEXT")
                    {
                        context = cci.Value;
                    }
                }
            }

            if (CommonUtils.CopyDic.ContainsKey("TAB"))
            {
                foreach(CopyColumnItem cci in CommonUtils.CopyDic["TAB"])
                {
                    if (cci.Name == "空格" && cci.IsChecked)
                    {
                        tabStr = " ";
                    }
                    else if (cci.Name == "Tab制表符" && cci.IsChecked)
                    {
                        tabStr = "\t";
                    }
                    else
                    {
                        tabStr = "    ";
                    }
                }
            }

            this.bp1_bid.Content = $"+{bp1}bp";
            this.bp2_bid.Content = $"+{bp2}bp";
            this.bp3_bid.Content = $"+{bp3}bp";

            this.bp1_ofr.Content = $"-{bp1}bp";
            this.bp2_ofr.Content = $"-{bp2}bp";
            this.bp3_ofr.Content = $"-{bp3}bp";

            if (!_IsCopy && LocalEnv.ConfInfo.PushQQGroup_PushView == "1")
            {
                tabStr = " ";
            }
        }

        private void FQSendWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string tmp = CopyStr; 
            if (!_IsCopy && LocalEnv.ConfInfo.PushQQGroup_PushView == "1")
            {
                tmp = CopyStr_YC;
            }
            parseTxt(tmp);
            //this.contentTxt.Text = context + "\r\n" + tmp;
            FeedRichTextBox();
            original_text = tmp;
        }

         

        private void parseTxt(string txt)
        {
            elementList.Clear();
            string temp = txt.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] lineArr = temp.Split('\n');
            if (lineArr != null && lineArr.Length > 0)
            {
                foreach(string lineTxt in lineArr)
                {
                    string[] element = lineTxt.Split(tabStr.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    CopyElement ce = new CopyElement();
                    StringBuilder startPart = new StringBuilder();
                    StringBuilder endPart = new StringBuilder();

                    foreach (string s in element)
                    {
                        if (s.Contains("/") && !s.Contains("主/债") && !s.Contains("休"))
                        {
                            string[] leftAndRight = s.Split('/');
                            if (string.IsNullOrEmpty(ce.LeftPrice) && string.IsNullOrEmpty(ce.RightPrice))
                            {
                                ce.LeftPrice = leftAndRight[0];
                                ce.RightPrice = leftAndRight[1];
                            }
                            else if (string.IsNullOrEmpty(ce.LeftQty) && string.IsNullOrEmpty(ce.RightQty))
                            {
                                ce.LeftQty = leftAndRight[0];
                                ce.RightQty = leftAndRight[1];
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ce.LeftPrice) && string.IsNullOrEmpty(ce.RightPrice)
                                && string.IsNullOrEmpty(ce.LeftQty) && string.IsNullOrEmpty(ce.RightQty))
                            {
                                if (startPart.Length > 0)
                                {
                                    startPart.Append(tabStr);
                                }
                                startPart.Append(s);
                            }
                            else
                            {
                                if (endPart.Length > 0)
                                {
                                    endPart.Append(tabStr);
                                }
                                endPart.Append(s);
                            }
                        }
                    }
                    ce.StartPart = startPart.ToString();
                    ce.EndPart = endPart.ToString();
                    elementList.Add(ce);
                }
            }
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LocalEnv.ConfInfo.PushQQGroup_PushView == "1")
            {
                PushMsg_YC();
            }
            else
            {
                PushMsg();
            }
            this.Close();
        }

        private void PushMsg_YC()
        {
            StringBuilder sb = new StringBuilder();
            //string copyTxt = this.contentTxt.Text;
            string copyTxt = GetContent_YC();

            if (string.IsNullOrEmpty(copyTxt))
            {
                CommonUtils.Prompt2Second("推送", "无可发送内容");
                return;
            }

            if (TraderCbb_YC != null)
            {
                if (!string.IsNullOrEmpty(TraderCbb_YC.SelectedValues))
                {
                    string[] orgArr = TraderCbb_YC.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                    if (orgArr.Length > 10)
                    {
                        CommonUtils.Prompt2Second("推送", "客户最多支持10位");
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            foreach (string qq in orgArr)
                            {
                                SendFQGroupMessageReq request = new SendFQGroupMessageReq();
                                request.message = copyTxt;
                                request.Username = LocalEnv.UserInfo.UserName;
                                request.groupcode = qq;
                                //request.groupOpenID = "806244298ACFCBD0A99F63DD48CDA338";
                                request.sender = LocalEnv.UserInfo.QQNumber;

                                PResult p = FinQHandler.Instance.SendFQGroupMessage(request);

                                if (p == null || !p.IsSuccess())
                                {
                                    string errorMsg = p != null && !string.IsNullOrEmpty(p.Msg) ? p.Msg : "推送失败";
                                    sb.AppendLine(errorMsg);
                                }
                            }

                            if (sb.Length > 0)
                            {
                                CommonUtils.Prompt2Second("推送", sb.ToString());
                            }
                            else
                            {
                                CommonUtils.Prompt2Second("推送", "推送成功");
                            }
                        });
                    }
                }
                else
                {
                    CommonUtils.Prompt2Second("推送", "请先选择客户");
                }
            }
        }

        private void PushMsg()
        {
            StringBuilder sb = new StringBuilder();
            //string copyTxt = this.contentTxt.Text;
            string copyTxt = GetRichTextBox();

            if (TraderCbb != null)
            {
                if (!string.IsNullOrEmpty(TraderCbb.SelectedValues))
                {
                    string[] orgArr = TraderCbb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);

                    if (orgArr.Length > 10)
                    {
                        CommonUtils.Prompt2Second("推送", "客户最多支持10位");
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            foreach (string idbKey in orgArr)
                            {
                                SendFQMessageReq request = new SendFQMessageReq();
                                request.content = copyTxt;
                                request.userid = LocalEnv.UserInfo.UserId;
                                request.Username = LocalEnv.UserInfo.UserName;
                                //request.price = _manualEntryViewModel.CustomPrice;
                                //request.code = _manualEntryViewModel.SearchText;
                                //request.direction = _manualEntryViewModel.Side == "1" ? "bid" : "ofr";
                                //request.name = _manualEntryViewModel.Symbol;
                                //request.volume = _manualEntryViewModel.SourceAmount;
                                List<CrmContact> contractList = CommonUtils.GetContactInfoPo(idbKey, null);

                                if (contractList != null && contractList.Count > 0)
                                {
                                    CrmContact cc = contractList[0];
                                    string qqlist = contractList[0].qqlist;
                                    if (string.IsNullOrEmpty(qqlist) == false)
                                    {
                                        request.traderQQ = qqlist;
                                    }
                                    else
                                    {
                                        sb.AppendLine($"{cc.namecn} {cc.instname} 未配置QQ");
                                        continue;
                                    }
                                }

                                PResult p = FinQHandler.Instance.SendFQMessage(request);

                                if (p == null || !p.IsSuccess())
                                {
                                    string errorMsg = p != null && !string.IsNullOrEmpty(p.Msg) ? p.Msg : "推送失败";
                                    sb.AppendLine(errorMsg);
                                }
                            }

                            if (sb.Length > 0)
                            {
                                CommonUtils.Prompt2Second("推送", sb.ToString());
                            }
                            else
                            {
                                CommonUtils.Prompt2Second("推送", "推送成功");
                            }
                        });
                    }
                }
                else
                {
                    CommonUtils.Prompt2Second("推送", "请先选择客户");
                }
            }
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            string copyTxt = GetRichTextBox();
            ClipboardHelper.CopyToClipboard(copyTxt);
            CommonUtils.Prompt2Second("提示", "已复制");
        }

        private void settingBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isIntentionPermission = AdminHandler.Instance.CheckPermisionById("4000", "4008");
            bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4013");
            string name = "内部看板";
            if (isPermission == false && isIntentionPermission)
            {
                name = "需求管理";
            }
            if (!WindowManager.Instance.IsExist(nameof(SettingView)))
            {
                SettingView settingView = new SettingView();
                DesignHandler.Instance.showDesignUI(settingView);
                settingView.SetInnerSettingSelect(name);
                WindowManager.Instance.OpenOnlyOneWindow(nameof(SettingView), settingView);
            }
            else
            {
                SettingView settingView = WindowManager.Instance.GetExistWin(nameof(SettingView)) as SettingView;
                if (settingView != null)
                    settingView.SetInnerSettingSelect(name);
            }
        }

        private void resettingBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadParam();
            //this.contentTxt.Text = context + "\r\n" + original_text;
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "FQ_Copy" });
            parseTxt(CopyStr);
            FeedRichTextBox();
        }

        private void bp_bid_Click(object sender, RoutedEventArgs e)
        {
            
            Button btn = sender as Button;
            if (btn.Name == "bp1_bid")
            {
                foreach (CopyElement ce in this.elementList)
                {
                    string price = ExtractFirstMatchingDouble(ce.LeftPrice);
                    if (CommonUtils.IsNumeric(price))
                    {
                        decimal d = price.ConvertToDecimal();
                        string newPrice = (d + bp1.ConvertToDecimal() / 100).ToString();
                        ce.LeftPrice = ce.LeftPrice.Replace(price, newPrice);
                    }
                }
            }
            else if (btn.Name == "bp2_bid")
            {
                foreach (CopyElement ce in this.elementList)
                {
                    string price = ExtractFirstMatchingDouble(ce.LeftPrice);
                    if (CommonUtils.IsNumeric(price))
                    {
                        decimal d = price.ConvertToDecimal();
                        string newPrice = (d + bp2.ConvertToDecimal() / 100).ToString();
                        ce.LeftPrice = ce.LeftPrice.Replace(price, newPrice);
                    }
                }
            }
            else if (btn.Name == "bp3_bid")
            {
                foreach (CopyElement ce in this.elementList)
                {
                    string price = ExtractFirstMatchingDouble(ce.LeftPrice);
                    if (CommonUtils.IsNumeric(price))
                    {
                        decimal d = price.ConvertToDecimal();
                        string newPrice = (d + bp3.ConvertToDecimal() / 100).ToString();
                        ce.LeftPrice = ce.LeftPrice.Replace(price, newPrice);
                    }
                }
            }
            else if (btn.Name == "bid")
            {
                foreach (CopyElement ce in this.elementList)
                {
                    if (ce.LeftPrice != "--")
                    {
                        ce.LeftPrice = "bid";
                    }
                }
            }
            //this.contentTxt.Text = context + "\r\n" + GenNewContext();
            FeedRichTextBox();
        }

        public string ExtractFirstMatchingDouble(string input)
        {
            Match match = Regex.Match(input, @"\b\d+(\.\d+)?\b");
            if (match.Success)
            {
                return match.Value;
            }
            return string.Empty;
        }

        private void bp_ofr_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Name == "bp1_ofr")
            {
                foreach (CopyElement ce in this.elementList)
                {
                    string price = ExtractFirstMatchingDouble(ce.RightPrice);
                    if (CommonUtils.IsNumeric(price))
                    {
                        decimal d = price.ConvertToDecimal();
                        string newPrice = (d - bp1.ConvertToDecimal() / 100).ToString();
                        ce.RightPrice = ce.RightPrice.Replace(price, newPrice);
                    }
                }
            }
            else if (btn.Name == "bp2_ofr")
            {
                foreach (CopyElement ce in this.elementList)
                {
                    string price = ExtractFirstMatchingDouble(ce.RightPrice);
                    if (CommonUtils.IsNumeric(price))
                    {
                        decimal d = price.ConvertToDecimal();
                        string newPrice = (d - bp2.ConvertToDecimal() / 100).ToString();
                        ce.RightPrice = ce.RightPrice.Replace(price, newPrice);
                    }
                }
            }
            else if (btn.Name == "bp3_ofr")
            {
                foreach (CopyElement ce in this.elementList)
                {
                    string price = ExtractFirstMatchingDouble(ce.RightPrice);
                    if (CommonUtils.IsNumeric(price))
                    {
                        decimal d = price.ConvertToDecimal();
                        string newPrice = (d - bp3.ConvertToDecimal() / 100).ToString();
                        ce.RightPrice = ce.RightPrice.Replace(price, newPrice);
                    }
                }
            }
            else if (btn.Name == "ofr")
            {
                foreach (CopyElement ce in this.elementList)
                {
                    if (ce.RightPrice != "--")
                    {
                        ce.RightPrice = "ofr";
                    }
                }
            }
            //this.contentTxt.Text = context + "\r\n" + GenNewContext();
            FeedRichTextBox();
        }

        private void qty_Click(object sender, RoutedEventArgs e)
        {
            foreach(CopyElement ce in this.elementList)
            {
                ce.LeftQty = "--";
                ce.RightQty = "--";
            }

            //this.contentTxt.Text = context + "\r\n" + GenNewContext();
            FeedRichTextBox();
        }

        private void FeedRichTextBox()
        {
            rtb.Document.Blocks.Clear();
            rtb.AppendText($"{context}\r", "#D7D7D7");
            int i = 0;
            foreach (CopyElement ce in this.elementList)
            {
                if (i > 0)
                {
                    rtb.AppendText("\r");
                }

                rtb.AppendText(ce.StartPart.Replace("主/债:", string.Empty));

                if (ce.LeftPrice != null && ce.RightPrice != null)
                {
                    rtb.AppendText(tabStr);
                    if (CommonUtils.IsNumeric(ce.LeftPrice) && ce.LeftPrice.ConvertToDecimal() <= 0)
                    {
                        rtb.AppendText(ce.LeftPrice, "Red");
                    }
                    else
                    {
                        rtb.AppendText(ce.LeftPrice, "#ED6D00");
                    }
                    rtb.AppendText("/", "#007FFF");
                    if (CommonUtils.IsNumeric(ce.RightPrice) && ce.RightPrice.ConvertToDecimal() <= 0)
                    {
                        rtb.AppendText(ce.RightPrice, "Red");
                    }
                    else
                    {
                        rtb.AppendText(ce.RightPrice, "#007FFF");
                    }
                }

                if (ce.LeftQty != null && ce.RightQty != null)
                {
                    rtb.AppendText(tabStr, "#D7D7D7");
                    rtb.AppendText(ce.LeftQty);
                    rtb.AppendText("/");
                    rtb.AppendText(ce.RightQty);
                }
                  
                rtb.AppendText(tabStr);
                rtb.AppendText(ce.EndPart);
                i++;
            }
        }

        private string GetContent_YC()
        {
            StringBuilder content = new StringBuilder();
            int i = 0;
            foreach (CopyElement ce in this.elementList)
            {
                if ((push_bid_chk.IsChecked.HasValue && push_bid_chk.IsChecked.Value && ce.LeftPrice == "--")
                    || (push_ofr_chk.IsChecked.HasValue && push_ofr_chk.IsChecked.Value && ce.RightPrice == "--"))
                {
                    continue;
                }
                if (i > 0)
                {
                    content.Append("\r");
                }
                content.Append(ce.StartPart.Replace("主/债:", string.Empty));
                content.Append(tabStr);

                if ((push_all_chk.IsChecked.HasValue && push_all_chk.IsChecked.Value)
                    || (push_bid_chk.IsChecked.HasValue && push_bid_chk.IsChecked.Value))
                {
                    content.Append(ce.LeftPrice);
                }
                else
                {
                    content.Append("--");
                }
                content.Append("/");
               
                if ((push_all_chk.IsChecked.HasValue && push_all_chk.IsChecked.Value)
                    || (push_ofr_chk.IsChecked.HasValue && push_ofr_chk.IsChecked.Value))
                {
                    content.Append(ce.RightPrice);
                }
                else
                {
                    content.Append("--");
                }
                

                content.Append(tabStr);
                if ((push_all_chk.IsChecked.HasValue && push_all_chk.IsChecked.Value)
                    || (push_bid_chk.IsChecked.HasValue && push_bid_chk.IsChecked.Value))
                {
                    content.Append(ce.LeftQty);
                }
                else
                {
                    content.Append("--");
                }
                content.Append("/");
                if ((push_all_chk.IsChecked.HasValue && push_all_chk.IsChecked.Value)
                    || (push_ofr_chk.IsChecked.HasValue && push_ofr_chk.IsChecked.Value))
                {
                    content.Append(ce.RightQty);
                }
                else
                {
                    content.Append("--");
                }
                content.Append(tabStr);
                content.Append(ce.EndPart);
                i++;
            }
            return content.ToString();
        }

        private string GetRichTextBox()
        {
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }

        private string GenNewContext()
        {
            StringBuilder sb = new StringBuilder();

            foreach(CopyElement ce in this.elementList)
            {
                if (sb.Length > 0)
                {
                    sb.Append("\r\n");
                }
                sb.Append(ce.StartPart);
                sb.Append(tabStr);
                sb.Append(ce.LeftPrice);
                sb.Append("/");
                sb.Append(ce.RightPrice);
                sb.Append(tabStr);
                sb.Append(ce.LeftQty);
                sb.Append("/");
                sb.Append(ce.RightQty);
                sb.Append(tabStr);
                sb.Append(ce.EndPart);
            }

            return sb.ToString();
        }
    }

    public class CopyElement
    {
        private string _startPart;

        public string StartPart
        {
            get { return _startPart; }
            set { _startPart = value; }
        }


        private string _leftPrice;

        public string LeftPrice
        {
            get { return _leftPrice; }
            set { _leftPrice = value; }
        }

        private string _rightPrice;

        public string RightPrice
        {
            get { return _rightPrice; }
            set { _rightPrice = value; }
        }


        private string _leftQty;

        public string LeftQty
        {
            get { return _leftQty; }
            set { _leftQty = value; }
        }

        private string _rightQty;

        public string RightQty
        {
            get { return _rightQty; }
            set { _rightQty = value; }
        }

        private string _endPart;

        public string EndPart
        {
            get { return _endPart; }
            set { _endPart = value; }
        }
    }

    public class YCQQ
    {
        private string _qq;

        public string qq
        {
            get { return _qq; }
            set { _qq = value; }
        }

        private string _name;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

    }
}
