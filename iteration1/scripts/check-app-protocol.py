"""Check Java/C# contracts and the Web declarations; --write-web refreshes Web files."""
import argparse
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
JAVA = ROOT / 'iteration1/cgws-ficc-saletrading-common/src/main/java/ficc/api'
CSHARP = ROOT / 'iteration1/stc-client/stc-client/STC.SalesTrading.Plugin/Ficc/Protocol'
WEB = ROOT / 'iteration1/web-client/src'


def csharp_type(value):
    return (value.replace('String', 'string').replace('Integer', 'int?')
            .replace('Boolean', 'bool?').replace('Object', 'object').replace('Map<', 'Dictionary<'))


def typescript_type(value):
    if value.startswith('List<'):
        return typescript_type(value[5:-1]) + '[]'
    if value.startswith('Map<'):
        return 'Record<string, ' + typescript_type(value[4:-1].split(',', 1)[1]) + '>'
    return {'String': 'string', 'Integer': 'number', 'Boolean': 'boolean', 'Object': 'unknown'}.get(value, value)


def service(topic):
    if topic.startswith('ficc.notification.'):
        return 'NotificationSvr'
    if topic.startswith('stc.crm.'):
        return 'CRMSvr'
    if topic.startswith('ficc.qtintake.'):
        return 'QtIntakeSvr'
    return 'QuoteSvr'


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--write-web', action='store_true')
    args = parser.parse_args()
    declarations = ['// 应用消息字段；运行期 wire 数据不做数字转换。由 check-app-protocol.py 校验。',
                    'export interface ProtocolModel {}']
    count = 0
    for path in sorted(JAVA.glob('*.java')):
        source = path.read_text(encoding='utf-8-sig')
        definition = re.search(r'public class (\w+) extends (\w+)', source)
        if not definition:
            continue
        name, parent = definition.groups()
        fields = {field: kind for kind, field in re.findall(r'^    public (\S+) (\w+);$', source, re.M)}
        client_source = (CSHARP / (name + '.cs')).read_text(encoding='utf-8-sig')
        client_fields = dict(re.findall(r'\[JsonProperty\("([^"]+)"\)\]\s+public (\S+) \w+ \{', client_source))
        expected = {name: csharp_type(kind) for name, kind in fields.items()}
        if expected != client_fields:
            raise AssertionError(f'{name}: Java/C# fields differ: {expected} != {client_fields}')
        if not re.search(r'public class ' + name + r'\s*:\s*' + parent + r'\b', client_source):
            raise AssertionError(f'{name}: Java/C# base types differ')
        declarations.append(f'export interface {name} extends {parent} {{')
        declarations.extend(f'    {field}?: {typescript_type(kind)} | null;' for field, kind in fields.items())
        declarations.append('}')
        count += 1
    declarations += ['export interface ApiReply<T extends ProtocolModel> extends Reply { Data?: T | null; }',
                     'export interface Endpoint<Q, R> { readonly topic: string; readonly service: string; }',
                     'export const SalesTradingApi: {']
    endpoints = re.findall(r'Endpoint<(\w+), (\w+)> (\w+) =\s*register\(new Endpoint<>\("([^"]+)"',
                           (JAVA / 'SalesTradingApi.java').read_text(encoding='utf-8'))
    cs_endpoints = re.findall(r'Endpoint<(\w+), (\w+)> (\w+) =\s*new Endpoint<\w+, \w+>\("([^"]+)"',
                              (CSHARP / 'SalesTradingApi.cs').read_text(encoding='utf-8'))
    if endpoints != cs_endpoints:
        raise AssertionError('Java/C# endpoint catalogs differ')
    runtime = ['// Topic/服务目录；字段类型见 app-protocol.d.ts。', 'export const SalesTradingApi = Object.freeze({']
    for request, result, name, topic in endpoints:
        declarations.append(f'    readonly {name}: Endpoint<{request}, {result}>;')
        runtime.append(f"    {name}: Object.freeze({{ topic: '{topic}', service: '{service(topic)}' }}),")
    declarations += ['};',
                     'export function endpointFor(topic: string): Endpoint<ProtocolModel, ProtocolModel>;',
                     'export function requestProtocol<Q, R>(transport: { request(service: string, topic: string, body: Q): Promise<R> }, endpoint: Endpoint<Q, R>, body: Q): Promise<R>;']
    runtime += ['});', '', 'const byTopic = new Map(Object.values(SalesTradingApi).map(endpoint => [endpoint.topic, endpoint]));',
                'export function endpointFor(topic) {', '    const endpoint = byTopic.get(topic);',
                '    if (!endpoint) throw new Error(`Unknown application topic: ${topic}`);', '    return endpoint;', '}', '',
                'export function requestProtocol(transport, endpoint, body = {}) {',
                '    return transport.request(endpoint.service, endpoint.topic, body);', '}', '']
    for filename, lines in [('app-protocol.d.ts', declarations), ('app-protocol.js', runtime)]:
        path = WEB / filename
        content = '\n'.join(lines).rstrip() + '\n'
        if args.write_web:
            path.write_text(content, encoding='utf-8')
        elif not path.exists() or path.read_text(encoding='utf-8') != content:
            raise AssertionError(f'{filename} is stale; run with --write-web')
    print(f'PASS {count} Java/C# model contracts and {len(endpoints)} Java/C#/Web endpoints')


if __name__ == '__main__':
    main()
