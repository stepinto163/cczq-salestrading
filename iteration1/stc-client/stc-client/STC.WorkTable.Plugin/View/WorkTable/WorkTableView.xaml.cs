using STC.Common.Data;
using STC.Common.Utils;
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

namespace STC.WorkTable.Plugin.View.WorkTable
{
    /// <summary>
    /// WorkTable.xaml 的交互逻辑
    /// </summary>
    public partial class WorkTableView : UserControl
    {
        private string _oldSecuityID = "Tradebook";

        public WorkTableView()
        {
            InitializeComponent();
            dgPrice.RemoveDbDoubleClickEvent();
            dgPrice.Init(_oldSecuityID);
        }

        private void MyOrderDefaultView_SelectedRowChanged(ViewModel.Data.IntentionQuoteItem obj)
        {
            if (obj != null)
            {
                string intentionId = obj.IntentionId;

                if (!string.IsNullOrEmpty(intentionId))
                {
                    DesignHandler.Instance.IntentionLink(intentionId);
                    //this.myOrderPart.HighligthRow(intentionId);
                    //this.myExePart.HighligthRow(intentionId);
                }

                string securityObj = obj.SecurityId;
                if (!string.IsNullOrEmpty(securityObj) && _oldSecuityID != securityObj)
                {
                    dgPrice.Init(securityObj);
                    _oldSecuityID = securityObj;
                }
            }
        }
    }
}
