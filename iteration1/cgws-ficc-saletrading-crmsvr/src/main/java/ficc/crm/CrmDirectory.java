package ficc.crm;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import ficc.common.ApiException;
import ficc.common.JsonUtils;

import org.apache.log4j.Logger;

import sc.app.stc.api.BrokerCrmAccountQueryResult;
import sc.app.stc.api.BrokerCrmContactQueryResult;
import sc.app.stc.api.BrokerCrmInstQueryResult;
import sc.app.stc.api.TBrokerCrmContact;
import sc.app.stc.api.TBrokerCrmInst;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Properties;
import java.util.Set;
import java.util.function.Function;

/**
 * CRM 内存目录，同一快照提供三类协议：FICC 分页/ID 查询、旧 STC 镜像以及 ParserSvr 名称查找。
 * 分页保持 PostgresCrmClient 的 records/hasMore/nextOffset 结构，避免消费方依赖数据库表字段。
 * 机构搜索覆盖 ID、名称、简称及拼音，联系人搜索覆盖 ID、姓名和 CFETS 交易员字段，忽略大小写。
 * 同简称机构在搜索列表中去重：优先保留有关联联系人的记录，其次按较小 ID；ID 索引仍保留原记录。
 * 联系人可关联多个机构，CONTACT 记录返回 institutionIds，不能假定总有唯一 institutionId 或机构名称。
 * 目录使用紧凑 Entry 保存数据，返回页面或镜像时才构造 JSON/STC DTO，减少常驻对象体积。
 */
public final class CrmDirectory {
    public static final String INSTITUTION = "INSTITUTION";
    public static final String CONTACT = "CONTACT";

    /** Image topics of the STC CRM protocol; CRMSvrClient subscribes to them with an image. */
    public static final String INST_QUERY_TOPIC = "stc.crm.inst.query";
    public static final String CONTACT_QUERY_TOPIC = "stc.crm.contact.query";
    public static final String ACCOUNT_QUERY_TOPIC = "stc.crm.account.query";

    /**
     * Single bounded lookup used by parsersvr to resolve a parsed company or trader name. It
     * replaces the STC CRM image: parsersvr asks for the few rows it needs instead of mirroring
     * every institution into its own heap.
     */
    public static final String LOOKUP_TOPIC = "stc.crm.lookup";

    private static final Logger LOGGER = Logger.getLogger(CrmDirectory.class);

    private final List<Entry> institutions = new ArrayList<>();
    private final List<Entry> contacts = new ArrayList<>();
    private final Map<String, Entry> institutionById = new HashMap<>();
    private final Map<String, Entry> contactById = new HashMap<>();
    private final Map<String, Integer> contactCountByOrg = new HashMap<>();
    private final String instTagList;
    private final String contactBizType;

    public CrmDirectory(CrmDataset dataset, Properties configuration) {
        instTagList = configuration.getProperty("crm.stc.instTagList", "BOND");
        contactBizType = configuration.getProperty("crm.stc.contactBizType", "BNC");
        Map<String, List<String>> orgsByContact = new LinkedHashMap<>();
        for (CrmDataset.ContactOrg relation : dataset.contactOrgs()) {
            orgsByContact
                    .computeIfAbsent(relation.contactId, key -> new ArrayList<>())
                    .add(relation.orgId);
        }
        Set<String> orgsWithContacts = new HashSet<>();
        for (List<String> orgIds : orgsByContact.values()) {
            Collections.sort(orgIds);
            orgsWithContacts.addAll(orgIds);
            for (String orgId : orgIds) {
                Integer count = contactCountByOrg.get(orgId);
                contactCountByOrg.put(orgId, count == null ? 1 : count + 1);
            }
        }

        Map<String, Entry> byShortName = new LinkedHashMap<>();
        for (CrmDataset.Institution row : dataset.institutions()) {
            if (row.id == null || row.id.trim().isEmpty()) {
                continue;
            }
            Entry entry =
                    new Entry(
                            row.id,
                            row.name,
                            row.shortName,
                            row.shortNamePinyin,
                            row.namePinyin,
                            null,
                            row.orgType,
                            row.cfetsCode,
                            searchText(
                                    row.id, row.name, row.shortName, row.namePinyin, row.shortNamePinyin),
                            Collections.<String>emptyList());
            institutionById.put(entry.id, entry);
            String shortName = entry.shortName == null ? "" : entry.shortName.trim();
            // A missing short name is not evidence that unrelated institutions are duplicates.
            String key = shortName.isEmpty() ? "id:" + entry.id : "name:" + shortName;
            Entry kept = byShortName.get(key);
            if (kept == null || preferred(entry.id, kept.id, orgsWithContacts)) {
                byShortName.put(key, entry);
            }
        }
        institutions.addAll(byShortName.values());
        institutions.sort(Comparator.comparing(entry -> entry.id));

        for (CrmDataset.Contact row : dataset.contacts()) {
            if (row.id == null || row.id.trim().isEmpty()) {
                continue;
            }
            List<String> orgIds =
                    orgsByContact.getOrDefault(row.id, Collections.<String>emptyList());
            Entry entry =
                    new Entry(
                            row.id,
                            row.name,
                            null,
                            null,
                            null,
                            row.positionName,
                            null,
                            null,
                            searchText(row.id, row.name, row.cfetsTraderId, row.cfetsTraderName),
                            orgIds);
            contactById.put(entry.id, entry);
            contacts.add(entry);
        }
        contacts.sort(Comparator.comparing(entry -> entry.id));

        LOGGER.info(
                "CRM directory ready: institutions="
                        + institutions.size()
                        + " (of "
                        + institutionById.size()
                        + " rows), contacts="
                        + contacts.size()
                        + ", instTagList="
                        + instTagList
                        + ", contactBizType="
                        + contactBizType);
    }

    /** Bounded page in the contract of {@code PostgresCrmClient.query}. */
    public JSONObject page(String kind, String text, String institutionId, int offset, int limit) {
        return page(kind, text, institutionId, null, offset, limit);
    }

    public JSONObject page(
            String kind, String text, String institutionId, String id, int offset, int limit) {
        if (offset < 0 || offset > 1000000 || limit < 1 || limit > 200) {
            throw ApiException.forField(
                    "1001", "VALIDATION_FAILED", "CRM offset/limit out of range", "limit");
        }
        if (INSTITUTION.equals(kind)) {
            return select(institutions, kind, text, institutionId, id, offset, limit);
        }
        if (CONTACT.equals(kind)) {
            return select(contacts, kind, text, institutionId, id, offset, limit);
        }
        throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid CRM kind", "kind");
    }

    /** Single record by id, or null when unknown. */
    public JSONObject find(String kind, String id) {
        if (!INSTITUTION.equals(kind) && !CONTACT.equals(kind)) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid CRM kind", "kind");
        }
        if (id == null || id.trim().isEmpty()) {
            return null;
        }
        Entry entry = INSTITUTION.equals(kind) ? institutionById.get(id.trim()) : contactById.get(id.trim());
        return entry == null ? null : record(kind, entry);
    }

    public int size(String kind) {
        return INSTITUTION.equals(kind) ? institutions.size() : contacts.size();
    }

    /** STC image: every institution, as consumed by {@code CRMSvrClient}. */
    public BrokerCrmInstQueryResult institutionImage() {
        BrokerCrmInstQueryResult result = new BrokerCrmInstQueryResult();
        for (Entry entry : institutions) {
            result.datas.add(institution(entry));
        }
        result.totalSize = result.datas.size();
        result.pageSize = result.datas.size();
        return result;
    }

    /** STC image: every contact, as consumed by {@code CRMSvrClient}. */
    public BrokerCrmContactQueryResult contactImage() {
        BrokerCrmContactQueryResult result = new BrokerCrmContactQueryResult();
        for (Entry entry : contacts) {
            result.datas.add(contact(entry));
        }
        result.totalSize = result.datas.size();
        result.pageSize = result.datas.size();
        return result;
    }

    /**
     * STC image: broker accounts. The CRM tables served here hold institutions and traders only,
     * so the account image stays empty until a source for it is migrated.
     */
    public BrokerCrmAccountQueryResult accountImage() {
        BrokerCrmAccountQueryResult result = new BrokerCrmAccountQueryResult();
        result.totalSize = 0;
        result.pageSize = 0;
        return result;
    }

    /**
     * {@code stc.crm.contact.query}: traders whose name, id or CFETS trader fields contain the
     * dealer text, capped by {@code PageSize}.
     */
    public List<JSONObject> contactsByDealer(String dealer, int pageSize) {
        return search(contacts, dealer, pageSize, this::contactJson);
    }

    /** {@code stc.crm.inst.query} request/reply paging, used by the C# clients. */
    public List<JSONObject> institutionsByDealer(String dealer, int pageSize) {
        return search(institutions, dealer, pageSize, this::institutionJson);
    }

    /**
     * {@code stc.crm.lookup}: the rows behind one name or id, in the STC field names.
     *
     * <p>{@code id} wins over {@code name}. {@code match=exact} compares the short name first and
     * falls back to the full name, which is how parsersvr resolves a parsed company; anything else
     * is a case-insensitive contains over the searchable text of the row.
     */
    public List<JSONObject> lookup(String kind, String name, String id, String match, int pageSize) {
        if (INSTITUTION.equals(kind)) {
            return lookup(
                    institutionById, institutions, name, id, match, pageSize, this::institutionJson);
        }
        if (CONTACT.equals(kind)) {
            return lookup(contactById, contacts, name, id, match, pageSize, this::contactJson);
        }
        throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid CRM kind", "kind");
    }

    private List<JSONObject> lookup(
            Map<String, Entry> byId,
            List<Entry> index,
            String name,
            String id,
            String match,
            int pageSize,
            Function<Entry, JSONObject> mapper) {
        List<JSONObject> matches = new ArrayList<>();
        if (id != null && !id.trim().isEmpty()) {
            Entry entry = byId.get(id.trim());
            if (entry != null) {
                matches.add(mapper.apply(entry));
            }
            return matches;
        }
        return byName(index, name, match, pageSize, mapper);
    }

    private List<JSONObject> byName(
            List<Entry> index,
            String name,
            String match,
            int pageSize,
            Function<Entry, JSONObject> mapper) {
        String keyword = name == null ? "" : name.trim();
        List<JSONObject> matches = new ArrayList<>();
        if (keyword.isEmpty()) {
            return matches;
        }
        int limit = pageSize <= 0 ? 200 : Math.min(pageSize, 5000);
        if ("exact".equals(match)) {
            for (Entry entry : index) {
                if (keyword.equals(shortName(entry))) {
                    matches.add(mapper.apply(entry));
                    if (matches.size() >= limit) {
                        return matches;
                    }
                }
            }
            for (Entry entry : index) {
                if (keyword.equals(fullName(entry))) {
                    matches.add(mapper.apply(entry));
                    if (matches.size() >= limit) {
                        return matches;
                    }
                }
            }
            return matches;
        }
        return search(index, keyword, pageSize, mapper);
    }

    private static String shortName(Entry entry) {
        return entry.shortName == null ? "" : entry.shortName.trim();
    }

    private static String fullName(Entry entry) {
        return entry.name == null ? "" : entry.name.trim();
    }

    private List<JSONObject> search(
            List<Entry> index,
            String dealer,
            int pageSize,
            Function<Entry, JSONObject> mapper) {
        String keyword = dealer == null ? "" : dealer.trim().toLowerCase(Locale.ROOT);
        int limit = pageSize <= 0 ? index.size() : Math.min(pageSize, 5000);
        List<JSONObject> matches = new ArrayList<>();
        if (keyword.isEmpty()) {
            return matches;
        }
        for (Entry entry : index) {
            if (entry.search.contains(keyword)) {
                matches.add(mapper.apply(entry));
                if (matches.size() >= limit) {
                    break;
                }
            }
        }
        return matches;
    }

    private TBrokerCrmInst institution(Entry entry) {
        TBrokerCrmInst value = new TBrokerCrmInst();
        value.idbkey = entry.id;
        value.crmid = entry.id;
        value.id = entry.id;
        value.shortnamecn = entry.shortName;
        value.fullnamecn = entry.name;
        value.pinyin = entry.shortNamePinyin;
        value.pinyinfull = entry.namePinyin;
        value.instcode = entry.code;
        value.insttypevalue = entry.type;
        // CRMSvrClient.getInstByIDBkey drops institutions whose taglist misses Consts.InstTagList.
        value.taglist = instTagList;
        value.contactcount = String.valueOf(contactCountByOrg.getOrDefault(entry.id, 0));
        value.useStatus = Integer.valueOf(0);
        return value;
    }

    private TBrokerCrmContact contact(Entry entry) {
        TBrokerCrmContact value = new TBrokerCrmContact();
        value.idbkey = entry.id;
        value.crmid = entry.id;
        value.id = entry.id;
        value.namecn = entry.name;
        value.ctctposition = entry.positionName;
        // CRMSvrClient.getContact drops traders whose biztype misses Consts.ContactBizType.
        value.biztype = contactBizType;
        value.commonuse = "1";
        value.isonboard = "1";
        Entry institution =
                entry.institutionIds.isEmpty() ? null : institutionById.get(entry.institutionIds.get(0));
        if (institution != null) {
            value.instid = institution.id;
            value.instname = institution.shortName;
            value.instfullnamecn = institution.name;
            value.instcode = institution.code;
            value.insttypevalue = institution.type;
        }
        return value;
    }

    /** Row as JSON with the STC field names, for the {@code datas} list of inst.query. */
    private JSONObject institutionJson(Entry entry) {
        TBrokerCrmInst value = institution(entry);
        return JsonUtils.object(
                "idbkey",
                value.idbkey,
                "crmid",
                value.crmid,
                "id",
                value.id,
                "shortnamecn",
                value.shortnamecn,
                "fullnamecn",
                value.fullnamecn,
                "pinyin",
                value.pinyin,
                "pinyinfull",
                value.pinyinfull,
                "instcode",
                value.instcode,
                "insttypevalue",
                value.insttypevalue,
                "taglist",
                value.taglist,
                "contactcount",
                value.contactcount);
    }

    /** Row as JSON with the STC field names, for the {@code datas} list of query and lookup. */
    private JSONObject contactJson(Entry entry) {
        TBrokerCrmContact value = contact(entry);
        return JsonUtils.object(
                "idbkey",
                value.idbkey,
                "crmid",
                value.crmid,
                "id",
                value.id,
                "namecn",
                value.namecn,
                "pinyin",
                value.pinyin,
                "pinyinfull",
                value.pinyinfull,
                "instid",
                value.instid,
                "instcode",
                value.instcode,
                "instname",
                value.instname,
                "instfullnamecn",
                value.instfullnamecn,
                "insttypevalue",
                value.insttypevalue,
                "ctctposition",
                value.ctctposition,
                "biztype",
                value.biztype,
                "commonuse",
                value.commonuse,
                "isonboard",
                value.isonboard);
    }

    private JSONObject select(
            List<Entry> index,
            String kind,
            String text,
            String institutionId,
            String id,
            int offset,
            int limit) {
        if (id != null && !id.trim().isEmpty()) {
            JSONObject exact = find(kind, id);
            JSONArray records = new JSONArray();
            if (exact != null) {
                records.add(exact);
            }
            return JsonUtils.object("records", records, "hasMore", false, "nextOffset", null);
        }
        String keyword = text == null ? "" : text.trim().toLowerCase(Locale.ROOT);
        String organization = institutionId == null ? "" : institutionId.trim();
        List<Entry> matches = new ArrayList<>();
        for (Entry entry : index) {
            if (!keyword.isEmpty() && !entry.search.contains(keyword)) {
                continue;
            }
            if (!organization.isEmpty() && !entry.institutionIds.contains(organization)) {
                continue;
            }
            matches.add(entry);
            if (matches.size() > offset + limit) {
                break;
            }
        }
        boolean hasMore = matches.size() > offset + limit;
        int from = Math.min(offset, matches.size());
        int to = Math.min(offset + limit, matches.size());
        JSONArray records = new JSONArray();
        for (Entry entry : matches.subList(from, to)) {
            records.add(record(kind, entry));
        }
        return JsonUtils.object(
                "records",
                records,
                "hasMore",
                hasMore,
                "nextOffset",
                hasMore ? Integer.valueOf(offset + limit) : null);
    }

    private static JSONObject record(String kind, Entry entry) {
        if (INSTITUTION.equals(kind)) {
            return JsonUtils.object(
                    "institutionId",
                    entry.id,
                    "institutionName",
                    entry.name,
                    "institutionShortName",
                    entry.shortName);
        }
        JSONArray institutionIds = new JSONArray();
        institutionIds.addAll(entry.institutionIds);
        return JsonUtils.object(
                "contactId",
                entry.id,
                "contactName",
                entry.name,
                "positionName",
                entry.positionName,
                "institutionIds",
                institutionIds,
                "institutionId",
                entry.institutionIds.size() == 1 ? entry.institutionIds.get(0) : null);
    }

    private static boolean preferred(String candidate, String kept, Set<String> orgsWithContacts) {
        boolean candidateHasContacts = orgsWithContacts.contains(candidate);
        boolean keptHasContacts = orgsWithContacts.contains(kept);
        if (candidateHasContacts != keptHasContacts) {
            return candidateHasContacts;
        }
        return candidate.compareTo(kept) < 0;
    }

    private static String searchText(String... values) {
        StringBuilder builder = new StringBuilder();
        for (String value : values) {
            if (value == null) {
                continue;
            }
            String trimmed = value.trim();
            if (trimmed.isEmpty()) {
                continue;
            }
            if (builder.length() > 0) {
                builder.append(' ');
            }
            builder.append(trimmed);
        }
        return builder.toString().toLowerCase(Locale.ROOT);
    }

    /** Compact directory row; JSON and STC DTOs are built only for the rows a caller receives. */
    private static final class Entry {
        private final String id;
        private final String name;
        private final String shortName;
        private final String shortNamePinyin;
        private final String namePinyin;
        private final String positionName;
        private final String type;
        private final String code;
        private final String search;
        private final List<String> institutionIds;

        private Entry(
                String id,
                String name,
                String shortName,
                String shortNamePinyin,
                String namePinyin,
                String positionName,
                String type,
                String code,
                String search,
                List<String> institutionIds) {
            this.id = id;
            this.name = name;
            this.shortName = shortName;
            this.shortNamePinyin = shortNamePinyin;
            this.namePinyin = namePinyin;
            this.positionName = positionName;
            this.type = type;
            this.code = code;
            this.search = search;
            this.institutionIds = institutionIds;
        }
    }
}
