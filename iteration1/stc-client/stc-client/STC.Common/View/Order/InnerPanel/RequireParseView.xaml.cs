using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.ViewModel.Order.InnerPanel;
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

namespace STC.Common.View.Order.InnerPanel
{
    /// <summary>
    /// RequireParseView.xaml 的交互逻辑
    /// </summary>
    public partial class RequireParseView : UserControl
    {
        private RequireParseViewModel requireParseViewModel = null;

        public RequireParseView()
        {
            InitializeComponent();

            requireParseViewModel = new RequireParseViewModel();
            this.DataContext = requireParseViewModel;

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                InsNameCb_edit.Visibility = Visibility.Collapsed;
                TrdNameCb_edit.Visibility = Visibility.Collapsed;
                InvestorBd.Visibility = Visibility.Visible;
                TraderBd.Visibility = Visibility.Visible;
                
                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    if (CommonUtils.IsInvestor())
                    {
                        InvestorBd.IsEnabled = false;
                        TraderBd.IsEnabled = false;
                    }
                    else
                    {
                        //TraderBd.IsEnabled = false;
                    }
                }
                
                tagGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!Data.IntentionOldHandler.Instance.isSALER())
                {
                    InstAndTradeGrid.Visibility = Visibility.Collapsed;
                    tagGrid.Visibility = Visibility.Collapsed;
                }
            }

            if (!Data.IntentionOldHandler.Instance.IsOrderOther)
            {
                this.BrokerCb_edit.IsEnabled = false;
            }

            DataObject.AddPastingHandler(tbContent, OnPaste);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (requireParseViewModel != null)
            //{
            //    requireParseViewModel.RecognizeIntentionQuote(tbContent.Text);
            //}
        }

        private void SelectedUserIDCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (requireParseViewModel != null)
            {
                requireParseViewModel.FilterTraderLocal(args);
            }
        }

        private void SelectedClientType_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (requireParseViewModel != null)
            {
                requireParseViewModel.FilterClientType(args);
            }
        }

        private void SelectedInstitutionCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (requireParseViewModel != null)
            {
                requireParseViewModel.FilterInstitution(args);
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

            if (requireParseViewModel != null)
            {
                requireParseViewModel.RecognizeIntentionQuote(text);
            }
        }


        private void tbContent_KeyDown(object sender, KeyEventArgs e)
        {
            //if (requireParseViewModel != null)
            //{
            //    if (Keyboard.IsKeyDown(Key.Enter))
            //    {
            //        //TextRange textRange = new TextRange(tbContent.Document.ContentStart, tbContent.Document.ContentEnd);

            //        requireParseViewModel.RecognizeIntentionQuote(tbContent.Text);

            //        e.Handled = true;
            //    }

                
            //}
        }

        private void tbContent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (requireParseViewModel != null)
            {
                if (Keyboard.IsKeyDown(Key.Enter) && !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    requireParseViewModel.RecognizeIntentionQuote(tbContent.Text);
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

        /// <summary>
        /// 设置识别框焦点，如果有识别文本，自动移动光标到末尾
        /// </summary>
        public void SetContentFocus()
        {
            try
            {
                if (tbContent.Text.Length > 0)
                {
                    tbContent.Focus();
                    tbContent.SelectionStart = tbContent.Text.Length;
                    tbContent.SelectionLength = 0;
                }
                else
                {
                    tbContent.Focus();
                }
            }
            catch { }
        }

        /// <summary>
        /// 设置识别内容，并自动识别
        /// </summary>
        /// <param name="text"></param>
        public void SetContentText(string text)
        {
            try
            {
                requireParseViewModel.BondTxt = text;
                requireParseViewModel.RecognizeIntentionQuote(text);
            }
            catch { }
        }

        /// <summary>
        /// 设置识别内容能够可见
        /// </summary>
        /// <param name="vis"></param>
        public void SetContentView(Visibility vis)
        {
            try
            {
                tbContent.Visibility = vis;
            }
            catch { }
        }

        /// <summary>
        /// 设置Broker是否可见
        /// </summary>
        /// <param name="vis"></param>
        public void SetBrokerContentView(Visibility vis)
        {
            try
            {
                BrokerCb_grid.Visibility = vis;
                BrokerCb_edit.Visibility = vis;
            }
            catch { }
        }
        
        public void SetContentVisible(Visibility vis)
        {
            try
            {
                tbContent.Visibility = vis;
                InstAndTradeGrid.Visibility = vis;
                tagGrid.Visibility = vis;
            }
            catch { }
        }

        private void BrokerCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(BrokerCb_edit.Text))
            {
                BrokerCb_edit.ExpandOptions();
            }
        }

        private void BrokerCb_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (requireParseViewModel != null)
            {
                requireParseViewModel.FilterBrokerType(args);
            }
        }

        /// <summary>
        /// 设置是否是IRS识别
        /// </summary>
        /// <param name="isirs"></param>
        public void SetIRS(bool isirs)
        {
            try
            {
                if (requireParseViewModel != null)
                {
                    requireParseViewModel.IsIRS = isirs;
                }
            }
            catch { }
        }
    }
}
