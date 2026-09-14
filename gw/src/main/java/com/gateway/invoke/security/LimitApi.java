package com.gateway.invoke.security;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.alibaba.csp.sentinel.AsyncEntry;
import com.alibaba.csp.sentinel.SphO;
import com.alibaba.csp.sentinel.SphU;
import com.alibaba.csp.sentinel.slots.block.BlockException;
import com.alibaba.csp.sentinel.slots.block.RuleConstant;
import com.alibaba.csp.sentinel.slots.block.flow.FlowRule;
import com.alibaba.csp.sentinel.slots.block.flow.FlowRuleManager;

/**
 * 限流组件，根据sid,进行限流
 * 
 * @author deshuai.kong
 *
 */
public class LimitApi {
	private static final Logger logger = LoggerFactory.getLogger(LimitApi.class);
	static CopyOnWriteArrayList<String> rules = new CopyOnWriteArrayList<String>();

	/**
	 * 添加限流规则
	 * 
	 * @param key
	 * @param qps
	 */
	public static void AddRule(String key, int qps) {
		addFlowRules(key, qps);
	}

	public static void RemoveRule(String key) {
		if (rules.contains(key)) {
			rules.remove(key);

		}
	}

	/**
	 * 获取异步限流实体
	 * 
	 * @param key
	 * @return
	 */
	public static AsyncEntry GetAsynLimitCheck(String key) {

		AsyncEntry entry = null;
		try {
			entry = SphU.asyncEntry(key);
		} catch (BlockException e) {

		}

		return entry;
	}

	/**
	 * 释放异步限流实体
	 * 
	 * @param key
	 * @return
	 */
	public static void ExitAsynLimitCheck(AsyncEntry entry) {
		if (entry != null)
			entry.exit();
	}

	/**
	 * 限流检查同步方法
	 * 
	 * @param key
	 * @return
	 */
	public static boolean LimitCheck(String key, int limitQps) {
		boolean pass = true;

		logger.debug("LimitCheck:{} begin", key);
		logger.debug("LimitCheck:" + key + ":" + Thread.currentThread().getId());
		if (!rules.contains(key)) {
			AddRule(key, limitQps);
			rules.add(key);
			logger.debug("rules add:{}", key);
		}
		try {
			logger.debug("entry:{} begin", key);
			pass = SphO.entry(key);
			logger.debug("entry:{} {} end", key, pass);
		} finally {
			if (pass) {
				SphO.exit();
			}
		}
		if (!pass) {
			logger.warn("LimitCheck:{} {}", key, pass ? "success" : "failed");
		}else {
			logger.debug("LimitCheck:{} {}", key, pass ? "success" : "failed");
		}

		return pass;

	}

	/**
	 * 添加限流规则
	 * 
	 * @param key
	 * @param qps
	 */
	private static void addFlowRules(String key, int qps) {

		logger.debug("addFlowRules:{} {} begin", key, qps);
		List<FlowRule> rules = new ArrayList<>();
		FlowRule rule = new FlowRule();

		rule.setResource(key);
		rule.setGrade(RuleConstant.FLOW_GRADE_QPS);
		// Set limit QPS to 20.
		rule.setCount(qps);
		rules.add(rule);

		FlowRuleManager.loadRules(rules);
		logger.info("addFlowRules:{},{}", key, qps);

	}
}
