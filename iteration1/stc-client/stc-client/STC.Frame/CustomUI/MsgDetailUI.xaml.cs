using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace STC.Frame.CustomUI
{
    /// <summary>
    /// MsgDetailUI.xaml 的交互逻辑
    /// </summary>
    public partial class MsgDetailUI : UserControl
    {
        private ConcurrentDictionary<string, MsgPo> msgDic = new ConcurrentDictionary<string, MsgPo>();
        private ObservableCollection<MsgPo> showData = new ObservableCollection<MsgPo>();

        /// <summary>
        /// 1：消息提醒
        /// 2：待办事项
        /// </summary>
        private string CurrentType = "1";

        private bool isInit = true;

        public MsgDetailUI()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            this.Dispatcher.Invoke(() =>
            {
                msgDic.Clear();
                showData.Clear();

                dgMsg.DataContext = null;

                lbCondition.SelectedIndex = 0;
            });
        }

        public void SetCurrentType(string type)
        {
            CurrentType = type;
        }

        public ObservableCollection<MsgPo> ShowData { get { return showData; } }
        public List<MsgPo> GetAllData()
        {
            return msgDic.Values.ToList();
        }

        public int GetShowMsgCount()
        {
            List<MsgPo> selectMsgs = msgDic.Values.Where(delegate (MsgPo msgPo)
            {
                if (!string.IsNullOrEmpty(msgPo.Level) && (msgPo.Level.Equals("1") || msgPo.Level.Equals("2"))
                    && (string.IsNullOrEmpty(msgPo.readFlag) || !msgPo.readFlag.Contains(LocalEnv.UserInfo.UserId)))
                    return true;
                else
                    return false;
            }).ToList();

            return selectMsgs.Count;
        }

        public void OnMsg(List<TTbookSystemMessage> sysMsg)
        {
            this.Dispatcher.Invoke(() =>
            {
                bool needChange = false;
               
                int selectIndex = lbCondition.SelectedIndex;
                foreach (TTbookSystemMessage msg in sysMsg)
                {
                    if (msg.content != null && msg.type.Equals(CurrentType))
                    {
                        MsgPo msgPo = ObjectConvert.SystemMsgToShowMsg(msg, this);
                        if (msgPo != null)
                        {
                            if (msgDic.ContainsKey(msg.id))
                            {
                                foreach (MsgPo ms in showData)
                                {
                                    if (ms.Id.Equals(msgPo.Id))
                                    {
                                        ObjectConvert.UpdateMsg(ms, msgPo);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (selectIndex == 0)
                                    showData.Add(msgPo);
                                else if (selectIndex == 1 && msgPo.Level.Equals("1"))
                                    showData.Add(msgPo);
                                else if (selectIndex == 2 && msgPo.Level.Equals("2"))
                                    showData.Add(msgPo);
                                else if (selectIndex == 3 && msgPo.Level.Equals("3"))
                                    showData.Add(msgPo);
                            }
                            msgDic.AddOrUpdate(msg.id, msgPo, (key, value) => msgPo);

                            needChange = true;
                        }
                    }
                }


                if (needChange)
                    RefreshData();

                if (isInit)
                    isInit = false;
            });
        }

        private void RefreshData()
        {
            List<MsgPo> tem = showData.ToList();
            tem.Sort(new MsgSort());
            showData = new ObservableCollection<MsgPo>(tem);
            dgMsg.DataContext = null;
            dgMsg.DataContext = showData;
        }

       

        private void lbCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit)
                return;

            int selectIndex = lbCondition.SelectedIndex;
            showData.Clear();

            List<MsgPo> temDatas = msgDic.Values.ToList();

            if (selectIndex == 0)
            {
                showData = new ObservableCollection<MsgPo>(temDatas);

            }
            else if (selectIndex == 1)
            {
                foreach (MsgPo msgPo in temDatas)
                {
                    if (msgPo.Level.Equals("1"))
                        showData.Add(msgPo);
                }
            }
            else if (selectIndex == 2)
            {
                foreach (MsgPo msgPo in temDatas)
                {
                    if (msgPo.Level.Equals("2"))
                        showData.Add(msgPo);
                }
            }
            else if (selectIndex == 3)
            {
                foreach (MsgPo msgPo in temDatas)
                {
                    if (msgPo.Level.Equals("3"))
                        showData.Add(msgPo);
                }
            }

            RefreshData();
        }

        private void btnDeal_Click(object sender, RoutedEventArgs e)
        {
            MsgPo msgPo = dgMsg.CurrentItem as MsgPo;
            if (msgPo != null)
            {

            }
        }

        private void btnKnow_Click(object sender, RoutedEventArgs e)
        {
            MsgPo msgPo = dgMsg.CurrentItem as MsgPo;
            KnowMsg(msgPo);
        }

        private void KnowMsg(MsgPo msgPo)
        {
            if (msgPo != null)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        AdminHandler.Instance.UpdateSysMsg(ObjectConvert.MsgPoToSystemMessageReq(msgPo, this.CurrentType));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                });
            }
        }
    }
}
