package com.gateway.invoke.security;

import java.util.ArrayList;
import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.proto.Proto;
import com.gateway.message.SystemMessage;

public class SqlInjSecurityCheck implements ISecurityCheck{
	private final Logger logger = LoggerFactory.getLogger(this.getClass());
	private static String WhiteListCheck = "10004";
	private static String WhiteListCheckMsg = "禁止访问";
	private List<String> sqls = new ArrayList<String>();

	public void init() {
		sqls.add("'");
		sqls.add("\"");
		sqls.add("{");
		sqls.add("}");
		sqls.add("[");
		sqls.add("]");
		sqls.add("\\");
		//sqls.add(":");
		sqls.add(";");
		sqls.add("<");
		sqls.add(">");
		sqls.add("?");
		sqls.add(",");
		sqls.add(".");
		sqls.add("`");
		sqls.add("~");
		sqls.add("!");
		//sqls.add("@");
		sqls.add("$");
		sqls.add("%");
		sqls.add("^");
		sqls.add("and");
		sqls.add("exec");
		sqls.add("insert");
		sqls.add("select");
		sqls.add("delete");
		sqls.add("update");
		sqls.add("count");
		sqls.add("count");
		sqls.add("*");
		sqls.add("chr");
		sqls.add("mid");
		sqls.add("master");
		sqls.add("truncate");
		sqls.add("char");
		sqls.add("declare");
		sqls.add("or");

	}
	@Override
	public SecurityResult check(SystemMessage sMsg, Proto message, String serverName, String method, String content) {
		SecurityResult pr=new SecurityResult();		 
		if(isInj(content)) {
			pr.Code=WhiteListCheck;
			pr.Msg=WhiteListCheckMsg;
		}
		return pr;
	}
	private boolean isInj(String value) {
		boolean flag = false;
		for (String sql : sqls) {
			if (value.indexOf(sql) >= 0) {
				flag = true;
				logger.warn(String.format("value:%s sql:%s", value, sql));
				break;
			}
		}
		return flag;
	}
}
