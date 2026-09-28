using System.Windows;
using System.Windows.Controls;
using System.Data;
using STC.Common.Model.Fix.Message;
using STC.quant.plugin.View.HomePage;
using STC.Common.Model.Quant;
using STC.Common.Handler.Quant;
using System.Threading;
using STC.quant.plugin.ViewModel;
using System.Collections.Generic;
using STC.Frame.CustomUI;
using STC.Common.View.Component;
using System;

namespace STC.quant.plugin.View.Component
{
    /// <summary>
    /// MMOrderControl.xaml 的交互逻辑
    /// </summary>
    public partial class ParamOrderControl : UserControl
    {
        public const string PARAMID_FIELD = "ParamId";
        public const string PARAMKEY_FIELD = "ParamKey";
        public const string PARAMTYPE_FIELD = "ParamType";
        public const string PARAMVALUE_FIELD = "ParamValue";
        public const string PARAMdESC_FIELD = "ParamDesc";
        public string quantID = string.Empty;
        public string quantCode= string.Empty;
        public string userName = string.Empty;
        public string info4 = string.Empty;
        public string info5 = string.Empty;
        public ParamOrderControl()
        {
            InitializeComponent();
 
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext == null) { return; }

            DataRowView currentDV = this.DataContext as DataRowView;

        }

    

        private void popupBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupWindow popOrderWin = new PopupWindow();
            ParamOrderControl orderControl = new ParamOrderControl();
            DataRowView currentDV = this.DataContext as DataRowView;
            orderControl.DataContext = currentDV;
           // orderControl.popupBtn.Visibility = Visibility.Collapsed;
            popOrderWin.Content = orderControl;
            popOrderWin.Width = 350;
            popOrderWin.SizeToContent = SizeToContent.Height;
            popOrderWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            popOrderWin.Show();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddParameterWindow addParameterWindow = new AddParameterWindow("新增参数");
            addParameterWindow.Owner = Application.Current.MainWindow;
            addParameterWindow.ShowDialog();
            if (addParameterWindow.Result == false)
            {
                return;
            }
            TTbookQuantParameter request = new TTbookQuantParameter();
            request.Quantid = quantCode;
            request.Param_Key = addParameterWindow.Param_key;
            request.Param_Type = addParameterWindow.Param_type;
            request.Param_Value = addParameterWindow.Param_value;
            request.Param_Desc = addParameterWindow.Param_desc;
            request.Action = "1"; // 添加
            request.Username = userName;
            List<TTbookQuantParameter> list = new List<TTbookQuantParameter>();
            list.Add(request);
            TTbookQuantParameterRequest po = new TTbookQuantParameterRequest();
            po.action = "1";
            po.quantid = quantCode;
            po.list.AddRange(list);
            ThreadPool.QueueUserWorkItem(o =>
            {
                TTbookQuantParameterReplyAlter pResult = QuantHandler.Instance.quantParasUpdate(po);
                if (pResult != null && pResult.IsSuccess())
                {
                    // CustomMessageBox.Info($"新增成功！", "新增参数");
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        CustomMessageBox.Warning(pResult == null ? "新增失败！" : pResult.Msg, "新增参数");
                    }));
                }
            });
        }

        //private void btnEdit_Click(object sender, RoutedEventArgs e)
        //{
        //    DataRowView drv = (DataRowView)this.ParamListView.SelectedItem;
        //    if (drv == null)
        //        return;
        //    AddParameterWindow addParameterWindow = new AddParameterWindow("修改参数");
        //    addParameterWindow.Owner = Application.Current.MainWindow;
        //    addParameterWindow.param_key.Text = drv[PARAMKEY_FIELD] + "";
        //    addParameterWindow.param_type.Text = drv[PARAMTYPE_FIELD] + "";
        //    addParameterWindow.param_value.Text = drv[PARAMVALUE_FIELD] + "";
        //    addParameterWindow.param_desc.Text = drv[PARAMdESC_FIELD] + "";
        //    addParameterWindow.param_key.IsEnabled = false;
        //    addParameterWindow.ShowDialog();
        //    if (addParameterWindow.Result == false)
        //    {
        //        return;
        //    }
        //    QuantParameterReq request = new QuantParameterReq();
        //    request.quantid = quantCode;
        //    request.id= drv[PARAMID_FIELD] + "";
        //    request.param_key = addParameterWindow.Param_key;
        //    request.param_type = addParameterWindow.Param_type;
        //    request.param_value = addParameterWindow.Param_value;
        //    request.param_desc = addParameterWindow.Param_desc;
        //    request.action = "2"; // 修改
        //    PResult pResult = QuantHandler.Instance.quantParasUpdate(request);
        //    if (pResult != null && pResult.IsSuccess())
        //    {
        //        //CustomMessageBox.Info($"修改成功！", "修改参数");
        //    }
        //    else
        //    {
        //        CustomMessageBox.Warning(pResult == null ? "修改失败！" : pResult.Msg, "修改参数");
        //    }

        //}

        private void btnEdit_Click2(object sender, RoutedEventArgs e)
        {
            MMListBoxViewModel mMListBoxViewModel = this.DataContext as MMListBoxViewModel;
            AlterParamOrderControl addParameterWindow = new AlterParamOrderControl();
            addParameterWindow.setColumnVisibility(info4);
            addParameterWindow.UserName = this.userName;
            addParameterWindow.viewModel.quantID = this.quantCode;
            if (info5.Contains(","))
                addParameterWindow.viewModel.info5 = info5.Split(',');
            else
                addParameterWindow.viewModel.info5 = new string[] { info5 };

            addParameterWindow.viewModel.IsParamEnabled = mMListBoxViewModel.IsParamEnabled;
            addParameterWindow.ShowDialog();
            if (addParameterWindow.Result == false)
            {
                return;
            }
            //QuantParameterReq request = new QuantParameterReq();
            //PResult pResult = QuantHandler.Instance.quantParasUpdate(request);
            //if (pResult != null && pResult.IsSuccess())
            //{
            //    //CustomMessageBox.Info($"修改成功！", "修改参数");
            //}
            //else
            //{
            //    CustomMessageBox.Warning(pResult == null ? "修改失败！" : pResult.Msg, "修改参数");
            //}
        }


        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = (DataRowView)this.QParamListView.SelectedItem;
            if (drv == null)
                return;
            string text = "确定要删除选中参数吗？";
            string title = "删除";
            if (CustomMessageBox.Question(text, title) == true)
            {
                TTbookQuantParameter request = new TTbookQuantParameter();
                request.Id = drv[PARAMID_FIELD]+"";
                request.Quantid = quantCode;
                request.Action = "3"; // 添加
                List<TTbookQuantParameter> list = new List<TTbookQuantParameter>();
                list.Add(request);
                TTbookQuantParameterRequest po = new TTbookQuantParameterRequest();
                po.action = "3";
                po.quantid = quantCode;
                po.list = list;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    TTbookQuantParameterReplyAlter pResult = QuantHandler.Instance.quantParasUpdate(po);
                });
            }
           
        }

        public void setColumnVisibility(string info4)
        {
            this.info4 = info4;
            int sub = 0;
            if (int.TryParse(info4, out sub))
            {
                if (sub == 0)
                {
                    this.QParamListView.Columns[3].Visibility = Visibility.Hidden;
                    this.QParamListView.Columns[4].Visibility = Visibility.Hidden;
                }
                else if (sub == 1)
                {
                    this.QParamListView.Columns[3].Visibility = Visibility.Visible;
                    this.QParamListView.Columns[4].Visibility = Visibility.Hidden;
                }
                else if (sub == 2)
                {
                    this.QParamListView.Columns[3].Visibility = Visibility.Visible;
                    this.QParamListView.Columns[4].Visibility = Visibility.Visible;
                }
            }
        }

        private void ParamListView_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //e.Row.IsEnabled = false;//行不可编辑
           
           // e.Row.Focusable = false;

        }



        //public void setButtonVisibility(Visibility visibility)
        //{
        //    btnFullScreen.Visibility = visibility;
        //}

    }
}
