using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using com.gateway.log;
using STC.Common.Model.Admin;
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

namespace BDB.Trade.Plugin.View.Trade.Card
{
    /// <summary>
    /// PageCardView.xaml 的交互逻辑
    /// </summary>
    public partial class PageCardView : UserControl
    {
        private PageCardViewModel pageCardViewModel = new PageCardViewModel();
        private string currentGroupId = string.Empty;
        

        public PageCardView()
        {
            InitializeComponent();

            this.DataContext = pageCardViewModel;

            btAdd.Click += BtAdd_Click;
            btLeft.Click += BtLeft_Click;
            btRight.Click += BtRight_Click;

            QuoteConfigHandler.Instance.OnDeletePage += Instance_OnDeletePage;
            QuoteConfigHandler.Instance.OnAddPanl += Instance_OnAddPanl;

            pageCardViewModel.InitSub();
        }

        

        #region 按钮事件

        private void BtRight_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1);
        }

        public void HomePage()
        {
            pageCardViewModel.PageSelectIndex = 0;

            SetRightVisibility();
        }

        public void LoopPage()
        {
            int selectPageNum = tcGroupPage.SelectedIndex;
            if (selectPageNum >= 0)
            {
                int toPage = pageCardViewModel.PageSelectIndex + 1;
                if (toPage >= pageCardViewModel.PageButtonList.Count)
                    toPage = tcGroupPage.Items.Count - 1;

                pageCardViewModel.PageSelectIndex = toPage;

            }

            SetRightVisibility();
        }

        private void SetRightVisibility()
        {
            //if (tcGroupPage.SelectedIndex == (tcGroupPage.Items.Count - 1))
            //{
            //    btRight.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    btRight.Visibility = Visibility.Visible;
            //}
        }

        public void ChangePage(int v)
        {
            if (pageCardViewModel.PageButtonList.Count > 1)
            {
                int selectPageNum = tcGroupPage.SelectedIndex;
                if (selectPageNum >= 0)
                {
                    int toPage = pageCardViewModel.PageSelectIndex + v;
                    if (toPage >= pageCardViewModel.PageButtonList.Count)
                        toPage = tcGroupPage.Items.Count - 1;

                    //pageCardViewModel.PageSelectIndex = toPage >= 0? toPage: 0;

                    pageCardViewModel.SetPageIndexAndResetPage(toPage >= 0 ? toPage : 0, DataType.SQLite);

                }

                SetRightVisibility();
            }
        }

        private void BtLeft_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(-1);

        }

        
        private void BtAdd_Click(object sender, RoutedEventArgs e)
        {
            btAdd.Visibility = Visibility.Collapsed;
            btRight.Visibility = Visibility.Visible;
            List<HomeGroupSecurityIDsInfo> securityList = new List<HomeGroupSecurityIDsInfo>();
            int pageCount = LayoutSetttingCache.Instance.layoutSettingPo.Row * LayoutSetttingCache.Instance.layoutSettingPo.Col;
            if (securityList.Count < pageCount)
            {
                for (int i = 0; i < pageCount; i++)
                {
                    securityList.Add(null);
                }
            }

            int addPageNum = pageCardViewModel.PageButtonList.Count + 1;
            pageCardViewModel.AddButton(addPageNum.ToString());

            QuoteConfigPo quoteConfigPo = QuoteConfigHandler.Instance.GetConfigByGroupId(pageCardViewModel.CurrentGroupId);//QueryConfig(pageCardViewModel.CurrentGroupId);
            if (quoteConfigPo != null)
            {
                quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;
                QcPagePo newPage = new QcPagePo();
                newPage.GroupId = pageCardViewModel.CurrentGroupId;
                newPage.Index = addPageNum;
                LogHelper.Info("Add page, index:{0}, button count:{1}", new object[] { addPageNum, pageCardViewModel.PageButtonList.Count });
                newPage.IsSeleted = true;
                newPage.Panels = new List<QcPanelPo>();

                for(int i = 0; i < pageCount; i++)
                {
                    QcPanelPo panelPo = new QcPanelPo();
                    panelPo.Id = string.Empty;
                    panelPo.BrokerId = string.Empty;

                    newPage.Panels.Add(panelPo);
                }

                quoteConfigPo.Pages.Add(newPage);

                QuoteConfigHandler.Instance.SaveConfig(quoteConfigPo, pageCardViewModel.CurrentGroupId);
            }

            pageCardViewModel.SetPageIndexAndResetPage(addPageNum - 1, DataType.AddEmptyPage);
        }

        #endregion

        //删除分页
        private void Instance_OnDeletePage(QcPagePo obj)
        {
            if (pageCardViewModel.CurrentGroupId.Equals(obj.GroupId))
            {

                int addPageNum = obj.Index - 2;


                if (addPageNum >= 0)
                    pageCardViewModel.DeleteButton(obj.Index.ToString());

                pageCardViewModel.PageSelectIndex = addPageNum >= 0 ? addPageNum : 0;

                if (pageCardViewModel.PageButtonList.IndexOf(obj.Index.ToString()) == (pageCardViewModel.PageButtonList.Count - 1))
                    btAdd.Visibility = Visibility.Visible;

                pageCardViewModel.SetPageIndexAndResetPage(pageCardViewModel.PageSelectIndex, DataType.SQLite);

            }
        }

        //分页里面添加债券
        private void Instance_OnAddPanl(QcPagePo obj)
        {
            if (obj.GroupId == pageCardViewModel.CurrentGroupId && pageCardViewModel.PageSelectIndex == obj.Index - 1
                && obj.Panels != null && obj.Panels.Count > 0)
            {
                btAdd.Visibility = Visibility.Visible;
            }
        }


        public void ClearUI()
        {
            tcGroupPage.Items.Clear();

            pageCardViewModel.PageButtonList.Clear();
        }
        
        public void Init(string groupId, bool refresh)
        {
            pageCardViewModel.Init(this, groupId, refresh);

        }

        public void SetGroupId(string groupId)
        {
            pageCardViewModel.SetCurrentGroupId(groupId);
        }


        private void btAdd_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (btAdd.Visibility == Visibility.Visible)
            {
                tempTxt.Visibility = Visibility.Collapsed;
               
            }
            else
            {
                tempTxt.Visibility = Visibility.Visible;
            }
        }
    }
}
