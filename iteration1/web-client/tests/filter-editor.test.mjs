import test from 'node:test';
import assert from 'node:assert/strict';
import { conditionValue } from '../src/filter-editor.js';

test('range, multiple choice, boolean and text conditions preserve protocol types', () => {
    assert.deepEqual(conditionValue({ type: 'decimal' }, 'BETWEEN', '2.5000,3'), ['2.5000', '3']);
    assert.deepEqual(conditionValue({ type: 'string' }, 'IN', 'S，A'), ['S', 'A']);
    assert.equal(conditionValue({ type: 'boolean' }, 'EQ', 'false'), false);
    assert.equal(conditionValue({ type: 'string' }, 'CONTAINS', '上海'), '上海');
    assert.throws(() => conditionValue({ type: 'decimal' }, 'BETWEEN', '3,2'));
    assert.throws(() => conditionValue({ type: 'decimal' }, 'EQ', 'NaN'));
    assert.throws(() => conditionValue({ type: 'boolean' }, 'EQ', 'unknown'));
});

test('dictionary labels are converted to protocol codes and unknown values fail', () => {
    const definition = {
        type: 'string', label: '企业性质',
        enumValues: [{ value: 'YQ', label: '央企' }, { value: 'GQ', label: '国企' }]
    };
    assert.deepEqual(conditionValue(definition, 'IN', '央企,GQ'), ['YQ', 'GQ']);
    assert.throws(() => conditionValue(definition, 'EQ', 'CGE'));
});
