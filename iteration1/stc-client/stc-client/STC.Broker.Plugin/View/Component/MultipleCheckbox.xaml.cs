using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// MultipleCheckbox.xaml 的交互逻辑
    /// </summary>
    public partial class MultipleCheckbox : UserControl
    {
        ListBox lb;
        TextBlock tb;
        public MultipleCheckbox()
        {
            InitializeComponent();
            combobox.ApplyTemplate();
            lb = combobox.Template.FindName("lb", combobox) as ListBox;
            tb = combobox.Template.FindName("tb", combobox) as TextBlock;
            Datas = new List<string> { "AAA", "BBB", "CCC" };
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// 下拉选项
        /// </summary>
        private List<string> datas = new List<string>();

        public List<string> Datas
        {
            get { return datas; }
            set
            {
                datas = value;
                lb.ItemsSource = datas;
            }
        }
        /// <summary>
        /// 默认选中项
        /// </summary>
        private string defaultSelect;
        public string DefaultSelect
        {
            get { return defaultSelect; }
            set
            {
                tb.Text = defaultSelect = value;
            }
        }
        /// <summary>
        /// 默认选中项
        /// </summary>
        private bool isOpen;
        public bool IsOpen
        {
            get { return isOpen; }
            set
            {
                if (value != isOpen)
                    isOpen = value;
                NotifyPropertyChanged("IsOpen");
            }
        }


        public bool IsSingle { get; set; }


        /// <summary>
        /// 当前选中项
        /// </summary>
        public List<string> selects = new List<string>();
        public List<string> Selects
        {
            get { return selects; }
            set { selects = value; }
        }
        /// <summary>
        /// 事件
        /// </summary>
        public Action<List<string>> SelectChanged;
        //事件触发方法
        private void OnSelectChange()
        {
            SelectChanged?.Invoke(Selects);
        }

        /// <summary>
        /// 选中项改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb1_Click(object sender, RoutedEventArgs e)
        {

            CheckBox cb = sender as CheckBox;
            if (cb.Content.ToString() == DefaultSelect)
            {
                uncheckOtherSelects(DefaultSelect);
            }
            else
            {
                uncheckItem(DefaultSelect);
                if (IsSingle)
                {
                    foreach (var item in selects)
                    {
                        uncheckItem(item);
                    }
                }
            }
            setSelects();
            if (selects.Count == 0)
                checkItem(DefaultSelect);
            setSelects();
            OnSelectChange();
        }
        /// <summary>
        /// 设置选择项
        /// </summary>
        void setSelects()
        {
            Selects.Clear();
            int total = 0;
            string str = string.Empty;
            for (int i = 0; i < lb.Items.Count; i++)
            {
                ListBoxItem listBoxItem = lb.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                // 当lb1未loaded ，listBoxItem为空
                if (listBoxItem != null)
                {
                    CheckBox cb = listBoxItem.Template.FindName("cb1", listBoxItem) as CheckBox;
                    if (cb.IsChecked.HasValue && cb.IsChecked.Equals(true))
                    {
                        Selects.Add(listBoxItem.Content.ToString());
                        total++;
                        if (str == "")
                            str = cb.Content.ToString();
                        else
                            str = str + "," + cb.Content.ToString();
                    }
                }
            }
            tb.Text = str.TrimEnd(new char[] { ',' });
        }

        /// <summary>
        /// uncheck所有选项
        /// </summary>
        private void uncheckAllSelects()
        {
            //取消所有勾选
            for (int i = 0; i < lb.Items.Count; i++)
            {
                ListBoxItem listBoxItem = lb.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                // 当lb1未loaded ，listBoxItem为空
                if (listBoxItem != null)
                {
                    CheckBox cb = listBoxItem.Template.FindName("cb1", listBoxItem) as CheckBox;
                    if (cb.IsChecked.HasValue && cb.IsChecked.Equals(true))
                    {
                        cb.IsChecked = false;
                    }
                }
            }
        }

        /// <summary>
        /// uncheck其他
        /// </summary>
        private void uncheckOtherSelects(string current)
        {
            //取消所有勾选
            for (int i = 0; i < lb.Items.Count; i++)
            {
                ListBoxItem listBoxItem = lb.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                // 当lb1未loaded ，listBoxItem为空
                if (listBoxItem != null)
                {
                    CheckBox cb = listBoxItem.Template.FindName("cb1", listBoxItem) as CheckBox;
                    if (cb.Content.ToString() != current && cb.IsChecked.HasValue && cb.IsChecked.Equals(true))
                    {
                        cb.IsChecked = false;
                    }
                }
            }
        }
        /// <summary>
        /// uncheck选项
        /// </summary>
        private void uncheckItem(string item)
        {
            //取消所有勾选
            for (int i = 0; i < lb.Items.Count; i++)
            {
                ListBoxItem listBoxItem = lb.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                // 当lb1未loaded ，listBoxItem为空
                if (listBoxItem != null)
                {
                    CheckBox cb = listBoxItem.Template.FindName("cb1", listBoxItem) as CheckBox;
                    if (cb.Content.ToString() == item && cb.IsChecked.HasValue && cb.IsChecked.Equals(true))
                    {
                        cb.IsChecked = false;
                    }
                }
            }
        }
        /// <summary>
        /// check选项
        /// </summary>
        private void checkItem(string item)
        {
            //取消所有勾选
            for (int i = 0; i < lb.Items.Count; i++)
            {
                ListBoxItem listBoxItem = lb.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                // 当lb1未loaded ，listBoxItem为空
                if (listBoxItem != null)
                {
                    CheckBox cb = listBoxItem.Template.FindName("cb1", listBoxItem) as CheckBox;
                    if (cb.Content.ToString() == item)
                    {
                        cb.IsChecked = true;
                    }
                }
            }
        }
        /// <summary>
        /// 下拉框弹出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_Click(object sender, RoutedEventArgs e)
        { 
           // popBondCode.IsOpen = (bool)(sender as CheckBox).IsChecked;
        }

        /// <summary>
        /// 默认选中设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lb_Loaded(object sender, RoutedEventArgs e)
        {
            if (selects.Count == 0)
                checkItem(DefaultSelect);

        }

    }
}
