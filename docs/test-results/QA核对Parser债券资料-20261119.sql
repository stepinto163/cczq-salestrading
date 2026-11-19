-- 只读核查，不修改数据。两个查询分别在对应数据源执行。
-- 1. 在QA ParserSvr实际配置的STC连接库执行；不要以其他服务连接库代替。
SELECT bond_id, short_name, listed_market, bond_key
FROM bond_list_info
WHERE lower(bond_id) IN ('2600006', '2600006.ib')
   OR short_name = '26超长特别国债06';

-- 2. 在QA DM连接库执行。
SELECT bond_code, bond_short_name
FROM onshore_bond_filter
WHERE bond_code IN ('2600006', '2600006.IB')
   OR bond_short_name = '26超长特别国债06';

-- 请返回两组查询结果、ParserSvr当前实例/连接的数据源标识，以及失败请求的时间、requestId和错误码。
-- 若STC库有该券而Parser仍返回空，再查加载/刷新是否成功、是否连接错库或请求到旧实例。
-- 不要把2600006改成260006；本机后者是另一只债券“26附息国债06”。
