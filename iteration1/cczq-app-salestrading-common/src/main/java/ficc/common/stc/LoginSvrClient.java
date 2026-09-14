package ficc.common.stc;

// Reused from sc-app-stc sc.app.stc.utils.LoginSvrClient.
// Package isolation prevents the older runtime JAR from shadowing this source version.

import java.util.HashMap;
import java.util.Hashtable;
import java.util.Map;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import org.apache.http.util.TextUtils;
import org.slf4j.Logger;

import com.adaptiveMQ2.message.Destination;
import com.adaptiveMQ2.message.Message;
import com.adaptiveMQ2.message.MessageBody;
import com.adaptiveMQ2.utils.ConstsMessage;

import ats.app.api.BaseApi;
import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;
import sc.app.stc.net.NetClientWrapper;

public class LoginSvrClient extends BaseApi {

	public static interface ILoginMsgHandler {
		public void process(Map<String, Object> map);
	}

	public static class UserInfo {
		public String sid;
		public String name; //用户名
		public String username;//登录名
		public String venueusersid;//对应渠道账户编号
		public String userid;
		public String biztypestr;//业务类型
		public String biztypeid;//业务类型编号
		public String roleid; //角色id
		public String rolename;//角色名
		public String ip;//ip
		public String mac;//主mac
		public String mac1;//备用mac1
		public String mac2;//备用mac2
		public String clientType;//登录类型
        public String groupids;// 分组id
        public String groupnames;// 组名
		public String loginMac;//登录MAC
		public String infs;//json
		public static UserInfo parse(Map<String, Object> resultMap) {
			UserInfo userInfo = new UserInfo();
			userInfo.sid = get("sid", resultMap);
			if (TextUtils.isEmpty(userInfo.sid)) {
				userInfo = null;
			} else {
				String code = get("Code", resultMap);
				if ("0001".equals(code)) {
					userInfo.name = get("name", resultMap);
					userInfo.username = get("username", resultMap);
					userInfo.userid = get("userid", resultMap);
					userInfo.venueusersid = get("venueusersid", resultMap);
					userInfo.biztypestr = get("biztypestr", resultMap);
					userInfo.biztypeid = get("biztypeid", resultMap);
					userInfo.roleid = get("roleid", resultMap);
					userInfo.rolename = get("rolename", resultMap);
					userInfo.ip = get("ip", resultMap);
					userInfo.mac = get("mac", resultMap);
					userInfo.mac1 = get("mac1", resultMap);
					userInfo.mac2 = get("mac2", resultMap);
					userInfo.clientType = get("clientType", resultMap);
					userInfo.groupids = get("groupids", resultMap);
					userInfo.groupnames = get("groupnames", resultMap);
					userInfo.loginMac = get("loginMac", resultMap);
					userInfo.infs = get("infs",resultMap);
				} else {
					userInfo = null;
				}
			}
			return userInfo;
		}

		private static String get(String string, Map<String, Object> resultMap) {
			Object object = resultMap.get(string);
			if (object != null) {
				return object.toString();
			}
			return null;
		}
	}

	private static Hashtable<String, ILoginMsgHandler> htMsgHandler = null;

	static {
		htMsgHandler = new Hashtable<String, ILoginMsgHandler>();
		htMsgHandler.put("login", new ILoginMsgHandler() {

			@Override
			public void process(Map<String, Object> map) {
				map.put("Code", "0001");
				handleMap(map, 1);
			}
		});

		htMsgHandler.put("logout", new ILoginMsgHandler() {

			@Override
			public void process(Map<String, Object> map) {
				handleMap(map, 0);
			}
		});
	}

	private static Logger logger = AsyncSlf4jLoggerFactory.getLogger(LoginSvrClient.class);
	private static Hashtable<String, UserInfo> sid2Account = new Hashtable<String, UserInfo>();
	private static Hashtable<Integer, String> name2Sid = new Hashtable<Integer, String>();
	private static NetClientWrapper cedaClientWrapper;
	private static String serverKey;
    private static long requestTimeoutMillis = 3000;

    public static void setRequestTimeoutMillis(long timeoutMillis) {
        requestTimeoutMillis = timeoutMillis;
    }
	public static boolean isLoadInstAuth = false;
	public static ReadWriteLock Instcodetableslock = new ReentrantReadWriteLock();
	public static ReadWriteLock Usernametableslock = new ReentrantReadWriteLock();
	public static ReadWriteLock InstTypetableslock = new ReentrantReadWriteLock();

	public static void initialize(String serverKey, NetClientWrapper cedaClientWrapper) {
		LoginSvrClient.serverKey = serverKey;
		LoginSvrClient.cedaClientWrapper = cedaClientWrapper;
		IMessage<String> messageListener = new IMessage<String>(){

			@Override
			public void onMessage(String topic,String content) {
				try {
					logger.debug("LoginSvr onMessage: {},{}", topic,
							content);

					logger.debug("onMessage:{}",content);
					Map<String, Object> map = MsgHelper.Deserialize(content, HashMap.class, CharsetConvertType.None);
					String method = map.get("type") + "";
					ILoginMsgHandler handler = htMsgHandler.get(method);
					if (handler != null) {
						handler.process(map);
					}
					if("update".equals(method)){
						updateUserCache(map);
					}
				} catch (Exception e) {
				}
			}

		};
		cedaClientWrapper.init(serverKey, messageListener);
		cedaClientWrapper.subscribe(serverKey, "SYS.ATS.LOGIN", messageListener);
	}

	protected static void handleMap(Map<String, Object> map, int op) {
		UserInfo userInfo = UserInfo.parse(map);
		switch (op) {
		case 0:
			remove(userInfo.sid);
			break;
		case 1:
			add(userInfo);
			break;
		default:
			break;
		}
	}

	protected static void add(UserInfo userInfo) {
		if (userInfo.sid != null) {
			sid2Account.put(userInfo.sid, userInfo);
			name2Sid.put((userInfo.username+userInfo.clientType).hashCode(), userInfo.sid);
			logger.debug("LoginSvr client add: sid={}, username={}, name={}, mac={}, mac1={}, mac2={}", userInfo.sid,
					userInfo.username,userInfo.name,userInfo.mac,userInfo.mac1,userInfo.mac2);
		}
	}

	protected static void remove(String sid) {
		if (sid != null) {
			UserInfo userInfo = sid2Account.remove(sid);
			String account = null;
			if (userInfo != null) {
				name2Sid.remove((userInfo.username+userInfo.clientType).hashCode());
				account = userInfo.username;
			}
			logger.debug("LoginSvr client remove: sid={}, username={}", sid, account);
		}
	}

	public static UserInfo get(String sid) {
		UserInfo userInfo =null;
		if(TextUtils.isBlank(sid)) {
			return userInfo;
		}
		  userInfo = sid2Account.get(sid);
		if (userInfo != null) {
			return userInfo;
		} else {
			userInfo = request(sid, "",null);

			return userInfo;
		}
	}

	public static boolean isByName = true;

	public static UserInfo getByName(String accountName) {
//		String sid = name2Sid.get(accountName);
//		if (sid != null) {
//			return get(sid);
//		} else {
//			if (isByName) {
//				UserInfo userInfo = request("", accountName);
//				return userInfo;
//			}
//			return null;
//		}
		return getByNameAndClientType(accountName,"client");
	}
	public static UserInfo getByNameOnManage(String accountName) {
		return getByNameAndClientType(accountName,"manage");
	}
	public static UserInfo getByNameAndClientType(String accountName,String clientType) {
		logger.info("login hash:{}",(accountName+clientType).hashCode());
		String sid = name2Sid.get((accountName+clientType).hashCode());
		if (sid != null) {
			return get(sid);
		} else {
			if (isByName) {
				UserInfo userInfo = request("", accountName,clientType);
				return userInfo;
			}
			return null;
		}
	}

	private static UserInfo request(String sid, String username,String clientType) {
		if (cedaClientWrapper.getOnlineStatus().isOnline(serverKey)) {
			try {
				HashMap<String, Object> map = new HashMap<String, Object>();
				map.put("method", "userInfo");
				map.put("clientType", clientType);
				HashMap<String, String> args = new HashMap<String, String>();
				args.put("sid", sid);
				args.put("userName", username);
				map.put("parameter", args);
				Message message = new Message();
				message.setDestination(new Destination(ConstsMessage.TOPIC_ATS_LOGIN));
				MessageBody messageBody = new MessageBody();
				messageBody.addString((short) 3, MsgHelper.Serializer(map, CharsetConvertType.None));
				message.setMessageBody(messageBody);

				String content=MsgHelper.Serializer(map, CharsetConvertType.None);
				String reply = cedaClientWrapper.requestSync(serverKey, sid, ConstsMessage.TOPIC_ATS_LOGIN, content,requestTimeoutMillis);
				if (reply == null) {
					return null;
				}
				content = reply;
				logger.info("request UserInfo: {},{},{}", sid,username, content);
				Map<String, Object> resultMap = MsgHelper.Deserialize(content, HashMap.class, CharsetConvertType.None);
				UserInfo userInfo = UserInfo.parse(resultMap);
				if (userInfo != null)
					add(userInfo);
				return userInfo;

			} catch (Exception e) {
				logger.warn("LoginSvr client request: " + sid, e);
			}
		} else {
			logger.info("{} is not online", serverKey);
		}
		return null;
	}

	public static void clear() {
		sid2Account.clear();
	}

	public static void updateUserCache(Map<String, Object> map){
		UserInfo userInfo = UserInfo.parse(map);

		if (name2Sid.containsKey((userInfo.username+"client").hashCode())){
			userInfo.clientType="client";
			sid2Account.put(userInfo.sid,userInfo);
			name2Sid.put((userInfo.username+"client").hashCode(),userInfo.sid);
		}else if(name2Sid.containsKey((userInfo.username+"manage").hashCode())){
			userInfo.clientType="manage";
			sid2Account.put(userInfo.sid,userInfo);
			name2Sid.put((userInfo.username+"manage").hashCode(),userInfo.sid);

		}

	}
}
