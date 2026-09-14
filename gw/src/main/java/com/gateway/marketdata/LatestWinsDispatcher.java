package com.gateway.marketdata;

import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.ThreadFactory;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class LatestWinsDispatcher<T> {

    public interface Handler<T> {
        HandleResult handle(T value) throws Exception;
    }

    public enum HandleResult {
        DONE,
        RETRY
    }

    private static final class Entry<T> {
        private final String key;
        private final Object lock = new Object();
        private volatile T value;
        private volatile String topicPrefix;
        private volatile String nodeId;
        private long version;
        private boolean queued;
        private boolean retryScheduled;

        private Entry(String key) {
            this.key = key;
        }
    }

    private final Logger logger;
    private final String name;
    private final String stage;
    private final String pendingType;
    private final int maxPendingKeys;
    private final Handler<T> handler;
    private final MarketDataBurstConfig burstConfig;
    private final ArrayBlockingQueue<Entry<T>> queue;
    private final ScheduledExecutorService retryScheduler;
    private final AtomicInteger pendingCount = new AtomicInteger();
    private final java.util.concurrent.ConcurrentHashMap<String, Entry<T>> entries = new java.util.concurrent.ConcurrentHashMap<String, Entry<T>>();

    public LatestWinsDispatcher(String name, String stage, String pendingType, int workerCount, int maxPendingKeys,
            MarketDataBurstConfig burstConfig, Handler<T> handler) {
        this.name = name;
        this.stage = stage;
        this.pendingType = pendingType;
        this.maxPendingKeys = Math.max(1, maxPendingKeys);
        this.handler = handler;
        this.burstConfig = burstConfig;
        this.logger = LoggerFactory.getLogger(LatestWinsDispatcher.class.getName() + "." + name);
        this.queue = new ArrayBlockingQueue<Entry<T>>(this.maxPendingKeys);
        this.retryScheduler = Executors.newSingleThreadScheduledExecutor(new NamedThreadFactory(name + "-retry"));
        int actualWorkerCount = Math.max(1, workerCount);
        for (int i = 0; i < actualWorkerCount; i++) {
            Thread worker = new Thread(new Runnable() {
                @Override
                public void run() {
                    runWorker();
                }
            }, name + "-" + i);
            worker.setDaemon(true);
            worker.start();
        }
    }

    public boolean submit(String key, T value, String topicPrefix, String nodeId) {
        if (StringUtils.isBlank(key) || value == null) {
            return false;
        }
        Entry<T> entry;
        boolean shouldEnqueue = false;
        while (true) {
            entry = entries.get(key);
            if (entry == null) {
                Entry<T> newEntry = new Entry<T>(key);
                Entry<T> existing = entries.putIfAbsent(key, newEntry);
                entry = existing != null ? existing : newEntry;
            }
            synchronized (entry.lock) {
                // The worker may remove an idle entry while this submitter is
                // waiting for its lock. Retry against the current map entry.
                if (entries.get(key) != entry) {
                    continue;
                }
                entry.value = value;
                entry.topicPrefix = topicPrefix;
                entry.nodeId = nodeId;
                entry.version++;
                if (entry.queued) {
                    burstConfig.recordCoalesce(stage, topicPrefix, nodeId);
                    return true;
                }
                if (pendingCount.incrementAndGet() > maxPendingKeys) {
                    pendingCount.decrementAndGet();
                    entry.value = null;
                    entries.remove(key, entry);
                    burstConfig.recordDrop(stage, topicPrefix, nodeId, "max_pending");
                    return false;
                }
                entry.queued = true;
                shouldEnqueue = true;
            }
            break;
        }
        if (!shouldEnqueue) {
            return true;
        }
        if (!queue.offer(entry)) {
            synchronized (entry.lock) {
                entry.queued = false;
                entry.value = null;
                entries.remove(key, entry);
            }
            pendingCount.decrementAndGet();
            updatePending();
            burstConfig.recordDrop(stage, topicPrefix, nodeId, "queue_full");
            return false;
        }
        updatePending();
        return true;
    }

    public int pendingSize() {
        return pendingCount.get();
    }

    public int trackedKeyCount() {
        return entries.size();
    }

    private void runWorker() {
        while (true) {
            try {
                Entry<T> entry = queue.take();
                process(entry);
            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
                return;
            } catch (Exception e) {
                logger.warn("{} worker loop error", name, e);
            }
        }
    }

    private void process(Entry<T> entry) {
        while (true) {
            T value;
            long version;
            String topicPrefix;
            String nodeId;
            synchronized (entry.lock) {
                value = entry.value;
                version = entry.version;
                topicPrefix = entry.topicPrefix;
                nodeId = entry.nodeId;
                entry.retryScheduled = false;
            }
            HandleResult result = HandleResult.DONE;
            try {
                result = handler.handle(value);
            } catch (Exception e) {
                logger.warn("{} handler error key={}", name, entry.key, e);
            }
            if (HandleResult.RETRY.equals(result)) {
                scheduleRetry(entry, topicPrefix, nodeId);
                return;
            }
            synchronized (entry.lock) {
                if (entry.version != version) {
                    continue;
                }
                entry.queued = false;
                entry.value = null;
                pendingCount.decrementAndGet();
                entries.remove(entry.key, entry);
            }
            updatePending();
            return;
        }
    }

    private void scheduleRetry(final Entry<T> entry, final String topicPrefix, final String nodeId) {
        boolean shouldSchedule = false;
        synchronized (entry.lock) {
            if (entry.queued && !entry.retryScheduled) {
                entry.retryScheduled = true;
                shouldSchedule = true;
            }
        }
        if (!shouldSchedule) {
            return;
        }
        retryScheduler.schedule(new Runnable() {
            @Override
            public void run() {
                synchronized (entry.lock) {
                    entry.retryScheduled = false;
                    if (!entry.queued) {
                        return;
                    }
                }
                if (!queue.offer(entry)) {
                    burstConfig.recordDrop(stage, topicPrefix, nodeId, "retry_queue_full");
                    scheduleRetry(entry, topicPrefix, nodeId);
                    return;
                }
            }
        }, Math.max(1L, burstConfig.getRetryDelayMillis()), TimeUnit.MILLISECONDS);
    }

    private void updatePending() {
        if ("ingress".equalsIgnoreCase(pendingType)) {
            burstConfig.updateIngressPendingKeys(pendingCount.get());
        } else {
            burstConfig.updateEgressPendingKeys(pendingCount.get());
        }
    }

    private static final class NamedThreadFactory implements ThreadFactory {
        private final AtomicInteger counter = new AtomicInteger();
        private final String prefix;

        private NamedThreadFactory(String prefix) {
            this.prefix = prefix;
        }

        @Override
        public Thread newThread(Runnable r) {
            Thread thread = new Thread(r, prefix + "-" + counter.getAndIncrement());
            thread.setDaemon(true);
            return thread;
        }
    }
}
