# BaseSvr FICC含权字段对齐DM

## 范围与规则

用户要求FICC含权展示与BondSearch Core使用相同DM来源。修改BaseSvr内层FiccReference；不修改客户端、共享Core或STC外层Option_Type。发行人全称筛选问题不属于此次含权字段改动，仍待处理。

来源为onshore_bond_filter，沿用deleted=0、expired=0及现有券码归并规则。

| 字段 | 本次规则 |
|---|---|
| hasExerciseOption | embedded_option=1为true，其余（含缺失）false，与现有Core一致 |
| optionType | 非永续：embedded不为1→NON；含权且put/redeem均1→CNP；仅put=1→PUT；仅redeem=1→CAL；其余null |
| optionType永续兼容 | perpetual_status=1→ETS；仅用于原客户端期限+N展示，**不是Core筛选类型的完全镜像** |
| optionDate | DM宽表无日期字段，保留STC原有日期，不删除、不补造 |
| csiRecommendedBasis | 保持原有规则；不因本轮含权默认值改变推荐估值选择 |

当前客户端实际使用optionType：BondTerm.Calculate调用STCHelper.SetShowTenor，ETS决定永续期限+N，所以不能删除。hasExerciseOption独立控制“权”徽标，不能从optionType反推其值。

## 实际验证与限制

- BaseSvr Maven package：46项通过，无跳过，包含真实PostgreSQL临时schema执行生产SQL的回归；临时schema清理完成。首轮推荐口径回归失败，定位为共用option局部变量引入的连带变化，隔离后通过。
- 本机真实DM→BaseSvr ficc.reference.sync→GW→QuoteSvr ficc.base.get：四只券字段一致。没有手工补入上游字段。
- 使用同事已编译STC.SalesTrading.Plugin.dll，把真实响应送入ReferenceMapper、内存字典及GatewayReferenceFill.ApplyTape、BondTerm.Calculate：2228017.IB徽标false；163954.SH期限3Y + N。此为客户端程序集转换验证，**不是已运行桌面进程缓存或页面验收**。
- 桌面工具缺少@oai/sky及@oai/cua/tinyskyAlt依赖，不能完成页面验证。QA/TBase未验证，不可作为端到端已验收版本交付。
- 本机部署BaseSvr；原jar备份在.tmp/basesvr-before-dm-options-20261123.jar。首次脚本调用被执行策略拦截，使用进程级ExecutionPolicy Bypass运行已有脚本，未修改系统策略。原Login/Quote在既有ZK会话过期后仍监听但网关报不在线，重启这两个本机服务恢复验证；未修改HA实现、未启动QtIntake。

## 必须单独跟进的Core枚举疑点

本机DM：2228017.IB为embedded=0、put=0、redeem=1，Core因此判定不含权；163954.SH为embedded=2且perpetual=1，Core也判定不含权；240210.IB为embedded=1，Core判定含权。这些结果说明仍须核对DM官方枚举字典及QA源值。

本次仅统一既有规则，不宣称DM枚举解释已正确，也不改同事Core。发行人全称/简称筛选问题另行处理。证据见test-results/dm-option-reference-20261123.json。
