# V1.8.1 BaseSvr遗漏补交

上一交付V1.8.0仅含QuoteSvr。本次补交11月23日已完成却未发送的两项BaseSvr改动，不覆盖旧包。

1. 提交2b15f68：DM首次发行规模initialIssueAmount、累计发行总额totalIssueAmount、剩余规模remainAmount，下发于FiccReference；十进制字符串、万元。来自bond_basic和bond_amount；当前余额只取上海当天已生效记录，缺失不补零或STC。
2. 提交4ed96cc：FICC含权字段对齐Core所用DM。hasExerciseOption遵循现有Core embedded_option=1；optionType保留永续ETS/+N客户端兼容，不能删除。optionDate保持原有来源，外层STC接口不改。

## 合并与依赖

仅BaseSvr完整源码参考及src增量补丁，不包含Quote/Common/Notification/Parser/CRM/客户端源码。本包不是全后端累计包。

QA来源基线：rel_0.1.1 bd291c3499038f122aa914bfd074af0b0405b1b8（11月19日邮件原始源码，本方原版导入7a8a0f2）。补丁本地基线aa85f11，其src与QA原档逐文件核对仅有LF/CRLF差异；目标4ed96cc。请在QA当前分支合并六个文件补丁，保留后续修改；不要用完整目录覆盖QA。基线应用补丁与目标源码逐文件内容一致已核对（忽略LF/CRLF）；QA当前分支可能比回邮更新，需核对冲突和换行。

V1.8.0的Quote修复与本包无新增先后依赖：可分别合并、部署；本包不包含旧Quote代码，不回退历史表版本7。Common沿用现有0.1.1任意属性透传，部署配置和协议形状不变。新增可选字段由前端同事接入展示。既有父POM/依赖不随本包重复提供，走原Jenkins/Salt流程。

## 验证及明确待办

已执行：三规模42项测试及真实DM→Base→GW→Quote三券核对；最终含权版本46项测试及四券真实接口核对，同事DLL转换、期限计算通过。详细原始摘要在validation目录。此轮仅打包既有提交，没有新增产品修改或重复运行测试。

未完成：实际STP缓存/页面及QA TBase端到端。含权仅统一现有Core口径，不代表DM枚举解释已正确：本机2228017.IB embedded=0/redeem=1，163954.SH embedded=2/perpetual=1，240210.IB embedded=1。请QA核对官方数据字典和实际源值，不要据此宣布最初“是否含权”业务问题彻底解决。发行人全称筛选仍未修复，不在已完成清单。

请回邮合并提交、构建/部署版本和回归结果。三规模检查空/零/万元单位和当前生效日；含权核对真实DM值、接口、客户端权标志及永续+N。当前为合并回归候选源码，不是完整验收通过版本。
