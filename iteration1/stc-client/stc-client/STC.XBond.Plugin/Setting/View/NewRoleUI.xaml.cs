using STC.Common.Model.Admin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace STC.XBond.Plugin.Setting.View
{
    /// <summary>
    /// NewRoleUI.xaml 的交互逻辑
    /// </summary>
    public partial class NewRoleUI : Window
    {
        public RoleUpdateReq RoleUpdateReq;
        public NewRoleUI(List<Roles> RolesSt)
        {
            InitializeComponent();
            // comRoles.DataContext = RolesSt;
            this.Loaded += NewRoleUI_Loaded;
            Dictionary<string, string> dicRole = new Dictionary<string, string>();
            foreach (Roles r in RolesSt)
            {
                if (!dicRole.ContainsKey(r.id))
                {
                    dicRole.Add(r.id, r.rolename);
                }
            }
            comRoles.ItemsSource = dicRole;
        }

        private void NewRoleUI_Loaded(object sender, RoutedEventArgs e)
        {
            //ObservableCollection<Roles> ro = new ObservableCollection<Roles>();
            //Roles d = new Roles();
            //d.id = "1";
            //d.rolename = "2222";
            //ro.Add(d);
            //comRoles.ItemsSource = ro;
           
            //
            //dicBook.Add("1", "语文");
            //dicBook.Add("2", "数学");
            //dicBook.Add("3", "英语");
            //comRoles.ItemsSource = dicBook;
        }

        private void tbWeight_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void tbWeight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void checkrole_Click(object sender, RoutedEventArgs e)
        {
            if (checkrole.IsChecked.Value)
            {
                comRoles.Visibility = Visibility.Visible;
            }
            else
                comRoles.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RoleUpdateReq = new RoleUpdateReq();
            RoleUpdateReq.rolename = tbRole.Text;
            RoleUpdateReq.action = "1";
            RoleUpdateReq.remark = tbRole.Text;
            if (checkrole.IsChecked.Value)
                RoleUpdateReq.copyRolePermissions = comRoles.Text;
            var win = Window.GetWindow(this);
            if (win != null)
            {
                this.DialogResult = true;
                win.Close();
            }
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            Window win = Window.GetWindow(this);
            if (win != null)
            {
                win.Close();
            }
        }
    }
}
