# V1.8.10 无界面回归扩展（候选，存在失败项）

## 范围与基线

前序交付V1.8.9，163最近邮件UID1321041258。后端基线`9bb65e4a`，未修改任何产品服务、共享库、部署配置或数据库结构。本包是完整Python无界面测试目录及说明，依赖先前V1.8.5–V1.8.9产品修复；不是累计后端服务源码包。无同事客户端/插件源码，C#控制台桥接仅调用既有DPS SDK。

对比主套件首次提交`e5cb7d5f`及后续专项脚本提交`8a2e4c6e/cc371cc5/eba5c5bb/e18bb9db/5dce890c`：原有专项未在一个入口串行运行，三框新条件也未与评级/区域/价格边界组合验证。本次补齐入口和组合测试，保留原有测试及未验范围。

## 覆盖对应关系

| 修复/合同 | 自动回归 | 本次实测 |
|---|---|---|
| V1.8.9 三框独立、AND/同字段OR、无结果、字面特殊字符 | search：查询、今日视图、历史分页、两归属人、机构修改后成员变化 | 通过 |
| 三框加债项/主体评级、省市区县 | DM宽表+com_basic+area_info独立取预期；每字段正反例及全部交集 | 通过，正例非空 |
| 原价与计价方式条件 | quoteType+price BETWEEN闭区间、相等端点、不匹配和计价方式冲突 | 通过 |
| 用户错误消息 | 版本冲突、无效价格、报价不存在；中文、不泄漏ID/内部符号/指定指令；结构化诊断保留 | 通过，拒绝更新不改变报价 |
| V1.8.5 Unrefer一次、六字段替换与幂等 | 原价保持REFERRED、旧有效价自动Refer、新价ACTIVE；重放不复活；A→B→C；真实推送/数据库 | 12项通过；本轮未做重启恢复 |
| V1.8.6 Parser价格与日志重复 | 16条文本的Parser直连及Quote转发；DPS正文/应用摘要对应及不重复 | **失败，未进入价格/日志断言** |
| V1.8.7/V1.8.8 原价、收益率排序、共档与提醒 | FULL/BEST/DEPTH、真实推送、DB原价、Notification最优原因及Refer切换 | 36项通过，250220.IB非含权样本 |

BEST只返回每侧第1档的全部并列报价；DEPTH=2返回前两档；并列档位例如1、1、2，不能按报价条数截断。本次真实接口已验证。

## 实测与失败

- Python套件自身13项单元测试通过，另完成语法编译检查；不是业务端到端验收。
- 最终真实运行`REG-20261127-093927-687eb7`：search 139项、unrefer 12项、market 36项通过，共187项。Parser组失败，总报告不是全部通过，退出码1。
- Parser直连返回`3999`：交易日历`CNY_CIB`尚未加载，`+0`无法计算交易/结算日。需要QA核对其实际日历数据、启动加载与清空后的重载，并重跑parser-log；本次没有修改Parser产品代码或补造日历。日志防重复断言未执行，不能引用旧成功报告冒充本轮通过。
- 首次执行前按标准Ubuntu启动脚本检查依赖；旧进程仍在，但Windows服务ZooKeeper会话过期后注册失效。先恢复Login，再用既有脚本重启Base/CRM/Parser/Quote/Notification，本轮无配置/JAR改动，QtIntake未启动。环境恢复不是Unrefer重启恢复测试。
- 新增地理反例首轮误用省码作为市码，服务正确拒绝；测试已改为按DM area_type=1/2/3分别取合法另一码，并重跑通过。保留失败记录，不修改产品绕过断言。
- 多轮共40笔测试报价均API清理；独立只读数据库核对全部DELETED。保留审计，不SQL删除。未压力测试。
- 含权2228017.IB计算能力缺口、UI、QA部署、跨日、外部QT及高可用等既有未验项不关闭。本次未改变前端职责。

## QA执行

将附件`iteration1/headless-tests`合并到测试工作目录，保留QA自己的`config.local.json`。不要覆盖服务源码，不需要重发服务JAR。依赖、SDK桥接构建及全部参数见目录README。

```powershell
python -m pip install -r requirements.txt
./build-bridge.ps1 -SdkDirectory <现有DPS-SDK目录>
python -m unittest discover -s tests -v
python run_regressions.py --config config.local.json --log-dir E:/qa/ats_root/log --date 20261127
```

`--date`使用QA实际日志日期；`--search-securities`可指定两只真实券，第一只需有评级和省市区数据；`--market-security`指定无已有有效价、真实计算器可处理的券。缺数据明确BLOCKED，不造样本。全部串行，共用主套件文件锁和每组看门狗，不自动启动/重启服务。

请回传测试源码合并提交、被测服务版本、summary和失败组报告，以及Parser日历加载结论。原始日志和账号配置不纳入Git/公开资料。SMTP接受只表示发送成功，不代表QA合并、部署或验收。
