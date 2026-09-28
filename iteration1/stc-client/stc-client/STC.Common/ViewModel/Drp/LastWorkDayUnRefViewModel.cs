using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Inner;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Inner;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Drp.Component;
using STC.Common.ViewModel.Drp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.Common.ViewModel.Drp
{
    /// <summary>
    /// 最后工作日UnRef界面
    /// </summary>
    public class LastWorkDayUnRefViewModel : ViewModelBase
    {
        #region 构造
        public bool IsSetFocus = false;

        private string id;
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                {
                    id = value;
                    this.OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string title = "UNREF提醒";
        /// <summary>
        /// 自定义标题
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (title != value)
                {
                    title = value;
                    this.OnPropertyChanged(nameof(Title));
                }
            }
        }

        private string message;
        /// <summary>
        /// 原始消息
        /// </summary>
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (message != value)
                {
                    message = value;
                    this.OnPropertyChanged(nameof(Message));
                }
            }
        }

        private string detail;
        /// <summary>
        /// DRPDetail
        /// </summary>
        public string Detail
        {
            get
            {
                return detail;
            }
            set
            {
                if (detail != value)
                {
                    detail = value;
                    this.OnPropertyChanged(nameof(Detail));
                }
            }
        }

        private string contenttype;
        /// <summary>
        /// 消息类型
        /// </summary>
        public string ContentType
        {
            get
            {
                return contenttype;
            }
            set
            {
                if (contenttype != value)
                {
                    contenttype = value;
                    this.OnPropertyChanged(nameof(ContentType));
                }
            }
        }


        private bool isshow;
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow
        {
            get
            {
                return isshow;
            }
            set
            {
                if (isshow != value)
                {
                    isshow = value;
                    this.OnPropertyChanged(nameof(IsShow));
                }
            }
        }

        private string delflag;
        /// <summary>
        /// 是否删除
        /// </summary>
        public string DelFlag
        {
            get
            {
                return delflag;
            }
            set
            {
                if (delflag != value)
                {
                    delflag = value;
                    this.OnPropertyChanged(nameof(DelFlag));
                }
            }
        }

        private Brush alertsignpointamountforground;
        /// <summary>
        /// 建议量>回价量 警示颜色
        /// </summary>
        public Brush AlertsignPointAmountforground
        {
            get { return alertsignpointamountforground; }
            set
            {
                alertsignpointamountforground = value;
                this.OnPropertyChanged();
            }
        }


        private string transacttime;
        public string TransactTime { get { return transacttime; } set { transacttime = value; this.OnPropertyChanged(); } }


        private string orderId;
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get { return orderId; } set { orderId = value; this.OnPropertyChanged(); } }


        private string name;
        public string UserName { get { return name; } set { name = value; this.OnPropertyChanged(); } }

        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
        /// <summary>
        /// 边框颜色，激活和失去焦点后变化
        /// </summary>
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                    this.OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }

        public ICommand CloseSelfCommand { get; set; }
        public ICommand RefuseCommand { get; set; }
        public ICommand CommitCommand { get; set; }
        public ICommand ReViewCommand { get; set; }

        public LastWorkDayUnRefView ParentWindow;

        private LastWorkDayUnRefMsgModel _msgModel;
        /// <summary>
        /// 原始消息
        /// </summary>
        public LastWorkDayUnRefMsgModel MsgModel
        {
            get { return _msgModel; }
            set
            {
                _msgModel = value;
                this.OnPropertyChanged(nameof(MsgModel));
            }
        }


        private UnRefContentModel _contentmodel;

        public UnRefContentModel ContentModel
        {
            get { return _contentmodel; }
            set
            {
                _contentmodel = value;
                this.Title = value.title;
                this.Message = value.content;
                this.OnPropertyChanged(nameof(MsgModel));
            }
        }


        public LastWorkDayUnRefViewModel()
        {
            this.ReViewCommand = new ComRelayCommand(OnReViewCommand);
            this.RefuseCommand = new ComRelayCommand(OnRefuseCommand);
            this.CommitCommand = new ComRelayCommand(OnCommitCommand);
        }

        #endregion

        #region Command
        /// <summary>
        /// 忽略
        /// </summary>
        public void OnRefuseCommand()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    TBatchMsgReq batchMsgReq = new TBatchMsgReq();
                    batchMsgReq.delflag = "1";
                    batchMsgReq.readflag = "1";
                    batchMsgReq.id = new List<string>();

                    batchMsgReq.id.Add(this.Id);
                    
                    PResult result = AdminHandler.Instance.BatchUpdateSysMsg(batchMsgReq);

                    if (result != null && result.IsSuccess())
                    {
                        MsgModel.readflag = "1";
                        MsgModel.delflag = "1";
                        string msg = Newtonsoft.Json.JsonConvert.SerializeObject(MsgModel);
                        TTbookSystemMessage tc = Newtonsoft.Json.JsonConvert.DeserializeObject<TTbookSystemMessage>(msg);
                        tc.closeby = LocalEnv.UserInfo.UserName;
                        tc.delflag = "1";
                        List<TTbookSystemMessage> LstSM = new List<TTbookSystemMessage>() { tc };
                       
                        //发送模拟admin消息
                        Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "AppSimulateMsg", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(LstSM) });
                    }
                    else
                    {
                        //CustomMessageBox.Warning($"忽略失败, 原因：{result.Msg}");
                    }


                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
                finally
                {
                    OnCloseSelfCommand();
                }
            });
        }

        /// <summary>
        /// 批量恢复
        /// </summary>
        private void OnCommitCommand()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    Model.Table.UnRefHistoryIntentionOrderReq innerOrderReq = new Model.Table.UnRefHistoryIntentionOrderReq();
                    innerOrderReq.username = LocalEnv.UserInfo.UserName;
                    innerOrderReq.type = ContentModel.type.ToString();

                    PResult result = TableHandler.Instance.UnRefHistoryIntentionOrder(innerOrderReq);
                    if (result.IsSuccess())
                    {
                        OnCloseSelfCommand();
                    }
                    else
                    {
                        CustomMessageBox.Warning($"批量恢复失败, 原因：{result.Msg}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        /// <summary>
        /// 查看
        /// </summary>
        private void OnReViewCommand()
        {
            //to do...
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvr_Focus_InnerHistoryRequireView", Msg =  string.Empty });
        }

        /// <summary>
        /// 关闭
        /// </summary>
        private void OnCloseSelfCommand()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ParentWindow != null)
                {
                    try
                    {
                        ParentWindow.Close();
                    }
                    catch(Exception ex)
                    { 
                    
                    }
                }
            });
        }

        #endregion


        #region Dispose
        ~LastWorkDayUnRefViewModel()
        {
            LogHandler.Info("LastWorkDayUnRefViewModel Dispose");
            this.Dispose();
        }
        #endregion
    }

}
