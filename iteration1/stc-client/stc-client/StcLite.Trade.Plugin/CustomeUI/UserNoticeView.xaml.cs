using BDB.Trade.Plugin.ViewModel.Order;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BDB.Trade.Plugin.CustomeUI
{
    /// <summary>
    /// SendPriceWarn.xaml 的交互逻辑
    /// </summary>
    public partial class UserNoticeView : Window
    {

        // private RiskViewUIViewModel viewModel = null;
        private bool _isSuccess = false;

        public event Action<object, bool, bool> OnAction;
        private bool _needDoubleCheck = false;

        public void OnClose(object sender, bool isCancel, bool isDoubleCheck)
        {
            _needDoubleCheck = isDoubleCheck;
            OnAction?.Invoke(sender, isCancel, isDoubleCheck);
        }

        public Delegate[] GetSubscribe()
        {
            return OnAction != null ? OnAction.GetInvocationList() : null;
        }

        public void SetSubscribe(Delegate[] sub)
        {
            try
            {
                foreach (Delegate d in sub)
                    OnAction += (Action<object, bool, bool>)d;
            }
            catch (Exception e)
            {

            }
        }

        public UserNoticeView()
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            this.Closed += SendPriceWarnView_Closed;
            this.InitText();
        }
        private string chstitle = @"                                                                 用户使用须知";
        private string s = @"尊敬的用户：
    为保证BDB软件（以下简称“本软件”）的正常运行，同时向用户提供不间断的优质服务，本软件开发者宁波森浦信息技术有限公司（以下简称“我司”）特此发布本用户使用须知（以下简称“本须知”）。建议您在接受本软件服务之前，敬请仔细阅读本须知内容，特别是以粗体标识的条款，在确认充分理解并同意后使用相关本软件或相关服务。如果您不同意本须知任何内容，您应立即停止使用本软件。
    1.本软件、衍生软件及软件数据的一切知识产权，以及与软件相关的所有信息内容，均为我司或其数据提供者所有，用户不得为任何商业或非商业目的自行或许可任何第三方实施、利用、转让上述知识产权。
    2.全部著作权、商标以及并入或固定于软件和（或）衍生软件的相关专有权利声明、标识或注释未经我司事先书面批准不得更改、取消或删除，否则我司将依法追究其法律责任。
    3.在使用本软件期间，授权用户不得实施下列行为：
    (i)修改、改编或翻译本软件，以任何形式或媒介拷贝、复写、转让、分发、反向汇编、反向编译、反向工程或复制软件和（或）其衍生软件、出租、出借或转售软件的使用或分销许可；
    (ii)将本软件用户账号外泄给未经我司授权的其他用户或第三方（包括未经我司同意的集团关联公司）使用，或许可任何第三方直接或间接使用本软件，包括但不限于将软件用于盈利、营销、培训或咨询服务等形式；
    (iii)对本软件及其组件、模块、数据进行修改或干扰；
    (iv)出版，或者向公众或第三方提供任何软件的任何评估结果或基准程序；
    (v)通过修改或伪造软件运行中的指令、数据，增加、删减、变动软件的功能或运行效果，或者将用于上述用途的软件、方法进行运营或向公众传播，无论这些行为是否为商业目的；
    (vi)自行开发或与第三方合作开发与软件类似、相同或相竞争的产品；
    (vii)将软件整体或部分并入任何其它软件程序；
    我司不对用户由于上述行为或不适当的使用导致的后果承担责任。
    4.用户理解并同意，本软件仅为交易协助工具。除非用户有证据表明数据错误系由森浦自身过失造成的，否则我司不对交易所产生的盈亏和交易过程中发生的错误承担除更正以外的法律责任，尤其免予我司因提供市场数据给用户的利益及期待利益造成的任何损失。用户自行使用本软件提供的市场数据并对自己使用和决策行为承担所有法律和经济责任。
    5.我司不对因用户自身网络或设备环境造成的或其他任何非我司过错造成的软件无法正常使用或无法正常交易，行情或交易单无法正常接收的情况及其该等情况造成的损失损害承担任何责任，包括但不限于因地震、台风、洪水、火灾或其它天灾、战争等不可抗力事件以及政策变化、卫星传输线路故障、电脑病毒、授权用户错误操作、指定办公设备设置错误、网络接连错误、任何第三方原因（如货币经纪公司、电信部门、网络服务提供商或政府行为，或黑客攻击）等造成软件异常或报价信息传递异常。但我司可以在其合理能力范围内提供必要协助，以促使排除异常情况。
    6.本须知之订立、修改、更新及最终解释权均属我司所有。我司可根据国家法律法规变化及业务需要，不时修改、补充本须知，变更后的须知将通过[站内信]的方式通知您。如您不同意变更后的须知，您应当于变更后的须知确定的生效之日起停止使用本软件，变更后的须知对您不产生效力；如您在变更后的须知生效后仍继续使用本软件，则视为您同意已生效的变更后的须知。
    7.本须知之订立、生效、解释、修订、补充、终止、执行与争议解决均适用中华人民共和国大陆地区法律；如法律无相关规定的，参照商业惯例及/或行业惯例。
    8.您因使用本软件所产生及与本软件服务有关的争议，由我司与您协商解决。协商不成时，任何一方均可向我司所在地有管辖权的人民法院提起诉讼。
    9.本须知任一条款被视为废止、无效或不可执行，该条应视为可分的且并不影响本须知其余条款的有效性及可执行性。

                                                                                                                         宁波森浦信息技术有限公司";
        private string engs = @"                                                                    Instructions to BDB Users
Dear users,
You are welcome to use the “BDB” software (hereinafter referred to as the “Software“). In order to provide you with more comprehensive and quality service, the developer of the Software, Ningbo Sumscope Information Technology Co., Ltd. (hereinafter referred to as “Sumscope“), has instituted this User Instruction (hereinafter referred to as the “Instruction“). Before using the Software, please read carefully the content of the Instruction, especially the terms marked in bold. You shall confirm that you fully understand and agree to the terms before using the Software or related services. IF YOU DO NOT ACCEPT THESE TERMS AND CONDITIONS, PLEASE STOP USING THE SOFTWARE IMMEDIATELY.
1.All intellectual property rights in the Software, Derivative Software and Software data, and all information related to the Software are owned by Sumscope or the data providers, and users may not, for any commercial or non-commercial purposes, use or transfer, or permit any third party to use or transfer, such intellectual property.
2.All copyrights, trademarks and related proprietary rights statements, labels and notes that are incorporated or fixed in the Software and / or Derivative Software shall not be changed, canceled or deleted without the prior written approval by Sumscope.
3.During the cooperation, without the prior written consent of Sumscope, users may not:
(i)modify, adapt or translate the Software, copy, reproduce, transfer, distribute, reverse - compile, reverse - engineer or duplicate the Software and / or its Derivative Software in any form or media, lease, lend or resell the license to use or distribute the Software;
(ii) disclose the Software user account to any unauthorized user or third party(including your affiliates that are not authorized by Sumscope) or permit any third party to have access to or use the Software directly or indirectly, including but not limited to use the Software for profit, marketing, training or consulting services;
(iii) modify or tamper with the Software and the components, modules and data thereof;
(iv) publish, or furnish with the public or third parties, any assessment results or benchmarks of any Software；
(v) add, delete or change the functionalities or execution results of the Software by altering or falsifying instructions or data for running the Software, or operate the Software or methods for the above-mentioned purposes, or communicate the same to the public, , whether or not these practices are performed for commercial purposes;
(vi) develop products similar to, same as or competing with the Software , whether by yourself or in collaboration with other third parties;
(vii) incorporate the Software, in whole or in part, into any other software programs;
Sumscope will not be responsible for any consequences caused by the above unauthorized or improper use of you.
4.4.You understand and agree that BDB is a tool to help with the trading.Unless you have evidence that the error is caused by the negligence of Sumscope, Sumscope shall not be liable for user account's profit or loss, or any inaccuracy of errors in the trading process other than for rectification. You, in particular, exempt Sumscope from the compensation of any loss of profits or anticipated profits caused by the data to you. You shall be liable for all the actions and determinations by yourself regarding the data.
5.5.Sumscope shall assume no responsibility for any malfunction in the Software, not be able to trade or any delayed display, loss or failed receipt of market data and order information therein that is caused by your own networks or device environment or other reasons that are not Sumscope’s fault, or any loss resulting therefrom, including but not limited to Software anomalies or market data transmission anomalies due to force majeure such as earthquake, typhoon, flood, fire or other natural disaster, war, and policy change, satellite transmission failure, computer virus, misuse by users, designated office device setting error, network connection error, or any reasons attributable to third parties (e.g., acts of currency brokers, telecommunications providers, network services providers or the government, or hackers’ attack). Nonetheless, Sumscope may provide necessary assistance within its reasonable capacity to facilitate the removal of anomalies.
6.Sumscope reserves all the rights for making, modifying, updating and the final interpretation of this Instruction.For the purpose of complying with regulatory policies or improving the experience of this product, Sumscope can modify or amend the content of the Instruction at any time, and shall announce such modification or amendment at the official website.If you refuse to accept the modified or amended Instruction, you shall immediately cease using the Software and the services of Sumscope; if you continue to use the Software after the Instruction is modified or amended, it shall be deemed as that you accept the terms upon the modification or amendment.
7.The execution, effectiveness, performance, termination and dispute settlement of this Instruction shall be governed by the laws of the People’s Republic of China；if there are no relevant provisions in the law, refer to business practices and/or industry practices.
8.As to any dispute arising in the execution, effectiveness, performance and termination of the Instruction, Sumscope expects to settle through friendly consultation with you; in case that the consultation fails or the consultation result is not satisfactory to you, you and Sumscope shall, upon consent, file a lawsuit to the people’s court with jurisdiction in the place where Sumscope is located.
9.In the event that any term of the Instruction becomes completely or partially invalid or not enforceable due to any reason, or violates any applicable law, the said term shall be regarded as divisible; however, the other terms of the Instruction shall remain effective and binding.
10.If there are inconsistencies in meaning between the Chinese-language version and English-language version of these terms and conditions, the Chinese-language version shall prevail.
";
        private void InitText()
        {
            FlowDocument fd = new FlowDocument();
            Run rt = new Run();
            rt.Style = (Style)FindResource("GoldTextBlockStyle");
            rt.Text = chstitle;

            Run rc = new Run();
            rc.Style = (Style)FindResource("LightYellowTextBlockStyle");
            rc.Text = s;

            Run engrc = new Run();
            engrc.Style = (Style)FindResource("LightYellowTextBlockStyle");
            engrc.Text = engs;

            Paragraph fttile = new Paragraph(rt);
            fttile.LineHeight = 22;
            fttile.Padding = new Thickness(10, 10, 0, 0);

            Paragraph fg = new Paragraph(rc);
            fg.Padding = new Thickness(10, 0, 0, 0);
            fg.LineHeight = 22;

            Paragraph engfg = new Paragraph(engrc);
            engfg.Padding = new Thickness(10, 0, 0, 0);
            engfg.LineHeight = 22;

            richTextBox.Document = fd;
            fd.Blocks.Add(fttile);
            fd.Blocks.Add(fg);
            fd.Blocks.Add(engfg);
        }

        private void SendPriceWarnView_Closed(object sender, EventArgs e)
        {
            if (!_needDoubleCheck)
            {
                this.OnClose(this, true, false);

                if (this.Owner != null)
                    this.Owner.Focus();
            }

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o => {

                this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                    new Action(() => {
                        if (_isSuccess)
                        {
                            OnClose(this, false, false);
                        }
                    }));

                Thread.Sleep(2000);

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() => {
                        if (_isSuccess)
                        {
                            this.Close();
                            if (this.Owner != null)
                                this.Owner.Focus();
                        }
                    }));
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                //if (viewModel != null && viewModel.OkCommand.CanExecute(null))
                //    viewModel.OkCommand.Execute(null);
            }
        }
    }
}
