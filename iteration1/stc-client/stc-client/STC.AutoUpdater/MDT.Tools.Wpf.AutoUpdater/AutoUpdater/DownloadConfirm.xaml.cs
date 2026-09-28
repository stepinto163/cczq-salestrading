using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MDT.Tools.Wpf.AutoUpdater
{
    /// <summary>
    /// DownloadConfirm.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadConfirm : Window
    {
        public DownloadConfirm()
        {
            InitializeComponent();
            this.Loaded += DownloadConfirm_Loaded;
        }


        void DownloadConfirm_Loaded(object sender, RoutedEventArgs e)
        {
            setUI();


            this.Activate();
            this.Focus();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnSkip_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = false;
            this.Close();
        }

        #region The private fields

        internal List<DownloadFileInfo> downloadFileList = null;
        public bool IsCanCancel;
        //{
        //    set { btnCancel.Visible = value; }
        //}

        #endregion

        #region The constructor of DownloadConfirm
        public DownloadConfirm(List<DownloadFileInfo> downloadfileList)
        {
            InitializeComponent();

            downloadFileList = downloadfileList;
        }

        #endregion

        private void setUI()
        {
            //label2.Text = string.Format("系统检查到有一个新版可用，你想现在就下载吗？");
            label3.Content = string.Format("名称： {0}", ConstFile.AppName);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void ForceChange()
        {

            btnSkip.Visibility = Visibility.Hidden;
            btnOk.Margin = new Thickness(370,140,0,0);

        }
    }
}
