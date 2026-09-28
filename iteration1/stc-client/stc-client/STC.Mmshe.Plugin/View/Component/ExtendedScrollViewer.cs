using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using STC.Mmshe.Plugin.View;
using System.Windows;

namespace STC.Mmshe.Plugin.View.Component
{
    public class ExtendedScrollViewer : ScrollViewer
    {
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is OrderDetailView)
            {
                if ((parentWindow as OrderDetailView).bond_txt.IsPopUpShow)
                {
                    return;
                }
            }

            base.OnMouseWheel(e);
        }
    }
}
