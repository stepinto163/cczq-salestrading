# 地区分类与期限档后端适配 V1.8.12（QA回归候选）

前序已发送版本V1.8.11；本机后端基线e89681ab，分支codex/qa-return-20261125。仅后端及无界面测试，无前端源码、无数据库迁移，不是全平台累计包。

## 来源与本轮范围

QA Core邮件UID1321041266，70d20f0；配套Common邮件UID1321041268，3548b39。Common原包SHA256为bd1fef5f3271f5ccd0936c2dc4b9db037450cc7bec89ed6a37ddde28812989fd。两份原档未经修改保存为独立本机Git提交89b712fe53cb7bccfbb21d8a179f388fc308cfe3，目录.tmp/qa-bondsearch-20261128/upstream-snapshots；不是原作者历史的替代品。

用户确认：沿用BondSearchHost.matchBondScope → BondSearchEngine.matchIds的进程内Java入口，适配regionCategory、termBucket；发行人名称另做兼容检查。前端由同事负责。地区树及报价数量展示需求未确认，本轮不接regionTree、ids网络接口或相似地方债功能；ownerUserName仍延期。

## 实际修改

- Common接入QA的termBucket字段/字典与地区分类展示词；旧“中等地区”保留为AVERAGE别名。
- Core新增两项字段、行值及倒排索引，装配时按DM所属地区和remaining_tenor_day/365派生。新增构造重载保留旧8参数接口；旧调用不隐式启用新规则。
- Core明确依赖Common 0.1.1-SNAPSHOT，避免编译时读到旧0.1.0，导致字段未知、字典失效。保留本机其他Maven版本，QA按现有版本链合并，不能覆盖其POM。
- QuoteSvr/NotificationSvr源码config与Salt files/config均配置regioncategory.rules、term.bucket.bands。部署自动生成，不能让QA手工复制example。真实本机配置与构建JAR哈希已核验，运行目录Common只有一份。
- 没有改变报价条件、权限、NCD、含权和流动性语义；历史仍以最新DM筛选，历史展示快照不改。

## 前端接口说明（交同事实施）

仍使用现有filter.conditions：不同field之间AND；同field多条件OR；一个IN列表也是OR。字段可用于今日、历史、行情、报价提醒现有接口，无需新服务。

```json
{"filter":{"conditions":[{"field":"regionCategory","operator":"IN","value":["GOOD","AVERAGE"]},{"field":"termBucket","operator":"IN","value":["Y0_1","Y1_3"]}]}}
```

regionCategory值：GOOD/AVERAGE/WEAK/POOR；termBucket值：Y0_1/Y1_3/Y3_5/Y5_7/Y7_10/Y10_15/Y15_20/Y20_25/Y25_30/Y30P。支持EQ、IN、EXISTS。未知档位返回校验失败，不静默忽略。空输入不传条件。

期限口径沿用QA实现的DM remaining_tenor_day/365；与现有FICC remainingYears按当前日期推算到期年限不同，不能把两个字段视为完全等价。默认左闭右开，25–30包含整30年，超过30进入Y30P。QA源注释仍标25年以下默认档位待业务确认；本次原样适配可配置规则，不代表替业务签认。地区市级优先省级，未知保持缺失；新疆兵团识别沿用QA规则。默认规则与源码/Salt配置一致，修改配置后需服务重新加载生效。

## 发行人兼容检查：本轮不直接替换

QA新Core把issuerName由宽表com_short_name改为com_info_out.com_full_name，缺全称回退简称，但综合searchText仍仅包含简称。这不等于全称、简称同时兼容。

只读本机真实DM发现：四川水电集团→四川省水电投资经营集团有限公司，深圳人民政府→深圳市人民政府，湘潭交发集团→湘潭交通发展集团有限公司，大连市政府→大连市人民政府，新疆兵团→新疆生产建设兵团。简称不是全称的连续子串，直接替换会使既有CONTAINS简称请求不命中，EQ同样改变。

本机本轮保留旧issuerName行为，不新增com_info_out加载依赖；原“发行人全称筛选”待办尚未解决。建议后续对issuerName匹配同时比较全称和简称，按已定义EQ/CONTAINS分别执行，不把两者拼接为一个假名称；需单独确认后实施。

## 合并注意

包内为本机V1.8.11基线的增量最终文件及组件patch，二选一参考合并。QA若已在Common3548b39/Core70d20f0上开发，新字段/规则原本已存在：不得将旧基线整文件覆盖QA新源码，否则会删除其地区树/ids/similar等公共接口。按方法合并FICC所需契约、旧构造兼容、配置与测试，保留同事公共API和历史。本轮“不接入”不等于要求删除这些功能。

QA请重点确认发行人行为是否已切换全称；如已切换，先保持旧简称筛选兼容，不把本次归类功能上线等同于发行人问题完成。Core/Common编译依赖与服务运行包必须一致；部署Common和Core到Quote与Notification两处并重启。原Base/Parser/QtIntake等依赖继续沿用V1.8.11前序版本。

## 实际验证与未验范围

Common168通过/1跳过，Core94通过，Quote91通过（既有RequestParserTest依赖排除），Notification12通过。真实网关77项通过：三个真实DM样本、真实Base/CRM、两个账号、正反匹配、跨字段交集、多档IN、价格交集、非法档位拒绝、今日/历史/行情全部与最优/通知查询和视图、真实对价提醒。两轮先前失败分别为服务离线和测试漏stream.open，已记录修复后重跑；累计12条测试报价API删除且独立SQL确认全部DELETED，提醒偏好已恢复。

UI/客户端缓存与QA部署未验，不宣称全部端到端验收。地区分类所有省市、期限所有边界由组件测试补充，不声称真实DM覆盖全部档位；未做压力测试。前序Parser CNY_CIB日历问题、ownerUserName延期及其他未验项继续保留。

无界面回归：
```text
python bond_category_regression.py --config config.local.json --report report.json --state unique-state.json
```
使用真实连接/账户配置，预置三只真实券2228017.IB、2280089.IB、250220.IB；数据属性变化会明确失败，不造DM数据补过关。
