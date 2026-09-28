using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.XBond.Plugin.ViewModel;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public class MultipleCheckboxModel : ViewModelBase
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                //NotifyPropertyChanged(x => x.IsSelected);
            }
        }
    }
}
