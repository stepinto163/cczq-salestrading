package ficc.quote;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.*;
import ficc.common.ApiException;
import ficc.common.BusinessClock;
import ficc.common.ReferenceData;
import java.nio.charset.StandardCharsets;
import java.time.*;
import java.time.temporal.ChronoUnit;
import java.util.*;

/** 本券推荐：所有归属人参与计算，输出时逐条脱敏；不生成模拟候选或成交事实。 */
final class RecommendationService {
    static final String SCORING_VERSION = "quote-evidence-v1";
    private final PgStore store;
    private final ReferenceData references;

    RecommendationService(PgStore store, ReferenceData references) {
        this.store = store;
        this.references = references;
    }

    static LocalDate[] window(RecommendationQueryRequest request) {
        try {
            if (request.securityId == null || request.securityId.trim().isEmpty()) {
                throw new IllegalArgumentException("请选择债券");
            }
            LocalDate start = LocalDate.parse(request.startDate);
            LocalDate end = LocalDate.parse(request.endDate);
            long days = ChronoUnit.DAYS.between(start, end) + 1;
            if (days < 1 || days > 365) {
                throw new IllegalArgumentException("推荐时间窗口必须为1至365天");
            }
            return new LocalDate[] {start, end};
        } catch (RuntimeException error) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "请选择债券及有效日期，窗口不得超过365天", "startDate");
        }
    }

    RecommendationQueryData query(String user, RecommendationQueryRequest request) throws Exception {
        window(request);
        List<QuoteRecord> evidence = store.recommendationEvidence(request.securityId, request.startDate, request.endDate);
        JSONObject security = evidence.isEmpty() ? null : references.security(request.securityId);
        String name = security == null ? request.securityId : security.getString("securityName");
        RecommendationQueryData result = new RecommendationQueryData();
        result.securityId = request.securityId;
        result.startDate = request.startDate;
        result.endDate = request.endDate;
        result.scoringVersion = SCORING_VERSION;
        result.recommendations = calculate(user, evidence, name, OffsetDateTime.parse(BusinessClock.currentTime()).toInstant());
        Map<String, String> feedback = new HashMap<>();
        for (JSONObject row : store.rows("SELECT jsonb_build_object('id',recommendation_id,'feedback',feedback)"
                + " FROM recommendation_feedback WHERE owner_id=?", user)) {
            feedback.put(row.getString("id"), row.getString("feedback"));
        }
        for (RecommendationRecord row : result.recommendations) {
            row.feedback = feedback.get(row.recommendationId);
        }
        return result;
    }

    static List<RecommendationRecord> calculate(String viewer, List<QuoteRecord> evidence, String securityName, Instant now) {
        Map<String, List<QuoteRecord>> groups = new LinkedHashMap<>();
        for (QuoteRecord quote : evidence) {
            // 输入必须为历史中已确认的 ACTIVE 快照，待确认报价永不作为推荐依据。
            if (!"ACTIVE".equals(quote.status) || !("1".equals(quote.side) || "2".equals(quote.side))) {
                continue;
            }
            String key = com.alibaba.fastjson2.JSON.toJSONString(Arrays.asList(quote.securityId, quote.side,
                    quote.ownerUserId, quote.institutionId, quote.contactId));
            groups.computeIfAbsent(key, ignored -> new ArrayList<>()).add(quote);
        }
        List<RecommendationRecord> result = new ArrayList<>();
        for (Map.Entry<String, List<QuoteRecord>> group : groups.entrySet()) {
            List<QuoteRecord> quotes = group.getValue();
            quotes.sort(Comparator.<QuoteRecord, Instant>comparing(q -> OffsetDateTime.parse(q.updateTime).toInstant())
                    .reversed().thenComparing(q -> q.quoteId));
            QuoteRecord latest = quotes.get(0);
            RecommendationRecord row = new RecommendationRecord();
            row.recommendationId = "REC-" + UUID.nameUUIDFromBytes(group.getKey().getBytes(StandardCharsets.UTF_8));
            row.securityId = latest.securityId;
            row.securityName = securityName;
            row.side = latest.side;
            row.ownerUserId = latest.ownerUserId;
            row.latestPrice = latest.price;
            row.quoteType = latest.quoteType;
            row.yieldType = latest.yieldType;
            row.latestQuantity = latest.quantity;
            row.latestTime = latest.updateTime;
            row.quoteCount = quotes.size();
            row.evidence = new ArrayList<>();
            double weight = 0;
            for (QuoteRecord quote : quotes) {
                double ageDays = Math.max(0, Duration.between(OffsetDateTime.parse(quote.updateTime).toInstant(), now).toMillis()
                        / 86400000.0);
                weight += Math.pow(0.5, ageDays / 30.0);
                RecommendationEvidence detail = new RecommendationEvidence();
                detail.quoteId = quote.quoteId;
                detail.ownerUserId = quote.ownerUserId;
                detail.securityId = quote.securityId;
                detail.securityName = securityName;
                detail.side = quote.side;
                detail.price = quote.price;
                detail.quoteType = quote.quoteType;
                detail.yieldType = quote.yieldType;
                detail.quantity = quote.quantity;
                detail.updateTime = quote.updateTime;
                // 不调用普通行情的可见范围过滤；只按当前会话归属控制敏感字段。
                if (Objects.equals(viewer, quote.ownerUserId)) {
                    detail.institutionName = quote.institutionName;
                    detail.contactName = quote.contactName;
                } else {
                    detail.clearField("institutionName");
                    detail.clearField("contactName");
                }
                row.evidence.add(detail);
            }
            if (!Objects.equals(viewer, row.ownerUserId)) {
                row.clearField("institutionName");
                row.clearField("contactName");
            }
            row.institutionName = row.evidence.get(0).institutionName;
            row.contactName = row.evidence.get(0).contactName;
            row.score = Math.round(10000 * (1 - Math.exp(-weight / 5.0))) / 100.0;
            result.add(row);
        }
        result.sort(Comparator.<RecommendationRecord, Double>comparing(row -> row.score).reversed()
                .thenComparing(row -> row.recommendationId));
        return result;
    }

    RecommendationFeedbackData feedback(String user, RecommendationFeedbackRequest request) throws Exception {
        if (!Arrays.asList("USEFUL", "NOT_USEFUL", "NOT_APPLICABLE").contains(request.feedback)) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "请选择有用、无用或不适用", "feedback");
        }
        boolean exists = query(user, request).recommendations.stream()
                .anyMatch(row -> row.recommendationId.equals(request.recommendationId));
        if (!exists) {
            throw ApiException.forField("1002", "NOT_FOUND", "推荐已不存在，请刷新后重试", "recommendationId");
        }
        store.update("INSERT INTO recommendation_feedback(owner_id,recommendation_id,feedback) VALUES (?,?,?)"
                + " ON CONFLICT(owner_id,recommendation_id) DO NOTHING", user, request.recommendationId, request.feedback);
        RecommendationFeedbackData result = new RecommendationFeedbackData();
        result.recommendationId = request.recommendationId;
        result.feedback = store.scalar("SELECT feedback FROM recommendation_feedback WHERE owner_id=? AND recommendation_id=?",
                user, request.recommendationId);
        return result;
    }
}
