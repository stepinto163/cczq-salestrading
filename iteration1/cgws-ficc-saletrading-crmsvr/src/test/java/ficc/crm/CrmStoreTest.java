package ficc.crm;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import org.junit.jupiter.api.Test;

import java.io.BufferedReader;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assumptions.assumeTrue;

final class CrmStoreTest {

    @Test
    void realDatabaseSnapshotServesThePageContract() throws Exception {
        String path = System.getProperty("crm.test.config");
        assumeTrue(path != null, "Supply crm.test.config to smoke CRMSvr against a real CRM database");
        Properties configuration = new Properties();
        try (BufferedReader reader = Files.newBufferedReader(Paths.get(path))) {
            configuration.load(reader);
        }
        try (CrmStore store = new CrmStore(configuration)) {
            CrmDirectory directory = new CrmDirectory(store.load(), configuration);
            assertTrue(directory.size(CrmDirectory.INSTITUTION) > 0, "institutions");
            assertTrue(directory.size(CrmDirectory.CONTACT) > 0, "contacts");

            JSONObject firstPage = directory.page(CrmDirectory.INSTITUTION, "", null, 0, 5);
            JSONArray institutions = firstPage.getJSONArray("records");
            assertEquals(5, institutions.size());
            assertTrue(firstPage.getBooleanValue("hasMore"));
            assertEquals(5, firstPage.getIntValue("nextOffset"));
            for (int index = 1; index < institutions.size(); index++) {
                assertTrue(
                        institutions
                                        .getJSONObject(index - 1)
                                        .getString("institutionId")
                                        .compareTo(
                                                institutions
                                                        .getJSONObject(index)
                                                        .getString("institutionId"))
                                < 0,
                        "institutions are ordered by id");
            }

            String lastOfFirstPage = institutions.getJSONObject(4).getString("institutionId");
            JSONObject secondPage = directory.page(CrmDirectory.INSTITUTION, "", null, 5, 5);
            assertTrue(
                    secondPage
                                    .getJSONArray("records")
                                    .getJSONObject(0)
                                    .getString("institutionId")
                                    .compareTo(lastOfFirstPage)
                            > 0,
                    "the next offset continues after the previous page");

            String institutionId = institutions.getJSONObject(0).getString("institutionId");
            assertNotNull(directory.find(CrmDirectory.INSTITUTION, institutionId));
            assertEquals(
                    institutionId,
                    directory.find(CrmDirectory.INSTITUTION, institutionId).getString("institutionId"));

            JSONArray contacts =
                    directory
                            .page(CrmDirectory.CONTACT, "", institutionId, 0, 50)
                            .getJSONArray("records");
            for (int index = 0; index < contacts.size(); index++) {
                JSONObject contact = contacts.getJSONObject(index);
                assertTrue(
                        contact.getJSONArray("institutionIds").contains(institutionId),
                        "filtered contacts belong to the institution");
            }

            JSONObject anyContact =
                    directory.page(CrmDirectory.CONTACT, "", null, 0, 1).getJSONArray("records").getJSONObject(0);
            assertEquals(
                    anyContact.getString("contactId"),
                    directory.find(CrmDirectory.CONTACT, anyContact.getString("contactId")).getString("contactId"));

            sc.app.stc.api.BrokerCrmInstQueryResult instImage = directory.institutionImage();
            assertEquals(directory.size(CrmDirectory.INSTITUTION), instImage.datas.size());
            assertNotNull(instImage.datas.get(0).idbkey);
            assertEquals("BOND", instImage.datas.get(0).taglist);

            sc.app.stc.api.BrokerCrmContactQueryResult contactImage = directory.contactImage();
            assertEquals(directory.size(CrmDirectory.CONTACT), contactImage.datas.size());
            if (!contactImage.datas.isEmpty()) {
                String trader = contactImage.datas.get(0).namecn;
                assertTrue(
                        directory.contactsByDealer(trader, 10).size() >= 1,
                        "the trader image is searchable by name");
            }
        }
    }
}
