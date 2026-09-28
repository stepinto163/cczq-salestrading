using STC.Common.View.Component;
using STC.IRS.Plugin.ViewModel;
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

namespace STC.IRS.Plugin.View
{
    /// <summary>
    /// InnerFuzzyRequireView.xaml 的交互逻辑
    /// </summary>
    public partial class InnerFuzzyRequireView : Window
    {
        private InnerFuzzyRequireViewModel _innerFuzzyRequireViewModel;

        public InnerFuzzyRequireView(string info = "")
        {
            InitializeComponent();
            _innerFuzzyRequireViewModel = new InnerFuzzyRequireViewModel(this, info);
            this.DataContext = _innerFuzzyRequireViewModel;
            this.Loaded += InnerFuzzyRequireView_Loaded;
            this.Unloaded += InnerFuzzyRequireView_Unloaded;
        }

        #region UI
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Escape))
            {
                this.Close();
            }
        }

        private void InnerFuzzyRequireView_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            Application.Current.Dispatcher.Invoke(() =>
            {
                contentTxt.Focus();
            });
        }

        private void InnerFuzzyRequireView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
        }

        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    InnerFuzzyRequireView po = (InnerFuzzyRequireView)sender;
                    var p = e.GetPosition(po);
                    if (p.Y > 0 && p.Y <= 30)
                    {
                        this.DragMove();
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                catch { }
            }));
        }
        #endregion


        #region 客户和客户机构

        /// <summary>
        /// 客户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SelectedInstitutionCB_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_innerFuzzyRequireViewModel != null)
            {
                _innerFuzzyRequireViewModel.FilterInstitution(args);
            }
        }

        /// <summary>
        /// 客户名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SelectedUserIDCB_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_innerFuzzyRequireViewModel != null)
            {
               _innerFuzzyRequireViewModel.FilterTraderLocal(args);
            }
        }

        /// <summary>
        /// 客户类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void cbClientType_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_innerFuzzyRequireViewModel != null)
            {
                _innerFuzzyRequireViewModel.FilterClientType(args);
            }
        }
        #endregion
    }
}
