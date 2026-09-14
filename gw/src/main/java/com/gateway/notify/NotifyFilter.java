package com.gateway.notify;

/**
 * 通知过滤器
 */
public abstract class NotifyFilter {
    protected NotifyFilter next;

    public void setNext(NotifyFilter next) {
        this.next = next;
    }

    public abstract NotifyProxy.NotifyMsg doFilter(NotifyProxy.NotifyMsg msg);
}
