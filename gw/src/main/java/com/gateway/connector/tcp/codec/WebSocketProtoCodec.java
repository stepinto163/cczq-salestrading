package com.gateway.connector.tcp.codec;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

import com.gateway.connector.proto.Proto;
import com.gateway.connector.utils.ProtoUtils;

import io.netty.buffer.ByteBuf;
import io.netty.buffer.Unpooled;
import io.netty.channel.ChannelHandler;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.MessageToMessageCodec;
import io.netty.handler.codec.http.websocketx.BinaryWebSocketFrame;
import io.netty.handler.codec.http.websocketx.WebSocketFrame;

/**
 * WebSocket 协议加解密
 */
@Component
@ChannelHandler.Sharable
public class WebSocketProtoCodec extends MessageToMessageCodec<WebSocketFrame, Proto> {
	public static final int INT_SIZE = Integer.BYTES;
    private Logger logger = LoggerFactory.getLogger(WebSocketProtoCodec.class);
    private boolean isGzip=false;
	public WebSocketProtoCodec(boolean isGzip) {
		this.isGzip=isGzip;
	}
    @Override
    protected void encode(ChannelHandlerContext ctx, Proto proto, List<Object> list) throws Exception {
    	proto.setBody(ProtoUtils.gzipBody(isGzip,proto.getBody()));
    	 int packageLenth = Proto.HEADER_LENGTH;
         byte[] sidByte = proto.getSessionId().getBytes();

         if (proto.getBody() != null)
             packageLenth = proto.getBody().length + packageLenth + sidByte.length;
         else
             packageLenth = packageLenth + sidByte.length;

         ByteBuf byteBuf = Unpooled.buffer(packageLenth);
         if (proto.getBody() != null) {
             byteBuf.writeInt(packageLenth);
             byteBuf.writeShort(Proto.HEADER_LENGTH);
             byteBuf.writeInt(proto.getCmd());
             byteBuf.writeShort(proto.getFormat());
             byteBuf.writeInt(proto.getSeq());
             byteBuf.writeInt(sidByte.length);
             if (sidByte.length != 0) {
                 byteBuf.writeBytes(sidByte);
             }
          
             byteBuf.writeBytes(proto.getBody());
         } else {
             byteBuf.writeInt(Proto.HEADER_LENGTH + sidByte.length);
             byteBuf.writeShort(Proto.HEADER_LENGTH);
             byteBuf.writeInt(proto.getCmd());
             byteBuf.writeShort(proto.getFormat());
             byteBuf.writeInt(proto.getSeq());
             byteBuf.writeInt(sidByte.length);
             if (sidByte.length != 0) {
                 byteBuf.writeBytes(sidByte);
             }
         }
      
       
        list.add(new BinaryWebSocketFrame(byteBuf));        
        logger.debug("encode: {}", proto);
    }

    @Override
    protected void decode(ChannelHandlerContext ctx, WebSocketFrame webSocketFrame, List<Object> list) throws Exception {
        ByteBuf byteBuf = webSocketFrame.content();
       
			Proto proto = parseFrom(byteBuf);
			if (proto != null) {
				list.add(proto);
				logger.debug("decode: {}", proto);
			} 
		 
    }
    public Proto parseFrom(ByteBuf byteBuf) {		 
		if (!byteBuf.isReadable(20))
			return null;

		int index = byteBuf.readerIndex();
		int len = byteBuf.readInt();

		if (len <20 || !byteBuf.isReadable(len - Integer.BYTES)) {	
			 
			byteBuf.readerIndex(index);
			return null;
		}

		Proto proto = new Proto();
		proto.setPacketLen(len);
		proto.setHeaderLen(byteBuf.readShort());
		proto.setCmd(byteBuf.readInt());
		proto.setFormat(byteBuf.readShort());
		proto.setSeq(byteBuf.readInt());
		int sidLength = byteBuf.readInt();
		if(sidLength>0) {
		byte[] sidByte = new byte[sidLength];
		try {
		byteBuf.readBytes(sidByte);
	 
		}
		catch(Exception e) {
			logger.warn("readBytes:{},readableBytes:{},PacketLen:{},HeaderLen:{},Cmd:{},Format:{},Seq:{}",sidLength,byteBuf.readableBytes(),proto.getPacketLen(),proto.getHeaderLen(),proto.getCmd(),proto.getFormat(),proto.getSeq(),e);
			byteBuf.readerIndex(index);
			return null;
		}
		proto.setSessionId(new String(sidByte));
		}
		int protSize = len - Proto.HEADER_LENGTH - sidLength;
		if (protSize > 0) {
			try {
			ByteBuf buffer = byteBuf.readSlice(protSize);

			byte[] req = new byte[protSize];
			buffer.readBytes(req);
			proto.setBody(ProtoUtils.ungzipBody(isGzip,req));
			}
			catch(Exception e) {
				logger.warn("readSlice:protSize:{},readableBytes:{}",protSize,byteBuf.readableBytes(),e);
				byteBuf.readerIndex(index);
				return null;
			}

		}
		return proto;
	}
}
