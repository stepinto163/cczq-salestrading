using  STC.Broker.Plugin.Utils.wpf.Interface;
using STC.Broker.Plugin.Utils.wpf.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace  STC.Broker.Plugin.Utils.wpf
{
    public class DefaultDragHandler : IDragSource
    {
        public virtual void StartDrag(DragInfo dragInfo)
        {
            int num = dragInfo.SourceItems.Cast<object>().Count();
            if (num == 1)
            {
                dragInfo.Data = dragInfo.SourceItems.Cast<object>().First();
            }
            else if (num > 1)
            {
                dragInfo.Data = TypeUtilities.CreateDynamicallyTypedList(dragInfo.SourceItems);
            }

            dragInfo.Effects = ((dragInfo.Data != null) ? (DragDropEffects.Copy | DragDropEffects.Move) : DragDropEffects.None);
        }
    }
}
