using STC.Common.Model.Admin;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Concurrent;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace STC.XBond.Plugin.CustomeUI
{
    public partial class MsgFloatView : UserControl
    {
        private MsgFloatViewModel _msgFloatViewModel = null;

        public MsgFloatView()
        {
            InitializeComponent();

            _msgFloatViewModel = new MsgFloatViewModel();
            this.DataContext = _msgFloatViewModel;

            _msgFloatViewModel.Init(this);
        }

        public void OnMsg(MsgPo data)
        {
            _msgFloatViewModel.OnMsg(data);
        }

        public void ClearMsg()
        {
            _msgFloatViewModel.ClearMsg();
        }

        public bool GetStatus()
        {
            return _msgFloatViewModel.GetStatus();
        }

        public void KnowMsg(MsgPo msgPo)
        {
            _msgFloatViewModel.KnowMsg(msgPo);
        }

        private object _dataGridLock = new object();
        private Dictionary<string, bool> _dic = new Dictionary<string, bool>();
        private void DgMsg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //lock (_dataGridLock)
            {
                foreach(MsgPo item in this.dgMsg.Items)
                {
                    DataGridRow targetRow = this.dgMsg.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    if (item.InnerColorAnimation != null)
                    {
                        targetRow.Loaded += Row_Loaded;
                    }
                }
            }
        }

        private ConcurrentDictionary<string, Storyboard> _storyDic = new ConcurrentDictionary<string, Storyboard>();

        public void ClearAllStorys()
        {
            foreach(Storyboard sb in _storyDic.Values)
            {
                sb.Stop();
            }
            _storyDic.Clear();
        }

        private void Row_Loaded(object sender, RoutedEventArgs e)
        {
            //lock (_dataGridLock)
            {
                DataGridRow dataGridRow = sender as DataGridRow;
                MsgPo mp = dataGridRow.DataContext as MsgPo;
                ContentPresenter contentProesenter = dgMsg.Columns[0].GetCellContent(sender as DataGridRow) as ContentPresenter;

                FloatDetailView floatDetailView = contentProesenter.ContentTemplate.FindName("floatDetailView", contentProesenter) as FloatDetailView;
                if (floatDetailView != null)
                {
                    MsgPo msgPo = floatDetailView.DataContext as MsgPo;

                    if (_storyDic.ContainsKey(msgPo.Id))
                    {
                        Storyboard oldStory = _storyDic[msgPo.Id];
                        oldStory.Stop();
                    }

                    if (msgPo != null)
                    {
                        msgPo.PropertyChanged += MsgPo_PropertyChanged;
                        Border border = floatDetailView.FindName("border") as Border;
                        border.Background = msgPo.ShowBG;

                        if (msgPo.InnerColorAnimation != null)
                        {
                            StartStory(msgPo, border);
                        }
                    }
                }
            }
        }

        private void MsgPo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InnerColorAnimation")
            {

                MsgPo msgPo = sender as MsgPo;
                if (msgPo != null && msgPo.InnerColorAnimation != null)
                {
                    DataGridRow data = this.dgMsg.ItemContainerGenerator.ContainerFromItem(msgPo) as DataGridRow;
                    ContentPresenter contentProesenter = dgMsg.Columns[0].GetCellContent(data) as ContentPresenter;

                    FloatDetailView floatDetailView = contentProesenter.ContentTemplate.FindName("floatDetailView", contentProesenter) as FloatDetailView;
                    if (floatDetailView != null)
                    {
                        Border border = floatDetailView.FindName("border") as Border;
                        if (msgPo.InnerColorAnimation != null)
                        {
                            StartStory(msgPo, border);
                        }
                    }
                }
            }
        }

        private void StartStory(MsgPo msgPo, Border border)
        {
            if (msgPo.InnerColorAnimation != null)
            {
                Storyboard story = new Storyboard();
                story.AutoReverse = false;
                if (msgPo.ContentType.Equals(AdminConsts.Admin_IT))
                    story.RepeatBehavior = RepeatBehavior.Forever;
                else
                {
                    msgPo.InnerColorAnimation.FillBehavior = FillBehavior.Stop;
                    story.Completed += Story_Completed;
                }

                story.Children.Add(msgPo.InnerColorAnimation);


                DependencyProperty[] propertyChain = new DependencyProperty[]
                    {
                                    Border.BackgroundProperty,
                                    SolidColorBrush.ColorProperty
                    };

                Storyboard.SetTarget(msgPo.InnerColorAnimation, border);
                Storyboard.SetTargetProperty(msgPo.InnerColorAnimation, new PropertyPath("(0).(1)", propertyChain));
                try
                {
                    story.Name = msgPo.Id.ToString();
                }
                catch (Exception exx) { }
                //story.Children.Add(msgPo.InnerColorAnimation);

                //story.Begin();
                _storyDic[msgPo.Id] = story;
                story.Begin();
                msgPo.InnerColorAnimation = null;
            }
        }

        private void Story_Completed(object sender, EventArgs e)
        {
            //线程补充没有完成的自动退出
            ClockGroup cg = sender as ClockGroup;
            string id = cg.Timeline.Name;
            //cg.Completed -= Storyboard_Completed;
            Console.WriteLine("------------------Remove---Id:" + id);
            MsgPo msgPo = this.GetMsgById(id);

            if (msgPo != null)
            {
                msgPo.TimeOut = true;
            }

            if (_storyDic.ContainsKey(id))
            {
                Storyboard removeStory;
                _storyDic.TryRemove(id, out removeStory);
            }

            _msgFloatViewModel.OnStoryboard();
        }

        public MsgPo GetMsgById(string id)
        {
            return _msgFloatViewModel.GetMsgById(id);
        }

        private void DgMsg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_msgFloatViewModel != null)
            {
                _msgFloatViewModel.SendQQ(dgMsg.SelectedItem as MsgPo);

                _msgFloatViewModel.PopDealClick();
            }
        }


        private void dgMsg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    MsgPo msgPo = (sender as DataGrid).CurrentItem as MsgPo;

                    MsgPo msgPo2 = (sender as DataGrid).SelectedItem as MsgPo;

                    if (msgPo2 != null)
                    {
                        msgPo = msgPo2;
                    }
                    if (msgPo != null)
                    {
                        //设置消息为已读
                        if (_msgFloatViewModel != null)
                        {
                            _msgFloatViewModel.SetReadflag(msgPo);
                        }
                    }
                }
                catch
                { 
                    
                }
            });
        }
    }
}
