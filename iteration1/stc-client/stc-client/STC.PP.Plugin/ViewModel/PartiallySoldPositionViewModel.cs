using com.gateway.log;
using STC.Common.Handler.Position;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.PP.Plugin.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace STC.PP.Plugin.ViewModel
{
    public class PartiallySoldPositionViewModel : ViewModelBase
    {
        #region 构造
        public Window myWindow;
        public ICommand CloseWindowCommand { get { return new ComRelayCommand(OnCloseWindowCommand); } }
        public ICommand CancelCommand { get { return new ComRelayCommand(OnCancelCommand); } }
        public ICommand OkCommand { get { return new ComRelayCommand(OnOkCommand); } }
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

        /// <summary>
        /// 控制量的按钮是否可用
        /// </summary>
        public bool isbtnenable = true;
        public bool IsBtnEnable
        {
            get { return isbtnenable; }
            set
            {
                isbtnenable = value;
                this.OnPropertyChanged("IsBtnEnable");
            }
        }

        /// <summary>
        /// 量是否只读
        /// </summary>
        public bool isreadonly = false;
        public bool IsReadOnly
        {
            get { return isreadonly; }
            set
            {
                isreadonly = value;
                IsBtnEnable = !value;
                this.OnPropertyChanged("IsReadOnly");
            }
        }


        // 显示用
        private string amount;
        public string Amount
        {
            get { return amount; }
            set
            {
                if (value != null && !string.IsNullOrEmpty(value))
                {
                    amount = value;
                }
                else
                {
                    amount = string.Empty;
                }
                this.OnPropertyChanged();
            }
        }

        private Brush borderBH;
        public Brush BorderBH
        {
            get { return borderBH; }
            set
            {
                if (value != null)
                {
                    borderBH = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Brush qtyBorderBH;
        public Brush QtyBorderBH
        {
            get { return qtyBorderBH; }
            set
            {
                if (value != null)
                {
                    qtyBorderBH = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        public PartiallySoldPositionViewModel()
        {

        }

        public PartiallySoldPositionViewModel(Window win)
        {
            this.myWindow = win;
            InitItemData();
        }

        private void InitItemData()
        {
            BorderBH = (Brush)myWindow.FindResource("ColorBrush.Blue8");
        }


        #region 命令

        public void onWindowActivated(int activeType)
        {
            if (activeType == 1) //Window_Deactivated
            {
                BorderBrush = (Brush)Application.Current.MainWindow.FindResource("LostFocusBorderBrush");
            }
            else if (activeType == 2) //Window_Activated
            {
                BorderBrush = (Brush)Application.Current.MainWindow.FindResource("GetFocusBorderBrush");
            }
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void OnCloseWindowCommand()
        {
            myWindow.DialogResult = false;
            myWindow.Close();
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void OnCancelCommand()
        {
            myWindow.DialogResult = false;
            myWindow.Close();
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        private void OnOkCommand()
        {
            myWindow.DialogResult = true;
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
