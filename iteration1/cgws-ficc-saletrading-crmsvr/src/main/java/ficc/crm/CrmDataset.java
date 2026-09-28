package ficc.crm;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/** Immutable snapshot of the CRM tables CRMSvr serves. */
public final class CrmDataset {

    /** One {@code crm_org} row. */
    public static final class Institution {
        public final String id;
        public final String name;
        public final String shortName;
        public final String namePinyin;
        public final String shortNamePinyin;
        public final String orgType;
        public final String cfetsCode;

        public Institution(
                String id,
                String name,
                String shortName,
                String namePinyin,
                String shortNamePinyin,
                String orgType,
                String cfetsCode) {
            this.id = id;
            this.name = name;
            this.shortName = shortName;
            this.namePinyin = namePinyin;
            this.shortNamePinyin = shortNamePinyin;
            this.orgType = orgType;
            this.cfetsCode = cfetsCode;
        }
    }

    /** One {@code crm_contact} row. */
    public static final class Contact {
        public final String id;
        public final String name;
        public final String positionName;
        public final String qq;
        public final String cfetsTraderId;
        public final String cfetsTraderName;

        public Contact(
                String id,
                String name,
                String positionName,
                String qq,
                String cfetsTraderId,
                String cfetsTraderName) {
            this.id = id;
            this.name = name;
            this.positionName = positionName;
            this.qq = qq;
            this.cfetsTraderId = cfetsTraderId;
            this.cfetsTraderName = cfetsTraderName;
        }
    }

    /** One current {@code crm_contact_org_rel} row. */
    public static final class ContactOrg {
        public final String contactId;
        public final String orgId;

        public ContactOrg(String contactId, String orgId) {
            this.contactId = contactId;
            this.orgId = orgId;
        }
    }

    private final List<Institution> institutions;
    private final List<Contact> contacts;
    private final List<ContactOrg> contactOrgs;

    public CrmDataset(
            List<Institution> institutions, List<Contact> contacts, List<ContactOrg> contactOrgs) {
        this.institutions = Collections.unmodifiableList(new ArrayList<>(institutions));
        this.contacts = Collections.unmodifiableList(new ArrayList<>(contacts));
        this.contactOrgs = Collections.unmodifiableList(new ArrayList<>(contactOrgs));
    }

    public List<Institution> institutions() {
        return institutions;
    }

    public List<Contact> contacts() {
        return contacts;
    }

    public List<ContactOrg> contactOrgs() {
        return contactOrgs;
    }
}
