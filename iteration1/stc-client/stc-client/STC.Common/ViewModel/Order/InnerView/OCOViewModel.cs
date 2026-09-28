using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.ViewModel.Order.InnerView
{
    public class OCOViewModel : BaseViewModel
    {

        #region

        private bool _checkAll = false;
        public bool CheckAll
        {
            get
            {
                return _checkAll;
            }

            set
            {
                _checkAll = value;
                this.OnPropertyChanged();


                if (value)
                {
                    CheckInner = false;
                    CheckXBond = false;
                    CheckNotOut = false;
                }
            }
        }

        private bool _checkInnerEnable = false;
        public bool CheckInnerEnable
        {
            get
            {
                return _checkInnerEnable;
            }

            set
            {
                _checkInnerEnable = value;
                this.OnPropertyChanged();
            }
        }


        private bool _checkInner = false;
        public bool CheckInner
        {
            get
            {
                return _checkInner;
            }

            set
            {
                _checkInner = value;
                this.OnPropertyChanged();

                if (value)
                {
                    CheckAll = false;
                    CheckXBond = false;
                    CheckNotOut = true;
                }
                else
                {
                    CheckNotOut = false;
                    CheckNotoutEnable = true;
                }
            }
        }


        private bool _checkXBondEnable = false;
        public bool CheckXBondEnable
        {
            get
            {
                return _checkXBondEnable;
            }

            set
            {
                _checkXBondEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _checkXBond = false;
        public bool CheckXBond
        {
            get
            {
                return _checkXBond;
            }

            set
            {
                _checkXBond = value;

                this.OnPropertyChanged();

                if (value)
                {
                    CheckAll = false;
                    CheckInner = false;

                    if (CheckXBondVisibility == Visibility.Visible)
                    {
                        if (!_checkXBond)
                        {
                            //if (DirectionItems != null)
                            //{
                            //    foreach (ListBoxItemData itemData in DirectionItems)
                            //    {
                            //        if (!itemData.Name.ToUpper().Equals(XBOND))
                            //            itemData.ItemEnable = false;
                            //    }
                            //}
                        }
                        else
                        {
                            //if (DirectionItems != null)
                            //{
                            //    foreach (ListBoxItemData itemData in DirectionItems)
                            //    {
                            //        if (!itemData.Name.ToUpper().Equals(XBOND))
                            //        {
                            //            itemData.ItemVisibility = Visibility.Collapsed;
                            //            itemData.Checked = false;
                            //        }
                            //        else
                            //        {
                            //            if (!CheckNotOut)
                            //            {
                            //                itemData.Checked = true;
                            //                itemData.ItemEnable = true;
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                }
            }
        }
    



        private bool _checkNotoutEnable = false;
        public bool CheckNotoutEnable
        {
            get
            {
                return _checkNotoutEnable;
            }

            set
            {
                _checkNotoutEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _checkNotOut = false;
        public bool CheckNotOut
        {
            get
            {
                return _checkNotOut;
            }

            set
            {
                _checkNotOut = value;

                this.OnPropertyChanged();
            }
        }

        #endregion

        #region 是否可见

        private Visibility _checkAllVisibility = Visibility.Visible;
        public Visibility CheckAllVisibility
        {
            get
            {
                return _checkAllVisibility;
            }

            set
            {
                _checkAllVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _checkInnerVisibility = Visibility.Collapsed;
        public Visibility CheckInnerVisibility
        {
            get
            {
                return _checkInnerVisibility;
            }

            set
            {
                _checkInnerVisibility = value;

                this.OnPropertyChanged(nameof(CheckInnerVisibility));
            }
        }

        private Visibility _checkXBondVisibility = Visibility.Collapsed;
        public Visibility CheckXBondVisibility
        {
            get
            {
                return _checkXBondVisibility;
            }

            set
            {
                _checkXBondVisibility = value;

                this.OnPropertyChanged();
            }
        }


        private Visibility _chanelAllVisibility = Visibility.Collapsed;
        public Visibility ChanelAllVisibility
        {
            get
            {
                return _chanelAllVisibility;
            }

            set
            {
                _chanelAllVisibility = value;

                this.OnPropertyChanged();
            }
        }
        #endregion


        /// <summary>
        /// 撮合渠道是否可见
        /// </summary>
        private Visibility venuevisible = Visibility.Visible;
        public Visibility VenueVisible
        {
            get { return venuevisible; }
            set
            {
                venuevisible = value;
                this.OnPropertyChanged("VenueVisible");
            }
        }

        public void SetCheckAll(bool isCheck)
        {
            _checkAll = isCheck;
            this.OnPropertyChanged("CheckAll");
        }


        public void SetCheckAllVisibility(Visibility visibility)
        {
            _checkAllVisibility = visibility;
            this.OnPropertyChanged("CheckAllVisibility");
        }

        public void SetCheckXBondVisibility(Visibility visibility)
        {
            _checkXBondVisibility = visibility;
            this.OnPropertyChanged("CheckXBondVisibility");
        }

        public void SetCheckInner(bool isCheck)
        {
            _checkInner = true;
            this.OnPropertyChanged("CheckInner");
        }

        public void SetCheckXBond(bool isCheck)
        {
            _checkXBond = isCheck;
            this.OnPropertyChanged("CheckXBond");
        }

        public void SetCheckXBondEnable(bool isEnable)
        {
            _checkXBond = false;
        }

        public void InitUI()
        {
            if ((!PermisionHelper.Instance.IsHaveInnerPermission) && (!PermisionHelper.Instance.IsHaveInnerXbondPermission) && (!PermisionHelper.Instance.IsHaveAllPermission))
            {
                VenueVisible = Visibility.Collapsed;
                CheckNotoutEnable = false;
                CheckNotOut = true;
            }
            else
            {
                this.VenueVisible = (!PermisionHelper.Instance.IsHaveVenueInnerPermission && !PermisionHelper.Instance.IsHaveVenueXbondPermission) ? Visibility.Collapsed : Visibility.Visible;
            }
        }


        

    }
}
