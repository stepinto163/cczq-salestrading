package com.gateway.invoke.ceda;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.app.common.thread.AsyncSeqThreadGroup;
import com.gateway.connector.proto.Format;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.TcpConnector;

import ats.app.api.BaseApi.IMessage;
import ats.app.ym.ex.utils.StringUtils;

public class RspCallBack implements IMessage<String> {
	private static Logger logger = LoggerFactory.getLogger(RspCallBack.class);
	private Proto message;
	private String serverName;
	private TcpConnector tcpConnector;
	private String topic;
	private static AsynRspCallBack asynRspCallBack = new AsynRspCallBack(
			Runtime.getRuntime().availableProcessors() * 2);

	static class AsynRspCallBack extends AsyncSeqThreadGroup<Object[]> {

		public AsynRspCallBack(int asyncBaseNum) {
			super(asyncBaseNum);
			// TODO Auto-generated constructor stub
		}

		@Override
		protected void process(Object[] t) {
			String content = (String) t[1];
			try {
				String sid = (String) t[0];

				Proto message = (Proto) t[2];
				TcpConnector tcpConnector = (TcpConnector) t[3];
				String serverName = (String) t[4];
				String topic = (String) t[5];
				if (!StringUtils.isEmpty(sid) && !StringUtils.isEmpty(content)) {
					logger.info("response->severName:{},{},content:{}[{}]", serverName, topic, content,message.getSeq());
					message.setBody(content.getBytes());
					message.setFormat(Format.REPLY);
					if (tcpConnector.exist(sid)) {
						String userName=tcpConnector.send(sid, message);
						logger.debug("{},{},{},{}", sid, userName,topic, content);

					}
				}
			} catch (Exception e) {
				logger.warn("send {} failed", content, e);
			}
		}

	}

	public RspCallBack(Proto proto, String serverName, String topic, TcpConnector tcpConnector) {
		this.message = proto;
		this.serverName = serverName;
		this.tcpConnector = tcpConnector;
		this.topic = topic;
	}

	@Override
	public void onMessage(String sid, String content) {
		asynRspCallBack.add(sid+"", new Object[] { sid, content, message, tcpConnector, serverName, topic });

	}

}
