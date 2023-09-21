using System;
using Toolshed.AzureStorage;

namespace Toolshed.Jobs
{
    public class JobInstanceDetail : BaseTableEntity
    {
        public JobInstanceDetail() { }

        public JobInstanceDetail(Guid jobId, Guid instanceId)
        {
            PartitionKey = instanceId.ToString();
            RowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);

            JobId = jobId;
            InstanceId = instanceId;
        }

        public Guid JobId { get; set; }
        public Guid InstanceId { get; set; }

        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Details { get; set; }
    }
}
