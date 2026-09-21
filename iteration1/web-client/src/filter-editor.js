// The server catalog owns field names and operators. Both queries and subscriptions use this contract.
export function conditionValue(definition, operator, text) {
    const values = text.split(/[,，]/).map(value => value.trim());
    const scalar = value => {
        if (operator === 'EXISTS' || definition.type === 'boolean') {
            if (!['true', 'false'].includes(value)) throw new Error(`${definition.label}: 请输入 true 或 false`);
            return value === 'true';
        }
        if (definition.type === 'decimal') {
            if (!/^-?\d+(\.\d+)?$/.test(value)) throw new Error(`${definition.label}: 请输入有效数值`);
        }
        if (definition.enumValues) {
            const option = definition.enumValues.find(item => item.value === value || item.label === value);
            if (!option) throw new Error(`${definition.label}: 未知字典值 ${value}`);
            return option.value;
        }
        return value;
    };
    if (operator === 'BETWEEN' && values.length !== 2) throw new Error(`${definition.label}: 输入下限,上限`);
    if (values.some(value => value === '')) throw new Error(`${definition.label}: 条件值不能为空`);
    const result = ['IN', 'BETWEEN'].includes(operator) ? values.map(scalar) : scalar(text.trim());
    if (operator === 'BETWEEN' && Number(result[0]) > Number(result[1])) throw new Error(`${definition.label}: 下限不能大于上限`);
    return result;
}

const labelFields = {
    '清算速度': 'settlType', '计价方式': 'quoteType', '上市场所': 'listedMarket',
    '地方债专项': 'localGovernmentUse', '地方债区域': 'region',
    '中债资信评级': 'issuerCbrRating', '中债资信': 'issuerCbrRating',
    '中债隐含': 'bondCbrRating', 'YY评分': 'yyRating', '中证隐含': 'csiRating',
    '地方债地区': 'localGovernmentRegion', '含权属性': 'optionType',
    '永续债': 'perpetualType', '利率方式': 'rateIndex', '地区等级': 'regionalRating',
    '地区': 'region', '债券评级': 'bondRating', '城投等级': 'municipalLevel'
};

// These aliases translate existing product labels; the form and its layout stay intact.
const valueAliases = {
    settlType: { 'T+0': ['1'], 'T+1': ['2'], '非远期': ['1', '2'],
        '远期': ['3', '4', '5', '9', '10', '11', '12', '13', '14', '15', '16'] },
    couponFrequency: { '按月': ['M1'], '按季': ['M3'], '半年': ['M6'], '按年': ['Y1'], '其他': ['Other'] },
    quoteType: { '收益率': ['1'], '净价': ['2'], '全价': ['3'] },
    optionType: {
        '不含权': ['NON'], '含权': ['PUT', 'CNP', 'CAL', 'ETS', 'ASS', 'DCN', 'CNV'],
        '赎回权': ['CAL', 'CNP'], '回售权': ['PUT', 'CNP'], '其他权': ['ETS', 'ASS', 'DCN', 'CNV']
    },
    bankType: { '股份制': ['GF'] }
};

function normalizedLabel(text) {
    return text.trim().replace(/[（）()]/g, '');
}

function rangeCondition(field, inputs, factor = 1) {
    const lower = inputs[0]?.value.trim();
    const upper = inputs[1]?.value.trim();
    if (!lower && !upper) return [];
    const numeric = value => {
        if (!/^-?\d+(\.\d+)?$/.test(value)) throw new Error('请输入有效的筛选数值');
        return String(Number(value) * factor);
    };
    if (lower && upper) {
        const value = [numeric(lower), numeric(upper)];
        if (Number(value[0]) > Number(value[1])) throw new Error('下限不能大于上限');
        return [{ field, operator: 'BETWEEN', value }];
    }
    return [{ field, operator: lower ? 'GTE' : 'LTE', value: numeric(lower || upper) }];
}

function termCondition(text) {
    const label = text.replace('＞', '>');
    const match = label.match(/^(<|>)?(\d+)(?:-(\d+))?([YMD])$/);
    if (!match) throw new Error('无法识别剩余期限: ' + text);
    const factor = { Y: 1, M: 1 / 12, D: 1 / 365 }[match[4]];
    return {
        field: 'remainingYears',
        operator: match[1] === '<' ? 'LT' : match[1] === '>' ? 'GT' : 'BETWEEN',
        value: match[1] ? String(Number(match[2]) * factor)
            : [String(Number(match[2]) * factor), String(Number(match[3]) * factor)]
    };
}

export function collectProductFilter(panel) {
    const conditions = [];
    const definitions = panel.ficcFilterDefinitions || new Map();
    const search = panel.querySelector('.filter-section-sticky input');
    if (search?.value.trim()) conditions.push({ field: 'searchText', operator: 'CONTAINS', value: search.value.trim() });
    for (const group of panel.querySelectorAll('[data-ficc-filter-field]')) {
        const field = group.dataset.ficcFilterField;
        const inputs = [...group.querySelectorAll('input')];
        const tags = [...group.querySelectorAll('.filter-tag.active')]
            .map(tag => normalizedLabel(tag.textContent)).filter(text => text !== '全部');
        const select = group.querySelector('select');
        if (group.dataset.ficcFilterType === 'decimal') {
            const factor = field === 'remainingYears' ? ({ M: 1 / 12, D: 1 / 365 }[select?.value] || 1) : 1;
            conditions.push(...rangeCondition(field, inputs, factor));
            for (const tag of tags) {
                conditions.push(field === 'remainingYears' ? termCondition(tag) : { field, operator: 'EQ', value: tag });
            }
            continue;
        }
        if (inputs.length && inputs[0].value.trim()) {
            conditions.push({ field, operator: 'CONTAINS', value: inputs[0].value.trim() });
        }
        if (select && select.value && !['不限', '全部'].includes(select.value)) tags.push(select.value);
        for (const tag of tags) {
            if (group.dataset.ficcFilterType === 'boolean') {
                conditions.push({ field, operator: 'EQ', value: !tag.startsWith('非') && !tag.startsWith('不') });
            } else {
                let targetField = field;
                let definition = definitions.get(field);
                let values = valueAliases[field]?.[tag];
                if (!values) {
                    let option = definition?.enumValues?.find(item => item.value === tag || item.label === tag);
                    if (!option && field === 'region') {
                        option = definitions.get('city')?.enumValues?.find(item => item.value === tag || item.label === tag);
                        if (option) targetField = 'city';
                    }
                    if (!option && ['region', 'localGovernmentRegion'].includes(field)) {
                        option = definition?.enumValues?.find(item => item.label.replace(/省$|市$|壮族自治区$|回族自治区$|维吾尔自治区$|自治区$/, '') === tag);
                    }
                    if (definition?.enumValues && !option) throw new Error(`${definition.label}: 未定义选项 ${tag}`);
                    values = [option ? option.value : tag];
                }
                conditions.push({ field: targetField, operator: values.length > 1 ? 'IN' : 'EQ', value: values.length > 1 ? values : values[0] });
            }
        }
    }
    if (conditions.some(item => item.field === 'city') && conditions.some(item => item.field === 'region')) {
        throw new Error('暂不支持省份和城市混选，请选择同一级地区');
    }
    return { conditions };
}

export function installFilterEditor(panel, definitions, apply, error) {
    // Bind the product's original tags, ranges, selects and buttons. Never replace its DOM.
    panel.ficcFilterBinding?.dispose();
    const byLabel = new Map(definitions.map(definition => [definition.label, definition]));
    const byField = new Map(definitions.map(definition => [definition.field, definition]));
    panel.ficcFilterDefinitions = byField;
    let previousLabel = '';
    for (const group of panel.querySelectorAll('.filter-group,.filter-grid-item')) {
        const label = group.querySelector('.filter-label')?.textContent.trim();
        let field = labelFields[label];
        if (label === '发行人类型' && /央企|国企|民企/.test(group.textContent)) field = 'ownershipType';
        if (label === '中债评级') field = previousLabel === '主体评级' ? 'issuerCbrRating' : 'bondCbrRating';
        const definition = field ? byField.get(field) : byLabel.get(label);
        previousLabel = label;
        if (!definition) continue;
        group.dataset.ficcFilterField = definition.field;
        group.dataset.ficcFilterType = definition.type;
        group.classList.remove('filter-group-config-unavailable', 'filter-group-unsupported', 'filter-page-unavailable');
        group.removeAttribute('aria-disabled');
        for (const control of group.querySelectorAll('input,select,button,.filter-tag')) {
            control.disabled = false;
            control.classList.remove('filter-control-unsupported');
            control.removeAttribute('aria-disabled');
        }
        const select = group.querySelector('select');
        if (select && definition.enumValues?.length) {
            const option = panel.ownerDocument.createElement('option');
            option.value = '';
            option.textContent = '不限';
            select.replaceChildren(option);
            const choices = definition.field === 'region'
                ? [...definition.enumValues, ...(byField.get('city')?.enumValues || []).filter(item => !definition.enumValues.some(province => province.label === item.label))]
                : definition.enumValues;
            for (const example of choices) {
                const choice = panel.ownerDocument.createElement('option');
                choice.value = example.value;
                choice.textContent = example.label;
                select.append(choice);
            }
        }
        if (definition.includesSimulatedData) group.title = '当前资料含模拟值，待外部数据源接入';
    }
    let timer;
    let pending = Promise.resolve();
    const submit = () => {
        clearTimeout(timer);
        try {
            const filter = collectProductFilter(panel);
            pending = pending.then(() => apply(filter)).catch(failure => error(failure.message));
        } catch (failure) {
            error(failure.message);
        }
        return pending;
    };
    const clear = () => {
        const search = panel.querySelector('.filter-section-sticky input');
        if (search) search.value = '';
        for (const group of panel.querySelectorAll('[data-ficc-filter-field]')) {
            group.querySelectorAll('.active').forEach(tag => tag.classList.remove('active'));
            group.querySelectorAll('input').forEach(input => { input.value = ''; });
            group.querySelectorAll('select').forEach(select => { select.selectedIndex = 0; });
        }
    };
    const onClick = event => {
        if (event.target.closest('[data-filter-clear]')) {
            event.preventDefault();
            event.stopImmediatePropagation();
            clear();
            submit();
            return;
        }
        const group = event.target.closest('[data-ficc-filter-field]');
        const tag = event.target.closest('.filter-tag');
        const button = event.target.closest('.filter-btn-primary');
        if (!(group && tag) && !button) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        if (tag) tag.classList.toggle('active');
        submit();
    };
    const onInput = event => {
        if (!event.target.closest('[data-ficc-filter-field],.filter-section-sticky')) return;
        event.stopImmediatePropagation();
        clearTimeout(timer);
        timer = setTimeout(submit, 300);
    };
    panel.addEventListener('click', onClick, true);
    panel.addEventListener('input', onInput, true);
    panel.addEventListener('change', onInput, true);
    panel.ficcFilterBinding = {
        submit,
        clear,
        get pending() { return pending; },
        dispose() {
            clearTimeout(timer);
            panel.removeEventListener('click', onClick, true);
            panel.removeEventListener('input', onInput, true);
            panel.removeEventListener('change', onInput, true);
        }
    };
}
