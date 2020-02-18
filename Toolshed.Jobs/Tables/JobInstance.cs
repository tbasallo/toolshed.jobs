using System;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Jobs
{
    public class JobInstance : TableEntity
    {
        public JobInstance() { }

        public JobInstance(Guid jobId, Guid instanceId, string jobVersion)
        {
            PartitionKey = jobId.ToString();
            RowKey = instanceId.ToString();

            JobId = jobId;
            JobVersion = jobVersion;
            InstanceId = instanceId;
        }

        public Guid JobId { get; set; }
        public string JobVersion { get; set; }
        public Guid InstanceId { get; set; }

        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string LastType { get; set; }
        public DateTime LastOn { get; set; }
        public int TotalDetails { get; set; }

        public bool HasException { get; set; }
        public bool HasWarning { get; set; }
        public bool HasError { get; set; }
        public double RunningTimeInSeconds { get; set; }
    }
}
