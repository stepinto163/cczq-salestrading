using STC.Common.Handler.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.Validation;
using STC.Mm.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using STC.Frame.Utils;

namespace STC.Mm.Plugin.ViewModel
{
    public class UpdateWidenSettingViewModel : ViewModelBase
    {
        #region 属性

        private string _midWidenSpreadB_BNC;
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmpty")]
        public string MidWidenSpreadB_BNC
        {
            get { return _midWidenSpreadB_BNC; }
            set { 
                if (_midWidenSpreadB_BNC != value)
                {
                    _midWidenSpreadB_BNC = value;
                    this.OnPropertyChanged();
                }
                
            }
        }

        private string _defenBP_BNC;
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmpty")]
        public string DefenBP_BNC
        {
            get
            {
                return _defenBP_BNC;
            }
            set
            {
                if (_defenBP_BNC != value)
                {
                    _defenBP_BNC = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string _midWidenSpreadB_BCO;

        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmpty")]
        public string MidWidenSpreadB_BCO
        {
            get
            {
                return _midWidenSpreadB_BCO;
            }
            set
            {
                if (_midWidenSpreadB_BCO != value)
                {
                    _midWidenSpreadB_BCO = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _defenBP_BCO;
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmpty")]
        public string DefenBP_BCO
        {
            get
            {
                return _defenBP_BCO;
            }
            set
            {
                if (_defenBP_BCO != value)
                {
                    _defenBP_BCO = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _resultErrorInfo;

        public string ResultErrorInfo
        {
            get { return _resultErrorInfo; }
            set
            {
                if (_resultErrorInfo != value)
                {
                    _resultErrorInfo = value;
                }

            }
        }

        #endregion

        public ICommand CancelBtnCommand { get; set; }

        public ICommand ConfirmBtnCommand { get; set; }

       

        public UpdateWidenSettingViewModel()
        {
            if (this.CancelBtnCommand == null)
                this.CancelBtnCommand = new RelayCommand<Window>(CloseWindow);

            if (this.ConfirmBtnCommand == null)
                this.ConfirmBtnCommand = new RelayCommand<Window>(Confirm);

            Task.Run(() => {
                MMQueryWidenSettingReplyPo reply = MMHandler.Instance.mmQueryWidenSetting();
                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null && reply.Datas.Count > 0)
                    {
                        foreach(MMUpdateWidenSettingReq item in reply.Datas)
                        {
                            if (MMConstants.BNC == item.type)
                            {
                                this.MidWidenSpreadB_BNC = item.midWidenSpreadB;
                                this.DefenBP_BNC = item.defenBP;
                            }
                            else if (MMConstants.BCO == item.type)
                            {
                                this.MidWidenSpreadB_BCO = item.midWidenSpreadB;
                                this.DefenBP_BCO = item.defenBP;
                            }
                        }
                    }
                }
            });
        }

        private async void Confirm(Window win)
        {
            PResult result = null;
            await Task.Run(() =>
            {
                List<MMUpdateWidenSettingReq> reqList = new List<MMUpdateWidenSettingReq>();
                reqList.Add(new MMUpdateWidenSettingReq()
                {
                    defenBP = this.DefenBP_BNC, //防守点
                    midWidenSpreadB = this.MidWidenSpreadB_BNC,// 拉宽点
                    type = "BNC",
                });
                reqList.Add(new MMUpdateWidenSettingReq()
                {
                    defenBP = this.DefenBP_BCO,
                    midWidenSpreadB = this.MidWidenSpreadB_BCO,
                    type = "BCO",
                });
                result = MMHandler.Instance.mmUpdateWidenSetting(reqList);
            });
            if (result != null)
            {
                if (result.IsSuccess())
                {
                    if (win != null)
                    {
                        win.Close();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(result.Msg) == false)
                    {
                        this.ResultErrorInfo = result.Msg;
                    }
                    else
                    {
                        this.ResultErrorInfo = "更新失败";
                    }
                }
            }
            else
            {
                this.ResultErrorInfo = "更新失败";
            }
        }

        private void CloseWindow(Window win)
        {
            if (win != null)
            {
                win.Close();
            }
        }
    }
}
