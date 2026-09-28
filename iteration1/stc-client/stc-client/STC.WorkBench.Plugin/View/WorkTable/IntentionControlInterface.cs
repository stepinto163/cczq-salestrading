using STC.WorkBench.Plugin.View.Component;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkBench.Plugin.View.WorkTable
{
    public interface IntentionControlInterface
    {
        CustomDefaultDataGrid GetCustomDefaultDataGrid();

        Action ClickWorkingDataGridAction { get; set; }

        Action OpenInquiryAction { get; set; }

        Action CloseInquiryAction { get; set; }

        Action<IntentionQuoteItem> ChangeSelectedRowAction { get; set; }
    }
}
