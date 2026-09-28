using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using STC.InnerBoard.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace STC.Common.View.Component
{
    /// <summary>
    /// AddGroupView.xaml 的交互逻辑
    /// </summary>
    public partial class AddSubscriptionGroupWindow : WindowBase
    {

        private AddSubscriptionGroupViewModel _AddGroupViewModel;

        public Action<FilterGroupItem> AddAction;


        public AddSubscriptionGroupWindow(ObservableCollection<FilterGroupItem> groupList, string focusTags, string groupid,string startTermDY, string endTermDY, bool isAppTerm,
            string startCoupon, string endCoupon, bool isCoupon, string startPrice, string endPrice, string startChinaBond, string endChinaBond,
            string startBidBp, string endBidBp, string startOfrBp, string endOfrBp,string inf5=null)
        {
            InitializeComponent();
            newGroupRd.Checked += newGroupRd_Checked;
            updateGroupRd.Checked += updateGroupRd_Checked;
            groupNameTxt.PreviewTextInput += TextBox_PreviewTextInput;
            groupNameTxt.TextChanged += TextBox_TextChanged;
            // 禁止粘贴命令 (Ctrl+V)
            groupNameTxt.CommandBindings.Add(
                new CommandBinding(
                    ApplicationCommands.Paste,
                    new ExecutedRoutedEventHandler(OnPasteCommand),
                    new CanExecuteRoutedEventHandler(OnCanExecutePaste)));
            // 禁止右键菜单（防止右键粘贴）
            groupNameTxt.ContextMenu = null;

            subChk_Bond.Visibility = inf5 != null ? Visibility.Collapsed : Visibility.Visible;
            Title= inf5 != null ? "保存筛选方案" : "保存订阅为组";
            _AddGroupViewModel = new AddSubscriptionGroupViewModel(groupList, focusTags, this, groupid, startTermDY, endTermDY, isAppTerm, startCoupon, endCoupon, 
                isCoupon, startPrice, endPrice, startChinaBond, endChinaBond, startBidBp, endBidBp, startOfrBp, endOfrBp, inf5);

            if (groupList != null && groupList.Count > 0 && !string.IsNullOrEmpty(groupid))
            {
                if (_AddGroupViewModel != null)
                {
                    _AddGroupViewModel.NewGroupRdIsEnableInput = false;
                }

                updateGroupRd.IsChecked = true;
            }

            this.DataContext = _AddGroupViewModel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.newGroupRd.IsChecked.Value == true)
            {
                _AddGroupViewModel.NewOne();
            }
            else
            {
                _AddGroupViewModel.UpdateOne();
            }

            this?.Close();
        }

        private void CheckData()
        {
            if  (this.newGroupRd.IsChecked.Value)
            {
                this.btnOK.IsEnabled = !string.IsNullOrEmpty(this.groupNameTxt.Text); 
            }
            else
            {
                this.btnOK.IsEnabled = true;
            }
        }

        private void newGroupRd_Checked(object sender, RoutedEventArgs e)
        {
            _AddGroupViewModel.NewGroupRdIsEnableInput = true;
            CheckData();
        }

        // 禁止输入的字符（可以自己调整）
        private readonly Regex InvalidCharRegex = new Regex(@"[~!#$%^&*()+=\[\]{};:"",.<>/?\\|、·￥]", RegexOptions.Compiled);
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            // 1️⃣ 禁止非法字符
            if (InvalidCharRegex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            // 2️⃣ 判断总长度
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            int len = GetWeightedLength(newText);
            if (len > 40)
            {
                e.Handled = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            // 删除非法字符
            string clean = InvalidCharRegex.Replace(textBox.Text, "");

            // 如果长度超限，逐个裁剪
            while (GetWeightedLength(clean) > 40)
            {
                clean = clean.Substring(0, clean.Length - 1);
            }

            if (clean != textBox.Text)
            {
                int caret = textBox.CaretIndex;
                textBox.Text = clean;
                textBox.CaretIndex = caret > clean.Length ? clean.Length : caret;
            }

            CheckData();
        }

        private void updateGroupRd_Checked(object sender, RoutedEventArgs e)
        {
            if (_AddGroupViewModel != null)
            {
                _AddGroupViewModel.GroupName = "";
                _AddGroupViewModel.NewGroupRdIsEnableInput = false;
                CheckData();
            }
        }


        // 计算字符串“长度”，中文算2个
        private int GetWeightedLength(string text)
        {
            int length = 0;
            foreach (char c in text)
            {
                // 中文范围（也可以加上其他全角字符）
                if (Regex.IsMatch(c.ToString(), @"[\u4e00-\u9fa5]"))
                    length += 2;
                else
                    length += 1;
            }
            return length;
        }

         // 🚫 禁止粘贴命令
        private void OnPasteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true; // 阻止 Ctrl+V
        }

        private void OnCanExecutePaste(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false; // 禁止执行粘贴命令
        }
    }
}
