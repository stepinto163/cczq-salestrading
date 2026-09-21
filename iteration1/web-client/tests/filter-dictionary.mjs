// Sequential dictionary/UI contract check; no quotes or load generation.
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { JSDOM, VirtualConsole } from '../../runtime/dom-test/node_modules/jsdom/lib/api.js';
import { GwClient } from '../src/gw-client.js';
import { collectProductFilter, installFilterEditor } from '../src/filter-editor.js';

const transport = new GwClient('ws://127.0.0.1:18601/gateway');
const watchdog = setTimeout(() => process.exit(1), 20000);
let dom;
try {
    await transport.connect('trader1', 'trader1');
    const { dictionaries, filterFields } = await transport.request('QuoteSvr', 'ficc.quote.dictionary.query', {});
    assert.equal(filterFields.length, 79);
    assert.equal(filterFields.filter(field => field.enumValues).length, 41);
    assert.deepEqual(dictionaries.ownershipType.map(item => item.value), ['YQ', 'GQ', 'MQ', 'Other']);
    const definitions = new Map(filterFields.map(field => [field.field, field]));
    const html = await readFile(new URL('../index.html', import.meta.url), 'utf8');
    dom = new JSDOM(html, { virtualConsole: new VirtualConsole() });
    let choices = 0;
    const failures = [];
    for (const id of ['filters-credit', 'filters-rate', 'filters-merged', 'filters-merged-mgmt']) {
        const panel = dom.window.document.getElementById(id);
        installFilterEditor(panel, filterFields, () => {}, error => { throw new Error(error); });
        panel.ficcFilterBinding.clear();
        for (const tag of panel.querySelectorAll('[data-ficc-filter-field] .filter-tag')) {
            const label = tag.textContent.trim();
            if (['全部', '不限'].includes(label)) continue;
            tag.classList.add('active');
            try {
                const filter = collectProductFilter(panel);
                for (const condition of filter.conditions) {
                    const definition = definitions.get(condition.field);
                    assert.ok(definition.operators.includes(condition.operator));
                    if (definition.enumValues && condition.operator !== 'EXISTS') {
                        const values = Array.isArray(condition.value) ? condition.value : [condition.value];
                        for (const value of values) {
                            assert.ok(definition.enumValues.some(option => option.value === value),
                                condition.field + '=' + value);
                        }
                    }
                }
                choices++;
            } catch (error) {
                failures.push(id + '/' + label + ': ' + error.message);
            } finally {
                tag.classList.remove('active');
            }
        }
        panel.ficcFilterBinding.dispose();
    }
    assert.deepEqual(failures, []);
    await assert.rejects(() => transport.request('QuoteSvr', 'ficc.quote.query', {
        filter: { conditions: [{ field: 'ownershipType', operator: 'EQ', value: '央企' }] }
    }), /Unknown dictionary code/);
    console.log('PASS 79 fields, 41 dictionaries, ' + choices + ' original UI choices and DPS enum validation');
} finally {
    dom?.window.close();
    transport.close();
    clearTimeout(watchdog);
}
