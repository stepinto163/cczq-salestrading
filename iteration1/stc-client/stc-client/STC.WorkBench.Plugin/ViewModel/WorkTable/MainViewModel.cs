using STC.Common.Model;
using STC.Common.Utils;
using STC.WorkBench.Plugin.Utils;
using STC.WorkBench.Plugin.View.WorkTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand QuoteIdentifyCommand { get; set; }

        public MainViewModel()
        {
            this.QuoteIdentifyCommand = new ComRelayCommand(this.DoQuoteIdentifyCommand);
        }

        private void DoQuoteIdentifyCommand()
        {
            if (!WindowManager.Instance.IsExist(WindowKeyHelper.RECOGENIZEQUOTE))
            {
                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    RecognizeIntentionQuoteWin_PA win = new RecognizeIntentionQuoteWin_PA();
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.RECOGENIZEQUOTE, win);
                }
                else
                {
                    RecognizeIntentionQuoteWin win = new RecognizeIntentionQuoteWin();
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.RECOGENIZEQUOTE, win);
                }
            }
        }

    }
}
