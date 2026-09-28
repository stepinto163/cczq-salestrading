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

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// WaterMarkControl.xaml 的交互逻辑
    /// </summary>
    public partial class WaterMarkControl : UserControl
    {
        public WaterMarkControl()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            for (int i = 0; i < 300; i++)
                MainPanel.Children.Add(new TextBlock() { Text = this.Text });
        }
        public void Init(string text)
        {
            this.Text = text;
            Init();
        }
        public string Text { get; set; } = LocalEnv.UserInfo.FullName;
    }
}
