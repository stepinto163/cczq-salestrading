using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Fix.Component
{
 	/// <summary>
	/// StandardTrailer
	/// </summary>
	/// <remarks>
	/// 2020.01.15: 创建. 模板插件(V1.3.6825.19619(build:20180908)) 模板版本(v1.0.1.1)<br/>
	/// </remarks>
public class StandardTrailer : BaseMessage
{

public StandardTrailer Clone()
	{
		StandardTrailer t=new StandardTrailer();
		return t;
	}
	}
}

