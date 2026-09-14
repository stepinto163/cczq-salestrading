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

1. **双值用 `|` 分隔**（如 `100.0136|100.0136`、`2.0035|2.8155`），对应「行权 / 到期」两个口径。
   本项目 `StcBaseMapper.valuation()` 已用 `bondTypeFlag`（`1=EXERCISE`、`2=MATURITY`）区分，
   但**当前实现按单值解析，遇到 `|` 需要显式拆分**。
2. **日期为 `YYYYMMDD` 字符串**（`20260726`），需归一化为 ISO 日期。

## 3. 与目标 IBOR 接口的差异（需要显式转换的地方）

1. **`bondRating` 可能取自债项评级，但对 PPN/私募债常为 NULL**（见 282644/282353），
   此时应回退到 `issuer_rating_current` 或 `csi_implied_rating`。目标接口只有单一 `bondRating`，
   **口径需要确认**（债项 / 发行人 / 隐含）。
2. **估值口径不一致**：目标 `chinaBondNetValuation` 是**净价**；本项目 `security` 模型只暴露
   `valuationYield`（收益率），净价仅在 `valuation.cleanPrice` 上。两者不是同一个量，必须显式映射。
3. 命名差异：`securityId` ↔ `bondCode`、`securityName` ↔ `bondName`、
   `remainingYears`(文本) ↔ `remainingTerm`(字符串)。

## 4. 复现方式

只读查询脚本见 [bond-field-mapping-probe.sql](../scripts/bond-field-mapping-probe.sql)。
连接参数通过本机 gitignored 配置或环境变量传入，**不要把库地址、账号或口令提交入库**。
