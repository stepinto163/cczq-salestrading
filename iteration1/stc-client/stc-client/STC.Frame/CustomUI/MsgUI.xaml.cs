using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Frame.CustomUI
{
    /// <summary>
    /// MsgUI.xaml 的交互逻辑
    /// </summary>
    public partial class MsgUI : UserControl
    {
        private bool isInit = true;

        public MsgUI()
        {
            InitializeComponent();
            
            btnClose_Click(null, null);

            msgUI.SetCurrentType("1");
            dealMsgUI.SetCurrentType("2");
            
        }

        private void Instance_OnClear(ClearPo clearPo)
        {
            if (clearPo != null && "Y".Equals(clearPo.IsClear))
            {
                msgUI.Clear();
                dealMsgUI.Clear();

                SetShowLevel();

                Init(false);
            }
        }

        public void Init(bool isSubscribe)
        {
            //msgTask = new AsyncThreadQueue<List<TTbookSystemMessage>>(Process);

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    BaseHandler.Instance.Init(Instance_OnClear);
                    SystemMessageReq req = new SystemMessageReq();
                    //req.type = "1";
                    //req.closeby = "admin";
                    req.begintime = DateTime.Now.ToString(AdminConsts.YYYYMMdd) + " 00:00:00";
                    req.endtime = DateTime.Now.ToString(AdminConsts.YYYYMMdd) + " 24:00:00";
                    TSystemMessge systemMessage = AdminHandler.Instance.SearchSysMsg(req);

                    this.Dispatcher.Invoke(() =>
                    {
                        if (systemMessage != null)
                        {
                            msgUI.OnMsg(systemMessage.Msg);
                            dealMsgUI.OnMsg(systemMessage.Msg);

                            SetShowLevel();


                        }
                        if (isSubscribe)
                            AdminHandler.Instance.Init(OnAdminMsg);
                    });

                    isInit = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

            });
        }

        private void Process(List<TTbookSystemMessage> sysMsg)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (sysMsg != null && sysMsg.Count > 0)
                {
                    msgUI.OnMsg(sysMsg);
                    dealMsgUI.OnMsg(sysMsg);
                   
                    SetShowLevel();

                    SetPopShow(true);
                }
            });
        }

        private void SetPopShow(bool status)
        {
            this.Dispatcher.Invoke(() =>
            {
                //Console.WriteLine("Application.Current.MainWindow.IsActive:" + Application.Current.MainWindow.IsActive);
                bool isShow = status && btnMsg.Visibility == Visibility.Visible && Application.Current.MainWindow.WindowState == WindowState.Normal && Application.Current.MainWindow.IsActive;
                if (isShow)
                {
                    bool ifLeft = SystemParameters.MenuDropAlignment;
                    if (!ifLeft)
                    {
                        popMsg.HorizontalOffset = (popMsg.Width - btnMsg.Width) * (-1);
                    }

                    popMsg.Visibility = Visibility.Visible;
                    popMsg.IsOpen = isShow;
                    //popMsg.StaysOpen = true;
                    
                    ProcessPop();
                }
                else
                {
                    //popMsg.StaysOpen = false;
                    popMsg.IsOpen = false;
                    popMsg.Visibility = Visibility.Collapsed;
                    
                    firtMsg.FloatAction(false);
                    secondMsg.FloatAction(false);
                    threeMsg.FloatAction(false);

                    if (status)
                        WindowIconHelper.Instance.Start();
                }

                if (btnMsg.Visibility == Visibility.Visible)
                    border.Visibility = Visibility.Collapsed;


            });
        }

        private void ProcessPop()
        {
            List<MsgPo> temDatas = msgUI.GetAllData();
            List<MsgPo> temDealDatas = dealMsgUI.GetAllData();

            temDatas.AddRange(temDealDatas);

            temDatas.Sort(new MsgSort());
            int index = 0;
            double popHeigh = 0;
            foreach (MsgPo msgPo in temDatas)
            {
                if (index > 2)
                    break;

                ObjectConvert.ChangeStyle(msgPo, this);

                if (index == 0)
                {
                    firtMsg.Visibility = Visibility.Visible;
                    firtMsg.Init(msgPo);

                    popHeigh += firtMsg.Height;
                }
                else if (index == 1)
                {
                    secondMsg.Visibility = Visibility.Visible;
                    secondMsg.Init(msgPo);

                    popHeigh += secondMsg.Height;
                }
                else if (index == 2)
                {
                    threeMsg.Visibility = Visibility.Visible;
                    threeMsg.Init(msgPo);

                    popHeigh += threeMsg.Height;
                }
                

                index++;
            }
            popMsg.Height = popHeigh + index * 2;
        }
        

        private void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            //msgTask.Enqueue(sysMsg);

            Process(sysMsg);
        }



        private void SetShowLevel()
        {
            this.Dispatcher.Invoke(() =>
            {
                Style style = (Style)FindResource("NoticeNormalButton");
                string level = "3";
                foreach (MsgPo msgPo in msgUI.GetAllData())
                {
                    if (msgPo.Level.Equals("1"))
                    {
                        style = (Style)FindResource("NoticeUrgebtButton");
                        level = msgPo.Level;
                        break;
                    }
                    else if (msgPo.Level.Equals("2"))
                    {
                        style = (Style)FindResource("NoticeWarntButton");
                        level = msgPo.Level;
                    }
                }

                if (level.Equals("3"))
                {
                    foreach (MsgPo msgPo in dealMsgUI.GetAllData())
                    {
                        if (msgPo.Level.Equals("1"))
                        {
                            style = (Style)FindResource("NoticeUrgebtButton");
                            level = msgPo.Level;
                            break;
                        }
                        else if (msgPo.Level.Equals("2"))
                        {
                            style = (Style)FindResource("NoticeWarntButton");
                            level = msgPo.Level;
                        }
                    }
                }

                btnMsg.Style = style;

                int msgShowCount = msgUI.GetShowMsgCount();
                int dealMsgShowCount = dealMsgUI.GetShowMsgCount();
                int total = msgShowCount + dealMsgShowCount;

                if (total > 99)
                    btnMsg.Content = "99";
                else
                {
                    if (total > 0)
                        btnMsg.Content = total;
                    else
                        btnMsg.Content = string.Empty;
                }

                if (msgShowCount > 0)
                    tabMsgNotice.Header = string.Format("消息提醒[{0}]", msgShowCount);
                else
                    tabMsgNotice.Header = "消息提醒";

                if (dealMsgShowCount > 0)
                    tabDo.Header = string.Format("待办事项[{0}]", dealMsgShowCount);
                else
                    tabDo.Header = "待办事项";
            });
        }

        private void btnMsg_Click(object sender, RoutedEventArgs e)
        {
            btnMsg.Visibility = Visibility.Collapsed;
            this.Width = 360;
            this.Height = 556;

            gMsg.Visibility = Visibility.Visible;
            border.Visibility = Visibility.Visible;

            //SetPopShow(true);


        }


        public void SetAlignment()
        {
            //获取系统是以Left-handed（true）还是Right-handed（false）
            var ifLeft = SystemParameters.MenuDropAlignment;

            if (!ifLeft)
            {
                // change to false
                var t = typeof(SystemParameters);
                var field = t.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                field.SetValue(null, false);

                ifLeft = SystemParameters.MenuDropAlignment;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            btnMsg.Visibility = Visibility.Visible;
            this.Width = 65;
            this.Height = 65;

            border.Visibility = gMsg.Visibility = Visibility.Collapsed;
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void KnowMsg(MsgPo msgPo)
        {
            if (msgPo != null)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        AdminHandler.Instance.UpdateSysMsg(ObjectConvert.MsgPoToSystemMessageReq(msgPo, "1"));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                });
            }
        }

        private void btnPopDeal_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPopKnow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            this.SetPopShow(false);
        }

        private void OnStoryboardComplete(bool isOk)
        {
            if (isOk)
            {
                SetPopShow(false);
            }
        }
        
        private void popMsg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("popMsg_MouseDown");
            firtMsg.FloatAction(false);
            secondMsg.FloatAction(false);
            threeMsg.FloatAction(false);
        }

        private void popMsg_MouseLeave(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition((IInputElement)sender);
            Console.WriteLine("popMsg_MouseLeave, x:" + p.X + ", Y:" + p.Y);

            if (p.X > popMsg.Width || p.X < -250 || p.Y > popMsg.Height || p.Y < 0)
            {
                Console.WriteLine("popMsg_MouseLeave run.");
                firtMsg.FloatAction(true);
                secondMsg.FloatAction(true);
                threeMsg.FloatAction(true);
            }

        }
     
    }
}
