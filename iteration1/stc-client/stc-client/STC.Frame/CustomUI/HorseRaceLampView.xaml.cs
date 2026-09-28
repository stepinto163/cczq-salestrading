using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Frame.CustomUI
{
    /// <summary>
    /// HorseRaceLampView.xaml 的交互逻辑
    /// </summary>
    public partial class HorseRaceLampView : UserControl
    {
        double n_height;   //滚动高度

        Canvas canvas_board = new Canvas();
        TextBlock image1 = new TextBlock();
        Storyboard storyboard_imgs = new Storyboard();

        public HorseRaceLampView()
        {
            InitializeComponent();

            this.GridLayout();
        }

        public void ChangeTextPadding(Thickness thickness)
        {
            this.image1.Padding = thickness;
        }

        void GridLayout()
        {
            canvas_board.VerticalAlignment = VerticalAlignment.Stretch;
            canvas_board.HorizontalAlignment = HorizontalAlignment.Stretch;
            canvas_board.Margin = new Thickness(10, 0, 10, 0);
            canvas_board.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#833B3B"));//Colors.GreenYellow);
            canvas_board.ClipToBounds = true;
            this.grid_main.Children.Add(canvas_board);

            image1.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
            //image1.Padding = new Thickness(0, 7, 0, 0);
            //image1.Width = 200;
            //image1.Height = 50;
            this.canvas_board.Children.Add(image1);
            image1.SetValue(Canvas.TopProperty, 0.0);
            image1.SetValue(Canvas.LeftProperty, 0.0);

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            n_height = this.canvas_board.ActualHeight;
            this.image1.Text = "测试环境Testing Environment";

            this.StoryLoad();
        }

        

        void StoryLoad()
        {
            DoubleAnimationUsingKeyFrames daukf_img1 = new DoubleAnimationUsingKeyFrames();
            LinearDoubleKeyFrame k1_img1 = new LinearDoubleKeyFrame(this.canvas_board.ActualWidth, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(12)));
            daukf_img1.KeyFrames.Add(k1_img1);
            storyboard_imgs.Children.Add(daukf_img1);
            Storyboard.SetTarget(daukf_img1, image1);
            Storyboard.SetTargetProperty(daukf_img1, new PropertyPath("(Canvas.Left)"));

            storyboard_imgs.RepeatBehavior = RepeatBehavior.Forever;
            storyboard_imgs.Begin();
        }
    }
}
