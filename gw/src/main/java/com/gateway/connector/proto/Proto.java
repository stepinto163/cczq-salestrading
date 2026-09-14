package com.gateway.connector.proto;

import java.io.Serializable;
import java.util.Arrays;

/**
 * 协议类
 */
public class Proto implements Serializable {

    public static final short HEADER_LENGTH = 20;
    private int packetLen;
    private short headerLen;
    private int cmd;//1，2，3，
    private short format;//请求/响应，广播
    private int seq;//序号
    private String companyId;
    private String sessionId;
    private byte[] body;
	public int getPacketLen() {
		return packetLen;
	}
	public void setPacketLen(int packetLen) {
		this.packetLen = packetLen;
	}
	public short getHeaderLen() {
		return headerLen;
	}
	public void setHeaderLen(short headerLen) {
		this.headerLen = headerLen;
	}

	public int getCmd() {
		return cmd;
	}
	public void setCmd(int cmd) {
		this.cmd = cmd;
	}
	public short getFormat() {
		return format;
	}
	public void setFormat(short format) {
		this.format = format;
	}
	public int getSeq() {
		return seq;
	}
	public void setSeq(int seq) {
		this.seq = seq;
	}
	public String getSessionId() {
		return sessionId;
	}
	public void setSessionId(String sessionId) {
		this.sessionId = sessionId;
	}

	public String getCompanyId() {
		return companyId;
	}

	public void setCompanyId(String companyId) {
		this.companyId = companyId;
	}

	public byte[] getBody() {
		return body;
	}
	public void setBody(byte[] body) {
		this.body = body;
	}
	@Override
	public String toString() {
		return "Proto [packetLen=" + packetLen + ", headerLen=" + headerLen + ", cmd=" + cmd + ", format=" + format
				+ ", seq=" + seq + ", sessionId=" + sessionId + ", body=" + Arrays.toString(body) + "]";
	}
	
}
