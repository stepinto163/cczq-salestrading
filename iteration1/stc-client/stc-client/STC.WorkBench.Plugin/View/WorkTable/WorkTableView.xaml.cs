using STC.Common.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// WorkTable.xaml 的交互逻辑
    /// </summary>
    public partial class WorkTableView : UserControl
    {
        private string _oldSecuityID = "Tradebook";

        Common.Interface.IPriceAndOrderViewInterface dgPrice;

        public WorkTableView()
        {
            InitializeComponent();

            if (CommonUtils.IsBDBAppName())
            {
                dgPrice = DesignHandler.Instance.CreatePriceAndOrderView_BDB();
            }
            else
            {
                dgPrice = new Common.View.Card.PriceAndOrderView();
                (dgPrice as Common.View.Card.PriceAndOrderView).SendPriceType = "require";
            }

            dgPrice.InitTableModel(_oldSecuityID, "STC");
            Border_Right1.Child = dgPrice as UIElement;

            //dgPrice.RemoveDbDoubleClickEvent();
            //dgPrice.Init(_oldSecuityID);
        }

        private void MyOrderDefaultView_SelectedRowChanged(ViewModel.Data.IntentionQuoteItem obj)
        {
            if (obj != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string intentionId = obj.IntentionId;
                    if (!string.IsNullOrEmpty(intentionId))
                    {
                        DesignHandler.Instance.IntentionLink(intentionId);
                        //this.myOrderPart.HighligthRow(intentionId);
                        //this.myExePart.HighligthRow(intentionId);
                    }

                    string securityObj = obj.SecurityId;
                    if (CommonUtils.IsBDBAppName())
                    {
                        securityObj = obj.SecurityId;
                        string settle = "全";
                        string id = obj.SecurityId;
                        dgPrice.SelectionChange("STC", id, securityObj, settle, false);
                        _oldSecuityID = securityObj;
                    }
                    else
                    {
                        securityObj = obj.SecurityId;
                        if (!string.IsNullOrEmpty(securityObj) && _oldSecuityID != securityObj)
                        {
                            dgPrice.InitTableModel(securityObj, "STC");
                            _oldSecuityID = securityObj;
                        }
                    }
                }));
                
            }
        }

        private bool _verClose = false;
        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            if (!_verClose)
            {
                workTableGrid.ColumnDefinitions[2].Width = new GridLength(0);
                _verClose = true;

                btnVer.Content = "<";

            }
            else
            {
                workTableGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                workTableGrid.ColumnDefinitions[2].Width = new GridLength(320, GridUnitType.Pixel);
                _verClose = false;

                btnVer.Content = ">";
            }
        }
    }
}
