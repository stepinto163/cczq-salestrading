using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.FinQ;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.WorkBench.Plugin.View;
using STC.WorkBench.Plugin.View.WorkTable;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class IntentionControl_Sell_ViewModel : MyOrderDefaultViewModel
    {
    }
}
