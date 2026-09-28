using STC.Common.Model;
using STC.Common.Model.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.Common.ViewModel.Drp
{
    /// <summary>
    /// 消息代码和消息提醒的数据结构
    /// </summary>
    public class MsgPo : BaseViewModel
    {
        public string username;

        public string counterpartyid;//对手交易员id
        public string counterpartyname;//对手交易员名字
        public string counterpartysubid; //对手机构id
        public string counterpartysubname;//对手机构名字
        public string transacttime;//交易时间
        public string createtime;
        public string readFlag;
        public string securityid;
        public string source;
        public string strategyOrderId;

        #region 漂浮属性
        
        private bool _timeOut = false;
        public bool TimeOut { get { return _timeOut; } set { _timeOut = value; this.OnPropertyChanged(); } }

        private ColorAnimation _innerColorAnimation;

        public ColorAnimation InnerColorAnimation {  get { return _innerColorAnimation; } set { _innerColorAnimation = value; this.OnPropertyChanged(); } }

        private Visibility _popQuickDealVisibility = Visibility.Collapsed;
        public Visibility PopQuickDealVisibility { get { return _popQuickDealVisibility; } set { _popQuickDealVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _popDealVisibility;
        public Visibility PopDealVisibility { get { return _popDealVisibility; } set { _popDealVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _popRefuseVisibility = Visibility.Collapsed;
        public Visibility PopRefuseVisibility { get { return _popRefuseVisibility; } set { _popRefuseVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _popIgnoreVisibility;
        public Visibility PopIgnoreVisibility { get { return _popIgnoreVisibility; } set { _popIgnoreVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _lookVisibility;
        public Visibility LookVisibility { get { return _lookVisibility; } set { _lookVisibility = value; this.OnPropertyChanged(); } }

        private GridLength _gridHeight;
        public GridLength GridHeight { get { return _gridHeight; } set { _gridHeight = value; this.OnPropertyChanged(); } }

        private string _timeOutTxt;
        public string TimeOutTxt { get { return _timeOutTxt; } set { _timeOutTxt = value; this.OnPropertyChanged(); } }

        private Visibility _timeOutTxtVisibility = Visibility.Collapsed;
        public Visibility TimeOutTxtVisibility { get { return _timeOutTxtVisibility; } set { _timeOutTxtVisibility = value; this.OnPropertyChanged(); } }
        #endregion

        private bool isPopShow = false;
        public bool IsPopShow { get { return isPopShow; } set { isPopShow = value; this.OnPropertyChanged(); } }

        private string contentType;
        public string ContentType { get { return contentType; } set { contentType = value; this.OnPropertyChanged(); } }

        private TMessageContent messageContent;
        public TMessageContent MessageContent { get { return messageContent; } set { messageContent = value; this.OnPropertyChanged(); } }

        private string venue;
        public string Venue { get { return venue; } set { venue = value; this.OnPropertyChanged(); } }

        private string btnShowTxt;
        public string BtnShowTxt { get { return btnShowTxt; } set { btnShowTxt = value; this.OnPropertyChanged(); } }

        private string btnDealShowTxt;
        public string BtnDealShowTxt { get { return btnDealShowTxt; } set { btnDealShowTxt = value; this.OnPropertyChanged(); } }

        private string delFlag;
        public string DelFlag { get { return delFlag; } set { delFlag = value; this.OnPropertyChanged(); } }
        private string detail;
        public string Detail { get { return detail; } set { detail = value; this.OnPropertyChanged(); } }

        private string signalDetail;
        public string SignalDetail { get { return signalDetail; } set { signalDetail = value; this.OnPropertyChanged(); } }


        private string msgType;

        public string MsgType { get { return msgType; } set { msgType = value; this.OnPropertyChanged(); } }

        private string borderName;
        public string BorderName { get { return borderName; } set { borderName = value; this.OnPropertyChanged(); } }

        private Storyboard storyboard;
        public Storyboard Storyboard { get { return storyboard; } set { storyboard = value; this.OnPropertyChanged(); } }


        private string id;
        public string Id { get { return id; } set { id = value; } }
        private string level;
        public string Level { get { return level; } set { level = value; } }

        private Brush showBG;
        public Brush ShowBG { get { return showBG; } set { showBG = value; this.OnPropertyChanged(); } }

        private Brush showPopBG;
        public Brush ShowPopBG { get { return showPopBG; } set { showPopBG = value; this.OnPropertyChanged(); } }

        private Brush showBorderBrush;//= new SolidColorBrush(Colors.Transparent);

        public Brush ShowBorderBrush { get { return showBorderBrush; } set { showBorderBrush = value; this.OnPropertyChanged(); } }

        private double showBorderThickness;
        public double ShowBorderThickness { get { return showBorderThickness; } set { showBorderThickness = value; this.OnPropertyChanged(); } }

        private string headerTitle;
        public string HeaderTitle { get { return headerTitle; } set { headerTitle = value; this.OnPropertyChanged(); } }

        private Brush contentTypeBrush;
        public Brush ContentTypeBrush { get { return contentTypeBrush; } set { contentTypeBrush = value; this.OnPropertyChanged(); } }

        private string closeBy;
        public string CloseBy { get { return closeBy; } set { closeBy = value; this.OnPropertyChanged(); } }

        private Brush closeByBrush;
        public Brush CloseByBrush { get { return closeByBrush; } set { closeByBrush = value; this.OnPropertyChanged(); } }

        private string content;
        public string Content { get { return content; } set { content = value; ShowContent = !string.IsNullOrEmpty(content) && content.Length > 50 ? (content.Substring(0, 50) + "...") : content; this.OnPropertyChanged(); } }

        private string showContent;
        public string ShowContent { get { return showContent; } set { showContent = value; this.OnPropertyChanged(); } }

        private Brush contentBrush;
        public Brush ContentBrush { get { return contentBrush; } set { contentBrush = value; this.OnPropertyChanged(); } }

        private string status;
        public string Status { get { return status; } set { status = value; this.OnPropertyChanged(); } }

        private Brush statusBrush;
        public Brush StatusBrush { get { return statusBrush; } set { statusBrush = value; this.OnPropertyChanged(); } }

        private string valueTime;
        public string ValueTime { get { return valueTime; } set { valueTime = value; this.OnPropertyChanged(); } }

        private Brush valueTimeBrush;
        public Brush ValueTimeBrush { get { return valueTimeBrush; } set { valueTimeBrush = value; this.OnPropertyChanged(); } }

        private string updateTime;
        public string UpdateTime { get { return updateTime; } set { updateTime = value; this.OnPropertyChanged(); } }

        private Visibility updateTimeVisibility = Visibility.Visible;
        public Visibility UpdateTimeVisibility { get { return updateTimeVisibility; } set { updateTimeVisibility = value; this.OnPropertyChanged(); } }

        private Brush updateTimeBrush;
        public Brush UpdateTimeBrush { get { return updateTimeBrush; } set { updateTimeBrush = value; this.OnPropertyChanged(); } }

        private Brush showStatusBG;
        public Brush ShowStatusBG { get { return showStatusBG; } set { showStatusBG = value; this.OnPropertyChanged(); } }
        
        private Visibility dealVisibility;
        public Visibility DealVisibility { get { return dealVisibility; } set { dealVisibility = value; this.OnPropertyChanged(); } }

        private Visibility knowVisibility;
        public Visibility KnowVisibility { get { return knowVisibility; } set { knowVisibility = value; this.OnPropertyChanged(); } }

        private Visibility valueTimeVisibility;
        public Visibility ValueTimeVisibility { get { return valueTimeVisibility; } set { valueTimeVisibility = value; this.OnPropertyChanged(); } }

        private Visibility statusVisibility;
        public Visibility StatusVisibility { get { return statusVisibility; } set { statusVisibility = value; this.OnPropertyChanged(); } }

        private double showPopHeight;
        public double ShowPopHeight { get { return showPopHeight; } set { showPopHeight = value; this.OnPropertyChanged(); } }

        private double showHeight;
        public double ShowHeight { get { return showHeight; } set { showHeight = value; this.OnPropertyChanged(); } }

        private double statusWidth = 35;
        public double StatusWidth { get { return statusWidth; } set { statusWidth = value; this.OnPropertyChanged(); } }

        private Visibility closeByVisibility = Visibility.Visible;
        public Visibility CloseByVisibility { get { return closeByVisibility; } set { closeByVisibility = value; this.OnPropertyChanged(); } }

        private double closeByHeight = 22;
        public double CloseByHeight { get { return closeByHeight; } set { closeByHeight = value; this.OnPropertyChanged(); } }

        private string presult;//处理结果
        private string ptime;//处理时间
        public string Presult
        {
            get
            {
                return presult;
            }

            set
            {
                presult = value;
                this.OnPropertyChanged();
            }
        }

        public string Ptime
        {
            get
            {
                return ptime;
            }

            set
            {
                ptime = value;
                this.OnPropertyChanged();
            }
        }

        private bool? isShow;
        public bool? IsShow
        { 
            get 
            {
                return isShow; 
            } 
            set 
            {
                isShow = value; 
                this.OnPropertyChanged(); 
            } 
        }

		 private string _groupname;
		 public string groupname
		 {
		     get
		     {
		         return _groupname;
		     }
		     set
		     {
		         _groupname = value;
		         this.OnPropertyChanged();
		     }
		 }
    }


    public class MsgSort : IComparer<MsgPo>
    {
        public int Compare(MsgPo x, MsgPo y)
        {
            //if (x.Level.CompareTo(y.Level) == 0)
                return y.UpdateTime.CompareTo(x.UpdateTime);
            
            //else
            //    return x.Level.CompareTo(y.Level);
        }
    }
}
