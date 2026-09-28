using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.quant.plugin.ViewModel.WorkTable
{
    public class RandomBtnModel : ViewModelBase
    {
        private string _content = string.Empty;
        public string Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}
