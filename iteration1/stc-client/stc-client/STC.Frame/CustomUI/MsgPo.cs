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

namespace STC.Frame.CustomUI
{
    public class MsgPo : BaseViewModel
    {
        public string counterpartyid;//对手交易员id
        public string counterpartyname;//对手交易员名字
        public string counterpartysubid; //对手机构id
        public string counterpartysubname;//对手机构名字
        public string transacttime;//交易时间
        public string createtime;
        public string readFlag;

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

        private string contentType;
        public string ContentType { get { return contentType; } set { contentType = value; this.OnPropertyChanged(); } }

        private Brush contentTypeBrush;
        public Brush ContentTypeBrush { get { return contentTypeBrush; } set { contentTypeBrush = value; this.OnPropertyChanged(); } }

        private string closeBy;
        public string CloseBy { get { return closeBy; } set { closeBy = value; this.OnPropertyChanged(); } }

        private Brush closeByBrush;
        public Brush CloseByBrush { get { return closeByBrush; } set { closeByBrush = value; this.OnPropertyChanged(); } }

        private string content;
        public string Content { get { return content; } set { content = value; this.OnPropertyChanged(); } }

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
    }


    public class MsgSort : IComparer<MsgPo>
    {
        public int Compare(MsgPo x, MsgPo y)
        {
            if (x.Level.CompareTo(y.Level) == 0)
                return y.UpdateTime.CompareTo(x.UpdateTime);
            else
                return x.Level.CompareTo(y.Level);
        }
    }
}
