using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.Frame.UI.Utils
{
    public class MainWindowHandler
    {
        public static int PreferWidth { get; set; }
        public static int PreferHeight { get; set; }
        public static int FirstLoadIndex { get; set; }

        static MainWindowHandler()
        {
            PreferWidth = 1366;
            PreferHeight = 768;
            FirstLoadIndex = 0;
            ViewScale = 1.5;
        }

        public static QbHandler QbHandler { get; set; }

        public static Window1 MainWindow { get; set; }
        public static IPageHandler PageHandler { get; set; }
        public static TabControl MainTabControl { get; set; }
        public static IWindowSize WindowSize { get; set; }
        public static ILastDealt LastDealt { get; set; }

        public static double ViewScale { get; set; }

        private static bool passPreviewKey = false;
        public static bool PassPreviewKey
        {
            get { return passPreviewKey; }
            set
            {
                passPreviewKey = value;
                Console.WriteLine("set PassPreviewKey: {0}", value);
            }
        }


        public static double[] StartPoint(Window window)
        {
            WpfScreen wpfScreen = WpfScreen.GetScreenFrom(window);
            Rect rect = wpfScreen.WorkingArea;
            double x = rect.X + (rect.Width - window.Width) / 2;
            if (x < rect.X)
            {
                x = rect.X;
            }
            double y = rect.Y + (rect.Height - window.Height) / 2;
            if (y < rect.Y)
            {
                y = rect.Y;
            }
            return new double[] { x, y };
        }

    }

    public class QbHandler
    {
        public virtual void Initialize()
        {
        }
    }

    public interface ILastDealt
    {
        void SetVisible(bool b);
    }

    public interface IPageHandler
    {
        void PrevPage();
        void NextPage();
        void FirstPage();
        void LastPage();
    }

    public interface IWindowSize
    {
        double CalcMinWidth();
        double CalcMinHeight();
    }
}
