using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Mm.Plugin.ViewModel
{
    public class FilterListBoxItemData : ViewModelBase
    {
        private string _itemText = string.Empty;

        public string itemText
        {
            get { return _itemText; }
            set { 
                _itemText = value;
                this.OnPropertyChanged();
            }
        }

        private bool _itemIsSelected = false;
        public bool itemIsSelected
        {
            get { return _itemIsSelected; }
            set
            {
                _itemIsSelected = value;
                this.OnPropertyChanged();
            }
        }

        private string _itemMargin = "0";

        public string Itemmargin
        {
            get { return _itemMargin; }
            set
            {
                _itemMargin = value;
                this.OnPropertyChanged();
            }
        }
    }
}
