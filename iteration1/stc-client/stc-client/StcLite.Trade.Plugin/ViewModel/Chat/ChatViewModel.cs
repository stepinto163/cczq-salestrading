using Ats.Foundation.Message;
using com.gateway.log;
using BDB.Trade.Plugin.Component;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Chat;
using BDB.Trade.Plugin.View.Order;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using STC.Common.Model;
using STC.Common.Utils.SqlLite;
using STC.Common.Utils;
using STC.Common.Model.QQ;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Handler.IM;

namespace BDB.Trade.Plugin.ViewModel.Chat
{
    public class ChatViewModel : BaseViewModel
    {
        #region 构造
        private ObservableCollection<ChatMessage> m_Messages = new ObservableCollection<ChatMessage>();
        public ObservableCollection<ChatMessage> Messages
        {
            get { return m_Messages; }
            set
            {
                m_Messages = value;
                this.OnPropertyChanged(nameof(Messages));
            }
        }
        /// <summary>
        /// 全部消息
        /// </summary>
        private List<QQUserConfigPo> DictFullMsg = new List<QQUserConfigPo>();

        public ICommand SendMessage { get { return new RelayCommand(OnSendMessage); } }
        public ICommand ClearMessage { get { return new RelayCommand(OnClearMessage); } }

        private ChatView myWindow;
        //Modi:cyc Date:2021-06-17 Func:增加倒计时
        private DispatcherTimer descTimer = new DispatcherTimer();
        private object objlock = new object();

        private int selectedMessage;
        public int SelectedMessage
        {
            get { return selectedMessage; }
            set
            {
                selectedMessage = value;
                this.OnPropertyChanged(nameof(SelectedMessage));
            }
        }

        /// <summary>
        /// 人工发送消息
        /// </summary>
        private string txtmessage;

        public string TxtMessage
        {
            get { return txtmessage; }
            set
            {
                txtmessage = value;
                this.OnPropertyChanged(nameof(TxtMessage));
            }
        }

        private string broker;

        public string Broker
        {
            get { return broker; }
            set
            {
                broker = value;
                this.OnPropertyChanged(nameof(Broker));
            }
        }

        private Dictionary<string, IList> filterDic = new Dictionary<string, IList>();
        public Dictionary<string, IList> FilterDic
        {
            get { return filterDic; }
            set
            {
                filterDic = value;
                if (filterDic.ContainsKey("Broker"))
                {
                    IList k = filterDic["Broker"];
                    if (k.Count > 0)
                    {
                        ListBoxItem li = k[0] as ListBoxItem;
                        Broker = ComboSourceUtil.Instance.FindEnum(li.Content.ToString(), "tovenues");
                        ChangeBroker(Broker);
                    }
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// UI Borker
        /// </summary>
        private ChatBrokerFilterView listBox;
        #endregion

        public ChatViewModel(ChatView my)
        {
           this.Init(my);
        }

        public void Init(ChatView my)
        {
            this.myWindow = my;
            listBox = myWindow.FindName("lbDirect") as ChatBrokerFilterView;
            listBox.SetItemSelect(BrokerType.TPSC.ToString());
            this.Broker = BrokerType.TPSC.ToString();
            // listBox.SetEllipseVisibility(BrokerType.TPSC.ToString(), "1");
            // OnClearMessage();

            this.descTimer = new DispatcherTimer();
            this.descTimer.Interval = new TimeSpan(1000);
            this.descTimer.Tick += DescTimer_Tick;
            this.descTimer.Start();

            // 订阅QQ消息
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    BDB.Common.Handler.IM.IMHandler.Instance.Init(OnMsg);
            //    Thread.Sleep(10);
            //    BDB.Common.Handler.IM.IMHandler.Instance.QQUserConfigQuery();
            //    //Common.Handler.Base.BaseHandler.Instance.Init(Instance_OnClear);
            //}), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ChangeBroker(string broker)
        {
            listBox = myWindow.FindName("lbDirect") as ChatBrokerFilterView;
            
            lock (objlock)
            {
                m_Messages.Clear();
                List<QQUserConfigPo> data = DictFullMsg.FindAll(delegate (QQUserConfigPo p) { return p.Venues.ToString() == broker; }).OrderBy(o => o.UpdateTime).ToList();
                RefreshUI(data);
            }
        }
        private void OnMsg(object obj)
        {
            try
            {
                List<QQUserConfigPo> data = (List<QQUserConfigPo>)obj;
                lock (objlock)
                {
                    // 保存记录
                    DictFullMsg.AddRange(data);
                    RefreshUI(data);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                     
            }
        }

        private void RefreshUI(List<QQUserConfigPo> data)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                for (int i = 0; i < data.Count; i++)
                {
                    QQUserConfigPo qcp = data[i];
                    if (this.Broker == qcp.Venues)
                    {
                        string time = qcp.UpdateTime.Substring(0, 19);
                        ChatMessage cm = new ChatMessage() { MsgID = qcp.Version, Broker = qcp.Venues, MsgType = MsgType.Time, Content = time.Substring(11, 8), ErrorMessage = "", TimeStamp = time };
                        ChatMessage tmpcm = new ChatMessage() { MsgID = qcp.Version, Broker = qcp.Venues, MsgType = qcp.MessageType == "B" ? MsgType.Broker : MsgType.Trader, Content = qcp.Text, IsPoint = qcp.Ispoint,ValidUntilTime = qcp.ValidUntilTime, OrdStatus = qcp.OrdStatus, ExecId = qcp.ExecId,  ErrorMessage = string.IsNullOrEmpty(qcp.ErrorMessage) ? "" : qcp.ErrorMessage, TimeStamp = time };
                        if (qcp.Ispoint == "1")
                        {
                            DateTime validTime = DateTimeEx.ServerTime.AddSeconds(int.Parse(qcp.IocTimeOut));
                            tmpcm.ValidUntilTime = validTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }

                        bool bFind = m_Messages.Any<ChatMessage>(p => p.TimeStamp == time);
                        if (!bFind)
                        {
                            m_Messages.Add(cm);
                        }
                        // 如果是点价并且不正常，则查找历史记录修改历史纪录的 OrderStatus
                        if (tmpcm.IsPoint == "1")
                        {
                            if (LstNotShowCountDown.Contains(tmpcm.OrdStatus))
                            {
                                var m  = m_Messages.FirstOrDefault(o =>o.IsPoint == "1" && o.ExecId == tmpcm.ExecId);
                                if (m != null)
                                {
                                    m.OrdStatus = tmpcm.OrdStatus;
                                }
                            }
                        }

                        m_Messages.Add(tmpcm);
                    }
                    else
                    {
                        // 更新Broker上方的 小红点
                        listBox.SetEllipseVisibility(qcp.Venues, "1");
                    }
                }

                SelectedMessage = m_Messages.Count - 1;
                // listBox.UpdateLayout();
            }));
         }

        #region 测试发送接收消息
        /// <summary>
        /// 发送消息测试
        /// </summary>
        private void OnSendMessage()
        {
            //lock (objlock)
            //{
            //    string time = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
            //    List<QQUserConfigPo> data = new List<QQUserConfigPo>();
            //    data.Add(new QQUserConfigPo() { Venues = BrokerType.TPSC.ToString(), Version = 4, ExecId = "0003", MessageType = "B", Text = "Refer 200210 ofr 3.4 1E 仅今天+1", ErrorMessage = "拒绝下单",  Ispoint = "1", UpdateTime = time, OrdStatus = "4", ValidUntilTime = DateTimeEx.ServerTime.AddSeconds(90).ToString("yyyy-MM-dd HH:mm:ss") });

            //    DictFullMsg.AddRange(data);
            //    RefreshUI(data);

            //    // 模拟发送清零信号
            //    //ClearPo nc = new ClearPo() { IsClear = "Y"};
            //    //Instance_OnClear(nc);
            //}
            //BDB.Common.Handler.IM.IMHandler.Instance.QQUserConfigQuery();
            //TxtMessage = "";

            //string time = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
            //ChatMessage tmpcm = new ChatMessage() { MsgID = 0, Broker = BrokerType.TPSC.ToString(), MsgType = MsgType.Trader, Content = "您好,200210.IB 3.5 3000万 卖出,已确定成交200万 still ofr", ExecId = "000", ErrorMessage = "", TimeStamp = time };
            //m_Messages.Add(tmpcm);
            //ChatMessage tmpcm2 = new ChatMessage() { MsgID = 0, Broker = BrokerType.TPSC.ToString(), MsgType = MsgType.Broker, Content = "您好,200210.IB 3.5 3000万 卖出,已确定成交200万 still ofr", ExecId = "000", ErrorMessage = "", TimeStamp = time };
            //m_Messages.Add(tmpcm2);
            //SelectedMessage = m_Messages.Count - 1;

            BaseHandler.Instance.Reset();
        }

        /// <summary>
        /// 清空和增加消息
        /// </summary>
        private void OnClearMessage()
        {
            lock (objlock)
            {
                m_Messages.Clear();
                DictFullMsg.Clear();

                string time = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                List<QQUserConfigPo> data = new List<QQUserConfigPo>();

                data.Add(new QQUserConfigPo() { Venues = BrokerType.TPSC.ToString(), Version = 1, ExecId = "0001", MessageType = "T", Text = "200210 bid 3.5 3000",  ErrorMessage = "发送失败，网络故障", UpdateTime = time });
                data.Add(new QQUserConfigPo() { Venues = BrokerType.TPSC.ToString(), Version = 2, ExecId = "0002", MessageType = "B", Text = "1", ErrorMessage = "发送失败，网络故障", UpdateTime = time });
                data.Add(new QQUserConfigPo() { Venues = BrokerType.TPSC.ToString(), Version = 3, ExecId = "0003", MessageType = "T", Text = "200210 ofr 3.4 1E 仅今天+1", ErrorMessage = "", Ispoint = "1", IocTimeOut = "30", UpdateTime = time, OrdStatus= "0", ValidUntilTime = DateTimeEx.ServerTime.AddSeconds(90).ToString("yyyy-MM-dd HH:mm:ss") });

                DictFullMsg.AddRange(data);
                RefreshUI(data);
            }
        }
        #endregion

        #region 清零信号
        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info($"ChatViewModel Received Clear Message：{obj.IsClear}");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        lock (this.objlock)
                        {
                            DictFullMsg.Clear();
                            Messages.Clear();
                            listBox.SetAllEllipseCollapsed();
                            Thread.Sleep(100);
                            IMHandler.Instance.QQUserConfigQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"ChatViewModel OnClear Error:{ex.Message}");
                    }
                }));
            }
        }
        #endregion

        #region 更新我的订单剩余时间
        private List<string> LstNotShowCountDown = new List<string>() {"4","8","2","C"};
        private void DescTimer_Tick(object sender, EventArgs e)
        {
            // MyObject item = Items.FirstOrDefault(obj => obj.Entity.ID == x);
            foreach (var r in this.Messages)
            {
                if (r.IsPoint == "1" && r.MsgType.ToString() == "Trader")
                {
                    if (!LstNotShowCountDown.Contains(r.OrdStatus))
                    {
                        DateTime transactTime = CommonUtils.ConvertToDateTime(r.ValidUntilTime);
                        TimeSpan diffTime = transactTime - DateTimeEx.ServerTime;
                        int res = Convert.ToInt32(diffTime.TotalSeconds);
                        res--;
                        if (res > 0)
                        {
                            r.TimeRemain = $"{res.ToString()}s";
                        }
                        else
                        {
                            r.TimeRemain = "";
                        }
                    }
                    else
                    {
                        r.TimeRemain = "";
                    }
                }
            }
        }

        #endregion
    }
} 