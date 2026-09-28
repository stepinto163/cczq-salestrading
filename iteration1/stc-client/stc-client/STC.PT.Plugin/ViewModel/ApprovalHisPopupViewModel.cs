using STC.Common.Model.PT;
using STC.PT.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PT.Plugin.ViewModel
{
    public class ApprovalHisPopupViewModel : ViewModelBase
    {
        private ObservableCollection<PTUpdatePo> _Mv  = new ObservableCollection<PTUpdatePo>();
        public ObservableCollection<PTUpdatePo> Mv
        {
            get
            {
                if (_Mv == null)
                    _Mv = new ObservableCollection<PTUpdatePo>(_Mv.OrderByDescending(item => item.Updatetime));

                return _Mv;
            }

            set
            {
                _Mv = value;
                this.OnPropertyChanged(nameof(_Mv));
            }

        }
        private string approvalorderid;
        public string Approvalorderid
        {
            get { return approvalorderid; }
            set 
            { 
                approvalorderid = value;
                this.OnPropertyChanged();
                Mv.Clear();
            }
        }
        public void setData(List<TbookPtExecorders> list) 
        {
            foreach (TbookPtExecorders pt in list) 
            {
                PTUpdatePo po = new PTUpdatePo();
                po.Approvalorderid = pt.Approvalorderid;
                po.Approvalstatus = pt.Approvalstatus;
                po.Updatetime = pt.Approvaltime;
                po.Approvalterminal = pt.Approvalterminal;
                Mv.Add(po);
            }
        }
    }
}
