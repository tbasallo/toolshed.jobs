using System;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Jobs
{
    public class Job : TableEntity
    {
        public Job()
        {

        }
        public Job(Guid id, string version = ServiceManager.DefaultVersionName)
        {
            PartitionKey = id.ToString();
            RowKey = version;
            Id = id;
            Version = version;
        }


        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }




        public DateTime CreatedOn { get; set; }
        public int TotalInstances { get; set; }
        public string LastInstanceStatus { get; set; }
        public DateTime? LastInstanceStatusOn { get; set; }
        public Guid LastInstanceId { get; set; }
        public bool HasException { get; set; }
        public bool HasWarning { get; set; }
        public bool HasError { get; set; }
        public bool IsRunning { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsMultipleRunningInstancesAllowed { get; set; }
        public double LastInstanceRunningTimeInSeconds { get; internal set; }
    }
}
