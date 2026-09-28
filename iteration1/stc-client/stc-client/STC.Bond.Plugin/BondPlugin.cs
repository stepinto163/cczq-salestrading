using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.Bond.Plugin
{
    public class BondPlugin : AbstractPlugin
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
            get { return 10; }
        }

        public override string PluginName
        {
            get { return "债券交易系统插件"; }
        }

        public override string Description
        {
            get { return "债权交易系统"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion

        #region load,unload

        TabItem item = new TabItem();
        //TabItem item2 = new TabItem();
        //TabItem item3 = new TabItem();
        protected override void load()
        {
            LoadConfig.LoadBond = true;
            //PricingPanel pricingPanel = new PricingPanel();
            //OrderSearchUI orderSearchUI = new OrderSearchUI();
            //SetPanel setPanel = new SetPanel();
            //item.Content = pricingPanel;
            //item.Header = pricingPanel.Title;
            //this.Application.Panel.Items.Add(item);
            //item2.Content = orderSearchUI;
            //item2.Header = orderSearchUI.Title;
            //this.Application.Panel.Items.Add(item2);
            //item3.Content = setPanel;
            //item3.Header = "设置";
            //this.Application.Panel.Items.Add(item3);
            this.Application.Panel.Items.Add(item);
            item.Header = "1111";
            CRMDataHandler.Instance.Application = this.Application;

            Application.Subscribe("STC_Clear", this);

            base.load();
        }
        public override void BeforeTerminating()
        {
            //this.Application.Panel.Items.Remove(item3);
            //this.Application.Panel.Items.Remove(item2);
            //this.Application.Panel.Items.Remove(item);
        }
        #endregion

        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals(Consts.Topic_Clear))
                    // 初始化界面
                    ;
                    //ClearHandler.Instance.NotifyClearMsg((ClearPo)o);
            }
        }

        #endregion
    }
}
