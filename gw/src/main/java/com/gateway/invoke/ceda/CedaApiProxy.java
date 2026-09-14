package com.gateway.invoke.ceda;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.TypeReference;
import com.gateway.connector.Session;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.TcpSessionManager;
import com.gateway.message.MessageWrapper;
import com.gateway.message.SystemMessage;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.BufferedReader;
import java.io.FileReader;
import java.util.HashMap;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.locks.ReentrantLock;

/**
 * Ceda API代理
 */
public class CedaApiProxy extends CedaProxy {
    private static final Logger logger = LoggerFactory.getLogger(CedaApiProxy.class);
    private String userListFilePath;
    private TcpSessionManager tcpSessionManager;
    private final ConcurrentHashMap<String, User> userMap = new ConcurrentHashMap<>();
    private static final ReentrantLock lock = new ReentrantLock();

    public void setUserListFilePath(String userListFilePath) {
        this.userListFilePath = userListFilePath;
    }

    public void setTcpSessionManager(TcpSessionManager tcpSessionManager) {
        this.tcpSessionManager = tcpSessionManager;
    }

    @Override
    public void init() {
        super.init();
        loadUserList();
    }

    public void loadUserList() {
        lock.lock();
        try (BufferedReader br = new BufferedReader(new FileReader(userListFilePath))) {
            String line;
            while ((line = br.readLine()) != null) {
                String[] parts = line.split(",(?=([^\"]*\"[^\"]*\")*[^\"]*$)");
                if (parts.length >= 2) {
                    String username = parts[0];
                    String password = parts[1];
                    String info = parts.length > 2 ? parts[2] : "";
                    userMap.put(username, new User(username, password, info));
                }
            }
            for (Session session : tcpSessionManager.getSessions()) {
                User user = userMap.get(session.getUserName());
                String info = session.getInfo();
                if (!user.getInfo().equals(info)) {
                    session.setInfo(user.getInfo());
                    tcpSessionManager.addSession(session);
                }
            }
        } catch (Exception e) {
            logger.error("Error loading user list", e);
        } finally {
            lock.unlock();
        }
    }

    @Override
    public MessageWrapper processLogin(SystemMessage sMsg, Proto message, String content) {
        try {
            HashMap<String, Object> hm = JSON.parseObject(content, new TypeReference<HashMap<String, Object>>() {
            });
            String username = (String) hm.get("userName");
            String password = (String) hm.get("pwd");

            User user = userMap.get(username);
            if (user != null && user.getPassword().equals(password)) {
                String sessionId = UUID.randomUUID().toString();
                message.setSessionId(sessionId);
                message.setBody("Login successful".getBytes());
                MessageWrapper mr = new MessageWrapper(MessageWrapper.MessageProtocol.CONNECT, sessionId, message);
                mr.setUserName(username);
                mr.setExtra(user.getInfo());
                return mr;
            } else {
                message.setBody("Login failed".getBytes());
                return new MessageWrapper(MessageWrapper.MessageProtocol.NO_CONNECT, "", message);
            }
        } catch (Exception e) {
            logger.error("Error processing login", e);
            throw e;
        }
    }

    private static class User {
        private final String username;
        private final String password;
        private final String info;

        public User(String username, String password, String info) {
            this.username = username;
            this.password = password;
            this.info = info;
        }

        public String getUsername() {
            return username;
        }

        public String getPassword() {
            return password;
        }

        public String getInfo() {
            return info;
        }
    }
}