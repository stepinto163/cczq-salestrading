using System.Windows;
using System.Data;
using STC.Common.Model.Fix.Message;
using STC.quant.plugin.View.HomePage;
using STC.Common.Model.Quant;
using STC.Common.Handler.Quant;
using System.Threading;
using STC.Common.View.Component;
using STC.quant.plugin.ViewModel;
using System.Collections.Generic;
using System;
using STC.quant.plugin.Utils.Data;
using System.Windows.Controls;
using STC.quant.plugin.SelectorTemplate;
using STC.quant.plugin.Utils;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace STC.quant.plugin.View.Component
{
    /// <summary>
    /// MMOrderControl.xaml 的交互逻辑
    /// </summary>
    public partial class AlterParamOrderControl : WindowBase
    {
        public AlterParamViewModel viewModel = AlterParamViewModel.Instance;
        public string UserName = null;

        //public const string PARAMID_FIELD = "ParamId";
        //public const string PARAMKEY_FIELD = "ParamKey";
        //public const string PARAMTYPE_FIELD = "ParamType";
        //public const string PARAMVALUE_FIELD = "ParamValue";
        //public const string PARAMVALUE_FIELD2 = "ParamValue2";
        //public const string PARAMdESC_FIELD = "ParamDesc";
        //public const string PARAMdESC_FIELD2 = "ParamDesc2";
        // private const decimal SpreadRate = 100m;
        //public string quantID = string.Empty;
        //public string quantCode= string.Empty;
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        public AlterParamOrderControl()
        {
            InitializeComponent();
           
           
            //viewModel.MultiTreeComboxItemsSource = STC.Common.Utils.CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(STC.Common.Utils.CommonUtils.SerializeObject(STC.Common.Handler.Admin.AdminHandler.Instance.GetTCurrentBookCollection()));
            this.DataContext = viewModel;
            this.Loaded += AlterParamOrderControl_Loaded;
        }

        private void AlterParamOrderControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.UserName = this.UserName;

            viewModel.init();
        }

        List<TTbookQuantParameter> Deletelist = new List<TTbookQuantParameter>();
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = (DataRowView)this.ParamListView.SelectedItem;
            if (drv == null)
                return;
            TTbookQuantParameter request = new TTbookQuantParameter();
            request.Id = drv[MMListBoxViewModel.PARAMID_FIELD] + "";
            request.Quantid = this.viewModel.quantID;
            request.Action = "3"; // 添加
            Deletelist.Add(request);
            lock (this.viewModel.ParamView)
            {
                drv.BeginEdit();
                drv.Delete();
                drv.EndEdit();
            }
            this.viewModel.ParamView.Table.AcceptChanges();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Deletelist.Clear();
            this.Result = false;
            this.Close();
            e.Handled = true;
            //foreach (DataRow dr in viewModel.ParamView.Table.Rows)
            //{
            //    dr.BeginEdit();
            //    dr[MMListBoxViewModel.PARAMVALUE_FIELD] = dr[MMListBoxViewModel.PARAMVALUEBACK_FIELD];
            //    dr[MMListBoxViewModel.PARAMVALUE1_FIELD] = dr[MMListBoxViewModel.PARAMVALUE1BACK_FIELD];
            //    dr[MMListBoxViewModel.PARAMVALUE2_FIELD] = dr[MMListBoxViewModel.PARAMVALUE2BACK_FIELD];
            //    dr.EndEdit();
            //}
        }
        
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            List<TTbookQuantParameter> list = new List<TTbookQuantParameter>();
            Dictionary<string, TTbookQuantParameter> dic = new Dictionary<string, TTbookQuantParameter>();
            foreach (DataRowView drv in viewModel.ParamView)
            {
                ConvertPo(dic, list, drv);
            }
            list = new List<TTbookQuantParameter>(dic.Values);
            list.AddRange(Deletelist);
            if (list.Count > 0)
            {
                TTbookQuantParameterRequest request = new TTbookQuantParameterRequest();
                request.action = "2";
                request.quantid = this.viewModel.quantID;
                request.list.AddRange(list);
                ThreadPool.QueueUserWorkItem(o =>
                {
                    TTbookQuantParameterReplyAlter pResult = QuantHandler.Instance.quantParasUpdate(request);
                    if (pResult != null && pResult.IsSuccess())
                    {
                        Deletelist.Clear();
                        this.Result = true;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.Close();
                        }));
                        e.Handled = true;
                    }
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (pResult != null && pResult.Datas != null)
                                CustomMessageBox.Warning(pResult == null ? "修改失败！" : pResult.Datas.msg, "修改参数");
                            else
                                CustomMessageBox.Warning(pResult == null ? "修改失败！" : pResult.Msg, "修改参数");
                        }));
                    }
                });
            }
            else
            {
                this.Result = true;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Close();
                }));
                e.Handled = true;
            }
        }


        public void ConvertPo(Dictionary<string, TTbookQuantParameter> dic, List<TTbookQuantParameter> list, DataRowView drv)
        {
            string id = drv[MMListBoxViewModel.PARAMID_FIELD] + "";
            string paramKey = drv[MMListBoxViewModel.PARAMKEY_FIELD] + "";
            string paramValue = drv[MMListBoxViewModel.PARAMVALUE_FIELD] + "";
            string paramValueb = drv[MMListBoxViewModel.PARAMVALUEBACK_FIELD] + "";

            string paramValue1 = drv[MMListBoxViewModel.PARAMVALUE1_FIELD] + "";
            string paramValue1b = drv[MMListBoxViewModel.PARAMVALUE1BACK_FIELD] + "";
            string paramValue2 = drv[MMListBoxViewModel.PARAMVALUE2_FIELD] + "";
            string paramValue2b = drv[MMListBoxViewModel.PARAMVALUE2BACK_FIELD] + "";
            string subkey = drv[MMListBoxViewModel.SUBPARAMKEY_FIELD] + "";
            string quantid = this.viewModel.quantID;
            string userName = drv[MMListBoxViewModel.USERNAME_FIELD] + "";
            TTbookQuantParameter param = QuantGroupHandler.Instance.GetParasByQuantIDAndID(quantid, id);
            if (param == null)
                return;
            //var str = JsonConvert.SerializeObject(temp);
            //TTbookQuantParameter param = JsonConvert.DeserializeObject<TTbookQuantParameter>(str);
            if ("Radio".Equals(param.Param_Type))
            {
                DataRow[] drs = viewModel.ParamView.Table.Select("ParamId =" + id);
                bool isContainCheck = false;
                foreach (DataRow row in drs)
                {
                    bool ischeck = string.IsNullOrWhiteSpace(row[MMListBoxViewModel.PARAMISCHECK_FIELD] + "") ? false : (bool)row[MMListBoxViewModel.PARAMISCHECK_FIELD];
                    if (ischeck)
                    {
                        isContainCheck = true;
                        param.Param_Value = row[MMListBoxViewModel.SUBPARAMKEY_FIELD] + "";
                        break;
                    }
                    else
                    {
                        param.Param_Value = "0";//未勾选
                                                //setSubParam(param, paramValue1, paramValue2);
                    }
                }
                if (!isContainCheck)
                {
                    setSubParam(subkey, param, paramValue1, paramValue2);
                }
                else if (!subkey.Equals(param.Param_Value))
                {
                    return;
                }
            }
            else if ("MultiString".Equals(param.Param_Type))
            {
                int index = Convert.ToInt32(subkey) - 1;
                param.Para_Optional_Values[index].Param_Value = paramValue;
                string value = "";
                foreach (ParaOptionalValues po in param.Para_Optional_Values)
                {
                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(po.Param_Value))
                        value += "," + po.Param_Value;
                    else
                        value += po.Param_Value;
                }
                param.Param_Value = value;
            }
            else if ("MultiSelect".Equals(param.Param_Type))
            {
                DataRow[] drs = viewModel.ParamView.Table.Select("ParamId =" + id);
                string value = "";
                foreach (DataRow row in drs)
                {
                    bool ischeck = string.IsNullOrWhiteSpace(row[MMListBoxViewModel.PARAMISCHECK_FIELD] + "") ? false : (bool)row[MMListBoxViewModel.PARAMISCHECK_FIELD];
                    if (ischeck)
                    {
                        string subKey = row[MMListBoxViewModel.SUBPARAMKEY_FIELD] + "";
                        if (!string.IsNullOrEmpty(value))
                            value += "," + subKey;
                        else
                            value += subKey;
                    }
                }
                param.Param_Value = value;
            }
            else if ("MultiSelectItem".Equals(param.Param_Type))
            {
                //if(QuantGroupHandler.Instance.dicBookUser)
                //param.Param_Value = value;
            }
            else
                param.Param_Value = paramValue;
            //if (!dic.ContainsKey(id))
            //{
            if (id.Contains("-"))
                id = id.Split('-')[0];
            if (!isEqual(paramValue, paramValueb))
            {
                param.Action = "2"; // 修改
                setSubParam(subkey, param, paramValue1, paramValue2);
                param.Param_Value = RemoveDuplicates(param.Param_Value);
                //list.Add(param);
                if (dic.ContainsKey(id))
                    dic.Remove(id);
                dic.Add(id, param);
            }
            else if (!isEqual(param.Param_Value, paramValueb) || !isEqual(paramValue1, paramValue1b) || !isEqual(paramValue2, paramValue2b))
            {
                param.Action = "2"; // 修改
                setSubParam(subkey, param, paramValue1, paramValue2);
                param.Param_Value = RemoveDuplicates(param.Param_Value);
                //list.Add(param);
                if (dic.ContainsKey(id))
                    dic.Remove(id);
                dic.Add(id, param);
            }
            //}
        }

        private string RemoveDuplicates(string value)
        {
            string text = "";
            if (!value.Contains(","))
                return value;
            List<string> list = new List<string>();
            string[] str = value.Split(',');
            foreach (string s in str)
            {
                if (!list.Contains(s))
                {
                    list.Add(s);
                    if (!string.IsNullOrEmpty(text))
                        text += ",";
                    text += s;

                }
            }
            return text;
        }

        private void setSubParam(string subkey, TTbookQuantParameter param, string paramValue1, string paramValue2)
        {
            if (param.Is_Independent_Para)
            {
                foreach (SubParam sub in param.Sub_Param)
                {
                    if (!string.IsNullOrEmpty(sub.Option_Key) )//&& (sub.Option_Key.Equals(param.Param_Value) || param.Param_Value.Contains(sub.Option_Key)))
                    {
                        if (!subkey.Equals(sub.Option_Key))
                            continue;
                        int i = 0;
                        foreach (QuantParameter s in sub.Param)
                        {
                            i++;
                            if (i == 1)
                                s.Param_Value = paramValue1;
                            else if (i == 2)
                                s.Param_Value = paramValue2;
                        }
                    }
                    else
                    {
                        foreach (QuantParameter s in sub.Param)
                        {
                            s.Param_Value = "";
                        }
                    }
                }
            }
        }

        private bool isEqual(string str1, string str2)
        {
            bool isEqual = false;
            if (str1.Equals(str2))
                isEqual = true;
            return isEqual;
        }

        public void setColumnVisibility(string info4)
        {
            int sub = 0;
            if (int.TryParse(info4, out sub))
            {
                if (sub == 0)
                {
                    this.ParamListView.Columns[3].Visibility = Visibility.Hidden;
                    this.ParamListView.Columns[4].Visibility = Visibility.Hidden;
                }
                else if (sub == 1)
                {
                    this.ParamListView.Columns[3].Visibility = Visibility.Visible;
                    this.ParamListView.Columns[4].Visibility = Visibility.Hidden;
                }
                else if (sub == 2)
                {
                    this.ParamListView.Columns[3].Visibility = Visibility.Visible;
                    this.ParamListView.Columns[4].Visibility = Visibility.Visible;
                }
            }
        }

        private void ParamListView_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            CheckBox ck = e.Column.GetCellContent(e.Row) as CheckBox;
            if (ck == null)
            {
                return;
            }
            bool isCheck = ck.IsChecked.Value;
        }

        private void TextFieldItem_AddEventHandler(object sender, RoutedEventArgs e)
        {
            //TextFieldItem ck = sender as TextFieldItem;
            //DataRowView d = ck.DataContext as DataRowView;
            //if (d == null)
            //    return;
            //string bondCode = d[MMListBoxViewModel.PARAMVALUE_FIELD] + "";
            //BatchAddBondsWindow batch = new BatchAddBondsWindow(true, bondCode);
            //batch.Show();

        }

        private void TextFieldItem_EditEventHandler(object sender, RoutedEventArgs e)
        {
            //TextFieldItem ck = sender as TextFieldItem;
            //DataRowView d = ck.DataContext as DataRowView;
            //if (d == null)
            //    return;
            //string bondCode = d[MMListBoxViewModel.PARAMVALUE_FIELD] + "";
            //BatchAddBondsWindow batch = new BatchAddBondsWindow(false, bondCode);
            //batch.IsAdd = false;
            //batch.Show();

            TextFieldItem ck = sender as TextFieldItem;
            DataRowView d = ck.DataContext as DataRowView;
            if (d == null)
                return;


            AlterParamGroup g = new AlterParamGroup();
            g.GroupId = d[MMListBoxViewModel.PARAMVALUE_FIELD] + "";
            g.GroupName = d[MMListBoxViewModel.BUTTONGROUPNAME_FIELD] + "";
            g.UserName = this.UserName;

           g.ShowDialog();
            if (g.Result == true)
            {
                d[MMListBoxViewModel.BUTTONGROUPNAME_FIELD] = g.GroupName;
                d[MMListBoxViewModel.PARAMVALUE_FIELD] = g.GroupId;
            }
        }

        private void TextItem_LostFocus(object sender, RoutedEventArgs e)
        {
            UserControl ck = sender as UserControl;
            if (ck == null)
                return;
            DataRowView drv = ck.DataContext as DataRowView;
            if (drv == null)
                return;
            List<TTbookQuantParameter> list = new List<TTbookQuantParameter>();
            Dictionary<string, DataRowView> dic = new Dictionary<string, DataRowView>();
            LostFocus("", 0, dic, list, drv);

        }

        private void TextItem1_LostFocus(object sender, RoutedEventArgs e)
        {
            UserControl ck = sender as UserControl;
            if (ck == null)
                return;
            DataRowView drv = ck.DataContext as DataRowView;
            if (drv == null)
                return;
            List<TTbookQuantParameter> list = new List<TTbookQuantParameter>();
            Dictionary<string, DataRowView> dic = new Dictionary<string, DataRowView>();
            LostFocus("", 1, dic, list, drv);
        }

        private void TextItem2_LostFocus(object sender, RoutedEventArgs e)
        {
            UserControl ck = sender as UserControl;
            if (ck == null)
                return;
            DataRowView drv = ck.DataContext as DataRowView;
            if (drv == null)
                return;
            List<TTbookQuantParameter> list = new List<TTbookQuantParameter>();
            Dictionary<string, DataRowView> dic = new Dictionary<string, DataRowView>();
            LostFocus("", 2, dic, list, drv);
        }

        private void TextItemMulti_LostFocus(object sender, RoutedEventArgs e)
        {
            UserControl ck = sender as UserControl;
            if (ck == null)
                return;
            DataRowView drv = ck.DataContext as DataRowView;
            if (drv == null)
                return;
            List<TTbookQuantParameter> list = new List<TTbookQuantParameter>();
            Dictionary<string, DataRowView> dic = new Dictionary<string, DataRowView>();
            string subKey = drv[MMListBoxViewModel.SUBPARAMKEY_FIELD] + "";
            LostFocus(subKey,0, dic, list, drv);
        }

        private void LostFocus(string subKey, int paramInt, Dictionary<string, DataRowView> dic, List<TTbookQuantParameter> list, DataRowView drv)
        {
            string id = drv[MMListBoxViewModel.PARAMID_FIELD] + "";
            string paramKey = drv[MMListBoxViewModel.PARAMKEY_FIELD] + "";
            string paramValue = drv[MMListBoxViewModel.PARAMVALUE_FIELD] + "";
            // QuantCommonUtils.setGroupID(drv[MMListBoxViewModel.PARAMVALUE_FIELD] + "", viewModel.dicGroup);
            string paramValueb = drv[MMListBoxViewModel.PARAMVALUEBACK_FIELD] + "";

            string paramValue1 = drv[MMListBoxViewModel.PARAMVALUE1_FIELD] + "";
            string paramValue1b = drv[MMListBoxViewModel.PARAMVALUE1BACK_FIELD] + "";
            string paramValue2 = drv[MMListBoxViewModel.PARAMVALUE2_FIELD] + "";
            string paramValue2b = drv[MMListBoxViewModel.PARAMVALUE2BACK_FIELD] + "";
            string subkey = drv[MMListBoxViewModel.SUBPARAMKEY_FIELD] + "";
            string quantid = this.viewModel.quantID;
            TTbookQuantParameter temp = QuantGroupHandler.Instance.GetParasByQuantIDAndID(quantid, id);
            if (temp == null)
                return;
            var str = JsonConvert.SerializeObject(temp);
            TTbookQuantParameter param = JsonConvert.DeserializeObject<TTbookQuantParameter>(str);
            if ("Radio".Equals(param.Param_Type))
            {
                DataRow[] drs = viewModel.ParamView.Table.Select("ParamId =" + id);
                bool isContainCheck = false;
                foreach (DataRow row in drs)
                {
                    bool ischeck = string.IsNullOrWhiteSpace(row[MMListBoxViewModel.PARAMISCHECK_FIELD] + "") ? false : (bool)row[MMListBoxViewModel.PARAMISCHECK_FIELD];
                    if (ischeck)
                    {
                        isContainCheck = true;
                        param.Param_Value = row[MMListBoxViewModel.SUBPARAMKEY_FIELD] + "";
                        break;
                    }
                    else
                    {
                        param.Param_Value = "0";//未勾选
                                                //setSubParam(param, paramValue1, paramValue2);
                    }
                }
                if (!isContainCheck)
                {
                    setSubParam(subkey, param, paramValue1, paramValue2);
                }
                else if (!subkey.Equals(param.Param_Value))
                {
                    return;
                }
            }
            else if ("MultiString".Equals(param.Param_Type))
            {
                //DataRow[] drs = viewModel.ParamView.Table.Select("ParamId =" + id);
                int index = Convert.ToInt32(subkey) - 1;
                param.Para_Optional_Values[index].Param_Value = paramValue;
                string value = "";
                foreach (ParaOptionalValues po in param.Para_Optional_Values)
                {
                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(po.Param_Value))
                        value += "," + po.Param_Value;
                    else
                        value += po.Param_Value;
                }
                param.Param_Value = value;
            }
            else if ("MultiSelect".Equals(param.Param_Type))
            {
                //DataRow[] drs = viewModel.ParamView.Table.Select("ParamId =" + id);
                int index = Convert.ToInt32(subkey) - 1;
                param.Para_Optional_Values[index].Param_Value = paramValue;
                string value = "";
                foreach (ParaOptionalValues po in param.Para_Optional_Values)
                {
                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(po.Param_Value))
                        value += "," + po.Param_Key;
                    else
                        value += po.Param_Key;
                }
                param.Param_Value = value;
            }
            else
                param.Param_Value = paramValue;
            if (!dic.ContainsKey(id))
            {
                if (id.Contains("-"))
                    id = id.Split('-')[0];
                if (!isEqual(paramValue, paramValueb))
                {
                    param.Action = "4"; // 校验
                    setSubParam(subkey, param, paramValue1, paramValue2);
                    param.Param_Value = RemoveDuplicates(param.Param_Value);
                    list.Add(param);
                    dic.Add(id, drv);
                }
                else if (!isEqual(param.Param_Value, paramValueb) || !isEqual(paramValue1, paramValue1b) || !isEqual(paramValue2, paramValue2b))
                {
                    param.Action = "4"; // 校验
                    setSubParam(subkey, param, paramValue1, paramValue2);
                    param.Param_Value = RemoveDuplicates(param.Param_Value);

                    if (!string.IsNullOrEmpty(param.Param_Value) && !string.IsNullOrEmpty(paramValue1) && !string.IsNullOrEmpty(paramValue2))
                    {
                        list.Add(param);
                        dic.Add(id, drv);
                    }
                    else if (string.IsNullOrEmpty(param.Param_Value) && string.IsNullOrEmpty(paramValue1) && string.IsNullOrEmpty(paramValue2))
                    {
                        list.Add(param);
                        dic.Add(id, drv);
                    }
                    else
                    {
                        switch (paramInt)
                        {
                            case 0:
                                if (string.IsNullOrEmpty(param.Param_Value))
                                {
                                    list.Add(param);
                                    dic.Add(id, drv);
                                }
                                break;
                            case 1:
                                if (string.IsNullOrEmpty(paramValue1))
                                {
                                    list.Add(param);
                                    dic.Add(id, drv);
                                }
                                break;
                            case 2:
                                if (string.IsNullOrEmpty(paramValue2))
                                {
                                    list.Add(param);
                                    dic.Add(id, drv);
                                }
                                break;
                        }
                    }
                }
            }
            list.AddRange(Deletelist);
            if (list.Count > 0)
            {
                TTbookQuantParameterRequest request = new TTbookQuantParameterRequest();
                request.action = "4";
                request.quantid = this.viewModel.quantID;
                request.list.AddRange(list);
                ThreadPool.QueueUserWorkItem(o =>
                {
                    TTbookQuantParameterReplyAlter pResult = QuantHandler.Instance.quantParasUpdate(request);
                    if (pResult != null && pResult.IsSuccess())
                    {
                        Deletelist.Clear();
                    }
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (pResult != null && pResult.Datas != null)
                                CustomMessageBox.Warning(pResult == null ? "修改失败！" : pResult.Datas.msg, "修改参数", this);
                            else
                                CustomMessageBox.Warning(pResult == null ? "修改失败！" : pResult.Msg, "修改参数", this);
                        }));
                    }
                });
            }
        }


       


    }
}
