using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using com.gateway.connector;
using com.gateway.connector.tcp;
using com.gateway.log;
using common.netapi.Api;
using netapi.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using Quartz.Impl;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Frame.UI;
using STC.Frame.Update;
using STC.Frame.Utils;
using STC.Speech.aliyun;
using STC.Speech.job;
using STC.Speech.speechresulf;
using STC.Speech.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Forms;
using EncrypterHelper = netapi.Api.EncrypterHelper;

namespace STC.Speech
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<string, bool> symbolDic = new Dictionary<string, bool>();
        public utils.AsyncThreadQueue<string> async;
        private Icon notifyImage;
        List<string> rics = new List<string>();
        ClsMCI cls; //音频播放
        ResulfApi resulf;
        private bool flag;
        //程序最小化
        WindowState wsl;
        NotifyIcon notifyIcon;
        //程序最小化
        public MainWindow()
        {

            try
            {
                InitializeComponent();
                //窗口最小化
                this.winMin();
                //读取配置文件
                this.getConfig();
                //创建文件夹
                this.createFile();
                //定时任务调度，每天几点定时获取token
                callScheduleManage();
                resulf = new ResulfApi(SpeechConsts.appkey, SpeechConsts.token);
                //添加订阅的topic
                rics.Add(SpeechConsts.Topic);
                async = new utils.AsyncThreadQueue<string>(asyncCallback);
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
                return;
            }
        }



        private void createFile()
        {
            //判断有无存放文件夹，没有创建
            if (!Directory.Exists(SpeechConsts.Data_File))
            {
                Directory.CreateDirectory(SpeechConsts.Data_File);
            }
        }

        /// <summary>
        /// 窗口最小化
        /// </summary>
        private void winMin()
        {
            //最小化
            icon();
            //保证窗体显示在上方
            wsl = WindowState;
        }


        /// <summary>
        /// 获取配置文件中的信息
        /// </summary>
        public void getConfig()
        {
            SpeechConsts.logLevel = ConfigurationManager.AppSettings["logLevel"].ToString();
            SpeechConsts.logAddress = ConfigurationManager.AppSettings["logAddress"].ToString();
            LogHandler.Info("logLevel:" + SpeechConsts.logLevel);
            LogHandler.Info("logAddress:" + SpeechConsts.logAddress);
            SpeechConsts.username = ConfigurationManager.AppSettings["username"].ToString();
            LogHandler.Info("username:" + SpeechConsts.username);
            SpeechConsts.password = ConfigurationManager.AppSettings["password"].ToString();
            LogHandler.Info("password:" + SpeechConsts.password);
            SpeechConsts.host = ConfigurationManager.AppSettings["host"].ToString();
            LogHandler.Info("host:" + SpeechConsts.host);
            SpeechConsts.port = ConfigurationManager.AppSettings["port"].ToString();
            LogHandler.Info("port:" + SpeechConsts.port);
            SpeechConsts.voiceMale = ConfigurationManager.AppSettings["voiceMale"].ToString();
            LogHandler.Info("voiceMale:" + SpeechConsts.voiceMale);
            SpeechConsts.speedMale = ConfigurationManager.AppSettings["speedMale"].ToString();
            LogHandler.Info("speedMale:" + SpeechConsts.speedMale);
            SpeechConsts.voiceFemale = ConfigurationManager.AppSettings["voiceFemale"].ToString();
            LogHandler.Info("voiceFemale:" + SpeechConsts.voiceFemale);
            SpeechConsts.speedFemale = ConfigurationManager.AppSettings["speedFemale"].ToString();
            LogHandler.Info("speedFemale:" + SpeechConsts.speedFemale);
            SpeechConsts.speaker = ConfigurationManager.AppSettings["speaker"].ToString();
            LogHandler.Info("speaker:" + SpeechConsts.speaker);
            SpeechConsts.schJob = ConfigurationManager.AppSettings["schJob"].ToString();
            LogHandler.Info("schJob:" + SpeechConsts.schJob);
            SpeechConsts.appkey = ConfigurationManager.AppSettings["appkey"].ToString();
            LogHandler.Info("appkey:" + SpeechConsts.appkey);
            SpeechConsts.accessKeyID = ConfigurationManager.AppSettings["accessKeyID"].ToString();
            LogHandler.Info("accessKeyID:" + SpeechConsts.accessKeyID);
            SpeechConsts.accessKeySecret = ConfigurationManager.AppSettings["accessKeySecret"].ToString();
            LogHandler.Info("accessKeySecret:" + SpeechConsts.accessKeySecret);
            SpeechConsts.gzip = ConfigurationManager.AppSettings["isGzip"].ToString();
            LogHandler.Info("gzip:" + SpeechConsts.gzip);
            SpeechConsts.gwOrCeda = ConfigurationManager.AppSettings["gwOrCeda"].ToString();
            LogHandler.Info("gwOrCeda:" + SpeechConsts.gwOrCeda);
            //获取最新token
            SpeechConsts.token = RefulshToken.getToken(SpeechConsts.accessKeyID, SpeechConsts.accessKeySecret);
        }

        private void icon()
        {
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "成交播报"; //设置程序启动时显示的文本
            this.notifyIcon.Text = "成交播报";//最小化到托盘时，鼠标点击时显示的文本
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri(SpeechConsts.IcoUrl)).Stream;
            this.notifyIcon.Icon = notifyImage = new System.Drawing.Icon(iconStream); //展示图标
            this.notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
            this.notifyIcon.ShowBalloonTip(1000);
        }


        //双击显示
        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            this.Show();
            WindowState = wsl;
        }

        /// <summary>
        /// 登陆
        /// </summary>
        private void LoginBefor()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Login();
            });
        }


        private void Login()
        {
            try
            {
                ApiManager.Instance.GetApi(SpeechConsts.gwOrCeda).NewApi();
                ApiManager.Instance.SetDefaultApi(SpeechConsts.gwOrCeda);
                if ("1".Equals(SpeechConsts.gzip))
                {
                    ApiManager.Instance.GetDefaultApi().SetGzip(true);
                }
                else
                {
                    ApiManager.Instance.GetDefaultApi().SetGzip(false);
                }
                ApiManager.Instance.GetDefaultApi().SubScribeConStatus(OnGateConnectStatus);


                LoginParam loginParam = new LoginParam();
                loginParam.UserName = SpeechConsts.username;
                loginParam.Pwd = SpeechConsts.password;
                loginParam.isEncrypt = false;
                loginParam.Host = SpeechConsts.host;
                loginParam.Port = int.Parse(SpeechConsts.port);
                loginParam.Protocal = "TCP";
                object loginObj;
                //if (SpeechConsts.gwOrCeda.Equals("2"))
                //{
                //    LoginMsgListener loginMsgListener = new LoginMsgListener();
                //    loginMsgListener.LoginHandler = LoginHandler.Instance;
                //    loginParam.OtherParam = loginMsgListener;
                //    loginObj = ApiManager.Instance.GetApi(SpeechConsts.gwOrCeda).Login(loginParam, LoginHandler.Instance);
                //}
                //else
                //{
                    GWLoginHandler gWLoginHandler = new GWLoginHandler();
                    gWLoginHandler.LoginParam = loginParam;
                    loginObj = ApiManager.Instance.GetApi(SpeechConsts.gwOrCeda).Login(loginParam, gWLoginHandler);
                //}

                if (loginObj == null)
                {
                    LogHandler.Error("connect message :host:" + loginParam.Host + ",port:" + loginParam.Port + "username:" + loginParam.UserName + ",password" + loginParam.Pwd + "isGZip:" + loginParam.isGzip);
                    LogHandler.Info("连接失败");
                    this.Dispatcher.Invoke(() =>
                    {
                        this.states.Text = "连接失败";
                    });
                    return;
                }
                NetapiLoginResult loginResult = JsonConvert.DeserializeObject<NetapiLoginResult>(loginObj.ToString());
                if (string.IsNullOrEmpty(loginResult.Code) || "0001".Equals(loginResult.Code + "", StringComparison.OrdinalIgnoreCase))
                {
                    LogHandler.Info("start subScribe");
                    subScribe();
                }
            }
            catch (Exception e)
            {

                LogHandler.Error(e.Message);
            }

        }

        /// <summary>
        /// 订阅数据
        /// </summary>
        public void subScribe()
        {
            try
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(call, rics.ToArray(), SpeechConsts.Server, "");
                Console.WriteLine("订阅成功");
                symbolDic.Add(SpeechConsts.Topic, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void unSubScribe()
        {
            ApiManager.Instance.GetDefaultApi().UnSubScribe("EMSSvr", rics.ToArray());
            symbolDic.Remove(SpeechConsts.Topic);
        }

        private void call(object obj)
        {
            try
            {
                ExecutionReport report = BaseHelper.ConvertStcMessage<ExecutionReport>(symbolDic.Keys.ToArray(), obj);
                
                if (report.VenueTypeGW.Equals("INNER") && !report.TimeInForce.Equals("3"))
                {
                    LogHandler.Info("=============="+report.OrderID + ": VenueTypeGW 为 INNER，TimeInForce 为 3,不进行播报");
                    return;
                }

                string audioSaveFile = SpeechConsts.Data_File + report.OrderID + ".wav";  //保存路径
                string format = "wav"; //音频格式
                string voice = ""; //声音
                int speech_rate = 0;  //语速
                //播放类型
                if (SpeechConsts.speaker.Equals("1"))
                {
                    voice = SpeechConsts.voiceMale;
                    speech_rate = int.Parse(SpeechConsts.speedMale);
                }
                else if (SpeechConsts.speaker.Equals("2"))
                {
                    voice = SpeechConsts.voiceFemale;
                    speech_rate = int.Parse(SpeechConsts.speedFemale);
                }
                else if (SpeechConsts.speaker.Equals("3"))
                {
                    if (report.Side.Equals("1"))
                    {
                        voice = SpeechConsts.voiceMale;
                        speech_rate = int.Parse(SpeechConsts.speedMale);
                    }
                    else
                    {
                        voice = SpeechConsts.voiceFemale;
                        speech_rate = int.Parse(SpeechConsts.speedFemale);
                    }
                }
                else if (SpeechConsts.speaker.Equals("4"))
                {
                    if (report.Side.Equals("1"))
                    {
                        voice = SpeechConsts.voiceFemale;
                        speech_rate = int.Parse(SpeechConsts.speedFemale);
                    }
                    else
                    {
                        voice = SpeechConsts.voiceMale;
                        speech_rate = int.Parse(SpeechConsts.speedMale);
                    }
                }

                string text;
                if (report.SecurityType.Equals(SpeechConsts.SecurityType))
                {
                    //BNC的类型  代码 方向  价格取小数点后位数，位数不足两位补0
                    text = SpeechConvert.formatStr(report.SecurityID, report.Side, report.Yield);
                }
                else
                {   //   价格取全部，期限取info2字段
                    text = SpeechConvert.formatStr(report.Symbol, report.Side, report.Yield, report.Info2);
                }
                LogHandler.Info(report.OrderID + ":" + text);
                //调用阿里接口转换语音
                resulf.processPOSTRequest(text, audioSaveFile, format, voice, speech_rate);

                //队列中存在八条的时候清空，从最新一条开始
                if (async.count() == 8)
                {
                    async.Clear();
                }
                async.Enqueue(audioSaveFile);
                Console.WriteLine(report);

            }
            catch (Exception e)
            {
                LogHandler.Error(e.Message);
            }

        }

        /// <summary>
        /// 声音播放
        /// </summary>
        /// <param name="file"></param>
        public void asyncCallback(string file)
        {
            cls = new ClsMCI();
            cls.FileName = file;
            cls.play();
        }

        /// <summary>
        /// 连接状态
        /// </summary>
        /// <param name="code"></param>
        private void OnGateConnectStatus(object code)
        {
            if (code != null)
            {
                int status = (int)code;
                switch (status)
                {
                    case IEventListener.CONNECTION_CLOSED:
                        Console.WriteLine("CONNECTION_CLOSED");
                        LogHandler.Info("GateWay CONNECTION_CLOSED");
                        this.Dispatcher.Invoke(() =>
                        {
                            this.states.Text = "连接断开";
                        });
                        break;
                    case IEventListener.CONNECTION_LOGIN_SUCCESS:
                        Console.WriteLine("CONNECTION_LOGIN_SUCCESS");
                        LogHandler.Info("GateWay CONNECTION_LOGIN_SUCCESS");
                        this.Dispatcher.Invoke(() =>
                        {
                            this.states.Text = "登录正常";
                        });
                        break;

                    case IEventListener.CONNECTION_SUCCESS:
                        Console.WriteLine("CONNECTION_SUCCESS");
                        LogHandler.Info("GateWay CONNECTION_SUCCESS");
                        this.Dispatcher.Invoke(() =>
                        {
                            this.states.Text = "连接正常";
                        });
                        break;

                    case IEventListener.CONNECTION_LOGOUT:
                        Console.WriteLine("CONNECTION_LOGOUT");
                        LogHandler.Info("GateWay CONNECTION_LOGOUT");
                        this.Dispatcher.Invoke(() =>
                        {
                            this.states.Text = "连接退出";
                        });
                        break;
                }
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void starts(object sender, RoutedEventArgs e)
        {
            this.startPlay.Visibility = Visibility.Hidden;
            this.endPlay.Visibility = Visibility.Visible;
            LoginBefor();
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ends(object sender, RoutedEventArgs e)
        {
            this.endPlay.Visibility = Visibility.Hidden;
            this.startPlay.Visibility = Visibility.Visible;
            LogHandler.Info("stop unsubScribe");
            async.Clear();
            this.unSubScribe();
            if (cls != null)
            {
                cls.StopT();
            }

        }


        //初始化按钮
        private void btnLoad(object sender, RoutedEventArgs e)
        {
            this.endPlay.Visibility = Visibility.Hidden;
            this.startPlay.Visibility = Visibility.Visible;
        }



        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.Hide();
        }

        /// <summary>
        /// 关闭程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginHelper.Instance.AppExit();
        }

        /// <summary>
        /// 解除界面不能随意拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// 定时任务，每天定时获取最新的token
        /// </summary>
        private void callScheduleManage()
        {
            try
            {
                //创建调度单元
                Task<IScheduler> tsk = StdSchedulerFactory.GetDefaultScheduler();
                IScheduler scheduler = tsk.Result;
                //2.创建一个具体的作业即job (具体的job需要单独在一个文件中执行)
                IJobDetail job = JobBuilder.Create<ScheduleToken>().WithIdentity("完成").Build();
                //3.创建并配置一个触发器即trigger   1s执行一次
                ITrigger _CronTrigger = TriggerBuilder.Create()
                  .WithIdentity("定时确认")
                  .WithCronSchedule(SpeechConsts.schJob) //秒 分 时 某一天 月 周 年(可选参数)
                  .Build()
                  as ITrigger;
                //4.将job和trigger加入到作业调度池中
                scheduler.ScheduleJob(job, _CronTrigger);
                //5.开启调度
                scheduler.Start();
            }
            catch (Exception e)
            {
                LogHandler.Error(e.Message);
            }

        }
    }

}
