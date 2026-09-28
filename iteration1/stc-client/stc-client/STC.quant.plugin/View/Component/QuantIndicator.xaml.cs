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

namespace STC.quant.plugin.View.Component
{
    /// <summary>
    /// QuantIndicator.xaml 的交互逻辑
    /// </summary>
    public partial class QuantIndicator : UserControl
    {
        public const string ID_FIELD = "Id";
        public const string KEY_FIELD = "Key";
        public const string VALUE_FIELD = "Value";
        public const string FLAG_FIELD = "Flag";
        public QuantIndicator()
        {
            InitializeComponent();
        }
    }
}
