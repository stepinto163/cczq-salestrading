using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace STC.Execute.Plugin.Command
{
    /// <summary>
    /// 命令执行器
    /// </summary>
    public class ExecuteCommandAction : TargetedTriggerAction<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ExecuteCommandAction), new FrameworkPropertyMetadata(null));
        /// <summary> 命令
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(ExecuteCommandAction), new FrameworkPropertyMetadata(null));
        /// <summary> 参数
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }


        /// <summary>由给定的路由事件触发，参数是事件的发送方
        /// </summary>
        protected override void Invoke(object parameter)
        {
            if (Command == null) return;
            if (Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
