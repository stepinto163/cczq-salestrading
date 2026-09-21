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
