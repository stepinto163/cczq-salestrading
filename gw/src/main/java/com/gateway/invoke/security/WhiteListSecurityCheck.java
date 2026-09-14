package com.gateway.invoke.security;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArraySet;

import com.gateway.connector.proto.Proto;
import com.gateway.connector.utils.TopicUtils;
import com.gateway.message.SystemMessage;

public class WhiteListSecurityCheck implements ISecurityCheck {

	private static String WhiteListCheck = "10004";
	private static String WhiteListCheckMsg = "禁止访问";
	private String whiteListPath;

	public String getWhiteListPath() {
		return whiteListPath;
	}

	public void setWhiteListPath(String whiteListPath) {
		this.whiteListPath = whiteListPath;
	}

	private ConcurrentHashMap<String, CopyOnWriteArraySet<String>> whiteListHm = new ConcurrentHashMap<String, CopyOnWriteArraySet<String>>();

	public void init() throws Exception {
		ArrayList<String> tmp = readAll(whiteListPath);
		tmp.add("loginsvr,login");//默认登录进入白名单
		if (tmp != null) {
			whiteListHm.clear();
			for (String str : tmp) {
				String[] strs = str.split(",");
				if (strs.length > 1) {
					String serverName = strs[0];
					String topic = strs[1];
					CopyOnWriteArraySet<String> cas = whiteListHm.get(serverName);
					if (cas == null) {
						cas = new CopyOnWriteArraySet<>();
						whiteListHm.put(serverName, cas);
					}
					if (!cas.contains(topic)) {
						cas.add(topic);
					}
				}
			}

		}
	}

	@Override
	public SecurityResult check(SystemMessage sMsg, Proto message, String serverName, String method, String content) {
		SecurityResult pr = new SecurityResult();		 
		CopyOnWriteArraySet<String> cas = whiteListHm.get(serverName.toLowerCase());
		if (cas != null) {
			if (method.equals("subscribe") || method.equals("unsubscribe")) {
				for (String stopic : cas) {
					if (TopicUtils.matchTopic(stopic, content.toLowerCase())) {
						return pr;
					}
				}
				pr.Code = WhiteListCheck;
				pr.Msg = WhiteListCheckMsg;
			} else if (!cas.contains(method.toLowerCase())) {
				pr.Code = WhiteListCheck;
				pr.Msg = WhiteListCheckMsg;
			}
		} else {
			pr.Code = WhiteListCheck;
			pr.Msg = WhiteListCheckMsg;
		}
		return pr;
	}

	public static ArrayList<String> readAll(String fileName) throws Exception {
		ArrayList<String> list = new ArrayList<String>();

		InputStreamReader fileReader = null;
		BufferedReader reader = null;
		try {

			fileReader = new InputStreamReader(new FileInputStream(fileName), Charset.forName("utf-8"));
			reader = new BufferedReader(fileReader);
			String line = null;
			while ((line = reader.readLine()) != null) {

				if (line != null && line.length() > 0) {
					list.add(line.toLowerCase());
				}
			}
		} finally {
			try {
				reader.close();
			} catch (IOException e) {
			}
			try {
				fileReader.close();
			} catch (IOException e) {
			}
		}
		return list;
	}
}
