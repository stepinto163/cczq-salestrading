using STC.Common.View.Component;
using STC.InnerBoard.Plugin.ViewModel;
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

namespace STC.InnerBoard.Plugin.View
{
    /// <summary>
    /// InnerFuzzyRequireView.xaml 的交互逻辑
    /// </summary>
    public partial class InnerFuzzyRequireView : WindowBase
    {
        private InnerFuzzyRequireViewModel _innerFuzzyRequireViewModel;

        private string Info = string.Empty;
        public InnerFuzzyRequireView(string info = "")
        {
            InitializeComponent();
            this.Info = info;
            _innerFuzzyRequireViewModel = new InnerFuzzyRequireViewModel(this, info);
            this.Loaded += InnerFuzzyRequireView_Loaded;
            this.Unloaded += InnerFuzzyRequireView_Unloaded;
            this.DataContext = _innerFuzzyRequireViewModel;
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
            //this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            Application.Current.Dispatcher.Invoke(() =>
            {
                tbContent.Focus();
            });
        }

        private void InnerFuzzyRequireView_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
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
                        // Retrieve the coordinate of the mouse position.
                        Point pt = e.GetPosition((UIElement)sender);

                        // Perform the hit test against a given portion of the visual object tree.
                        HitTestResult result = VisualTreeHelper.HitTest(this.desCtrl, pt);

                        if (result != null)
                        {
                            // Perform action on hit visual object.
                        }

                        e.Handled = false;
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

        private void contentTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Enter) && !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                _innerFuzzyRequireViewModel.RequestParserVirtual(tbContent.Text);
                e.Handled = true;
            }

            if (Keyboard.IsKeyDown(Key.Enter) && ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))))
            {
                if (tbContent.Text.Length > 0)
                {
                    try
                    {
                        int sourcepos = tbContent.CaretIndex;
                        if (tbContent.CaretIndex >= 0)
                        {
                            var xbContent = tbContent.Text.Substring(0, tbContent.CaretIndex);
                            var xblevase = tbContent.Text.Substring(tbContent.CaretIndex, tbContent.Text.Length - tbContent.CaretIndex);
                            tbContent.Text = xbContent + "\r\n" + xblevase;
                            tbContent.SelectionStart = sourcepos + 2;
                        }
                    }
                    catch { }
                }

                e.Handled = true;
            }
        }
    }
}
