using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge {
    /// <summary>
    /// 加桥的成交单,即显示在过桥窗口里的成交单
    /// </summary>
    public class BridgeOrderDetail : OrderDetailPo, INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string deleteImageSource;
        /// <summary>
        /// 删除成交单图标
        /// </summary>
        public string DeleteImageSource { get { return deleteImageSource; } set { deleteImageSource = value; NotifyPropertyChanged("DeleteImageSource"); } }


        private bool bidIsEnable = true;
        /// <summary>
        /// B加桥按钮使能
        /// </summary>
        public bool BidIsEnable { get { return bidIsEnable; } set { bidIsEnable = value; NotifyPropertyChanged("BidIsEnable"); } }

        private bool ofrIsEnable = true;
        public bool OfrIsEnable { get { return ofrIsEnable; } set { ofrIsEnable = value; NotifyPropertyChanged("OfrIsEnable"); } }

        private bool editIsEnable = true;
        /// <summary>
        /// 桥编辑按钮使能
        /// </summary>
        public bool EditIsEnable { get { return editIsEnable; } set { editIsEnable = value; NotifyPropertyChanged("EditIsEnable"); } }

        private bool deleteIsEnable = true;
        /// <summary>
        /// 桥删除按钮使能
        /// </summary>
        public bool DeleteIsEnable { get { return deleteIsEnable; } set { deleteIsEnable = value; NotifyPropertyChanged("DeleteIsEnable"); } }


        private string orderTxt;
        /// <summary>
        /// 桥的显示详情文本
        /// </summary>
        public string OrderTxt { get { return orderTxt; } set { orderTxt = value; NotifyPropertyChanged("OrderTxt"); } }

        private string oldOrderTxt;
        /// <summary>
        /// 桥的显示详情文本
        /// </summary>
        public string OldOrderTxt
        {
            get
            {
                return oldOrderTxt;
            }
            set
            {
                oldOrderTxt = value;
            }
        }

        private string oldOrderNoseqTxt;
        /// <summary>
        /// 桥的显示详情不带序号文本
        /// </summary>
        public string OldOrderNoseqTxt
        {
            get
            {
                return oldOrderNoseqTxt;
            }
            set
            {
                oldOrderNoseqTxt = value;
            }
        }
        private string ofrImageSource;
        /// <summary>
        /// O加桥按钮图标
        /// </summary>
        public string OfrImageSource { get { return ofrImageSource; } set { ofrImageSource = value; NotifyPropertyChanged("OfrImageSource"); } }

        private string bidImageSource;
        /// <summary>
        /// B加桥按钮图标
        /// </summary>
        public string BidImageSource { get { return bidImageSource; } set { bidImageSource = value; NotifyPropertyChanged("BidImageSource"); } }

        private string editImageSource;
        /// <summary>
        /// 桥编辑按钮图标
        /// </summary>
        public string EditImageSource { get { return editImageSource; } set { editImageSource = value; NotifyPropertyChanged("EditImageSource"); } }

        private int orderGroup;

        /// <summary>
        /// 分组展示
        /// 正常单子标号:1 
        /// 今日发单标号:2 这里显示的是今天发单的历史成交单的header 
        /// 今日发单数据标号:3 这里显示的是今天发单的历史成交单
        /// </summary>
        public int OrderGroup { get { return orderGroup; } set { orderGroup = value; NotifyPropertyChanged("OrderGroup"); } }

        private string showBrokerNameA;
        public string ShowBrokerNameA
        {
            get
            {
                return showBrokerNameA;
            }
            set
            {
                showBrokerNameA = value;

                NotifyPropertyChanged("ShowBrokerNameA");
            }
        }

        private string showBrokerNameB;
        public string ShowBrokerNameB
        {
            get
            {
                return showBrokerNameB;
            }
            set
            {
                showBrokerNameB = value;

                NotifyPropertyChanged("ShowBrokerNameB");
            }
        }


        private string ofrInstName;
        public string OfrInstName
        {
            get
            {
                return ofrInstName;
            }
            set
            {
                ofrInstName = value;

                NotifyPropertyChanged("OfrInstName");
            }
        }

        private string bidInstName;
        public string BidInstName
        {
            get
            {
                return bidInstName;
            }
            set
            {
                bidInstName = value;

                NotifyPropertyChanged("BridgeTrader");
            }
        }

        public int bridgeSeq;
        /// <summary>
        /// 桥的单号，格式： 1）
        /// </summary>
        public string ShowSeq { get { return bridgeSeq + ")"; } }

        public string ShowTime { get; set; }

        /// <summary>
        /// 20200316 2.3需求 过桥窗口的机构和交易员显示
        /// </summary>
        public string InstAndTraderAShow
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TraderName) && string.IsNullOrWhiteSpace(InstitutionName) && string.IsNullOrWhiteSpace(InstitutionShowName))
                    return "";

                if (!string.IsNullOrWhiteSpace(TraderName) && (!string.IsNullOrWhiteSpace(InstitutionName) || !string.IsNullOrEmpty(InstitutionShowName)))
                {
                    if (!string.IsNullOrWhiteSpace(InstitutionShowName))
                        return string.Format("{0} {1}", InstitutionShowName, TraderName.Trim());
                    else
                        return string.Format("{0} {1}", InstitutionName, TraderName.Trim());
                }

                if (string.IsNullOrWhiteSpace(TraderName) && (!string.IsNullOrWhiteSpace(InstitutionName) || !string.IsNullOrEmpty(InstitutionShowName)))
                {
                    if (!string.IsNullOrWhiteSpace(InstitutionShowName))
                        return InstitutionShowName;
                    else
                        return InstitutionName;
                }
                if (!string.IsNullOrWhiteSpace(TraderName) && string.IsNullOrWhiteSpace(InstitutionName) && string.IsNullOrWhiteSpace(InstitutionShowName))
                    return TraderName;

                return "";
            }
        }
        /// <summary>
        /// 20200316 2.3需求 过桥窗口的机构和交易员显示
        /// </summary>
        public string InstAndTraderBShow
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CounterPartyTraderName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                    return "";

                if (!string.IsNullOrWhiteSpace(CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(CounterPartyInstitutionName) || !string.IsNullOrEmpty(CounterPartyInstitutionShowName)))
                {
                    if (!string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                        return string.Format("{0} {1}", CounterPartyInstitutionShowName, CounterPartyTraderName.Trim());
                    else
                        return string.Format("{0} {1}", CounterPartyInstitutionName, CounterPartyTraderName.Trim());
                }


                if (string.IsNullOrWhiteSpace(CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(CounterPartyInstitutionName) || !string.IsNullOrEmpty(CounterPartyInstitutionShowName)))
                {
                    if (!string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                        return CounterPartyInstitutionShowName;
                    else
                        return CounterPartyInstitutionName;
                }

                if (!string.IsNullOrWhiteSpace(CounterPartyTraderName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                    return CounterPartyTraderName;

                return "";
            }
        }
        /// <summary>
        /// 20200316 2.3需求 过桥窗口的机构显示
        /// </summary>
        public string InstAShow
        {
            get
            {
                //if (string.IsNullOrWhiteSpace(InstitutionName) && string.IsNullOrWhiteSpace(PartyName))
                //    return "";

                //if ((!string.IsNullOrWhiteSpace(InstitutionName) || !string.IsNullOrEmpty(PartyName)))
                //{
                //    if (!string.IsNullOrWhiteSpace(PartyName))
                //        return PartyName;
                //    else
                //        return InstitutionName;
                //}
                if (string.IsNullOrWhiteSpace(InstitutionShowName) && string.IsNullOrWhiteSpace(InstitutionName))
                    return "";

                if ((!string.IsNullOrWhiteSpace(InstitutionShowName) || !string.IsNullOrEmpty(InstitutionName)))
                {
                    if (!string.IsNullOrWhiteSpace(InstitutionShowName))
                        return InstitutionShowName;
                    else
                        return InstitutionName;
                }
                return "";
            }
        }
        /// <summary>
        /// 20200316 2.3需求 过桥窗口的机构显示
        /// </summary>
        public string InstBShow
        {
            get
            {
                //if (string.IsNullOrWhiteSpace(CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(CounterPartyName))
                //    return "";

                //if ((!string.IsNullOrWhiteSpace(CounterPartyInstitutionName) || !string.IsNullOrEmpty(CounterPartyName)))
                //{
                //    if (!string.IsNullOrWhiteSpace(CounterPartyName))
                //        return CounterPartyName;
                //    else
                //        return CounterPartyInstitutionName;
                //}
                if (string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionName))
                    return "";

                if ((!string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName) || !string.IsNullOrEmpty(CounterPartyInstitutionName)))
                {
                    if (!string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                        return CounterPartyInstitutionShowName;
                    else
                        return CounterPartyInstitutionName;
                }
                return "";
            }
        }
        public string TeaderAShow
        {
            get
            {
                if ((!string.IsNullOrWhiteSpace(InstitutionName) || !string.IsNullOrEmpty(PartyName)))
                {
                    if (!string.IsNullOrWhiteSpace(PartyName))
                        return PartyName;
                    else
                        return InstitutionName;
                }
                return "";
            }
        }
        public string TeaderBShow
        {
            get
            {
                if ((!string.IsNullOrWhiteSpace(CounterPartyInstitutionName) || !string.IsNullOrEmpty(CounterPartyName)))
                {
                    if (!string.IsNullOrWhiteSpace(CounterPartyName))
                        return CounterPartyName;
                    else
                        return CounterPartyInstitutionName;
                }
                return "";
            }
        }
        #region style属性
        private string fontFamily;
        public string FontFamily { get { return fontFamily; } set { fontFamily = value; NotifyPropertyChanged("FontFamily"); } }//字体
        private string fontSize;
        public string FontSize { get { return fontSize; } set { fontSize = value; NotifyPropertyChanged("FontSize"); } }//字号
        private string isbrokerunread;
        public string Isbrokerunread { get { return isbrokerunread; } set { isbrokerunread = value; NotifyPropertyChanged("Isbrokerunread"); } }
        private string isShowColor;
        public string IsShowColor { get { return isShowColor; } set { isShowColor = value; NotifyPropertyChanged("IsShowColor"); } }

        private string padding;
        public string Padding { get { return padding; } set { padding = value; NotifyPropertyChanged("Padding"); } }
        private string isliq;
        public string Isliq { get { return isliq; } set { isliq = value; NotifyPropertyChanged("Isliq"); } }

        #endregion

        #region 排序
        /// <summary>
        /// ofr名字的拼音首字母
        /// </summary>
        public string OfrInstNamePY;
        /// <summary>
        /// bid名字的拼音首字母
        /// </summary>
        public string BidInstNamePY;

        #endregion
        /// <summary>
        /// 桥状态
        /// </summary>
        public EBridgeState bridgeState { get { return bridgeOrder != null ? bridgeOrder.bridgeStateEnum : EBridgeState.未知; } }
        /// <summary>
        /// 桥状态2
        /// </summary>
        public EBridgeState bridgeState2 { get { return bridgeOrder2 != null ? bridgeOrder2.bridgeStateEnum : EBridgeState.未知; } }



        public string bridgeStateShow;
        /// <summary>
        /// 桥状态展示
        /// </summary>
        public string BridgeStateShow { get { return bridgeStateShow; } set { bridgeStateShow = value; NotifyPropertyChanged("BridgeStateShow"); } }


        /// <summary>
        /// Ofr桥状态
        /// </summary>
        public BridgeState OfrbridgeState
        {
            get
            {
                return bridgeOrder != null ? bridgeOrder.OfrbridgeStateEnum : BridgeState.Confirmed;
            }
        }

        /// <summary>
        /// Ofr桥状态
        /// </summary>
        public BridgeState BidbridgeState
        {
            get
            {
                return bridgeOrder != null ? bridgeOrder.BidbridgeStateEnum : BridgeState.Confirmed;
            }
        }
        /// <summary>
        /// Ofr桥状态
        /// </summary>
        public BridgeState OfrbridgeState2
        {
            get
            {
                return bridgeOrder2 != null ? bridgeOrder2.OfrbridgeStateEnum : BridgeState.Confirmed;
            }
        }

        /// <summary>
        /// Ofr桥状态
        /// </summary>
        public BridgeState BidbridgeState2
        {
            get
            {
                return bridgeOrder2 != null ? bridgeOrder2.BidbridgeStateEnum : BridgeState.Confirmed;
            }
        }

        private string ofrbridgeStateShow;
        /// <summary>
        /// Ofr桥状态展示
        /// </summary>
        public string OfrBridgeStateShow
        {
            get
            {
                return ofrbridgeStateShow;
            }
            set
            {
                ofrbridgeStateShow = value;
                NotifyPropertyChanged("OfrBridgeStateShow");
            }
        }

        private string bidbridgeStateShow;
        /// <summary>
        /// Bid桥状态展示
        /// </summary>
        public string BidBridgeStateShow
        {
            get
            {
                return bidbridgeStateShow;
            }
            set
            {
                bidbridgeStateShow = value;
                NotifyPropertyChanged("BidBridgeStateShow");
            }
        }
        /// <summary>
        /// 桥订单唯一标识，与后台数据的里的桥订单一一对应
        /// </summary>
        public string MasterID;
        /// <summary>
        /// 查看ID
        /// </summary>
        public string Reviewid;
        public EBridgeisChecked bridgeisChecked = EBridgeisChecked.以查看;
        /// <summary>
        /// 桥是否已查看
        /// </summary>
        public EBridgeisChecked BridgeisChecked { get { return bridgeisChecked; } set { bridgeisChecked = value; NotifyPropertyChanged("BridgeisCheckedShow"); ; } }
        /// <summary>
        /// 桥是否已查看展示
        /// </summary>
        public string BridgeisCheckedShow { get { return BridgeisChecked.GetDescription(); } }


        private int ofrDir;
        /// <summary>
        /// ofr桥方向
        /// </summary>
        public int OfrDir
        {
            get
            {
                return ofrDir;
            }
            set
            {
                ofrDir = value; NotifyPropertyChanged("OfrDir");
            }
        }

        private int bidDir;
        /// <summary>
        /// ofr桥方向
        /// </summary>
        public int BidDir
        {
            get
            {
                return bidDir;
            }
            set
            {
                bidDir = value; NotifyPropertyChanged("BidDir");
            }
        }
        private int bidDir2;
        /// <summary>
        /// bid桥方向2
        /// </summary>
        public int BidDir2
        {
            get
            {
                return bidDir2;
            }
            set
            {
                bidDir2 = value; NotifyPropertyChanged("BidDir2");
            }
        }


        private int doublebridgedir = 1;
        /// <summary>
        /// 桥之间方向,0无方向，1：ofr到bid；2: bid到ofr
        /// </summary>
        public int Doublebridgedir
        {
            get
            {
                return doublebridgedir;
            }
            set
            {
                doublebridgedir = value; NotifyPropertyChanged("Doublebridgedir");
            }
        }


        #region 20221206 V3.2.0
        private Visibility ofrToolTipVisibility;
        /// <summary>
        /// Ofr桥状态展示
        /// </summary>
        public Visibility OfrToolTipVisibility
        {
            get
            {
                return ofrToolTipVisibility;
            }
            set
            {
                ofrToolTipVisibility = value;
                NotifyPropertyChanged("OfrToolTipVisibility");
            }
        }
        private Visibility bidToolTipVisibility;
        /// <summary>
        /// bid桥状态展示
        /// </summary>
        public Visibility BidToolTipVisibility
        {
            get
            {
                return bidToolTipVisibility;
            }
            set
            {
                bidToolTipVisibility = value;
                NotifyPropertyChanged("BidToolTipVisibility");
            }
        }

        private Visibility ofrCancelStatusVisibility;
        /// <summary>
        /// Ofr桥状态展示
        /// </summary>
        public Visibility OfrCancelStatusVisibility
        {
            get
            {
                return ofrCancelStatusVisibility;
            }
            set
            {
                ofrCancelStatusVisibility = value;
                NotifyPropertyChanged("OfrCancelStatusVisibility");
            }
        }
        private Visibility bidCancelStatusVisibility;
        /// <summary>
        /// bid桥状态展示
        /// </summary>
        public Visibility BidCancelStatusVisibility
        {
            get
            {
                return bidCancelStatusVisibility;
            }
            set
            {
                bidCancelStatusVisibility = value;
                NotifyPropertyChanged("BidCancelStatusVisibility");
            }
        }

        private Visibility ofrContentStatusVisibility;
        /// <summary>
        /// Ofr桥状态展示
        /// </summary>
        public Visibility OfrContentStatusVisibility
        {
            get
            {
                return ofrContentStatusVisibility;
            }
            set
            {
                ofrContentStatusVisibility = value;
                NotifyPropertyChanged("OfrContentStatusVisibility");
            }
        }
        private Visibility bidContentStatusVisibility;
        /// <summary>
        /// bid桥状态展示
        /// </summary>
        public Visibility BidContentStatusVisibility
        {
            get
            {
                return bidContentStatusVisibility;
            }
            set
            {
                bidContentStatusVisibility = value;
                NotifyPropertyChanged("BidContentStatusVisibility");
            }
        }

        private string ofrSendStatusreason;
        /// <summary>
        /// Bid桥状态展示
        /// </summary>
        public string OfrSendStatusreason
        {
            get
            {
                return ofrSendStatusreason;
            }
            set
            {
                ofrSendStatusreason = value;
                NotifyPropertyChanged("OfrSendStatusreason");
            }
        }

        private string bidSendStatusreason;
        /// <summary>
        /// Bid桥状态展示
        /// </summary>
        public string BidSendStatusreason
        {
            get
            {
                return bidSendStatusreason;
            }
            set
            {
                bidSendStatusreason = value;
                NotifyPropertyChanged("BidSendStatusreason");
            }
        }
        #endregion
    }

}
