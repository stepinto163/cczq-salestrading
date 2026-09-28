using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BDB.Trade.Plugin.View.Trade
{
    public class AnimationHelper
    {
        private Color _redBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#70FF3C3C");
        private Color _redSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#30FF3C3C");

        private Color _greenBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#744C21");
        private Color _greenSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#30744C21");

        private Color _upSideColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#311615");

        private AnimationHelper() { }
        private static AnimationHelper _animationHelper = new AnimationHelper();


        public static AnimationHelper Instance { get { return _animationHelper; } }

        private static List<Storyboard> storyboardList = new List<Storyboard>();

        public void StartAnimation(FrameworkElement ui, string name, string isUpSide, bool useAnimation)
        {
            Border border = ui.FindName(name) as Border;
            
            //if (isUpSide == "1")
            //    border.Background = new SolidColorBrush(_upSideColor);
            //else
            //    border.Background = new SolidColorBrush(Colors.Transparent);

            if (!useAnimation)
            {
                return;
            }

            Storyboard story = new Storyboard();
            story.AutoReverse = false;
            story.Completed += Story_Completed;

            storyboardList.Add(story);

            ColorAnimation ca = GetColorAnimation(isUpSide);
            story.Children.Add(ca);

            DependencyProperty[] propertyChain = new DependencyProperty[]
                {
                                    Border.BackgroundProperty,
                                    SolidColorBrush.ColorProperty
                };

            Storyboard.SetTargetName(ca, name);
            Storyboard.SetTargetProperty(ca, new PropertyPath("(0).(1)", propertyChain));
            //try
            //{
            //    story.Name = Guid.NewGuid().ToString();//msgPo.Id.ToString();
            //}
            //catch (Exception exx) { }
            //story.Children.Add(msgPo.InnerColorAnimation);

            //story.Begin();
            try
            {
                border.BeginStoryboard(story);
            }
            catch (InvalidOperationException ex)
            { }
            catch (Exception e)
            {
                LogHelper.Info(e);
            }
        }

        public void StopAndClear()
        {
            if (storyboardList != null && storyboardList.Count > 0)
            {
                foreach (Storyboard sb in storyboardList)
                {
                    sb.Stop();
                }

                storyboardList.Clear();
            }
        }

        private void Story_Completed(object sender, EventArgs e)
        {
            Storyboard story = sender as Storyboard;
            if (story != null && storyboardList.Contains(story))
            {
                storyboardList.Remove(story);
            }
        }

        private ColorAnimation GetColorAnimation(string isUpSide)
        {
            ColorAnimation ca = new ColorAnimation();
            ca.Duration = new Duration(TimeSpan.FromSeconds(2));
            ca.AutoReverse = false;
            ca.FillBehavior = FillBehavior.Stop;

            if (isUpSide == "1")
            {
                ca.From = _redBig;

                ca.To = _redSmall;
            }
            else
            {
                ca.From = _greenBig;

                ca.To = _greenSmall;
            }

            return ca;
        }
        
    }
}
