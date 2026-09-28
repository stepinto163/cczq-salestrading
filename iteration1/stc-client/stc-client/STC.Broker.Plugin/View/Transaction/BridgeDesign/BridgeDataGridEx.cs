
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using STC.Common.Model.PT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static STC.Broker.Plugin.Utils.CommonEnum;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    /// <summary>
    /// 自定义网格控件，用于放加桥成交单
    /// </summary>
    public class BridgeDataGridEx : DataGrid
    {
        private UserBridge _userBridge;
        /// <summary>
        /// 当前桥信息
        /// </summary>
        public UserBridge Bridge
        {
            get
            {
                return _userBridge;
            }
            set
            {
                _userBridge = value;
            }
        }

        static BridgeDataGridEx()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(BridgeDataGridEx),
                new CommandBinding(ApplicationCommands.Copy,
                    new ExecutedRoutedEventHandler(OnExecutedCopy),
                    new CanExecuteRoutedEventHandler(OnCanExecuteCopy)));
        }
        private static void OnCanExecuteCopy(object target, CanExecuteRoutedEventArgs args)
        {
            ((BridgeDataGridEx)target).OnCanExecuteCopy(args);
        }
        protected new void OnCanExecuteCopy(CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = ClipboardCopyMode != DataGridClipboardCopyMode.None && this.SelectedCells.Count > 0;
            args.Handled = true;
        }

        private static void OnExecutedCopy(object target, ExecutedRoutedEventArgs args)
        {
            //V2.3需求调整：2）过桥窗口中，两种复制的方法互换，选中或按Ctrl+C直接复制时带单号，右键菜单中复制出的结果是“不带单号复制”
            ((BridgeDataGridEx)target).OnExecutedCopy(null);
        }

        /// <summary>
        /// 带单号赋值
        /// </summary>
        /// <param name="args">null：带单号复制，否则不带单号复制</param>
        public  void OnExecutedCopy(BrigeCopyType brigeCopyType)
        {
            if (this.SelectedCells != null && this.SelectedCells.Count > 0)
            {
                BridgeHelper.CopyBridgeInfoOrderTxt(SelectedCells, _userBridge, brigeCopyType);
            }
        }

    }
}
