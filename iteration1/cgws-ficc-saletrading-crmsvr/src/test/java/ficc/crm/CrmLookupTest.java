package ficc.crm;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.ApiException;

import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

/**
 * {@code stc.crm.lookup}: the topic parsersvr uses instead of the STC CRM image, so the fields it
 * reads (idbkey, shortnamecn, namecn, instid, taglist, biztype) are part of the contract.
 */
class CrmLookupTest {

    @Test
    void institutionIsFoundByExactShortName() {
        CrmDirectory directory = directory();
        List<JSONObject> rows =
                directory.lookup(CrmDirectory.INSTITUTION, "示例银行", null, "exact", 50);
        assertEquals(1, rows.size());
        assertEquals("2001", rows.get(0).getString("idbkey"));
        assertEquals("示例银行", rows.get(0).getString("shortnamecn"));
        assertEquals("示例银行股份有限公司", rows.get(0).getString("fullnamecn"));
        // CRMSvrClient drops institutions whose taglist misses Consts.InstTagList.
        assertEquals("BOND", rows.get(0).getString("taglist"));
    }

    @Test
    void institutionFallsBackToTheFullName() {
        CrmDirectory directory = directory();
        List<JSONObject> rows =
                directory.lookup(
                        CrmDirectory.INSTITUTION, "示例银行股份有限公司", null, "exact", 50);
        assertEquals(1, rows.size());
        assertEquals("2001", rows.get(0).getString("idbkey"));

        assertEquals(
                0, directory.lookup(CrmDirectory.INSTITUTION, "不存在的机构", null, "exact", 50).size());
        assertEquals(0, directory.lookup(CrmDirectory.INSTITUTION, "  ", null, "exact", 50).size());
    }

    @Test
    void institutionContainsSearchIsBoundedByPageSize() {
        CrmDirectory directory = directory();
        assertEquals(
                2, directory.lookup(CrmDirectory.INSTITUTION, "银行", null, "contains", 50).size());
        assertEquals(
                1, directory.lookup(CrmDirectory.INSTITUTION, "银行", null, "contains", 1).size());
    }

    @Test
    void contactIsFoundByNameContains() {
        CrmDirectory directory = directory();
        List<JSONObject> rows = directory.lookup(CrmDirectory.CONTACT, "王五", null, "contains", 50);
        assertEquals(1, rows.size());
        JSONObject row = rows.get(0);
        assertEquals("t001", row.getString("idbkey"));
        assertEquals("王五", row.getString("namecn"));
        assertEquals("2001", row.getString("instid"));
        assertEquals("示例银行", row.getString("instname"));
        // CRMSvrClient.getContact drops traders whose biztype misses Consts.ContactBizType.
        assertEquals("BNC", row.getString("biztype"));

        assertEquals(
                0, directory.lookup(CrmDirectory.CONTACT, "不存在的人", null, "contains", 50).size());
    }

    @Test
    void idWinsOverName() {
        CrmDirectory directory = directory();
        List<JSONObject> institution =
                directory.lookup(CrmDirectory.INSTITUTION, "示例银行", "2002", "exact", 50);
        assertEquals(1, institution.size());
        assertEquals("2002", institution.get(0).getString("idbkey"));

        List<JSONObject> contact =
                directory.lookup(CrmDirectory.CONTACT, "王五", "t001", "contains", 50);
        assertEquals(1, contact.size());
        assertEquals("t001", contact.get(0).getString("idbkey"));

        assertEquals(
                0, directory.lookup(CrmDirectory.INSTITUTION, null, "9999", "exact", 50).size());
    }

    @Test
    void unknownKindIsRejected() {
        CrmDirectory directory = directory();
        ApiException error =
                assertThrows(
                        ApiException.class,
                        () -> directory.lookup("OTHER", "示例银行", null, "exact", 50));
        assertEquals("1001", error.code);
        assertEquals("kind", error.details.getString("field"));
    }

    private static CrmDirectory directory() {
        List<CrmDataset.Institution> institutions = new ArrayList<>();
        institutions.add(
                new CrmDataset.Institution(
                        "2001", "示例银行股份有限公司", "示例银行", "shiliyinhang", "shiliyinhang", "BANK", "0000"));
        institutions.add(
                new CrmDataset.Institution(
                        "2002", "另一家银行", "另一家银行", "lingyijiayinhang", "lingyi", "BANK", "0001"));

        List<CrmDataset.Contact> contacts = new ArrayList<>();
        contacts.add(new CrmDataset.Contact("t001", "王五", "交易员", "10000", "trader-1", "王五"));

        List<CrmDataset.ContactOrg> relations = new ArrayList<>();
        relations.add(new CrmDataset.ContactOrg("t001", "2001"));

        return new CrmDirectory(new CrmDataset(institutions, contacts, relations), new Properties());
    }
}
