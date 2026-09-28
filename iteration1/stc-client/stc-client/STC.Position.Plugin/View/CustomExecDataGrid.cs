using com.gateway.log;
using STC.Common.View.Component;
using STC.Position.Plugin.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.Position.Plugin.View
{
   public class CustomExecDataGrid : ExDataGrid
    {
        protected override void OnExecutedCopy(ExecutedRoutedEventArgs args)
        {
            try
            {
                //ClientDeal();
            }
            catch (Exception ex)
            {
                LogHelper.Error("CustomExecDataGrid OnExecutedCopy", ex);
            }
        }



        public void copyId()
        {
            Sub_copyId(this.SelectedItems as IList);
        }

        public void Sub_copyId(IList datas)
        {
            StringBuilder plainTxt = new StringBuilder();
            IEnumerable<PositionCheckData> list = datas.Cast<PositionCheckData>();
            if (list != null)
            {
                foreach (PositionCheckData data in list)
                {
                    if (plainTxt.Length > 0)
                    {
                        plainTxt.Append("\n");
                    }
                    string id = data.venueExecId + "";
                    plainTxt.Append(id);
                }
                //复制,不加try会导致程序无法退出
                try
                {
                    if (plainTxt.Length > 0)
                    {
                        Common.Utils.ClipboardHelper.CopyToClipboard(plainTxt.ToString(), true);
                    }
                }
                catch { }
            }
        }

      

    }

}
