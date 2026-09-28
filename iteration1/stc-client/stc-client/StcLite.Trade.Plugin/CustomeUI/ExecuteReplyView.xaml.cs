using Ats.Foundation.Message;
using BDB.Trade.Plugin.CustomUI;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BDB.Trade.Plugin.CustomeUI
{
    /// <summary>
    /// ExecuteReplyView.xaml 的交互逻辑
    /// </summary>
    public partial class ExecuteReplyView : Window
    {
        private ExecuteReplyViewModel _executeReplyViewModel = null;

        public ExecuteReplyView()
        {
            InitializeComponent();

            this.Unloaded += ExecuteReplyView_Unloaded;
            this.Closed += ExecuteReplyView_Closed;
        }

        private void ExecuteReplyView_Closed(object sender, EventArgs e)
        {
            if (_executeReplyViewModel != null)
                _executeReplyViewModel.Clear();

            this.Unloaded -= ExecuteReplyView_Unloaded;
            this.Closed -= ExecuteReplyView_Closed;
        }

        private void ExecuteReplyView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_executeReplyViewModel != null)
                _executeReplyViewModel.Clear();

            this.Unloaded -= ExecuteReplyView_Unloaded;
            this.Closed -= ExecuteReplyView_Closed;
        }

        public ExecuteReplyView(MsgPo msgPo)
        {
            InitializeComponent();
            
            this.Icon = LocalEnv.ServerInfo.AppImage;

            _executeReplyViewModel = new ExecuteReplyViewModel();
            this.DataContext = _executeReplyViewModel;
            ExecutionReport er = null;
            if (!string.IsNullOrEmpty(msgPo.Detail))
            {
                er = MsgHelper.Deserialize<ExecutionReport>(msgPo.Detail);
            }
            _executeReplyViewModel.Init(this, er, msgPo.ContentType);
        }

        public ExecuteReplyView(MsgPo msgPo, bool isLook)
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            _executeReplyViewModel = new ExecuteReplyViewModel();
            this.DataContext = _executeReplyViewModel;
            ExecutionReport er = null;
            if (!string.IsNullOrEmpty(msgPo.Detail))
            {
                er = MsgHelper.Deserialize<ExecutionReport>(msgPo.Detail);
                //if (!string.IsNullOrEmpty(er.TransactTime))
                //{
                //    string[] arr = er.TransactTime.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                //    if (arr != null && arr.Length > 1)
                //        er.TransactTime = string.Format("{0} {1}", arr[0], msgPo.UpdateTime);
                //}
            }
            _executeReplyViewModel.Init(this, er, string.Empty);
        }

        public ExecuteReplyView(ExecutionReport er)
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            _executeReplyViewModel = new ExecuteReplyViewModel();
            this.DataContext = _executeReplyViewModel;
            _executeReplyViewModel.Init(this, er, er.Venues);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //cbIDB.IsDropDownOpen = !cbIDB.IsDropDownOpen;
        }
    }
}
