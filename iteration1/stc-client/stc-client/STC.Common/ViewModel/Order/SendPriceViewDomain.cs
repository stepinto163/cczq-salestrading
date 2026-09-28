using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace STC.Common.ViewModel.Order
{
    public class SendPriceViewDomain : BaseViewModel
    {
        /// <summary>
        /// 边框颜色
        /// </summary>
        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                }
            }
        }


    }
}
