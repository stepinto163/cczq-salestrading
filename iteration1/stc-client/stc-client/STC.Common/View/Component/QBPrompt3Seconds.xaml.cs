using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace STC.Common.View.Component
{
    /// <summary>
    /// Prompt2Seconds.xaml 的交互逻辑
    /// </summary>
    public partial class QBPrompt3Seconds : Window
    {
        /// <summary>
        /// 提示标题
        /// </summary>
        public string PromptTitle
        {
            get { return (string)GetValue(PromptTitleProperty); }
            set { SetValue(PromptTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PromptTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PromptTitleProperty =
            DependencyProperty.Register("PromptTitle", typeof(string), typeof(QBPrompt3Seconds), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 提示内容
        /// </summary>
        public string PromptContext
        {
            get { return (string)GetValue(PromptContextProperty); }
            set { SetValue(PromptContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrompContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PromptContextProperty =
            DependencyProperty.Register("PromptContext", typeof(string), typeof(QBPrompt3Seconds), new PropertyMetadata(string.Empty));




        public QBPrompt3Seconds()
        {
            InitializeComponent();
        }

        private async void root_Initialized(object sender, EventArgs e)
        {
            await Task.Run(() => { Thread.Sleep(2000); });
            this.Close();
        }
    }
}
