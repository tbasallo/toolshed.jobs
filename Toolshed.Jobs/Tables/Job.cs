using System;

using Toolshed.AzureStorage;

namespace Toolshed.Jobs
{
    public class Job : BaseTableEntity
    {
        public Job()
        {

        }
        public Job(Guid id, string version = ServiceManager.DefaultVersionName)
        {
            PartitionKey = id.ToString();
            RowKey = version.ToValue(ServiceManager.DefaultVersionName);
            Id = id;
            Version = version.ToValue(ServiceManager.DefaultVersionName);
        }


        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }




        public DateTime CreatedOn { get; set; }
        
        public string LastInstanceStatus { get; set; }
        public DateTime? LastInstanceStatusOn { get; set; }
        public Guid LastInstanceId { get; set; }
        public bool HasException { get; set; }
        public bool HasWarning { get; set; }
        public bool HasError { get; set; }
        public bool IsEnabled { get; set; }

        public double LastInstanceRunningTimeInSeconds { get; set; }

        /// <summary>
        /// Indicates whether the last instance of this job is running. This is not a perfect way to track this since the job (this one) could be updated to false when there are still jobs running. We don;t actually check if there are jobs running.
        /// In theory, if multiple jobs are not allowed (why this setting matters) then it would always be accurate. However, if multiple jobs are allowed this setting can be inaccurate. 
        /// </summary>
        public bool IsRunning { get; set; }
        /// <summary>
        /// Indicates if multiple instances of this job can run simultaneously. If false and the job is IsRunning = true when this job is started an JobCurrentlyRunningException is thrown
        /// </summary>
        public bool IsMultipleRunningInstancesAllowed { get; set; }

        /// <summary>
        /// If set to true, will abort a running instance and continue if the instance has been running longer than MinimumMinutesRunningForInstanceAbortion
        /// </summary>
        public bool IsRunningExceptionAborted { get; set; }

        /// <summary>
        /// The default number of minutes that an instance must be running before it will be aborted IF IsRunningExceptionAborted is true.
        /// When setting IsRunningExceptionAborted to true, this property should be adjusted to reflect the number of minutes that is acceptable. The default is 240 minutes (4 hours)
        /// </summary>
        public int MinimumMinutesRunningForInstanceAbortion { get; set; } = 240;


        public DateTime StatsLastUpdatedOn { get; set; }

        /// <summary>
        /// Total instances that have ever been executed. This is not reset when statistics are reset.
        /// </summary>
        public long TotalLifetimeInstances { get; set; }
        /// <summary>
        /// Total number of instances that are currently running
        /// </summary>
        public int TotalInstancesRunning { get; set; }
        /// <summary>
        /// Total number of instances run since the last statistics reset. This is reset when statistics are reset.
        /// </summary>
        public int TotalInstances { get; set; }
        /// <summary>
        /// Total number of exceptions since the last statistics reset. Each instance is counted only once, even if it has multiple events.
        /// </summary>
        public int TotalExceptions { get; set; }
        /// <summary>
        /// Total number of errors since the last statistics reset. Each instance is counted only once, even if it has multiple events.
        /// </summary>
        public int TotalErrors { get; set; }
        /// <summary>
        /// Total number of warnings since the last statistics reset. Each instance is counted only once, even if it has multiple events.
        /// </summary>
        public int TotalWarnings { get; set; }        
    }
}
