using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Quant;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.quant.plugin.View.HomePage;
using STC.quant.plugin.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.quant.plugin.View.Component
{
    /// <summary>
    /// GroupManagerControl.xaml 的交互逻辑
    /// </summary>
    public partial class GroupManagerControl : WindowBase
    {
        public AlterParamViewModel viewModel = AlterParamViewModel.Instance;

        public GroupManagerControl()
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.Loaded += GroupManagerControl_Loaded;
            GroupView.MouseDoubleClick += GroupView_MouseDoubleClick;
        }


        public void setDefaultParam(string defaultTag)
        {
            viewModel.defaultTag = defaultTag;
        }
        private void GroupManagerControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.initGroup();
        }

        private void GroupView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            viewModel.UnsubscribeAppMsg();
            viewModel.clearGroup();
        }
        bool isEdit = false;
        //双击修改组
        private void GroupView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point aP = e.GetPosition(GroupView);//返回鼠标指针相对于指定元素的位置。
            IInputElement obj = GroupView.InputHitTest(aP);//返回坐标上的当前元素中的输入元素
            DependencyObject target = obj as DependencyObject;
            DependencyObject target2 = obj as DependencyObject;
            while (target != null)
            {
                if (target is DataGridRow)
                {
                    target2 = target;
                    // MessageBox.Show("这就是我双击的行！");
                }
                target = VisualTreeHelper.GetParent(target);
            }

            viewModel.EditGroupNameClick();
            TextBox chb = CommonUtils.FindVisualChild<TextBox>(target2) as TextBox;
            if (null != chb)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                {
                    chb.Focus();
                    chb.Select(chb.Text.Length, 0);
                    isEdit = true;
                }));
            }
        }

        //回车修改组入库
        private void TxGroupName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                viewModel.EditGroupNameOKClick();
                isEdit = false;
            }
        }

        private void TxGroupName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (isEdit)
            {
                viewModel.EditGroupNameOKClick();
                isEdit = false;
            }
        }


        private void WindowBase_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isEdit)
            {
                viewModel.EditGroupNameOKClick();
                isEdit = false;
            }
        }
    }
}
