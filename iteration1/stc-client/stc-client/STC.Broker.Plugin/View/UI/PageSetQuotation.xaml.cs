using com.gateway.log;
using STC.Broker.Plugin.ViewModel.UI;
using STC.Common.Data;
using STC.Common.Model;
using STC.Common.Utils;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace STC.Broker.Plugin.View.UI
{
    /// <summary>
    /// PageSetQuotation.xaml 的交互逻辑
    /// </summary>
    public partial class PageSetQuotation : UserControl, ISetting
    {
        private readonly string TransactionSetting = "TransactionSetting";
        private readonly string TransactionModel = "TransactionModel";
        /// <summary>
        ///交易处理模式 1.卡片，2列表，3自适应
        /// </summary>
        string _transactionModel = "3";
        public bool isLayoutChanged;
        public string GetName()
        {
            return "布局设置";
        }
        public void OnSave()
        {
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public Action<bool> callBack { get; set; }
        public PageSetQuotation()
        {
            InitializeComponent();

            Loaded += SettingUI_Loaded;
            layoutSetting.LayoutChanged += LayoutSetting_LayoutChanged;
        }

        private void SettingUI_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        /// <summary>
        /// 初始化设置界面
        /// </summary>
        private void Init()
        {
            if (Common.Utils.LocalEnv.GrLayoutSetting.ShowModel == 1)
            {
                modeList.IsChecked = true;
                modeCard.IsChecked = false;

                tbLayoutInfo.Text = string.Empty;
            }
            else
            {
                layoutSetting.SetRowColumn(Common.Utils.LocalEnv.GrLayoutSetting.Row, Common.Utils.LocalEnv.GrLayoutSetting.Col);

                tbLayoutInfo.Text = string.Format("{0}X{1}", Common.Utils.LocalEnv.GrLayoutSetting.Row, Common.Utils.LocalEnv.GrLayoutSetting.Col);
                modeList.IsChecked = false;
                modeCard.IsChecked = true;
            }

            if (Common.Utils.LocalEnv.GrLayoutSetting.QuotesShowMode == 1)
            {
                fullQuotesMode.IsChecked = true;
                simpleQuotesMode.IsChecked = false;
            }
            else
            {
                fullQuotesMode.IsChecked = false;
                simpleQuotesMode.IsChecked = true;
            }

            string transactionModel = IniFileHelper.ReadValue(string.Format("{0}_{1}", TransactionSetting, LocalEnv.UserInfo.UserId), TransactionModel);

            if (!string.IsNullOrWhiteSpace(transactionModel))
            {
                if (transactionModel == "1")
                {
                    TransactionCard.IsChecked = true;
                    _transactionModel = transactionModel;
                }
                else if (transactionModel == "2")
                {
                    TransactionList.IsChecked = true;
                    _transactionModel = transactionModel;
                }
                else
                {
                    TransactionAutomatic.IsChecked = true;
                }
            }
            else
            {
                TransactionAutomatic.IsChecked = true;
            }

            this.SaveIsEnable.IsEnabled = true;
        }

        public Common.Model.PT.GrLayoutSettingModel GetLayoutSetting()
        {
            Common.Model.PT.GrLayoutSettingModel layoutSettingPo = new Common.Model.PT.GrLayoutSettingModel();
            layoutSettingPo.Row = layoutSetting.Rows;
            layoutSettingPo.Col = layoutSetting.Columns;
            layoutSettingPo.ShowModel = modeList.IsChecked.Value ? 1 : modeCard.IsChecked.Value ? 2 : 1;
            layoutSettingPo.QuotesShowMode = fullQuotesMode.IsChecked.Value ? 1 : simpleQuotesMode.IsChecked.Value ? 2 : 1;
            return layoutSettingPo;
        }

        private void LayoutSetting_LayoutChanged(object sender, object e)
        {
            modeList.IsChecked = false;
            modeCard.IsChecked = true;
            isLayoutChanged = true;
            tbLayoutInfo.Text = string.Format("{0}X{1}", layoutSetting.Rows, layoutSetting.Columns);

        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.SaveIsEnable.IsEnabled = true;
        }

        public void btConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.SaveIsEnable.IsEnabled = false;

            Common.Model.PT.GrLayoutSettingModel currentSetting = this.GetLayoutSetting();
            //保存配置
            IniFileHelper.WriteValue($"GrLayoutSetting_{LocalEnv.UserInfo.UserName}", "QuotesShowMode", currentSetting.QuotesShowMode.ToString());
            IniFileHelper.WriteValue($"GrLayoutSetting_{LocalEnv.UserInfo.UserName}", "ShowModel", currentSetting.ShowModel.ToString());
            IniFileHelper.WriteValue($"GrLayoutSetting_{LocalEnv.UserInfo.UserName}", "Row", currentSetting.Row.ToString());
            IniFileHelper.WriteValue($"GrLayoutSetting_{LocalEnv.UserInfo.UserName}", "Col", currentSetting.Col.ToString());


            Common.Utils.LocalEnv.GrLayoutSetting = currentSetting;
            //CommonHandler.Instance.ResetNum();
            LogHelper.Warn("Reset SettingChange");
            DesignHandler.Instance.SettingChange(currentSetting);
            this.SaveIsEnable.IsEnabled = true;


            if (TransactionCard.IsChecked.Value)
            {
                TransactionCard.IsChecked = true;
                _transactionModel = "1";
            }
            else if (TransactionList.IsChecked.Value)
            {
                _transactionModel = "2";
            }
            else if (TransactionAutomatic.IsChecked.Value)
            {
                _transactionModel = "3";
            }

           
            IniFileHelper.WriteValue($"{TransactionSetting}_{LocalEnv.UserInfo.UserId}", TransactionModel, _transactionModel);

            DesignHandler.Instance.TransactionModelChange(_transactionModel);
            //CommonUtils.Prompt2Second("消息", "保存成功，正在应用，请稍后...", Application.Current.MainWindow);

        }

        /// <summary>
        /// 自动模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeListChecked(object sender, RoutedEventArgs e)
        {
            tbLayoutInfo.Text = "";

            cbSmall.IsEnabled = cbMid.IsEnabled = cbBig.IsEnabled = layoutSetting.IsEnabled = false;
        }

        /// <summary>
        /// 卡片模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeCardChecked(object sender, RoutedEventArgs e)
        {
            cbSmall.IsEnabled = cbMid.IsEnabled = cbBig.IsEnabled = layoutSetting.IsEnabled = true;

            tbLayoutInfo.Text = string.Format("{0}X{1}", new Common.Model.PT.GrLayoutSettingModel().Row, new Common.Model.PT.GrLayoutSettingModel().Col);
            layoutSetting.SetRowColumn(new Common.Model.PT.GrLayoutSettingModel().Row, new Common.Model.PT.GrLayoutSettingModel().Col);
        }

        private void smallBtn_Click(object sender, RoutedEventArgs e)
        {
            //LayoutSetttingCache.Instance.layoutSettingPo.Row = 2;
            //LayoutSetttingCache.Instance.layoutSettingPo.Col = 6;
            tbLayoutInfo.Text = string.Format("{0}X{1}", 2, 6);
            layoutSetting.SetRowColumn(2, 6);
        }

        private void bigBtn_Click(object sender, RoutedEventArgs e)
        {
            //LayoutSetttingCache.Instance.layoutSettingPo.Row = 3;
            //LayoutSetttingCache.Instance.layoutSettingPo.Col = 7;
            tbLayoutInfo.Text = string.Format("{0}X{1}", 3, 8);
            layoutSetting.SetRowColumn(new Common.Model.PT.GrLayoutSettingModel().Row, new Common.Model.PT.GrLayoutSettingModel().Col);

            layoutSetting.SetRowColumn(3, 8);
        }

        private void vBtn_Click(object sender, RoutedEventArgs e)
        {
            //LayoutSetttingCache.Instance.layoutSettingPo.Row = 5;
            //LayoutSetttingCache.Instance.layoutSettingPo.Col = 4;
            tbLayoutInfo.Text = string.Format("{0}X{1}", 5, 4);
            layoutSetting.SetRowColumn(new Common.Model.PT.GrLayoutSettingModel().Row, new Common.Model.PT.GrLayoutSettingModel().Col);


            layoutSetting.SetRowColumn(5, 4);
        }

        private void small_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                cbMid.IsChecked = cbBig.IsChecked = false;

                tbLayoutInfo.Text = string.Format("{0}X{1}", 2, 6);
                layoutSetting.SetRowColumn(2, 6);
            }
        }

        private void mid_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                cbSmall.IsChecked = cbBig.IsChecked = false;

                tbLayoutInfo.Text = string.Format("{0}X{1}", 3, 8);
                layoutSetting.SetRowColumn(new Common.Model.PT.GrLayoutSettingModel().Row, new Common.Model.PT.GrLayoutSettingModel().Col);

                layoutSetting.SetRowColumn(3, 8);
            }
        }

        private void big_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                cbMid.IsChecked = cbSmall.IsChecked = false;

                tbLayoutInfo.Text = string.Format("{0}X{1}", 5, 4);
                layoutSetting.SetRowColumn(new Common.Model.PT.GrLayoutSettingModel().Row, new Common.Model.PT.GrLayoutSettingModel().Col);


                layoutSetting.SetRowColumn(5, 4);
            }
        }
    }
}
