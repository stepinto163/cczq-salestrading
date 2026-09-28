using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.PrePaidList.Event
{
   public class ReflextionUtils
    {
		/*
	 * 反射获取对象字段
	 */
		private static Dictionary<string, Dictionary<string, PropertyInfo>> propertys = new Dictionary<string, Dictionary<string, PropertyInfo>>();
		//private static Dictionary<string, Dictionary<string, Method>> methods = new Dictionary<string, Dictionary<string, Method>>();
	}

	public class PropertyInfo
	{

		private string name = "";// 名字
		private string value = "";

		public String getValue()
		{
			return value;
		}

		public void setValue(String value)
		{
			this.value = value;
		}

		public String getName()
		{
			return name;
		}

		public void setName(String name)
		{
			this.name = name;
		}

		public String getPropertyType()
		{
			return propertyType;
		}

		public void setPropertyType(String propertyType)
		{
			this.propertyType = propertyType;
		}

		private String propertyType = "";// 类型
	}
}
