using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class TextBlockModel
    {
        public FontFamily FontFamily { get; set; }

        public double FontSize { get; set; }

        public FontStyle FontStyle { get; set; }

        public FontWeight FontWeight { get; set; }

        public FontStretch FontStretch { get; set; }

        public string Text { get; set; }

    }
}
