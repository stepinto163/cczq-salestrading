using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Timers;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace STC.WorkTable.Plugin.View.Component
{
    /// <summary>
    /// AutoCompleteComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class AutoCompleteComboBox : TextBox
    {
        #region Events and Delegates
        /// <summary>
        /// Used to pass arguments for autocomplete control
        /// </summary>
        public class AutoCompleteArgs : EventArgs
        {
            private string _pattern;
            /// <summary>
            /// The current pattern in the auto complete
            /// </summary>
            public string Pattern
            {
                get
                {
                    return this._pattern;
                }
            }

            private IEnumerable _dataSource;
            /// <summary>
            /// Used to the the new datasource for the autocomplete
            /// </summary>
            public IEnumerable DataSource
            {
                get
                {
                    return this._dataSource;
                }
                set
                {
                    this._dataSource = value;
                }
            }

            /// <summary>
            /// default false
            /// </summary>
            private bool _cancelBinding = false;
            /// <summary>
            /// Determines weather or not the datasource should be bounded to the autocomplete control
            /// </summary>
            public bool CancelBinding
            {
                get
                {
                    return this._cancelBinding;
                }
                set
                {
                    this._cancelBinding = value;
                }
            }

            public AutoCompleteArgs(string Pattern)
            {
                this._pattern = Pattern;
            }
        }

        /// <summary>
        /// event handler for auto complete pattern changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public delegate void AutoCompleteHandler(object sender, AutoCompleteArgs args);
        /// <summary>
        /// Event for when pattern has been changed
        /// </summary>
        public event AutoCompleteHandler PatternChanged;
        #endregion

        #region Fields
        private Timer _interval;
        private int _maxRecords = 10;
        private bool _typeAhead = false;
        private bool IsKeyEvent = false;
        private bool _clearOnEmpty = false;
        private int _delay = DEFAULT_DELAY;
        private const int DEFAULT_DELAY = 200; // 1 seconds delay
        #endregion

        #region Properties
        /// <summary>
        /// The timespan between keypress and pattern changed event
        /// </summary>
        [DefaultValue(DEFAULT_DELAY)]
        public int Delay
        {
            get
            {
                return this._delay;
            }
            set
            {
                this._delay = value;
            }
        }
        /// <summary>
        /// The maximum number of records to show in the drop down
        /// </summary>
        public int MaxRecords
        {
            get
            {
                return this._maxRecords;
            }
            set
            {
                this._maxRecords = value;
            }
        }

        /// <summary>
        /// Determines weather textbox does type ahead
        /// </summary>
        public bool TypeAhead
        {
            get
            {
                return this._typeAhead;
            }
            set
            {
                this._typeAhead = value;
            }
        }

        /// <summary>
        /// Specify weather or not to clear the datasource when the text is set to empty string
        /// </summary>
        /// <remarks>When this property is set to true, the patternchanged event will not be fired when the text is empty.</remarks>
        public bool ClearOnEmpty
        {
            get
            {
                return this._clearOnEmpty;
            }
            set
            {
                this._clearOnEmpty = true;
            }
        }


        /// <summary>
        /// Gets the text box in charge of the editable portion of the combo box.
        /// </summary>
        protected DataGrid dgBondCode
        {
            get
            {
                return base.GetTemplateChild("dgBondCode") as DataGrid;
            }
        }

        protected Popup popBondCode
        {
            get
            {
                return base.GetTemplateChild("popBondCode") as Popup;
            }
        }

        private ObservableCollection<DataGridColumn> _autoCompColumns = new ObservableCollection<DataGridColumn>();
        public ObservableCollection<DataGridColumn> AutoCompColumns
        {
            get
            {
                return _autoCompColumns;
            }
        }

        public event Action<object> AfterSearch;//搜索后的结果
        public event Action<string> InputText;//输入值

        private bool TextChangeFlag = true;//文本改变标志
        private bool IsInit = true;//是否初始化


        //鼠标划过边框的颜色（默认值）
        static System.Windows.Media.Brush defaultMouseOverBorderBrushCob = new SolidColorBrush(Color.FromRgb(255, 63, 68));
        //文本框获取焦点边框的颜色（默认值）
        static System.Windows.Media.Brush defaultFocusBorderBrushCob = new SolidColorBrush(Color.FromRgb(230, 48, 52));
        //文本框的背景颜色（默认值）
        static System.Windows.Media.Brush defaultBackGroundCob = new SolidColorBrush(Color.FromRgb(15, 15, 15));
        /// <summary>
        /// 鼠标划过边框的颜色
        /// </summary>
        public System.Windows.Media.Brush MouseOverBorderBrushCob
        {
            get { return (System.Windows.Media.Brush)GetValue(MouseOverBorderBrushProperty); }
            set { SetValue(MouseOverBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseOverBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverBorderBrushProperty =
            DependencyProperty.Register("MouseOverBorderBrush", typeof(System.Windows.Media.Brush), typeof(AutoCompleteComboBox), new FrameworkPropertyMetadata(defaultMouseOverBorderBrushCob));
        /// <summary>
        /// 文本框获取焦点边框的颜色
        /// </summary>
        public System.Windows.Media.Brush FocusBorderBrushCob
        {
            get { return (System.Windows.Media.Brush)GetValue(FocusBorderBrushProperty); }
            set { SetValue(FocusBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusBorderBrushProperty =
            DependencyProperty.Register("FocusBorderBrush", typeof(System.Windows.Media.Brush), typeof(AutoCompleteComboBox), new FrameworkPropertyMetadata(defaultFocusBorderBrushCob));

        /// <summary>
        /// 文本框的背景颜色
        /// </summary>
        public System.Windows.Media.Brush BackGroundCob
        {
            get { return (System.Windows.Media.Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusBorderBrush.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(System.Windows.Media.Brush), typeof(AutoCompleteComboBox), new FrameworkPropertyMetadata(defaultBackGroundCob));

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteComboBox), new PropertyMetadata(null));




        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Watermark.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(AutoCompleteComboBox), new PropertyMetadata("搜索..."));




        public string DisplayValuePath
        {
            get { return (string)GetValue(DisplayValuePathProperty); }
            set { SetValue(DisplayValuePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayValuePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayValuePathProperty =
            DependencyProperty.Register("DisplayValuePath", typeof(string), typeof(AutoCompleteComboBox), new PropertyMetadata(null));



        #endregion

        public AutoCompleteComboBox()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this._interval = new Timer(this.Delay);
                this._interval.AutoReset = true;
                this._interval.Elapsed += new ElapsedEventHandler(_interval_Elapsed);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            foreach(DataGridColumn c in this.AutoCompColumns)
            {
                this.dgBondCode.Columns.Add(c);
            }
        }

        #region Event Handlers
        void _interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsKeyEvent = false;
            // pause the timer
            this._interval.Stop();

            // show the drop down when user starts typing then stops
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                this.popBondCode.IsOpen = true;

                // load from event source
                if (this.PatternChanged != null)
                {
                    AutoCompleteArgs args = new AutoCompleteArgs(this.Text);

                    BindData(this, args);

                    //// bind the new datasource if user did not cancel
                    //if (!args.CancelBinding)
                    //{
                    //    this.dgBondCode.ItemsSource = args.DataSource;

                    //    if (!this.TypeAhead)
                    //    {

                    //        // override auto complete behavior
                    //        this.Text = args.Pattern;
                    //        this.CaretIndex = args.Pattern.Length;

                    //    }
                    //}
                }
            },
            System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private async void BindData(AutoCompleteComboBox cbb, AutoCompleteArgs args)
        {
            await Task.Run(() => {

                if (cbb.PatternChanged != null)
                {
                    cbb.PatternChanged(cbb, args);
                }

                cbb.Dispatcher.BeginInvoke((Action)delegate
                {
                    // bind the new datasource if user did not cancel
                    if (!args.CancelBinding)
                    {
                        cbb.dgBondCode.ItemsSource = args.DataSource;

                        if (!cbb.TypeAhead)
                        {

                            // override auto complete behavior
                            cbb.Text = args.Pattern;
                            cbb.CaretIndex = args.Pattern.Length;
                        }
                    }
                });
            });
        }

        private void tbTraderAInfoChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            this.IsKeyEvent = true;
        }

        private void tbTraderAInfoChange_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            this.IsKeyEvent = false;
        }

        protected void ResetTimer()
        {
            this._interval.Stop();
            this._interval.Start();
        }
        #endregion

        /// <summary>
        /// keydow 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (popBondCode.IsOpen)
                {
                    this.IsKeyEvent = false;
                    dgBondCode_MouseUp(null, null);
                    e.Handled = true;
                }
            }
        }
        /// <summary>
        /// PreviewKeyDown 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.IsKeyEvent = true;
            if (e.Key == Key.Up)//上
            {
                this.IsKeyEvent = false;
                this._interval.Stop();
                if (dgBondCode.SelectedIndex > 0)
                    dgBondCode.SelectedIndex--;
                if (dgBondCode.SelectedItem != null)
                    dgBondCode.ScrollIntoView(dgBondCode.SelectedItem);
            }
            else if (e.Key == Key.Down)//下
            {
                this.IsKeyEvent = false;
                this._interval.Stop();
                if (dgBondCode.SelectedIndex < dgBondCode.Items.Count - 1)
                    dgBondCode.SelectedIndex++;
                if (dgBondCode.SelectedItem != null)
                    dgBondCode.ScrollIntoView(dgBondCode.SelectedItem);
            }
            else if (e.Key == Key.Escape)//空格
            {
                this.IsKeyEvent = false;
                this._interval.Stop();
                this.SelectedItem = null;
                this.Text = string.Empty;
                e.Handled = true;
            }
        }
        /// <summary>
        /// GotFocus 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            this.SelectAll();
            IsInit = false;
        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.SelectedItem != null)
            {
                string dispalyName = GetModelValue(this.DisplayValuePath, this.SelectedItem);
                if (this.Text != dispalyName)
                    this.SelectedItem = null;
            }

            this.IsKeyEvent = false;
            //this.popBondCode.IsOpen = false;
            this._interval.Close();
        }

        /// <summary>
        /// 下拉框mouseup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgBondCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this._interval.Stop();
            object si = dgBondCode.SelectedItem;
            dgBondCode.SelectedIndex = -1;

            if (this.SelectedItem != si
                || (si != null && this.Text != GetModelValue(DisplayValuePath, si)))
            {
                this.SelectedItem = si;
                if (!string.IsNullOrEmpty(DisplayValuePath))
                {
                    this.Text = GetModelValue(DisplayValuePath, si);
                }
            }

            popBondCode.IsOpen = false;

            AfterSearch?.Invoke(si);

            this.Focus();
        }

        /// <summary>
        /// 获取类中的属性值
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetModelValue(string FieldName, object obj)
        {
            try
            {
                if (obj == null)
                    return string.Empty;
                Type Ts = obj.GetType();
                object o = Ts.GetProperty(FieldName).GetValue(obj, null);
                string Value = Convert.ToString(o);
                if (string.IsNullOrEmpty(Value)) return string.Empty;
                return Value;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 文本框文字改变时候
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // clear the itemsource when text is empty
            if (this.ClearOnEmpty && string.IsNullOrEmpty(this.Text.Trim()))
                this.dgBondCode.ItemsSource = null; // this should also clear selection
            else if (IsKeyEvent)
                this.ResetTimer();
        }

        /// <summary>
        /// 点击关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            IsInit = true;
            this.SelectedItem = null;
            this.Text = string.Empty;

            IsInit = false;
        }

        private void toggleBtn_Click(object sender, RoutedEventArgs e)
        {
            ExpandOptions();
        }

        internal void ExpandOptions()
        {
            if (popBondCode.IsOpen == false)
                this.ResetTimer();
        }
    }
}
