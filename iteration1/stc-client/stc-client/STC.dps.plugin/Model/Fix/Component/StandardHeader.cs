 
using System;
using System.Collections.Generic;
using System.Text;

namespace STC.dps.plugin.Model.Fix.Component
{
 	/// <summary>
	/// StandardHeader
	/// </summary>
	/// <remarks>
	/// 2020.01.15: 创建. 模板插件(V1.3.6825.19619(build:20180908)) 模板版本(v1.0.1.1)<br/>
	/// </remarks>
public class StandardHeader : BaseMessage
{

	public string MsgType
	{
	get{ return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MsgType)); }
	set{ this.Put(MessageDictionary.Fields.MsgType, value); }
	}
public StandardHeader Clone()
	{
		StandardHeader t=new StandardHeader();
		t.MsgType=MsgType;
		return t;
	}
	}
}

