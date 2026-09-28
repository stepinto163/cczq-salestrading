using com.gateway.log;
using Newtonsoft.Json;
using BDB.Trade.Plugin.View.Order;
using BDB.Trade.Plugin.ViewModel.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STC.Frame.Plugin;
using STC.Common.Handler.Simulate;
using STC.Common.Model.Fix.Message;
using STC.Frame.Common;
using STC.Common.Data;

namespace BDB.Trade.Plugin
{
    public class SimulatePlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 3;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.SimulatePlugin; }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }

        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private bool isFullScreen = false;
        public override bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }

            set
            {
                isFullScreen = value;
            }
        }

        public override string PluginName
        {
            get { return "模拟交易"; }
        }

        public override string Description
        {
            get { return "模拟交易"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
        }

        #endregion

        protected override void load()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                SimulateHandler.Instance.Subscribe(Instance_OnNewOrderSingle);
            });
        }

        private void Instance_OnNewOrderSingle(NewOrderSingle newOrderSingle)
        {
            string str = JsonConvert.SerializeObject(newOrderSingle);
            LogHelper.Info("OnNewOrderSingle:" + str);

            string timeInForce = newOrderSingle.TimeInForce;

            if (timeInForce == "0")
            {
                PriceMatchParam priceMatchParam = new PriceMatchParam();
                priceMatchParam.Side = newOrderSingle.Side == "1" ? "SELL" : "BUY";
                priceMatchParam.Price = newOrderSingle.Price;
                priceMatchParam.UpdataPrice = false;
                priceMatchParam.CustomFirmAmount = newOrderSingle.OrderQty;
                priceMatchParam.IsFirm = true;
                priceMatchParam.Symbol = newOrderSingle.SecurityID;
                priceMatchParam.NonfirmTime = string.Empty;
                priceMatchParam.OperationType = "1";
                priceMatchParam.Info1 = newOrderSingle.Info1;
                priceMatchParam.Info2 = newOrderSingle.Info2;
                priceMatchParam.Info3 = newOrderSingle.Info3;
                priceMatchParam.Info4 = newOrderSingle.Info4;
                priceMatchParam.Info5 = newOrderSingle.Info5;
                priceMatchParam.ClOrdId = newOrderSingle.ClOrdID;
                //设置默认结算方式
                priceMatchParam.SettlType = newOrderSingle.SettlType;
                //priceMatchParam.NetPrice = GetNetPrice(drv);

                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    SendPriceViewLite view = new SendPriceViewLite(priceMatchParam);

                    view.Show();
                }));
            }
            else if (timeInForce == "3")
            {
                string source = newOrderSingle.Venues;
                //source = ComboSourceUtil.Instance.FindEnum(source, "tovenues");
                string security = newOrderSingle.SecurityID;
                string price = newOrderSingle.Price;
                string qty = newOrderSingle.OrderQty;
                bool isBid = newOrderSingle.Side == "1";
                
                if (!string.IsNullOrEmpty(price))
                {
                    decimal tmpprice = 0;
                    if (!decimal.TryParse(price, out tmpprice))
                    {
                        return;
                    }
                    string settle = newOrderSingle.SettlType;
                    string orderinfotip = string.Empty;

                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        SendPointPriceViewLite sendPointPriceViewLite = new SendPointPriceViewLite(security, isBid, price, qty, settle, string.Empty, source, string.Empty, true, orderinfotip, newOrderSingle.ClOrdID);
                        sendPointPriceViewLite.Show();
                    }));
                }
            }
        }
    }
}
