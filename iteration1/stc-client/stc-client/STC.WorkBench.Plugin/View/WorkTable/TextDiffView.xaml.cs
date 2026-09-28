using STC.Common.Data;
using STC.Common.Handler.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// TextDiffView.xaml 的交互逻辑
    /// </summary>
    public partial class TextDiffView : WindowBase
    {
        private string _old_text = string.Empty;
        private string _new_text = string.Empty;
        private IntentionQuoteItem _currentItem; 

        public TextDiffView(string oldTxt, string newTxt, IntentionQuoteItem item)
        {
            InitializeComponent();

            _currentItem = item;

            _old_text = oldTxt;
            _new_text = newTxt;
        }

        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            string[] oldArr = _old_text.Split(new char[] { ' ' });
            string[] newArr = _new_text.Split(new char[] { ' ' });
            List<int> sequence1 = new List<int>();
            List<int> sequence2 = new List<int>();
            compare_strings_by_sequence(oldArr, newArr, sequence1, sequence2);

            ori_txt.Document.Blocks.Clear();
            for (int i = 0; i < oldArr.Length; i++)
            {
                string subText = oldArr[i];
                if (sequence1.Contains(i))
                {
                    ori_txt.AppendText_bg($"{subText} ", "#FF9200");
                }
                else
                {
                    ori_txt.AppendText_bg($"{subText} ", "#000000");
                }
            }

            new_txt.Document.Blocks.Clear();
            for (int j = 0; j < newArr.Length; j++)
            {
                string subText = newArr[j];
                if (sequence2.Contains(j))
                {
                    new_txt.AppendText_bg($"{subText} ", "#FF9200");
                }
                else
                {
                    new_txt.AppendText_bg($"{subText} ", "#000000");
                }
            }
        }
   
        private void compare_strings_by_sequence(string[] oldArr, string[] newArr, List<int> sequence1, List<int> sequence2)
        {
            //***************************
            int j2 = 0;
            for (int i = 0; i < oldArr.Length; i++)
            {
                string item1 = oldArr[i];
                bool isfind = false;
                for(int j = j2; j < newArr.Length; j++)
                {
                    string item2 = newArr[j];
                    if (item1 == item2)
                    {
                        isfind = true;
                        j2 = j + 1;
                        break;
                    }
                }
                if (!isfind)
                {
                    sequence1.Add(i);
                }
            }

            // ***********
            int i1 = 0;
            for (int j = 0; j < newArr.Length; j++)
            {
                string item2 = newArr[j];
                bool isfind = false;
                for (int i = i1; i < oldArr.Length; i++)
                {
                    string item1 = oldArr[i];
                    if (item1 == item2)
                    {
                        isfind = true;
                        i1 = i + 1;
                        break;
                    }
                }
                if (!isfind)
                {
                    sequence2.Add(j);
                }
            }

        }

        private void know_btn_Click(object sender, RoutedEventArgs e)
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                // 标记已查看
                STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                request.intentionId = _currentItem.IntentionId;
                request.actionType = "80";// 查看
                request.userName = _currentItem.BrokerId;
                request.closeBy = LocalEnv.UserInfo.UserName;
                request.closeByName = LocalEnv.UserInfo.FullName;
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    request.quoteType = "BUYER_INTENTION";
                }
                else
                {
                    request.quoteType = "INTENTION";
                }
                STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo replyPo = IntentionOldHandler.Instance.ConfirmIntentionQuote(request);
                if (replyPo != null && replyPo.IsSuccess())
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.Close();
                    }));
                }
            });
        }
    }
}

