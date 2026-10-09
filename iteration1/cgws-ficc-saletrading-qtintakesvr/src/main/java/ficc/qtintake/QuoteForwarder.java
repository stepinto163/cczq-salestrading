package ficc.qtintake;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.ApiReply;
import ficc.api.QuoteCreateRequest;
import ficc.api.QuoteOperationResult;
import ficc.api.QuoteWriteData;
import ficc.api.SalesTradingApi;

import ficc.common.client.StcTransport;

import org.apache.log4j.Logger;

import java.nio.file.Path;

/**
 * 持久收件箱到 QuoteSvr 的有序转发器，同一时刻只处理队首请求，不伪造终端登录会话。
 * 成功后立即处理下一条；固定一秒等待仅用于失败恢复，不是每秒一条的吞吐限制。
 * 仅明确的报价字段校验错误进入 rejected；映射、权限、配置或未知错误保留队首，后续不越过它。
 * 当前没有最大重试次数；相同 requestId 与已解析 ownerUserId 随文件保留，供重试复用。
 */
public final class QuoteForwarder {
    private static final long RETRY_INTERVAL_MILLIS = 1000;
    private static final Logger LOGGER = Logger.getLogger(QuoteForwarder.class);
    private final DurableInbox inbox;
    private final StcTransport transport;
    private final String token;
    private final OwnerResolver owners;
    private boolean connectionUnavailable;

    public interface OwnerResolver {
        String resolve(String account) throws Exception;
    }

    public QuoteForwarder(
            DurableInbox inbox, StcTransport transport, String token, OwnerResolver owners) {
        this.inbox = inbox;
        this.transport = transport;
        this.token = token;
        this.owners = owners;
    }

    public enum Outcome {
        IDLE, COMPLETED, RETRY, BLOCKED
    }

    /** 空闲时等入箱通知，失败时等待重试间隔，连续成功时不附加定时延迟。 */
    public void run() {
        String lastFailure = null;
        while (!Thread.currentThread().isInterrupted()) {
            try {
                if (!inbox.awaitPending()) {
                    return;
                }
                Outcome outcome = tick();
                lastFailure = null;
                if (outcome == Outcome.COMPLETED || outcome == Outcome.IDLE) {
                    continue;
                }
                Thread.sleep(RETRY_INTERVAL_MILLIS);
            } catch (InterruptedException error) {
                Thread.currentThread().interrupt();
                return;
            } catch (Exception error) {
                if (Thread.currentThread().isInterrupted()) {
                    return;
                }
                String failure = inbox.next() + ":" + error.getClass().getName() + ":" + error.getMessage();
                if (!failure.equals(lastFailure)) {
                    LOGGER.error("FORWARD state=BLOCKED"
                            + " head=" + inbox.next() + " pending=" + inbox.pendingCount()
                            + " reason=UNCLASSIFIED_FAILURE requestRetained=true", error);
                    lastFailure = failure;
                }
                try {
                    Thread.sleep(RETRY_INTERVAL_MILLIS);
                } catch (InterruptedException interrupted) {
                    Thread.currentThread().interrupt();
                    return;
                }
            }
        }
    }

    public Outcome tick() throws Exception {
        Path pending = inbox.next();
        if (pending == null) {
            return Outcome.IDLE;
        }
        QuoteCreateRequest request = SalesTradingApi.QuoteCreate.request(inbox.request(pending));
        if (!transport.online()) {
            if (!connectionUnavailable) {
                LOGGER.warn("FORWARD state=WAITING target=SERVER.QuoteSvr"
                        + " reason=CLIENT_NOT_CONNECTED requestId=" + request.requestId
                        + " pending=" + inbox.pendingCount());
                connectionUnavailable = true;
            }
            return Outcome.RETRY;
        }
        if (connectionUnavailable) {
            LOGGER.info("FORWARD state=CONNECTED target=SERVER.QuoteSvr"
                    + " pending=" + inbox.pendingCount());
            connectionUnavailable = false;
        }
        if (request.intake.ownerUserId == null) {
            String owner = owners.resolve(request.intake.sourceUserId);
            if (owner == null || owner.trim().isEmpty()) {
                throw new IllegalStateException("Owner mapping returned no STC userid");
            }
            request.intake.ownerUserId = owner;
            inbox.saveRequest(pending, request.toJson());
        }
        request.intake.token = token;
        JSONObject reply;
        try {
            reply = transport.request(SalesTradingApi.QuoteCreate.topic, null, request.toJson());
        } catch (Exception error) {
            for (Throwable cause = error; cause != null; cause = cause.getCause()) {
                if (cause instanceof java.io.IOException
                        || cause instanceof java.util.concurrent.TimeoutException) {
                    LOGGER.warn("FORWARD state=RETRY requestId=" + request.requestId
                            + " reason=COMMUNICATION_FAILURE pending=" + inbox.pendingCount(), error);
                    return Outcome.RETRY;
                }
            }
            throw error;
        }
        ApiReply<QuoteWriteData> response = SalesTradingApi.QuoteCreate.reply(reply);
        if (response == null || (response.requestId != null && !request.requestId.equals(response.requestId))) {
            throw new IllegalStateException("QuoteSvr returned a missing or mismatched reply");
        }
        String code = response.Code;
        ficc.api.ErrorDetail detail = response.error;
        String message = response.Msg;
        if ("0000".equals(code)) {
            QuoteWriteData data = response.Data;
            if (data == null || data.results == null || data.results.size() != 1
                    || data.results.get(0) == null) {
                throw new IllegalStateException("QuoteSvr returned an incomplete create reply");
            }
            QuoteOperationResult result = data.results.get(0);
            if (result.clientQuoteId != null
                    && !request.quotes.get(0).clientQuoteId.equals(result.clientQuoteId)) {
                throw new IllegalStateException("QuoteSvr returned a mismatched quote result");
            }
            code = result.Code;
            detail = result.error;
            message = result.Msg;
        }
        boolean success = "0000".equals(code);
        // 只有白名单内字段的明确校验拒绝才能结束队首；权限、映射、冲突和未知结果不能当作坏报价丢弃。
        boolean rejected = "1001".equals(code) && detail != null
                && "VALIDATION_FAILED".equals(detail.type) && detail.field != null
                && java.util.Arrays.asList("securityId", "side", "quoteType", "yieldType", "price",
                        "quantity", "quantityUnit", "currency", "tradeDate", "settlType",
                        "urgent", "recommended", "whole", "oco", "packing", "remark")
                        .contains(detail.field);
        Outcome outcome;
        if (success || rejected) {
            inbox.finish(pending, reply, success);
            outcome = Outcome.COMPLETED;
        } else {
            outcome = "1009".equals(code) || "1099".equals(code)
                    ? Outcome.RETRY : Outcome.BLOCKED;
        }
        String summary = "FORWARD state=" + (success ? "SENT" : rejected ? "REJECTED" : outcome.name())
                + " requestId=" + request.requestId
                + " clientQuoteId=" + request.quotes.get(0).clientQuoteId
                + " code=" + code + " errorType=" + (detail == null ? null : detail.type)
                + " field=" + (detail == null ? null : detail.field)
                + " reason=" + message + " pending=" + inbox.pendingCount();
        if (success) {
            LOGGER.info(summary);
        } else {
            LOGGER.warn(summary);
        }
        return outcome;
    }
}
