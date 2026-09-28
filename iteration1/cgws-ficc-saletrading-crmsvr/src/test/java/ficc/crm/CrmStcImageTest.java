package ficc.crm;

import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;

import com.alibaba.fastjson2.JSONObject;

import org.junit.jupiter.api.Test;

import sc.app.stc.api.BrokerCrmAccountQueryResult;
import sc.app.stc.api.BrokerCrmContactQueryResult;
import sc.app.stc.api.BrokerCrmInstQueryResult;
import sc.app.stc.api.TBrokerCrmContact;
import sc.app.stc.api.TBrokerCrmInst;

import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/** The STC CRM contract parsersvr relies on: images, tag filters and the trader lookup. */
class CrmStcImageTest {

    @Test
    void institutionImagePassesTheConsumerTagFilter() {
        BrokerCrmInstQueryResult image = directory(new Properties()).institutionImage();
        // 1001 and 1002 share a short name; only the listed row reaches the consumer.
        assertEquals(2, image.datas.size());
        assertEquals(2, image.totalSize);
        TBrokerCrmInst first = image.datas.get(0);
        assertEquals("1002", first.idbkey);
        assertEquals("1002", first.crmid);
        assertEquals("1002", first.id);
        assertEquals("测试银行", first.shortnamecn);
        assertEquals("测试银行股份有限公司", first.fullnamecn);
        assertEquals("ceshiyinhang", first.pinyin);
        assertEquals("ceshiyinhang", first.pinyinfull);
        // CRMSvrClient.getInstByIDBkey drops institutions whose taglist misses Consts.InstTagList.
        assertEquals("BOND", first.taglist);
        assertEquals(Integer.valueOf(0), first.useStatus);
        assertEquals("2", first.contactcount);
    }

    @Test
    void contactImageCarriesInstitutionAndBizType() {
        BrokerCrmContactQueryResult image = directory(new Properties()).contactImage();
        assertEquals(2, image.datas.size());
        TBrokerCrmContact first = image.datas.get(0);
        assertEquals("c001", first.idbkey);
        assertEquals("张三", first.namecn);
        assertEquals("交易员", first.ctctposition);
        // CRMSvrClient.getContact drops traders whose biztype misses Consts.ContactBizType.
        assertEquals("BNC", first.biztype);
        assertEquals("1", first.commonuse);
        assertEquals("1002", first.instid);
        assertEquals("测试银行", first.instname);
        assertEquals("测试银行股份有限公司", first.instfullnamecn);
        assertEquals("BANK", first.insttypevalue);
        assertEquals("0000", first.instcode);
    }

    @Test
    void contactWithSeveralInstitutionsUsesTheSmallestOne() {
        TBrokerCrmContact contact = directory(new Properties()).contactImage().datas.get(1);
        assertEquals("c002", contact.idbkey);
        // c002 belongs to 1002 and 1003; one row per contact, so the smallest id wins.
        assertEquals("1002", contact.instid);
    }

    @Test
    void tagListAndBizTypeAreConfigurable() {
        Properties configuration = new Properties();
        configuration.setProperty("crm.stc.instTagList", "NCD");
        configuration.setProperty("crm.stc.contactBizType", "BCO");
        CrmDirectory directory = directory(configuration);
        assertEquals("NCD", directory.institutionImage().datas.get(0).taglist);
        assertEquals("BCO", directory.contactImage().datas.get(0).biztype);
    }

    @Test
    void accountImageStaysEmptyUntilAccountsAreMigrated() {
        BrokerCrmAccountQueryResult image = directory(new Properties()).accountImage();
        assertTrue(image.datas.isEmpty());
        assertEquals(0, image.totalSize);
    }

    @Test
    void contactQueryMatchesTheDealerText() {
        CrmDirectory directory = directory(new Properties());
        List<JSONObject> matches = directory.contactsByDealer("张三", 5000);
        assertEquals(1, matches.size());
        assertEquals("c001", matches.get(0).getString("idbkey"));
        assertEquals("张三", matches.get(0).getString("namecn"));
        assertEquals("1002", matches.get(0).getString("instid"));
        assertEquals("BNC", matches.get(0).getString("biztype"));

        assertEquals(1, directory.contactsByDealer("trader-9", 5000).size());
        assertTrue(directory.contactsByDealer("不存在", 5000).isEmpty());
        assertTrue(directory.contactsByDealer("", 5000).isEmpty());
        assertTrue(directory.contactsByDealer(null, 5000).isEmpty());
    }

    @Test
    void contactQueryHonoursPageSize() {
        CrmDirectory directory = directory(new Properties());
        assertEquals(1, directory.contactsByDealer("c00", 1).size());
    }

    @Test
    void institutionRequestMatchesTheDealerText() {
        CrmDirectory directory = directory(new Properties());
        List<JSONObject> matches = directory.institutionsByDealer("ceshi", 100);
        assertEquals(1, matches.size());
        assertEquals("1002", matches.get(0).getString("idbkey"));
        assertEquals("测试银行", matches.get(0).getString("shortnamecn"));
        assertEquals("BOND", matches.get(0).getString("taglist"));
        assertTrue(directory.institutionsByDealer("不存在", 100).isEmpty());
    }

    @Test
    void institutionImageSurvivesTheWireFormat() throws Exception {
        String content =
                MsgHelper.Serializer(
                        directory(new Properties()).institutionImage(), CharsetConvertType.None);
        BrokerCrmInstQueryResult parsed =
                MsgHelper.Deserialize(content, BrokerCrmInstQueryResult.class, CharsetConvertType.None);
        assertEquals(2, parsed.datas.size());
        assertEquals("1002", parsed.datas.get(0).idbkey);
        assertEquals("测试银行", parsed.datas.get(0).shortnamecn);
        assertEquals("BOND", parsed.datas.get(0).taglist);
    }

    @Test
    void contactImageSurvivesTheWireFormat() throws Exception {
        String content =
                MsgHelper.Serializer(
                        directory(new Properties()).contactImage(), CharsetConvertType.None);
        BrokerCrmContactQueryResult parsed =
                MsgHelper.Deserialize(
                        content, BrokerCrmContactQueryResult.class, CharsetConvertType.None);
        assertEquals(2, parsed.datas.size());
        assertEquals("c001", parsed.datas.get(0).idbkey);
        assertEquals("张三", parsed.datas.get(0).namecn);
        assertEquals("BNC", parsed.datas.get(0).biztype);
    }

    private static CrmDirectory directory(Properties configuration) {
        List<CrmDataset.Institution> institutions = new ArrayList<>();
        institutions.add(institution("1001", "测试银行", "测试银行", "ceshiyinhang", "ceshiyinhang"));
        institutions.add(institution("1002", "测试银行股份有限公司", "测试银行", "ceshiyinhang", "ceshiyinhang"));
        institutions.add(institution("1003", "示例基金", "示例基金", "shilijijin", "shilijijin"));

        List<CrmDataset.Contact> contacts = new ArrayList<>();
        contacts.add(contact("c001", "张三", "trader-9"));
        contacts.add(contact("c002", "李四", "trader-8"));

        List<CrmDataset.ContactOrg> relations = new ArrayList<>();
        relations.add(new CrmDataset.ContactOrg("c001", "1002"));
        relations.add(new CrmDataset.ContactOrg("c002", "1003"));
        relations.add(new CrmDataset.ContactOrg("c002", "1002"));

        return new CrmDirectory(new CrmDataset(institutions, contacts, relations), configuration);
    }

    private static CrmDataset.Institution institution(
            String id, String name, String shortName, String pinyin, String shortPinyin) {
        return new CrmDataset.Institution(id, name, shortName, pinyin, shortPinyin, "BANK", "0000");
    }

    private static CrmDataset.Contact contact(String id, String name, String traderId) {
        return new CrmDataset.Contact(id, name, "交易员", "10000", traderId, name);
    }
}
