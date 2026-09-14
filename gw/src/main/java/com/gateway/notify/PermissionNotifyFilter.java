package com.gateway.notify;

import com.alibaba.fastjson.JSON;
import com.app.common.utils.StringUtil;
import com.gateway.connector.Session;
import com.gateway.connector.tcp.TcpSessionManager;
import sc.app.stc.fix.message.ExecutionReport;
import sc.app.stc.fix.message.MarketDataSnapshotFullRefresh;

import java.util.HashSet;
import java.util.Set;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;

/**
 * 权限过滤器
 */
public class PermissionNotifyFilter extends NotifyFilter {

    private TcpSessionManager tcpSessionManager;

    public PermissionNotifyFilter(TcpSessionManager tcpSessionManager) {
        this.tcpSessionManager = tcpSessionManager;
    }

    @Override
    public NotifyProxy.NotifyMsg doFilter(NotifyProxy.NotifyMsg msg) {
        String sid = msg.sid;
        if (sid == null) {
            return next != null ? next.doFilter(msg) : msg;
        }

        Session session = tcpSessionManager.getSession(sid);
        if (session == null) {
            return null;
        }
        String info = session.getInfo();
        if (StringUtil.isBlank(info)) {
            return null;
        }

        Set<String> permissions = new HashSet<>();
        for (String permission : info.split("\\|")) {
            permissions.add(permission.trim());
        }

        if (msg.topic.startsWith("stc.md")) {
            MarketDataSnapshotFullRefresh marketData = JSON.parseObject(msg.getContent(), MarketDataSnapshotFullRefresh.class);
            if (marketData.getNoMDEntries() != null) {
                int oldSize = marketData.getNoMDEntries().size();
                CopyOnWriteArrayList<MarketDataSnapshotFullRefresh.GNoMDEntries> filteredEntries =
                        filterMDEntries(marketData.getNoMDEntries(), permissions);
                if (filteredEntries.size() != oldSize) {
                    marketData.setNoMDEntries(filteredEntries);
                    msg.setContent(JSON.toJSONString(marketData));
                }
            }
        } else if (msg.topic.startsWith("stc.ems.c")) {
            ExecutionReport executionReport = JSON.parseObject(msg.getContent(), ExecutionReport.class);
            if (!permissions.contains(executionReport.getVenues())) {
                return null;
            }
        }

        return next != null ? next.doFilter(msg) : msg;
    }

    private CopyOnWriteArrayList<MarketDataSnapshotFullRefresh.GNoMDEntries> filterMDEntries(List<MarketDataSnapshotFullRefresh.GNoMDEntries> entries, Set<String> permissions) {
        CopyOnWriteArrayList<MarketDataSnapshotFullRefresh.GNoMDEntries> filteredEntries = new CopyOnWriteArrayList<>();
        for (MarketDataSnapshotFullRefresh.GNoMDEntries entry : entries) {
            if (permissions.contains(entry.getVenues())) {
                filteredEntries.add(entry);
            }
        }
        return filteredEntries;
    }
}
