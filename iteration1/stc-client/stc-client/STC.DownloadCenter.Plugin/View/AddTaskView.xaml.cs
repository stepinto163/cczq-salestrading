using STC.Common.Utils;
using STC.Common.View.Component;
using STC.DownloadCenter.Plugin.Utils;
using STC.DownloadCenter.Plugin.ViewModel;
using STC.DownloadCenter.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
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

namespace STC.DownloadCenter.Plugin.View
{
    /// <summary>
    /// AddTaskView.xaml 的交互逻辑
    /// </summary>
    public partial class AddTaskView : WindowBase
    {
        private AddTaskViewModel addTaskViewModel = new AddTaskViewModel();

        public AddTaskView()
        {
            InitializeComponent();
            this.DataContext = addTaskViewModel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            List<DownloadItem> list = new List<DownloadItem>();
            
            if (settleTypeMode_ccb.Visibility == Visibility.Visible)
            {
                list.Add(new DownloadItem() {
                    ID = System.Guid.NewGuid().ToString("N"),
                    SecurityID = this.addTaskViewModel.Securityid_Edit,
                    Symbol = this.addTaskViewModel.GetSymbol(),
                    DataType = this.addTaskViewModel.DataType_Edit.DataTypeCode,
                    SettleType = this.addTaskViewModel.EditSettleTypeMode,
                    StartDate = $"{this.addTaskViewModel.StartDate?.ToString("yyyy-MM-dd")} {this.addTaskViewModel.StartHour}:{this.addTaskViewModel.StartMinute}:{this.addTaskViewModel.StartSecond}",
                    EndDate = $"{this.addTaskViewModel.EndDate?.ToString("yyyy-MM-dd")} {this.addTaskViewModel.EndHour}:{this.addTaskViewModel.EndMinute}:{this.addTaskViewModel.EndSecond}",
                    CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    UserName = LocalEnv.UserInfo.UserName,
                    Frequency = this.addTaskViewModel.DataType_Edit.DataTypeCode == "7"? this.addTaskViewModel.EditTextMode : string.Empty,
                    Type = this.addTaskViewModel.DataType_Edit.DataTypeCode == "8" ? this.addTaskViewModel.EditTypeMode : string.Empty,
                });
            }
            else
            {
                list.Add(new DownloadItem() {
                    ID = System.Guid.NewGuid().ToString("N"),
                    SecurityID = this.addTaskViewModel.Securityid_Edit,
                    Symbol = this.addTaskViewModel.GetSymbol(),
                    DataType = this.addTaskViewModel.DataType_Edit.DataTypeCode,
                    StartDate = $"{this.addTaskViewModel.StartDate?.ToString("yyyy-MM-dd")} {this.addTaskViewModel.StartHour}:{this.addTaskViewModel.StartMinute}:{this.addTaskViewModel.StartSecond}",
                    EndDate = $"{this.addTaskViewModel.EndDate?.ToString("yyyy-MM-dd")} {this.addTaskViewModel.EndHour}:{this.addTaskViewModel.EndMinute}:{this.addTaskViewModel.EndSecond}",
                    CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    UserName = LocalEnv.UserInfo.UserName,
                    Frequency = this.addTaskViewModel.DataType_Edit.DataTypeCode == "7" ? this.addTaskViewModel.EditTextMode : string.Empty,
                    Type = this.addTaskViewModel.DataType_Edit.DataTypeCode == "8" ? this.addTaskViewModel.EditTypeMode : string.Empty,
                });
            }
            
            if(!DownloadSqliteManager.Instance.Insert(list))
            {
                CommonUtils.Prompt2Second("提示", "新增下载失败");
            }

            this.Close();
        }

        private void SecurityID_TextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            this.addTaskViewModel.SetBondInfo(obj);
            this.addTaskViewModel.Filter();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.addTaskViewModel.Filter();
        }

        private void ValidMinute_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        private void ValidMinute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = null;
            regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Handled)
                return;

            string tag = tb.Tag as string;
            int max = 500;
            int min = (tag == "0") ? 0 : 1;

            if (tag == "0")// [0-60)
            {
                max = 59;
            }
            else if (tag == "1") // [0-500]
            {
                min = 0;
                max = 500;
            }
            else if (tag == "2") // (0-24]
            {
                max = 24;
            }
            else if (tag == "3") // [0-60)
            {
                min = 0;
                max = 59;
            }
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".")) || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;
            if (selectTxt.Length == text.Length)
            {
                if (decimal.TryParse(e.Text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);


            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }
        }

    }
}
