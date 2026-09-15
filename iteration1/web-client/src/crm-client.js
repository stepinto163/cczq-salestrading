// CRM searches share the QuoteSvr DPS connection. IDs always remain strings.
export class CrmClient {
    constructor(gateway) {
        this.gateway = gateway;
        this.institutions = new Map();
        this.contacts = new Map();
    }

    clear() {
        this.institutions.clear();
        this.contacts.clear();
    }

    seed(quote) {
        if (quote.institutionId && quote.institutionName) {
            this.institutions.set(quote.institutionId, { institutionId: quote.institutionId, institutionName: quote.institutionName });
        }
        if (quote.contactId && quote.contactName) {
            const previous = this.contacts.get(quote.contactId);
            this.contacts.set(quote.contactId, { ...previous, contactId: quote.contactId, contactName: quote.contactName,
                institutionIds: [...new Set([...(previous?.institutionIds || []), quote.institutionId].filter(Boolean))] });
        }
    }

    selected(kind, value, existingId) {
        const records = kind === 'INSTITUTION' ? this.institutions : this.contacts;
        const nameKey = kind === 'INSTITUTION' ? 'institutionName' : 'contactName';
        const text = String(value || '').trim();
        for (const [id, record] of records) {
            if (text === `${record[nameKey]} [${id}]` || (id === existingId && text === record[nameKey])) return record;
        }
        return null;
    }

    async search(kind, text, institutionText) {
        const request = { kind, text: String(text || '').trim(), limit: 50 };
        const selected = this.selected(kind, request.text);
        if (selected) request.text = selected[kind === 'INSTITUTION' ? 'institutionName' : 'contactName'];
        if (kind === 'CONTACT') {
            const institution = this.selected('INSTITUTION', institutionText);
            if (!institution) throw new Error('请先从 CRM 下拉列表选择机构，再查询联系人');
            request.institutionId = institution.institutionId;
        }
        const data = await this.gateway.request('QuoteSvr', 'ficc.base.query', request);
        const isInstitution = kind === 'INSTITUTION';
        const idKey = isInstitution ? 'institutionId' : 'contactId';
        const nameKey = isInstitution ? 'institutionName' : 'contactName';
        const records = isInstitution ? this.institutions : this.contacts;
        return {
            hasMore: data.hasMore,
            items: data.records.map(record => {
                records.set(record[idKey], record);
                return { value: `${record[nameKey]} [${record[idKey]}]`, record };
            })
        };
    }

    resolve(row) {
        const institution = this.selected('INSTITUTION', row.inst, row.institutionId);
        const contact = this.selected('CONTACT', row.tr, row.contactId);
        const members = contact?.institutionIds || [contact?.institutionId];
        const belongs = institution && members.includes(institution.institutionId);
        return {
            institutionId: institution?.institutionId || null,
            institutionName: institution?.institutionName || row.inst?.trim() || null,
            contactId: belongs ? contact?.contactId || null : null,
            contactName: contact?.contactName || row.tr?.trim() || null
        };
    }
}
