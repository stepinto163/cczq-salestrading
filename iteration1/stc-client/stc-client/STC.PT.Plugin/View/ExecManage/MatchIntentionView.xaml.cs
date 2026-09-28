using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.ViewModel.ExecManage;
using System;
using System.Collections.Generic;
using System.Data;
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
using static STC.Common.View.Component.AutoCompleteComboBox;

namespace STC.PT.Plugin.View.ExecManage
{
    /// <summary>
    /// RelationView.xaml 的交互逻辑
    /// </summary>
    public partial class MatchIntentionView : WindowBase
    {
        MatchIntentionViewModel _viewModel = new MatchIntentionViewModel();
        public event EventHandler ViewClosed;

        public MatchIntentionView(PtExecordersData data, bool IsInvestor, bool IsAdd = false)
        {
            InitializeComponent();
            _viewModel.IsInvestor = IsInvestor;
            _viewModel.IsAdd = IsAdd;
            _viewModel.CurrentItem = data;
            _viewModel.RequestClose += () => CancelBtn_Click("", null);
            _viewModel.matchIntentionView = this;
            this.DataContext = _viewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (_viewModel != null)
                {
                    _viewModel.CheckBoxAllValueChange(flag);
                }
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.CheckBoxSingleClick();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Clear();
            this.Close();
            ViewClosed?.Invoke(this, EventArgs.Empty);
        }

        private void bindQty_Edit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown && _viewModel != null)
            {
                _viewModel.checkRisk();
                IntentionQuoteItem row = ((NumericUpDown)sender).DataContext as IntentionQuoteItem;
                _viewModel.CacheBindQTY(row);
            }
        }

        private void Selected_Chk_Unchecked(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;

            IntentionQuoteItem row = ((CheckBox)sender).DataContext as IntentionQuoteItem;
            if (row != null && row.BindQty_Edit.HasValue && row.BindQty_Edit.Value.CompareTo(0)>0) 
            {
                row.BindQty_Edit = 0; 
                _viewModel.checkRisk();
            }
            
        }

        private void Investor_edit_PatternChanged(object sender, AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            List<StcTbookUsers> investors = CommonUtils.GetInvestors();
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                IntentionQuoteItem row = ((AutoCompleteComboBox)sender).DataContext as IntentionQuoteItem;
                if (row != null && row.SelectedTrader_Buy_Edit != null && LocalEnv.ConfInfo.IsI2T)
                {
                    List<string> managerList = AdminHandler.Instance.GetManagerList(row.SelectedTrader_Buy_Edit.Userid);
                    if (managerList != null)
                    {
                        list = investors.Where(d => managerList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                    }
                }
                else
                {
                    if (investors != null)
                    {
                        if (!string.IsNullOrEmpty(args.Pattern))
                        {
                            list = investors.Where(d => (d.Username != null && d.Username.Contains(args.Pattern))
                            || (d.Name != null && d.Name.Contains(args.Pattern))).ToList();//多条件筛选，新功能增加
                        }
                        else
                        {
                            list = investors.ToList();
                        }
                    }
                }
                args.DataSource = list;
            }));
        }

        private void TraderCb_edit_PatternChanged(object sender, AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            List<StcTbookUsers> traders = CommonUtils.GetTraders();
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                IntentionQuoteItem row = ((AutoCompleteComboBox)sender).DataContext as IntentionQuoteItem;
                if (row != null && row.SelectedInvestor_Edit != null && LocalEnv.ConfInfo.IsI2T)
                {
                    List<string> traderList = AdminHandler.Instance.GetTraderList(row.SelectedInvestor_Edit.Userid);
                    if (traderList != null)
                    {
                        list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                    }
                }
                else
                {
                    if (traders != null)
                    {
                        if (!string.IsNullOrEmpty(args.Pattern))
                        {
                            list = traders.Where(d => (d.Username != null && d.Username.Contains(args.Pattern))
                            || (d.Name != null && d.Name.Contains(args.Pattern))).ToList();//多条件筛选，新功能增加
                        }
                        else
                        {
                            list = traders.ToList();
                        }
                    }
                }

                args.DataSource = list;
            }));
        }

        private void cmbMultiTree_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            IntentionQuoteItem row = ((MultiTreeComboBox)sender).DataContext as IntentionQuoteItem;
            if (row != null && row.SelectedGroupItem != null && row.SelectedGroupItem.Count > 0)
                row.BookId = row.SelectedGroupItem[0];
            //var cmbMultiTree = (MultiTreeComboBox)sender;
            //if (_viewModel != null)
            //{
            //    if (_viewModel.IntentionOrders != null && _viewModel.IntentionOrders.Count > 0)
            //    {
            //        foreach (IntentionQuoteItem item in _viewModel.IntentionOrders)
            //        {
            //            if (!string.IsNullOrWhiteSpace(cmbMultiTree.SelectedId))
            //            {
            //                //item.SelectedTradeAccountId = cmbMultiTree.SelectedId;
            //                item.SelectedTradeAccountCode = cmbMultiTree.SelectedId;
            //            }
            //            else if (cmbMultiTree.SelectedParentId != null && cmbMultiTree.SelectedParentId.Count > 0)
            //            {
            //                item.SelectedTradeAccountCode = cmbMultiTree.SelectedParentId[0];
            //            }

                //            item.SelectedTradeAccountName = cmbMultiTree.SelectedTexts;
                //            item.SelectedGroupItem = cmbMultiTree.SelectedParentId;
                //            item.SelectedGroupText = cmbMultiTree.SelectedParentTexts;
                //            item.HsProductId = cmbMultiTree.SelectedProductId + "";

                //            if (item.SelectedGroupItem != null && item.SelectedGroupItem.Count > 0)
                //                item.BookId = item.SelectedGroupItem[0];

                //            //item._selectedGroupDict = cmbMultiTree.SelectedParentItems;
                //            // item.SelectedProductId = cmbMultiTree.SelectedProductId?.ToString();
                //            //string bookname = string.Empty;
                //            //string hsProductId = "( )";
                //            //if (cmbMultiTree.SelectedProductId != null)
                //            //{
                //            //    hsProductId = $"({cmbMultiTree.SelectedProductId.ToString()})";
                //            //}
                //            //if (cmbMultiTree.SelectedParentTexts != null && cmbMultiTree.SelectedParentTexts.Count > 0)
                //            //{
                //            //    bookname = cmbMultiTree.SelectedParentTexts[0];
                //            //    item.DisplayAccountName = $"{bookname}{hsProductId}";
                //            //}
                //        }
                //    }
                //}
        }

        private void PriceNU_Edit_ValueChanged(object sender, RoutedEventArgs e)
        {
            decimal? price = ((sender as NumericUpDown).DataContext as IntentionQuoteItem).Price;
            decimal? thisPrice = (sender as NumericUpDown).NumericValue;
            if (thisPrice != price)
            {
                ((sender as NumericUpDown).DataContext as IntentionQuoteItem).Price = thisPrice;
            }
        }

        private void PriceNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void bindQty_Edit_ValueChanged(object sender, RoutedEventArgs e)
        {
            IntentionQuoteItem item = (sender as NumericUpDown).DataContext as IntentionQuoteItem;
            if (item.SaveBtnVisible == "0")
            {
                decimal? bindQty = item.BindQty_Edit;
                item.Qty = bindQty;
            }
        }
    }
}
