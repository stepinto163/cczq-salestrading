using STC.Common.Model.Drp;
using STC.Common.Utils;
using STC.Common.View.Drp.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.ViewModel.Drp
{
    public class MMBreakAlarmReminderViewModel : ViewModelBase
    {
        #region 定义
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

        private string title = "双边待办" +
            "";
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
        private string detail;
        /// <summary>
        /// 原始消息
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

        private string updateTime;
        /// <summary>
        /// 原始消息
        /// </summary>
        public string UpdateTime
        {
            get
            {
                return updateTime;
            }
            set
            {
                if (updateTime != value)
                {
                    updateTime = value;
                    this.OnPropertyChanged(nameof(UpdateTime));
                }
            }
        }

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
        public ICommand CancelCommand { get; set; }
        public ICommand TacklesCommand { get; set; }
        public ICommand CloseSelfCommand { get; set; }
        public MMBreakAlarmReminder ParentWindow;

        private DataView _DataDV;
        public DataView DataDV
        {
            get
            {                
                if (_DataDV == null)
                {
                    _DataDV = new DataTable().DefaultView;
                    _DataDV.Table.Columns.Add("MsgTitle", typeof(string));
                    _DataDV.Table.Columns.Add("MsgContent", typeof(string));
                }
                return _DataDV;
            }
        }
        #endregion
        public MMBreakAlarmReminderViewModel() 
        {
            this.CancelCommand = new ComRelayCommand(OnCancelCommand);
            this.TacklesCommand = new ComRelayCommand(OnTacklesCommand);
            this.CloseSelfCommand = new ComRelayCommand(OnCloseSelfCommand);

        }

        private void OnTacklesCommand()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                string key = $"MMBreakAlarmReminder-TradeBook";
                MsgPo msgPo = new MsgPo();
                msgPo.Id = this.id;
                msgPo.Detail = this.detail;
                MMAgencyMsgView msgView = new MMAgencyMsgView(msgPo);
                WindowManager.Instance.OpenOnlyOneWindow(key, msgView);
            }));
        }

        private void OnCancelCommand()
        {
            OnCloseSelfCommand();
        }

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
                    catch (Exception ex)
                    {

                    }
                }
            });
        }
    }
}
