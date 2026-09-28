using STC.Common.View.Component;
using STC.Mmshe.Plugin.ViewModel;
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

namespace STC.Mmshe.Plugin.View
{
    /// <summary>
    /// ChangeGroupView.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeGroupView : WindowBase
    {
        ChangeGroupViewModel vm;

        public ChangeGroupView(string currentid)
        { 
            InitializeComponent();

            ChangeGroupViewModel vm = new ChangeGroupViewModel(currentid);
            this.DataContext = vm;
        }
    }
}
