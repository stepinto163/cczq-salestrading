package ficc.crm;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import ficc.common.ApiException;

import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

class CrmDirectoryTest {

    @Test
    void duplicateShortNamesCollapseToTheRowWithContacts() {
        CrmDirectory directory = directory();
        // 1001 and 1002 share a short name; only 1002 has a contact.
        assertEquals(2, directory.size(CrmDirectory.INSTITUTION));
        assertNotNull(directory.find(CrmDirectory.INSTITUTION, "1001"));
        JSONArray page =
                directory.page(CrmDirectory.INSTITUTION, "", null, 0, 50).getJSONArray("records");
        assertEquals(2, page.size());
        assertEquals("1002", page.getJSONObject(0).getString("institutionId"));
        assertEquals("1003", page.getJSONObject(1).getString("institutionId"));
    }

    @Test
    void searchIsCaseInsensitiveAndMatchesPinyin() {
        CrmDirectory directory = directory();
        JSONArray byName =
                directory.page(CrmDirectory.INSTITUTION, "银行", null, 0, 50).getJSONArray("records");
        assertEquals(1, byName.size());
        // 1001/1002 share the short name; the listed row is the one with contacts.
        assertEquals("测试银行", byName.getJSONObject(0).getString("institutionShortName"));
        JSONArray byPinyin =
                directory.page(CrmDirectory.INSTITUTION, "ceshi", null, 0, 50).getJSONArray("records");
        assertEquals(1, byPinyin.size());
        JSONArray byId =
                directory.page(CrmDirectory.INSTITUTION, "1002", null, 0, 50).getJSONArray("records");
        assertEquals(1, byId.size());
    }

    @Test
    void contactSearchCoversNameAndTraderFieldsOnly() {
        CrmDirectory directory = directory();
        assertEquals(
                1,
                directory
                        .page(CrmDirectory.CONTACT, "张三", null, 0, 50)
                        .getJSONArray("records")
                        .size());
        assertEquals(
                1,
                directory
                        .page(CrmDirectory.CONTACT, "trader-9", null, 0, 50)
                        .getJSONArray("records")
                        .size());
        // The institution name is not part of the contact search text, matching the SQL path.
        assertEquals(
                0,
                directory
                        .page(CrmDirectory.CONTACT, "测试银行", null, 0, 50)
                        .getJSONArray("records")
                        .size());
    }

    @Test
    void contactsCanBeFilteredByInstitution() {
        CrmDirectory directory = directory();
        // c001 -> 1002, c002 -> 1002 and 1003.
        JSONArray both =
                directory.page(CrmDirectory.CONTACT, "", "1002", 0, 50).getJSONArray("records");
        assertEquals(2, both.size());
        assertEquals("c001", both.getJSONObject(0).getString("contactId"));
        assertEquals("c002", both.getJSONObject(1).getString("contactId"));
        assertEquals("1002", both.getJSONObject(0).getString("institutionId"));
        JSONArray only =
                directory.page(CrmDirectory.CONTACT, "", "1003", 0, 50).getJSONArray("records");
        assertEquals(1, only.size());
        assertEquals("c002", only.getJSONObject(0).getString("contactId"));
        assertEquals(
                0,
                directory
                        .page(CrmDirectory.CONTACT, "", "1001", 0, 50)
                        .getJSONArray("records")
                        .size());
    }

    @Test
    void contactWithSeveralInstitutionsKeepsAllOfThem() {
        CrmDirectory directory = directory();
        JSONObject contact = directory.find(CrmDirectory.CONTACT, "c002");
        assertEquals(Arrays.asList("1002", "1003"), contact.getJSONArray("institutionIds"));
        assertNull(contact.get("institutionId"));
    }

    @Test
    void pagingReportsHasMoreAndNextOffset() {
        CrmDirectory directory = directory();
        JSONObject first = directory.page(CrmDirectory.CONTACT, "", null, 0, 1);
        assertEquals(1, first.getJSONArray("records").size());
        assertTrue(first.getBooleanValue("hasMore"));
        assertEquals(1, first.getIntValue("nextOffset"));

        JSONObject last = directory.page(CrmDirectory.CONTACT, "", null, 1, 1);
        assertEquals(1, last.getJSONArray("records").size());
        assertFalse(last.getBooleanValue("hasMore"));
        assertNull(last.get("nextOffset"));

        JSONObject beyond = directory.page(CrmDirectory.CONTACT, "", null, 99, 10);
        assertEquals(0, beyond.getJSONArray("records").size());
        assertFalse(beyond.getBooleanValue("hasMore"));
    }

    @Test
    void findLooksUpEveryRowEvenWhenItIsNotListed() {
        CrmDirectory directory = directory();
        assertEquals(
                "测试银行",
                directory.find(CrmDirectory.INSTITUTION, "1001").getString("institutionName"));
        assertNull(directory.find(CrmDirectory.INSTITUTION, "9999"));
        assertNull(directory.find(CrmDirectory.INSTITUTION, null));
        assertNull(directory.find(CrmDirectory.CONTACT, " "));
    }

    @Test
    void returnedRecordsAreCopies() {
        CrmDirectory directory = directory();
        JSONObject first = directory.find(CrmDirectory.CONTACT, "c001");
        first.put("contactName", "changed");
        assertEquals("张三", directory.find(CrmDirectory.CONTACT, "c001").getString("contactName"));
    }

    @Test
    void invalidPageOrKindIsRejected() {
        CrmDirectory directory = directory();
        ApiException bounds =
                assertThrows(
                        ApiException.class,
                        () -> directory.page(CrmDirectory.INSTITUTION, "", null, 0, 0));
        assertEquals("1001", bounds.code);
        assertEquals("limit", bounds.details.getString("field"));

        ApiException kind =
                assertThrows(
                        ApiException.class, () -> directory.page("OTHER", "", null, 0, 10));
        assertEquals("1001", kind.code);
        assertEquals("kind", kind.details.getString("field"));

        ApiException lookup =
                assertThrows(ApiException.class, () -> directory.find("OTHER", "1"));
        assertEquals("1001", lookup.code);
    }

    @Test
    void institutionsWithoutShortNamesRemainIndividuallySearchable() {
        List<CrmDataset.Institution> institutions = new ArrayList<>();
        institutions.add(institution("2001", "First Bank", null, "first", null));
        institutions.add(institution("2002", "Second Bank", "", "second", ""));
        institutions.add(institution("2003", "Third Bank", "   ", "third", ""));
        CrmDirectory directory = new CrmDirectory(
                new CrmDataset(institutions, new ArrayList<>(), new ArrayList<>()),
                new Properties());

        assertEquals(3, directory.page(CrmDirectory.INSTITUTION, "Bank", null, 0, 10)
                .getJSONArray("records").size());
        for (CrmDataset.Institution institution : institutions) {
            JSONArray result = directory.page(CrmDirectory.INSTITUTION, institution.id, null, 0, 10)
                    .getJSONArray("records");
            assertEquals(1, result.size());
            assertEquals(institution.id, result.getJSONObject(0).getString("institutionId"));
        }
    }

    private static CrmDirectory directory() {
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

        return new CrmDirectory(new CrmDataset(institutions, contacts, relations), new Properties());
    }

    private static CrmDataset.Institution institution(
            String id, String name, String shortName, String pinyin, String shortPinyin) {
        return new CrmDataset.Institution(id, name, shortName, pinyin, shortPinyin, "BANK", "0000");
    }

    private static CrmDataset.Contact contact(String id, String name, String traderId) {
        return new CrmDataset.Contact(id, name, "交易员", "10000", traderId, name);
    }
}
