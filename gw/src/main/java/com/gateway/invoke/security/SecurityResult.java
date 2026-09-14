package com.gateway.invoke.security;

public class SecurityResult {
	public boolean isSuccess()
	{
		return "0000".equals(Code);
	}
	public String Code="0000";
	public String Msg="Success";
}
