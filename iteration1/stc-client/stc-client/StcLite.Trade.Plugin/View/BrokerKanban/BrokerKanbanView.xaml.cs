using STC.Common.Handler.Base;
using STC.Common.Model.Base;
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

namespace BDB.Trade.Plugin.View.BrokerKanban
{
    /// <summary>
    /// BrokerKanbanView.xaml 的交互逻辑
    /// </summary>
    public partial class BrokerKanbanView : UserControl
    {
        public BrokerKanbanView()
        {
            InitializeComponent();
            this.Loaded += BrokerKanbanView_Loaded;
            BaseHandler.Instance.UnSubscribe(Instance_OnClear);
        }


        #region 清零信号
        private void Instance_OnClear(ClearPo obj)
        {
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        ResetHandler();
                    }
                    catch (Exception ex)
                    {
                    }
                }));
            }
        }

        public async void ResetHandler()
        {
            await Task.Run(() =>
            {
                qtrsOrderView.Dispose();
                qtrsTradeView.Dispose();

                qtrsOrderView.ReLoadViewModel();
                qtrsTradeView.ReLoadViewModel();

                qtrsOrderView.load();
                qtrsTradeView.load();
            });
        }
        #endregion

        private void BrokerKanbanView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= BrokerKanbanView_Loaded;

            priceView.Init(0, string.Empty, null, null);

            marketView.Init(0, string.Empty);
        }

        /// <summary>
        /// 插件自动调用
        /// </summary>
        public void load()
        {
            qtrsOrderView.load();
            qtrsTradeView.load();
        }

        public void Dispose()
        {
            qtrsTradeView.Dispose();
            qtrsOrderView.Dispose();
                                
        }
    }
}
