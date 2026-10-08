// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import org.apache.commons.configuration.AbstractConfiguration;
import org.quartz.CronScheduleBuilder;
import org.quartz.Job;
import org.quartz.JobBuilder;
import org.quartz.JobDetail;
import org.quartz.JobKey;
import org.quartz.ScheduleBuilder;
import org.quartz.Scheduler;
import org.quartz.SchedulerException;
import org.quartz.Trigger;
import org.quartz.TriggerBuilder;
import org.quartz.TriggerKey;
import org.quartz.impl.StdSchedulerFactory;
import org.quartz.impl.triggers.CronTriggerImpl;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Schedule {
    private Logger logger = LoggerFactory.getLogger(this.getClass());
    private Scheduler scheduler = null;

    public void init(AbstractConfiguration config) {
        String sfile = config.getString("Schedule.Config");
        try {
            StdSchedulerFactory sf = new StdSchedulerFactory(sfile);
            this.scheduler = sf.getScheduler();
        } catch (Exception e) {
            this.logger.error("can't create quartz Scheduler with file: " + sfile);
        }
    }

    public void start() {
        try {
            this.scheduler.start();
        } catch (SchedulerException e) {
            this.logger.error("Scheduler Error", (Throwable) e);
        }
    }

    public void stop() {
        try {
            this.scheduler.shutdown();
        } catch (SchedulerException e) {
            this.logger.error("Scheduler Error", (Throwable) e);
        }
    }

    public void deleteJob(String type, Class<? extends Job> job) {
        String name = job.getSimpleName() + "_" + type;
        JobKey jobKey = new JobKey(name, name);
        TriggerKey tk = new TriggerKey(name, name);
        try {
            if (this.scheduler.checkExists(jobKey) && this.scheduler.checkExists(tk)) {
                this.scheduler.deleteJob(jobKey);
            }
        } catch (Exception e) {
            this.logger.error("", (Throwable) e);
        }
    }

    public void updateJob(String cron, String type, Class<? extends Job> job, Object... paras) {
        String name = job.getSimpleName() + "_" + type;
        JobKey jobKey = new JobKey(name, name);
        TriggerKey tk = new TriggerKey(name, name);
        this.logger.info("job cron:{} {} {}.", new Object[] {cron, job.getSimpleName(), paras});
        try {
            if (this.scheduler.checkExists(jobKey) && this.scheduler.checkExists(tk)) {
                JobDetail jd = this.scheduler.getJobDetail(jobKey);
                Trigger trigh = this.scheduler.getTrigger(tk);
                jd.getJobDataMap().clear();
                jd.getJobDataMap().put("Object", (Object) paras);
                this.scheduler.deleteJob(jobKey);
                if (trigh instanceof CronTriggerImpl) {
                    CronTriggerImpl cronTrig = (CronTriggerImpl) trigh;
                    cronTrig.setCronExpression(cron);
                    this.scheduler.scheduleJob(jd, (Trigger) cronTrig);
                }
            } else {
                JobDetail jobDetail =
                        JobBuilder.newJob(job).withIdentity(new JobKey(name, name)).build();
                jobDetail.getJobDataMap().put("Object", (Object) paras);
                Trigger trigger =
                        TriggerBuilder.newTrigger()
                                .withIdentity(new TriggerKey(name, name))
                                .forJob(jobDetail.getKey())
                                .withSchedule(
                                        (ScheduleBuilder)
                                                CronScheduleBuilder.cronSchedule((String) cron))
                                .build();
                this.scheduler.scheduleJob(jobDetail, trigger);
            }
        } catch (Exception e) {
            this.logger.error("", (Throwable) e);
        }
    }
}
