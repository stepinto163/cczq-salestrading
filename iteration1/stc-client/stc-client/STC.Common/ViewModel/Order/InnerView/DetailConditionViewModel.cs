using Newtonsoft.Json;
using STC.Common.Model;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Order.InnerPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STC.Common.ViewModel.Order.InnerView
{
    public class DetailConditionViewModel : BaseViewModel
    {
        /// <summary>
        /// 0：Bid
        /// 1: Ofr
        /// </summary>
        private string side;
        public string Side
        {
            get { return side; }
            set
            {
                side = value;


                if (side == "0")
                {
                    SyntropyTxt = "Bid";
                    ReverseTxt = "Ofr";

                    //CheckSyntropy = false;
                    //CheckReverse = true;
                }
                else
                {
                    SyntropyTxt = "Ofr";
                    ReverseTxt = "Bid";


                    //CheckSyntropy = false;
                    //CheckReverse = true;
                }

                SetBg();
            }
        }


        private Visibility cancelConVisibility;
        public Visibility CancelConVisibility
        {
            get { return cancelConVisibility; }
            set
            {
                cancelConVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility subConVisibility = Visibility.Collapsed;
        public Visibility SubConVisibility
        {
            get { return subConVisibility; }
            set
            {
                subConVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility conditionvisibility = Visibility.Visible;
        public Visibility ConditionVisibility
        {
            get { return conditionvisibility; }
            set
            {
                conditionvisibility = value;

                this.OnPropertyChanged();
            }
        }

        

        private Visibility _lbSubVisibility = Visibility.Visible;
        public Visibility LBSubVisibility
        {
            get
            {
                // 当 app.config 配置 HideIfSubBond_TBook=1 时，强制隐藏【订阅】标签
                if ("1".Equals(LocalEnv.ConfInfo.HideIfSubBond_TBook))
                {
                    return Visibility.Collapsed;
                }
                return _lbSubVisibility;
            }
            set
            {
                _lbSubVisibility = value;
                if (value != Visibility.Visible)
                {
                    SettleBoxIndex = 0;
                    //CheckSyntropy = false;
                    //CheckReverse = false;
                }
                else
                {
                    if (side == "0")
                    {
                        SyntropyTxt = "Bid";
                        ReverseTxt = "Ofr";

                        //CheckSyntropy = false;
                        //CheckReverse = true;
                    }
                    else
                    {
                        SyntropyTxt = "Ofr";
                        ReverseTxt = "Bid";


                        //CheckSyntropy = false;
                        //CheckReverse = true;
                    }
                }
                
                this.OnPropertyChanged();
            }
        }


        private int settleBoxIndex = 0;
        public int SettleBoxIndex
        {
            get { return settleBoxIndex; }
            set
            {
                settleBoxIndex = value;

                if (value == 0)
                {
                    SubConVisibility = Visibility.Collapsed;
                    CancelConVisibility = Visibility.Visible;
                    ConditionVisibility = Visibility.Visible;
                }
                else if (value == 1)
                {
                    SubConVisibility = Visibility.Visible;
                    CancelConVisibility = Visibility.Collapsed;
                    ConditionVisibility = Visibility.Collapsed;
                }

                SetBg();

                this.OnPropertyChanged();
            }
        }

        private string bgImageSource;
        public string BgImageSource
        {
            get
            {
                return bgImageSource;
            }

            set
            {
                bgImageSource = value;

                this.OnPropertyChanged();

            }
        }

        private Brush bgImageBrush;
        public Brush BgImageBrush
        {
            get
            {
                return bgImageBrush;
            }

            set
            {
                bgImageBrush = value;

                this.OnPropertyChanged();

            }
        }


        private FrameworkElement myWindow;
        public FrameworkElement MyWindow
        {
            get
            {
                return myWindow;
            }

            set
            {
                myWindow = value;

                this.OnPropertyChanged();

            }
        }

        private Brush itemHeadForeground;
        public Brush ItemHeadForeground
        {
            get { return itemHeadForeground; }
            set { itemHeadForeground = value; this.OnPropertyChanged(); }
        }


        public void SetBg()
        {
            if (this.myWindow != null)
            {
                if (this.myWindow.DataContext is SendPriceViewModel)
                {
                    ChangeBg((myWindow.DataContext as SendPriceViewModel).IsBid);
                }
                else if (this.myWindow.DataContext is SendRequireViewModel)
                {
                    ChangeBg((myWindow.DataContext as SendRequireViewModel).IsBid);
                }
            }
        }

        private void ChangeBg(bool isBid)
        {
            ImageBrush brush = new ImageBrush();
            string imageSource = string.Empty;
            if (SettleBoxIndex == 0)
            {
                imageSource = isBid ? "/STC.Common;component/Images/stc_oco_bid_first.png" : "/STC.Common;component/Images/stc_oco_ofr_first.png";
            }
            else if (SettleBoxIndex == 1)
            {
                //imageSource = isBid ? "/STC.Common;component/Images/stc_oco_bid.png" : "/STC.Common;component/Images/stc_oco_ofr.png";
                imageSource = isBid ? "/STC.Common;component/Images/stc_oco_bid.png" : "/STC.Common;component/Images/stc_oco_ofr.png";
            }
          
            brush.ImageSource = new BitmapImage(new Uri(@imageSource, UriKind.Relative));

            var converter = new System.Windows.Media.BrushConverter();
            ItemHeadForeground = isBid ? (Brush)converter.ConvertFromString("#ED6D00") : (Brush)converter.ConvertFromString("#0082D8");
            BgImageBrush = brush;

            BgImageSource = imageSource;

        }

        #region 撤单条件

        #region 倒挂撤单

        private string morethanMarketBP;
        public string MorethanMarketBP
        {
            get { return morethanMarketBP; }
            set
            {
                morethanMarketBP = value;
                this.OnPropertyChanged();
            }
        }

        private bool morethanMarketBPCheck;
        public bool MorethanMarketBPCheck
        {
            get { return morethanMarketBPCheck; }
            set
            {
                morethanMarketBPCheck = value;
                this.OnPropertyChanged();

            }
        }


        #endregion


        #region 有效时长

        private int validSelectIndex = 0;

        public int ValidSelectIndex
        {
            get
            {
                return validSelectIndex;
            }

            set
            {
                validSelectIndex = value;
                if (validSelectIndex == 0)
                {
                    DefaultValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 1)
                {
                    HourValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 2)
                {
                    TimeValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 3)
                {
                    NoLimitValidTimeQuotationCheck = true;
                }

                this.OnPropertyChanged();
            }
        }

        private int defaultValidUntilTime = 30;
        public int DefaultValidUntilTime
        {
            get
            {
                return defaultValidUntilTime;
            }

            set
            {
                defaultValidUntilTime = value;
                this.OnPropertyChanged();
            }
        }



        private bool defaultValidTimeQuotationCheck = true;
        /// <summary>
        /// 默认
        /// </summary>
        public bool DefaultValidTimeQuotationCheck
        {
            get
            {
                return defaultValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(HourValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                    {
                        return;
                    }
                }

                defaultValidTimeQuotationCheck = value;
                if (defaultValidTimeQuotationCheck)
                {
                    HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }

                this.OnPropertyChanged();
            }
        }

        private bool hourValidTimeQuotationCheck = false;
        /// <summary>
        /// 时长
        /// </summary>
        public bool HourValidTimeQuotationCheck
        {
            get
            {
                return hourValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(DefaultValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                    {
                        return;
                    }
                }
                hourValidTimeQuotationCheck = value;
                if (hourValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = TimeValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 时长 - 分钟
        /// </summary>
        private string validMinute = string.Empty;
        public string ValidMinute
        {
            get { return validMinute; }
            set
            {
                if (value != null)
                {
                    validMinute = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  时长 - 秒钟
        /// </summary>
        private string validSecond = string.Empty;
        public string ValidSecond
        {
            get { return validSecond; }
            set
            {
                if (value != null)
                {
                    validSecond = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 时点 - 小时
        /// </summary>
        private string validTimeHour = string.Empty;
        public string ValidTimeHour
        {
            get { return validTimeHour; }
            set
            {
                if (value != null)
                {
                    validTimeHour = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 时点 - 分钟
        /// </summary>
        private string validTimeMinute = string.Empty;
        public string ValidTimeMinute
        {
            get { return validTimeMinute; }
            set
            {
                if (value != null)
                {
                    validTimeMinute = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 时点 - 秒钟
        /// </summary>
        private string validTimeSecond = string.Empty;
        public string ValidTimeSecond
        {
            get { return validTimeSecond; }
            set
            {
                if (value != null)
                {
                    validTimeSecond = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private bool timeValidTimeQuotationCheck = false;
        /// <summary>
        /// 时点
        /// </summary>
        public bool TimeValidTimeQuotationCheck
        {
            get
            {
                return timeValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(DefaultValidTimeQuotationCheck ^ HourValidTimeQuotationCheck))
                    {
                        return;
                    }
                }
                timeValidTimeQuotationCheck = value;
                if (timeValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        private bool noLimitValidTimeQuotationCheck = false;
        /// <summary>
        /// 无
        /// </summary>
        public bool NoLimitValidTimeQuotationCheck
        {
            get
            {
                return noLimitValidTimeQuotationCheck;
            }

            set
            {
                noLimitValidTimeQuotationCheck = value;
                if (noLimitValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        #endregion


        #endregion


        #region 订阅

        /// <summary>
        /// 该券同向
        /// </summary>
        private bool checkSyntropy;
        public bool CheckSyntropy
        {
            get { return checkSyntropy; }
            set { checkSyntropy = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 该券反向
        /// </summary>
        private bool checkReverse;
        public bool CheckReverse
        {
            get { return checkReverse; }
            set { checkReverse = value; this.OnPropertyChanged(); }
        }

        private string syntropyTxt;
        public string SyntropyTxt
        {
            get { return syntropyTxt; }
            set { syntropyTxt = value; this.OnPropertyChanged(); }
        }

        private string reverseTxt;
        public string ReverseTxt
        {
            get { return reverseTxt; }
            set { reverseTxt = value; this.OnPropertyChanged(); }
        }


        private string securityID;
        public string SecurityID
        {
            get { return securityID; }
            set { securityID = value; this.OnPropertyChanged(); }
        }

        #endregion


        #region 命令事件

        public ICommand AddCommand { get { return new ComRelayCommand(AddClick); } }

        public ICommand DownCommand { get { return new ComRelayCommand(DownClick); } }


        private void AddClick()
        {
            decimal d = 0;
            if (decimal.TryParse(MorethanMarketBP, out d))
            {
                d += 1;

                MorethanMarketBP = d.ToString();
            }
            else if (string.IsNullOrEmpty(MorethanMarketBP))
            {
                d = 1;

                MorethanMarketBP = d.ToString();
            }

        }

        private void DownClick()
        {
            decimal d = 0;
            if (decimal.TryParse(MorethanMarketBP, out d))
            {
                d -= 1;

                if (d >= 0)
                    MorethanMarketBP = d.ToString();
            }
        }



        #endregion


        public void Init(string securityId, string side, FrameworkElement myW)
        {
            this.SecurityID = securityId;
            this.Side = side;
            this.MyWindow = myW;
        }

        #region 撤单条件

        public PriceCondition GetPriceCondition()
        {
            PriceCondition priceCondition = new PriceCondition();
            if (morethanMarketBPCheck)
            {
                priceCondition.MorethanMarketBP = morethanMarketBP == null ? string.Empty : morethanMarketBP;
                priceCondition.MorethanMarketBPCheck = morethanMarketBPCheck ? "1" : "0";
            }

            return priceCondition;
        }


        public void SetPriceCondition(string condtionJson)
        {
            if (!string.IsNullOrEmpty(condtionJson))
            {
                PriceCondition priceCondition = JsonConvert.DeserializeObject<PriceCondition>(condtionJson);
                if (priceCondition != null)
                {
                    if (string.IsNullOrEmpty(priceCondition.MorethanMarketBPCheck) && string.IsNullOrEmpty(priceCondition.MorethanMarketBP))
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(priceCondition.MorethanMarketBPCheck))
                    {
                        MorethanMarketBPCheck = false;
                    }
                    else
                    {
                        if (priceCondition.MorethanMarketBPCheck.Equals("0"))
                            MorethanMarketBPCheck = false;
                        else
                            MorethanMarketBPCheck = true;
                    }

                    if (string.IsNullOrEmpty(priceCondition.MorethanMarketBP))
                    {
                        MorethanMarketBP = string.Empty;
                    }
                    else
                    {
                        MorethanMarketBP = priceCondition.MorethanMarketBP;
                        MorethanMarketBPCheck = true;
                    }
                }
            }
            else
            {
                MorethanMarketBP = string.Empty;
                MorethanMarketBPCheck = false;

                MorethanMarketBPCheck = false;
            }
        }

        #endregion


        #region 订阅

        private string BID = "BID";
        private string OFR = "OFR";
        private string ALL = "ALL";

        public TagData GetTagData()
        {
            TagData tagData = new TagData();

            if (checkSyntropy && !CheckReverse)
            {
                tagData.IF_SUB_BOND = Side == "0" ? "BID" : "OFR";
            }
            else if (!checkSyntropy && CheckReverse)
            {
                tagData.IF_SUB_BOND = Side == "0" ? "OFR" : "BID";
            }
            else if (checkSyntropy && CheckReverse)
            {
                tagData.IF_SUB_BOND = "ALL";
            }
            else
            {
                tagData.IF_SUB_BOND = string.Empty;
            }

            return tagData;
        }


        public void SetTagData(string info2, string side = "1")
        {
            if (!string.IsNullOrEmpty(info2))
            {
                try
                {
                    TagData tagData = JsonConvert.DeserializeObject<TagData>(info2);

                    if (tagData.IF_SUB_BOND == BID)
                    {
                        CheckSyntropy = side == "1" ? true : false;
                        CheckReverse = side == "1" ? false : true;
                    }
                    if (tagData.IF_SUB_BOND == OFR)
                    {
                        CheckSyntropy = side == "1" ? false : true;
                        CheckReverse = side == "1" ? true: false;
                    }

                    if (tagData.IF_SUB_BOND == ALL)
                    {
                        CheckSyntropy = true;
                        CheckReverse = true;
                    }

                } catch (Exception e)
                {

                }
            }
        }

        #endregion

        #region valideTime设置

        public void SetValideTime(string info)
        {
            if (JsonSplit.IsJson(info))
            {
                OrderTimeModel otm = JsonConvert.DeserializeObject<OrderTimeModel>(info);

                if (otm.ValidTimeType.StartsWith("0|"))
                {
                    validSelectIndex = 0;
                    DefaultValidTimeQuotationCheck = true;
                }
                else if (otm.ValidTimeType.StartsWith("1|"))
                {
                    validSelectIndex = 1;
                    HourValidTimeQuotationCheck = true;
                    // 时长
                    if (HourValidTimeQuotationCheck)
                    {
                        // ValidMinute,ValidSecond
                        string s = otm.ValidTimeType.Split('|')[1];
                        if (s.Contains(":"))
                        {
                            validMinute = s.Split(':')[0];
                            ValidSecond = s.Split(':')[1];
                        }
                    }
                }
                else if (otm.ValidTimeType.StartsWith("2|"))
                {
                    validSelectIndex = 2;
                    TimeValidTimeQuotationCheck = true;
                    // 时点
                    if (TimeValidTimeQuotationCheck)
                    {
                        // ValidTimeHour ValidTimeMinute ValidTimeSecond
                        string s = otm.ValidTimeType.Split('|')[1];
                        if (s.Contains(":"))
                        {
                            ValidTimeHour = s.Split(':')[0];
                            ValidTimeMinute = s.Split(':')[1];
                            ValidTimeSecond = s.Split(':')[2];
                        }
                    }
                }
                else if (otm.ValidTimeType.StartsWith("3|"))
                {
                    ValidSelectIndex = 3;
                    NoLimitValidTimeQuotationCheck = true;
                    // 无
                }
            }
        }

        #endregion
    }
}
