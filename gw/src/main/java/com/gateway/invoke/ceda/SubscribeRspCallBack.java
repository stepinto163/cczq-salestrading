package com.gateway.invoke.ceda;

import java.util.HashMap;
import java.util.Map.Entry;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
 
import com.app.common.thread.AsyncSeqThreadGroup;
import com.gateway.connector.utils.JsonUtils;
import com.gateway.notify.NotifyProxy;

import ats.app.api.BaseApi.IMessage;
import ats.app.ym.ex.utils.StringUtils;

public class SubscribeRspCallBack implements IMessage<String> {
	private static Logger logger = LoggerFactory.getLogger(SubscribeRspCallBack.class);

	private NotifyProxy notifyProxy;
	private static AsynSubscribeRspCallBack asynSubscribeRspCallBack = new AsynSubscribeRspCallBack(
			Runtime.getRuntime().availableProcessors() * 2);

	static class AsynSubscribeRspCallBack extends AsyncSeqThreadGroup<Object[]> {

		public AsynSubscribeRspCallBack(int asyncBaseNum) {
			super(asyncBaseNum);
		}

		@SuppressWarnings("unchecked")
		@Override
		protected void process(Object[] t) {
			String body = (String) t[1];

			try {
				String sid = (String) t[0];
				NotifyProxy notifyProxy = (NotifyProxy) t[2];
				if (!StringUtils.isEmpty(sid)) {
					if (!StringUtils.isEmpty(body)) {
						HashMap<String, Object> rhm = JsonUtils.Deserialize(body, HashMap.class);

						rhm.remove("SRL");
						for (Entry<String, Object> entry : rhm.entrySet()) {
							String topic = entry.getKey();
							String content = entry.getValue() + "";
							if (notifyProxy != null) {
								notifyProxy.notify(sid, topic, content, null);
							}
						}
					}
				}
			} catch (Exception e) {
				logger.warn("send {} failed", body, e);
			}
		}

	}

	public SubscribeRspCallBack(NotifyProxy notifyProxy) {

		this.notifyProxy = notifyProxy;

	}

	@Override
	public void onMessage(String sid, String body) {
		asynSubscribeRspCallBack.add(sid+"", new Object[] { sid, body, notifyProxy });
	}

}
