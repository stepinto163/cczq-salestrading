using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.Model.Transaction
{
    /// <summary>
    /// 成交明细所用到的数据结构
    /// </summary>
    public class PositionDetailPo : INotifyPropertyChanged
    {//0->true,1->false

        //public event PropertyChangedEventHandler PropertyChanged;
        //public void NotifyPropertyChanged(string propertyName) {
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        /// <summary>
        /// 主键(用于拷贝)
        /// </summary>
        public string KeyForCopy { get; set; }
        /// <summary>
        /// 成交单序号
        /// </summary>
        public decimal? Seq { get; set; }

        /// <summary>
        /// 明细序号，用于复制
        /// </summary>
        public string CopySeq { get; set; }
        /// <summary>
        /// 间距
        /// </summary>
        public string Padding { get; set; }
        /// <summary>
        /// 字体 0 头尾，1 正文 2汇总
        /// </summary>
        public string FontFamily { get; set; }
        /// <summary>
        /// 字号
        /// </summary>
        public string FontSize { get; set; }
        /// <summary>
        /// 一般文本&&机构header的文本内容1
        /// </summary>
        private string text;

        public string Text
        {
            get { return text; }
            set
            {
                if (value == text)
                    return;
                text = value;
                OnPropertyChanged("Text");
            }
        }

        /// <summary>
        /// 机构header的文本内容2
        /// </summary>
        public string Text2 { get; set; }
        /// <summary>
        /// 机构名称，明细的统计单位
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 机构，明细统计单位
        /// </summary>
        public DetailTrader DetailTrader { get; set; }
        public string TextFormat { get; set; }
        /// <summary>
        /// broker是否查看，未查看背景色显红
        /// </summary>
        //public string Isbrokerunread { get; set; }

        private string isbrokerunread;
        /// <summary>
        /// broker是否查看，未查看背景色显红
        /// </summary>
        public string Isbrokerunread
        {
            get
            {
                return isbrokerunread;
            }
            set
            {
                if (isbrokerunread != value)
                {
                    isbrokerunread = value;
                    OnPropertyChanged("Isbrokerunread");
                }
            }
        }
        /// <summary>
        /// 是否有背景颜色 有未读修改则有背景色
        /// </summary>
        //public bool IsBackgColor { get; set; }

        private bool isBackgColor;
        /// <summary>
        /// 是否有背景颜色 有未读修改则有背景色
        /// </summary>
        public bool IsBackgColor
        {
            get
            {
                return isBackgColor;
            }
            set
            {
                if (isBackgColor != value)
                {
                    isBackgColor = value;
                    OnPropertyChanged("IsBackgColor");
                }
            }
        }
        /// <summary>
        /// 缺桥颜色展示-红  2-头颜色，3-汇总颜色
        /// </summary>
        public string IsShowColor { get; set; }
        /// <summary>
        /// 缺桥颜色展示-红
        /// </summary>
        public bool IsForeColor { get; set; }
        /// <summary>
        /// 是否细体，交易日期是否是今天
        /// </summary>
        public string Isliq { get; set; }
        /// <summary>
        /// 是否字体加粗  交易日期不是今天 
        /// </summary>
        public bool IsFontWeight { get; set; }
        /// <summary>
        /// 汇总按钮是否显示
        /// 其他人和无机构分组不需要显示汇总按钮
        /// </summary>
        public string SumIsVisible { get; set; }
        /// <summary>
        /// 发送按钮是否显示
        /// 没有交易员的分组不需要显示发送按钮
        /// 其他人和无机构分组不需要显示发送按钮
        /// </summary>
        public string SendIsVisible { get; set; }
        /// <summary>
        /// 是否是机构header信息
        /// 除了主体数据，header，尾，历史分割线，汇总数据行都属于header
        /// Header--0
        /// </summary>
        public string IsHeader { get; set; }
        public string IsWidth { get; set; }
        public string IsMargin { get; set; }

        /// <summary>
        /// wxp 20200217 机构的复制文本:不带broker复制
        public string WithoutBrokerText { get; set; }

        /// <summary>
        /// 机构的复制文本:Ctrl-C
        public string TotalText { get; set; }


        /// <summary>
        /// 带联系人复制
        /// </summary>
        public string WithContactsText { get; set; }

        /// <summary>
        /// 隐藏对手方复制
        /// </summary>
        public string HideCounterpartyText { get; set; }


        /// <summary>
        /// 带联系人复制
        /// </summary>
        public string WithTraderText { get; set; }

        /// <summary>
        /// 机构的复制文本:带单号
        /// </summary>
        public string CopyText { get; set; }

        /// <summary>
        /// 机构的提示文本
        /// </summary>
        public string ToolText { get; set; }

        /// <summary>
        /// 是否是汇总信息
        /// </summary>
        public bool IsSum = false;
        public string BrokerOfr { get; set; }
        public string BrokerBid { get; set; }
        public string BondCode { get; set; }
        /// <summary>
        /// Ofr机构
        /// </summary>
        public string Insta { get; set; }
        /// <summary>
        /// Bid机构
        /// </summary>
        public string Instb { get; set; }
        /// <summary>
        /// TraderA  按照trade判断方向
        /// </summary>
        public string Tradera { get; set; }
        /// <summary>
        /// TraderB
        /// </summary>
        public string Traderb { get; set; }
        /// <summary>
        /// 是否是历史单
        /// </summary>
        public bool IsHist { get; set; }
        public decimal Price { get; set; }
        public string OfrDealdate { get; set; }
        public string BidDealdate { get; set; }
        public string OfrSettlementdate { get; set; }
        public string BidSettlementdate { get; set; }
        /// <summary>
        /// 交易员A名字
        /// </summary>
        public string BankAgentNameA { get; set; }
        /// <summary>
        /// 交易员B名字
        /// </summary>
        public string BankAgentNameB { get; set; }
        /// <summary>
        /// 成交单ID
        /// </summary>
        public string ExecId { get; set; }
        /// <summary>
        /// 2.4需求 ，是否展示删除线
        /// </summary>
        public bool IsShowDel { get; set; }

        /// <summary>
        /// V2.5添加数据类型，用于区分数据项
        /// </summary>
        public EDataType DataType { get; set; }
        /// <summary>
        /// 2.5需求 ,机构方向
        /// </summary>
        public string SideShow { get; set; }
        //****************v2.6*********
        /// <summary>
        /// //1信用,2利率,11 ABS,12 NCD
        /// </summary>
        public string goodstype { get; set; }
        /// <summary>
        /// 债券发行量
        /// </summary>
        public decimal? IssueAmount { get; set; }
        /// <summary>
        /// 评级
        /// </summary>
        public string IssuerRatingCurrent { get; set; }


        /// <summary>
        /// 机构的复制文本:Ctrl-C
        public string HideSendQQMsgText { get; set; }

        /// <summary>
        /// 助理A文本
        /// </summary>
        public string AssistantAText { get; set; }
        /// <summary>
        /// 助理B文本
        /// </summary>
        public string AssistantBText { get; set; }


        public string htmlText;
        public string HtmlText
        {
            get
            {
                string htmltxt = string.Empty;
                if (DataType != EDataType.汇总)
                {
                    htmltxt = TotalText;
                }
                else
                {
                    htmltxt = string.IsNullOrWhiteSpace(Text) ? Text : Text.TrimStart();
                }
                //text-decoration line-through none
                string html = "";
                if (FontFamily == null || FontSize == null)
                    return "";
                string strValue = FontFamily.ToString();
                if (strValue.IndexOf('-') > 0)
                {
                    string[] ss = strValue.Split('-');
                    if (ss[1].Equals(""))
                        switch (ss[2])
                        {
                            case "0":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:楷体"
                 + "; font-size:18"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            case "2":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:宋体"
                 + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            default:
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:Microsoft YaHei Semibold"
                 + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                        }
                }
                if (!IsFontWeight)
                    html = html.Replace("<b>", "").Replace("</b>", "");
                return html;
            }
            set { htmlText = value; }
        }

        public string htmlWithContactsText;
        public string HtmlWithContactsText
        {
            get
            {
                string htmltxt = string.Empty;
                if (DataType != EDataType.汇总)
                {
                    htmltxt = WithContactsText;
                }
                else
                {
                    htmltxt = string.IsNullOrWhiteSpace(Text) ? Text : Text.TrimStart();
                }
                //text-decoration line-through none
                string html = "";
                if (FontFamily == null || FontSize == null)
                    return "";
                string strValue = FontFamily.ToString();
                if (strValue.IndexOf('-') > 0)
                {
                    string[] ss = strValue.Split('-');
                    if (ss[1].Equals(""))
                        switch (ss[2])
                        {
                            case "0":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:楷体"
                 + "; font-size:18"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            case "2":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:宋体"
                 + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            default:
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:Microsoft YaHei Semibold"
                 + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                        }
                }
                if (!IsFontWeight)
                    html = html.Replace("<b>", "").Replace("</b>", "");
                return html;
            }
            set
            {
                htmlWithContactsText = value;
            }
        }

        /// <summary>
        /// 隐藏对手方复制
        /// </summary>
        public string htmlHideCounterpartyText;
        public string HtmlHideCounterpartyText
        {
            get
            {
                string htmltxt = string.Empty;
                if (DataType != EDataType.汇总)
                {
                    htmltxt = HideCounterpartyText;
                }
                else
                {
                    htmltxt = string.IsNullOrWhiteSpace(Text) ? Text : Text.TrimStart();
                }
                //text-decoration line-through none
                string html = "";
                if (FontFamily == null || FontSize == null)
                    return "";
                string strValue = FontFamily.ToString();
                if (strValue.IndexOf('-') > 0)
                {
                    string[] ss = strValue.Split('-');
                    if (ss[1].Equals(""))
                        switch (ss[2])
                        {
                            case "0":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:楷体"
                 + "; font-size:18"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            case "2":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:宋体"
                 + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            default:
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:Microsoft YaHei Semibold"
                 + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                        }
                }
                if (!IsFontWeight)
                    html = html.Replace("<b>", "").Replace("</b>", "");
                return html;
            }
            set
            {
                htmlHideCounterpartyText = value;
            }
        }

        public string copyHtmlText;
        public string CopyHtmlText
        {
            get
            {
                string htmltxt = string.Empty;
                if (DataType != EDataType.汇总)
                {
                    htmltxt = CopyText;
                }
                else
                {
                    htmltxt = string.IsNullOrWhiteSpace(Text) ? Text : Text.TrimStart();
                }
                string html = "";
                if (FontFamily == null || FontSize == null)
                    return "";
                string strValue = FontFamily.ToString();
                if (strValue.IndexOf('-') > 0)
                {
                    string[] ss = strValue.Split('-');
                    if (ss[1].Equals(""))
                        switch (ss[2])
                        {
                            case "0":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                  + "; font-family:楷体"
                    + "; font-size:18"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            case "2":
                                html = "<div><span style =\"background:"
          + (IsBackgColor ? "#FFC271" : "#White")
          + "; color:" + (IsForeColor ? "red" : "#000000")
           + "; font-family:宋体"
            + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
          + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            default:
                                html = "<div><span style =\"background:"
      + (IsBackgColor ? "#FFC271" : "#White")
      + "; color:" + (IsForeColor ? "red" : "#000000")
       + "; font-family:Microsoft YaHei Semibold"
        + "; font-size:14"
      + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
      + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                        }
                }
                if (!IsFontWeight)
                    html = html.Replace("<b>", "").Replace("</b>", "");
                return html;
            }
            set { copyHtmlText = value; }
        }
        /// <summary>
        /// wxp 20200117 添加不带broker复制 
        /// </summary>
        public string withoutBrokerHtmlText;
        public string WithoutBrokerHtmlText
        {
            get
            {
                string htmltxt = string.Empty;
                if (DataType != EDataType.汇总)
                {
                    htmltxt = WithoutBrokerText;
                }
                else
                {
                    htmltxt = string.IsNullOrWhiteSpace(Text) ? Text : Text.TrimStart();
                }
                string html = "";
                if (FontFamily == null || FontSize == null)
                    return "";
                string strValue = FontFamily.ToString();
                if (strValue.IndexOf('-') > 0)
                {
                    string[] ss = strValue.Split('-');
                    if (ss[1].Equals(""))
                        switch (ss[2])
                        {
                            case "0":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                  + "; font-family:楷体"
                    + "; font-size:18"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            case "2":
                                html = "<div><span style =\"background:"
          + (IsBackgColor ? "#FFC271" : "#White")
          + "; color:" + (IsForeColor ? "red" : "#000000")
           + "; font-family:宋体"
            + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
          + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            default:
                                html = "<div><span style =\"background:"
      + (IsBackgColor ? "#FFC271" : "#White")
      + "; color:" + (IsForeColor ? "red" : "#000000")
       + "; font-family:Microsoft YaHei Semibold"
        + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
      + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                        }
                }
                if (!IsFontWeight)
                    html = html.Replace("<b>", "").Replace("</b>", "");
                return html;
            }
            set { withoutBrokerHtmlText = value; }
        }


        /// <summary>
        /// 隐藏对手方复制
        /// </summary>
        public string htmlWithTraderText;
        public string HtmlWithTraderText
        {
            get
            {
                string htmltxt = string.Empty;
                if (DataType != EDataType.汇总)
                {
                    htmltxt = WithTraderText;
                }
                else
                {
                    htmltxt = string.IsNullOrWhiteSpace(Text) ? Text : Text.TrimStart();
                }
                //text-decoration line-through none
                string html = "";
                if (FontFamily == null || FontSize == null)
                    return "";
                string strValue = FontFamily.ToString();
                if (strValue.IndexOf('-') > 0)
                {
                    string[] ss = strValue.Split('-');
                    if (ss[1].Equals(""))
                        switch (ss[2])
                        {
                            case "0":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:楷体"
                 + "; font-size:18"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            case "2":
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:宋体"
                 + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                            default:
                                html = "<div><span style =\"background:"
                 + (IsBackgColor ? "#FFC271" : "#White")
                 + "; color:" + (IsForeColor ? "red" : "#000000")
                 + "; font-family:Microsoft YaHei Semibold"
                 + "; font-size:14"
                 + "; text-decoration:" + (IsShowDel ? "line-through" : "none")
                 + "\"><b>" + htmltxt + "</b></span></div></br>";
                                break;
                        }
                }
                if (!IsFontWeight)
                    html = html.Replace("<b>", "").Replace("</b>", "");
                return html;
            }
            set
            {
                htmlWithTraderText = value;
            }
        }


        /// <summary>
        /// 内部成交图标是否显示
        /// </summary>
        public Visibility ForbidRefreshFlagVisible { get; set; } = Visibility.Collapsed;


        /// <summary>
        /// 内部成交图标是否显示
        /// </summary>
        //public string ForbidRefreshBrush { get; set; }= "#F0892A";



        /// <summary>
        /// 手动发送按钮
        /// </summary>
        private string sendManuallyTag = "-1";
        /// <summary>
        /// 手动发送按钮
        /// </summary>
        public string SendManuallyTag
        {
            get
            {
                return sendManuallyTag;
            }
            set
            {
                if (sendManuallyTag != value)
                {
                    sendManuallyTag = value;
                    OnPropertyChanged("SendManuallyTag");
                }
            }
        }


        /// <summary>
        /// 手动发送按钮
        /// </summary>
        private string detailMarkDisplay = "0";
        /// <summary>
        /// 标记标签
        /// </summary>
        public string DetailMarkDisplay
        {
            get
            {
                return detailMarkDisplay;
            }
            set
            {
                if (detailMarkDisplay != value)
                {
                    detailMarkDisplay = value;
                    OnPropertyChanged("DetailMarkDisplay");
                }
            }
        }


        public List<string> MarkDetailUserList { get; set; }

        /// <summary>
        /// 手动发送按钮
        /// </summary>
        public Visibility SendManuallyVisible { get; set; } = Visibility.Collapsed;

        public Visibility SendManuallyNoVisible { get; set; } = Visibility.Collapsed;


        /// <summary>
        /// 是否brokerA 1,brokerA, 2 BrokerB
        /// </summary>
        public string IsBrokerATag { get; set; }

        /// <summary>
        /// 成交单序号
        /// </summary>
        public string OrderId { get; set; }


        /// <summary>
        /// 同步状态成功是否显示
        /// </summary>
        public Visibility SynsStatusS { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// 同步状态失败是否显示
        /// </summary>
        public Visibility SynsStatusF { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// 经纪商名称 20220728 新增  v2.20.3 版本
        /// </summary>
        public string BrokerCompanyName { get; set; }

        private Visibility sendQQMsgVisible = Visibility.Collapsed;
        /// <summary>
        /// 单条发送qq消息按钮是否展示
        /// </summary>
        public Visibility SendQQMsgVisible
        {
            get
            {
                return sendQQMsgVisible;
            }
            set
            {
                if (sendQQMsgVisible != value)
                {
                    sendQQMsgVisible = value;
                    OnPropertyChanged("SendQQMsgVisible");
                }
            }
        }

        private Visibility todayLaterVisible = Visibility.Collapsed;
        /// <summary>
        /// 今+按钮是否展示
        /// </summary>
        public Visibility TodayLaterVisible
        {
            get
            {
                return todayLaterVisible;
            }
            set
            {
                if (todayLaterVisible != value)
                {
                    todayLaterVisible = value;
                    OnPropertyChanged("TodayLaterVisible");
                }
            }
        }

        /// <summary>
        /// 发送qq消息机构 IDB唯一标识
        /// </summary>
        public string SendQQbankId { get; set; }
        /// <summary>
        /// 发送qq消息交易员 IDB唯一标识
        /// </summary>
        public string SendQQbankagentId { get; set; }
        /// <summary>
        /// 发送qq消息交易员名字
        /// </summary>
        public string SendQQbankagentName { get; set; }

        /// <summary>
        /// 发送qq消息交易员 IDB唯一标识
        /// </summary>
        public string TxtBankagentIdA { get; set; }
        /// <summary>
        /// 发送qq消息交易员名字
        /// </summary>
        public string TxtBankagentIdB { get; set; }
        /// <summary>
        /// 经纪人id
        /// </summary>
        public string BrokerID { get; set; }

        /// <summary>
        /// 头行汇总check框是否勾选
        /// </summary>
        public bool isCheckSum = false;
        public bool IsCheckSum
        {
            get
            {
                return isCheckSum;
            }
            set
            {
                if (isCheckSum != value)
                {
                    isCheckSum = value;
                    OnPropertyChanged("IsCheckSum");
                }
            }
        }

        /// <summary>
        /// 头行汇总check框是否勾选
        /// </summary>
        public bool isCheckSide = false;
        public bool IsCheckSide
        {
            get
            {
                return isCheckSide;
            }
            set
            {
                if (isCheckSide != value)
                {
                    isCheckSide = value;
                    OnPropertyChanged("IsCheckSide");
                }
            }
        }
        #region 属性变化事件

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
    public enum EDataType
    {
        头 = 1,
        数据 = 2,
        汇总 = 3,
        尾 = 4,
        历史头 = 5,
        其他人成交单头,
        空格
    }
}
