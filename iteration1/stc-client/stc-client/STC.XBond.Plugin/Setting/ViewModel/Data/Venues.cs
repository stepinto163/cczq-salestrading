using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel.Data
{
    public class Venues : ViewModelBase
    {
        public int sn; 

        public int Sn
        {
            get
            {
                return this.sn;
            }

            set
            {
                if (this.sn != value)
                {
                    this.sn = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string venue = string.Empty;

        public string Venue
        {
            get
            {
                return this.venue;
            }

            set
            {
                if (this.venue != value)
                {
                    this.venue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string name = string.Empty;

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string type = string.Empty;

        public string Type
        {
            get
            {
                return this.type;
            }

            set
            {
                if (this.type != value)
                {
                    this.type = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int level;

        public int Level
        {
            get
            {
                return this.level;
            }

            set
            {
                if (this.level != value)
                {
                    this.level = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string levelformat;

        public string Levelformat
        {
            get
            {
                return this.levelformat;
            }

            set
            {
                if (this.levelformat != value)
                {
                    this.levelformat = value;
                    this.OnPropertyChanged();
                }
            }
        }
        
        private int weight;

        public int Weight
        {
            get
            {
                return this.weight;
            }
            set
            {
                if (this.weight != value)
                {
                    this.weight = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string venuetypegw;

        public string Venuetypegw
        {
            get
            {
                return this.venuetypegw;
            }
            set
            {
                if (this.venuetypegw != value)
                {
                    this.venuetypegw = value;
                    this.OnPropertyChanged();
                }
            }
        }
       

        #region 信用
        private string venuec = string.Empty;

        public string Venuec
        {
            get
            {
                return this.venuec;
            }

            set
            {
                if (this.venuec != value)
                {
                    this.venuec = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string namec = string.Empty;

        public string Namec
        {
            get
            {
                return this.namec;
            }

            set
            {
                if (this.namec != value)
                {
                    this.namec = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string typec = string.Empty;

        public string Typec
        {
            get
            {
                return this.typec;
            }

            set
            {
                if (this.typec != value)
                {
                    this.typec = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int levelc;

        public int Levelc
        {
            get
            {
                return this.levelc;
            }

            set
            {
                if (this.levelc != value)
                {
                    this.levelc = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string levelformatc;

        public string Levelformatc
        {
            get
            {
                return this.levelformatc;
            }

            set
            {
                if (this.levelformatc != value)
                {
                    this.levelformatc = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int weightc;

        public int Weightc
        {
            get
            {
                return this.weightc;
            }
            set
            {
                if (this.weightc != value)
                {
                    this.weightc = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string venuetypegwc;

        public string Venuetypegwc
        {
            get
            {
                return this.venuetypegwc;
            }
            set
            {
                if (this.venuetypegwc != value)
                {
                    this.venuetypegwc = value;
                    this.OnPropertyChanged();
                }
            }
        }
        #endregion
    }
}