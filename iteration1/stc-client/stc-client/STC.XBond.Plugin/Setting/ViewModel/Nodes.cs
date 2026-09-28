using STC.Common.Model.Admin;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public class Nodes : ViewModelBase
    {
        public Nodes()
        {
            this.IsSelected = false;
            this.IsEnabled = false;
            this.Childs = new List<Nodes>();
        }
        public bool initSelect = false;
        public bool IsChange
        {
            get
            {
                if (initSelect == IsSelected)
                    return false;
                else
                    return true;
            }
        }
        public string id { get; set; }
        public string Id
        {
            get
            {
                return this.id;
            }

            set
            {
                if (this.id != value)
                {
                    this.id = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        ///  节点名称
        ///</summary>
        public string modulename { get; set; }
        public string Modulename
        {
            get
            {
                return this.modulename;
            }

            set
            {
                if (this.modulename != value)
                {
                    this.modulename = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public List<ModulesAction> actionst { get; set; }
        public List<ModulesAction> Actionst
        {
            get
            {
                return this.actionst;
            }

            set
            {
                if (this.actionst != value)
                {
                    this.actionst = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool? isSelected = false;
        /// <summary>
        /// 是否勾选
        /// </summary>
        public bool? IsSelected
        {
            set
            {
                this.isSelected = value;
                this.OnPropertyChanged("IsSelected");
            }
            get
            {
                return this.isSelected;
            }
        }

        private bool? isEnabled = false;
        /// <summary>
        /// 是否勾选
        /// </summary>
        public bool? IsEnabled
        {
            set
            {
                this.isEnabled = value;
                this.OnPropertyChanged("IsEnabled");
            }
            get
            {
                return this.isEnabled;
            }
        }

        public string parentid { get; set; }
        public string ParentId
        {
            get
            {
                return this.parentid;
            }

            set
            {
                if (this.parentid != value)
                {
                    this.parentid = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string remark; 
        public string Remark
        {
            get
            {
                return this.remark;
            }

            set
            {
                if (this.remark != value)
                {
                    this.remark = value;
                    this.OnPropertyChanged();
                }
            }
        }
        #region action


        #endregion
        /// <summary>
        /// 父项
        /// </summary>
        public Nodes Parent{ get; set; }
        /// <summary>
        ///  子节点集合
        /// </summary>
        public List<Nodes> Childs { get; set; }
    }
}