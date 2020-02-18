﻿namespace Toolshed.Jobs
{
    public static class TableAssist
    {
        const string JobsTableName = "Jobs";
        const string JobInstanceTableName = "JobInstance";
        const string JobInstanceDetailTableName = "JobInstanceDetail";

        public static string Jobs()
        {
            return string.Format("{0}{1}", JobServiceManager.TablePrefix, JobsTableName);
        }
        public static string JobInstances()
        {
            return string.Format("{0}{1}", JobServiceManager.TablePrefix, JobInstanceTableName);
        }
        public static string JobInstanceDetails()
        {
            return string.Format("{0}{1}", JobServiceManager.TablePrefix, JobInstanceDetailTableName);
        }
    }
}