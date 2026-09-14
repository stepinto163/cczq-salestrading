package com.gateway.invoke.security;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArraySet;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.gateway.connector.Session;
import com.gateway.connector.proto.Proto;
import com.gateway.connector.tcp.TcpSessionManager;
import com.gateway.connector.utils.TopicUtils;
import com.gateway.message.SystemMessage;

import sc.app.stc.encrypt.RSA;

/**
 * 黑名单检查
 *
 * @author enhao.dong
 */
public class BlackListSecurityCheck implements ISecurityCheck {
    private final static Logger logger = LoggerFactory.getLogger(BlackListSecurityCheck.class);
    private static final String privateKey = "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBAK9gCvTz3ekApk9PzqCqHpk" +
            "bp11e4yNklhwsuFsjFP1fvdV2P+senDNdYYtF8sG+RSSdpigjvwWTukl8gkdSPQC5bPCTmoZKxRIFhQ0zlW+bJFaEoDpYkqfxtQHEoGR" +
            "AxvGPZZSKRN1azQKEZfyJbPd9bsfAMBa730mPsT6G6+HrAgMBAAECgYBm43DqLtyFI3Vu2IKALe5PozK4i7+0dwG+O9pY1act02q6/mo" +
            "XldjMJb/hhsMaOXe1OKxPplEeg4HqNRr1nuDcPi3oFFrU6lCY7bPTMnA7TbFqDAkCVKA71cpaBoxTdfwIxLpoKzrvkzLI+SrDmYmM9A8" +
            "v2WFR9smJ70/lLbm60QJBAPF10K1q5bNnV9OLK5QxqmFpmQiOZuRllrt1c3ZDyb53h188IrrefV5ejiKZWn8RzVM3YYPNqV0hNaE3GDP" +
            "3nvMCQQC574GE/1BW4NoKDHcHu9VFrsipJSOPiZDtxCQphsGhX7qdHybXcTjvW+x9NMSwBF2TQuzd8aD9e7YISQyQ7B8pAkEAtNWhdV9" +
            "LrHLHKd1ew9v2WII3ZFsR4BfdLYOm93mJTCZEsiRddkpO5jA2jt7/EY+JkLs3Qvjx0JegjJ6wd6sJ1QJAXN34JdJTcSEbJhu1s+IoEob" +
            "K6FiJW17NSURG/4qz2YLztRhjEQYW9s5F3v9lIbj8d9ncbroJu7F3aMn7gcK9iQJBAIhu/WIBckFJlmY3RFPhUlQ9A1VNTfsu9DxKUEh" +
            "09VriYZKDBFwCgN4j9vWLo7+GlUtVkkRo2sKWEKCDNhNqW/s=";
    private static final String BlackListCheck = "10004";
    private static final String BlackListCheckMsg = "禁止访问";
    private String blackListPath;
    private TcpSessionManager tcpSessionManager;

    public String getBlackListPath() {
        return blackListPath;
    }

    public void setBlackListPath(String blackListPath) {
        this.blackListPath = blackListPath;
    }

    public void setTcpSessionManager(TcpSessionManager tcpSessionManager) {
        this.tcpSessionManager = tcpSessionManager;
    }

    private final ConcurrentHashMap<String, CopyOnWriteArraySet<String>> blackListHm = new ConcurrentHashMap<>();

    public void init() throws Exception {
        ArrayList<String> tmp = readAll(blackListPath);
        blackListHm.clear();
        for (String str : tmp) {
            String[] strs = str.split(",");
            if (strs.length > 1) {
                String serverName = strs[0];
                String topic = strs[1];
                CopyOnWriteArraySet<String> cas = blackListHm.get(serverName);
                if (cas == null) {
                    cas = new CopyOnWriteArraySet<>();
                    blackListHm.put(serverName, cas);
                }
                cas.add(topic);
            }
        }

    }

    @Override
    public SecurityResult check(SystemMessage sMsg, Proto message, String serverName, String method, String content) {
        SecurityResult pr = new SecurityResult();
        String clientType = null;
        logger.info("serverName: {}, method: {}", serverName, method);
        if ("LoginSvr".equals(serverName)) {
            return pr;
        }
        if (StringUtils.isNoneBlank(message.getSessionId())) {
            Session session = tcpSessionManager.getSession(message.getSessionId());
            if (session != null) {
                clientType = session.getClientType();
            }
        }
        if ("client".equals(clientType)) {
            logger.info("clientType: {}", clientType);
            return pr;
        }
        CopyOnWriteArraySet<String> cas = blackListHm.get(serverName.toLowerCase());
        if (cas != null) {
            if (method.equals("subscribe") || method.equals("unsubscribe")) {
                for (String stopic : cas) {
                    if (TopicUtils.matchTopic(stopic, content.toLowerCase())) {
                        pr.Code = BlackListCheck;
                        pr.Msg = BlackListCheckMsg;
                        return pr;
                    }
                }
            } else if (cas.contains(method.toLowerCase())) {
                pr.Code = BlackListCheck;
                pr.Msg = BlackListCheckMsg;
            }
        }
        return pr;
    }

    public static ArrayList<String> readAll(String fileName) throws Exception {
        ArrayList<String> list = new ArrayList<>();

        try (InputStreamReader fileReader = new InputStreamReader(Files.newInputStream(Paths.get(fileName)), StandardCharsets.UTF_8);
             BufferedReader reader = new BufferedReader(fileReader)) {

            String line;
            while ((line = reader.readLine()) != null) {

                if (line.length() > 0) {
                    logger.info("line: {}", line);
                    list.add(RSA.decrypt(line, privateKey).toLowerCase());
                }
            }
        }
        logger.info("black list: {}", list);
        return list;
    }

    public static void main(String[] args) throws Exception {
//        Map<String, String> keyPair = RSA.generateKeyPair();
//        System.out.println(keyPair);
//        String privateKey = "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBAK9gCvTz3ekApk9PzqCqHpkbp11e4yNklhwsuFsjFP1fvdV2P+senDNdYYtF8sG+RSSdpigjvwWTukl8gkdSPQC5bPCTmoZKxRIFhQ0zlW+bJFaEoDpYkqfxtQHEoGRAxvGPZZSKRN1azQKEZfyJbPd9bsfAMBa730mPsT6G6+HrAgMBAAECgYBm43DqLtyFI3Vu2IKALe5PozK4i7+0dwG+O9pY1act02q6/moXldjMJb/hhsMaOXe1OKxPplEeg4HqNRr1nuDcPi3oFFrU6lCY7bPTMnA7TbFqDAkCVKA71cpaBoxTdfwIxLpoKzrvkzLI+SrDmYmM9A8v2WFR9smJ70/lLbm60QJBAPF10K1q5bNnV9OLK5QxqmFpmQiOZuRllrt1c3ZDyb53h188IrrefV5ejiKZWn8RzVM3YYPNqV0hNaE3GDP3nvMCQQC574GE/1BW4NoKDHcHu9VFrsipJSOPiZDtxCQphsGhX7qdHybXcTjvW+x9NMSwBF2TQuzd8aD9e7YISQyQ7B8pAkEAtNWhdV9LrHLHKd1ew9v2WII3ZFsR4BfdLYOm93mJTCZEsiRddkpO5jA2jt7/EY+JkLs3Qvjx0JegjJ6wd6sJ1QJAXN34JdJTcSEbJhu1s+IoEobK6FiJW17NSURG/4qz2YLztRhjEQYW9s5F3v9lIbj8d9ncbroJu7F3aMn7gcK9iQJBAIhu/WIBckFJlmY3RFPhUlQ9A1VNTfsu9DxKUEh09VriYZKDBFwCgN4j9vWLo7+GlUtVkkRo2sKWEKCDNhNqW/s=";
        String publicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCvYAr0893pAKZPT86gqh6ZG6ddXuMjZJYcLLhbIxT9X73Vdj/rHpwzXWGLRfLBvkUknaYoI78Fk7pJfIJHUj0AuWzwk5qGSsUSBYUNM5VvmyRWhKA6WJKn8bUBxKBkQMbxj2WUikTdWs0ChGX8iWz3fW7HwDAWu99Jj7E+huvh6wIDAQAB, modulus=AK9gCvTz3ekApk9PzqCqHpkbp11e4yNklhwsuFsjFP1fvdV2P+senDNdYYtF8sG+RSSdpigjvwWTukl8gkdSPQC5bPCTmoZKxRIFhQ0zlW+bJFaEoDpYkqfxtQHEoGRAxvGPZZSKRN1azQKEZfyJbPd9bsfAMBa730mPsT6G6+Hr";
        String originStr = "ParserSvr,stc.parser.bond";
//        String originStr = "CalSvr,stc.broker.calbond";
        System.out.println("originStr: " + originStr);
//        String encryptStr = RSA.encrypt(originStr, publicKey);
        String encryptStr = "WqeUeCdl7hnAzjkOsuqS6OgRZhtoR7/r5bpbNDMJdWL+ScRVseGrlE6r2tsRzdoBumLOB9oeFTVgkiy1szKGzRThXSLfEMxoI2BgB6QLDMFr3ibXmkvPphiu3WC+cS6EfK9GGPVcnDlDcb+lSFGDP7leurXpqTXJ5nyjN4unxqY=";
        System.out.println("encryptStr: " + encryptStr);
        String decryptStr = RSA.decrypt(encryptStr, privateKey);
        System.out.println("decryptStr: " + decryptStr);
    }
}
