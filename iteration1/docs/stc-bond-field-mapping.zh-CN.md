# STC 债券评级与估值字段映射（实测，来自 QA 库）

本文件记录 **一只债券的评级与估值字段在各层的实际映射**，数据来自 QA 环境共享库
`<STC 库>`（PostgreSQL 10.23 / TBase，连接参数见本机 gitignored 配置，不在此文件出现）。
所有查询均为只读（`SET SESSION CHARACTERISTICS AS TRANSACTION READ ONLY`），未修改任何对象。

## 1. 涉及的表

| 表 | 行数 | 作用 |
|---|---|---|
| `public.bond` | 91 421 | 债券主档（评级、期限、类型、上市市场） |
| `public.china_bond_valuation` | 104 583 | 中债估值（净价 / 收益率 / 久期 / 凸性 / BPV） |

**关键差异：两张表的债券代码格式不同。**

- `bond.bond_id` = **裸代码**，例如 `269152`、`250001`；另有 `bond_key`（如 `F0000302023CORPPN02`）。
- `china_bond_valuation.bond_id` = **带交易所后缀的代码**，例如 `269152.SH`、`282644.SH`。
- 后缀来自交易所：`CIB → .IB`、`SSE → .SH`、`SZE → .SZ`——与本项目
  `StcBaseMapper.security()` 里的 `Listed_Market` 归一化逻辑**完全一致**，可直接复用。

## 2. 字段映射

### 评级

| 业务含义 | STC 库字段 | 本项目原型字段 | IBOR 接口字段 |
|---|---|---|---|
| 债项评级 | `bond.rating_current` | `security.bondRating`（源 `Rating_Current`） | `BondOrderDTO.bondRating` |
| 发行人评级 | `bond.issuer_rating_current` | `security.issuerRating`（源 `Issuer_Rating_Current`） | 无独立字段 |
| 中证隐含评级 | `bond.csi_implied_rating` | — | — |
| 中债隐含评级 | `bond.cdc_implied_rating` | — | — |
| 评级日期 | `bond.rating_date`（`YYYYMMDD` 字符串） | — | — |
| 评级机构 | `bond.rating_institution_code` | — | — |

### 估值

| 业务含义 | STC 库字段 | 本项目原型字段 | IBOR 接口字段 |
|---|---|---|---|
| 中债估值净价 | `china_bond_valuation.valuation_net_price` | `valuation.cleanPrice`（源 `valuation_net_price`） | `BondOrderDTO.chinaBondNetValuation` |
| 中债估值收益率 | `china_bond_valuation.valuation_yield` | `security.valuationYield`（源 `valuation_yield`） | 无独立字段 |
| 估值日期 | `china_bond_valuation.valuation_date`（`YYYYMMDD`） | `valuationDate`（源 `valuation_date`） | — |
| 修正久期 | `china_bond_valuation.val_modified_duration` | — | `BondCalcResDTO.modifiedDurationTM/TE` |
| 凸性 | `china_bond_valuation.val_convexity` | — | `BondCalcResDTO.convexityTM/TE` |
| 基点价值 | `china_bond_valuation.val_basis_point_value` | — | `BondCalcResDTO.basisPointValueTM/TE` |

### 目标债 250001（验收项指定的那只）

实测行（只读查询）：

```text
bond:                  250001 | F0000302023CORPPN02 | mkt_type=2 | PPN | COR
                       福建漳龙集团有限公司2023年面向专业投资者非公开发行公司债券(第二期)
rating_current       = AAA        -- 债项评级
issuer_rating_current= AAA        -- 发行人评级
csi_implied_rating   = AA+        -- 中证隐含评级（与债项评级不同，注意口径）
cdc_implied_rating   = NULL
rating_date          = 20240619
maturity_date        = 20280224
evaluation_date      = NULL
china_bond_valuation = 0 行（按 bond_id='250001.IB' 与按 bond_key 关联均为 0）
```

**结论：250001 的评级可得（AAA），但中债估值不可得。**
该债为 **PPN（非公开发行）**，`evaluation_date` 为 NULL，`china_bond_valuation` 中无对应行，
因此「同一只债券的评级 + 估值」在本 QA 库中**无法同时取到**。
估值字段映射需改用其他有中债覆盖的债券验证（见下）。

### 有估值覆盖的对照样例（同一次查询，证明映射可用）

```text
bond_id  | full_name                          | rating_current | issuer_rating_current | bond_subtype
269152   | 华金-邗江产发金控扬州京华城资产支持专项计划优先级资产支持证券 | AAA | NULL | AMP
282644   | 金华市国际陆港集团…非公开发行公司债券(第二期)   | NULL | AAA  | PPN
282353   | 金华市国际陆港集团…非公开发行公司债券(第一期)   | NULL | AAA  | PPN

china_bond_valuation.bond_id | valuation_date | valuation_net_price | valuation_yield | val_basis_point_value
269152.SH                    | 20260726       | 100.0136|100.0136   | 2.0035|2.8155   | 0.0246
282644.SH                    | 20260726       | 99.9622             | 1.793           | 0.0271
282353.SH                    | 20260726       | 99.775              | 1.7841          | 0.0264
```

两个必须注意的格式约定：

1. **双值用 `|` 分隔**（如 `100.0136|100.0136`、`2.0035|2.8155`），第 1 段是**行权**口径、
   第 2 段是**到期**口径；**单值属于到期口径**。已按此修正 `StcBaseMapper`（原先按单值解析，
   且错误地用 `bondTypeFlag` 判定口径），依据见第 3 节。
2. **日期为 `YYYYMMDD` 字符串**（`20260726`），需归一化为 ISO 日期。

## 3. `|` 双值与 `bondTypeFlag` 的口径依据（源码核对）

此前把 `bondTypeFlag` 当成估值口径（`1=EXERCISE`、`2=MATURITY`）是**错的**，已纠正。
`sc-app-stc` 源码给出两个**互相独立**的概念：

**(a) 估值口径由「是否存在双值」决定**，不是由 `bondTypeFlag` 决定：

| 位置 | 代码 | 含义 |
|---|---|---|
| `stc-client/STC.InnerBoard.Plugin/…/InnerHistoryRequireViewModel.cs:1377` | `int speIndex = er.chinaBondValuationFull.IndexOf('\|')` | 有 `\|` 即拆成两个口径 |
| 同上 `:1380` | `ChinaBondExercise = …Substring(0, speIndex)` | **第 1 段 = 行权** |
| 同上 `:1381` | `ChinaBondDue = …Substring(speIndex + 1)` | **第 2 段 = 到期** |
| 同上 `:1385` | `else { ChinaBondDue = …ConvertToDecimal(); }` | **单值 = 到期** |
| `sc-app-stc-basesvr/…/CommonDataManager.java:1528` | `splitToArray(valuation_yield, "\|")[0]` | 服务端主值取第 1 段 |
| `sc-app-stc-basesvr/…/UserCommonDataManager.java:103` | `getCbv()` 取 `[0]` | 主值 = 行权口径 |
| `stc-client/STC.Mmshe.Plugin/…/MmsheListBoxViewModel.cs:140` | `//中债估值类型1-行权；2-到期（或者空）` | 客户端字段的独立枚举 |

**(b) `bondTypeFlag` 描述的是「含权债是否已行权」**，与 (a) 是两回事
（`sc-app-stc-basesvr/…/CommonDataManager.java:1420-1438` 为权威实现）：

```
bondTypeFlag 1:含权债，未行权   2:含权债，已行权   空:非含权债
是否行权：option_date 是否大于今天，为空或小于今天则视为已行权
ETS 期权类型默认未行权（=1）
```

> 注：`TBondValuationView.java` 里字段上的注释写作 `//1-行权 2-到期`，
> 与该类自身头部的类注释及 `CommonDataManager` 的实际赋值逻辑**不一致**，属源码注释错误。
> 以实际代码为准。

因此修正后 `StcBaseMapper` 的行为是：

- `valuationBasis`：出现双值 → `EXERCISE`，单值 → `MATURITY`（与 STC 客户端一致）；
- `yield` / `cleanPrice`：取第 1 段（行权主值，与 BaseSvr `getCbv()` 一致）；
- 新增 `exerciseYield` / `dueYield` / `exerciseCleanPrice` / `dueCleanPrice`，两个口径都不丢失；
- 新增 `optionState`：`NOT_EXERCISED` / `EXERCISED` / `null`，正确承载 `bondTypeFlag`。

**该项为源码级核对结论；与之配套的「双值行数占比」等库内统计，因本轮未重新连库，
在 result.json 中标为 `not_run`。**（此前一轮实测：104 583 行估值中 16 971 行为双值，占 16.2%。）

## 4. 与目标 IBOR 接口的差异（需要显式转换的地方）

1. **`bondRating` 可能取自债项评级，但对 PPN/私募债常为 NULL**（见 282644/282353）。
   已实现回退：`Rating_Current` → `Issuer_Rating_Current`，并新增 `bondRatingSource`
   （`ISSUE` / `ISSUER` / `null`）显式标注口径来源，消费方可据此判断。
   **未**回退到 `csi_implied_rating`：它是「隐含评级」，与债项评级不是同一口径
   （实测目标债 250001 上二者分别为 `AAA` 与 `AA+`），是否使用仍需业务确认。
2. **估值口径不一致**：目标 `chinaBondNetValuation` 是**净价**；本项目 `security` 模型只暴露
   `valuationYield`（收益率），净价仅在 `valuation.cleanPrice` 上。两者不是同一个量，必须显式映射。
3. 命名差异：`securityId` ↔ `bondCode`、`securityName` ↔ `bondName`、
   `remainingYears`(文本) ↔ `remainingTerm`(字符串)。

## 5. 复现方式

只读查询脚本见 [bond-field-mapping-probe.sql](../scripts/bond-field-mapping-probe.sql)。
连接参数通过本机 gitignored 配置或环境变量传入，**不要把库地址、账号或口令提交入库**。

口径回归测试见
`iteration1/cczq-app-salestrading-common/src/test/java/ficc/common/client/AdapterContractTest.java`
中的 `valuationSplitsChinaBondExerciseAndDueConventions` 与
`securityFallsBackToIssuerRatingForPrivatePlacements`（`mvn -f iteration1/pom.xml verify` 运行）。
