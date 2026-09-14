package com.gateway.invoke.security;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.CopyOnWriteArraySet;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.regex.Pattern;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.proto.Proto;
import com.gateway.message.SystemMessage;

public class APIKeySecurityCheck implements ISecurityCheck {
	private Logger logger = LoggerFactory.getLogger(APIKeySecurityCheck.class);

	private static final String ListCheck = "10004";
	private static final String ListCheckMsg = "没有权限访问,请检查apikey是否正确";
	private String apiKeyListPath;
	private long period = 6 * 60;

	public long getPeriod() {
		return period;
	}

	public void setPeriod(long period) {
		this.period = period;
	}

	public String getApiKeyListPath() {
		return apiKeyListPath;
	}

	public void setApiKeyListPath(String apiKeyListPath) {
		this.apiKeyListPath = apiKeyListPath;
	}

	private volatile ConcurrentHashMap<String, CopyOnWriteArraySet<String>> apiKeyListHm = new ConcurrentHashMap<String, CopyOnWriteArraySet<String>>();
	private volatile CopyOnWriteArrayList<ApiKeyPatternRule> apiKeyPatternRules = new CopyOnWriteArrayList<ApiKeyPatternRule>();
	private ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);

	private Runnable task1 = () -> initData();

	public void init() {
		initData();
		executor.scheduleAtFixedRate(task1, period, period, TimeUnit.MINUTES);
	}

	private void initData() {
		try {
			ArrayList<String> tmp = readAll(apiKeyListPath);
			ConcurrentHashMap<String, CopyOnWriteArraySet<String>> exactHm = new ConcurrentHashMap<String, CopyOnWriteArraySet<String>>();
			CopyOnWriteArrayList<ApiKeyPatternRule> patternRules = new CopyOnWriteArrayList<ApiKeyPatternRule>();
			if (tmp != null) {
				for (String str : tmp) {
					String[] strs = str.split(",", 3);
					if (strs.length > 2) {
						String apiKey = strs[0].trim();
						String serverName = normalizePattern(strs[1]);
						String topic = normalizePattern(strs[2]);
						if (hasPattern(serverName) || hasPattern(topic)) {
							patternRules.add(new ApiKeyPatternRule(apiKey, serverName, topic));
							logger.info("pattern {}_{} put {}", serverName, topic, apiKey);
							continue;
						}

						String key = serverName + "_" + topic;
						CopyOnWriteArraySet<String> cas = exactHm.get(key);
						if (cas == null) {
							cas = new CopyOnWriteArraySet<String>();
							exactHm.put(key, cas);
						}
						if (!cas.contains(apiKey)) {
							cas.add(apiKey);
							logger.info("{} put {} ", key, apiKey);
						}
					}
				}
			}
			apiKeyListHm = exactHm;
			apiKeyPatternRules = patternRules;
		} catch (Exception e) {
			logger.error("initData", e);
		}
	}

	@Override
	public SecurityResult check(SystemMessage sMsg, Proto message, String serverName, String method, String content) {
		SecurityResult pr = new SecurityResult();
		if (sMsg.isApiKeyCheck()) {
			String server = serverName == null ? "" : serverName.toLowerCase();
			String topic = method == null ? "" : method.toLowerCase();
			String key = server + "_" + topic;
			CopyOnWriteArraySet<String> cas = apiKeyListHm.get(key);
			String apiKey = (sMsg.getApiKey()+"").toLowerCase();		 
			if (cas != null) {
				if (cas.contains(apiKey)) {
					return pr;
				}				 
			}
			if (!matchPatternRule(server, topic, apiKey)) {
				pr.Code = ListCheck;
				pr.Msg = ListCheckMsg;
			}
		}
		return pr;
	}

	private boolean matchPatternRule(String serverName, String method, String apiKey) {
		if (apiKeyPatternRules == null || apiKeyPatternRules.isEmpty()) {
			return false;
		}
		for (ApiKeyPatternRule rule : apiKeyPatternRules) {
			if (rule.matches(serverName, method, apiKey)) {
				return true;
			}
		}
		return false;
	}

	private static boolean hasPattern(String value) {
		return value != null && (value.contains("*") || value.contains("${"));
	}

	private static String normalizePattern(String value) {
		if (value == null) {
			return "";
		}
		return value.trim().replaceAll("\\$\\{[^}]+\\}", "*").toLowerCase();
	}

	private static final class ApiKeyPatternRule {
		private final String apiKey;
		private final Pattern serverPattern;
		private final Pattern methodPattern;

		private ApiKeyPatternRule(String apiKey, String serverPattern, String methodPattern) {
			this.apiKey = apiKey;
			this.serverPattern = toPattern(serverPattern);
			this.methodPattern = toPattern(methodPattern);
		}

		private boolean matches(String serverName, String method, String apiKey) {
			return this.apiKey.equals(apiKey) && serverPattern.matcher(serverName).matches()
					&& methodPattern.matcher(method).matches();
		}

		private static Pattern toPattern(String value) {
			StringBuilder sb = new StringBuilder("^");
			for (int i = 0; i < value.length(); i++) {
				char ch = value.charAt(i);
				if (ch == '*') {
					sb.append(".*");
				} else {
					sb.append(Pattern.quote(String.valueOf(ch)));
				}
			}
			sb.append("$");
			return Pattern.compile(sb.toString());
		}
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
