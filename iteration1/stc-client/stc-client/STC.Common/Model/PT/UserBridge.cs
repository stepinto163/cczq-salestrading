using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class UserBridge : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// 主键
        /// </summary>
        public string bridgeId;
        /// <summary>
        /// brokerid
        /// </summary>
        public string partySubid;
        /// <summary>
        /// broker名称
        /// </summary>
        public string partySubname;
        /// <summary>
        /// 机构id
        /// </summary>
        public string bankInstitutionid;
        /// <summary>
        /// 机构名称
        /// </summary>
        public string bankInstitutionname;
        /// <summary>
        /// 机构台子简称
        /// </summary>
        public string bankShortName;
        /// <summary>
        /// 交易员id
        /// </summary>
        public string bankTraderid;
        /// <summary>
        /// 交易员名称
        /// </summary>
        public string bankTradername;
        /// <summary>
        /// 联系方式
        /// </summary>
        public string contactType;
        /// <summary>
        /// 机构id-计费
        /// </summary>
        public string feeTraderid;
        /// <summary>
        /// 机构名称-计费
        /// </summary>
        public string feeTradername;
        /// <summary>
        /// 交易员ID-计费
        /// </summary>
        public string feeInstitutionid;
        /// <summary>
        /// 交易员名称-计费
        /// </summary>
        public string feeInstitutionname;
        /// <summary>
        /// 渠道：1:对话，2:请求,3:双边,4:xbond,5:ideal
        /// </summary>
        public string channel;
        /// <summary>
        /// 发给
        /// </summary>
        public string sendto;

        /// <summary>
        /// 发给显示
        /// </summary>
        public string Sendto
        {
            get { return sendto; }
            set
            {
                sendto = value;
                NotifyPropertyChanged("Sendto");
                NotifyPropertyChanged("InstTraderSendToShow");
            }
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark;
        /// <summary>
        /// 合单备注
        /// </summary>
        public string combinedOrderRemark;

        /// <summary>
        /// 机构名称拼音：用于检索
        /// </summary>
        public string PinYin { get; set; } = string.Empty;
        /// <summary>
        /// 机构名称全拼
        /// </summary>
        public string PinYinFull { get; set; } = string.Empty;
        /// <summary>
        /// 交易员拼音：用于检索
        /// </summary>
        public string TraderPinYin { get; set; } = string.Empty;
        /// <summary>
        /// 交易员名称全拼
        /// </summary>
        public string TraderPinYinFull { get; set; } = string.Empty;

        /// <summary>
        /// 排序码
        /// </summary>
        public int? OderCode { get; set; }
        /// <summary>
        /// 对象拷贝
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        public static UserBridge Copy(UserBridge old)
        {
            UserBridge userBridge = new UserBridge();
            userBridge.bankTraderid = old.bankTraderid;
            userBridge.bankTradername = old.bankTradername;
            userBridge.bankInstitutionname = old.bankInstitutionname;
            userBridge.bankInstitutionid = old.bankInstitutionid;
            userBridge.channel = old.channel;
            userBridge.sendto = old.sendto;
            userBridge.remark = old.remark;
            userBridge.PinYin = old.PinYin;
            userBridge.PinYinFull = old.PinYinFull;
            userBridge.TraderPinYin = old.TraderPinYin;
            userBridge.TraderPinYinFull = old.TraderPinYinFull;
            userBridge.combinedOrderRemark = old.combinedOrderRemark;
            //V2.8新增字段
            userBridge.bridgeId = old.bridgeId;
            userBridge.contactType = old.contactType;
            userBridge.partySubid = old.partySubid;
            userBridge.partySubname = old.partySubname;
            userBridge.feeInstitutionid = old.feeInstitutionid;
            userBridge.feeInstitutionname = old.feeInstitutionname;
            userBridge.feeTraderid = old.feeTraderid;
            userBridge.feeTradername = old.feeTradername;
            //V2.9.2新增字段
            userBridge.OderCode = old.OderCode;
            return userBridge;
        }

        //*************V2.8*************
        /// <summary>
        /// 机构+交易员 
        /// </summary>

        public string InstTraderShow
        {
            get { return string.Format("{0}({1})", bankInstitutionname, bankTradername); }
        }
        /// <summary>
        /// 机构+交易员+发给 
        /// </summary>

        public string InstTraderSendToShow
        {
            get { return string.Format("{0}({1}){2}", bankInstitutionname, bankTradername, sendto); }
        }
        private string execOrderCount;
        /// <summary>
        /// 桥下成交单数量
        /// 如果添加桥数字+1，预约桥数字+1，但如果预约桥被桥机构拒绝后，取消，则数字-1。
        /// 如果尚未添加桥，则不显示数字框。数字根据筛选条件而改变，即桥下面当前显示几笔交易，则数字根据此显示。
        /// </summary>
        public string ExecOrderCount
        {
            get
            {
                return execOrderCount;
            }
            set
            {
                execOrderCount = value;
                NotifyPropertyChanged("ExecOrderCount");
            }
        }

    }
}