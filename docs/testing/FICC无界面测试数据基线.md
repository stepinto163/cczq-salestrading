# FICC无界面功能测试数据基线

## 用户确认的测试目标（2026-11-25）

- 按页面对应业务场景覆盖接口功能，包括报价录入/确认、行情、今日/历史、提醒、CRM、筛选、QT接入和生命周期；不启动客户端UI。UI表现仍是独立验收范围。
- 参数包含网关/登录地址、测试用户、DM连接、FICC业务库连接及场景选择。真实DM资料选券，通过业务接口创建测试报价，再通过真实接口和订阅验证；数据库用于独立预期及持久化核对。
- 从真实数据选择不同含权、期限、评级、区域、流动性等样本。缺样本标记覆盖缺口，不造假DM字段，不把缺样本算通过；边界、空值、组合条件列入覆盖。
- 本轮仅核对字典和保存设计依据，完整测试套件尚未实现或执行，既有单项测试不等于全覆盖。

## 字典来源

用户附件：境内债筛选表数据字典20261021.xlsx.xlsx。
工作表：onshore_bond_filter；C3:C134共132个字段。另一工作表为目录。
原件路径：.codex-attachments/c1305057-d0c1-405f-afbb-d4e420f5b854/43502d10-82f9-4716-9cae-fdce34b2d3cd/境内债筛选表数据字典20261021.xlsx.xlsx
SHA256：a5e62c274a90d86849af10bb2bfb8e826d714237e9efeae5d191c7b4e6af50ea

## 已明确的字段契约

| 字段 | 字典原义/关联 | 单元格 |
|---|---|---|
| embedded_option | 0含权；1不含权；2永续。不能套通用0否1是，也不能先转boolean丢掉2 | C130:H130 |
| put_option_status | 0否；1是 | C126:H126 |
| redeem_status | 0否；1是 | C127:H127 |
| perpetual_status | 0非永续；1永续 | C28:H28 |
| liquidity_score | S+=100、S=95、S-=90；A+=85、A=80、A-=70；B+=60、B=55、B-=50；C+=35、C=30、C-=25；D+=10、D=5、D-=3；E=0 | C110:H110 |
| maturity_date / remaining_tenor_day | 到期日期 / 剩余期限（日）；字典未解释含权日期选择及业务日刷新规则 | C48:H48、C55:H55 |
| bond_ext_rating_filter_mapping / com_ext_rating_filter_mapping | 1 AAA；2 AA+；3 AA；4 AA-；5 A+；999其他。分类字段不能替代全部评级文本 | C25:H26 |
| bond_implied_rating_mapping | 独立隐含评级枚举，包含55=AA(2)、110=BBB、230=C、999其他 | C24:H24 |
| province_uni_code / city_uni_code / district_uni_code | 关联area_info.area_uni_code；另有province2/city2，具体产品使用列须对照接口约定 | C77:H78、C123:H125 |
| indu_level1_code / indu_level2_code | 关联sw_indu_code.indu_uni_code | C72:H72、C33:H33 |
| com_uni_code | 发行人唯一代码 | C5:H5 |
| ncd_type | 2国有、3股份、4城商、5农商、8外资、999其他、1001非存单 | C23:H23 |
| deleted / expired | 0未删除/未到期；1已删除/已到期 | C106:H106、C121:H121 |

以上字段多允许空值，空值与0、999、1001的语义必须分别断言。流动性字典只定义离散分数符号；五档合并及区间边界来自已确认产品规则，不能宣称字典本身定义全部连续区间。期限按当前业务日期/行权日期的规则同样另行明确。

## 对前次含权判断的更正

本字典明确解决此前基础信息字典没有embedded_option定义的问题：embedded_option=1是不含权，前次Base对齐Core的==1判断错误。2为永续单独保留，不能自行决定映射成hasExerciseOption=true或false。用户已指定QA修复含权及流动性后回传，本地本轮未改Base/Core/Quote，后续接收QA代码时按该字典核对，并单独验证计算收益率口径。
