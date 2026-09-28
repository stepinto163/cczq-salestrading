using STC.Common.Handler.CDH;
using STC.Common.Model.CDH;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
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

namespace STC.XBond.Plugin.View.HomePage
{
    /// <summary>
    /// TradeRequestView.xaml 的交互逻辑
    /// </summary>
    public partial class TradeRequestView : WindowBase
    {
        //private TradeRequestViewModel tradeRequestViewModel = new TradeRequestViewModel();

        public TradeRequestView()
        {
            InitializeComponent();
          //  this.DataContext = tradeRequestViewModel;
        }

        private void TradeInfo_tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private string Venue = "";
        private void VenuesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (isCheckShowAll == true)
            //    return;
            Venue = "";
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (lbi.IsSelected == true)
                        Venue = lbi.Content+"";
                    switch (Venue)
                    {
                        case CommonConstants.CNEX:
                            Venue= "CNEX";
                            break;
                        case CommonConstants.TPSC:
                            Venue = "TPSC";
                            break;
                        case CommonConstants.CCTB:
                            Venue = "CCTB"; 
                            break;
                        case CommonConstants.CTTJ:
                            Venue = "CTTJ";
                            break;
                        case CommonConstants.PTCN:
                            Venue = "PTCN";
                            break;
                        case CommonConstants.UEDA:
                            Venue = "PTCN";
                            break;
                    }
                }
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Venue))
            {

                Common.View.Component.CustomMessageBox.Warning("未选中渠道!");
                return;
            }
            QmDataInfo po = new QmDataInfo();
            po.venue = Venue;
            po.content = txRemark.Text;
            PResult result = CDHHandler.Instance.EnterMarketRequest(po);
            if ("0000".Equals(result.Code))
                txRemark.Text = "";

        }
    }
}
