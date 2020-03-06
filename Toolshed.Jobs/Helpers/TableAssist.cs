namespace Toolshed.Jobs
{
    public static class TableAssist
    {
        const string JobsTableName = "Jobs";
        const string JobInstanceTableName = "JobInstances";
        const string JobInstanceDetailTableName = "JobInstanceDetails";

        public static string Jobs()
        {
            return string.Format("{0}{1}", ServiceManager.TablePrefix, JobsTableName);
        }
        public static string JobInstances()
        {
            return string.Format("{0}{1}", ServiceManager.TablePrefix, JobInstanceTableName);
        }
        public static string JobInstanceDetails()
        {
            return string.Format("{0}{1}", ServiceManager.TablePrefix, JobInstanceDetailTableName);
        }
    }
}
