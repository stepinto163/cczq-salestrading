using STC.Common.Handler.Admin;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Mm.Plugin.ViewModel
{
    public class MMSettingViewModel : BaseViewModel
    {
        private bool _Tip1Enable = true;
        private bool _Tip2Enable = true;
        private bool _Tip3Enable = true;
        private bool _BncMoniEnable;
        private string _BncMoniValue;
        private bool _BcoMoniEnable;
        private string _BcoMoniValue;
        private bool _Tip4Enable = true;
        public bool Tip4Enable
        {
            get
            {
                return _Tip4Enable;
            }

            set
            {
                _Tip4Enable = value;
                this.OnPropertyChanged();
            }
        }

        public bool Tip1Enable
        {
            get
            {
                return _Tip1Enable;
            }

            set
            {
                _Tip1Enable = value;
                this.OnPropertyChanged();
            }
        }

        public bool Tip2Enable
        {
            get
            {
                return _Tip2Enable;
            }

            set
            {
                _Tip2Enable = value;
                this.OnPropertyChanged();
            }
        }
        public bool Tip3Enable
        {
            get
            {
                return _Tip3Enable;
            }

            set
            {
                _Tip3Enable = value;
                this.OnPropertyChanged();
            }
        }

        public bool BncMoniEnable
        {
            get
            {
                return _BncMoniEnable;
            }

            set
            {
                _BncMoniEnable = value;
                this.OnPropertyChanged();
            }
        }

        public string BncMoniValue
        {
            get
            {
                return _BncMoniValue;
            }

            set
            {
                _BncMoniValue = value;
                this.OnPropertyChanged();
            }
        }

        public bool BcoMoniEnable
        {
            get
            {
                return _BcoMoniEnable;
            }

            set
            {
                _BcoMoniEnable = value;
                this.OnPropertyChanged();
            }
        }

        public string BcoMoniValue
        {
            get
            {
                return _BcoMoniValue;
            }

            set
            {
                _BcoMoniValue = value;
                this.OnPropertyChanged();
            }
        }
        private bool _saveIsEnable = true;

        public bool SaveIsEnable
        {
            get
            {
                return _saveIsEnable;
            }

            set
            {
                _saveIsEnable = value;
            }
        }

        private bool _dealCloseAll = false;
        public bool DealCloseAll
        {
            get
            {
                return _dealCloseAll;
            }

            set
            {
                _dealCloseAll = value;
                setGroupCheckEnable();
                this.OnPropertyChanged();
            }
        }


        private ObservableCollection<DealCloseGroupObj> _dealCloseGroupObjs = new ObservableCollection<DealCloseGroupObj>();
        public ObservableCollection<DealCloseGroupObj> DealCloseGroupObjs
        {
            get
            {
                return _dealCloseGroupObjs;
            }

            set
            {
                _dealCloseGroupObjs = value; 
                this.OnPropertyChanged();
            }
        }

        private Visibility _dealCloseEnable = Visibility.Hidden;
        public Visibility DealCloseEnable
        {
            get
            {
                return _dealCloseEnable;
            }

            set
            {
                _dealCloseEnable = value;
                this.OnPropertyChanged();
            }
        }

        /* private bool _groupCheckEnable = false;
         public bool GroupCheckEnable
         {
             get
             {
                 return _groupCheckEnable;
             }

             set
             {
                 _groupCheckEnable = value;
                 this.OnPropertyChanged();
             }
         }*/

        #region 树状分组数据源
        private ObservableCollection<TreeNodeModel> _MultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        {
            get { return _MultiTreeComboxItemsSource; }
            set
            {
                if (_MultiTreeComboxItemsSource != value)
                {
                    _MultiTreeComboxItemsSource = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _SelectedItem = null;
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value;
                this.OnPropertyChanged();
            }
        }


        private string _SelectedText = null;
        public string SelectedText
        {
            get { return _SelectedText; }
            set
            {
                _SelectedText = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        private UserSetting userSetting;

        public static event Action MoniValueChange;
        public List<ChildTag> bookChildTagList = new List<ChildTag>();
        public void init()
        {
            //convertBookPo();
            userSetting = new UserSetting();

            MultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();

            select();
        }
        private void convertBookPo() 
        {
            bookChildTagList.Clear();
            ChildTag tag = new ChildTag();
            tag.childTag = "1";
            tag.childTagName = "组合1";
            tag.childTags = new List<ChildTag>();
            tag.childTags.Add(new ChildTag { 
                childTag = "2", 
                childTagName = "第二组合2"
            });
            bookChildTagList.Add(tag);
            //foreach (KeyValuePair<string, ObservableCollection<TBookBookTreeItemModel>> keyValuePair in dicBookUser) 
            //{
            //    ObservableCollection<TBookBookTreeItemModel> bookList = keyValuePair.Value;
            //    foreach (TBookBookTreeItemModel itemModel in bookList) 
            //    {
            //    }
            //}
        }

        private void select()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                UserSettingReplyPo result = MMHandler.Instance.queryMMUserSetting();
                if (result == null)
                    return;
                DealCloseEnableReply enableReply = MMHandler.Instance.queryDealCloseEnableSetting();
           
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DealCloseGroupObjs.Clear();
                    if (result.Data != null)
                    {
                        userSetting = result.Data;
                        if (!string.IsNullOrEmpty(userSetting.Tip1Enable))
                        { Tip1Enable = (!string.IsNullOrEmpty(userSetting.Tip1Enable) && "1".Equals(userSetting.Tip1Enable)) ? true : false; }

                        if (!string.IsNullOrEmpty(userSetting.Tip2Enable))
                        { Tip2Enable = (!string.IsNullOrEmpty(userSetting.Tip2Enable) && "1".Equals(userSetting.Tip2Enable)) ? true : false; }

                        if (!string.IsNullOrEmpty(userSetting.Tip3Enable))
                        { Tip3Enable = (!string.IsNullOrEmpty(userSetting.Tip3Enable) && "1".Equals(userSetting.Tip3Enable)) ? true : false; }

                        if (!string.IsNullOrEmpty(userSetting.Tip4Enable))
                        { Tip4Enable = (!string.IsNullOrEmpty(userSetting.Tip4Enable) && "1".Equals(userSetting.Tip4Enable)) ? true : false; }

                        BncMoniEnable = (!string.IsNullOrEmpty(userSetting.BncMoniEnable) && "1".Equals(userSetting.BncMoniEnable)) ? true : false;
                        BncMoniValue = !string.IsNullOrEmpty(userSetting.BncMoniValue) ? userSetting.BncMoniValue : "";

                        BcoMoniEnable = (!string.IsNullOrEmpty(userSetting.BcoMoniEnable) && "1".Equals(userSetting.BcoMoniEnable)) ? true : false;
                        BcoMoniValue = !string.IsNullOrEmpty(userSetting.BcoMoniValue) ? userSetting.BcoMoniValue : "";

                        DealCloseAll = (!string.IsNullOrEmpty(userSetting.DealCloseAll) && "1".Equals(userSetting.DealCloseAll)) ? true : false;
                        if (enableReply != null && enableReply.Data)
                        {
                            DealCloseEnable = Visibility.Visible;
                            if (userSetting.DealCloseGroups != null && userSetting.DealCloseGroups.Count > 0)
                            {
                                convertData(userSetting.DealCloseGroups);
                            }
                            else
                            {
                                DealCloseGroupObjs.Clear();
                            }
                        }
                        else
                        {
                            DealCloseEnable = Visibility.Hidden;
                            DealCloseGroupObjs.Clear();
                        }

                        //接续手工单默认组合
                        this.SelectedItem = userSetting.BookId ?? null;
                        //SelectedBookIdTexts = userSetting.BookId;
                        //SelectedBookNameTexts = userSetting.BookName;
                    }
                    else
                    {
                        if (enableReply != null && enableReply.Data)
                        {
                            DealCloseEnable = Visibility.Visible;

                        }
                        else 
                        {
                            DealCloseEnable = Visibility.Hidden;
                        }
                    }
                });
            });
        }

        private void convertData(List<DealCloseGroupPo> DealCloseGroups)
        {
            try
            {
                DealCloseGroupObjs.Clear();
                if (DealCloseGroups != null)
                {
                    foreach (DealCloseGroupPo po in DealCloseGroups)
                    {
                        DealCloseGroupObj obj = new DealCloseGroupObj();
                        obj.GroupId = po.GroupId;
                        obj.GroupName = po.GroupName;
                        obj.DealCloseGroup = !string.IsNullOrEmpty(po.DealCloseGroup) && "1".Equals(po.DealCloseGroup) ? true : false;
                        if (DealCloseAll)
                        {
                            obj.GroupCheckEnable = false;
                        }
                        else
                        {
                            obj.GroupCheckEnable = true;
                        }
                        DealCloseGroupObjs.Add(obj);
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void setGroupCheckEnable() 
        {
            if (DealCloseGroupObjs != null) 
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    convertData(userSetting.DealCloseGroups);
                });
            }
        }

        private void setUserSettingGroupsData() 
        {
            if (DealCloseGroupObjs != null && DealCloseGroupObjs.Count() > 0 && userSetting.DealCloseGroups != null)
            {
                userSetting.DealCloseGroups.Clear();
                foreach (DealCloseGroupObj po in DealCloseGroupObjs)
                {
                    DealCloseGroupPo obj = new DealCloseGroupPo();
                    obj.GroupId = po.GroupId;
                    obj.GroupName = po.GroupName;
                    obj.DealCloseGroup = po.DealCloseGroup ? "1" : "0";
                    userSetting.DealCloseGroups.Add(obj);
                }
            }
        }

        public void OnOKClick()
        {
            saveIsEnableClick(false);
            if (checkData())
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    msgBox.setWarnMsg(string.Format($"{failureMsg}."), true);
                    msgBox.ShowDialog();
                }));
                return;
            }
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                userSetting.Tip1Enable = Tip1Enable ? "1" : "0";
                userSetting.Tip2Enable = Tip2Enable ? "1" : "0";
                userSetting.Tip3Enable = Tip3Enable ? "1" : "0";
                userSetting.Tip4Enable = Tip4Enable ? "1" : "0";
                userSetting.BncMoniEnable = BncMoniEnable ? "1" : "0";
                userSetting.BncMoniValue = BncMoniValue;
                userSetting.BcoMoniEnable = BcoMoniEnable ? "1" : "0";
                userSetting.BcoMoniValue = BcoMoniValue;
                userSetting.DealCloseAll = DealCloseAll ? "1" : "0";


                //手工单默认组合
                if (SelectedItem != null && !string.IsNullOrEmpty(SelectedItem))
                {
                    userSetting.BookId = SelectedItem;
                    userSetting.BookName = SelectedText;
                }
                else 
                {
                    userSetting.BookId = "";
                    userSetting.BookName = "";
                }

                setUserSettingGroupsData();
                PResult reply = MMHandler.Instance.updateMMUserSetting(userSetting);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            MessageBoxGUI msgBox = new MessageBoxGUI();
                            msgBox.setWarnMsg("保存成功", true);
                            msgBox.ShowDialog();
                            MainViewModel.Tip1Enable = Tip1Enable;
                            MainViewModel.Tip2Enable = Tip2Enable;
                            MainViewModel.Tip4Enable = Tip4Enable;
                            if(BncMoniEnable)
                            {
                                MainViewModel.BncMoniValue = BncMoniValue;
                            }
                            else
                            {
                                MainViewModel.BncMoniValue = string.Empty;
                            }
                            if (BcoMoniEnable)
                            {
                                MainViewModel.BcoMoniValue = BcoMoniValue;
                            }
                            else
                            {
                                MainViewModel.BcoMoniValue = string.Empty;
                            }
                            MoniValueChange?.Invoke();
                        }));
                    }
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            MessageBoxGUI msgBox = new MessageBoxGUI();
                            msgBox.setWarnMsg(string.Format($"更新失败, 原因:{reply.Msg}."), true);
                            msgBox.ShowDialog();
                        }));
                    }
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg(string.Format($"系统异常"), true);
                        msgBox.ShowDialog();
                    }));
                }
                saveIsEnableClick(true);
            });
        }
        private void saveIsEnableClick(bool status)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                SaveIsEnable = status;
            }));
        }
        private string failureMsg = "";
        private bool checkData()
        {
            bool status = false;
            failureMsg = "";
            if (BncMoniEnable && string.IsNullOrEmpty(BncMoniValue))
            {
                failureMsg = "<利率债>允许输入范围[0,1000]";
                status = true;
                return status;
            }
            else if (BncMoniEnable)
            {
                decimal? value = "".Equals(BncMoniValue) ? -1 : (checkNum(BncMoniValue)? -1 : decimal.Parse(BncMoniValue));
                if (value < 0 || value > 1000)
                {
                    if (!"".Equals(failureMsg))
                    {
                        failureMsg = string.Format("{0}\n{1}", failureMsg, "<利率债>允许输入范围[0,1000]");
                    }
                    else
                    {
                        failureMsg = "<利率债>允许输入范围[0,1000]";
                    }
                    status = true;
                    return status;
                }
            }
            if (BcoMoniEnable && string.IsNullOrEmpty(BcoMoniValue))
            {
                failureMsg = "<信用债>允许输入范围[0,1000]";
                status = true;
                return status;
            }
            else if (BcoMoniEnable)
            {
                decimal? value = "".Equals(BcoMoniValue) ? -1 : (checkNum(BcoMoniValue) ? -1 : decimal.Parse(BcoMoniValue));
                if (value < 0 || value > 1000)
                {
                    if (!"".Equals(failureMsg))
                    {
                        failureMsg = string.Format("{0}\n{1}", failureMsg, "<信用债>允许输入范围[0,1000]");
                    }
                    else
                    {
                        failureMsg = "<信用债>允许输入范围[0,1000]";
                    }
                    status = true;
                    return status;
                }
            }


            return status;
        }
        public bool checkNum(string text)
        {
            if (text != null && text != "")
            {
                int j = 0;
                char[] chars = text.ToCharArray();
                for (int i = 0; i < text.Length; i++)
                {

                    if (chars[i] == '.')
                    {

                        j++;

                    }
                }
                if (j > 1 || (chars.Length >= 0 && chars[0] == '.'))
                {
                    return true;
                }
                if (chars.Length >= 2 && (chars[0] == '0' && chars[1] != '.'))
                {
                    return true;
                }
                if (chars.Length == 2 && (chars[0] == '0' && chars[1] == '.'))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();
                me.tbTitle.Text = "提示";
                me.setWarnMsg(message);
                me.Owner = Application.Current.MainWindow;
                me.ShowDialog();
            }));
        }
    }
}
