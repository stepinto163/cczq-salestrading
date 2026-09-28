using com.gateway.log;
using STC.Common.Handler.Position;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Position.Plugin.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Position.Plugin.ViewModel
{
    public class HistoryProcessingPositionStatusViewModel : ViewModelBase
    {
        #region 构造
        public HistoryProcessingPositionStatus myWindow;

        private bool isInit = false;

        private DateTime _begindate;
        public DateTime BeginDate
        {
            get { return _begindate; }
            set
            {
                if (value != _begindate)
                {
                    _begindate = value;
                    this.OnPropertyChanged();

                    if (isInit)
                    {
                        QueryProcessingPositionHistResult(BeginDate, EndDate);
                    }
                }
            }
        }

        private DateTime _enddate;
        public DateTime EndDate
        {
            get { return _enddate; }
            set
            {
                if (value != _enddate)
                {
                    _enddate = value;
                    this.OnPropertyChanged();

                    if (isInit)
                    {
                        QueryProcessingPositionHistResult(BeginDate, EndDate);
                    }
                }
            }
        }

        private bool _manualProcessingEnabled = true;
        public bool ManualProcessingEnabled
        {
            get { return _manualProcessingEnabled; }
            set
            {
                if (value != _manualProcessingEnabled)
                {
                    _manualProcessingEnabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<QueryProcessingRecord> _histprocessingrecord = new ObservableCollection<QueryProcessingRecord>();
        /// <summary>
        /// 历史跑批记录
        /// </summary>
        public ObservableCollection<QueryProcessingRecord> HistProcessingRecord
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


        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }
        private object lockObj = new object();



        private List<string> keyword = new List<string>();
        public List<string> Keyword
        {
            get
            {
                return keyword;
            }
            set
            {
                keyword = value;
                this.OnPropertyChanged(nameof(Keyword));
                this.Mv.View.Refresh();
            }
        }

        public List<string> _lstselectstatus = new List<string>();
        /// <summary>
        /// 获取状态对应的QuoteStatus
        /// </summary>
        public List<string> LstSelectStatus
        {
            get
            {
                return _lstselectstatus;
            }
            set
            {
                if (_lstselectstatus != value)
                {
                    List<string> tmpkeyworklist = new List<string>();

                    for (int i = 0; i < value.Count; i++)
                    {
                        List<string> tmp = GetStatusList(value[i]);
                        tmpkeyworklist = tmpkeyworklist.Union(tmp).ToList();
                    }

                    tmpkeyworklist = tmpkeyworklist.Distinct().ToList();
                    _lstselectstatus = tmpkeyworklist;
                    Keyword = tmpkeyworklist;
                    this.OnPropertyChanged();
                }
            }
        }


        public ICommand ManualProcessingPositionCommand { get { return new ComRelayCommand(OnManualProcessingPositionCommand); } }

        #endregion

        public HistoryProcessingPositionStatusViewModel(HistoryProcessingPositionStatus win)
        {
            this.myWindow = win;
            InitItemData();
        }

        private void InitItemData()
        {


            this.Mv.Source = HistProcessingRecord;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();
            //this.Mv.SortDescriptions.Add(new SortDescription("startDate", ListSortDirection.Descending));
            this.Mv.SortDescriptions.Add(new SortDescription("actionTime", ListSortDirection.Descending));

            BeginDate = DateTime.Now.AddDays(-7);
            EndDate = DateTime.Now.AddDays(0);

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                CalendarDateRange calendarDateRange2 = new CalendarDateRange(EndDate.AddDays(1), DateTime.MaxValue);
                ((HistoryProcessingPositionStatus)myWindow).dpTo.BlackoutDates.Add(calendarDateRange2);
                ((HistoryProcessingPositionStatus)myWindow).dpFrom.BlackoutDates.Add(calendarDateRange2);
            }));

            //查询 查询跑批历史
            QueryProcessingPositionHistResult(BeginDate, EndDate);
            PositionHandler.Instance.SubscribeProcessingRecordStatus(OnSubscribeLoadtMarketExecutionsStatusCallBack);
            isInit = true;
        }


        private void Mv_Filter(object sender, FilterEventArgs e)
        {
            QueryProcessingRecord pbd = e.Item as QueryProcessingRecord;

            if (this.Keyword.Count > 0)
            {
                if (this.Keyword.Contains(pbd.status))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = false;
                }
            }
            else
                e.Accepted = true;

            return;
        }

        #region 查询跑批历史
        private async void QueryProcessingPositionHistResult(DateTime startdate, DateTime datedate)
        {
            try
            {
                lock (lockObj)
                {
                    HistProcessingRecord.Clear();
                }

                QueryProcessingRecordHistReq req = new QueryProcessingRecordHistReq();
                req.startDate = startdate.ToString("yyyy-MM-dd");
                req.endDate = datedate.ToString("yyyy-MM-dd");
                QueryProcessingRecordResult reply = await QueryProcessingPositionHist(req);
                if (reply != null)
                {
                    if (reply.Datas != null)
                    {
                        if (reply.Datas.Count > 0)
                        {
                            List<QueryProcessingRecord> curdaterecord = reply.Datas.OrderByDescending(p => p.startDate).ThenByDescending(p => p.actionTime).ToList();
                            ManualProcessingEnabled = curdaterecord[0].status != "1";
                        }

                        foreach (QueryProcessingRecord pbm in reply.Datas)
                        {
                            QueryProcessingRecord newpdd = new QueryProcessingRecord();
                            newpdd.id = pbm.id;
                            newpdd.startDate = pbm.startDate;
                            newpdd.status = pbm.status;
                            newpdd.actionTime = pbm.actionTime;
                            newpdd.actionUser = pbm.actionUser;
                            newpdd.actionUserName = pbm.actionUserName;
                            newpdd.createTime = pbm.createTime;
                            newpdd.endDate = pbm.endDate;
                            newpdd.endTime = pbm.endTime;
                            newpdd.comment = pbm.comment ?? "";
                            newpdd.info1 = pbm.info1 ?? "";

                            HistProcessingRecord.Add(newpdd);
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Mv.View.Refresh();
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 跑批结果回调
        /// </summary>
        /// <param name="callback"></param>
        private void OnSubscribeLoadtMarketExecutionsStatusCallBack(QueryProcessingRecordResult callback)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                List<QueryProcessingRecord> curdaterecord = callback.Datas;
                if (curdaterecord != null)
                {
                    if (curdaterecord.Count > 0)
                        ManualProcessingEnabled = curdaterecord[0].status != "1";

                    foreach (QueryProcessingRecord pbm in curdaterecord)
                    {
                        QueryProcessingRecord m2 = HistProcessingRecord.FirstOrDefault(o => o.id == pbm.id);
                        if (m2 != null)
                        {
                            m2.startDate = pbm.startDate;
                            m2.status = pbm.status;
                            m2.actionTime = pbm.actionTime;
                            m2.actionUser = pbm.actionUser;
                            m2.actionUserName = pbm.actionUserName;
                            m2.createTime = pbm.createTime;
                            m2.endDate = pbm.endDate;
                            m2.endTime = pbm.endTime;
                            m2.comment = pbm.comment ?? "";
                            m2.info1 = pbm.info1 ?? "";
                        }
                        else
                        {
                            QueryProcessingRecord newpdd = new QueryProcessingRecord();
                            newpdd.id = pbm.id;
                            newpdd.startDate = pbm.startDate;
                            newpdd.status = pbm.status;
                            newpdd.actionTime = pbm.actionTime;
                            newpdd.actionUser = pbm.actionUser;
                            newpdd.actionUserName = pbm.actionUserName;
                            newpdd.createTime = pbm.createTime;
                            newpdd.endDate = pbm.endDate;
                            newpdd.endTime = pbm.endTime;
                            newpdd.comment = pbm.comment ?? "";
                            newpdd.info1 = pbm.info1 ?? "";

                            lock (lockObj)
                            {
                                HistProcessingRecord.Add(newpdd);
                            }
                        }
                    }

                    //((HistoryProcessingPositionStatus)myWindow).ShowEMSMask(false);
                }
            }));
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
        /// 手工跑批计算
        /// </summary>
        private void OnManualProcessingPositionCommand()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                ManualProcessingPositionView Mppv = new ManualProcessingPositionView();
                Mppv.Show();
            }));
        }

        /// <summary>
        /// 获取查询用的过滤状态列表
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private List<string> GetStatusList(string status)
        {
            ///All 0==> 无
            List<string> res = new List<string>();
            switch (status)
            {
                case "0":
                    //All 0
                    break;
                case "1":
                    res.Add("1");
                    break;
                case "2":
                    res.Add("2");
                    break;
                case "3":
                    res.Add("3");
                    break;
            }
            return res;
        }
    }
}
