using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public class EnumHelper
    {
        private static EnumHelper instance = null;
        private static readonly object padlock = new object();

        private List<ComboxBaseModel> _comboxBases;
        public List<ComboxBaseModel> ChannelComboxModel
        {
            get
            {
                return _comboxBases;
            }
        }

        private EnumHelper()
        {
            InitChannel();
        }

        public static EnumHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        instance = new EnumHelper();
                    }
                }
                return instance;
            }
        }


        private void InitChannel()
        {
            List<ComboxBaseModel> comboxBaseModels = new List<ComboxBaseModel>();
            foreach (var item in Enum.GetValues(typeof(ChannelEnum)))
            {
                FieldInfo fieldInfo = item.GetType().GetField(item.ToString());
                DescriptionAttribute descriptionAttribute = System.Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
                string valueEnum = Convert.ToInt32(item).ToString();
                string keyEnum = item.ToString();
                string desEnum = descriptionAttribute.Description;
                ComboxBaseModel comboxBaseModel = new ComboxBaseModel()
                {
                    DisplayMemberPath = desEnum,
                    KeyMemberPath = keyEnum,
                    ValueMemberPath = valueEnum
                };
                comboxBaseModels.Add(comboxBaseModel);
            }
            _comboxBases = comboxBaseModels;
        }

    }
}
