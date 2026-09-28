using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.XBond.Plugin.ViewModel.Batch;
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

namespace STC.XBond.Plugin.View.Batch
{
    /// <summary>
    /// BatchVenueView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchVenueView : WindowBase
    {
        Brush BNC_ColorBrush;
        Brush BCO_ColorBrush;
        BatchVenueViewModel viewModel;

        public BatchVenueView()
        {
            InitializeComponent();

            viewModel = new BatchVenueViewModel(this);
            this.DataContext = viewModel;


            var converter = new System.Windows.Media.BrushConverter();

            BNC_ColorBrush = (Brush)converter.ConvertFromString("#E63034");
            BCO_ColorBrush = (Brush)converter.ConvertFromString("#2E58DB");
        }


        /// <summary>
        /// 界面初始化设置
        /// </summary>
        /// <param name="toNonfirmFlag">0: tofirm; 1: toNonFirm</param>
        /// <param name="newSingleOrderDic"></param>
        public void Init(Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic)
        {

            this.Title = "批量恢复报价";

            if (newOrderSingleDic != null && newOrderSingleDic.Count > 0)
            {

                bool hasBNC = false; // 有利率债
                bool hasBCO = false; // 有信用债

                foreach (NewOrderSingle item in newOrderSingleDic.Values)
                {
                    if (CommonUtils.IsRate(item.SecurityType))
                    {
                        hasBNC = true;
                    }
                    else
                    {
                        hasBCO = true;
                    }
                }
                // 选择订单数量
                viewModel.SelectedCount = newOrderSingleDic.Count;


                Interest_Part.Children.Remove(execVenueView);
                Credit_Part.Children.Remove(execVenueViewBCO);

                // 设置显示利率债， 信用债
                if (hasBNC)
                {
                    viewModel.IsShowInterestRateDebt = Visibility.Visible;

                    

                    execVenueView.init("BNC");
                    Interest_Part.Children.Add(execVenueView);
                    Grid.SetColumn(execVenueView, 1);

                    //ChannelCheckUI bncChannelCheckUI = new ChannelCheckUI();
                    //bncChannelCheckUI.CheckValueChanged += bncChannelCheckUI_CheckValueChanged;
                    //bncChannelCheckUI.Init("BNC", BNC_ColorBrush, string.Empty);

                    //Interest_Part.Children.Add(bncChannelCheckUI);
                    //Grid.SetColumn(bncChannelCheckUI, 1);
                }
                if (hasBCO)
                {
                    viewModel.IsShowCreditDebt = Visibility.Visible;

                    
                    execVenueViewBCO.init("BCO");
                    Credit_Part.Children.Add(execVenueViewBCO);
                    Grid.SetColumn(execVenueViewBCO, 1);


                    //ChannelCheckUI bcoChannelCheckUI = new ChannelCheckUI();
                    //bcoChannelCheckUI.CheckValueChanged += BcoChannelCheckUI_CheckValueChanged;
                    //bcoChannelCheckUI.Init("BCO", BCO_ColorBrush, string.Empty);

                    //Credit_Part.Children.Add(bcoChannelCheckUI);
                    //Grid.SetColumn(bcoChannelCheckUI, 1);
                }

                this.viewModel.newOrderSingleDic = newOrderSingleDic;
            }
        }

    }
}
