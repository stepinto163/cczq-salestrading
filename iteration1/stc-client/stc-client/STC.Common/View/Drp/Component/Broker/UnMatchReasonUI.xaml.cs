using Newtonsoft.Json;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
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

namespace STC.Common.View.Drp.Component.Broker
{
    /// <summary>
    /// UnMatchReasonUI.xaml 的交互逻辑
    /// </summary>
    public partial class UnMatchReasonUI : UserControl
    {
        public UnMatchReasonUI()
        {
            InitializeComponent();
            this.Loaded += UnMatchReasonUI_Loaded;
        }
        public ExecutionReport orderPo { get; set; }
        private void UnMatchReasonUI_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshForm();
        }

        public void RefreshForm()
        {
            if (orderPo != null)
            {
                tbPrice.Text = orderPo.Price;
                tbCount.Text = orderPo.Info2;
                tbReason.Text = orderPo.Info1;
                lbSide.Content = orderPo.Side.Equals("1") ? "Bid" : "Ofr";

                if (!string.IsNullOrWhiteSpace(orderPo.RejectText))
                {
                    string[] strs = orderPo.RejectText.Split(',');
                    if (strs != null && strs.Length > 0)
                    {
                        tbReason.Text = ConstCode.Instance.getMsg(strs[0]);
                    }
                }

                if (orderPo.Side.Equals("1"))
                {
                    tbPrice.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9200"));
                    lbSide.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9200"));
                }
                else
                {
                    tbPrice.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12A1FF"));
                    lbSide.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12A1FF"));
                }
            }
        }

        private async void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            /*if (orderPo == null)
                return;
            GoodsPo po = new GoodsPo();
            po.GoodsCode = orderPo.SecurityID;
            po.GoodsShortname = orderPo.Symbol;
            BondsInfoPo bondData = STC.Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(orderPo.SecurityID);

            if (bondData == null)
            {
                return;
            }
            po.Goodstype = bondData.SECURITY_TYPE;
            po.Subtype = bondData.Bond_Subtype;
            po.FRNIndexID = bondData.FRNIndexID;
*/
            //await MultDispaterHelper.InvokeByModelName(Constant.PricingTab, () => GoodsPricingHandler.Instance.ShowGoodsPrice(po));
            //Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            //{
            //    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "ShowGoodsPrice", Info1 = "ShowGoodsPrice", Msg = JsonConvert.SerializeObject(po) });
            //    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            //}));
        }
    }
}
