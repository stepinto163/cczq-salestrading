using com.gateway.log;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Position;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Position.Plugin.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static log4net.Appender.RollingFileAppender;


namespace STC.Position.Plugin.ViewModel
{
    public class ManualProcessingPositionViewModel : ViewModelBase
    {
        #region 构造
        public Window myWindow;
        public ICommand CloseWindowCommand { get { return new ComRelayCommand(OnCloseWindowCommand); } }
        public ICommand CancelCommand { get { return new ComRelayCommand(OnCancelCommand); } }
        public ICommand OkCommand { get { return new ComRelayCommand(OnOkCommand); } }
        public ICommand ForceRunCommand { get { return new ComRelayCommand(OnForceRunCommand); } }

        private bool _IsCanRun = true;
        public bool IsCanRun
        {
            get 
            {
                return _IsCanRun;
            }
            set 
            {
                _IsCanRun = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime _selecteddate;
        public DateTime SelectedDate
        {
            get { return _selecteddate; }
            set
            {
                
                if (value != null && value != _selecteddate)
                {
                    _selecteddate = value;
                    StatusCode = "";
                }

                ////StatusMsg 0-未执行 1-执行中 2-执行成功 3-执行失败
                //IsCanRun = string.IsNullOrEmpty(StatusCode) ? true : StatusCode == "1" ? false : true;

                this.OnPropertyChanged();
            }
        }


        private DateTime _begindate;
        public DateTime BeginDate
        {
            get { return _begindate; }
            set
            {
                _begindate = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime _enddate;
        public DateTime EndDate
        {
            get { return _enddate; }
            set
            {
                _enddate = value;
                this.OnPropertyChanged();
            }
        }

        private string _statuscode;
        /// <summary>
        /// 本次结转状态
        /// </summary>
        public string StatusCode
        {
            get
            {
                return _statuscode;
            }
            set
            {
                _statuscode = value;
                this.OnPropertyChanged();
            }
        }

        private string _laststatuscode;
        /// <summary>
        /// 上一次结转状态
        /// </summary>
        public string LastStatusCode
        {
            get
            {
                return _laststatuscode;
            }
            set
            {
                _laststatuscode = value;
                this.OnPropertyChanged();
            }
        }


        private string _lastStatusMsg;
        /// <summary>
        /// 最后状态消息
        /// </summary>
        public string LastStatusMsg
        {
            get
            {
                return _lastStatusMsg;
            }
            set
            {
                _lastStatusMsg = value;
                this.OnPropertyChanged();
            }
        }
        

        private List<QueryProcessingRecord> _histprocessingrecord = new List<QueryProcessingRecord>();
        /// <summary>
        /// 历史跑批记录
        /// </summary>
        public List<QueryProcessingRecord> HistProcessingRecord
        {
            get
            {
                return _histprocessingrecord;
            }
            set
            {
                _histprocessingrecord = value;
                this.OnPropertyChanged();
            }
        }

        private object lockObj = new object();

       
        public bool IsInit = false;

        private string iconUrl;
        /// <summary>
        /// 图标
        /// </summary>
        public string IconUrl
        {
            get
            {
                return iconUrl;
            }
            set
            {
                iconUrl = value;
                this.OnPropertyChanged(nameof(IconUrl));
            }
        }
        #endregion

        public ManualProcessingPositionViewModel()
        {

        }

        public ManualProcessingPositionViewModel(Window win)
        {
            this.myWindow = win;
            InitItemData();
        }

        private void InitItemData()
        {
            //查询 当前跑批任务
            //QueryProcessingRecordResult();
            //查询 查询跑批历史
            BeginDate = DateTime.Now.AddDays(-30);
            EndDate = DateTime.Now.AddDays(0);
            SelectedDate = EndDate;
            QueryProcessingPositionHistResult(BeginDate, EndDate, DateTime.Now.AddDays(0));

            PositionHandler.Instance.SubscribeProcessingRecordStatus(OnSubscribeLoadtMarketExecutionsStatusCallBack);
        }

        #region 命令
        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void OnCloseWindowCommand()
        {
            PositionHandler.Instance.UnSubscribeProcessingRecordStatus(OnSubscribeLoadtMarketExecutionsStatusCallBack);
            myWindow.Close();
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void OnCancelCommand()
        {
            PositionHandler.Instance.UnSubscribeProcessingRecordStatus(OnSubscribeLoadtMarketExecutionsStatusCallBack);
            myWindow.Close();
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        private void OnOkCommand()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    string manualdate = SelectedDate.ToString("yyyy-MM-dd") ?? "";
                    if (string.IsNullOrEmpty(manualdate))
                    {
                        showMessage("请输入开始日期！");
                        return;
                    }

                    IsCanRun = false;
                    StatusCode = "执行中";

                    ((ManualProcessingPositionView)myWindow).ShowEMSMask(true);
                    CommmitManualProcessing(manualdate);
                }));
            });
        }


        #region 重新跑批执行
        /// <summary>
        /// 重新跑批执行
        /// </summary>
        private async void CommmitManualProcessing(string manualdate)
        {
            try
            {
                //Show Wait ...
                ((ManualProcessingPositionView)myWindow).ShowEMSMask(false);

                //查询当前跑批任务
                QueryProcessingRecordResult curreply = null;
                try
                {
                    curreply = await QueryProcessingRecordResultTask();
                    if (curreply.Datas != null)
                    {
                        List<QueryProcessingRecord> curdaterecord = curreply.Datas.OrderByDescending(o => o.actionTime).ToList();
                        //StatusMsg 0-未执行 1-执行中 2-执行成功 3-执行失败
                        string statuscode = curdaterecord.Count > 0 ? curdaterecord[0].status : "";
                        if (statuscode == "1")
                        {
                            IsCanRun = true;
                            StatusCode = $"{curdaterecord[0].actionUserName} 正在结转！请稍后重试... \r\n执行时间:{curdaterecord[0].actionTime}";
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"查询当前结转任务失败：{ex.Message}");
                }


                //开始跑批
                ManualProcessingPositionReq req = new ManualProcessingPositionReq();
                req.startDate = manualdate;
                PResult reply = await ManualProcessingPositionTask(req);

                if (reply != null && reply.IsSuccess())
                {
                    StatusCode = "执行中";
                }
                else
                {
                    if (reply != null)
                    {
                        StatusCode = $"{reply.Msg}";
                        ((ManualProcessingPositionView)myWindow).ShowEMSMask(false);
                    }
                    else
                    {
                        StatusCode = $"执行中";
                        ((ManualProcessingPositionView)myWindow).ShowEMSMask(false);
                        CustomMessageBox.Info($"结转仍在后台执行中，请耐心等待结转完成！!\r\n你可以随时关闭窗口！", "提示", myWindow);
                    }
                }
            }
            catch (Exception ex)
            {
                IsCanRun = true;
                LogHelper.Error(ex);
                StatusCode = $"结转失败! {ex.Message}";
                ((ManualProcessingPositionView)myWindow).ShowEMSMask(false);
                CustomMessageBox.Error($"结转失败: {ex.Message}", "提示", myWindow);
            }
        }

        /// <summary>
        /// 跑批结果回调
        /// </summary>
        /// <param name="callback"></param>
        private void OnSubscribeLoadtMarketExecutionsStatusCallBack(QueryProcessingRecordResult callback)
        {
            List<QueryProcessingRecord> curdaterecord = callback.Datas;
            if (curdaterecord != null)
            {
                // StatusMsg 0 - 未执行 1 - 执行中 2 - 执行成功 3 - 执行失败
                string statuscode = curdaterecord.Count > 0 ? curdaterecord[0].status : "";
                StatusCode = statuscode;
                if (statuscode != "1")
                {
                    IsCanRun = true;
                    StatusCode = statuscode;
                    LastStatusCode = statuscode;
                    LastStatusMsg = $"最后一次结转的开始日期：{curdaterecord[0].startDate}\r\n执行时间时间是：{DateTime.Parse(curdaterecord[0].actionTime).ToString("yyyy-MM-dd HH:mm:ss")}";
                    ((ManualProcessingPositionView)myWindow).ShowEMSMask(false);
                }
            }
        }

            
        /// <summary>
        /// 重新跑批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<PResult> ManualProcessingPositionTask(ManualProcessingPositionReq req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.ManualProcessingPosition(req);
            });

            return reply;
        }
        #endregion

        #region 当前跑批任务

        /// <summary>
        /// 当前跑批任务进程
        /// </summary>
        /// <returns></returns>
        public async Task<QueryProcessingRecordResult> QueryProcessingRecordResultTask()
        {
            QueryProcessingRecordResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.QueryProcessingPosition();
            });

            return reply;
        }
        #endregion


        #region 查询跑批历史
        private async void QueryProcessingPositionHistResult(DateTime startdate, DateTime datedate, DateTime curdate)
        {
            try
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    CalendarDateRange calendarDateRange1 = new CalendarDateRange(DateTime.MinValue, BeginDate.AddDays(-1));
                    ((ManualProcessingPositionView)myWindow).dpFrom.BlackoutDates.Add(calendarDateRange1);
                    CalendarDateRange calendarDateRange2 = new CalendarDateRange(EndDate.AddDays(1), DateTime.MaxValue);
                    ((ManualProcessingPositionView)myWindow).dpFrom.BlackoutDates.Add(calendarDateRange2);
                }));

                QueryProcessingRecordHistReq req = new QueryProcessingRecordHistReq();
                req.startDate = startdate.ToString("yyyy-MM-dd");
                req.endDate = datedate.ToString("yyyy-MM-dd");
                QueryProcessingRecordResult reply = await QueryProcessingPositionHist(req);
                if (reply != null)
                {
                    if (reply.Datas != null)
                    {
                        HistProcessingRecord = reply.Datas.Distinct().OrderByDescending(o=>o.actionTime).ToList();
                    }
                }


                IsInit = true;

                if (curdate != null)
                    SelectedDate = curdate;

                if (HistProcessingRecord.Count > 0)
                {
                    LastStatusCode = HistProcessingRecord[0].status ?? "";
                    LastStatusMsg = $"最后一次结转的开始日期：{HistProcessingRecord[0].startDate}\r\n执行时间是：{DateTime.Parse(HistProcessingRecord[0].actionTime).ToString("yyyy-MM-dd HH:mm:ss")}";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            
        }


        /// <summary>
        /// 查询跑批历史
        /// </summary>
        /// <returns></returns>
        public async Task<QueryProcessingRecordResult> QueryProcessingPositionHist(QueryProcessingRecordHistReq req)
        {
            QueryProcessingRecordResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.QueryProcessingPositionHist(req);
            });

            return reply;
        }
        #endregion

        /// <summary>
        /// 强制停止
        /// </summary>
        private void OnForceRunCommand()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        StatusCode = "强制停止";
                        IsCanRun = true;
                        ((ManualProcessingPositionView)myWindow).ShowEMSMask(false);

                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        #endregion

       
        #region 显示消息
        /// <summary>
        /// 提示Warn消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = this.myWindow;
                me.ShowDialog();
            }));
        }

        /// <summary>
        /// 提示成功消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showSuccessMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                SuccessBoxView me = new SuccessBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = this.myWindow;
                me.ShowDialog();
            }));
        }

        #endregion
    }

}
