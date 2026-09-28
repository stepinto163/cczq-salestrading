using STC.Common.Handler.Quant;
using STC.Common.Model.Quant;
using STC.Common.View.Component;
using STC.quant.plugin.Utils.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace STC.quant.plugin.View.HomePage
{
    /// <summary>
    /// AddGroupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddStrategWindow : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// 策略名
        /// </summary>
        public string QuantName { get; private set; }

        /// <summary>
        /// 策略说明
        /// </summary>
        public string Remark { get; private set; }

        /// <summary>
        /// 策略组代码
        /// </summary>  
        public string GroupCode { get; set; }
        //public Dictionary<string, string> GroupCode { get; set; }

        /// <summary>
        /// 模板
        /// </summary>
        public string TempletName { get; private set; }

        /// <summary>
        /// 策略类型 10000=C++策略，Java策略的type值为模板编号
        /// </summary>
        public string Type { get; private set; }

        Dictionary<string, string> dicTempletIdType = new Dictionary<string, string>();
        Dictionary<string, string> dicTempletIdTempletName = new Dictionary<string, string>();

        public AddStrategWindow(string title)
        {
            InitializeComponent();
            this.Title = title;
            //IOrderedEnumerable<TTbookQuantGroups> list = QuantGroupHandler.Instance.CacheSortedGroups;
            //if (list != null && list.Count<TTbookQuantGroups>() > 0)
            //{
            //    foreach (TTbookQuantGroups g in list)
            //    {
            //        if (!dic.ContainsKey(g.Id))
            //        {
            //            dic.Add(g.Id, g.Groupname);
            //        }
            //    }
            //}
            //groupcode.ItemsSource = dic;

            dicTempletIdTempletName.Clear();
            dicTempletIdType.Clear();
            ThreadPool.QueueUserWorkItem(O =>
            {
                TbookQuantTempletReplyPo reply = QuantHandler.Instance.QueryTemplet();
                if (reply != null)
                {
                    if (reply != null && reply.Code == "0000" && reply.Datas != null)
                    {
                        foreach (TbookQuantTemplet templet in reply.Datas)
                        {
                            if (!dicTempletIdTempletName.ContainsKey(templet.Id))
                            {
                                dicTempletIdTempletName.Add(templet.Id, templet.Templetname);
                                dicTempletIdType.Add(templet.Id, templet.Templettype);
                            }
                        }
                    }
                }
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    comTemplet.ItemsSource = null;
                    comTemplet.ItemsSource = dicTempletIdTempletName;
                }));
            });
        }

        //type 添加的策略类型，10000=C++策略
        public AddStrategWindow(string title, string type)
        {
            InitializeComponent();
            this.Title = title;

            dicTempletIdTempletName.Clear();
            dicTempletIdType.Clear();
            Type = type;
            comTemplet.Visibility = Visibility.Hidden;
            ckTemplet.Visibility = Visibility.Hidden;
            lblTemplate.Visibility = Visibility.Hidden;

            Grid.SetRow(lblStrategyDesc, 2);
            Grid.SetRow(remark, 2);
            Grid.SetRow(lblStrategyKeyword, 4);
            Grid.SetRow(groupcode, 4);
            //ThreadPool.QueueUserWorkItem(O =>
            //{
            //    TbookQuantTempletReplyPo reply = QuantHandler.Instance.QueryTemplet();
            //    if (reply != null)
            //    {
            //        if (reply != null && reply.Code == "0000" && reply.Datas != null)
            //        {
            //            foreach (TbookQuantTemplet templet in reply.Datas)
            //            {
            //                if (!dic2.ContainsKey(templet.Id))
            //                {
            //                    dic2.Add(templet.Id, templet.Templetname);
            //                }
            //            }
            //        }
            //    }
            //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //    {
            //        comTemplet.ItemsSource = null;
            //        comTemplet.ItemsSource = dic2;
            //    }));
            //});
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            QuantName = quantname.Text;
            int Qlength = quantname.Text.Length;
            if (string.IsNullOrWhiteSpace(QuantName))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CustomMessageBox.Warning("策略名不能为空！");
                }));
                return;
            }
            else if (Qlength > 45)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CustomMessageBox.Warning("策略名不能超过45字符！");
                }));
                return;
            }
            if (ckTemplet.IsChecked.Value && string.IsNullOrWhiteSpace(comTemplet.Text))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CustomMessageBox.Warning("策略模板不能为空！");
                }));
                return;
            }
            int length = remark.Text.Length;
            if (length > 45)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CustomMessageBox.Warning("策略说明不能超过45字符！");
                }));
                return;
            }
            Remark = remark.Text;
            //GroupCode = groupcode.SelectedValue + "";
            string temp= groupcode.SearchTextBox.Text;
            int i = 1;
            if (groupcode.SelectedValues.Values.Count > 0)
            {
                foreach (string key in groupcode.SelectedValues.Values)
                {
                    GroupCode += key;
                    if (i < groupcode.SelectedValues.Values.Count)
                        GroupCode += " ";
                    i++;
                }
            }
            else
                GroupCode = groupcode.SearchTextBox.Text;
            if (ckTemplet.IsChecked.Value)
            {
                //TempletName = comTemplet.SelectedValue + "";
                if (comTemplet.SelectedItem != null)
                {
                    KeyValuePair<string, string> selected = (KeyValuePair<string, string>)comTemplet.SelectedItem;
                    TempletName = selected.Key;
                    string templettype = string.Empty;
                    dicTempletIdType.TryGetValue(selected.Key, out templettype);
                    Type = templettype;
                }
            }
            this.Result = true;
            this.Close();
            e.Handled = true;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ckTemplet.IsChecked.Value)
                comTemplet.IsEnabled = true;
            else
                comTemplet.IsEnabled = false;
        }

    
    }
}
