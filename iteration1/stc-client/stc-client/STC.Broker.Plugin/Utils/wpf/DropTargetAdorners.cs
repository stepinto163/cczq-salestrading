using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  STC.Broker.Plugin.Utils.wpf
{
    public class DropTargetAdorners
    {
        public static Type Highlight => typeof(DropTargetHighlightAdorner);

        public static Type Insert => typeof(DropTargetInsertionAdorner);
    }
}
