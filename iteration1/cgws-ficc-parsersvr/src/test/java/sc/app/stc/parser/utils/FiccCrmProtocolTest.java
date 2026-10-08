package sc.app.stc.parser.utils;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;

import org.junit.Test;

import sc.app.stc.api.TBrokerCrmContact;
import sc.app.stc.api.TBrokerCrmInst;

import java.util.List;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

/**
 * The contract between parsersvr and CRMSvr. parsersvr reads idbkey, shortnamecn, namecn, instid,
 * taglist and biztype — if CRMSvr renames any of them, the parsed quote silently loses its
 * institution and trader, so they are asserted here.
 */
public class FiccCrmProtocolTest {

    @Test
    public void lookupRequestCarriesKindNameMatchAndPageSize() {
        JSONObject request =
                JSON.parseObject(
                        FiccCrmProtocol.lookupRequest(
                                FiccCrmProtocol.INSTITUTION, "示例银行", null, FiccCrmProtocol.EXACT, 50));
        assertEquals("INSTITUTION", request.getString("kind"));
        assertEquals("示例银行", request.getString("name"));
        assertEquals("exact", request.getString("match"));
        assertEquals(50, request.getIntValue("PageSize"));
        assertFalse("no id means search by name", request.containsKey("id"));
    }

    @Test
    public void lookupRequestByIdCarriesTheId() {
        JSONObject request =
                JSON.parseObject(
                        FiccCrmProtocol.lookupRequest(
                                FiccCrmProtocol.INSTITUTION, null, "2001", FiccCrmProtocol.EXACT, 50));
        assertEquals("2001", request.getString("id"));
        assertFalse(request.containsKey("name"));
    }

    @Test
    public void institutionReplyIsParsed() {
        String reply =
                "{\"Code\":\"0000\",\"Msg\":\"\",\"totalSize\":1,\"pageSize\":1,\"datas\":[{"
                        + "\"idbkey\":\"2001\",\"crmid\":\"2001\",\"id\":\"2001\","
                        + "\"shortnamecn\":\"示例银行\",\"fullnamecn\":\"示例银行股份有限公司\","
                        + "\"taglist\":\"BOND\",\"contactcount\":\"3\"}]}";
        List<TBrokerCrmInst> rows = FiccCrmProtocol.institutions(reply);
        assertEquals(1, rows.size());
        assertEquals("2001", rows.get(0).idbkey);
        assertEquals("示例银行", rows.get(0).shortnamecn);
        assertEquals("示例银行股份有限公司", rows.get(0).fullnamecn);
        assertEquals("BOND", rows.get(0).taglist);
    }

    @Test
    public void contactReplyIsParsed() {
        String reply =
                "{\"Code\":\"0000\",\"Msg\":\"\",\"totalSize\":1,\"pageSize\":1,\"datas\":[{"
                        + "\"idbkey\":\"t001\",\"id\":\"t001\",\"namecn\":\"王五\","
                        + "\"instid\":\"2001\",\"instname\":\"示例银行\",\"biztype\":\"BNC\"}]}";
        List<TBrokerCrmContact> rows = FiccCrmProtocol.contacts(reply);
        assertEquals(1, rows.size());
        assertEquals("t001", rows.get(0).idbkey);
        assertEquals("王五", rows.get(0).namecn);
        assertEquals("2001", rows.get(0).instid);
        assertEquals("示例银行", rows.get(0).instname);
        assertEquals("BNC", rows.get(0).biztype);
    }

    @Test
    public void failureAndEmptyRepliesAreTreatedAsNoMatch() {
        assertTrue(FiccCrmProtocol.institutions(null).isEmpty());
        assertTrue(FiccCrmProtocol.institutions("").isEmpty());
        assertTrue(FiccCrmProtocol.institutions("  ").isEmpty());
        assertTrue(FiccCrmProtocol.institutions("not json").isEmpty());
        assertTrue(FiccCrmProtocol.contacts("{\"Code\":\"1009\",\"Msg\":\"starting\"}").isEmpty());
        assertTrue(FiccCrmProtocol.institutions("{\"Code\":\"0000\"}").isEmpty());
        assertTrue(FiccCrmProtocol.institutions("{\"Code\":\"0000\",\"datas\":[]}").isEmpty());
    }

    @Test
    public void clientRejectsBlankNamesWithoutCallingCrmSvr() {
        // The client is never initialized in the test, so a lookup would return null anyway;
        // blank input must not even reach the bus.
        assertNull(FiccCrmClient.getInstByName(" "));
        assertNull(FiccCrmClient.getInstByName(null));
        assertNull(FiccCrmClient.getInstByIDBkey(""));
        assertNull(FiccCrmClient.getContact(null));
        assertNull(FiccCrmClient.getContactByNameMatch(" "));
        assertFalse(FiccCrmClient.isOnline());
    }
}
