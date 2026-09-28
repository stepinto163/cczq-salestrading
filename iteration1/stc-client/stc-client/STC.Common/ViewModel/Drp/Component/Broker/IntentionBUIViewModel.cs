using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Command;
using STC.Common.Handler.PT;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Drp.Component.Broker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.ViewModel.Drp.Component.Broker
{
    public class IntentionBUIViewModel : ViewModelBase
    {
        #region 构造
        public IntentionBUI ParentWindow;
       
        private string _OrderID;
        public string OrderID
        {
            get
            {
                return _OrderID;
            }
            set
            {
                _OrderID = value;
                this.OnPropertyChanged();
            }
        }

        private string _ExecID;
        public string ExecID
        {
            get
            {
                return _ExecID;
            }
            set
            {
                _ExecID = value;
                this.OnPropertyChanged();
            }
        }

        private string _SecurityID;
        public string SecurityID
        {
            get
            {
                return _SecurityID;
            }
            set
            {
                _SecurityID = value;
                this.OnPropertyChanged();
            }
        }

        private string _Symbol;
        public string Symbol
        {
            get
            {
                return _Symbol;
            }
            set
            {
                _Symbol = value;
                this.OnPropertyChanged();
            }
        }

        private string _UpdateTime;
        public string UpdateTime
        {
            get
            {
                return _UpdateTime;
            }
            set
            {
                _UpdateTime = value;
                this.OnPropertyChanged();
            }
        }


        private string _Side;
        public string Side
        {
            get
            {
                return _Side;
            }
            set
            {
                _Side = value;
                this.OnPropertyChanged();
            }
        }
       
        private Brush bgbackground = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
        public Brush BGBackground
        {
            get { return bgbackground; }
            set
            {
                if (value != null)
                {
                    bgbackground = value;
                    this.OnPropertyChanged(nameof(BGBackground));
                }
            }
        }
    
        private string _MsgId;
        public string MsgId
        {
            get
            {
                return _MsgId;
            }
            set
            {
                _MsgId = value;
                this.OnPropertyChanged();
            }
        }

        private string _TraderBName;
        public string TraderBName
        {
            get
            {
                return _TraderBName;
            }
            set
            {
                _TraderBName = value;
                this.OnPropertyChanged();
            }
        }


        private string _CumQty;
        public string CumQty
        {
            get
            {
                return _CumQty;
            }
            set
            {
                _CumQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _Count;
        public string Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<GrFeedBackViewModel> _FeedBackQuotations = new ObservableCollection<GrFeedBackViewModel>();
        /// <summary>
        /// 列表
        /// </summary>
        public ObservableCollection<GrFeedBackViewModel> FeedBackQuotations
        {
            get
            {
                return _FeedBackQuotations;
            }
            set
            {
                _FeedBackQuotations = value;
                this.OnPropertyChanged();
            }
        }


        private string _BlockNum;
        public string BlockNum
        {
            get
            {
                return _BlockNum;
            }
            set
            {
                _BlockNum = value;
                this.OnPropertyChanged();
            }
        }
        private object updlock = new object();

        #region  Command
        public ICommand RejectCommand { get; set; }

        public ICommand OkCommand { get; set; }

        public ICommand SubtractionCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand ManualCommand { get; set; }
        public ICommand AutoCommand { get; set; }

        #endregion

        #endregion

        public IntentionBUIViewModel()
        {
            this.RejectCommand = new DelegateCommand(new Action<object>(OnRejectCommand));
            this.OkCommand = new DelegateCommand(new Action<object>(OnOkCommand));
            this.SubtractionCommand = new DelegateCommand(OnSubtractionClick);
            this.AddCommand = new DelegateCommand(OnAddClick);
            this.ManualCommand = new DelegateCommand(OnManualCommand);
            this.AutoCommand = new DelegateCommand(OnAutoCommand);
        }

        private bool IsInitData = false;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="polist"></param>
        public void InitData(List<ExecutionReport> polist)
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                ExecutionReport po = polist[0];
                if (!IsInitData)
                {
                    IsInitData = true;
                    string tag1 = po.Tag1;
                    if (tag1 != null)
                    {
                        GrTBookOrderModel grdr = CommonUtils.DeserializeObject<GrTBookOrderModel>(tag1);
                        CumQty = grdr?.QuoteQty;
                    }
                    SecurityID = po.SecurityID;
                    Symbol = po.Symbol;
                    UpdateTime = po.UpdateTime;
                    Side = po.Side.Equals("1") ? "2" : "1";
                    BGBackground = Side == "2" ? (Brush)ParentWindow.FindResource("OfrLinearGradientBrush") : (Brush)ParentWindow.FindResource("BidLinearGradientBrush");
                    TraderBName = po.BridgeTraderName2;
                    
                    Count = po.Info2 ?? "0";
                    BlockNum = po.BlockNum ?? "";
                }

                UpdateData(polist);
            }));

        }


        public void UpdateData(List<ExecutionReport> polist)
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        foreach (ExecutionReport po in polist)
                        {
                            string key = po.ExecID;
                            bool isfound = false;
                            GrFeedBackViewModel findDr = null;
                            lock (updlock)
                            {
                                findDr = FeedBackQuotations.Where(a => a.ExecID == key).FirstOrDefault();
                                if (findDr == null)
                                {
                                    isfound = false;
                                    findDr = new GrFeedBackViewModel();
                                    findDr.HandlInst = po.HandlInst;
                                    findDr.ManualEnable = "1";
                                    findDr.PartySubID = po.CounterPartySubID;
                                    findDr.PartyTraderId = po.CounterPartyTraderId;
                                    findDr.ExecID = po.ExecID;
                                    findDr.SecurityID = po.SecurityID;
                                    findDr.Symbol = po.Symbol;
                                    findDr.Side = po.Side.Equals("1") ? "2" : "1";


                                    string counterTraderName = po.CounterPartyTraderName;
                                    if (!string.IsNullOrEmpty(counterTraderName))
                                    {
                                        findDr.CounterPartyInstitutionName = po.CounterPartyInstitutionName + string.Format("({0})", counterTraderName);
                                    }
                                    else
                                    {
                                        findDr.CounterPartyInstitutionName = po.CounterPartyInstitutionName;
                                    }
                                    findDr.CounterPartyTraderName = po.CounterPartySubName;
                                    if (findDr.PartySubID.Equals(po.PartySubID))
                                    {
                                        findDr.IsMe = true;
                                    }
                                    findDr.TraderName = po.PartySubName;
                                    if (!string.IsNullOrEmpty(po.Tag1))
                                    {
                                        PopupERPo er = CommonUtils.DeserializeObject<PopupERPo>(po.Tag1);
                                        findDr.DayPrice = er.Price;
                                        findDr.DayOrderQty = er.OrderQty;
                                        findDr.DaySettlType = er.SettlType;
                                        findDr.DayComment = er.Text;
                                        findDr.DayHandlInst = er.HandlInst;

                                        string bargainFlag = er.BargainFlag;
                                        // 换券标识 true: 换券; false 不换券
                                        findDr.ExchangeSignFlag = string.IsNullOrEmpty(er.ExchangeSignFlag) ? false : er.ExchangeSignFlag == "0" ? false : true;
                                        //议价标识 0:无星; 1: 1星; 2: 2星
                                        findDr.bargainFlag = er.BargainFlag;
                                        //OCO 打包 1:oco 2:打包
                                        findDr.ocoFlag = er.OCOFlag ?? "";

                                        findDr.wholeFlag = er.WholeFlag;

                                        if ("1".Equals(er.WholeFlag))
                                        {
                                            findDr.DayComment = "整量 " + findDr.DayComment;
                                        }

                                        if (po.OrdStatus.Equals("2"))
                                        {
                                            findDr.ButtonIsEnable = "1";
                                        }
                                        else
                                        {
                                            findDr.ButtonIsEnable = "0";
                                        }

                                        findDr.TraderBQQ = er.counterPartySubQQ;
                                        findDr.SettlType = po.SettlType;

                                    }
                                }
                                else
                                {
                                    isfound = true;
                                }


                                
                                findDr.QQMsg = po.SendQQMsgTem;
                                string sendqqmsgflag = po.SendQQMsgFlag;
                               
                                if ("1".Equals(sendqqmsgflag))
                                {
                                    findDr.AutoEnable = "0";
                                }
                                else
                                {
                                    findDr.AutoEnable = "1";
                                }

                                //如果没有配置QQ，则禁止自动和手动发送按钮
                                if (string.IsNullOrEmpty(findDr.TraderBQQ))
                                {
                                    findDr.AutoEnable = "0";
                                    findDr.ManualEnable = "0";
                                }

                                findDr.OrderID = po.StrategyOrderId;
                                
                                findDr.UpdateTime = po.UpdateTime;
                                findDr.Price = po.Price;
                                findDr.Comment = po.Remark;
                                findDr.LastQty = po.LastQty == "0" ? po.BrokerBConfirmQty : po.LastQty;
                                findDr.OrderQty = po.OrderQty;
                                findDr.MaxAmount = CommonUtils.ConvertToDecimal(findDr.OrderQty);
                                findDr.OrdStatus = po.OrdStatus;

                                if ((po.OrdStatus.Equals("E")) && !po.LastQty.Equals(po.OrderQty))
                                {
                                    findDr.OrdStatus = "1";
                                    findDr.ButtonIsEnable = "0";
                                    findDr.PartLastQty = po.LastQty;
                                    
                                }
                                else if ((po.OrdStatus.Equals("G")) && !po.LastQty.Equals(po.OrderQty))
                                {
                                    findDr.OrdStatus = "1";
                                    findDr.ButtonIsEnable = "0";
                                }
                                else if ((po.OrdStatus.Equals("E")) && po.LastQty.Equals(po.OrderQty))
                                {
                                    findDr.ButtonIsEnable = "0";
                                }

                                if (findDr.OrdStatus.Equals("F") || findDr.OrdStatus.Equals("H"))
                                {
                                    findDr.ButtonIsEnable = "0";
                                }

                                findDr.PointOrderQty = (CommonUtils.ConvertToLong(findDr.OrderQty) / 1000).ToString();

                                if (!isfound)
                                {
                                    FeedBackQuotations.Add(findDr);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                    //UpdateConfirmData();
                });
            }));
        }


        #region  OnCommand 
        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="inparam"></param>
        public void OnOkCommand(object inparam)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GrFeedBackViewModel dr = FeedBackQuotations.FirstOrDefault(a => a.ExecID == (string)inparam);
                    if (dr == null)
                    {
                        return;
                    }

                    if (dr.ButtonIsEnable == "0")
                        return;

                    ExecutionReport er = new ExecutionReport();
                    er.OrderID = dr.OrderID;
                    er.LastQty = CommonUtils.ConvertToString(dr.Amount);
                    er.OrdStatus = "E";
                    er.ExecID = dr.ExecID;
                    if (CommonUtils.ConvertToDecimal(dr.OrderQty).CompareTo(CommonUtils.ConvertToDecimal(dr.LastQty)) != 0)
                    {
                        er.CommandType = "27";
                    }
                    else
                    {
                        er.CommandType = "26";
                    }
                    er.CloseBy = LocalEnv.UserInfo.UserName;
                    er.CloseById = LocalEnv.UserInfo.UserId;
                    PResult reply = GRHandler.Instance.ExecOrderUpdate(er);
                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        //OnCloseSelf();
                    }
                    else
                    {
                        dr.OrdStatus = "X";
                    }
                });
            });
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        public void OnRejectCommand(object inparam)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GrFeedBackViewModel dr = FeedBackQuotations.FirstOrDefault(a => a.ExecID == (string)inparam);
                    if (dr == null)
                    {
                        return;
                    }

                    ExecutionReport er = new ExecutionReport();
                    //er.OrderID = OrderID;
                    er.OrdStatus = "F";
                    er.CommandType = "28";
                    er.ExecID = dr.ExecID;
                    er.CloseBy = LocalEnv.UserInfo.UserName;
                    er.CloseById = LocalEnv.UserInfo.UserId;
                    PResult reply = GRHandler.Instance.ExecOrderUpdate(er);
                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        //OnCloseSelf();
                    }
                    else
                    {
                        dr.OrdStatus = "X";
                    }
                });
            });
        }

        /// <summary>
        /// 手动发送消息
        /// </summary>
        /// <param name="obj"></param>
        private void OnManualCommand(object inparam)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GrFeedBackViewModel dr = FeedBackQuotations.FirstOrDefault(a => a.ExecID == (string)inparam);
                    if (dr == null)
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(dr.TraderBQQ))
                    {
                        Process pro = new Process();
                        ClipboardHelper.CopyToClipboard(dr.QQMsg);
                        string arguments = string.Format("tencent://message/?Menu=yes&amp;uin={0}", dr.TraderBQQ);
                        Process.Start(arguments);
                        Thread.Sleep(1000);
                        //if (LocalEnv.ConfInfo.QQStartSleep.HasValue && LocalEnv.ConfInfo.QQStartSleep.Value > 0)
                        //    Thread.Sleep(LocalEnv.ConfInfo.QQStartSleep.Value);
                        System.Windows.Forms.SendKeys.SendWait("^V");

                        //if (LocalEnv.ConfInfo.QQSendSleep.HasValue && LocalEnv.ConfInfo.QQSendSleep.Value > 0)
                        //    Thread.Sleep(LocalEnv.ConfInfo.QQSendSleep.Value);

                        Thread.Sleep(1000);
                        System.Windows.Forms.SendKeys.SendWait("{Enter}");
                        ClipboardHelper.CopyToClipboard(dr.QQMsg);
                    }
                    else
                    {
                        string errorMsg = string.Format("{0} QQ is null", TraderBName);
                        BrokerMessageBoxGUI msgBox = new BrokerMessageBoxGUI();
                        msgBox.setWarnMsg(errorMsg, false);
                        msgBox.Show();
                    }
                });
            });
        }

        /// <summary>
        /// 自动发送消息
        /// </summary>
        /// <param name="obj"></param>
        private void OnAutoCommand(object inparam)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GrFeedBackViewModel dr = FeedBackQuotations.FirstOrDefault(a => a.ExecID == (string)inparam);
                    if (dr == null)
                    {
                        return;
                    }

                    SendQQMsgReq er = new SendQQMsgReq();
                    er.execId = dr.ExecID;
                    er.partySubId = dr.PartySubID;
                    er.traderId = dr.PartyTraderId;
                    er.userName = LocalEnv.UserInfo.UserName;
                    er.message = dr.QQMsg;
                    er.sendType = "1";
                    PResult reply = GRHandler.Instance.sendQQMsg(er);
                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        //AutoEnable = "0";
                    }
                });
            });
        }
        #endregion

     
        private string getDayOrderInfo(string info7, int index)
        {
            string result = "";
            if (!string.IsNullOrEmpty(info7))
            {
                string[] info7Array = info7.Split('|');
                if (info7Array.Count() > (index + 1))
                {
                    result = "null".Equals(info7Array[index]) ? "" : info7Array[index];
                }
            }
            return result;
        }

        public void OnSubtractionClick(object inparam)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                GrFeedBackViewModel dr = FeedBackQuotations.FirstOrDefault(a => a.ExecID == (string)inparam);
                if (dr == null)
                {
                    return;
                }

                if (STC.Common.Utils.CommonUtils.ConvertToDecimal(dr.PointOrderQty) * dr.AmountUnit - dr.AmountUnit >= dr.MinAmount)
                {
                    dr.Amount = STC.Common.Utils.CommonUtils.ConvertToDecimal(dr.PointOrderQty) * dr.AmountUnit - dr.AmountUnit;
                    dr.PointOrderQty = (dr.Amount / dr.AmountUnit).ToString();
                }
            }));
        }

        public void OnAddClick(object inparam)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                GrFeedBackViewModel dr = FeedBackQuotations.FirstOrDefault(a => a.ExecID == (string)inparam);
                if (dr == null)
                {
                    return;
                }

                if (STC.Common.Utils.CommonUtils.ConvertToDecimal(dr.PointOrderQty) * dr.AmountUnit + dr.AmountUnit <= dr.MaxAmount)
                {
                    dr.Amount = STC.Common.Utils.CommonUtils.ConvertToDecimal(dr.PointOrderQty) * dr.AmountUnit + dr.AmountUnit;
                    dr.PointOrderQty = (dr.Amount / dr.AmountUnit).ToString();
                }
            }));
        }

        public void OnCloseSelf()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ParentWindow != null)
                {
                    try
                    {
                        ParentWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Error(ex);
                    }
                }
            });
        }
    }
}
