using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.PP.Plugin.View
{
    /// <summary>
    /// 双命令按钮控件 - 包含主区域和辅助区域（齿轮图标），各自执行独立的命令
    /// </summary>
    public class DualCommandButton : Control
    {
        static DualCommandButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DualCommandButton), new FrameworkPropertyMetadata(typeof(DualCommandButton)));
        }


        #region 依赖属性

        /// <summary>
        /// 主区域命令
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(DualCommandButton), new PropertyMetadata(null, OnCommandChanged));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// 主区域命令参数
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(DualCommandButton), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// 辅助区域命令（齿轮按钮）
        /// </summary>
        public static readonly DependencyProperty SecondaryCommandProperty = DependencyProperty.Register("SecondaryCommand", typeof(ICommand), typeof(DualCommandButton), new PropertyMetadata(null));

        public ICommand SecondaryCommand
        {
            get { return (ICommand)GetValue(SecondaryCommandProperty); }
            set { SetValue(SecondaryCommandProperty, value); }
        }

        /// <summary>
        /// 辅助区域命令参数
        /// </summary>
        public static readonly DependencyProperty SecondaryCommandParameterProperty = DependencyProperty.Register("SecondaryCommandParameter", typeof(object), typeof(DualCommandButton), new PropertyMetadata(null));

        public object SecondaryCommandParameter
        {
            get { return GetValue(SecondaryCommandParameterProperty); }
            set { SetValue(SecondaryCommandParameterProperty, value); }
        }

        /// <summary>
        /// 按钮内容
        /// </summary>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(DualCommandButton), new PropertyMetadata(null));

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// 辅助区域工具提示
        /// </summary>
        public static readonly DependencyProperty SecondaryToolTipProperty = DependencyProperty.Register("SecondaryToolTip", typeof(object), typeof(DualCommandButton), new PropertyMetadata("设置"));

        public object SecondaryToolTip
        {
            get { return GetValue(SecondaryToolTipProperty); }
            set { SetValue(SecondaryToolTipProperty, value); }
        }

        /// <summary>
        /// 主区域是否启用（只读，由Command的CanExecute决定）
        /// </summary>
        private static readonly DependencyPropertyKey IsMainEnabledPropertyKey = DependencyProperty.RegisterReadOnly("IsMainEnabled", typeof(bool), typeof(DualCommandButton), new PropertyMetadata(true));

        public static readonly DependencyProperty IsMainEnabledProperty = IsMainEnabledPropertyKey.DependencyProperty;

        public bool IsMainEnabled
        {
            get { return (bool)GetValue(IsMainEnabledProperty); }
            private set { SetValue(IsMainEnabledPropertyKey, value); }
        }

        #endregion

        #region 命令处理

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DualCommandButton)d;

            if (e.OldValue is ICommand)
            {
                ICommand oldCommand = (ICommand)e.OldValue;
                oldCommand.CanExecuteChanged -= control.OnCommandCanExecuteChanged;
            }

            if (e.NewValue is ICommand)
            {
                ICommand newCommand = (ICommand)e.NewValue;
                newCommand.CanExecuteChanged += control.OnCommandCanExecuteChanged;
                control.UpdateCanExecute();
            }
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            IsMainEnabled = Command != null ? Command.CanExecute(CommandParameter) : true;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 获取模板中的主区域和辅助区域
            Border mainArea = GetTemplateChild("PART_MainArea") as Border;
            if (mainArea != null)
            {
                mainArea.MouseLeftButtonDown -= OnMainAreaClick;
                mainArea.MouseLeftButtonDown += OnMainAreaClick;
            }

            Border secondaryArea = GetTemplateChild("PART_SecondaryArea") as Border;
            if (secondaryArea != null)
            {
                secondaryArea.MouseLeftButtonDown -= OnSecondaryAreaClick;
                secondaryArea.MouseLeftButtonDown += OnSecondaryAreaClick;
            }

            UpdateCanExecute();
        }

        private void OnMainAreaClick(object sender, MouseButtonEventArgs e)
        {
            if (IsMainEnabled && Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
                e.Handled = true;
            }
        }

        private void OnSecondaryAreaClick(object sender, MouseButtonEventArgs e)
        {
            // 辅助命令不受主命令的CanExecute影响
            if (SecondaryCommand != null && SecondaryCommand.CanExecute(SecondaryCommandParameter))
            {
                SecondaryCommand.Execute(SecondaryCommandParameter);
                e.Handled = true;
            }
        }
        #endregion
    }
}