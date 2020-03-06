using System;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Jobs
{
    /// <summary>
    /// Keeps track of instances run per day. Allows for easy purging of data based on date and to determine what jobs ran on a given date
    /// </summary>
    public class JobInstanceHistory : TableEntity
    {
        public JobInstanceHistory(DateTime date, Guid instanceId, Guid jobid)
        {
            PartitionKey = date.ToString("yyyyMMdd)");
            RowKey = instanceId.ToString();
            InstanceId = instanceId;
            JobId = jobid;
            Date = date;
        }

        public Guid InstanceId { get; set; }
        public Guid JobId { get; set; }
        public DateTime Date { get; set; }
    }
}
