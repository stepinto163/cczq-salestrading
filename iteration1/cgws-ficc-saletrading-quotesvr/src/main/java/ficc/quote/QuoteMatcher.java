package ficc.quote;

import ficc.api.Filter;
import ficc.api.FilterSubgroup;
import ficc.api.QuoteRecord;
import ficc.api.QuoteQueryRequest;
import ficc.api.ReferenceRecord;
import ficc.api.SettingRecord;
import ficc.api.WatchlistSecurity;
import ficc.common.ApiException;
import ficc.common.FilterEngine;
import ficc.common.QuoteFilterValues;
import ficc.common.ReferenceData;
import ficc.common.client.AccessPolicy;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.function.Predicate;

/**
 * 查询与增量视图共用的报价筛选器，直接读取 QuoteRecord 和债券 ReferenceRecord。
 * prepareQuery 组合状态、归属范围、权限与业务条件；prepare 只准备业务条件，不单独保证权限。
 * 每次准备的谓词拥有自己的债券缓存，避免同次匹配重复查基础数据，也避免跨查询永久缓存。
 */
final class QuoteMatcher {
    private final ReferenceData references;
    private final Map<String, SettingRecord> settings;

    QuoteMatcher(ReferenceData references, Map<String, SettingRecord> settings) {
        this.references = references;
        this.settings = settings;
    }

    /** 查询与增量视图使用同一套可见性、状态和条件规则，避免初次加载与后续推送口径不同。 */
    Predicate<QuoteRecord> prepareQuery(String user, QuoteQueryRequest request, AccessPolicy access) {
        Filter filter = request.filter;
        FilterEngine.validate(filter == null ? null : filter.toJson());
        Predicate<QuoteRecord> matches = prepare(user, filter);
        String scope = request.ownerScope == null ? "VISIBLE" : request.ownerScope;
        if (!Arrays.asList("VISIBLE", "MINE").contains(scope)) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid ownerScope", "ownerScope");
        }
        Set<String> statuses = request.status == null
                ? Collections.singleton("ACTIVE") : new HashSet<>(request.status);
        if (!Arrays.asList("ACTIVE", "PENDING_CONFIRM", "REFERRED", "DELETED").containsAll(statuses)) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Unknown quote status", "status");
        }
        return quote -> statuses.contains(quote.status)
                && access.canViewQuote(user, quote.ownerUserId,
                        quote.source == null ? null : quote.source.type, quote.status)
                && (!"MINE".equals(scope) || user.equals(quote.ownerUserId))
                && matches.test(quote);
    }

    Predicate<QuoteRecord> prepare(String user, Filter filter) {
        if (filter == null || (filter.market == null && empty(filter.conditions)
                && empty(filter.watchlistIds) && empty(filter.filterGroupIds))) {
            return quote -> true;
        }
        List<SettingRecord> watchlists = groups(user, filter, filter.watchlistIds, "watchlistIds");
        List<SettingRecord> filterGroups = groups(user, filter, filter.filterGroupIds, "filterGroupIds");
        List<Filter> groupFilters = new ArrayList<>();
        for (SettingRecord group : filterGroups) {
            Filter combined = new Filter();
            combined.market = group.market;
            combined.conditions = new ArrayList<>();
            if (group.subgroups != null) {
                for (FilterSubgroup subgroup : group.subgroups) {
                    if (subgroup.conditions != null) {
                        combined.conditions.addAll(subgroup.conditions);
                    }
                }
            }
            groupFilters.add(combined);
        }
        // Scope to one query: fresh reference data on the next query, one lookup per bond here.
        Map<String, ReferenceRecord> securities = new HashMap<>();
        return quote -> {
            ReferenceRecord security = quote.securityId == null ? null
                    : securities.computeIfAbsent(quote.securityId, references::requireRecord);
            QuoteFilterValues values = new QuoteFilterValues(quote, security);
            if (!FilterEngine.match(values, filter)) {
                return false;
            }
            if (!watchlists.isEmpty()) {
                boolean matched = false;
                for (SettingRecord watchlist : watchlists) {
                    if (watchlist.securities != null) {
                        for (WatchlistSecurity member : watchlist.securities) {
                            matched |= Objects.equals(quote.securityId, member.securityId);
                        }
                    }
                }
                if (!matched) {
                    return false;
                }
            }
            if (!groupFilters.isEmpty()) {
                boolean matched = false;
                for (Filter groupFilter : groupFilters) {
                    matched |= FilterEngine.match(values, groupFilter);
                }
                if (!matched) {
                    return false;
                }
            }
            return true;
        };
    }

    private List<SettingRecord> groups(String user, Filter filter, List<String> ids, String field) {
        if (empty(ids)) {
            return Collections.emptyList();
        }
        List<SettingRecord> result = new ArrayList<>();
        for (String id : ids) {
            SettingRecord group = settings.get(id);
            if (group == null || !Objects.equals(user, group.userId)) {
                throw ApiException.forField("1002", "NOT_FOUND", "Group not found", field);
            }
            if (filter.market != null && !Objects.equals(filter.market, group.market)) {
                throw ApiException.forField("1001", "VALIDATION_FAILED", "Group market mismatch", field);
            }
            result.add(group);
        }
        return result;
    }

    private static boolean empty(List<?> values) {
        return values == null || values.isEmpty();
    }
}
