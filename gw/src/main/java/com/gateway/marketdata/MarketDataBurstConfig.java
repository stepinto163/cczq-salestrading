package com.gateway.marketdata;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map.Entry;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicLong;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class MarketDataBurstConfig {

    private static final Logger logger = LoggerFactory.getLogger(MarketDataBurstConfig.class);

    private static final String UNKNOWN_TOPIC_PREFIX = "unknown";
    private static final String UNKNOWN_NODE_ID = "*";

    private String topicPrefixes = "stc.md.";
    private volatile List<String> topicPrefixList = parseTopicPrefixes(topicPrefixes);

    private int ingressMaxPendingKeys = 20000;
    private int egressMaxPendingKeys = 20000;
    private int notifyCacheWorkerCount = 8;
    private int notifyCacheMaxSize = 600000;
    private boolean rawNotifyEnabled = false;
    private boolean dropWhenNotWritable = true;
    private int outboundSoftLimit = 1000;
    private int warnPendingThreshold = 1000;
    private boolean logNormalSummary = false;
    private long retryDelayMillis = 10L;
    private long summaryIntervalMillis = 5000L;

    private final AtomicInteger ingressPendingKeys = new AtomicInteger();
    private final AtomicInteger egressPendingKeys = new AtomicInteger();
    private final AtomicLong coalesceOverwriteCount = new AtomicLong();
    private final AtomicLong dropCount = new AtomicLong();
    private final AtomicLong nonWritableSessionCount = new AtomicLong();
    private final AtomicLong rawNotifyInterceptCount = new AtomicLong();
    private final AtomicLong rawDirectSendCount = new AtomicLong();
    private final AtomicLong lazyDecodeCount = new AtomicLong();
    private final AtomicLong lastSummaryAt = new AtomicLong();
    private final ConcurrentHashMap<String, AtomicLong> dimensionCounts = new ConcurrentHashMap<String, AtomicLong>();

    public String getTopicPrefixes() {
        return topicPrefixes;
    }

    public void setTopicPrefixes(String topicPrefixes) {
        this.topicPrefixes = topicPrefixes;
        this.topicPrefixList = parseTopicPrefixes(topicPrefixes);
    }

    public int getIngressMaxPendingKeys() {
        return ingressMaxPendingKeys;
    }

    public void setIngressMaxPendingKeys(int ingressMaxPendingKeys) {
        this.ingressMaxPendingKeys = ingressMaxPendingKeys;
    }

    public int getEgressMaxPendingKeys() {
        return egressMaxPendingKeys;
    }

    public void setEgressMaxPendingKeys(int egressMaxPendingKeys) {
        this.egressMaxPendingKeys = egressMaxPendingKeys;
    }

    public int getNotifyCacheWorkerCount() {
        return notifyCacheWorkerCount;
    }

    public void setNotifyCacheWorkerCount(int notifyCacheWorkerCount) {
        this.notifyCacheWorkerCount = notifyCacheWorkerCount;
    }

    public int getNotifyCacheMaxSize() {
        return notifyCacheMaxSize;
    }

    public void setNotifyCacheMaxSize(int notifyCacheMaxSize) {
        this.notifyCacheMaxSize = notifyCacheMaxSize;
    }

    public boolean isRawNotifyEnabled() {
        return rawNotifyEnabled;
    }

    public void setRawNotifyEnabled(boolean rawNotifyEnabled) {
        this.rawNotifyEnabled = rawNotifyEnabled;
    }

    public boolean isDropWhenNotWritable() {
        return dropWhenNotWritable;
    }

    public void setDropWhenNotWritable(boolean dropWhenNotWritable) {
        this.dropWhenNotWritable = dropWhenNotWritable;
    }

    public int getOutboundSoftLimit() {
        return outboundSoftLimit;
    }

    public void setOutboundSoftLimit(int outboundSoftLimit) {
        this.outboundSoftLimit = outboundSoftLimit;
    }

    public int getWarnPendingThreshold() {
        return warnPendingThreshold;
    }

    public void setWarnPendingThreshold(int warnPendingThreshold) {
        this.warnPendingThreshold = warnPendingThreshold;
    }

    public boolean isLogNormalSummary() {
        return logNormalSummary;
    }

    public void setLogNormalSummary(boolean logNormalSummary) {
        this.logNormalSummary = logNormalSummary;
    }

    public long getRetryDelayMillis() {
        return retryDelayMillis;
    }

    public void setRetryDelayMillis(long retryDelayMillis) {
        this.retryDelayMillis = retryDelayMillis;
    }

    public long getSummaryIntervalMillis() {
        return summaryIntervalMillis;
    }

    public void setSummaryIntervalMillis(long summaryIntervalMillis) {
        this.summaryIntervalMillis = summaryIntervalMillis;
    }

    public boolean isMarketDataTopic(String topic) {
        return resolveTopicPrefix(topic) != null;
    }

    public String resolveTopicPrefix(String topic) {
        if (StringUtils.isBlank(topic)) {
            return null;
        }
        for (String prefix : topicPrefixList) {
            if (topic.startsWith(prefix)) {
                return prefix;
            }
        }
        return null;
    }

    public void updateIngressPendingKeys(int pendingKeys) {
        ingressPendingKeys.set(Math.max(pendingKeys, 0));
        logSummaryIfDue();
    }

    public void updateEgressPendingKeys(int pendingKeys) {
        egressPendingKeys.set(Math.max(pendingKeys, 0));
        logSummaryIfDue();
    }

    public void recordCoalesce(String stage, String topicPrefix, String nodeId) {
        coalesceOverwriteCount.incrementAndGet();
        recordDimension("coalesce", stage, topicPrefix, nodeId);
    }

    public void recordDrop(String stage, String topicPrefix, String nodeId, String reason) {
        dropCount.incrementAndGet();
        recordDimension("drop:" + StringUtils.defaultIfBlank(reason, "unknown"), stage, topicPrefix, nodeId);
    }

    public void recordNonWritable(String stage, String topicPrefix, String nodeId) {
        nonWritableSessionCount.incrementAndGet();
        recordDimension("nonWritable", stage, topicPrefix, nodeId);
    }

    public void recordRawNotifyIntercept(String topicPrefix, String nodeId) {
        rawNotifyInterceptCount.incrementAndGet();
        recordDimension("rawIntercept", "ingress", topicPrefix, nodeId);
    }

    public void recordRawDirectSend(String topicPrefix, String nodeId) {
        rawDirectSendCount.incrementAndGet();
        recordDimension("rawDirectSend", "egress", topicPrefix, nodeId);
    }

    public void recordLazyDecode(String topicPrefix, String nodeId) {
        lazyDecodeCount.incrementAndGet();
        recordDimension("lazyDecode", "filter", topicPrefix, nodeId);
    }

    public void logSummaryIfDue() {
        long now = System.currentTimeMillis();
        long previous = lastSummaryAt.get();
        if (now - previous < summaryIntervalMillis) {
            return;
        }
        if (!lastSummaryAt.compareAndSet(previous, now)) {
            return;
        }
        int ingressPending = ingressPendingKeys.get();
        int egressPending = egressPendingKeys.get();
        long coalesceOverwrite = coalesceOverwriteCount.getAndSet(0L);
        long drop = dropCount.getAndSet(0L);
        long nonWritable = nonWritableSessionCount.getAndSet(0L);
        long rawIntercept = rawNotifyInterceptCount.getAndSet(0L);
        long rawDirectSend = rawDirectSendCount.getAndSet(0L);
        long lazyDecode = lazyDecodeCount.getAndSet(0L);
        boolean hasActivity = ingressPending > 0 || egressPending > 0 || coalesceOverwrite > 0L || drop > 0L
                || nonWritable > 0L || rawIntercept > 0L || rawDirectSend > 0L
                || lazyDecode > 0L;
        boolean abnormal = drop > 0L || nonWritable > 0L
                || ingressPending >= warnPendingThreshold || egressPending >= warnPendingThreshold;
        if (abnormal) {
            logger.warn(
                    "marketDataBurstSummary ingressPendingKeys:{} egressPendingKeys:{} coalesceOverwriteCount:{} dropCount:{} nonWritableSessionCount:{} rawNotifyInterceptCount:{} rawDirectSendCount:{} lazyDecodeCount:{}",
                    ingressPending, egressPending, coalesceOverwrite, drop, nonWritable, rawIntercept,
                    rawDirectSend, lazyDecode);
        } else if (logNormalSummary && hasActivity) {
            logger.info(
                    "marketDataBurstSummary ingressPendingKeys:{} egressPendingKeys:{} coalesceOverwriteCount:{} dropCount:{} nonWritableSessionCount:{} rawNotifyInterceptCount:{} rawDirectSendCount:{} lazyDecodeCount:{}",
                    ingressPending, egressPending, coalesceOverwrite, drop, nonWritable, rawIntercept,
                    rawDirectSend, lazyDecode);
        }
        for (Entry<String, AtomicLong> entry : dimensionCounts.entrySet()) {
            long count = entry.getValue().getAndSet(0L);
            if (count <= 0L) {
                continue;
            }
            String[] tokens = entry.getKey().split("\\|", 4);
            if (tokens.length != 4) {
                continue;
            }
            if (!abnormal && !logNormalSummary) {
                continue;
            }
            if (abnormal && !shouldLogAbnormalDimension(tokens[0])) {
                continue;
            }
            if (abnormal) {
                logger.warn("marketDataBurstDimensions metric:{} stage:{} topicPrefix:{} nodeId:{} count:{}",
                        tokens[0], tokens[1], tokens[2], tokens[3], count);
            } else {
                logger.info("marketDataBurstDimensions metric:{} stage:{} topicPrefix:{} nodeId:{} count:{}",
                        tokens[0], tokens[1], tokens[2], tokens[3], count);
            }
        }
    }

    private boolean shouldLogAbnormalDimension(String metric) {
        return "nonWritable".equals(metric) || StringUtils.startsWith(metric, "drop:");
    }

    private void recordDimension(String metric, String stage, String topicPrefix, String nodeId) {
        String dimensionKey = buildDimensionKey(metric, stage, topicPrefix, nodeId);
        AtomicLong counter = dimensionCounts.get(dimensionKey);
        if (counter == null) {
            AtomicLong newCounter = new AtomicLong();
            AtomicLong existing = dimensionCounts.putIfAbsent(dimensionKey, newCounter);
            counter = existing != null ? existing : newCounter;
        }
        counter.incrementAndGet();
        logSummaryIfDue();
    }

    private String buildDimensionKey(String metric, String stage, String topicPrefix, String nodeId) {
        String safeStage = StringUtils.defaultIfBlank(stage, "unknown");
        String safeTopicPrefix = StringUtils.defaultIfBlank(topicPrefix, UNKNOWN_TOPIC_PREFIX);
        String safeNodeId = StringUtils.defaultIfBlank(nodeId, UNKNOWN_NODE_ID);
        return metric + "|" + safeStage + "|" + safeTopicPrefix + "|" + safeNodeId;
    }

    private List<String> parseTopicPrefixes(String topicPrefixes) {
        if (StringUtils.isBlank(topicPrefixes)) {
            return Collections.emptyList();
        }
        List<String> list = new ArrayList<String>();
        String[] prefixes = topicPrefixes.split(",");
        for (String prefix : prefixes) {
            String trimmed = StringUtils.trimToEmpty(prefix);
            if (StringUtils.isNotBlank(trimmed)) {
                list.add(trimmed);
            }
        }
        return Collections.unmodifiableList(list);
    }
}
